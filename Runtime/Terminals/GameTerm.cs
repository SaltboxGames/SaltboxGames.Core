/*
 * Copyright (c) 2024 SaltboxGames, Jonathan Gardner
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */


using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#if UNITY_2021_1_OR_NEWER
using UnityEngine;
#endif

namespace SaltboxGames.Core.Terminals
{
    public static class GameTerm
    {
        private static readonly List<ITerminal> terminals = new List<ITerminal>();
        
        private static readonly AggregateCommandRunner aggregate_command_runner;
        private static readonly CommandRegistry command_registry;
        public static ICommandRegistry Commands => command_registry;

        static GameTerm()
        {
            command_registry = new CommandRegistry();
            
            aggregate_command_runner = new AggregateCommandRunner();
            aggregate_command_runner.Register(command_registry);
        }

        public static async void StartTerminal(ITerminal terminal, CancellationToken cancellationToken)
        {
            lock (terminals)
            {
                terminals.Add(terminal);
            }

            try
            {
                await Task.Run(() => terminal.StartTerminal(aggregate_command_runner, cancellationToken), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                //Noop;
            }
            catch (Exception e)
            {
                Log(e);
            }
            finally
            {
                lock (terminals)
                {
                    terminals.Remove(terminal);
                }
            }
        }

        //For sending server info to elevated clients, make sure to wait before disconnecting.
        // Maybe build a disconnection manager; cause we'll want to make sure we call flush, not just send.
#if UNITY_2021_1_OR_NEWER
        [HideInCallstack]
#endif
        public static void Log(string message, LogCategory category = LogCategory.General, LogType logType = LogType.Log)
        {
            for (int index = 0; index < terminals.Count; index++)
            {
                ITerminal terminal = terminals[index];
                terminal.LogFormat(logType, category, message);
            }
        }

#if UNITY_2021_1_OR_NEWER
        [HideInCallstack]
#endif
        public static void Log(Exception exception, LogCategory category = LogCategory.General)
        {
            for (int index = 0; index < terminals.Count; index++)
            {
                ITerminal terminal = terminals[index];
                terminal.LogException(exception, category);
            }
        }

        public static void AddCommandRunner(ICommandRunner runner)
        {
            aggregate_command_runner.Register(runner);
        }

        public static void RemoveCommandRunner(ICommandRunner runner)
        {
            aggregate_command_runner.Remove(runner);
        }

        private class AggregateCommandRunner : ICommandRunner
        {
            public List<ICommandRunner> runners = new List<ICommandRunner>();
            public void Register(ICommandRunner runner)
            {
                lock (runners)
                {
                    runners.Add(runner);
                }
            }

            public void Remove(ICommandRunner runner)
            {
                lock (runners)
                {
                    runners.Remove(runner);
                }
            }
            
            public async ValueTask<CommandResult> Execute(string[] args)
            {
                CommandResult localResult = await runners[0].Execute(args);
                if (runners.Count == 1)
                {
                    return localResult;
                }
                
                if (localResult.ResultStatus != CommandResultStatus.NotFound)
                {
                    return localResult;
                }

                for (int i = 1; i < runners.Count; i++)
                {
                    CommandResult result = await runners[i].Execute(args);
                    if (result.ResultStatus != CommandResultStatus.NotFound)
                    {
                        return result;
                    }
                }
                
                return localResult;
            }

            public async ValueTask<List<string>> TryAutoComplete(string input)
            {
                List<string> results = await runners[0].TryAutoComplete(input);
                if (runners.Count == 1)
                {
                    return results;
                }

                for (int i = 1; i < runners.Count; i++)
                {
                    List<string> remoteResults = await runners[i].TryAutoComplete(input);
                    results.AddRange(remoteResults);
                }

                return results;
            }
            
        }
    }
}
