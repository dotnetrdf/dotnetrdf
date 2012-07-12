using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.Common.Trees
{
    /// <summary>
    /// An unbalanced binary tree
    /// </summary>
    /// <typeparam name="TKey">Key Type</typeparam>
    /// <typeparam name="TValue">Value Type</typeparam>
    public sealed class UnbalancedBinaryTree<TKey, TValue>
        : BinaryTree<IBinaryTreeNode<TKey, TValue>, TKey, TValue>
    {
        public UnbalancedBinaryTree()
            : base() { }

        public UnbalancedBinaryTree(IComparer<TKey> comparer)
            : base(comparer) { }

        protected sealed override IBinaryTreeNode<TKey, TValue> CreateNode(IBinaryTreeNode<TKey, TValue> parent, TKey key, TValue value)
        {
            return new BinaryTreeNode<TKey, TValue>(parent, key, value);
        }
    }
}
