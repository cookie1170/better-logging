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

            Foldout unityLog = new() { text = "Unity Log" };
            Foldout betterLog = new() { text = "Better Log" };

            Button unityLogList = new() {
                text = "Unity Log List",
            };

            Button unityLogNestedList = new() {
                text = "Unity Log Nested List",
            };

            Button betterLogList = new() {
                text = "Better Log List",
            };

            Button betterLogNestedList = new() {
                text = "Better Log Nested List",
            };

            unityLogList.clicked += () => logger.UnityLogList();
            unityLogNestedList.clicked += () => logger.UnityLogNestedList();
            betterLogList.clicked += () => logger.BetterLogList();
            betterLogNestedList.clicked += () => logger.BetterLogNestedList();

            // root.Add(field);
            unityLog.Add(unityLogList);
            unityLog.Add(unityLogNestedList);
            betterLog.Add(betterLogList);
            betterLog.Add(betterLogNestedList);
            root.Add(unityLog);
            root.Add(betterLog);

            return root;
        }
    }
}