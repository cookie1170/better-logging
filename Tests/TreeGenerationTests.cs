using System;
using System.Collections.Generic;
using System.Linq;
using Cookie.BetterLogging.TreeGeneration;
using NUnit.Framework;
using UnityEngine;
using static Cookie.BetterLogging.TreeGeneration.TreeGenerator;

namespace Cookie.BetterLogging.Tests
{
    public class TreeGenerationTests
    {
        [Test]
        public void SimpleTypes()
        {
            Assert.IsTrue(IsSimple("hello world"));
            Assert.IsTrue(IsSimple(1));
            Assert.IsTrue(IsSimple(1f));
            Assert.IsTrue(IsSimple(Color.white));
            Assert.IsTrue(IsSimple(Vector3.zero));
            Assert.IsFalse(IsSimple(new List<int>() { 1, 2, 3 }));
        }

        [Test]
        public void SimpleTypeTrees()
        {
            Node stringNode = GenerateTree("Hello world!");
            Assert.AreEqual("Hello world!", stringNode.Name);
            Assert.IsTrue(stringNode.IsLeaf());

            Node vector3Node = GenerateTree(Vector3.zero);
            Assert.AreEqual(Vector3.zero.ToString(), vector3Node.Name);
            Assert.IsTrue(vector3Node.IsLeaf());

            Node floatNode = GenerateTree(Mathf.PI);
            Assert.AreEqual("3.14", floatNode.Name);
            Assert.IsTrue(floatNode.IsLeaf());

            Node doubleNode = GenerateTree(Math.PI);
            Assert.AreEqual("3.14", doubleNode.Name);
            Assert.IsTrue(doubleNode.IsLeaf());
        }

        [Test]
        public void Enums()
        {
            TestEnum enumValue = TestEnum.AnotherMember;
            Node enumNode = GenerateTree(enumValue);
            Assert.IsTrue(enumNode.IsLeaf());
            Assert.AreEqual(Node.Type.Enum, enumNode.NodeType);
            Assert.AreEqual("AnotherMember", enumNode.Name);
        }

        [Test]
        public void ComplexTrees()
        {
            List<int> testList = new() { 1, 2, 3 };
            Node listNode = GenerateTree(testList);

            Assert.IsFalse(listNode.IsLeaf());
            Assert.AreEqual(Node.Type.Collection, listNode.NodeType);
            Assert.AreEqual(testList.Count, listNode.Children.Count);

            for (int i = 0; i < testList.Count; i++)
            {
                Node childNode = listNode.Children[i];
                Assert.IsTrue(childNode.IsLeaf());
                Assert.AreEqual(testList[i].ToString(), childNode.Name);
                Assert.AreEqual(i.ToString(), childNode.Prefix);
            }

            Dictionary<string, int> simpleDictionary = new()
            {
                { "Bob", 1 },
                { "Alice", 2 },
                { "John", 3 },
            };
            Node simpleDictionaryNode = GenerateTree(simpleDictionary);

            Assert.IsFalse(simpleDictionaryNode.IsLeaf());
            Assert.AreEqual(simpleDictionary.Count, simpleDictionaryNode.Children.Count);
            Assert.AreEqual(Node.Type.Dictionary, simpleDictionaryNode.NodeType);

            List<string> simpleKeys = simpleDictionary.Keys.ToList();
            List<int> simpleValues = simpleDictionary.Values.ToList();
            for (int i = 0; i < simpleDictionary.Count; i++)
            {
                Node childNode = simpleDictionaryNode.Children[i];
                Assert.AreEqual(simpleValues[i].ToString(), childNode.Name);
                Assert.AreEqual(simpleKeys[i], childNode.Prefix);
            }

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
            Node complexDictionaryNode = GenerateTree(complexDictionary);

            Assert.IsFalse(complexDictionaryNode.IsLeaf());
            Assert.AreEqual(complexDictionaryNode.Children.Count, complexDictionary.Count);
            Assert.AreEqual(Node.Type.Dictionary, complexDictionaryNode.NodeType);

            List<string[]> complexKeys = complexDictionary.Keys.ToList();
            List<List<int>> complexValues = complexDictionary.Values.ToList();
            for (int i = 0; i < complexDictionary.Count; i++)
            {
                Node childNode = complexDictionaryNode.Children[i];
                Assert.IsFalse(childNode.IsLeaf()); // in a complex dictionary like this, it shouldn't be a leaf node!
                Assert.AreEqual(2, childNode.Children.Count);
                Assert.AreEqual("Entry", childNode.Name);

                Node key = childNode.Children[0];
                Assert.AreEqual("Key", key.Prefix);
                for (int j = 0; j < complexKeys[i].Length; j++)
                    Assert.AreEqual(complexKeys[i][j].ToString(), key.Children[j].Name);

                Node value = childNode.Children[1];
                Assert.AreEqual("Value", value.Prefix);
                for (int j = 0; j < complexValues[i].Count; j++)
                    Assert.AreEqual(complexValues[i][j].ToString(), value.Children[j].Name);
            }
        }

        enum TestEnum
        {
            Member,
            AnotherMember,
        }
    }
}
