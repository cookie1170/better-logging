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

        private static void OnLogMessageReceived(string message, string stackTrace, LogType type) {
            if (_justDebugLogged) return;

            var logInfo = new LogInfo(stackTrace);
            AddEntry(new LogEntry(GetLogFor(message, logInfo), DateTime.Now));
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
            LogInfo info = new(stackTrace, filePath, lineNumber);
            AddEntry(new LogEntry(GetLogFor(obj, info), DateTime.Now));

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

        private static LogNode GetLogFor(object obj, LogInfo info, int depth = 0, string label = null) {
            if (depth > DepthLimit) return new LogNode(Serializer.Serialize(obj), info);
            switch (obj) {
                case string str:
                    return new LogNode(str, info);
                case Vector2 or Vector3 or Vector4 or Vector2Int or Vector3Int:
                    return new LogNode(obj.ToString(), info);
            }

            LogNode[] children = obj switch {
                IDictionary dictionary => GetLogForDictionary(dictionary, info, depth + 1),
                IEnumerable enumerable => GetLogForEnumerable(enumerable, info, depth + 1),
                _ => GetLogForFields(obj, info, depth + 1),
            };

            LogNode root = new(label ?? obj.GetType().Name, info, children);

            return root;
        }

        private static LogNode[] GetLogForEnumerable(IEnumerable enumerable, LogInfo info, int depth) {
            List<object> items = enumerable.Cast<object>().ToList();

            List<LogNode> result = new();

            for (int i = 0; i < items.Count; i++) result.Add(GetLogFor(items[i], info, depth + 1, i.ToString()));

            return result.ToArray();
        }

        private static LogNode[] GetLogForDictionary(IDictionary dictionary, LogInfo info, int depth) =>
            throw new NotImplementedException();

        private static LogNode[] GetLogForFields(object o, object obj, int depth) =>
            throw new NotImplementedException();
    }

    public struct LogEntry
    {
        public readonly LogNode Content;

        public LogEntry(
            LogNode content,
            DateTime time
        ) {
            Content = content;
            string timeString = time.ToLongTimeString();
            Content.Label = $"[{timeString}] {Content.Label}";
        }
    }

    public class LogNode
    {
        [CanBeNull] public readonly IReadOnlyList<LogNode> Children;
        public readonly LogInfo Info;
        public string Label;

        public LogNode(string label, LogInfo info, LogNode[] children = null) {
            Label = label;
            Info = info;
            Children = children ?? Array.Empty<LogNode>();
        }
    }

    public class LogInfo
    {
        [CanBeNull] public readonly string FilePath;
        public readonly int? LineNumber;
        public readonly string StackTrace;

        public LogInfo(string stackTrace, [CanBeNull] string filePath = null, int? lineNumber = null) {
            StackTrace = stackTrace;
            FilePath = filePath;
            LineNumber = lineNumber;
        }
    }
}