using Cookie.BetterLogging.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cookie.BetterLogging.Editor
{
    public class BetterConsole : EditorWindow
    {
        private StyleSheet _styleSheet;

        private void OnEnable() {
            if (!_styleSheet)
                _styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                    "Packages/com.cookie.better-logging/Editor/BetterConsoleStyle.uss"
                );

            rootVisualElement.styleSheets.Add(_styleSheet);

            BetterLog.OnLog += OnLog;
        }

        private void OnDisable() {
            BetterLog.OnLog -= OnLog;
        }

        private void CreateGUI() {
            VisualElement topBar = new();
            topBar.AddToClassList("top-bar");

            Button clearButton = new(Clear) {
                text = "Clear",
            };
            clearButton.AddToClassList("top-bar-button");

            topBar.Add(clearButton);

            ScrollView entriesContainer = new();
            entriesContainer.scrollOffset = new Vector2(0, entriesContainer.verticalScroller.highValue);
            entriesContainer.AddToClassList("entries");

            for (int i = 0; i < BetterLog.Logs.Count; i++) {
                LogEntry log = BetterLog.Logs[i];
                entriesContainer.Add(GetEntryFor(log, i % 2 == 0));
            }

            rootVisualElement.Add(topBar);
            rootVisualElement.Add(entriesContainer);
        }

        private void Clear() {
            BetterLog.Logs.Clear();
            Redraw();
        }

        private void OnLog(LogEntry _) {
            Redraw();
        }

        private void Redraw() {
            rootVisualElement.Clear();
            CreateGUI();
            Repaint();
        }

        private static VisualElement GetEntryFor(LogEntry entry, bool isAlternate) {
            VisualElement container = new();
            container.AddToClassList("entry");
            container.AddToClassList(isAlternate ? "entry-alternate" : "entry-normal");

            container.Add(new Label($"[{entry.Time}] {Serializer.Serialize(entry.Content)}"));
            container.Add(new Label(entry.StackTrace));

            return container;
        }

        [MenuItem("Window/Cookie/Better Console")]
        public static void OpenWindow() {
            CreateWindow<BetterConsole>("Better Console");
        }
    }
}