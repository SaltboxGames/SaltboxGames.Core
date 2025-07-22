/*
 * Copyright (c) 2024 SaltboxGames, Jonathan Gardner
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */


using System;
using System.Threading;
using System.Threading.Tasks;

namespace SaltboxGames.Core.Terminals
{
    public enum LogType : byte
    {
        Log,
        Warning,
        Assert,
        Error,
        Exception
    }

    public enum LogCategory : byte
    {
        General,
        StateManagement,
        Network,
        
        TerminalResponse,
    }
    
    public interface ITerminal
    {
        public void LogFormat(LogType logType, LogCategory category, string format, params object[] args);
        public void LogException(Exception exception, LogCategory category);
        public Task StartTerminal(ICommandRunner commandRunner, CancellationToken cancellationToken);
    }

    public static class TerminalExtensions
    {
        public static void LogCommandResult(this ITerminal terminal, CommandResult result)
        {
            terminal.LogFormat(result.LogType, result.Category, result.Message);
        }
    }
    
#if UNITY_2019_4_OR_NEWER
    public static class TerminalUnityExtensions
    {
        public static UnityEngine.LogType ToUnity(this LogType logType)
        {
            switch(logType)
            {
                case LogType.Log:
                    return UnityEngine.LogType.Log;
                case LogType.Warning:
                    return UnityEngine.LogType.Warning;
                case LogType.Assert:    
                    return UnityEngine.LogType.Assert;
                case LogType.Error:
                    return UnityEngine.LogType.Error;
                case LogType.Exception:
                    return UnityEngine.LogType.Exception;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logType), logType, null);
            }
        }
        
        public static LogType ToITerm(this UnityEngine.LogType logType)
        {
            switch(logType)
            {
                case UnityEngine.LogType.Log:
                    return LogType.Log;
                case UnityEngine.LogType.Warning:
                    return LogType.Warning;
                case UnityEngine.LogType.Assert:
                    return LogType.Assert;
                case UnityEngine.LogType.Error:
                    return LogType.Error;
                case UnityEngine.LogType.Exception:
                    return LogType.Exception;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logType), logType, null);
            }
        }
    }
#endif
}
