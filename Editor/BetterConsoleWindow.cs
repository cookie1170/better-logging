using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace Cookie.BetterLogging.Editor
{
    [EditorWindowTitle(title = "Better Console")]
    public class BetterConsoleWindow : EditorWindow
    {
        private const string StylesheetPath =
            "Packages/com.cookie.better-logging/Editor/BetterConsoleStyle.uss";
        private const string LinkCursor = "link-cursor";
        private const string StackTrace = "stack-trace";
        private const string StackTraceFoldout = "stack-trace-foldout";
        private const string StackTraceView = "stack-trace-view";
        private const string StackTraceLabel = "stack-trace-label";
        private readonly HashSet<(int id, bool isExpanded, bool allChildren)> _expandedItems =
            new();
        private TreeView _entries;
        private StyleSheet _styleSheet;
        private bool _isBeingRefreshed;
        private bool _isVisible;
        private string _searchQuery = "";

        private void OnEnable()
        {
            if (!_styleSheet)
                _styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StylesheetPath);

            rootVisualElement.styleSheets.Add(_styleSheet);

            LogStorage.instance.LogReceived += OnLog;
            LogStorage.instance.Cleared += Refresh;
        }

        private void OnDisable()
        {
            _expandedItems?.Clear();
            _entries = null;
            LogStorage.instance.LogReceived -= OnLog;
            LogStorage.instance.Cleared -= Refresh;
        }

        private void CreateGUI()
        {
            LogNode selectedEntry = null;

            Toolbar toolbar = new();

            ToolbarButton clearButton = new(Clear) { text = "Clear" };

            ToolbarMenu clearOn = new();
            clearOn.menu.AppendAction(
                "Clear on play",
                (a) => LogStorage.instance.clearOnPlay = !LogStorage.instance.clearOnPlay,
                (a) =>
                    LogStorage.instance.clearOnPlay
                        ? DropdownMenuAction.Status.Checked
                        : DropdownMenuAction.Status.Normal
            );
            clearOn.menu.AppendAction(
                "Clear on recompile",
                (a) => LogStorage.instance.clearOnRecompile = !LogStorage.instance.clearOnRecompile,
                (a) =>
                    LogStorage.instance.clearOnRecompile
                        ? DropdownMenuAction.Status.Checked
                        : DropdownMenuAction.Status.Normal
            );

            ToolbarSpacer spacer = new() { style = { flexGrow = 1f } };
            ToolbarSearchField search = new();
            search.RegisterValueChangedCallback(OnSearch);

            toolbar.Add(clearButton);
            toolbar.Add(clearOn);
            toolbar.Add(spacer);
            toolbar.Add(search);

            VisualElement stackTraceContainer = new();
            stackTraceContainer.AddToClassList(StackTrace);

            Foldout stackTraceFoldout = new() { text = "<b>Stack Trace</b>", value = false };
            stackTraceFoldout.AddToClassList(StackTraceFoldout);
            stackTraceContainer.Add(stackTraceFoldout);

            ScrollView stackTraceView = new();
            stackTraceView.AddToClassList(StackTraceView);
            stackTraceFoldout.Add(stackTraceView);

            Label stackTraceLabel = new();
            stackTraceLabel.AddToClassList(StackTraceLabel);
            stackTraceLabel.RegisterCallback<PointerUpLinkTagEvent>(LinkOnPointerUp);

            stackTraceLabel.RegisterCallback<PointerOverLinkTagEvent>(LinkOnPointerOver);

            stackTraceLabel.RegisterCallback<PointerOutLinkTagEvent>(LinkOnPointerOut);

            stackTraceView.Add(stackTraceLabel);

            if (_entries != null)
            {
                _entries.itemsChosen -= OnEntryChosen;
                _entries.selectedIndicesChanged -= OnEntrySelected;
                _entries.itemExpandedChanged -= OnItemExpandedChanged;
            }

            _entries = new TreeView
            {
                makeItem = MakeItem,
                selectionType = SelectionType.Single,
                showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
            };

            _entries.bindItem = (element, i) =>
            {
                ((Label)element).text = _entries.GetItemDataForIndex<LogNode>(i).Label;
            };

            _entries.itemsChosen += OnEntryChosen;
            _entries.selectedIndicesChanged += OnEntrySelected;
            _entries.itemExpandedChanged += OnItemExpandedChanged;

            _entries.AddToClassList("entries");

            rootVisualElement.Add(toolbar);
            rootVisualElement.Add(_entries);
            rootVisualElement.Add(stackTraceContainer);

            UpdateStackTraceDisplay();
            Refresh();

            return;

            void LinkOnPointerUp(PointerUpLinkTagEvent evt)
            {
                int separatorIndex = evt.linkID.LastIndexOf(':');
                string path = evt.linkID[..separatorIndex];
                int line = int.Parse(evt.linkID[(separatorIndex + 1)..]);
                InternalEditorUtility.OpenFileAtLineExternal(path, line);
            }

            void LinkOnPointerOver(PointerOverLinkTagEvent evt)
            {
                stackTraceLabel.AddToClassList(LinkCursor);
            }

            void LinkOnPointerOut(PointerOutLinkTagEvent evt)
            {
                stackTraceLabel.RemoveFromClassList(LinkCursor);
            }

            void OnEntrySelected(IEnumerable<int> selectedIndices)
            {
                int[] indices = selectedIndices as int[] ?? selectedIndices.ToArray();

                if (!indices.Any())
                {
                    selectedEntry = null;
                    UpdateStackTraceDisplay();

                    return;
                }

                int index = indices.FirstOrDefault();
                selectedEntry = _entries?.GetItemDataForIndex<LogNode>(index);

                UpdateStackTraceDisplay();
            }

            void OnEntryChosen(IEnumerable<object> items)
            {
                object chosenItem = items.FirstOrDefault();

                if (chosenItem == null)
                    return;

                LogInfo info = ((LogNode)chosenItem).Info;
                if (info is not { FilePath: not null, LineNumber: not null })
                    return;

                if (info.Column is not null)
                {
                    InternalEditorUtility.OpenFileAtLineExternal(
                        info.FilePath,
                        (int)info.LineNumber,
                        (int)info.Column
                    );
                }
                else
                {
                    InternalEditorUtility.OpenFileAtLineExternal(
                        info.FilePath,
                        (int)info.LineNumber
                    );
                }
            }

            void UpdateStackTraceDisplay()
            {
                stackTraceLabel.text = selectedEntry?.Info.StackTrace ?? "No entry selected";
            }
        }

        private void OnSearch(ChangeEvent<string> evt)
        {
            _searchQuery = evt.newValue;
            Refresh();
        }

        private void OnBecameInvisible()
        {
            _isVisible = false;
        }

        private void OnBecameVisible()
        {
            _isVisible = true;
        }

        private void OnItemExpandedChanged(TreeViewExpansionChangedArgs args)
        {
            if (_isBeingRefreshed)
                return; // hacky

            // awful
            (int id, bool isExpanded, bool isAppliedToAllChildren) item = (
                args.id,
                args.isExpanded,
                args.isAppliedToAllChildren
            );
            if (args.isExpanded)
            {
                _expandedItems.Add(item);
            }
            else
            {
                (int id, bool, bool isAppliedToAllChildren) expanded = (
                    args.id,
                    true,
                    args.isAppliedToAllChildren
                );
                if (_expandedItems.Contains(expanded))
                    _expandedItems.Remove((args.id, true, args.isAppliedToAllChildren));
                else
                    _expandedItems.Add(item);
            }
        }

        private static VisualElement MakeItem()
        {
            var label = new Label
            {
                style = { flexGrow = 1, unityTextAlign = TextAnchor.MiddleLeft },
            };
            label.AddToClassList("entry-label");

            return label;
        }

        private void Clear()
        {
            _expandedItems.Clear();
            LogStorage.instance.Clear();
        }

        private void OnLog(LogEntry _)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (_entries == null)
                return;

            List<TreeViewItemData<LogNode>> data = new(LogStorage.instance.Logs.Count);

            int indexOffset = 0;
            for (int i = 0; i < LogStorage.instance.Logs.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(_searchQuery))
                {
                    if (!LogStorage.instance.Logs[i].Content.MatchesSearchQuery(_searchQuery))
                        continue;
                }

                data.Add(GetTreeViewItemData(LogStorage.instance.Logs[i].Content, ref indexOffset));
            }

            _entries.SetRootItems(data);
            _isBeingRefreshed = true;

            foreach ((int id, bool isExpanded, bool allChildren) in _expandedItems)
            {
                if (isExpanded)
                    _entries.ExpandItem(id, allChildren, false);
                else
                    _entries.CollapseItem(id, allChildren, false);
            }

            _entries.RefreshItems();

            _isBeingRefreshed = false;

            if (LogStorage.instance.Logs.Count > 0 && _isVisible)
                _entries.ScrollToItem(-1);
        }

        private static TreeViewItemData<LogNode> GetTreeViewItemData(
            LogNode item,
            ref int indexOffset
        )
        {
            int index = 0 + indexOffset;

            var result = Process(item);
            indexOffset = index + 1;

            return result;

            TreeViewItemData<LogNode> Process(LogNode i)
            {
                List<TreeViewItemData<LogNode>> children = null;

                if (i.Children is { Count: > 0 })
                {
                    children = new List<TreeViewItemData<LogNode>>(i.Children.Count);
                    foreach (LogNode child in i.Children)
                        children.Add(Process(child));
                }

                TreeViewItemData<LogNode> data = new(index++, i, children);

                return data;
            }
        }

        [MenuItem("Window/Cookie/Better Console")]
        public static void OpenWindow()
        {
            CreateWindow<BetterConsoleWindow>(Type.GetType("ConsoleWindow"));
        }
    }
}
