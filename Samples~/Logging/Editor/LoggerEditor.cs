using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Cookie.BetterLogging.Samples
{
    [CustomEditor(typeof(Logger))]
    public class LoggerEditor : Editor
    {
        public override VisualElement CreateInspectorGUI() {
            var logger = (Logger)target;

            VisualElement root = new();
            PropertyField field = new() {
                bindingPath = "list",
            };
            Button button = new() {
                text = "Log",
            };

            button.clicked += () => logger.Log();

            root.Add(field);
            root.Add(button);

            return root;
        }
    }
}