using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Cookie.BetterLogging.Samples
{
    [CustomEditor(typeof(Logger))]
    public class LoggerEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var logger = (Logger)target;

            VisualElement root = new();

            Foldout unityLog = new() { text = "Unity Log" };
            Foldout betterLog = new() { text = "Better Log" };

            Button unityLogList = new() { text = "Unity Log List" };

            Button unityLogNestedList = new() { text = "Unity Log Nested List" };

            Button unityLogDictionary = new() { text = "Unity Log Dictionary" };

            Button betterLogList = new() { text = "Better Log List" };

            Button betterLogNestedList = new() { text = "Better Log Nested List" };

            Button betterLogDictionary = new() { text = "Better Log Dictionary" };

            Button betterLogFormatString = new() { text = "Better Log Format String" };

            unityLogList.clicked += () => logger.UnityLogList();
            unityLogNestedList.clicked += () => logger.UnityLogNestedList();
            unityLogDictionary.clicked += () => logger.UnityLogDictionary();
            betterLogList.clicked += () => logger.BetterLogList();
            betterLogNestedList.clicked += () => logger.BetterLogNestedList();
            betterLogDictionary.clicked += () => logger.BetterLogDictionary();
            betterLogFormatString.clicked += () => logger.BetterLogFormatString();

            var typeField = new PropertyField(serializedObject.FindProperty("logType"));
            root.Add(typeField);
            unityLog.Add(unityLogList);
            unityLog.Add(unityLogNestedList);
            unityLog.Add(unityLogDictionary);
            betterLog.Add(betterLogList);
            betterLog.Add(betterLogNestedList);
            betterLog.Add(betterLogDictionary);
            betterLog.Add(betterLogFormatString);
            root.Add(unityLog);
            root.Add(betterLog);

            return root;
        }
    }
}
