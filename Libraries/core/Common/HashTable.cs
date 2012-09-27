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

namespace VDS.Common
{
    /// <summary>
    /// Controls the bias of the hash table
    /// </summary>
    /// <remarks>
    /// <para>
    /// The bias is a parameter used to control how the hash table stores actual multiple values in its slots.  Changing the bias parameter will cause different data structures to be used internally and thus modify the performance characteristics of the Hash Table
    /// </para>
    /// </remarks>
    [Obsolete("The HashTable class has known deficiencies in hash code collision behaviour and has been superceded by MultiDictionary which should be used instead, HashTable will be removed in subsequent releases", true)]
    public enum HashTableBias
    {
        /// <summary>
        /// Bias towards Compactness i.e. memory usage should be minimised
        /// </summary>
        Compactness,
#if !SILVERLIGHT
        /// <summary>
        /// Bias towards IO i.e. will be more performant in scenarios with lots of adds, removes and contains
        /// </summary>
        IO,
#endif
        /// <summary>
        /// Bias towards Enumeration i.e. will be more performant for scenarios with lots of enumerating over the data (Default)
        /// </summary>
        Enumeration
    }

    /// <summary>
    /// A Hash Table for use as in-memory storage
    /// </summary>
    /// <typeparam name="TKey">Type of Keys</typeparam>
    /// <typeparam name="TValue">Type of Values</typeparam>
    /// <remarks>
    /// <para>
    /// Essentially a Dictionary which allows multiple values for each key, unlike the newer <see cref="MultiDictionary"/> this implementation is lossy since it does not store unique keys so if you have colliding keys all values associated with them are mushed together under a single key.  However the advantage of the HashTable is that it transparently maps a key to an arbitrary list of values where a MultiDictionary requires the user to manage that if that want to map multiple values to a single key
    /// </para>
    /// </remarks>
    [Obsolete("The HashTable class has known deficiencies in hash code collision behaviour and has been superceded by MultiDictionary which should be used instead, HashTable will be removed in subsequent releases", false)]
    public class HashTable<TKey, TValue> 
        : IDictionary<TKey, TValue>, IEnumerable<TValue>
    {
        private Dictionary<TKey, IHashSlot<TValue>> _values = new Dictionary<TKey, IHashSlot<TValue>>();
        private int _capacity = 1;
        private bool _emptyKeys = false;
        private HashTableBias _bias = HashTableBias.Enumeration;

        /// <summary>
        /// Creates a new Hash Table
        /// </summary>
        /// <remarks>
        /// Use this constructor if you expect to use this primarily as a dictionary i.e. there is a mostly 1:1 mapping of keys to values with minimal collisions
        /// </remarks>
        public HashTable()
        {

        }

        /// <summary>
        /// Creates a new Hash Table with the given bias
        /// </summary>
        /// <param name="bias">Bias</param>
        /// <remarks>
        /// The bias controls what the underlying storage of the table is, different biases have different performance characteristics and allow you to tailor the table to your intended usage
        /// </remarks>
        public HashTable(HashTableBias bias)
        {
            this._bias = bias;
        }

        /// <summary>
        /// Creates a new Hash Table
        /// </summary>
        /// <param name="emptyKeys">Whether Keys are allowed to have no values associated with them</param>
        public HashTable(bool emptyKeys)
        {
            this._emptyKeys = emptyKeys;
        }

        /// <summary>
        /// Creates a new Hash Table with the given bias
        /// </summary>
        /// <param name="bias">Bias</param>
        /// <param name="emptyKeys">Whether Keys are allowed to have no values associated with them</param>
        /// <remarks>
        /// The bias controls what the underlying storage of the table is, different biases have different performance characteristics and allow you to tailor the table to your intended usage
        /// </remarks>
        public HashTable(HashTableBias bias, bool emptyKeys)
            : this(emptyKeys)
        {
            this._bias = bias;
        }

        /// <summary>
        /// Creates a new Hash Table where the initial capacity at each key is specified
        /// </summary>
        /// <param name="capacity">Initial Capacity at each Key</param>
        /// <remarks>
        /// Use this if you expect to use this as a true HashTable i.e. there is a 1:Many mapping of keys to values.  Choose a capcity value that seems reasonable for the data you expect to store.
        /// </remarks>
        public HashTable(int capacity)
        {
            if (capacity >= 1) this._capacity = capacity;
        }

        /// <summary>
        /// Creates a new Hash Table with the given bias
        /// </summary>
        /// <param name="bias">Bias</param>
        /// <param name="capacity">Initial Capacity at each Key</param>
        /// <remarks>
        /// <para>
        /// Use this if you expect to use this as a true Hash Table i.e. there is a 1:Many mapping of keys to values.  Choose a capcity value that seems reasonable for the data you expect to store.
        /// </para>
        /// <para>
        /// The bias controls what the underlying storage of the table is, different biases have different performance characteristics and allow you to tailor the table to your intended usage
        /// </para>
        /// </remarks>
        public HashTable(HashTableBias bias, int capacity)
            : this(capacity)
        {
            this._bias = bias;
        }

        /// <summary>
        /// Creates a new Hash Table where the initial capacity at each key is specified
        /// </summary>
        /// <param name="capacity">Initial Capacity at each Key</param>
        /// <param name="emptyKeys">Whether keys are allowed to have no values associated with them</param>
        /// <remarks>
        /// Use this if you expect to use this as a true HashTable i.e. there is a 1:Many mapping of keys to values.  Choose a capcity value that seems reasonable for the data you expect to store.
        /// </remarks>
        public HashTable(int capacity, bool emptyKeys)
            : this(emptyKeys)
        {
            if (capacity >= 1) this._capacity = capacity;
        }

        /// <summary>
        /// Creates a new Hash Table with the given bias
        /// </summary>
        /// <param name="bias">Bias</param>
        /// <param name="capacity">Initial Capacity at each Key</param>
        /// <param name="emptyKeys">Whether keys are allowed to have no values associated with them</param>
        /// <remarks>
        /// <para>
        /// Use this if you expect to use this as a true Hash Table i.e. there is a 1:Many mapping of keys to values.  Choose a capcity value that seems reasonable for the data you expect to store.
        /// </para>
        /// <para>
        /// The bias controls what the underlying storage of the table is, different biases have different performance characteristics and allow you to tailor the table to your intended usage
        /// </para>
        /// </remarks>
        public HashTable(HashTableBias bias, int capacity, bool emptyKeys)
            : this(capacity, emptyKeys)
        {
            this._bias = bias;
        }

        private IHashSlot<TValue> CreateSlot(TValue value, int capacity)
        {
            switch (this._bias)
            {
                case HashTableBias.Compactness:
                    return new CompactSlot<TValue>(value);
#if !SILVERLIGHT
                case HashTableBias.IO:
                    return new SetSlot<TValue>(value);
#endif
                case HashTableBias.Enumeration:
                default:
                    return new ListSlot<TValue>(value, capacity);
            }
        }

        private IHashSlot<TValue> CreateEmptySlot(int capacity)
        {
            switch (this._bias)
            {
                case HashTableBias.Compactness:
                    return new CompactSlot<TValue>();
#if !SILVERLIGHT
                case HashTableBias.IO:
                    return new SetSlot<TValue>();
#endif
                case HashTableBias.Enumeration:
                default:
                    return new ListSlot<TValue>(capacity);
            }
        }

        /// <summary>
        /// Adds a Key with an empty value set to the Hash Table
        /// </summary>
        /// <param name="key">Key</param>
        public void AddEmpty(TKey key)
        {
            if (!this._values.ContainsKey(key))
            {
                if (!this._emptyKeys) throw new InvalidOperationException("HashTable must be instantiated with the emptyKeys parameter set to true in order to allow empty keys");
                this._values.Add(key, this.CreateEmptySlot(this._capacity));
            }
        }

        /// <summary>
        /// Adds an item to the Hash Table
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        public void Add(TKey key, TValue value)
        {
            IHashSlot<TValue> slot;
            if (this._values.TryGetValue(key, out slot))
            {
                slot.Add(value);
            }
            else
            {
                this._values.Add(key, this.CreateSlot(value, this._capacity));
            }
        }

        /// <summary>
        /// Checks whether the given Key exists in the Hash Table
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns></returns>
        public bool ContainsKey(TKey key)
        {
            return this._values.ContainsKey(key);
        }

        /// <summary>
        /// Checks whether the given Key Value pair exists in the Hash Table
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns></returns>
        public bool Contains(TKey key, TValue value)
        {
            IHashSlot<TValue> slot;
            if (this._values.TryGetValue(key, out slot))
            {
                return slot.Contains(value);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the Keys of the Hash Table
        /// </summary>
        public ICollection<TKey> Keys
        {
            get 
            {
                return this._values.Keys;
            }
        }

        /// <summary>
        /// Removes everything with the given Key from the Hash Table
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns></returns>
        public bool Remove(TKey key)
        {
            return this._values.Remove(key);
        }

        /// <summary>
        /// Removes the given Key Value pair from the Hash Table
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns></returns>
        public bool Remove(TKey key, TValue value)
        {
            IHashSlot<TValue> slot;
            if (this._values.TryGetValue(key, out slot))
            {
                bool res = slot.Remove(value);
                //Remove the Key if it has no values and emptyKeys is set to false
                if (slot.Count == 0 && !this._emptyKeys) this.Remove(key);
                return res;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Tries to get the first Value with the given Key
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns></returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            IHashSlot<TValue> slot;
            if (this._values.TryGetValue(key, out slot))
            {
                value = slot.First();
                return true;
            }
            else
            {
                value = default(TValue);
                return false;
            }
        }

        /// <summary>
        /// Tries to get the actual instance of the given Key and Value from the Collection
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <param name="result">Actual Value from the Collection</param>
        /// <returns></returns>
        public bool TryGetValue(TKey key, TValue value, out TValue result)
        {
            IHashSlot<TValue> slot;
            if (this._values.TryGetValue(key, out slot))
            {
                result = slot.FirstOrDefault(v => v.Equals(value));
                return true;
            }
            else
            {
                result = default(TValue);
                return false;
            }
        }

        /// <summary>
        /// Gets all the values in the Hash Table
        /// </summary>
        public ICollection<TValue> Values
        {
            get 
            {
                return (from slot in this._values.Values
                        from value in slot
                        select value).ToList();
            }
        }

        /// <summary>
        /// Gets the first value with the given key from the Hash Table
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns></returns>
        public TValue this[TKey key]
        {
            get
            {
                IHashSlot<TValue> slot;
                if (this._values.TryGetValue(key, out slot))
                {
                    return slot.FirstOrDefault();
                }
                else
                {
                    throw new KeyNotFoundException();
                }
            }
            set
            {
                if (!this.Contains(key, value))
                {
                    this.Add(key, value);
                }
            }
        }

        /// <summary>
        /// Gets all values with the given Key from the Hash Table
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns></returns>
        public IEnumerable<TValue> GetValues(TKey key)
        {
            IHashSlot<TValue> slot;
            if (this._values.TryGetValue(key, out slot))
            {
                return slot;
            }
            else
            {
                throw new KeyNotFoundException();
            }
        }

        /// <summary>
        /// Adds a Key Value Pair to the Hash Table
        /// </summary>
        /// <param name="item">Key Value pair</param>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            this.Add(item.Key, item.Value);
        }

        /// <summary>
        /// Clears the Hash Table
        /// </summary>
        public void Clear()
        {
            this._values.Clear();
        }

        /// <summary>
        /// Determines whether the given Key Value pair is in the Hash Table
        /// </summary>
        /// <param name="item">Key Value pair</param>
        /// <returns></returns>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return this.Contains(item.Key, item.Value);
        }

        /// <summary>
        /// Copies the Hash Table to an array
        /// </summary>
        /// <param name="array">Array</param>
        /// <param name="arrayIndex">Index of the array to start copying into at</param>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            int i = arrayIndex;
            foreach (KeyValuePair<TKey, IHashSlot<TValue>> pair in this._values)
            {
                foreach (TValue value in pair.Value)
                {
                    array[i] = new KeyValuePair<TKey,TValue>(pair.Key, value);
                    i++;
                }
            }
        }

        /// <summary>
        /// Gets the number of Values in the Hash Table
        /// </summary>
        public int Count
        {
            get 
            { 
                return this._values.Values.Sum(slot => slot.Count); 
            }
        }

        /// <summary>
        /// Gets the number of Values for a given Key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns></returns>
        public int ValueCount(TKey key)
        {
            IHashSlot<TValue> slot;
            if (this._values.TryGetValue(key, out slot))
            {
                return slot.Count;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets the number of Keys in the Hash Table
        /// </summary>
        public int KeyCount
        {
            get
            {
                return this._values.Keys.Count;
            }
        }

        /// <summary>
        /// Returns that the Hash Table is not read only
        /// </summary>
        public bool IsReadOnly
        {
            get 
            {
                return false;
            }
        }

        /// <summary>
        /// Removes a Key Value pair from the Hash Table
        /// </summary>
        /// <param name="item">Key Value Pair to remove</param>
        /// <returns></returns>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return this.Remove(item.Key, item.Value);
        }

        /// <summary>
        /// Gets the Enumerator for the Key Value pairs in the Hash Table
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// If a Key has multiple values a pair for each value will be generated
        /// </remarks>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return (from pair in this._values
                    from value in pair.Value
                    select new KeyValuePair<TKey, TValue>(pair.Key, value)).GetEnumerator();
        }

        /// <summary>
        /// Gets the Enumerator for the Hash Table
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #region IEnumerable<TValue> Members

        /// <summary>
        /// Gets the Enumerator for Values in the Hash Table
        /// </summary>
        /// <returns></returns>
        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            return (from slot in this._values.Values
                    from value in slot
                    select value).GetEnumerator();
        }

        #endregion
    }
}
