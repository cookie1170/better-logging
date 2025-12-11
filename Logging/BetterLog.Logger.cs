using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cookie.BetterLogging.Serialization;
using UnityEngine;

namespace Cookie.BetterLogging
{
    public static partial class BetterLog
    {
        private static LogNode GetLogFor(object obj, LogInfo info, int depth = DepthLimit, string prefix = null) {
            #if !UNITY_EDITOR
            return new LogNode(Serializer.Serialize(obj), info);
        }
            #else
            if (depth < 0) return new LogNode(AddPrefix(Serializer.Serialize(obj), prefix), info);
            switch (obj) {
                case string str:
                    return new LogNode(AddPrefix(str, prefix), info);
                case Vector2 or Vector3 or Vector4 or Vector2Int or Vector3Int:
                    return new LogNode(AddPrefix(obj.ToString(), prefix), info);
            }

            Type type = obj.GetType();

            if (type.IsPrimitive) return new LogNode(AddPrefix(obj.ToString(), prefix), info);

            var children = obj switch {
                IDictionary dictionary => GetLogForDictionary(dictionary, info, depth),
                IEnumerable enumerable => GetLogForEnumerable(enumerable, info, depth),
                _ => GetLogForFields(obj, info, depth),
            };

            LogNode root = new(AddPrefix(type.Name, prefix), info, children);

            return root;
        }

        private static LogNode[] GetLogForEnumerable(IEnumerable enumerable, LogInfo info, int depth) {
            var items = enumerable.Cast<object>().ToList();

            List<LogNode> result = new(items.Count);

            for (int i = 0; i < items.Count; i++) result.Add(GetLogFor(items[i], info, depth - 1, i.ToString()));

            return result.ToArray();
        }

        private static LogNode[] GetLogForDictionary(IDictionary dictionary, LogInfo info, int depth) {
            List<LogNode> result = new(dictionary.Count);

            foreach (DictionaryEntry entry in dictionary)
                result.Add(GetLogFor(entry.Value, info, depth - 1, Serializer.Serialize(entry.Key, 1)));

            return result.ToArray();
        }

        private static LogNode[] GetLogForFields(object o, object obj, int depth) =>
            throw new NotImplementedException();

        private static string AddPrefix(string name, string prefix) => prefix == null ? name : $"{prefix}: {name}";
        #endif
    }
}