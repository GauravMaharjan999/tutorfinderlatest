﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Internal.Utilities;
using Microsoft.ML.Model.Pfa;
using Microsoft.ML.Numeric;
using Microsoft.ML.Runtime;
using Newtonsoft.Json.Linq;

[assembly: LoadableClass(typeof(ClusteringScorer), typeof(ClusteringScorer.Arguments), typeof(SignatureDataScorer),
    "Clustering Scorer", "ClusteringScorer", AnnotationUtils.Const.ScoreColumnKind.Clustering)]

[assembly: LoadableClass(typeof(ClusteringScorer), null, typeof(SignatureLoadDataTransform),
    "Clustering Scorer", ClusteringScorer.LoaderSignature)]

namespace Microsoft.ML.Data
{
    internal sealed class ClusteringScorer : PredictedLabelScorerBase
    {
        public sealed class Arguments : ScorerArgumentsBase
        {
        }

        public const string LoaderSignature = "ClusteringScoreTrans";
        private static VersionInfo GetVersionInfo()
        {
            return new VersionInfo(
                modelSignature: "CLSTRSCR",
                //verWrittenCur: 0x00010001, // Initial
                //verWrittenCur: 0x00010002, // ISchemaBindableMapper
                verWrittenCur: 0x00010003, // ISchemaBindableMapper update
                verReadableCur: 0x00010003,
                verWeCanReadBack: 0x00010003,
                loaderSignature: LoaderSignature,
                loaderAssemblyName: typeof(ClusteringScorer).Assembly.FullName);
        }

        private const string RegistrationName = "ClusteringScore";

        [BestFriend]
        internal ClusteringScorer(IHostEnvironment env, Arguments args, IDataView data, ISchemaBoundMapper mapper, RoleMappedSchema trainSchema)
            : base(args, env, data, mapper, trainSchema, RegistrationName, AnnotationUtils.Const.ScoreColumnKind.Clustering,
                AnnotationUtils.Const.ScoreValueKind.Score, OutputTypeMatches, GetPredColType)
        {
        }

        private ClusteringScorer(IHostEnvironment env, ClusteringScorer transform, IDataView newSource)
            : base(env, transform, newSource, RegistrationName)
        {
        }

        private ClusteringScorer(IHost host, ModelLoadContext ctx, IDataView input)
            : base(host, ctx, input, OutputTypeMatches, GetPredColType)
        {
            // *** Binary format ***
            // <base info>
        }

        public static ClusteringScorer Create(IHostEnvironment env, ModelLoadContext ctx, IDataView input)
        {
            Contracts.CheckValue(env, nameof(env));
            var h = env.Register(RegistrationName);
            h.CheckValue(ctx, nameof(ctx));
            h.CheckValue(input, nameof(input));
            ctx.CheckAtModel(GetVersionInfo());

            return h.Apply("Loading Model", ch => new ClusteringScorer(h, ctx, input));
        }

        private protected override void SaveCore(ModelSaveContext ctx)
        {
            Contracts.AssertValue(ctx);
            ctx.SetVersionInfo(GetVersionInfo());

            // *** Binary format ***
            // <base info>

            base.SaveCore(ctx);
        }

        private protected override IDataTransform ApplyToDataCore(IHostEnvironment env, IDataView newSource)
        {
            Contracts.CheckValue(env, nameof(env));
            Contracts.CheckValue(newSource, nameof(newSource));

            return new ClusteringScorer(env, this, newSource);
        }

        protected override Delegate GetPredictedLabelGetter(DataViewRow output, out Delegate scoreGetter)
        {
            Contracts.AssertValue(output);
            Contracts.Assert(output.Schema == Bindings.RowMapper.OutputSchema);
            Contracts.Assert(output.IsColumnActive(output.Schema[Bindings.ScoreColumnIndex]));

            ValueGetter<VBuffer<float>> mapperScoreGetter = output.GetGetter<VBuffer<float>>(output.Schema[Bindings.ScoreColumnIndex]);

            long cachedPosition = -1;
            VBuffer<float> score = default(VBuffer<float>);
            int keyCount = Bindings.PredColType is KeyDataViewType key ? key.GetCountAsInt32(Host) : 0;
            int scoreLength = keyCount;

            ValueGetter<uint> predFn =
                (ref uint dst) =>
                {
                    EnsureCachedPosition(ref cachedPosition, ref score, output, mapperScoreGetter);
                    Contracts.Check(score.Length == scoreLength);
                    int index = VectorUtils.ArgMin(in score);
                    if (index < 0)
                        dst = 0;
                    else
                        dst = (uint)index + 1;
                };
            ValueGetter<VBuffer<float>> scoreFn =
                (ref VBuffer<float> dst) =>
                {
                    EnsureCachedPosition(ref cachedPosition, ref score, output, mapperScoreGetter);
                    Contracts.Check(score.Length == scoreLength);
                    score.CopyTo(ref dst);
                };

            scoreGetter = scoreFn;
            return predFn;
        }

        private protected override JToken PredictedLabelPfa(string[] mapperOutputs)
        {
            Contracts.Assert(Utils.Size(mapperOutputs) == 1);
            return PfaUtils.Call("a.argmax", mapperOutputs[0]);
        }

        private static DataViewType GetPredColType(DataViewType scoreType, ISchemaBoundRowMapper mapper)
        {
            return new KeyDataViewType(typeof(uint), scoreType.GetVectorSize());
        }

        private static bool OutputTypeMatches(DataViewType scoreType)
        {
            return scoreType is VectorDataViewType vectorType
                && vectorType.IsKnownSize
                && vectorType.ItemType == NumberDataViewType.Single;
        }
    }
}
