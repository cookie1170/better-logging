using Cookie.BetterLogging.Serialization;
using UnityEditor;
using UnityEngine.UIElements;

namespace Cookie.BetterLogging.Editor
{
    public class BetterConsole : EditorWindow
    {
        private ListView _currentEntriesContainer;
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

            _currentEntriesContainer = new ListView {
                bindItem = BindEntry,
                makeItem = MakeEntry,
                itemsSource = BetterLog.Logs,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                showAlternatingRowBackgrounds = AlternatingRowBackground.All,
                makeNoneElement = () => null,
            };

            _currentEntriesContainer.AddToClassList("entries");

            rootVisualElement.Add(topBar);
            rootVisualElement.Add(_currentEntriesContainer);
        }

        private static VisualElement MakeEntry() {
            VisualElement item = new();
            VisualElement content = new();

            Label contentLabel = new() { name = "content" };
            Label traceLabel = new() { name = "stackTrace" };

            content.Add(contentLabel);
            content.Add(traceLabel);
            item.Add(content);
            item.AddToClassList("entry");
            content.AddToClassList("entry-content");

            return item;
        }

        private static void BindEntry(VisualElement element, int index) {
            LogEntry entry = BetterLog.Logs[index];

            element.Q<Label>("content").text = $"[{entry.Time}] {Serializer.Serialize(entry.Content)}";
            element.Q<Label>("stackTrace").text = entry.StackTrace;
        }

        private void Clear() {
            BetterLog.Logs.Clear();
            Redraw();
        }

        private void OnLog(LogEntry _) {
            Redraw();
        }

        private void Redraw() {
            if (_currentEntriesContainer == null) return;

            _currentEntriesContainer.RefreshItems();
            if (BetterLog.Logs.Count > 0) _currentEntriesContainer.ScrollToItem(BetterLog.Logs.Count - 1);
        }

        [MenuItem("Window/Cookie/Better Console")]
        public static void OpenWindow() {
            CreateWindow<BetterConsole>("Better Console");
        }
    }
}