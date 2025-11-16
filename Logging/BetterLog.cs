using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        private const int DepthLimit = 5;
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

        private static void OnLogMessageReceived(string condition, string stackTrace, LogType type) {
            if (_justDebugLogged) return;

            AddEntry(new LogEntry(GetLogFor(condition), stackTrace, DateTime.Now));
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
            string stackTrace = FormatStackTrace(new StackTrace(true).ToString());
            AddEntry(new LogEntry(GetLogFor(obj), stackTrace, DateTime.Now, filePath, lineNumber));

            StringBuilder sb = new();
            sb.AppendLine(serializedObj);
            sb.AppendLine(stackTrace);
            _justDebugLogged = true;
            Debug.Log(sb.ToString());
            _justDebugLogged = false;
        }

        private static string FormatStackTrace(string originalTrace) {
            string[] splitTrace = originalTrace.Split(Environment.NewLine)
                .Where(s => !s.Contains("Cookie.BetterLogging.BetterLog.Log")).ToArray();
            string trace = splitTrace.Aggregate((s1, s2) => s1 + Environment.NewLine + s2);
            #if UNITY_EDITOR
            return trace.Replace(Directory.GetCurrentDirectory(), ".");
            #else
            return trace;
            #endif
        }

        private static void AddEntry(LogEntry entry) {
            Logs.Add(entry);
            OnLog?.Invoke(entry);
        }

        private static LogNode GetLogFor(object obj, int depth = 0, string label = null) {
            if (depth > DepthLimit) return new LogNode(Serializer.Serialize(obj));
            switch (obj) {
                case string str:
                    return new LogNode(str);
                case Vector2 or Vector3 or Vector4 or Vector2Int or Vector3Int:
                    return new LogNode(obj.ToString());
            }

            LogNode[] children = obj switch {
                IDictionary dictionary => GetLogForDictionary(dictionary, depth + 1),
                IEnumerable enumerable => GetLogForEnumerable(enumerable, depth + 1),
                _ => GetLogForFields(obj, depth + 1),
            };

            LogNode root = new(label ?? obj.GetType().Name, children);

            return root;
        }

        private static LogNode[] GetLogForEnumerable(IEnumerable enumerable, int depth) {
            List<object> items = enumerable.Cast<object>().ToList();

            List<LogNode> result = new();

            for (int i = 0; i < items.Count; i++) result.Add(GetLogFor(items[i], depth + 1, i.ToString()));

            return result.ToArray();
        }

        private static LogNode[] GetLogForDictionary(IDictionary dictionary, int depth) =>
            throw new NotImplementedException();

        private static LogNode[] GetLogForFields(object obj, int depth) => throw new NotImplementedException();
    }

    public struct LogEntry
    {
        public readonly LogNode Content;
        public readonly string StackTrace;
        public readonly string Time;
        [CanBeNull] public readonly string FilePath;
        public readonly int? LineNumber;

        public LogEntry(
            LogNode content,
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

    public class LogNode
    {
        [CanBeNull] public readonly IReadOnlyList<LogNode> Children;
        public readonly string Label;

        public LogNode(string label, LogNode[] children = null) {
            Label = label;
            Children = children ?? Array.Empty<LogNode>();
        }
    }
}