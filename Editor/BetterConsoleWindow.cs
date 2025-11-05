using System;
using System.Collections.Generic;
using System.Linq;
using Cookie.BetterLogging.Serialization;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine.UIElements;

namespace Cookie.BetterLogging.Editor
{
    public class BetterConsoleWindow : EditorWindow
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
            _currentEntriesContainer = null;
            BetterLog.OnLog -= OnLog;
        }

        private void CreateGUI() {
            LogEntry? selectedEntry = null;

            VisualElement topBar = new();
            topBar.AddToClassList("top-bar");

            Button clearButton = new(Clear) {
                text = "Clear",
            };
            clearButton.AddToClassList("top-bar-button");

            topBar.Add(clearButton);

            ScrollView stackTrace = new();
            stackTrace.Add(new Label("<b>Stack Trace</b>"));
            stackTrace.AddToClassList("stack-trace");

            Label stackTraceLabel = new();
            stackTraceLabel.AddToClassList("stack-trace-label");

            stackTrace.Add(stackTraceLabel);

            _currentEntriesContainer = new ListView {
                bindItem = BindEntry,
                makeItem = MakeEntry,
                itemsSource = BetterLog.Logs,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
                makeNoneElement = () => null,
            };

            _currentEntriesContainer.selectedIndicesChanged += OnEntrySelected;
            _currentEntriesContainer.itemsChosen += OnEntryChosen;

            _currentEntriesContainer.AddToClassList("entries");

            rootVisualElement.Add(topBar);
            rootVisualElement.Add(_currentEntriesContainer);
            rootVisualElement.Add(stackTrace);
            UpdateStackTraceDisplay();

            return;

            void OnEntrySelected(IEnumerable<int> selectedIndices) {
                int[] indices = selectedIndices as int[] ?? selectedIndices.ToArray();

                if (!indices.Any()) {
                    selectedEntry = null;
                    UpdateStackTraceDisplay();

                    return;
                }

                int index = indices.FirstOrDefault();
                selectedEntry = BetterLog.Logs[index];

                UpdateStackTraceDisplay();
            }

            void OnEntryChosen(IEnumerable<object> items) {
                object chosenItem = items.FirstOrDefault();

                if (chosenItem == null) return;

                var chosenEntry = (LogEntry)chosenItem;
                if (chosenEntry is { FilePath: not null, LineNumber: not null })
                    InternalEditorUtility.OpenFileAtLineExternal(chosenEntry.FilePath, (int)chosenEntry.LineNumber);
            }

            void UpdateStackTraceDisplay() {
                stackTrace.style.display = selectedEntry != null ? DisplayStyle.Flex : DisplayStyle.None;
                stackTraceLabel.text = selectedEntry?.StackTrace ?? "";
            }
        }

        private static VisualElement MakeEntry() {
            VisualElement item = new();
            VisualElement content = new();

            Label contentLabel = new() { name = "content" };

            content.Add(contentLabel);
            item.Add(content);
            item.AddToClassList("entry");
            content.AddToClassList("entry-content");
            contentLabel.AddToClassList("entry-label");

            return item;
        }

        private static void BindEntry(VisualElement element, int index) {
            LogEntry entry = BetterLog.Logs[index];

            element.Q<Label>("content").text = $"[{entry.Time}] {Serializer.Serialize(entry.Content)}";
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
            CreateWindow<BetterConsoleWindow>("Better Console", Type.GetType("ConsoleWindow"));
        }
    }
}