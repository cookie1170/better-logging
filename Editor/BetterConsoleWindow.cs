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
        private const string StylesheetPath = "Packages/com.cookie.better-logging/Editor/BetterConsoleStyle.uss";
        private const string LinkCursorClassName = "link-cursor";
        private readonly HashSet<(int id, bool isExpanded, bool allChildren)> _expandedItems = new();
        private TreeView _entries;
        private bool _isBeingRefreshed;
        private bool _isVisible;
        private StyleSheet _styleSheet;

        private void OnEnable() {
            if (!_styleSheet) _styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StylesheetPath);

            rootVisualElement.styleSheets.Add(_styleSheet);

            BetterLog.OnLog += OnLog;
        }

        private void OnDisable() {
            _expandedItems?.Clear();
            _entries = null;
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

            if (_entries != null) {
                _entries.itemsChosen -= OnEntryChosen;
                _entries.selectedIndicesChanged -= OnEntrySelected;
                _entries.itemExpandedChanged -= OnItemExpandedChanged;
            }

            _entries = new TreeView {
                makeItem = MakeItem,
                selectionType = SelectionType.Single,
                showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
            };

            _entries.bindItem = (element, i) => {
                ((Label)element).text = _entries.GetItemDataForIndex<LogNode>(i).Label;
            };

            _entries.itemsChosen += OnEntryChosen;
            _entries.selectedIndicesChanged += OnEntrySelected;
            _entries.itemExpandedChanged += OnItemExpandedChanged;

            _entries.AddToClassList("entries");

            rootVisualElement.Add(topBar);
            rootVisualElement.Add(_entries);
            rootVisualElement.Add(stackTraceContainer);

            UpdateStackTraceDisplay();
            Refresh();

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
                selectedEntry = _entries?.GetItemDataForIndex<LogNode>(index);

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

        private void OnBecameInvisible() {
            _isVisible = false;
        }

        private void OnBecameVisible() {
            _isVisible = true;
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
            if (_entries == null) return;

            List<TreeViewItemData<LogNode>> data = new(BetterLog.Logs.Count);

            int indexOffset = 0;
            for (int i = 0; i < BetterLog.Logs.Count; i++)
                data.Add(GetTreeViewItemData(BetterLog.Logs[i].Content, ref indexOffset));

            _entries.SetRootItems(data);
            _isBeingRefreshed = true;

            foreach ((int id, bool isExpanded, bool allChildren) item in _expandedItems) {
                if (item.isExpanded)
                    _entries.ExpandItem(item.id, item.allChildren, false);
                else
                    _entries.CollapseItem(item.id, item.allChildren, false);
            }

            _entries.RefreshItems();

            _isBeingRefreshed = false;

            if (BetterLog.Logs.Count > 0 && _isVisible) _entries.ScrollToItem(-1);
        }


        private static TreeViewItemData<LogNode> GetTreeViewItemData(LogNode item, ref int indexOffset) {
            int index = 0 + indexOffset;

            var result = Process(item);
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