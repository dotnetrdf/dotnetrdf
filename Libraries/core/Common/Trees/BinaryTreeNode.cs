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

        public BinaryTreeNode(IBinaryTreeNode<TKey, TValue> parent, TKey key, TValue value)
        {
            this.Parent = parent;
            this.Key = key;
            this.Value = value;
        }

        public IBinaryTreeNode<TKey, TValue> Parent
        {
            get;
            set;
        }

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

        public IBinaryTreeNode<TKey, TValue> RightChild
        {
            get
            {
                return this._right;
            }
            set//
            {
                this._right = value;
                if (this._right != null) this._right.Parent = this;
                this.RecalculateHeight();
            }
        }

        public TKey Key
        {
            get;
            set;
        }

        public TValue Value
        {
            get;
            set;
        }

        public bool HasChildren
        {
            get
            {
                return this.LeftChild != null || this.RightChild != null;
            }
        }

        public int ChildCount
        {
            get
            {
                return (this.LeftChild != null ? 1 : 0) + (this.RightChild != null ? 1 : 0);
            }
        }

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

        public void RecalculateHeight()
        {
            long newHeight = Math.Max((this._left != null ? this._left.Height : 0), (this._right != null ? this._right.Height : 0)) + 1;
            if (newHeight != this._height)
            {
                this._height = newHeight;
                if (this.Parent != null) this.Parent.RecalculateHeight();
            }
        }

        public IEnumerable<IBinaryTreeNode<TKey, TValue>> Nodes
        {
            get
            {
                return (this.LeftChild != null ? this.LeftChild.Nodes : Enumerable.Empty<IBinaryTreeNode<TKey, TValue>>()).Concat(this.AsEnumerable()).Concat(this.RightChild != null ? this.RightChild.Nodes : Enumerable.Empty<IBinaryTreeNode<TKey, TValue>>());
            }
        }

        public override string ToString()
        {
            return "Key: " + this.Key.ToString() + " Value: " + this.Value.ToSafeString();
        }
    }
}
