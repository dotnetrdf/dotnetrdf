/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

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

        public static long GetHeight<TKey, TValue>(this IBinaryTreeNode<TKey, TValue> node)
        {
            if (node == null) return 0;
            return node.Height;
            //return 1 + Math.Max(node.LeftChild.GetHeight(), node.RightChild.GetHeight());
        }

        public static long GetBalance<TKey, TValue>(this IBinaryTreeNode<TKey, TValue> node)
        {
            if (node == null) return 0;
            long left = node.LeftChild.GetHeight();
            long right = node.RightChild.GetHeight();
            return right - left;
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
