/*
 * Copyright (c) 2024 SaltboxGames, Jonathan Gardner
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */


using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
#if MEMORY_PACK
using MemoryPack;
#endif

namespace SaltboxGames.Core.Terminals
{
    public interface ICommandRegistry : ICommandRunner
    {
        public void Register<T>(Func<T, ValueTask<string>> handler);
        public void Remove<T>();
    }

    public static class CommandRegistryExtensions
    {
        public static void Register<T>(this ICommandRegistry commandRegistry, Func<T, string> handler)
        {
            commandRegistry.Register<T>((com) => new ValueTask<string>(handler(com)));
        }
        
        public static void Register<T>(this ICommandRegistry commandRegistry, Func<T, Task<string>> handler)
        {
            commandRegistry.Register<T>(async com => await handler(com));
        }

        public static ValueTask<CommandResult> Execute(this ICommandRunner commandRunner, string input)
        {
            // TODO: maybe allow quotes inside quotes???
            // https://github.com/Jonathan-Gardner/AutoDeck/issues/1
            string[] args = Regex.Matches(input, @"[\""].+?[\""]|[^ ]+")
                .Select(m => m.Value.Replace("\"", ""))
                .ToArray();

            return commandRunner.Execute(args);
        }
    }
    
    public interface ICommandRunner
    {
        public ValueTask<CommandResult> Execute(string[] args);
        public ValueTask<List<string>> TryAutoComplete(string input);
    }
#if MEMORY_PACK
    [MemoryPackable]
#endif
    public partial struct CommandResult
    {
        public CommandResultStatus ResultStatus;
        
        public LogType LogType;
        public LogCategory Category;
        public string Message;
    }

    
    public enum CommandResultStatus : byte
    {
        NotFound,
        NoInput,
        Executed,
        ExecutionError,
    }
}
