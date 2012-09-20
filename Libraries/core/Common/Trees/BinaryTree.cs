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
    /// Abstract base implementation of an unbalanced binary search tree
    /// </summary>
    /// <typeparam name="TNode">Tree Node Type</typeparam>
    /// <typeparam name="TKey">Key Type</typeparam>
    /// <typeparam name="TValue">Value Type</typeparam>
    public abstract class BinaryTree<TNode, TKey, TValue>
        : ITree<TNode, TKey, TValue>
        where TNode : class, IBinaryTreeNode<TKey, TValue>
    {
        /// <summary>
        /// Key Comparer
        /// </summary>
        protected IComparer<TKey> _comparer = Comparer<TKey>.Default;

        /// <summary>
        /// Creates a new unbalanced Binary Tree
        /// </summary>
        public BinaryTree()
            : this(null) { }

        /// <summary>
        /// Creates a new unbalanced Binary Tree
        /// </summary>
        /// <param name="comparer">Comparer for keys</param>
        public BinaryTree(IComparer<TKey> comparer)
        {
            this._comparer = (comparer != null ? comparer : this._comparer);
        }

        /// <summary>
        /// Gets/Sets the Root of the Tree
        /// </summary>
        public virtual TNode Root
        {
            get;
            set;
        }

        /// <summary>
        /// Adds a Key Value pair to the tree, replaces an existing value if the key already exists in the tree
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns></returns>
        public virtual bool Add(TKey key, TValue value)
        {
            if (this.Root == null)
            {
                //No root yet so just insert at the root
                this.Root = this.CreateNode(null, key, value);
                return true;
            }
            else
            {
                //Move to the node
                bool created = false;
                IBinaryTreeNode<TKey, TValue> node = this.MoveToNode(key, out created);
                node.Value = value;
                return created;
            }
        }

        /// <summary>
        /// Creates a new node for the tree
        /// </summary>
        /// <param name="parent">Parent node</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns></returns>
        protected abstract TNode CreateNode(IBinaryTreeNode<TKey, TValue> parent, TKey key, TValue value);

        /// <summary>
        /// Finds a Node based on the key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Node associated with the given Key or null if the key is not present in the tree</returns>
        public virtual TNode Find(TKey key)
        {
            if (this.Root == null) return null;

            //Iteratively binary search for the key
            TNode current = this.Root;
            int c;
            do
            {
                c = this._comparer.Compare(key, current.Key);
                if (c < 0)
                {
                    current = (TNode)current.LeftChild;
                }
                else if (c > 0)
                {
                    current = (TNode)current.RightChild;
                }
                else
                {
                    //If we find a match on the key then return it
                    return current;
                }
            } while (current != null);

            return null;
        }

        /// <summary>
        /// Moves to the node with the given key inserting a new node if necessary
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="created">Whether a new node was inserted</param>
        /// <returns></returns>
        public virtual TNode MoveToNode(TKey key, out bool created)
        {
            if (this.Root == null)
            {
                this.Root = this.CreateNode(null, key, default(TValue));
                created = true;
                return this.Root;
            }
            else
            {
                //Iteratively binary search for the key
                TNode current = this.Root;
                TNode parent = null;
                int c;
                do
                {
                    c = this._comparer.Compare(key, current.Key);
                    if (c < 0)
                    {
                        parent = current;
                        current = (TNode)current.LeftChild;
                    }
                    else if (c > 0)
                    {
                        parent = current;
                        current = (TNode)current.RightChild;
                    }
                    else
                    {
                        //If we find a match on the key then return it
                        created = false;
                        return current;
                    }
                } while (current != null);

                //Key doesn't exist so need to do an insert
                current = this.CreateNode(parent, key, default(TValue));
                created = true;
                if (c < 0)
                {
                    parent.LeftChild = current;
                    this.AfterLeftInsert(parent, current);
                }
                else
                {
                    parent.RightChild = current;
                    this.AfterRightInsert(parent, current);
                }

                //Return the newly inserted node
                return current;
            }
        }

        /// <summary>
        /// Virtual method that can be used by derived implementations to perform tree balances after an insert
        /// </summary>
        /// <param name="parent">Parent</param>
        /// <param name="node">Newly inserted node</param>
        protected virtual void AfterLeftInsert(IBinaryTreeNode<TKey, TValue> parent, IBinaryTreeNode<TKey, TValue> node)
        {  }

        /// <summary>
        /// Virtual method that can be used by derived implementations to perform tree balances after an insert
        /// </summary>
        /// <param name="parent">Parent</param>
        /// <param name="node">Newly inserted node</param>
        protected virtual void AfterRightInsert(IBinaryTreeNode<TKey, TValue> parent, IBinaryTreeNode<TKey, TValue> node)
        {  }

        /// <summary>
        /// Removes a Node based on the Key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>True if a Node was removed</returns>
        public virtual bool Remove(TKey key)
        {
            //Iteratively binary search for the key
            TNode current = this.Root;
            int c;
            do
            {
                c = this._comparer.Compare(key, current.Key);
                if (c < 0)
                {
                    current = (TNode)current.LeftChild;
                }
                else if (c > 0)
                {
                    current = (TNode)current.RightChild;
                }
                else
                {
                    //If we find a match on the key then stop searching
                    //Calculate the comparison with the parent key (if there is a parent) as we need this info
                    //for the deletion implementation
                    c = (current.Parent != null ? this._comparer.Compare(current.Key, current.Parent.Key) : c);
                    break;
                }
            } while (current != null);

            //Perform the delete if we found a node
            if (current != null)
            {
                if (current.ChildCount == 2)
                {
                    //Has two children
                    //Therefore we need to in order successor of the left child (which must exist) and move it's key and value
                    //to this node and then delete the successor
                    TNode successor = this.FindRightmostChild((TNode)current.LeftChild);
                    if (ReferenceEquals(successor, current.LeftChild))
                    {
                        //If the successor is just the left child i.e. the left child has no right children
                        //then we can simply move the left child up to this position
                        current.Key = successor.Key;
                        current.Value = successor.Value;
                        current.LeftChild = successor.LeftChild;
                        this.AfterDelete(current);
                        return true;
                    }
                    else
                    {
                        //We've found a successor which is a rightmost child of our left child
                        //Move it's value to here and either delete the successor if it was a leaf
                        //node or move up it's left child - note it can never have a right child because
                        //we traversed to the rightmost child
                        current.Key = successor.Key;
                        current.Value = successor.Value;
                        if (successor.HasChildren)
                        {
                            successor.Parent.RightChild = successor.LeftChild;
                        }
                        else
                        {
                            successor.Parent.RightChild = null;
                        }
                        this.AfterDelete(current);
                        return true;
                    }
                }
                else if (current.HasChildren)
                {
                    //Is an internal node with a single child
                    //Thus just set the appropriate child of the parent to the appropriate child of the node we are deleting
                    if (c < 0)
                    {
                        current.Parent.LeftChild = (current.LeftChild != null ? current.LeftChild : current.RightChild);
                        this.AfterDelete((TNode)current.Parent);
                        return true;
                    }
                    else if (c > 0)
                    {
                        current.Parent.RightChild = (current.RightChild != null ? current.RightChild : current.LeftChild);
                        this.AfterDelete((TNode)current.Parent);
                        return true;
                    }
                    else
                    {
                        TNode successor;
                        if (current.LeftChild != null)
                        {
                            //Has a left subtree so get the in order successor which is the rightmost child of the left
                            //subtree
                            successor = this.FindRightmostChild((TNode)current.LeftChild);
                            if (ReferenceEquals(current.LeftChild, successor))
                            {
                                //There were no right children on the left subtree
                                if (current.Parent == null)
                                {
                                    //At Root and no right child of left subtree so can move left child up to root
                                    this.Root = (TNode)current.LeftChild;
                                    if (this.Root != null) this.Root.Parent = null;
                                    this.AfterDelete(this.Root);
                                    return true;
                                }
                                else
                                {
                                    //Not at Root and no right child of left subtree so can move left child up
                                    current.Parent.LeftChild = current.LeftChild;
                                    this.AfterDelete((TNode)current.Parent.LeftChild);
                                    return true;
                                }
                            }
                            //Move value up to this node and delete the rightmost child
                            current.Key = successor.Key;
                            current.Value = successor.Value;

                            //Watch out for the case where the rightmost child had a left child
                            if (successor.Parent.RightChild.HasChildren)
                            {
                                successor.Parent.RightChild = successor.LeftChild;
                            }
                            else
                            {
                                successor.Parent.RightChild = null;
                            }
                            this.AfterDelete(current);
                            return true;
                        }
                        else
                        {
                            //Must have a right subtree so find the in order sucessor which is the
                            //leftmost child of the right subtree
                            successor = this.FindLeftmostChild((TNode)current.RightChild);
                            if (ReferenceEquals(current.RightChild, successor))
                            {
                                //There were no left children on the right subtree
                                if (current.Parent == null)
                                {
                                    //At Root and no left child of right subtree so can move right child up to root
                                    this.Root = (TNode)current.RightChild;
                                    if (this.Root != null) this.Root.Parent = null;
                                    this.AfterDelete(this.Root);
                                    return true;
                                }
                                else
                                {
                                    //Not at Root and no left child of right subtree so can move right child up
                                    current.Parent.RightChild = current.RightChild;
                                    this.AfterDelete((TNode)current.Parent.RightChild);
                                    return true;
                                }
                            }
                            //Move value up to this node and delete the leftmost child
                            current.Key = successor.Key;
                            current.Value = successor.Value;

                            //Watch out for the case where the lefttmost child had a right child
                            if (successor.Parent.LeftChild.HasChildren)
                            {
                                successor.Parent.LeftChild = successor.RightChild;
                            }
                            else
                            {
                                successor.Parent.LeftChild = null;
                            }
                            this.AfterDelete(current);
                            return true;
                        }
                    }
                }
                else
                {
                    //Must be an external node
                    //Thus just set the appropriate child of the parent to be null
                    if (c < 0)
                    {
                        current.Parent.LeftChild = null;
                        this.AfterDelete((TNode)current.Parent);
                        return true;
                    }
                    else if (c > 0)
                    {
                        current.Parent.RightChild = null;
                        this.AfterDelete((TNode)current.Parent);
                        return true;
                    }
                    else
                    {
                        //Root of tree is only way we can get here so just
                        //set root to null
                        this.Root = null;
                        return true;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Finds the leftmost child of the given node
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns></returns>
        protected TNode FindLeftmostChild(TNode node)
        {
            while (node.LeftChild != null)
            {
                node = (TNode)node.LeftChild;
            }
            return node;
        }

        /// <summary>
        /// Finds the rightmost child of the given node
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns></returns>
        protected TNode FindRightmostChild(TNode node)
        {
            while (node.RightChild != null)
            {
                node = (TNode)node.RightChild;
            }
            return node;
        }

        /// <summary>
        /// Virtual method that can be used by derived implementations to perform tree balances after a delete
        /// </summary>
        /// <param name="node">Node at which the deletion happened</param>
        protected virtual void AfterDelete(TNode node)
        { }

        /// <summary>
        /// Determines whether a given Key exists in the Tree
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>True if the key exists in the Tree</returns>
        public virtual bool ContainsKey(TKey key)
        {
            return this.Find(key) != null;
        }

        /// <summary>
        /// Gets/Sets the value for a key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Returns the value associated with the key</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the key doesn't exist</exception>
        public TValue this[TKey key]
        {
            get
            {
                TNode n = this.Find(key);
                if (n != null)
                {
                    return n.Value;
                }
                else
                {
                    throw new KeyNotFoundException();
                }
            }
            set
            {
                TNode n = this.Find(key);
                if (n != null)
                {
                    n.Value = value;
                }
                else
                {
                    throw new KeyNotFoundException();
                }
            }
        }

        /// <summary>
        /// Tries to get a value based on a key
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value or default for the value type if the key is not present</param>
        /// <returns>True if there is a value associated with the key</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            TNode n = this.Find(key);
            if (n != null)
            {
                value = n.Value;
                return true;
            }
            else
            {
                value = default(TValue);
                return false;
            }
        }

        /// <summary>
        /// Gets the Nodes of the Tree
        /// </summary>
        public IEnumerable<TNode> Nodes
        {
            get
            {
                if (this.Root == null)
                {
                    return Enumerable.Empty<TNode>();
                }
                else
                {
                    return (this.Root.LeftChild != null ? this.Root.LeftChild.Nodes.OfType<TNode>() : Enumerable.Empty<TNode>()).Concat(this.Root.AsEnumerable()).Concat(this.Root.RightChild != null ? this.Root.RightChild.Nodes.OfType<TNode>() : Enumerable.Empty<TNode>());
                }
            }
        }

        /// <summary>
        /// Gets the Keys of the Tree
        /// </summary>
        public IEnumerable<TKey> Keys
        {
            get 
            {
                return (from n in this.Nodes
                        select n.Key);
            }
        }

        /// <summary>
        /// Gets the Values of the Tree
        /// </summary>
        public IEnumerable<TValue> Values
        {
            get 
            {
                return (from n in this.Nodes
                        select n.Value);
            }
        }

    }
}
