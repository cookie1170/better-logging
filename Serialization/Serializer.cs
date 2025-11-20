using System.Collections;
using JetBrains.Annotations;

namespace Cookie.BetterLogging.Serialization
{
    [PublicAPI]
    public static partial class Serializer
    {
        private const int DepthLimit = 8;

        public static string Serialize<T>(T obj, int depth = DepthLimit) {
            if (obj is null) return "null";
            if (depth < 0) return obj.ToString();

            return obj switch {
                string str => str,
                IDictionary dictionary => SerializeDictionary(dictionary, depth),
                IEnumerable enumerable => SerializeEnumerable(enumerable, depth),
                _ => obj.ToString(),
            };
        }
    }
}