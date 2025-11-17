using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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
        private const int DepthLimit = 8;
        private const int MaxLogs = 256;
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
            #if UNITY_EDITOR // avoid the expensive stuff if we're not in the editor and only serialize the object, as we're not gonna see them in the log files anyway 
            string stackTrace = FormatStackTrace(new StackTrace(true).ToString());
            LogInfo info = new(stackTrace, filePath, lineNumber);
            AddEntry(new LogEntry(GetLogFor(obj, info), DateTime.Now));
            #endif

            _justDebugLogged = true;
            Debug.Log(serializedObj);
            _justDebugLogged = false;
        }

        private static void AddEntry(LogEntry entry) {
            if (Logs.Count >= MaxLogs) Logs.RemoveRange(0, Logs.Count - MaxLogs + ClearIfMaxAmount);
            Logs.Add(entry);
            OnLog?.Invoke(entry);
        }

        #if UNITY_EDITOR
        private static string FormatStackTrace(string originalTrace) {
            string[] splitTrace = originalTrace.Split(Environment.NewLine)
                .Where(s => !s.Contains("Cookie.BetterLogging.BetterLog.Log")).ToArray();
            string trace = splitTrace.Aggregate((s1, s2) => s1 + Environment.NewLine + s2);

            return trace.Replace(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar, "");
        }
        #endif

        private static LogNode GetLogFor(object obj, LogInfo info, int depth = DepthLimit, string prefix = null) {
            #if !UNITY_EDITOR
            return new LogNode(Serializer.Serialize(obj), info);
        }
            #else
            if (depth < 0) return new LogNode(AddPrefix(Serializer.Serialize(obj), prefix), info);
            switch (obj) {
                case string str:
                    return new LogNode(AddPrefix(str, prefix), info);
                case Vector2 or Vector3 or Vector4 or Vector2Int or Vector3Int:
                    return new LogNode(AddPrefix(obj.ToString(), prefix), info);
            }

            Type type = obj.GetType();

            if (type.IsPrimitive) return new LogNode(AddPrefix(obj.ToString(), prefix), info);

            LogNode[] children = obj switch {
                IDictionary dictionary => GetLogForDictionary(dictionary, info, depth),
                IEnumerable enumerable => GetLogForEnumerable(enumerable, info, depth),
                _ => GetLogForFields(obj, info, depth),
            };

            LogNode root = new(AddPrefix(type.Name, prefix), info, children);

            return root;
        }

        private static LogNode[] GetLogForEnumerable(IEnumerable enumerable, LogInfo info, int depth) {
            List<object> items = enumerable.Cast<object>().ToList();

            List<LogNode> result = new(items.Count);

            for (int i = 0; i < items.Count; i++) result.Add(GetLogFor(items[i], info, depth - 1, i.ToString()));

            return result.ToArray();
        }

        private static LogNode[] GetLogForDictionary(IDictionary dictionary, LogInfo info, int depth) {
            List<LogNode> result = new(dictionary.Count);

            foreach (DictionaryEntry entry in dictionary)
                result.Add(GetLogFor(entry.Value, info, depth - 1, Serializer.Serialize(entry.Key, 1)));

            return result.ToArray();
        }

        private static LogNode[] GetLogForFields(object o, object obj, int depth) =>
            throw new NotImplementedException();

        private static string AddPrefix(string name, string prefix) => prefix == null ? name : $"{prefix}: {name}";
        #endif
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
        /// <summary>
        ///     The children of the node
        /// </summary>
        /// <remarks>
        ///     This is always empty during runtime to avoid the expensive calculations
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