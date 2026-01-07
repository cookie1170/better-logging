using System;
using System.Collections.Generic;
using UnityEditor;

namespace Cookie.BetterLogging.Editor
{
    internal class LogStorage : ScriptableSingleton<LogStorage>
    {
        private const int MaxLogs = 512;
        private const int ClearIfMaxAmount = 64;

        private readonly List<LogEntry> _logs = new();
        public IReadOnlyList<LogEntry> Logs => _logs;
        public event Action<LogEntry> LogReceived;
        public event Action Cleared;
        public bool clearOnPlay = true;
        public bool clearOnRecompile = true;

        private void OnLog(LogEntry entry)
        {
            if (_logs.Count > MaxLogs)
                _logs.RemoveRange(_logs.Count - ClearIfMaxAmount - 1, ClearIfMaxAmount);

            _logs.Add(entry);
            LogReceived?.Invoke(entry);
        }

        private void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (!clearOnPlay)
                return;

            if (change != PlayModeStateChange.EnteredPlayMode)
                return;

            Clear();
        }

        private void OnCompilationFinished()
        {
            if (!clearOnRecompile)
                return;

            Clear();
        }

        private void OnEnable()
        {
            BetterLog.LogReceived += OnLog;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            AssemblyReloadEvents.afterAssemblyReload += OnCompilationFinished;
        }

        private void OnDisable()
        {
            BetterLog.LogReceived -= OnLog;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            AssemblyReloadEvents.afterAssemblyReload -= OnCompilationFinished;
        }

        public void Clear()
        {
            _logs.Clear();
            ClearUnityLog();
            Cleared?.Invoke();
        }

        // reflection D: unity doesn't have an api for this
        // the reason we're clearing it here is so that the bottom black bar with console logs is also cleared
        internal static void ClearUnityLog()
        {
            var assembly = System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.Editor));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
        }
    }
}
