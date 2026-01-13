using System;
using System.Collections.Generic;
using System.Linq;

namespace Cookie.BetterLogging.TreeGeneration
{
    /// <summary>
    /// Represents a single node in the generated tree
    /// </summary>
    public struct Node
    {
        /// <summary>
        /// The label shown for the node
        /// </summary>
        public string Label;

        /// <summary>
        /// The prefix of the node. This could be something like an array index, dictionary key or a field name
        /// </summary>
        public string Prefix;

        /// <summary>
        /// The type of the node
        /// </summary>
        /// <seealso cref="TreeGeneration.NodeType"/>
        public Type NodeType;

        /// <summary>
        /// The type of the object
        /// </summary>
        public System.Type ObjectType;

        /// <summary>
        /// The children of the node, <c>null</c> if it's a leaf node
        /// </summary>
        public List<Node> Children;

        /// <summary>
        /// Is this node a leaf node?
        /// </summary>
        /// <returns>True if <c>Children</c> is null or empty</returns>
        public readonly bool IsLeaf() => Children == null || Children.Count <= 0;

        /// <summary>
        /// Whether the node matches <c>searchQuery</c>
        /// </summary>
        /// <param name="searchQuery">The query to search for</param>
        /// <returns>True if <c>searchQuery</c> appears anywhere in the prefix or name of this node or its' children</returns>
        public readonly bool MatchesSearchQuery(string searchQuery)
        {
            if (Label.Contains(searchQuery, StringComparison.InvariantCultureIgnoreCase))
                return true;

            if (
                Prefix != null
                && Prefix.Contains(searchQuery, StringComparison.InvariantCultureIgnoreCase)
            )
                return true;

            if (IsLeaf())
                return false;

            return Children.Any(c => c.MatchesSearchQuery(searchQuery));
        }

        public Node(string label, Type nodeType, System.Type objectType, params Node[] children)
        {
            Prefix = null;
            Label = label;
            NodeType = nodeType;
            ObjectType = objectType;
            Children = new(children);
        }

        public Node(
            string label,
            string prefix,
            Type nodeType,
            System.Type objectType,
            params Node[] children
        )
            : this(label, nodeType, objectType, children)
        {
            Prefix = prefix;
        }

        public Node(string label, Type nodeType, System.Type objectType, List<Node> children)
        {
            Prefix = null;
            Label = label;
            NodeType = nodeType;
            ObjectType = objectType;
            Children = children;
        }

        public Node(
            string label,
            string prefix,
            Type nodeType,
            System.Type objectType,
            List<Node> children
        )
            : this(label, nodeType, objectType, children)
        {
            Prefix = prefix;
        }

        /// <summary>
        /// Represents the types that a <see cref="Node"/> can be
        /// </summary>
        public enum Type
        {
            /// <summary>
            /// Used for nodes whose depth has run out (usually caused by a circular reference)
            /// </summary>
            Shallow,

            /// <summary>
            /// A simple type, like a primitive, string, vector, colour etc
            /// </summary>
            Simple,

            /// <summary>
            /// An enum member
            /// </summary>
            Enum,

            /// <summary>
            /// A collection (IEnumerable), like a List or an array
            /// </summary>
            Collection,

            /// <summary>
            /// A dictionary
            /// </summary>
            Dictionary,

            /// <summary>
            /// A struct or a class (NOT YET IMPLEMENTED)
            /// </summary>
            Object,

            /// <summary>
            /// Null
            /// </summary>
            Null,
        }
    }
}
