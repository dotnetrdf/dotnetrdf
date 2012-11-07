/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.Common.Trees
{
    /// <summary>
    /// Useful extensions for working with Trees
    /// </summary>
    public static class TreeExtensions
    {
        /// <summary>
        /// Gets the depth of the given node in the Tree
        /// </summary>
        /// <typeparam name="TKey">Key Type</typeparam>
        /// <typeparam name="TValue">Value Type</typeparam>
        /// <param name="node">Node</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the Height of the given node in the Tree
        /// </summary>
        /// <typeparam name="TKey">Key</typeparam>
        /// <typeparam name="TValue">Value</typeparam>
        /// <param name="node">Node</param>
        /// <returns></returns>
        public static long GetHeight<TKey, TValue>(this IBinaryTreeNode<TKey, TValue> node)
        {
            if (node == null) return 0;
            return node.Height;
            //return 1 + Math.Max(node.LeftChild.GetHeight(), node.RightChild.GetHeight());
        }

        /// <summary>
        /// Gets the balance of the given node in the Tree
        /// </summary>
        /// <typeparam name="TKey">Key Type</typeparam>
        /// <typeparam name="TValue">Value Type</typeparam>
        /// <param name="node">Node</param>
        /// <returns></returns>
        public static long GetBalance<TKey, TValue>(this IBinaryTreeNode<TKey, TValue> node)
        {
            if (node == null) return 0;
            long left = node.LeftChild.GetHeight();
            long right = node.RightChild.GetHeight();
            return right - left;
        }

        /// <summary>
        /// Gets the sibling of a binary tree node (if any)
        /// </summary>
        /// <typeparam name="TKey">Key Type</typeparam>
        /// <typeparam name="TValue">Value Type</typeparam>
        /// <param name="node">Node</param>
        /// <returns></returns>
        public static IBinaryTreeNode<TKey, TValue> GetSibling<TKey, TValue>(this IBinaryTreeNode<TKey, TValue> node)
        {
            if (node.Parent == null) return null;
            IBinaryTreeNode<TKey, TValue> parent = node.Parent;
            return (ReferenceEquals(node, parent.LeftChild) ? parent.RightChild : parent.LeftChild);
        }

        /// <summary>
        /// Gets the size of the subtree rooted at the given node
        /// </summary>
        /// <typeparam name="TKey">Key</typeparam>
        /// <typeparam name="TValue">Valye</typeparam>
        /// <param name="node">Node</param>
        /// <returns></returns>
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
