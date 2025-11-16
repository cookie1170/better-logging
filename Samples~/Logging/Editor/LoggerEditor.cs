using UnityEditor;
using UnityEngine.UIElements;

namespace Cookie.BetterLogging.Samples
{
    [CustomEditor(typeof(Logger))]
    public class LoggerEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI() {
            var logger = (Logger)target;

            VisualElement root = new();
            // PropertyField field = new() {
            //     bindingPath = "list",
            // };

            Button betterLogList = new() {
                text = "Better Log List",
            };

            Button unityLogList = new() {
                text = "Unity Log List",
            };

            Button betterLogNestedList = new() {
                text = "Better Log Nested List",
            };

            Button unityLogNestedList = new() {
                text = "Unity Log Nested List",
            };

            betterLogList.clicked += () => logger.BetterLogList();
            unityLogList.clicked += () => logger.UnityLogList();
            betterLogNestedList.clicked += () => logger.BetterLogNestedList();
            unityLogNestedList.clicked += () => logger.UnityLogNestedList();

            // root.Add(field);
            root.Add(betterLogList);
            root.Add(unityLogList);
            root.Add(betterLogNestedList);
            root.Add(unityLogNestedList);

            return root;
        }
    }
}