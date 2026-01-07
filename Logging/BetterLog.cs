using System;
using System.Diagnostics;
using System.Linq;
using Cookie.BetterLogging.Serialization;
using JetBrains.Annotations;
using UnityEngine;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Cookie.BetterLogging
{
    [PublicAPI]
    public static partial class BetterLog
    {
        private const int DepthLimit = 8;

        private static bool _justDebugLogged = false;
        public static event Action<LogEntry> LogReceived;

        [InitializeOnLoadMethod]
        private static void Init()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
            Application.logMessageReceived += OnLogMessageReceived;

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlaymodeStateChanged;
            EditorApplication.playModeStateChanged += OnPlaymodeStateChanged;
#endif
        }

        private static void OnLogMessageReceived(string message, string stackTrace, LogType type)
        {
            if (_justDebugLogged)
                return;

            var logInfo = new LogInfo(type, stackTrace);
            LogEntry(new LogEntry(GetLogFor(message, logInfo), DateTime.Now));
        }

#if UNITY_EDITOR
        private static void OnPlaymodeStateChanged(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange != PlayModeStateChange.ExitingPlayMode)
                return;

            _justDebugLogged = false;
        }
#endif

        private static void LogEntry(LogEntry entry) => LogReceived?.Invoke(entry);

        public static void Log(LogType type, object obj) => Log(type, "{0}", obj);

        public static void Log(LogType type, string format, params object[] args)
        {
            string message = string.Format(format, args.Select(Serializer.Serialize).ToArray());

#if UNITY_EDITOR // avoid the expensive stuff if we're not in the editor and only serialize the object, as we're not going to see them in the log files anyway

            StackTrace stackTrace = new(true);
            StackFrame[] frames = stackTrace.GetFrames();
            string filePath = "";
            int lineNumber = 0;
            int column = 0;

            foreach (StackFrame frame in frames)
            {
                if (frame.GetFileName().EndsWith("Logging/BetterLog.cs")) // there's gotta be a better way to do this, right?
                    continue;

                filePath = frame.GetFileName();
                lineNumber = frame.GetFileLineNumber();
                column = frame.GetFileColumnNumber();

                break;
            }

            string traceText = FormatStackTrace(stackTrace);
            LogInfo info = new(type, traceText, filePath, lineNumber, column);
            LogEntry entry;
            if (format == "{0}" && args.Length > 0)
            {
                entry = new LogEntry(GetLogFor(args[0], info), DateTime.Now);
            }
            else
            {
                entry = GetFormatLog(format, args, info);
            }

            LogEntry(entry);
#endif

            _justDebugLogged = true;
            Debug.unityLogger.Log(type, message);
            _justDebugLogged = false;
        }
    }
}
