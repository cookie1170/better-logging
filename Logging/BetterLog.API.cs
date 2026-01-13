using UnityEngine;

namespace Cookie.BetterLogging
{
    public static partial class BetterLog
    {
        public static partial void WithType(LogType type, string format, params object[] args);

        public static void WithType(LogType type, object obj) => WithType(type, "{0}", obj);

        public static void Info(string format, params object[] args) =>
            WithType(LogType.Log, format, args);

        public static void Info(object obj) => WithType(LogType.Log, "{0}", obj);

        public static void Warning(string format, params object[] args) =>
            WithType(LogType.Warning, format, args);

        public static void Warning(object obj) => WithType(LogType.Warning, "{0}", obj);

        public static void Error(string format, params object[] args) =>
            WithType(LogType.Error, format, args);

        public static void Error(object obj) => WithType(LogType.Error, "{0}", obj);
    }
}
