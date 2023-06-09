﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using Microsoft.ML;
using Microsoft.ML.Command;
using Microsoft.ML.CommandLine;
using Microsoft.ML.Runtime;
using Microsoft.ML.Tools;

[assembly: LoadableClass(ChainCommand.Summary, typeof(ChainCommand), typeof(ChainCommand.Arguments), typeof(SignatureCommand),
    "Chain Command", "Chain")]

namespace Microsoft.ML.Tools
{
    using Stopwatch = System.Diagnostics.Stopwatch;

    [BestFriend]
    internal sealed class ChainCommand : ICommand
    {
        public sealed class Arguments
        {
#pragma warning disable 649 // never assigned
            [Argument(ArgumentType.Multiple, HelpText = "Command", Name = "Command", ShortName = "cmd", SignatureType = typeof(SignatureCommand))]
            public IComponentFactory<ICommand>[] Commands;
#pragma warning restore 649 // never assigned
        }

        internal const string Summary = "A command that chains multiple other commands.";

        private readonly IHost _host;

        private readonly Arguments _args;

        public ChainCommand(IHostEnvironment env, Arguments args)
        {
            Contracts.CheckValue(env, nameof(env));
            env.CheckValue(args, nameof(args));

            _args = args;
            _host = env.Register("Chain");
        }

        public void Run()
        {
            using (var ch = _host.Start("Run"))
            {
                var sw = new Stopwatch();
                int count = 0;

                sw.Start();
                if (_args.Commands != null)
                {
                    for (int i = 0; i < _args.Commands.Length; i++)
                    {
                        using (var chCmd = _host.Start(string.Format(CultureInfo.InvariantCulture, "Command[{0}]", i)))
                        {
                            var sub = _args.Commands[i];

                            chCmd.Info("=====================================================================================");
                            chCmd.Info("Executing: {0}", sub);
                            chCmd.Info("=====================================================================================");

                            var cmd = sub.CreateComponent(_host);
                            cmd.Run();
                            count++;

                            chCmd.Info(" ");
                        }
                    }
                }
                sw.Stop();

                ch.Info("=====================================================================================");
                ch.Info("Executed {0} commands in {1}", count, sw.Elapsed);
                ch.Info("=====================================================================================");
            }
        }
    }
}
