﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ML.Data;
using Microsoft.ML.Runtime;
using Tensorflow.Contexts;

#nullable enable
namespace Microsoft.ML.AutoML
{
    internal interface IMLContextManager
    {
        MLContext CreateMLContext();
    }

    /// <summary>
    /// The default mlContext manager which always return a new context based on main context.
    /// </summary>
    internal class DefaultMLContextManager : IMLContextManager
    {
        private readonly MLContext _mainContext;
        private readonly IChannel _channel;

        public DefaultMLContextManager(MLContext mainContext, string? channelName = "ChildContext")
        {
            _mainContext = mainContext;
            _channel = ((IChannelProvider)mainContext).Start(channelName);
        }

        public MLContext CreateMLContext()
        {
            var seed = ((IHostEnvironmentInternal)_mainContext.Model.GetEnvironment()).Seed;

            var newContext = new MLContext(seed);
            newContext.GpuDeviceId = _mainContext.GpuDeviceId;
            newContext.FallbackToCpu = _mainContext.FallbackToCpu;
            newContext.TempFilePath = _mainContext.TempFilePath;
            newContext.Log += (o, e) =>
            {
                // Relay logs that are generated by the current MLContext to the Experiment class's
                // _logger.
                _channel.Send(new ChannelMessage(e.Kind, MessageSensitivity.Unknown, e.Message));
            };

            return newContext;
        }
    }
}
