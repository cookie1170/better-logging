using UnityEngine;

namespace Cookie.BetterLogging.Samples
{
    public class Logger : MonoBehaviour
    {
        private readonly Vector3[] _list = {
            new(1, 1, 1), new(1, 2, 3), new(1.5f, 10.25f, -8.5f), new(4, 5, 6),
        };

        private readonly Vector3[][] _nestedList = {
            new Vector3[] {
                new(1, 2, 3), new(4, 5, 6), new(1, 1, 1),
            },
            new Vector3[] {
                new(-1, 10, -5), new(1, -2, 3), new(-1, -1, -1),
            },
        };

        public void UnityLogList() {
            Debug.Log(_list);
        }

        public void UnityLogNestedList() {
            Debug.Log(_nestedList);
        }

        public void BetterLogList() {
            BetterLog.Log(_list);
        }

        public void BetterLogNestedList() {
            BetterLog.Log(_nestedList);
        }
    }
}