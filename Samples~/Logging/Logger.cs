using System.Collections.Generic;
using UnityEngine;

namespace Cookie.BetterLogging.Samples
{
    public class Logger : MonoBehaviour
    {
        [SerializeField]
        private LogType logType = LogType.Log;

        private readonly Dictionary<string, float> _dictionary = new()
        {
            { "John", 10.5f },
            { "Bob", -15f },
            { "Alice", 10.25f },
        };

        private readonly Dictionary<string[], List<int>> _complexDictionary = new()
        {
            {
                new string[] { "Bob", "Alice" },
                new List<int>() { 1, 2, 3 }
            },
            {
                new string[] { "Guy", "John" },
                new List<int>() { 4, 5, 6 }
            },
        };

        private readonly Vector3[] _list =
        {
            new(1, 1, 1),
            new(1, 2, 3),
            new(1.5f, 10.25f, -8.5f),
            new(4, 5, 6),
        };

        private readonly Vector3[][] _nestedList =
        {
            new Vector3[] { new(1, 2, 3), new(4, 5, 6), new(1, 1, 1) },
            new Vector3[] { new(-1, 10, -5), new(1, -2, 3), new(-1, -1, -1) },
        };

        private readonly LoggerStruct _struct = new(
            "This is a public field!",
            "And i'm a public property encapsulating _privateField!"
        );

        public void UnityLogList()
        {
            Debug.unityLogger.Log(logType, _list);
        }

        public void UnityLogNestedList()
        {
            Debug.unityLogger.Log(logType, _nestedList);
        }

        public void UnityLogDictionary()
        {
            Debug.unityLogger.Log(logType, _dictionary);
        }

        public void UnityLogComplexDictionary()
        {
            Debug.unityLogger.Log(logType, _complexDictionary);
        }

        public void UnityLogStruct()
        {
            Debug.unityLogger.Log(logType, _struct);
        }

        public void BetterLogList()
        {
            BetterLog.WithType(logType, _list);
        }

        public void BetterLogNestedList()
        {
            BetterLog.WithType(logType, _nestedList);
        }

        public void BetterLogDictionary()
        {
            BetterLog.WithType(logType, _dictionary);
        }

        public void BetterLogComplexDictionary()
        {
            BetterLog.WithType(logType, _complexDictionary);
        }

        public void BetterLogStruct()
        {
            BetterLog.WithType(logType, _struct);
        }

        public void BetterLogFormatString()
        {
            BetterLog.WithType(
                logType,
                "List: {0}, Nested list: {1}, Dictionary: {2}, Struct: {3}",
                _list,
                _nestedList,
                _dictionary,
                _struct
            );
        }

        readonly struct LoggerStruct
        {
            public readonly string PublicField;
            public readonly string PublicProperty => _privateField;
            private readonly string _privateField;

            public LoggerStruct(string publicField, string privateField)
            {
                PublicField = publicField;
                _privateField = privateField;
            }
        }
    }
}
