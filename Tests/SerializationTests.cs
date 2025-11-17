using System.Collections.Generic;
using Cookie.BetterLogging.Serialization;
using NUnit.Framework;

namespace Cookie.BetterLogging.Tests
{
    public class SerializationTests
    {
        [Test]
        public void SerializeEnumerable() {
            int[] array = { 1, 5, 2, 3 };
            string serializedArray = Serializer.SerializeEnumerable(array);

            Assert.AreEqual("[1, 5, 2, 3]", serializedArray);
        }

        [Test]
        public void SerializeDictionary() {
            Dictionary<string, int> dict = new() {
                { "John", 1 },
                { "Alice", 3 },
                { "Bob", 5 },
            };

            string serializedDictionary = Serializer.SerializeDictionary(dict);
            Assert.AreEqual("{ [John: 1], [Alice: 3], [Bob: 5] }", serializedDictionary);
        }

        [Test]
        public void TypeRecognition() {
            int[] array = { 1, 4, 8, 10 };
            List<int> list = new() { 5, 12, 59 };
            Dictionary<string, int> dict = new() {
                { "John", 1 },
                { "Alice", 3 },
                { "Bob", 5 },
            };

            Assert.AreEqual(Serializer.SerializeEnumerable(array), Serializer.Serialize(array), "Array");
            Assert.AreEqual(Serializer.SerializeEnumerable(list), Serializer.Serialize(list), "List");
            Assert.AreEqual(Serializer.SerializeDictionary(dict), Serializer.Serialize(dict), "Dictionary");
        }

        [Test]
        public void DepthLimit() {
            int[][][] array = { new[] { new[] { 1, 2, 3 }, new[] { 4, 5, 6 } } };

            Assert.AreNotEqual(Serializer.SerializeEnumerable(array, 2), Serializer.SerializeEnumerable(array, 1));
        }
    }
}