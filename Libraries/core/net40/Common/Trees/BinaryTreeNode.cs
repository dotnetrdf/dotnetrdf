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
