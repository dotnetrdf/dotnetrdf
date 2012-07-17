using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.Common.Trees;

namespace VDS.Common
{
    public enum MultiDictionaryMode
    {
        Unbalanced,
        Scapegoat,
        AVL    }


    /// <summary>
    /// A multi dictionary implementation
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <remarks>
    /// <para>
    /// A multi-dictionary is essentially just a dictionary which deals properly with key collisions, with a normal .Net dictionary if two keys had colliding hash codes then they could interfer with each other.  With this implementation the keys are used to split the values into buckets and then each bucket uses a binary search tree to maintain full information about the keys and values.  This means that all keys and values are properly preserved and keys cannot interfer with each other in most cases.  In the case where keys have the same hash code and compare to be equal then they will interfere with each other but that is the correct behaviour.  The implementation is designed to be flexible in that it allows you to specify the hash function, the comparer used and the form of tree used for the buckets.
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


        public MultiDictionary()
            : this(null, null, DefaultMode) { }

        public MultiDictionary(Func<TKey, int> hashFunction)
            : this(hashFunction, null, DefaultMode) { }

        public MultiDictionary(IComparer<TKey> comparer)
            : this(null, comparer, DefaultMode) { }

        /// <summary>
        /// Creates a new multi-dictionary
        /// </summary>
        /// <param name="hashFunction">Hash Function</param>
        /// <param name="comparer">Comparer</param>
        /// <param name="mode">Tree Mode</param>
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
