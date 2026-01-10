using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cookie.BetterLogging.Serialization;
using Cookie.BetterLogging.TreeGeneration;
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

            var logInfo = new LogInfo(type, DateTime.Now, stackTrace);
            LogEntry(new LogEntry(new Node(message, Node.Type.Simple, typeof(string)), logInfo));
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
            List<Node> argTrees = args.Select(TreeGenerator.GenerateTree).ToList();
            string message = string.Format(format, argTrees.Select(Serializer.Serialize).ToArray());

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
            LogInfo info = new(type, DateTime.Now, traceText, filePath, lineNumber, column);
            LogEntry entry;
            if (format == "{0}" && args.Length > 0)
            {
                entry = new LogEntry(argTrees[0], info);
            }
            else
            {
                entry = new LogEntry(GetFormatNode(format, argTrees), info);
            }

            LogEntry(entry);
#endif

            _justDebugLogged = true;
            Debug.unityLogger.Log(type, message);
            _justDebugLogged = false;
        }

        private static Node GetFormatNode(string format, List<Node> argTrees)
        {
            List<Node> children = new();
            string[] newFormatArgs = new string[argTrees.Count];
            for (int i = 0; i < argTrees.Count; i++)
            {
                newFormatArgs[i] = $"({i})"; // (1) instead of {1} because i think it looks nicer. sue me
                Node tree = argTrees[i];
                tree.Prefix = i.ToString();
                children.Add(tree);
            }

            return new Node(
                string.Format(format, newFormatArgs),
                Node.Type.Object,
                typeof(string),
                children
            );
        }
    }
}
