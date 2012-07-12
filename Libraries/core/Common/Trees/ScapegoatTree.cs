using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.Common.Trees
{
    /// <summary>
    /// A scapegoat tree
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public sealed class ScapegoatTree<TKey, TValue>
        : BinaryTree<IBinaryTreeNode<TKey, TValue>, TKey, TValue>
    {
        private double _balanceFactor = 0.75d;
        private long _nodeCount = 0, _maxNodeCount = 0;
        private double _logBase = 1d / 0.75d;

        public ScapegoatTree()
            : base() { }

        public ScapegoatTree(IComparer<TKey> comparer)
            : base(comparer) { }

        public ScapegoatTree(double balanceFactor)
            : this(null, balanceFactor) { }

        public ScapegoatTree(IComparer<TKey> comparer, double balanceFactor)
            : base(comparer)
        {
            if (balanceFactor < 0.5d || balanceFactor > 1.0d) throw new ArgumentOutOfRangeException("balanceFactor", "Must meet the condition 0.5 < balanceFactor < 1");
            this._balanceFactor = balanceFactor;
            this._logBase = 1d / this._balanceFactor;
        }

        protected override IBinaryTreeNode<TKey, TValue> CreateNode(IBinaryTreeNode<TKey, TValue> parent, TKey key, TValue value)
        {
            return new BinaryTreeNode<TKey, TValue>(parent, key, value);
        }

        protected sealed override void AfterLeftInsert(IBinaryTreeNode<TKey, TValue> parent, IBinaryTreeNode<TKey, TValue> node)
        {
            this._nodeCount++;
            this._maxNodeCount = Math.Max(this._maxNodeCount, this._nodeCount);

            long depth = node.GetDepth();
            if (depth > Math.Log(this._nodeCount, this._logBase))
            {
                this.RebalanceAfterInsert(node);
            }
        }

        protected sealed override void AfterRightInsert(IBinaryTreeNode<TKey, TValue> parent, IBinaryTreeNode<TKey, TValue> node)
        {
            this._nodeCount++;
            this._maxNodeCount = Math.Max(this._maxNodeCount, this._nodeCount);

            long depth = node.GetDepth();
            if (depth > Math.Log(this._nodeCount, this._logBase))
            {
                this.RebalanceAfterInsert(node);
            }
        }

        private void RebalanceAfterInsert(IBinaryTreeNode<TKey, TValue> node)
        {
            this.Rebalance(node, 1);
        }

        private void RebalanceAfterDelete(IBinaryTreeNode<TKey, TValue> node)
        {
            this.Rebalance(node, node.GetSize<TKey, TValue>());
        }

        private void Rebalance(IBinaryTreeNode<TKey, TValue> node, long selfSize)
        {
            //Find the scapegoat
            long currSize = selfSize;
            long siblingSize, nodeSize;
            IBinaryTreeNode<TKey, TValue> current = node;
            do
            {
                //Get the sibling subtree size
                siblingSize = current.GetSibling<TKey, TValue>().GetSize<TKey, TValue>();
                if (current.Parent != null)
                {
                    //Total size of the Node is Current size of this subtree plus size of
                    //sibling subtree plus one for the current node
                    nodeSize = currSize + siblingSize + 1;
                    current = current.Parent;

                    //Is the current node weight balanced?
                    if (currSize <= (this._balanceFactor * nodeSize) && siblingSize <= (this._balanceFactor * siblingSize))
                    {
                        //Weight balanced so continue on
                        currSize = nodeSize;
                    }
                    else
                    {
                        //Not weight balanced so this is the scapegoat we rebalance from
                        break;
                    }
                }
                else
                {
                    //Rebalance at the root is gonna be O(n) for sure
                    break;
                }
            } while (current != null);

            //Now do a rebalance of the scapegoat which will be whatever current is set to
            IBinaryTreeNode<TKey, TValue>[] nodes = current.Nodes.ToArray();
            int median = nodes.Length / 2;
            IBinaryTreeNode<TKey, TValue> root = this.CreateNode(null, nodes[median].Key, nodes[median].Value);
            root.LeftChild = this.RebuildSubtree(nodes, 0, median);
            root.RightChild = this.RebuildSubtree(nodes, median + 1, nodes.Length - 1);

            //Use the rebalanced tree in place of the current node
            if (current.Parent == null)
            {
                //Replace entire tree
                this.Root = root;
            }
            else
            {
                //Replace subtree
                if (ReferenceEquals(current, current.Parent.LeftChild))
                {
                    current.Parent.LeftChild = root;
                }
                else
                {
                    current.Parent.RightChild = root;
                }
            }

            //Reset Max Node code after a rebalance
            this._maxNodeCount = this._nodeCount;
        }

        private IBinaryTreeNode<TKey, TValue> RebuildSubtree(IBinaryTreeNode<TKey, TValue>[] nodes, int start, int end)
        {
            //Empty Node
            if (end == start) return this.CreateNode(null, nodes[start].Key, nodes[start].Value);
            //if (end - start == 1) return this.CreateNode(null, nodes[

            //Rebuild the tree
            int median = start + ((end - start) / 2);
            IBinaryTreeNode<TKey, TValue> root = this.CreateNode(null, nodes[median].Key, nodes[median].Value);
            root.LeftChild = this.RebuildSubtree(nodes, start, median - 1);
            root.RightChild = this.RebuildSubtree(nodes, median + 1, end - 1);

            return root;
        }

        protected sealed override void AfterDelete(IBinaryTreeNode<TKey, TValue> node)
        {
            this._nodeCount--;

            if (this._nodeCount <= (this._maxNodeCount / 2))
            {
                this.RebalanceAfterDelete(node);
            }
        }
    }
}
