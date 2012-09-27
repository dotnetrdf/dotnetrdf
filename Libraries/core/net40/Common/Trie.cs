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

namespace VDS.Common
{
    /// <summary>
    /// Trie data structure which maps strings to generic values.
    /// </summary>
    /// <typeparam name="TKey">Type of keys</typeparam>
    /// <typeparam name="TKeyBit">Type of key bits</typeparam>
    /// <typeparam name="TValue">Type of values to map to</typeparam>
    /// <remarks>
    /// <para>
    /// Keys are converted into a sequence of key bits using a user provided function
    /// </para>
    /// <para>
    /// Original code taken from <a href="http://code.google.com/p/typocalypse/source/browse/#hg/Trie">Typocolypse</a> but has been heavily rewritten to be much more generic and LINQ friendly
    /// </para>
    /// </remarks>
    public class Trie<TKey, TKeyBit, TValue>
        where TValue : class
    {
        private Func<TKey, IEnumerable<TKeyBit>> _keyMapper;
        private TrieNode<TKeyBit, TValue> _root;
        
        /// <summary>
        /// Create an empty trie with an empty root node.
        /// </summary>
        public Trie(Func<TKey, IEnumerable<TKeyBit>> keyMapper)
        {
            if (keyMapper == null) throw new ArgumentNullException("keyMapper", "Key Mapper function cannot be null");
            this._keyMapper = keyMapper;
            this._root = new TrieNode<TKeyBit, TValue>(null, default(TKeyBit));
        }

        /// <summary>
        /// Gets the Root Node of the Trie
        /// </summary>
        public TrieNode<TKeyBit, TValue> Root
        {
            get
            {
                return this._root;
            }
        }

        /// <summary>
        /// Adds a new key value pair, overwriting the existing value if the given key is already in use
        /// </summary>
        /// <param name="key">Key to search for value by</param>
        /// <param name="value">Value associated with key</param>
        public void Add(TKey key, TValue value)
        {
            TrieNode<TKeyBit, TValue> node = _root;
            IEnumerable<TKeyBit> bs = this._keyMapper(key);
            foreach (TKeyBit b in bs)
            {
                node = node.AddChild(b);
            }
            node.Value = value;
        }

        /// <summary>
        /// Remove the value that a key leads to and any redundant nodes which result from this action
        /// </summary>
        /// <param name="key">Key of the value to remove</param>
        public void Remove(TKey key)
        {
            TrieNode<TKeyBit, TValue> node = this._root;
            IEnumerable<TKeyBit> bs = this._keyMapper(key);
            foreach (TKeyBit b in bs)
            {
                //Bail out early if the key doesn't go anywhere
                if (!node.TryGetChild(b, out node)) return;
            }
            node.Value = null;

            //Remove all ancestor nodes which don't lead to a value.
            while (!node.IsRoot && !node.HasValue && node.Count == 0)
            {
                TKeyBit prevKey = node.KeyBit;
                node = node.Parent;
                node.RemoveChild(prevKey);
            }
        }

        /// <summary>
        /// Finds and returns a Node for the given Key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Null if the Key does not map to a Node</returns>
        public TrieNode<TKeyBit, TValue> Find(TKey key)
        {
            return this.Find(key, this._keyMapper);
        }

        /// <summary>
        /// Finds and returns a Node for the given Key using the given Key to Key Bit mapping function
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="keyMapper">Function to map keys to key bits</param>
        /// <returns>Null if the Key does not map to a Node</returns>
        /// <remarks>
        /// The ability to provide a custom mapping function allows you to do custom lookups into the Trie.  For example you might want to only match some portion of the key rather than the entire key
        /// </remarks>
        public TrieNode<TKeyBit, TValue> Find(TKey key, Func<TKey, IEnumerable<TKeyBit>> keyMapper)
        {
            return this.Find(keyMapper(key));
        }

        /// <summary>
        /// Finds and returns a Node for the given sequence of Key Bits
        /// </summary>
        /// <param name="bs">Key Bits</param>
        /// <returns>Null if the Key does not map to a Node</returns>
        /// <remarks>
        /// The ability to provide a specific seqeunce of key bits may be useful for custom lookups where you don't necessarily have a value of the <strong>TKey</strong> type but do have values of the <strong>TKeyBit</strong> type
        /// </remarks>
        public TrieNode<TKeyBit, TValue> Find(IEnumerable<TKeyBit> bs)
        {
            TrieNode<TKeyBit, TValue> node = this._root;
            foreach (TKeyBit b in bs)
            {
                //Bail out early if key does not exist
                if (!node.TryGetChild(b, out node)) return null;
            }
            return node;
        }

        /// <summary>
        /// Moves to the Node associated with the given Key creating new nodes if necessary
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Trie Node</returns>
        public TrieNode<TKeyBit, TValue> MoveToNode(TKey key)
        {
            TrieNode<TKeyBit, TValue> node = _root;
            IEnumerable<TKeyBit> bs = this._keyMapper(key);
            foreach (TKeyBit b in bs)
            {
                node = node.AddChild(b);
            }
            return node;
        }

        /// <summary>
        /// Gets/Sets the Value associated with a given Key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Value associated with the given Key, may be null if no value is associated</returns>
        /// <exception cref="KeyNotFoundException">Thrown if you try to get a value for a key that is not in the Trie, consider using <see cref="TryGetValue()">TryGetValue()</see> instead to avoid this</exception>
        public TValue this[TKey key]
        {
            get
            {
                TrieNode<TKeyBit, TValue> node = this.Find(key);
                if (node == null) throw new KeyNotFoundException();
                return node.Value;
            }
            set
            {
                this.Add(key, value);
            }
        }

        /// <summary>
        /// Tries to get the Value associated with a given Key
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>True if the Key exists in the Trie, False if it does not</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            TrieNode<TKeyBit, TValue> node = this._root;
            IEnumerable<TKeyBit> bs = this._keyMapper(key);
            foreach (TKeyBit b in bs)
            {
                //Bail out early if key does not exist
                if (!node.TryGetChild(b, out node))
                {
                    value = null;
                    return false;
                }
            }
            value = node.Value;
            return true;
        }

        /// <summary>
        /// Gets the Count of all Nodes in the Trie
        /// </summary>
        public int Count
        {
            get
            {
                return this._root.CountAll;
            }
        }

        /// <summary>
        /// Gets all the Values in the Trie
        /// </summary>
        public IEnumerable<TValue> Values
        {
            get
            {
                return this._root.Values;
            }
        }

        /// <summary>
        /// Clears the Trie
        /// </summary>
        public void Clear()
        {
            this._root.Clear();
        }
    }
}