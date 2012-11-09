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
using VDS.RDF;

namespace VDS.Common.Trees
{
    /// <summary>
    /// Binary Tree node implementation
    /// </summary>
    /// <typeparam name="TKey">Key Type</typeparam>
    /// <typeparam name="TValue">Value Type</typeparam>
    public class BinaryTreeNode<TKey, TValue>
        : IBinaryTreeNode<TKey, TValue>
    {
        private IBinaryTreeNode<TKey, TValue> _left, _right;
        private long _height = 1;

        /// <summary>
        /// Creates a new Binary Tree Node
        /// </summary>
        /// <param name="parent">Parent</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        public BinaryTreeNode(IBinaryTreeNode<TKey, TValue> parent, TKey key, TValue value)
        {
            this.Parent = parent;
            this.Key = key;
            this.Value = value;
        }

        /// <summary>
        /// Gets/Sets the Parent Node (if any)
        /// </summary>
        public IBinaryTreeNode<TKey, TValue> Parent
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the Left Child (if any)
        /// </summary>
        public IBinaryTreeNode<TKey, TValue> LeftChild
        {
            get
            {
                return this._left;
            }
            set
            {
                this._left = value;
                if (this._left != null) this._left.Parent = this;
                this.RecalculateHeight();
            }
        }

        /// <summary>
        /// Gets/Sets the Right Child (if any)
        /// </summary>
        public IBinaryTreeNode<TKey, TValue> RightChild
        {
            get
            {
                return this._right;
            }
            set
            {
                this._right = value;
                if (this._right != null) this._right.Parent = this;
                this.RecalculateHeight();
            }
        }

        /// <summary>
        /// Gets/Sets the Key
        /// </summary>
        public TKey Key
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the Value
        /// </summary>
        public TValue Value
        {
            get;
            set;
        }

        /// <summary>
        /// Gets whether this Node has children
        /// </summary>
        public bool HasChildren
        {
            get
            {
                return this.LeftChild != null || this.RightChild != null;
            }
        }

        /// <summary>
        /// Gets the number of child nodes present (0, 1 or 2)
        /// </summary>
        public int ChildCount
        {
            get
            {
                return (this.LeftChild != null ? 1 : 0) + (this.RightChild != null ? 1 : 0);
            }
        }

        /// <summary>
        /// Gets the height of the subtree
        /// </summary>
        public long Height
        {
            get
            {
                return this._height;
            }
            private set
            {
                this._height = value;
            }
        }

        /// <summary>
        /// Recalculates the height of the subtree
        /// </summary>
        public void RecalculateHeight()
        {
            long newHeight = Math.Max((this._left != null ? this._left.Height : 0), (this._right != null ? this._right.Height : 0)) + 1;
            if (newHeight != this._height)
            {
                this._height = newHeight;
                if (this.Parent != null) this.Parent.RecalculateHeight();
            }
        }

        /// <summary>
        /// Gets the nodes of the subtree including this node
        /// </summary>
        public IEnumerable<IBinaryTreeNode<TKey, TValue>> Nodes
        {
            get
            {
                return (this.LeftChild != null ? this.LeftChild.Nodes : Enumerable.Empty<IBinaryTreeNode<TKey, TValue>>()).Concat(((IBinaryTreeNode<TKey, TValue>)this).AsEnumerable()).Concat(this.RightChild != null ? this.RightChild.Nodes : Enumerable.Empty<IBinaryTreeNode<TKey, TValue>>());
            }
        }

        /// <summary>
        /// Gets a String representation of the node
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Key: " + this.Key.ToString() + " Value: " + this.Value.ToSafeString();
        }
    }
}
