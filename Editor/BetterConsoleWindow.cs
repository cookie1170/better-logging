using System;
using System.Collections.Generic;
using System.Linq;
using Cookie.BetterLogging.TreeGeneration;
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
        private readonly HashSet<int> _expandedIDs = new();
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
            _expandedIDs?.Clear();
            _entries = null;
            LogStorage.instance.LogReceived -= OnLog;
            LogStorage.instance.Cleared -= Refresh;
        }

        private void CreateGUI()
        {
            LogNode? selectedEntry = null;

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
            ToolbarSearchField search = new() { viewDataKey = "log-search" };
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
                ((Label)element).text = _entries.GetItemDataForIndex<LogNode>(i).GetLabel();
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

            if (_entries == null)
                return;

            (int id, bool allChildren, bool expanded) = (
                args.id,
                args.isAppliedToAllChildren,
                args.isExpanded
            );

            // if we just expanded/collapsed one - that's easy
            if (!allChildren)
            {
                if (expanded)
                    _expandedIDs.Add(id);
                else
                    _expandedIDs.Remove(id);

                return;
            }

            // otherwise, we need to do this for all the children recursively
            Stack<int> stack = new();
            stack.Push(id);
            while (stack.Count > 0)
            {
                int childId = stack.Pop();

                if (expanded)
                    _expandedIDs.Add(childId);
                else
                    _expandedIDs.Remove(childId);

                int index = _entries.viewController.GetIndexForId(childId);
                IEnumerable<int> children = _entries.GetChildrenIdsForIndex(index);

                foreach (int child in children)
                    stack.Push(child);
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
            _expandedIDs.Clear();
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

                data.Add(GetTreeViewItemData(LogStorage.instance.Logs[i], ref indexOffset));
            }

            _entries.SetRootItems(data);
            _isBeingRefreshed = true;

            foreach (int id in _expandedIDs)
                _entries.ExpandItem(id, false, false);

            _entries.RefreshItems();

            _isBeingRefreshed = false;

            if (LogStorage.instance.Logs.Count > 0 && _isVisible)
                _entries.ScrollToItem(-1);
        }

        private static TreeViewItemData<LogNode> GetTreeViewItemData(
            LogEntry entry,
            ref int indexOffset
        )
        {
            int index = 0 + indexOffset;

            var result = Process(new LogNode(entry.Content, entry.Info));
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

        /// <summary>
        /// Wrapper struct around Node
        /// </summary>
        // is there a better way to do this? just copying all the fields seems dumb..
        // but i can't really think of a way to make it work with the children so idk?
        struct LogNode
        {
            /// <summary>
            /// The name of the node
            /// </summary>
            public string Name;

            /// <summary>
            /// The prefix of the node. This could be something like an array index, dictionary key or a field name
            /// </summary>
            public string Prefix;

            /// <summary>
            /// The type of the node
            /// </summary>
            /// <seealso cref="TreeGeneration.NodeType"/>
            public Node.Type NodeType;

            /// <summary>
            /// The type of the object
            /// </summary>
            public Type ObjectType;

            /// <summary>
            /// The children of the node, <c>null</c> if it's a leaf node
            /// </summary>
            public List<LogNode> Children;

            /// <summary>
            /// The LogInfo associated with this node
            /// </summary>
            public LogInfo Info;

            /// <summary>
            /// Is the node a root node?
            /// </summary>
            public bool IsRoot;

            /// <summary>
            /// Is this node a leaf node?
            /// </summary>
            /// <returns>True if <c>Children</c> is null or empty</returns>
            public readonly bool IsLeaf() => Children == null || Children.Count <= 0;

            /// <summary>
            /// Whether the node matches <c>searchQuery</c>
            /// </summary>
            /// <param name="searchQuery">The query to search for</param>
            /// <returns>True if <c>searchQuery</c> appears anywhere in the prefix or name of this node or its' children</returns>
            public readonly bool MatchesSearchQuery(string searchQuery)
            {
                if (Name.Contains(searchQuery, StringComparison.InvariantCultureIgnoreCase))
                    return true;

                if (
                    Prefix != null
                    && Prefix.Contains(searchQuery, StringComparison.InvariantCultureIgnoreCase)
                )
                    return true;

                if (IsLeaf())
                    return false;

                return Children.Any(c => c.MatchesSearchQuery(searchQuery));
            }

            private const string ErrorColour = "#ff534a";
            private const string WarningColour = "#ffc107";

            public readonly string GetLabel()
            {
                string timeString = Info.Time.ToLongTimeString();
                string infoPrefix = "";
                if (IsRoot)
                {
                    infoPrefix =
                        Info.Type == LogType.Log
                            ? $"[{timeString}] "
                            : $"[{timeString} - {Info.Type}] ";
                }

                string prefix = Prefix == null ? "" : $"{Prefix}: ";

                return Info.Type switch
                {
                    LogType.Assert or LogType.Exception or LogType.Error =>
                        $"<color=\"{ErrorColour}\"><u>{infoPrefix}{prefix}{Name}</u></color>",
                    LogType.Warning =>
                        $"<color=\"{WarningColour}\"><u>{infoPrefix}{prefix}{Name}</u></color>",
                    _ => $"{infoPrefix}{prefix}{Name}",
                };
            }

            public LogNode(Node source, LogInfo info, bool isRoot = true)
            {
                Info = info;
                Name = source.Name;
                Prefix = source.Prefix;
                NodeType = source.NodeType;
                ObjectType = source.ObjectType;
                IsRoot = isRoot;
                Children = source.Children.Select(c => new LogNode(c, info, false)).ToList();
            }
        }
    }
}
