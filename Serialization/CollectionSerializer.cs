using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace Cookie.BetterLogging.Serialization
{
    public static partial class Serializer
    {
        public static string SerializeDictionary(IDictionary dictionary) {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));

            StringBuilder sb = new();
            sb.Append("{ ");
            foreach (DictionaryEntry entry in dictionary) {
                sb.Append("[");
                sb.Append(entry.Key);
                sb.Append(": ");
                sb.Append(entry.Value);
                sb.Append("]");
                sb.Append(", ");
            }

            sb.Remove(sb.Length - 2, 2);
            sb.Append(" }");

            return sb.ToString();
        }

        public static string SerializeEnumerable(IEnumerable enumerable) {
            if (enumerable == null) throw new ArgumentNullException(nameof(enumerable));

            StringBuilder sb = new();
            sb.Append("[");
            object[] array = enumerable as object[] ?? enumerable.Cast<object>().ToArray();

            foreach (object element in array) {
                sb.Append(Serialize(element));
                sb.Append(", ");
            }

            sb.Remove(sb.Length - 2, 2);
            sb.Append("]");

            return sb.ToString();
        }
    }
}