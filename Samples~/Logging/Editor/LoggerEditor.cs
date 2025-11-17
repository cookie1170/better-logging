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

            Foldout unityLog = new() { text = "Unity Log" };
            Foldout betterLog = new() { text = "Better Log" };

            Button unityLogList = new() {
                text = "Unity Log List",
            };

            Button unityLogNestedList = new() {
                text = "Unity Log Nested List",
            };

            Button unityLogDictionary = new() {
                text = "Unity Log Dictionary",
            };

            Button betterLogList = new() {
                text = "Better Log List",
            };

            Button betterLogNestedList = new() {
                text = "Better Log Nested List",
            };

            Button betterLogDictionary = new() {
                text = "Better Log Dictionary",
            };

            unityLogList.clicked += () => logger.UnityLogList();
            unityLogNestedList.clicked += () => logger.UnityLogNestedList();
            unityLogDictionary.clicked += () => logger.UnityLogDictionary();
            betterLogList.clicked += () => logger.BetterLogList();
            betterLogNestedList.clicked += () => logger.BetterLogNestedList();
            betterLogDictionary.clicked += () => logger.BetterLogDictionary();

            // root.Add(field);
            unityLog.Add(unityLogList);
            unityLog.Add(unityLogNestedList);
            unityLog.Add(unityLogDictionary);
            betterLog.Add(betterLogList);
            betterLog.Add(betterLogNestedList);
            betterLog.Add(betterLogDictionary);
            root.Add(unityLog);
            root.Add(betterLog);

            return root;
        }
    }
}