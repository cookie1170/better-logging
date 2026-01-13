using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Cookie.BetterLogging.TreeGeneration
{
    public static partial class TreeGenerator
    {
        /// <summary>
        /// Generates children nodes for an <c>IEnumerable</c>
        /// </summary>
        /// <param name="enumerable">The enumerable to generate the tree for</param>
        /// <param name="depth">The depth at which to generate the tree</param>
        /// <returns>A list of nodes representing the enumerable's elements</returns>
        private static List<Node> GenerateEnumerable(IEnumerable enumerable, int depth)
        {
            List<Node> children;

            if (enumerable is IList list)
                children = new(list.Count);
            else
                children = new();

            int index = 0;
            foreach (object element in enumerable)
                children.Add(GenerateTree(element, index++.ToString(), depth - 1));

            return children;
        }

        /// <summary>
        /// Generates children nodes for an <c>IDictionary</c>
        /// </summary>
        /// <param name="dictionary">The dictionary to generate the tree for</param>
        /// <param name="depth">The depth at which to generate the tree</param>
        /// <returns>A list of nodes representing the dictionary's entries</returns>
        private static List<Node> GenerateDictionary(IDictionary dictionary, int depth)
        {
            List<Node> children = new(dictionary.Count);

            foreach (DictionaryEntry entry in dictionary)
                children.Add(GetNodeForEntry(entry, depth));

            return children;
        }

        /// <summary>
        /// Gets a node for the dictionary entry <c>entry</c>
        /// </summary>
        /// <param name="entry">The entry to get the node for</param>
        /// <param name="depth">The depth at which to get the dictionary's tree is generated</param>
        /// <returns>The node of the entry</returns>
        private static Node GetNodeForEntry(DictionaryEntry entry, int depth)
        {
            if (IsSimple(entry.Key))
                return GenerateTree(entry.Value, entry.Key.ToString(), depth - 1);

            Node keyNode = GenerateTree(entry.Key, "Key", depth - 1);
            Node valueNode = GenerateTree(entry.Value, "Value", depth - 1);

            return new Node("Entry", Node.Type.Object, typeof(DictionaryEntry), keyNode, valueNode);
        }

        /// <summary>
        /// Generates children nodes for an arbitrary object based on its' fields
        /// </summary>
        /// <param name="target">The object for which to generate the tree for</param>
        /// <param name="depth">The depth at which to generate the tree</param>
        /// <returns>A list of nodes representing the object's fields</returns>
        private static List<Node> GenerateObject(object target, int depth)
        {
            List<Node> children = new();

            Type type = target.GetType();
            FieldInfo[] fields = type.GetFields();
            PropertyInfo[] props = type.GetProperties();

            foreach (FieldInfo field in fields)
            {
                Node node = GenerateTree(field.GetValue(target), field.Name, depth - 1);
                children.Add(node);
            }

            foreach (PropertyInfo prop in props)
            {
                Node node = GenerateTree(prop.GetValue(target), prop.Name, depth - 1);
                children.Add(node);
            }

            return children;
        }
    }
}
