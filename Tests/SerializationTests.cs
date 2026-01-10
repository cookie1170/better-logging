using System.Collections.Generic;
using Cookie.BetterLogging.TreeGeneration;
using NUnit.Framework;
using UnityEngine;
using static Cookie.BetterLogging.Serialization.Serializer;

namespace Cookie.BetterLogging.Tests
{
    public class SerializationTests
    {
        [Test]
        public void SimpleTypes()
        {
            Assert.AreEqual("hello world", GenSer("hello world"));
            Assert.AreEqual("1", GenSer(1));
            Assert.AreEqual("3.14", GenSer(Mathf.PI));
            Assert.AreEqual("(0.00, 0.00, 0.00)", GenSer(Vector3.zero));
        }

        [Test]
        public void Prefixes()
        {
            Assert.AreEqual("Prefix: hello world", GenSer("hello world", "Prefix"));
        }

        [Test]
        public void ComplexTypes()
        {
            List<int> testList = new() { 1, 2, 3 };
            Assert.AreEqual(
                @"List`1: [
  0: 1,
  1: 2,
  2: 3
]",
                GenSer(testList)
            );

            Dictionary<string, int> simpleDictionary = new()
            {
                { "Bob", 1 },
                { "Alice", 2 },
                { "John", 3 },
            };
            Assert.AreEqual(
                @"Dictionary`2: {
  Bob: 1,
  Alice: 2,
  John: 3
}",
                GenSer(simpleDictionary)
            );

            Dictionary<string[], List<int>> complexDictionary = new()
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

            Assert.AreEqual(
                @"Dictionary`2: {
  Entry: {
    Key: String[]: [
      0: Bob,
      1: Alice
    ],
    Value: List`1: [
      0: 1,
      1: 2,
      2: 3
    ]
  },
  Entry: {
    Key: String[]: [
      0: Guy,
      1: John
    ],
    Value: List`1: [
      0: 4,
      1: 5,
      2: 6
    ]
  }
}",
                GenSer(complexDictionary)
            );
        }

        private static string GenSer(object target) =>
            Serialize(TreeGenerator.GenerateTree(target));

        private static string GenSer(object target, string prefix) =>
            Serialize(TreeGenerator.GenerateTree(target, prefix));
    }
}
