using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cookie.BetterLogging.TreeGeneration
{
    /// <summary>
    /// This class has utility methods for generating tree representations of objects and collections
    /// </summary>
    /// <seealso cref="Node"/>
    /// <seealso cref="GenerateTree(object, int)"/>
    public static partial class TreeGenerator
    {
        public const int MaxDepth = 8;

        /// <summary>
        /// Generates a tree for <c>target</c> <br/>
        /// </summary>
        /// <param name="target">The object to generate the tree for</param>
        /// <param name="prefix">The prefix of the root node</param>
        /// <param name="depth">The depth of the tree</param>
        /// <returns>The root node of the tree</returns>
        /// <remarks>
        /// When <paramref name="depth"/> is 0 or under, a <c>NodeType.Shallow</c> node is returned <br/>
        /// with <c>object.ToString()</c> as the <c>Name</c>
        /// </remarks>
        public static Node GenerateTree(object target, string prefix, int depth)
        {
            if (target == null)
                return new Node("null", prefix, Node.Type.Null, null);

            Type type = target.GetType();

            if (depth <= 0)
                return new Node(target.ToString(), prefix, Node.Type.Shallow, type);

            if (target is float f)
                return new Node(f.ToString("0.00"), prefix, Node.Type.Simple, type);

            if (target is double d)
                return new Node(d.ToString("0.00"), prefix, Node.Type.Simple, type);

            if (IsSimple(target, type))
                return new Node(target.ToString(), prefix, Node.Type.Simple, type);

            if (type.IsEnum)
                return new Node(target.ToString(), prefix, Node.Type.Enum, type);

            // now if it failed all of the previous checks, it's either a dictionary, collection or an arbitrary object
            (List<Node> children, Node.Type nodeType) = target switch
            {
                IDictionary dictionary => (
                    GenerateDictionary(dictionary, depth),
                    Node.Type.Dictionary
                ),
                IEnumerable enumerable => (
                    GenerateEnumerable(enumerable, depth),
                    Node.Type.Collection
                ),
                _ => (GenerateObject(target, depth), Node.Type.Object),
            };

            return new Node(type.Name, prefix, nodeType, type, children);
        }

        /// <summary>
        /// Is <c>target</c> a simple object? (primitive, string, etc)
        /// </summary>
        /// <param name="target">The object to check</param>
        /// <param name="type">The type of the object</param>
        /// <returns>True if it's a simple type</returns>
        /// <seealso cref="NodeType.Simple"/>
        public static bool IsSimple(object target, Type type)
        {
            if (type.IsPrimitive)
                return true;

            return target switch
            {
                // string isn't a primitive per se, but it is for our purposes
                // it also implements IEnumerbale so if we passed it further along, it'd be a collection of its' characters
                string => true,
                // colours are also simple enough for our purposes
                Color => true,
                // very long case! lots of vector types
                Vector2
                or Vector3
                or Vector4
                or Vector2Int
                or Vector3Int
                or System.Numerics.Vector2
                or System.Numerics.Vector3
                or System.Numerics.Vector4 => true,
                _ => false,
            };
        }

        /// <inheritdoc cref="IsSimple(object, Type)"/>
        public static bool IsSimple(object target) => IsSimple(target, target.GetType());

        /// <inheritdoc cref="GenerateTree(object, string, int)" />
        public static Node GenerateTree(object target, string prefix) =>
            GenerateTree(target, prefix, MaxDepth);

        /// <inheritdoc cref="GenerateTree(object, string, int)" />
        public static Node GenerateTree(object target) => GenerateTree(target, null, MaxDepth);
    }
}
