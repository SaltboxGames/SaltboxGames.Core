/*
 * Copyright (c) 2024 SaltboxGames, Jonathan Gardner
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using ZLinq;

namespace SaltboxGames.Core.Terminals
{
    public class CommandRegistry : ICommandRegistry
    {
        private static Parser _parser = new Parser(settings =>
        {
            settings.EnableDashDash = true;
            settings.HelpWriter = null;
            settings.AutoVersion = false;
        });

        private class Command
        {
            public Func<string[], ValueTask<string>> Parser;
            public string Verb;
            public string HelpText;
        }

        private readonly Dictionary<string, Command> commands = new Dictionary<string, Command>();

        public CommandRegistry()
        {
            Register<HelpCommand>(HelpExecutor);
        }

        public void Register<T>(Func<T, ValueTask<string>> handler)
        {
            VerbAttribute verbAttr = typeof(T)
                .GetCustomAttributes(typeof(VerbAttribute), false)
                .FirstOrDefault() as VerbAttribute;
            if (verbAttr == null)
            {
                return;
            }

            string key = verbAttr.Name.ToLowerInvariant();
            if (commands.ContainsKey(key))
            {
                return;
            }

            commands[key] = new Command()
            {
                Parser = BuildParser(handler, verbAttr),
                Verb = key,
                HelpText = verbAttr.HelpText,
            };
        }

        public void Remove<T>()
        {
            VerbAttribute verbAttr = typeof(T)
                .GetCustomAttributes(typeof(VerbAttribute), false)
                .FirstOrDefault() as VerbAttribute;
            if (verbAttr == null)
            {
                return;
            }

            string key = verbAttr.Name.ToLowerInvariant();
            commands.Remove(key);
        }

        private static Func<string[], ValueTask<string>> BuildParser<T>(Func<T, ValueTask<string>> handler, VerbAttribute verbAttr)
        {
            return args =>
            {
                ParserResult<T> result = _parser.ParseArguments<T>(args);
                if (result.Tag == ParserResultType.Parsed)
                {
                    var parsed = (Parsed<T>)result;
                    return handler(parsed.Value);
                }
                else
                {
                    return new ValueTask<string>(BuildHelpText(result, verbAttr));
                }
            };
        }

        private static string BuildHelpText<T>(ParserResult<T> result, VerbAttribute verbAttr)
        {
            HelpText help = HelpText.AutoBuild(result, h =>
            {
                h.Heading = $"{verbAttr.Name} - {verbAttr.HelpText}";
                h.Copyright = string.Empty;
                h.AutoVersion = false;
                h.AdditionalNewLineAfterOption = false;
                return h;
            }, e => e);
            return help.ToString();
        }

        public async ValueTask<CommandResult> Execute(string[] args)
        {
            if (args.Length == 0)
            {
                return new CommandResult()
                {
                    ResultStatus = CommandResultStatus.NoInput,
                    LogType = LogType.Error,
                    Category = LogCategory.TerminalResponse,
                    Message = "No command provided.",
                };
            }

            string verb = args[0].ToLowerInvariant();
            if (commands.TryGetValue(verb, out Command command))
            {
                try
                {
                    string result = await command.Parser(args);
                    return new CommandResult()
                    {
                        ResultStatus = CommandResultStatus.Executed,
                        LogType = LogType.Log,
                        Category = LogCategory.TerminalResponse,
                        Message = result,
                    };
                }
                catch (Exception ex)
                {
                    return new CommandResult()
                    {
                        ResultStatus = CommandResultStatus.ExecutionError,
                        LogType = LogType.Exception,
                        Category = LogCategory.TerminalResponse,
                        Message = ex.Message,
                    };
                }
            }

            return new CommandResult()
            {
                ResultStatus = CommandResultStatus.NotFound,
                LogType = LogType.Error,
                Category = LogCategory.TerminalResponse,
                Message = $"Unknown command: {verb}",
            };
        }

        public ValueTask<List<string>> TryAutoComplete(string input)
        {
            List<string> matches = commands.Values
                .AsValueEnumerable()
                .Select(v => v.Verb)
                .Where(name => name.StartsWith(input, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return new ValueTask<List<string>>(matches);
        }

        [Verb("help", HelpText = "Lists all available commands and their descriptions.")]
        private class HelpCommand
        {
            [Option('c', "command", Required = false, HelpText = "Specific command to describe (optional).")]
            public string Command { get; set; }
        }

        private ValueTask<string> HelpExecutor(HelpCommand cmd)
        {
            if (string.IsNullOrWhiteSpace(cmd.Command))
            {
                return new ValueTask<string>(
                    "\n" + string.Join("\n", commands
                        .Select(v => $"{v.Key,16} - {v.Value.HelpText}")));
            }

            if (!commands.TryGetValue(cmd.Command.ToLowerInvariant(), out Command command))
            {
                return new ValueTask<string>($"Unknown command: {cmd.Command}");
            }

            return command.Parser(new[] { command.Verb, "--help" });
        }
    }
}
