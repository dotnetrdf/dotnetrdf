using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.Common.Trees
{
    public static class TreeExtensions
    {
        public static long GetDepth<TKey, TValue>(this IBinaryTreeNode<TKey, TValue> node)
        {
            long depth = 0;
            while (node.Parent != null)
            {
                depth++;
                node = node.Parent;
            }
            return depth;
        }

        public static IBinaryTreeNode<TKey, TValue> GetSibling<TKey, TValue>(this IBinaryTreeNode<TKey, TValue> node)
        {
            if (node.Parent == null) return null;
            IBinaryTreeNode<TKey, TValue> parent = node.Parent;
            return (ReferenceEquals(node, parent.LeftChild) ? parent.RightChild : parent.LeftChild);
        }

        public static long GetSize<TKey, TValue>(this IBinaryTreeNode<TKey, TValue> node)
        {
            if (node == null) return 0;
            return node.Nodes.LongCount();
        }

        /// <summary>
        /// Isolates a Node from the tree by setting its parent and child links to be null
        /// </summary>
        /// <typeparam name="TKey">Key Type</typeparam>
        /// <typeparam name="TValue">Valye Type</typeparam>
        /// <param name="node">Node</param>
        /// <returns></returns>
        internal static IBinaryTreeNode<TKey, TValue> Isolate<TKey, TValue>(this IBinaryTreeNode<TKey, TValue> node)
        {
            node.Parent = null;
            node.LeftChild = null;
            node.RightChild = null;
            return node;
        }
    }
}
