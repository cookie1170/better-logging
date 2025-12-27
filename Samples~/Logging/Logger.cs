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

        public void BetterLogList()
        {
            BetterLog.Log(_list, logType);
        }

        public void BetterLogNestedList()
        {
            BetterLog.Log(_nestedList, logType);
        }

        public void BetterLogDictionary()
        {
            BetterLog.Log(_dictionary, logType);
        }
    }
}
