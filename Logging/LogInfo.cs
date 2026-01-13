using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Cookie.BetterLogging
{
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
