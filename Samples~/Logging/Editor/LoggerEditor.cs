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

            Button unityLogComplexDictionary = new() { text = "Unity Log Complex Dictionary" };

            Button unityLogStruct = new() { text = "Unity Log Struct" };

            Button betterLogList = new() { text = "Better Log List" };

            Button betterLogNestedList = new() { text = "Better Log Nested List" };

            Button betterLogDictionary = new() { text = "Better Log Dictionary" };

            Button betterLogComplexDictionary = new() { text = "Better Log Complex Dictionary" };

            Button betterLogStruct = new() { text = "Better Log Struct" };

            Button betterLogFormatString = new() { text = "Better Log Format String" };

            unityLogList.clicked += () => logger.UnityLogList();
            unityLogNestedList.clicked += () => logger.UnityLogNestedList();
            unityLogDictionary.clicked += () => logger.UnityLogDictionary();
            unityLogComplexDictionary.clicked += () => logger.UnityLogComplexDictionary();
            unityLogStruct.clicked += () => logger.UnityLogStruct();
            betterLogList.clicked += () => logger.BetterLogList();
            betterLogNestedList.clicked += () => logger.BetterLogNestedList();
            betterLogDictionary.clicked += () => logger.BetterLogDictionary();
            betterLogComplexDictionary.clicked += () => logger.BetterLogComplexDictionary();
            betterLogStruct.clicked += () => logger.BetterLogStruct();
            betterLogFormatString.clicked += () => logger.BetterLogFormatString();

            var typeField = new PropertyField(serializedObject.FindProperty("logType"));
            root.Add(typeField);
            unityLog.Add(unityLogList);
            unityLog.Add(unityLogNestedList);
            unityLog.Add(unityLogDictionary);
            unityLog.Add(unityLogComplexDictionary);
            unityLog.Add(unityLogStruct);
            betterLog.Add(betterLogList);
            betterLog.Add(betterLogNestedList);
            betterLog.Add(betterLogDictionary);
            betterLog.Add(betterLogComplexDictionary);
            betterLog.Add(betterLogStruct);
            betterLog.Add(betterLogFormatString);
            root.Add(unityLog);
            root.Add(betterLog);

            return root;
        }
    }
}
