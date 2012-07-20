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
    /// <summary>
    /// An AVL tree implementation
    /// </summary>
    /// <typeparam name="TKey">Key Type</typeparam>
    /// <typeparam name="TValue">Valye Type</typeparam>
    /// <remarks>
    /// <para>
    /// Code based in part on various examples from around the web including (but not limited to) <a href="http://www.vcskicks.com/AVL-tree.php">VCSKicks</a> and <a href="http://en.wikipedia.org/wiki/AVL_tree">Wikipedia</a>
    /// </para>
    /// </remarks>
    public sealed class AVLTree<TKey, TValue>
        : BinaryTree<IBinaryTreeNode<TKey, TValue>, TKey, TValue>
    {

        public AVLTree()
            : base() { }

        public AVLTree(IComparer<TKey> comparer)
            : base(comparer) { }

        protected override IBinaryTreeNode<TKey, TValue> CreateNode(IBinaryTreeNode<TKey, TValue> parent, TKey key, TValue value)
        {
            return new BinaryTreeNode<TKey, TValue>(parent, key, value);
        }

        protected sealed override void AfterLeftInsert(IBinaryTreeNode<TKey, TValue> parent, IBinaryTreeNode<TKey, TValue> node)
        {
            this.RebalanceAfterInsert(node);
        }

        protected sealed override void AfterRightInsert(IBinaryTreeNode<TKey, TValue> parent, IBinaryTreeNode<TKey, TValue> node)
        {
            this.RebalanceAfterInsert(node);   
        }

        private void RebalanceAfterInsert(IBinaryTreeNode<TKey, TValue> node)
        {
            IBinaryTreeNode<TKey, TValue> current = node.Parent;
            while (current != null)
            {
                long balance = current.GetBalance();
                switch (balance)
                {
                    case -2:
                    case 2:
                        this.Rebalance(current, balance);
                        break;
                    case -1:
                    case 0:
                    case 1:
                        //Balanced
                        break;
                    default:
                        throw new Exception("Illegal AVL Tree state");
                }
                current = current.Parent;
            }
        }

        protected sealed override void AfterDelete(IBinaryTreeNode<TKey, TValue> node)
        {
            IBinaryTreeNode<TKey, TValue> current = node.Parent;
            while (current != null)
            {
                long balance = current.GetBalance();
                if (Math.Abs(balance) == 1) break; //Short circuit where possible
                if (Math.Abs(balance) == 2)
                {
                    this.Rebalance(current, balance);
                }
                current = current.Parent;
            }
        }

        private void Rebalance(IBinaryTreeNode<TKey, TValue> node, long balance)
        {
            if (balance == 2)
            {
                //Right subtree is heavier
                long rightBalance = node.RightChild.GetBalance();
                if (rightBalance == 1 || rightBalance == 0)
                {
                    //Left Rotation
                    this.RotateLeft(node);
                }
                else if (rightBalance == -1)
                {
                    //Right Rotation of right child followed by left rotation
                    this.RotateRight(node.RightChild);
                    this.RotateLeft(node);
                }
            }
            else if (balance == -2)
            {
                //Left subtree is heavier
                long leftBalance = node.LeftChild.GetBalance();
                if (leftBalance == 1)
                {
                    //Left rotation of left child followed by right rotation
                    this.RotateLeft(node.LeftChild);
                    this.RotateRight(node);
                }
                else if (leftBalance == -1 || leftBalance == 0)
                {
                    //Right rotation
                    this.RotateRight(node);
                }
            }
        }

        private void RotateLeft(IBinaryTreeNode<TKey, TValue> node)
        {
            if (node == null) return;

            IBinaryTreeNode<TKey, TValue> pivot = node.RightChild;
            if (pivot == null) return;

            IBinaryTreeNode<TKey, TValue> parent = node.Parent;
            bool left = (parent != null && ReferenceEquals(node, parent.LeftChild));
            bool atRoot = (parent == null);

            //Rotate
            node.RightChild = pivot.LeftChild;
            pivot.LeftChild = node;

            //Update Parents
            node.Parent = pivot;
            pivot.Parent = parent;

            if (node.RightChild != null) node.RightChild.Parent = node;
            if (atRoot) this.Root = pivot;
            if (left)
            {
                parent.LeftChild = pivot;
            }
            else if (parent != null)
            {
                parent.RightChild = pivot;
            }
        }

        private void RotateRight(IBinaryTreeNode<TKey, TValue> node)
        {
            if (node == null) return;

            IBinaryTreeNode<TKey, TValue> pivot = node.LeftChild;
            if (pivot == null) return;

            IBinaryTreeNode<TKey, TValue> parent = node.Parent;
            bool left = (parent != null && ReferenceEquals(node, parent.LeftChild));
            bool atRoot = (parent == null);

            //Rotate
            node.LeftChild = pivot.RightChild;
            pivot.RightChild = node;

            //Update Parents
            node.Parent = pivot;
            pivot.Parent = parent;

            if (node.LeftChild != null) node.LeftChild.Parent = node;
            if (atRoot) this.Root = pivot;
            if (left)
            {
                parent.LeftChild = pivot;
            }
            else if (parent != null)
            {
                parent.RightChild = pivot;
            }
        }
    }
}
