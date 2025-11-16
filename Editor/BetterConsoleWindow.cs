using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cookie.BetterLogging.Editor
{
    public class BetterConsoleWindow : EditorWindow
    {
        private TreeView _currentEntriesContainer;
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

            // TODO: fix the stack trace
            _currentEntriesContainer = new TreeView {
                makeItem = () => new Label {
                    style = {
                        flexGrow = 1,
                        unityTextAlign = TextAnchor.MiddleLeft,
                    },
                },
                selectionType = SelectionType.Single,
            };

            _currentEntriesContainer.bindItem = (element, i) => {
                ((Label)element).text = _currentEntriesContainer.GetItemDataForIndex<LogNode>(i).Label;
            };

            _currentEntriesContainer.AddToClassList("entries");

            rootVisualElement.Add(topBar);
            rootVisualElement.Add(_currentEntriesContainer);
            rootVisualElement.Add(stackTrace);
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

            List<TreeViewItemData<LogNode>> data = new(BetterLog.Logs.Count);

            int indexOffset = 0;
            for (int i = 0; i < BetterLog.Logs.Count; i++)
                data.Add(GetTreeViewItemData(BetterLog.Logs[i].Content, ref indexOffset));

            _currentEntriesContainer.SetRootItems(data);
            _currentEntriesContainer.RefreshItems();
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