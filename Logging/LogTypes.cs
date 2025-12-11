using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Cookie.BetterLogging
{
    public struct LogEntry
    {
        public readonly LogNode Content;

        private const string ErrorColour = "#ff534a";
        private const string WarningColour = "#ffc107";

        public LogEntry(
            LogNode content,
            DateTime time
        ) {
            Content = content;
            content.Label = GetLabel(time, content.Info, content.Label);
        }

        private static string GetLabel(DateTime time, LogInfo info, string label) {
            string timeString = time.ToLongTimeString();
            string prefix = info.Type == LogType.Log ? $"[{timeString}]" : $"[{timeString} - {info.Type}]";

            return info.Type switch {
                LogType.Assert or LogType.Exception or LogType.Error =>
                    $"<color=\"{ErrorColour}\"><u>{prefix} {label}</color>",
                LogType.Warning => $"<color=\"{WarningColour}\"><u>{prefix} {label}</color>",
                _ => $"{prefix} {label}",
            };
        }
    }

    public class LogInfo
    {
        [CanBeNull] public readonly string FilePath;
        public readonly int? LineNumber;
        public readonly string StackTrace;
        public readonly LogType Type;

        public LogInfo(LogType type, string stackTrace, [CanBeNull] string filePath = null, int? lineNumber = null) {
            StackTrace = stackTrace;
            Type = type;
            FilePath = filePath;
            LineNumber = lineNumber;
        }
    }

    public class LogNode
    {
        /// <summary>
        ///     The children of the node
        /// </summary>
        /// <remarks>
        ///     This is always empty in builds to avoid the expensive calculations
        /// </remarks>
        [CanBeNull] public readonly IReadOnlyList<LogNode> Children;

        public readonly LogInfo Info;
        public string Label;

        public LogNode(string label, LogInfo info, LogNode[] children = null) {
            Label = label;
            Info = info;
            Children = children ?? Array.Empty<LogNode>();
        }
    }
}