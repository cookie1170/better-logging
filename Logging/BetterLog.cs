using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Cookie.BetterLogging.Serialization;
using JetBrains.Annotations;
using UnityEngine;
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
            [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = ""
        ) {
            string serializedObj = Serializer.Serialize(obj);
            string stackTrace = $"{memberName} (at {ReplaceProjectPath(filePath)}:{lineNumber})";
            AddEntry(new LogEntry(obj, stackTrace, DateTime.Now));

            StringBuilder sb = new();
            sb.AppendLine(serializedObj);
            sb.AppendLine(stackTrace);
            _justDebugLogged = true;
            Debug.Log(sb.ToString());
            _justDebugLogged = false;
        }

        private static string ReplaceProjectPath(string filePath) {
            #if UNITY_EDITOR
            return filePath.Replace(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar, "");
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

        public LogEntry(object content, string stackTrace, DateTime time) {
            Content = content;
            StackTrace = stackTrace;
            Time = time.ToLongTimeString();
        }
    }
}