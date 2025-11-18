using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace Cookie.BetterLogging.Editor
{
    public class BetterConsoleWindow : EditorWindow
    {
        private const string LinkCursorClassName = "link-cursor";
        private readonly HashSet<(int id, bool isExpanded, bool allChildren)> _expandedItems = new();
        private TreeView _currentEntriesContainer;
        private bool _isBeingRefreshed;
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
            LogNode selectedEntry = null;

            VisualElement topBar = new();
            topBar.AddToClassList("top-bar");

            Button clearButton = new(Clear) {
                text = "Clear",
            };
            clearButton.AddToClassList("top-bar-button");

            topBar.Add(clearButton);

            VisualElement stackTraceContainer = new();
            stackTraceContainer.AddToClassList("stack-trace");

            Foldout stackTraceFoldout = new() { text = "<b>Stack Trace</b>", value = false };
            stackTraceFoldout.AddToClassList("stack-trace-foldout");
            stackTraceContainer.Add(stackTraceFoldout);

            ScrollView stackTraceView = new();
            stackTraceView.AddToClassList("stack-trace-view");
            stackTraceFoldout.Add(stackTraceView);

            Label stackTraceLabel = new();
            stackTraceLabel.AddToClassList("stack-trace-label");
            stackTraceLabel.RegisterCallback<PointerUpLinkTagEvent>(LinkOnPointerUp);

            stackTraceLabel.RegisterCallback<PointerOverLinkTagEvent>(LinkOnPointerOver);

            stackTraceLabel.RegisterCallback<PointerOutLinkTagEvent>(LinkOnPointerOut);

            stackTraceView.Add(stackTraceLabel);

            if (_currentEntriesContainer != null) {
                _currentEntriesContainer.itemsChosen -= OnEntryChosen;
                _currentEntriesContainer.selectedIndicesChanged -= OnEntrySelected;
                _currentEntriesContainer.itemExpandedChanged -= OnItemExpandedChanged;
            }

            _currentEntriesContainer = new TreeView {
                makeItem = MakeItem,
                selectionType = SelectionType.Single,
                showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
            };

            _currentEntriesContainer.bindItem = (element, i) => {
                ((Label)element).text = _currentEntriesContainer.GetItemDataForIndex<LogNode>(i).Label;
            };

            _currentEntriesContainer.itemsChosen += OnEntryChosen;
            _currentEntriesContainer.selectedIndicesChanged += OnEntrySelected;
            _currentEntriesContainer.itemExpandedChanged += OnItemExpandedChanged;

            _currentEntriesContainer.AddToClassList("entries");

            rootVisualElement.Add(topBar);
            rootVisualElement.Add(_currentEntriesContainer);
            rootVisualElement.Add(stackTraceContainer);

            UpdateStackTraceDisplay();

            return;


            void LinkOnPointerUp(PointerUpLinkTagEvent evt) {
                int separatorIndex = evt.linkID.LastIndexOf(':');
                string path = evt.linkID[..separatorIndex];
                int line = int.Parse(evt.linkID[(separatorIndex + 1)..]);
                InternalEditorUtility.OpenFileAtLineExternal(path, line);
            }

            void LinkOnPointerOver(PointerOverLinkTagEvent evt) {
                stackTraceLabel.AddToClassList(LinkCursorClassName);
            }

            void LinkOnPointerOut(PointerOutLinkTagEvent evt) {
                stackTraceLabel.RemoveFromClassList(LinkCursorClassName);
            }

            void OnEntrySelected(IEnumerable<int> selectedIndices) {
                int[] indices = selectedIndices as int[] ?? selectedIndices.ToArray();

                if (!indices.Any()) {
                    selectedEntry = null;
                    UpdateStackTraceDisplay();

                    return;
                }

                int index = indices.FirstOrDefault();
                selectedEntry = _currentEntriesContainer?.GetItemDataForIndex<LogNode>(index);

                UpdateStackTraceDisplay();
            }

            void OnEntryChosen(IEnumerable<object> items) {
                object chosenItem = items.FirstOrDefault();

                if (chosenItem == null) return;

                LogInfo info = ((LogNode)chosenItem).Info;
                if (info is { FilePath: not null, LineNumber: not null })
                    InternalEditorUtility.OpenFileAtLineExternal(info.FilePath, (int)info.LineNumber);
            }

            void UpdateStackTraceDisplay() {
                stackTraceLabel.text = selectedEntry?.Info.StackTrace ?? "No entry selected";
            }
        }

        private void OnItemExpandedChanged(TreeViewExpansionChangedArgs args) {
            if (_isBeingRefreshed) return; // hacky

            // awful
            (int id, bool isExpanded, bool isAppliedToAllChildren) item = (args.id, args.isExpanded,
                args.isAppliedToAllChildren);
            if (args.isExpanded) {
                _expandedItems.Add(item);
            } else {
                (int id, bool, bool isAppliedToAllChildren) expanded = (args.id, true, args.isAppliedToAllChildren);
                if (_expandedItems.Contains(expanded))
                    _expandedItems.Remove((args.id, true, args.isAppliedToAllChildren));
                else _expandedItems.Add(item);
            }
        }

        private static VisualElement MakeItem() {
            var label = new Label { style = { flexGrow = 1, unityTextAlign = TextAnchor.MiddleLeft } };
            label.AddToClassList("entry-label");

            return label;
        }

        private void Clear() {
            BetterLog.Logs.Clear();
            _expandedItems.Clear();
            Refresh();
        }

        private void OnLog(LogEntry _) {
            Refresh();
        }

        private void Refresh() {
            if (_currentEntriesContainer == null) return;

            List<TreeViewItemData<LogNode>> data = new(BetterLog.Logs.Count);

            int indexOffset = 0;
            for (int i = 0; i < BetterLog.Logs.Count; i++)
                data.Add(GetTreeViewItemData(BetterLog.Logs[i].Content, ref indexOffset));

            _currentEntriesContainer.SetRootItems(data);
            _isBeingRefreshed = true;

            foreach ((int id, bool isExpanded, bool allChildren) item in _expandedItems) {
                if (item.isExpanded)
                    _currentEntriesContainer.ExpandItem(item.id, item.allChildren, false);
                else
                    _currentEntriesContainer.CollapseItem(item.id, item.allChildren, false);
            }

            _currentEntriesContainer.RefreshItems();

            _isBeingRefreshed = false;

            if (BetterLog.Logs.Count > 0) _currentEntriesContainer.ScrollToItem(-1);
        }


        private static TreeViewItemData<LogNode> GetTreeViewItemData(LogNode item, ref int indexOffset) {
            int index = 0 + indexOffset;

            TreeViewItemData<LogNode> result = Process(item);
            indexOffset = index + 1;

            return result;

            TreeViewItemData<LogNode> Process(LogNode i) {
                List<TreeViewItemData<LogNode>> children = null;

                if (i.Children is { Count: > 0 }) {
                    children = new List<TreeViewItemData<LogNode>>(i.Children.Count);
                    foreach (LogNode child in i.Children) children.Add(Process(child));
                }

                TreeViewItemData<LogNode> data = new(index++, i, children);

                return data;
            }
        }

        [MenuItem("Window/Cookie/Better Console")]
        public static void OpenWindow() {
            CreateWindow<BetterConsoleWindow>("Better Console", Type.GetType("ConsoleWindow"));
        }
    }
}