using System.Runtime.CompilerServices;
using System.Text;
using Cookie.BetterLogging.Serialization;
using JetBrains.Annotations;
using UnityEngine;

namespace Cookie.BetterLogging
{
    [PublicAPI]
    public static class BetterLog
    {
        public static void Log<T>(
            T obj,
            [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = ""
        ) {
            StringBuilder sb = new();
            sb.AppendLine(Serializer.Serialize(obj));
            sb.AppendLine($"{memberName} (at {filePath}:{lineNumber})");
            Debug.Log(sb.ToString());
        }
    }
}