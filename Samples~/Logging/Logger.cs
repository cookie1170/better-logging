using System.Collections.Generic;
using UnityEngine;

namespace Cookie.BetterLogging.Samples
{
    public class Logger : MonoBehaviour
    {
        [SerializeField] private List<Vector3> list = new();

        public void Log() {
            BetterLog.Log(list);
        }
    }
}