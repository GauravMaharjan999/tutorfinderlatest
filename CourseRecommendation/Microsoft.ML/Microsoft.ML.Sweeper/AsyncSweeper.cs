﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.ML;
using Microsoft.ML.CommandLine;
using Microsoft.ML.Internal.Utilities;
using Microsoft.ML.Runtime;
using Microsoft.ML.Sweeper;

[assembly: LoadableClass(typeof(SimpleAsyncSweeper), typeof(SweeperBase.OptionsBase), typeof(SignatureAsyncSweeper),
    "Asynchronous Uniform Random Sweeper", "UniformRandomSweeper", "UniformRandom")]
[assembly: LoadableClass(typeof(SimpleAsyncSweeper), typeof(RandomGridSweeper.Options), typeof(SignatureAsyncSweeper),
    "Asynchronous Random Grid Sweeper", "RandomGridSweeper", "RandomGrid")]
[assembly: LoadableClass(typeof(DeterministicSweeperAsync), typeof(DeterministicSweeperAsync.Options), typeof(SignatureAsyncSweeper),
    "Asynchronous and Deterministic Sweeper", "DeterministicSweeper", "Deterministic")]

namespace Microsoft.ML.Sweeper
{
    public delegate void SignatureAsyncSweeper();

    public sealed class ParameterSetWithId
    {
        public readonly int Id;
        public readonly ParameterSet ParameterSet;

        public ParameterSetWithId(int id, ParameterSet param)
        {
            Contracts.CheckParam(id >= 0, nameof(id));
            Contracts.CheckValueOrNull(param);
            ParameterSet = param;
            Id = id;
        }
    }

    /// <summary>
    /// An interface for sweeper with asynchornous update and proposal.
    /// </summary>
    public interface IAsyncSweeper
    {
        /// <summary>
        /// Propose a <see cref="ParameterSet"/>.
        /// </summary>
        /// <returns>A future <see cref="ParameterSet"/> and its id. Null if unavailable or cancelled.</returns>
        Task<ParameterSetWithId> ProposeAsync();

        /// <summary>
        /// Notify the sweeper of a finished run.
        /// </summary>
        /// <param name="id">Id of the run.</param>
        /// <param name="result">Result of the run. Null if not available.</param>
        void Update(int id, IRunResult result);

        /// <summary>
        /// Request the sweeper to stop generating and dispensing new parameters.
        /// </summary>
        void Cancel();
    }

    /// <summary>
    /// Expose existing <see cref="ISweeper"/>s as <see cref="IAsyncSweeper"/> with no synchronization over the past runs.
    /// Nelder-Mead requires synchronization so is not compatible with SimpleAsyncSweeperBase.
    /// </summary>
    public partial class SimpleAsyncSweeper : IAsyncSweeper, IDisposable
    {
        private readonly List<IRunResult> _results;
        private readonly object _lock;
        private readonly ISweeper _baseSweeper;

        private volatile bool _canceled;
        private bool _disposed;

        // The number of ParameterSets generated so far. Used for indexing.
        private int _numGenerated;

        private SimpleAsyncSweeper(ISweeper baseSweeper)
        {
            Contracts.CheckValue(baseSweeper, nameof(baseSweeper));
            Contracts.CheckParam(!(baseSweeper is NelderMeadSweeper), nameof(baseSweeper), "baseSweeper cannot be Nelder-Mead");

            _baseSweeper = baseSweeper;
            _lock = new object();
            _results = new List<IRunResult>();
        }

        public SimpleAsyncSweeper(IHostEnvironment env, UniformRandomSweeper.OptionsBase options)
            : this(new UniformRandomSweeper(env, options))
        {
        }

        public SimpleAsyncSweeper(IHostEnvironment env, RandomGridSweeper.Options options)
            : this(new UniformRandomSweeper(env, options))
        {
        }

        public void Update(int id, IRunResult result)
        {
            Contracts.CheckParam(0 <= id && id < _numGenerated, nameof(id), "Invalid run id");
            if (!_canceled && result != null)
            {
                lock (_lock)
                    _results.Add(result);
            }
        }

        public Task<ParameterSetWithId> ProposeAsync()
        {
            if (_canceled)
                return Task.FromResult<ParameterSetWithId>(null);
            lock (_lock)
            {
                if (_disposed)
                    throw Contracts.Except("Calling Propose after the sweeper is disposed");
                var paramSets = _baseSweeper.ProposeSweeps(1, _results);
                if (Utils.Size(paramSets) > 0)
                    return Task.FromResult(new ParameterSetWithId(_numGenerated++, paramSets[0]));
            }
            return Task.FromResult<ParameterSetWithId>(null);
        }

