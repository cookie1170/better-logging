using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Cookie.BetterLogging.Serialization;
using JetBrains.Annotations;
using UnityEngine;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace Cookie.BetterLogging
{
    [PublicAPI]
    public static class BetterLog
    {
        public static readonly List<LogEntry> Logs = new();

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

        private static void OnLogMessageReceived(string condition, string stackTrace, LogType type) {
            if (_justDebugLogged) return;

            AddEntry(new LogEntry(condition, stackTrace, DateTime.Now));
        }

        #if UNITY_EDITOR
        private static void OnPlaymodeStateChanged(PlayModeStateChange playModeStateChange) {
            if (playModeStateChange != PlayModeStateChange.ExitingPlayMode) return;

            _justDebugLogged = false;
            Logs.Clear();
        }
        #endif

        public static void Log(
            object obj,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0
        ) {
            string serializedObj = Serializer.Serialize(obj);
            string stackTrace = ReplaceProjectPath(new StackTrace(true).ToString());
            AddEntry(new LogEntry(obj, stackTrace, DateTime.Now, filePath, lineNumber));

            StringBuilder sb = new();
            sb.AppendLine(serializedObj);
            sb.AppendLine(stackTrace);
            _justDebugLogged = true;
            Debug.Log(sb.ToString());
            _justDebugLogged = false;
        }

        private static string ReplaceProjectPath(string filePath) {
            #if UNITY_EDITOR
            return filePath.Replace(Directory.GetCurrentDirectory(), ".");
            #else
            return filePath;
            #endif
        }

        private static void AddEntry(LogEntry entry) {
            Logs.Add(entry);
            OnLog?.Invoke(entry);
        }
    }

    public struct LogEntry
    {
        public readonly object Content;
        public readonly string StackTrace;
        public readonly string Time;
        [CanBeNull] public readonly string FilePath;
        public readonly int? LineNumber;

        public LogEntry(
            object content,
            string stackTrace,
            DateTime time,
            string filePath = null,
            int? lineNumber = null
        ) {
            Content = content;
            StackTrace = stackTrace;
            FilePath = filePath;
            LineNumber = lineNumber;
            Time = time.ToLongTimeString();
        }
    }
}