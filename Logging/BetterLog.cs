using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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
        private const int MaxLogs = 512;
        private const int ClearIfMaxAmount = 64;

        public static List<LogEntry> Logs = new();
        private static bool _justDebugLogged = false;
        public static event Action<LogEntry> OnLog;

        [InitializeOnLoadMethod]
        private static void Init() {
            Application.logMessageReceived -= OnLogMessageReceived;
            Application.logMessageReceived += OnLogMessageReceived;

            #if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlaymodeStateChanged;
            EditorApplication.playModeStateChanged += OnPlaymodeStateChanged;
            #endif
        }

        private static void OnLogMessageReceived(string message, string stackTrace, LogType type) {
            if (_justDebugLogged) return;

            var logInfo = new LogInfo(type, stackTrace);
            AddEntry(new LogEntry(GetLogFor(message, logInfo), DateTime.Now));
        }

        #if UNITY_EDITOR
        private static void OnPlaymodeStateChanged(PlayModeStateChange playModeStateChange) {
            if (playModeStateChange != PlayModeStateChange.ExitingPlayMode) return;

            _justDebugLogged = false;
            Logs.Clear();
        }
        #endif

        private static void AddEntry(LogEntry entry) {
            if (Logs.Count >= MaxLogs) Logs.RemoveRange(0, Logs.Count - MaxLogs + ClearIfMaxAmount);
            Logs.Add(entry);
            OnLog?.Invoke(entry);
        }

        public static void LogWarning(
            object obj,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0
        ) {
            Log(obj, LogType.Warning, filePath, lineNumber);
        }

        public static void LogError(
            object obj,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0
        ) {
            Log(obj, LogType.Error, filePath, lineNumber);
        }

        public static void LogAssertion(
            object obj,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0
        ) {
            Log(obj, LogType.Assert, filePath, lineNumber);
        }

        public static void LogException(
            object obj,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0
        ) {
            Log(obj, LogType.Exception, filePath, lineNumber);
        }

        public static void Log(
            object obj,
            LogType type = LogType.Log,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0
        ) {
            string serializedObj = Serializer.Serialize(obj);

            #if UNITY_EDITOR // avoid the expensive stuff if we're not in the editor and only serialize the object, as we're not going to see them in the log files anyway 

            string stackTrace = FormatStackTrace(new StackTrace(true).ToString());
            LogInfo info = new(type, stackTrace, filePath, lineNumber);
            AddEntry(new LogEntry(GetLogFor(obj, info), DateTime.Now));

            #endif

            _justDebugLogged = true;
            Debug.unityLogger.Log(type, serializedObj);
            _justDebugLogged = false;
        }
    }
}