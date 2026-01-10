using System.Text;
using Cookie.BetterLogging.TreeGeneration;
using JetBrains.Annotations;

namespace Cookie.BetterLogging.Serialization
{
    [PublicAPI]
    public static class Serializer
    {
        const int IndentSize = 2;

        public static string Serialize(Node target)
        {
            int indentLevel = 0;

            return Serialize(target);

            string Serialize(Node target)
            {
                StringBuilder sb = new();
                AppendIndent(sb, indentLevel);
                if (target.Prefix != null)
                {
                    sb.Append(target.Prefix);
                    sb.Append(": ");
                }

                sb.Append(target.Name);

                if (target.IsLeaf())
                    return sb.ToString();

                sb.Append(": ");
                sb.Append(GetOpenChar(target.NodeType));
                sb.Append('\n');
                indentLevel++;

                for (int i = 0; i < target.Children.Count; i++)
                {
                    Node child = target.Children[i];
                    sb.Append(Serialize(child));
                    if (i < target.Children.Count - 1)
                        sb.Append(",\n");
                    else
                        sb.Append('\n');
                }

                indentLevel--;
                AppendIndent(sb, indentLevel);
                sb.Append(GetCloseChar(target.NodeType));

                return sb.ToString();
            }
        }

        private static void AppendIndent(StringBuilder sb, int indentLevel)
        {
            for (int i = 0; i < IndentSize * indentLevel; i++)
                sb.Append(' ');
        }

        private static char GetOpenChar(Node.Type nodeType)
        {
            return nodeType switch
            {
                Node.Type.Collection => '[',
                _ => '{',
            };
        }

        private static char GetCloseChar(Node.Type nodeType)
        {
            return nodeType switch
            {
                Node.Type.Collection => ']',
                _ => '}',
            };
        }
    }
}
