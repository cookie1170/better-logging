using System.Collections;
using JetBrains.Annotations;

namespace Cookie.BetterLogging.Serialization
{
    [PublicAPI]
    public static partial class Serializer
    {
        public static string Serialize<T>(T obj) {
            return obj switch {
                IDictionary dictionary => SerializeDictionary(dictionary),
                IEnumerable enumerable => SerializeEnumerable(enumerable),
                _ => obj.ToString(),
            };
        }
    }
}