using System;
using Cookie.BetterLogging.TreeGeneration;
using JetBrains.Annotations;
using UnityEngine;

namespace Cookie.BetterLogging
{
    public struct LogEntry
    {
        public readonly Node Content;
        public readonly LogInfo Info;

        private const string ErrorColour = "#ff534a";
        private const string WarningColour = "#ffc107";

        public LogEntry(Node content, LogInfo info)
        {
            Content = content;
            Info = info;
        }

        private static string GetPrefix(DateTime time, LogInfo info, string prefix)
        {
            string timeString = time.ToLongTimeString();
            string newPrefix =
                info.Type == LogType.Log ? $"[{timeString}]" : $"[{timeString} - {info.Type}]";

            if (prefix != null)
                newPrefix += " " + prefix;

            return info.Type switch
            {
                LogType.Assert or LogType.Exception or LogType.Error =>
                    $"<color=\"{ErrorColour}\"><u>{newPrefix}",
                LogType.Warning => $"<color=\"{WarningColour}\"><u>{newPrefix}",
                _ => $"{newPrefix}",
            };
        }
    }

    public class LogInfo
    {
        public readonly LogType Type;
        public readonly DateTime Time;
        public readonly string StackTrace;

        [CanBeNull]
        public readonly string FilePath;
        public readonly int? LineNumber;
        public readonly int? Column;

        public LogInfo(
            LogType type,
            DateTime time,
            string stackTrace,
            [CanBeNull] string filePath = null,
            int? lineNumber = null,
            int? column = null
        )
        {
            Type = type;
            Time = time;
            StackTrace = stackTrace;
            FilePath = filePath;
            LineNumber = lineNumber;
            Column = column;
        }
    }
}