        public void Cancel()
        {
            _canceled = true;
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (!_disposed)
                {
                    (_baseSweeper as IDisposable)?.Dispose();
                    _disposed = true;
                    Cancel();
                }
            }
        }
    }

    /// <summary>
    /// An wrapper around <see cref="ISweeper"/> which enforces determinism by imposing synchronization over past runs.
    /// Suppose n <see cref="ParameterSet"/>s are generated up to this point. The sweeper will refrain from making a decision
    /// until the runs with indices in [0, n - relaxation) have all finished. A new batch of <see cref="ParameterSet"/>s will be
    /// generated based on the first n - relaxation runs.
    /// </summary>
    public sealed class DeterministicSweeperAsync : IAsyncSweeper, IDisposable
    {
        public sealed class Options
        {
            [Argument(ArgumentType.Multiple | ArgumentType.Required, HelpText = "Base sweeper", ShortName = "sweeper", SignatureType = typeof(SignatureSweeper))]
            public IComponentFactory<ISweeper> Sweeper;

            [Argument(ArgumentType.AtMostOnce, HelpText = "Sweep batch size", ShortName = "batchsize")]
            public int BatchSize = 5;

            [Argument(ArgumentType.AtMostOnce, HelpText = "Synchronization relaxation", ShortName = "relaxation")]
            public int Relaxation = 0;

            [Argument(ArgumentType.AtMostOnce, HelpText = "Random seed", ShortName = "seed")]
            public int? RandomSeed;
        }

        private readonly object _lock;
        private readonly CancellationTokenSource _cts;

        private readonly Channel<ParameterSetWithId> _paramChannel;
        private readonly int _relaxation;
        private readonly ISweeper _baseSweeper;
        private readonly IHost _host;
        private readonly int _batchSize;
        private bool _disposed;

        // Minimum id of unfinished runs. All runs with id < _minUnfinishedId are finished.
        private int _minUnfinishedId;

        // The ith element of _results corresponds to the result of the ith run.
        private readonly List<IRunResult> _results;

        // The indices of the runs with null IRunResult. We have to keep track of both the indices and
        // the results of finished runs to determine if the synchronization barrier is satisfied.
        // Using _results alone won't do it as the result could be null.
        // Note that we only need to record those >= _minUnfinishedId.
        private readonly HashSet<int> _nullRuns;

        // The number of ParameterSets generated so far. Used for indexing.
        private int _numGenerated;

        public DeterministicSweeperAsync(IHostEnvironment env, Options options)
        {
            _host = env.Register("DeterministicSweeperAsync", options.RandomSeed);
            _host.CheckValue(options.Sweeper, nameof(options.Sweeper), "Please specify a sweeper");
            _host.CheckUserArg(options.BatchSize > 0, nameof(options.BatchSize), "Batch size must be positive");
            _host.CheckUserArg(options.Relaxation >= 0, nameof(options.Relaxation), "Synchronization relaxation must be non-negative");
            _host.CheckUserArg(options.Relaxation <= options.BatchSize, nameof(options.Relaxation),
                "Synchronization relaxation cannot be larger than batch size");
            _batchSize = options.BatchSize;
            _baseSweeper = options.Sweeper.CreateComponent(_host);
            _host.CheckUserArg(!(_baseSweeper is NelderMeadSweeper) || options.Relaxation == 0, nameof(options.Relaxation),
                "Nelder-Mead requires full synchronization (relaxation = 0)");

            _cts = new CancellationTokenSource();
            _relaxation = options.Relaxation;
            _lock = new object();
            _results = new List<IRunResult>();
            _nullRuns = new HashSet<int>();
            _paramChannel = Channel.CreateUnbounded<ParameterSetWithId>(
                new UnboundedChannelOptions { SingleWriter = true });

            PrepareNextBatch(null);
        }

        private void PrepareNextBatch(IEnumerable<IRunResult> results)
        {
            _host.Check(!_disposed, "Creating parameters while sweeper is disposed");
            var paramSets = _baseSweeper.ProposeSweeps(_batchSize, results);
            if (Utils.Size(paramSets) == 0)
            {
                // Mark the queue as completed.
                _paramChannel.Writer.Complete();
                return;
            }
            // Assign an id to each ParameterSet and enque it.
            foreach (var paramSet in paramSets)
                _paramChannel.Writer.TryWrite(new ParameterSetWithId(_numGenerated++, paramSet));
            EnsureResultsSize();
        }

        private void EnsureResultsSize()
        {
            // Allocate the result slots for the new batch.
            while (_results.Count < _numGenerated)
                _results.Add(null);
        }

        public void Update(int id, IRunResult result)
        {
            if (_cts.IsCancellationRequested)
                return;
            _host.Check(0 <= id && id < _results.Count, "Invalid index");
            lock (_lock)
            {
                UpdateResult(id, result);
                UpdateBarrierStatus(id);
                if (CheckBarrier())
                    PrepareNextBatch(_results.GetRange(0, Math.Max(0, _numGenerated - _relaxation)));
            }
        }

        private void UpdateResult(int id, IRunResult result)
        {
            if (result == null)
                _nullRuns.Add(id);
            else
                _results[id] = result;
        }

        private bool CheckBarrier()
        {
            return _minUnfinishedId >= _numGenerated - _relaxation;
        }

        private void UpdateBarrierStatus(int id)
        {
            if (id == _minUnfinishedId)
            {
                while (++_minUnfinishedId < _numGenerated && _results[_minUnfinishedId] != null || _nullRuns.Remove(_minUnfinishedId))
                    ;
            }
        }

        public async Task<ParameterSetWithId> ProposeAsync()
        {
            if (_cts.IsCancellationRequested)
                return null;
            try
            {
                return await _paramChannel.Reader.ReadAsync(_cts.Token);
            }
            catch (InvalidOperationException)
            {
                // Do nothing. When the queue is empty and completed, InvalidOperationException will be thrown.
            }
            catch (OperationCanceledException)
            {
                // Nothing to do for canceled tasks.
            }
            return null;
        }

        public void Cancel()
        {
            _cts.Cancel();
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (!_disposed)
                {
                    (_baseSweeper as IDisposable)?.Dispose();
                    _disposed = true;
                    Cancel();
                }
            }
        }
    }
}
