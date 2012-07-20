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
using VDS.Common.Trees;

namespace VDS.Common
{
    /// <summary>
    /// Possible modes to use for the binary search tree based buckets of the <see cref="MultiDictionary"/>
    /// </summary>
    public enum MultiDictionaryMode
    {
        /// <summary>
        /// Use unbalanced trees, best when you expect minimal key collisions and are willing to trade faster insert performance for slower lookup performance
        /// </summary>
        Unbalanced,
        /// <summary>
        /// Use Scapegoat trees, good when there are a few key collisions and key comparisons are inexpensive.  Provides amortized O(log n) performance but ocassional operations may be O(n)
        /// </summary>
        Scapegoat,
        AVL   
    }


    /// <summary>
    /// A multi dictionary implementation
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <remarks>
    /// <para>
    /// A multi-dictionary is essentially just a dictionary which deals properly with key collisions, with a normal .Net dictionary if two keys had colliding hash codes then their values would overwrite each other.
    /// </para>
    /// <para>
    /// With this implementation the keys are used to split the values into buckets and then each bucket uses a binary search tree to maintain full information about the keys and values.  This means that all keys and values are properly preserved and keys cannot interfer with each other in most cases.  In the case where keys have the same hash code and compare to be equal then they will interfere with each other but that is the correct behaviour.  The implementation is designed to be flexible in that it allows you to specify the hash function, the comparer used and the form of tree used for the buckets.
    /// </para>
    /// </remarks>
    public class MultiDictionary<TKey, TValue>
        : IDictionary<TKey, TValue>, IEnumerable<TValue>
    {
        public const MultiDictionaryMode DefaultMode = MultiDictionaryMode.AVL;

        private Dictionary<int, ITree<IBinaryTreeNode<TKey, TValue>, TKey, TValue>> _dict;
        private IComparer<TKey> _comparer = Comparer<TKey>.Default;
        private Func<TKey, int> _hashFunc = (k => k.GetHashCode());
        private MultiDictionaryMode _mode = DefaultMode;

        /// <summary>
        /// Creates a new multi-dictionary
        /// </summary>
        public MultiDictionary()
            : this(null, null, DefaultMode) { }

        /// <summary>
        /// Creates a new multi-dictionary
        /// </summary>
        /// <param name="mode">Mode to use for the buckets</param>
        public MultiDictionary(MultiDictionaryMode mode)
            : this(null, null, mode) { }

        /// <summary>
        /// Creates a new multi-dictionary
        /// </summary>
        /// <param name="hashFunction">Hash Function to split the keys into the buckets</param>
        public MultiDictionary(Func<TKey, int> hashFunction)
            : this(hashFunction, null, DefaultMode) { }


        /// <summary>
        /// Creates a new multi-dictionary
        /// </summary>
        /// <param name="comparer">Comparer used for keys within the binary search trees</param>
        public MultiDictionary(IComparer<TKey> comparer)
            : this(null, comparer, DefaultMode) { }

        /// <summary>
        /// Creates a new multi-dictionary
        /// </summary>
        /// <param name="hashFunction">Hash Function to split the keys into the buckets</param>
        /// <param name="mode">Mode to use for the buckets</param>
        public MultiDictionary(Func<TKey, int> hashFunction, MultiDictionaryMode mode)
            : this(hashFunction, null, mode) { }

        /// <summary>
        /// Creates a new multi-dictionary
        /// </summary>
        /// <param name="comparer">Comparer used for keys within the binary search trees</param>
        /// <param name="mode">Mode to use for the buckets</param>
        public MultiDictionary(IComparer<TKey> comparer, MultiDictionaryMode mode)
            : this(null, comparer, mode) { }

        /// <summary>
        /// Creates a new multi-dictionary
        /// </summary>
        /// <param name="hashFunction">Hash Function to splut the keys into the buckets</param>
        /// <param name="comparer">Comparer used for keys within the binary search trees</param>
        /// <param name="mode">Mode to use for the buckets</param>
        public MultiDictionary(Func<TKey, int> hashFunction, IComparer<TKey> comparer, MultiDictionaryMode mode)
        {
            this._comparer = (comparer != null ? comparer : this._comparer);
            this._hashFunc = (hashFunction != null ? hashFunction : this._hashFunc);
            this._dict = new Dictionary<int, ITree<IBinaryTreeNode<TKey, TValue>, TKey, TValue>>();
            this._mode = mode;
        }

        /// <summary>
        /// Creates a new Tree to be used as a key/value bucket
        /// </summary>
        /// <returns></returns>
        private ITree<IBinaryTreeNode<TKey, TValue>, TKey, TValue> CreateTree()
        {
            switch (this._mode)
            {
                case MultiDictionaryMode.AVL:
                    return new AVLTree<TKey, TValue>(this._comparer);
                case MultiDictionaryMode.Scapegoat:
                    return new ScapegoatTree<TKey, TValue>(this._comparer);
                case MultiDictionaryMode.Unbalanced:
                    return new UnbalancedBinaryTree<TKey, TValue>(this._comparer);
                default:
                    return new AVLTree<TKey, TValue>(this._comparer);
            }
        }

        #region IDictionary<TKey,TValue> Members

        public void Add(TKey key, TValue value)
        {
            ITree<IBinaryTreeNode<TKey, TValue>, TKey, TValue> tree;
            int hash = this._hashFunc(key);
            if (this._dict.TryGetValue(hash, out tree))
            {
                //Add into existing tree
                tree.Add(key, value);
            }
            else
            {
                //Add new tree
                tree = this.CreateTree();
                tree.Add(key, value);
                this._dict.Add(hash, tree);
            }
        }

        public bool ContainsKey(TKey key)
        {
            ITree<IBinaryTreeNode<TKey, TValue>, TKey, TValue> tree;
            int hash = this._hashFunc(key);
            if (this._dict.TryGetValue(hash, out tree))
            {
                return tree.ContainsKey(key);
            }
            else
            {
                return false;
            }
        }

        public ICollection<TKey> Keys
        {
            get 
            {
                return (from hashKey in this._dict.Keys
                        from k in this._dict[hashKey].Keys
                        select k).ToList();
            }
        }

        public bool Remove(TKey key)
        {
            ITree<IBinaryTreeNode<TKey, TValue>, TKey, TValue> tree;
            int hash = this._hashFunc(key);
            if (this._dict.TryGetValue(hash, out tree))
            {
                return tree.Remove(key);
            }
            else
            {
                return false;
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            ITree<IBinaryTreeNode<TKey, TValue>, TKey, TValue> tree;
            int hash = this._hashFunc(key);
            if (this._dict.TryGetValue(hash, out tree))
            {
                return tree.TryGetValue(key, out value);
            }
            else
            {
                value = default(TValue);
                return false;
            }
        }

        public bool TryGetKey(TKey key, out TKey actualKey)
        {
            ITree<IBinaryTreeNode<TKey, TValue>, TKey, TValue> tree;
            int hash = this._hashFunc(key);
            if (this._dict.TryGetValue(hash, out tree))
            {
                IBinaryTreeNode<TKey, TValue> node = tree.Find(key);
                if (node == null)
                {
                    actualKey = default(TKey);
                    return false;
                }
                else
                {
                    actualKey = node.Key;
                    return true;
                }
            }
            else
            {
                actualKey = default(TKey);
                return false;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                return (from hashKey in this._dict.Keys
                        from v in this._dict[hashKey].Values
                        select v).ToList();
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue value;
                if (this.TryGetValue(key, out value))
                {
                    return value;
                }
                else
                {
                    throw new KeyNotFoundException(key.ToString() + " not found");
                }
            }
            set
            {
                //Just call Add() since it overwrites an existing value associated with the given key which is the expected behaviour
                this.Add(key, value);
            }
        }

        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            this.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            this._dict.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            TValue value;
            if (this.TryGetValue(item.Key, out value))
            {
                if (value != null) return value.Equals(item.Value);
                if (item.Value == null) return true; //Both null so equal
                return false; //One is null so not equal
            }
            else
            {
                return false;
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            int i = arrayIndex;
            foreach (ITree<IBinaryTreeNode<TKey, TValue>, TKey, TValue> tree in this._dict.Values)
            {
                foreach (IBinaryTreeNode<TKey, TValue> node in tree.Nodes)
                {
                    array[i] = new KeyValuePair<TKey, TValue>(node.Key, node.Value);
                    i++;
                }
            }
        }

        public int Count
        {
            get
            {
                return this._dict.Values.Sum(t => t.Nodes.Count()); 
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return this.Remove(item.Key);
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return (from t in this._dict.Values
                    from n in t.Nodes
                    select new KeyValuePair<TKey, TValue>(n.Key, n.Value)).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region IEnumerable<TValue> Members

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            return (from hashKey in this._dict.Keys
                    from v in this._dict[hashKey].Values
                    select v).GetEnumerator();
        }

        #endregion
    }
}
