using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.Common.Trees;

namespace VDS.RDF.Test.Common
{
    public static class BinaryTreeTools
    {
        public static void PrintBinaryTreeStructs<TNode, TKey>(ITree<TNode, TKey, TKey> tree)
            where TNode : class, IBinaryTreeNode<TKey, TKey>
            where TKey : struct
        {
            Console.Write("Root: ");
            Console.WriteLine(PrintBinaryTreeNodeStructs<TNode, TKey>(tree.Root));
            Console.WriteLine();
        }

        public static String PrintBinaryTreeNodeStructs<TNode, TKey>(TNode node)
            where TNode : class, IBinaryTreeNode<TKey, TKey>
            where TKey : struct
        {
            if (node == null)
            {
                return " null";
            }
            else
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("{");
                builder.AppendLine("  Key: " + node.Key);
                builder.AppendLine("  Value: " + node.Value);
                String lhs = PrintBinaryTreeNodeStructs<TNode, TKey>((TNode)node.LeftChild);
                if (lhs.Contains('\n'))
                {
                    builder.Append("  Left Child: ");
                    builder.AppendLine(AddIndent(lhs));
                }
                else
                {
                    builder.AppendLine("  Left Child: " + lhs);
                }
                String rhs = PrintBinaryTreeNodeStructs<TNode, TKey>((TNode)node.RightChild);
                if (rhs.Contains('\n'))
                {
                    builder.Append("  Right Child: ");
                    builder.AppendLine(AddIndent(rhs));
                }
                else
                {
                    builder.AppendLine("  Right Child: " + rhs);
                }
                builder.Append("}");
                return builder.ToString();
            }
        }

        private static String AddIndent(String input)
        {
            String[] lines = input.Split('\n');
            StringBuilder output = new StringBuilder();
            for (int i = 0; i < lines.Length; i++)
            {
                if (i > 0)
                {
                    output.Append("  " + lines[i]);
                    if (i < lines.Length - 1) output.AppendLine();
                }
                else
                {
                    output.AppendLine(lines[i]);
                }
            }

            return output.ToString();
        }
    }
}
