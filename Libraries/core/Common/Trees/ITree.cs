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

using System.Collections.Generic;

namespace VDS.Common.Trees
{
    /// <summary>
    /// Interface for Trees
    /// </summary>
    /// <typeparam name="TNode">Node Type</typeparam>
    /// <typeparam name="TKey">Key Type</typeparam>
    /// <typeparam name="TValue">Value Type</typeparam>
    public interface ITree<TNode, TKey, TValue>
        where TNode : class, ITreeNode<TKey, TValue>
    {
        /// <summary>
        /// Gets the root of the tree
        /// </summary>
        TNode Root
        {
            get;
            set;
        }

        /// <summary>
        /// Adds a Key Value pair to the tree or replaces the existing value associated with a key
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>True if a new node was created, false otherwise.  In the case false is returned the existing value will still be updated to the given value</returns>
        bool Add(TKey key, TValue value);

        /// <summary>
        /// Finds a Node based on the key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Node associated with the given Key or null if the key is not present in the tree</returns>
        TNode Find(TKey key);

        /// <summary>
        /// Moves to a Node based on the key inserting a new Node if necessary
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="created">Whether a new node was inserted</param>
        /// <returns>Node associated with the given Key which may be newly inserted</returns>
        TNode MoveToNode(TKey key, out bool created);

        /// <summary>
        /// Removes a Node based on the Key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>True if a Node was removed</returns>
        bool Remove(TKey key);

        /// <summary>
        /// Determines whether a given Key exists in the Tree
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>True if the key exists in the Tree</returns>
        bool ContainsKey(TKey key);

        /// <summary>
        /// Gets/Sets the value for a key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Returns the value associated with the key</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the key doesn't exist</exception>
        TValue this[TKey key]
        {
            get;
            set;
        }

        /// <summary>
        /// Tries to get a value based on a key
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value or default for the value type if the key is not present</param>
        /// <returns>True if there is a value associated with the key</returns>
        bool TryGetValue(TKey key, out TValue value);

        /// <summary>
        /// Gets the Nodes of the tree
        /// </summary>
        IEnumerable<TNode> Nodes
        {
            get;
        }

        /// <summary>
        /// Gets the keys in the tree
        /// </summary>
        IEnumerable<TKey> Keys
        {
            get;
        }

        /// <summary>
        /// Gets the values in the tree
        /// </summary>
        IEnumerable<TValue> Values
        {
            get;
        }
    }

    /// <summary>
    /// Interface for Tree Nodes
    /// </summary>
    /// <typeparam name="TKey">Key Type</typeparam>
    /// <typeparam name="TValue">Value Type</typeparam>
    public interface ITreeNode<TKey, TValue>
    {
        /// <summary>
        /// Gets/Sets the key associated with the node
        /// </summary>
        TKey Key
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the value associated with the node
        /// </summary>
        TValue Value
        {
            get;
            set;
        }

        /// <summary>
        /// Gets whether the node has children
        /// </summary>
        bool HasChildren
        {
            get;
        }

        /// <summary>
        /// Gets whether the number of child nodes
        /// </summary>
        int ChildCount
        {
            get;
        }
    }

    /// <summary>
    /// Interface for Binary Tree Nodes
    /// </summary>
    /// <typeparam name="TKey">Key Type</typeparam>
    /// <typeparam name="TValue">Value Type</typeparam>
    public interface IBinaryTreeNode<TKey, TValue>
        : ITreeNode<TKey, TValue>
    {
        /// <summary>
        /// Gets the left child of this node
        /// </summary>
        IBinaryTreeNode<TKey, TValue> LeftChild
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the right child of this node
        /// </summary>
        IBinaryTreeNode<TKey, TValue> RightChild
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the parent of the node
        /// </summary>
        IBinaryTreeNode<TKey, TValue> Parent
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the nodes present for the entire subtree (including this node)
        /// </summary>
        IEnumerable<IBinaryTreeNode<TKey, TValue>> Nodes
        {
            get;
        }

        /// <summary>
        /// Gets the Height of the subtree this node represents
        /// </summary>
        long Height
        {
            get;
        }

        /// <summary>
        /// Indicates that the node should recauclate the height of the subtree it represents
        /// </summary>
        void RecalculateHeight();
    }

}
