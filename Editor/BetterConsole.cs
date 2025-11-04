using Cookie.BetterLogging.Serialization;
using UnityEditor;
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

            Button clearButton = new(() => {
                    BetterLog.Logs.Clear();
                    Redraw();
                }
            ) {
                text = "Clear",
            };
            topBar.Add(clearButton);

            VisualElement entriesContainer = new();
            entriesContainer.AddToClassList("entries");

            for (int i = 0; i < BetterLog.Logs.Count; i++) {
                LogEntry log = BetterLog.Logs[i];
                entriesContainer.Add(GetEntryFor(log, i % 2 == 0));
            }

            rootVisualElement.Add(topBar);
            rootVisualElement.Add(entriesContainer);
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

            container.Add(new Label(Serializer.Serialize(entry.Content)));
            container.Add(new Label(entry.StackTrace));

            return container;
        }

        [MenuItem("Window/Cookie/Better Console")]
        public static void OpenWindow() {
            CreateWindow<BetterConsole>("Better Console");
        }
    }
}