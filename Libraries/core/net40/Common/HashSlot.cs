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
    /// Represents a slot in the Hash Table
    /// </summary>
    /// <typeparam name="T">Type of Values stored in the Slot</typeparam>
    /// <remarks>
    /// Hash Slots may contain duplicate values
    /// </remarks>
    [Obsolete("The HashTable class has known deficiencies in hash code collision behaviour and has been superceded by MultiDictionary which should be used instead, HashTable will be removed in subsequent releases", true)]
    class ListSlot<T> 
        : IHashSlot<T>
    {
        private List<T> _values;

        /// <summary>
        /// Creates a new Hash Slot which contains the given Value
        /// </summary>
        /// <param name="value">Value</param>
        public ListSlot(T value)
        {
            this._values = new List<T>(1);
            this._values.Add(value);
        }

        /// <summary>
        /// Creates a new Hash Slot which is an empty
        /// </summary>
        /// <param name="capacity">Initial Capacity of Slot</param>
        public ListSlot(int capacity)
        {
            this._values = new List<T>(capacity);
        }

        /// <summary>
        /// Creates a new Hash Slot which contains the given Value and has the given initial capacity
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="capacity">Initial Capacity of Slot</param>
        public ListSlot(T value, int capacity)
        {
            if (capacity >= 1)
            {
                this._values = new List<T>(capacity);
            }
            else
            {
                this._values = new List<T>();
            }
            this._values.Add(value);
        }

        /// <summary>
        /// Gets the Enumerator of the Values in the Slot
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return this._values.GetEnumerator();
        }

        /// <summary>
        /// Gets the Enumerator of the Values in the Slot
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this._values.GetEnumerator();
        }

        /// <summary>
        /// Clears the contents of the Hash Slot
        /// </summary>
        public void Clear()
        {
            this._values.Clear();
        }

        /// <summary>
        /// Checks whether the Hash Slot contains a given Value
        /// </summary>
        /// <param name="item">Item to look for</param>
        /// <returns></returns>
        public bool Contains(T item)
        {
            return this._values.Contains(item);
        }

        /// <summary>
        /// Copies the contents of the Hash Slot to an array
        /// </summary>
        /// <param name="array">Array to copy to</param>
        /// <param name="arrayIndex">Index of the array at which to start copying data in</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            int i = arrayIndex;
            foreach (T value in this._values)
            {
                array[i] = value;
                i++;
            }
        }

        /// <summary>
        /// Gets the Count of Values in the Hash Slot
        /// </summary>
        public int Count
        {
            get
            {
                return this._values.Count;
            }
        }

        /// <summary>
        /// Returns that a Hash Slot is not read only
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Removes an Item from the Hash Slot
        /// </summary>
        /// <param name="item">Value to remove</param>
        /// <returns></returns>
        public bool Remove(T item)
        {
            return this._values.Remove(item);
        }

        /// <summary>
        /// Adds an Item to the Hash Slot
        /// </summary>
        /// <param name="item">Value to add</param>
        public void Add(T item)
        {
            this._values.Add(item);
        }
    }

    [Obsolete("The HashTable class has known deficiencies in hash code collision behaviour and has been superceded by MultiDictionary which should be used instead, HashTable will be removed in subsequent releases", false)]
    class CompactSlot<T>
        : IHashSlot<T>
    {
        private LinkedList<T> _values;

        public CompactSlot()
        {
            this._values = new LinkedList<T>();
        }

        public CompactSlot(T value)
        {
            this._values = new LinkedList<T>();
            this._values.AddLast(value);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this._values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Add(T item)
        {
            this._values.AddLast(item);
        }

        public void Clear()
        {
            this._values.Clear();
        }

        public bool Contains(T item)
        {
            return this._values.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this._values.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                return this._values.Count;
            }
        }

        public bool IsReadOnly
        {
            get 
            {
                return false;
            }
        }

        public bool Remove(T item)
        {
            return this._values.Remove(item);
        }
    }

#if !SILVERLIGHT

    [Obsolete("The HashTable class has known deficiencies in hash code collision behaviour and has been superceded by MultiDictionary which should be used instead, HashTable will be removed in subsequent releases", false)]
    class SetSlot<T>
        : IHashSlot<T>
    {
        private HashSet<T> _values;

        public SetSlot()
        {
            this._values = new HashSet<T>();
        }

        public SetSlot(T value)
        {
            this._values = new HashSet<T>();
            this._values.Add(value);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this._values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Add(T item)
        {
            this._values.Add(item);
        }

        public void Clear()
        {
            this._values.Clear();
        }

        public bool Contains(T item)
        {
            return this._values.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this._values.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get 
            {
                return this._values.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool Remove(T item)
        {
            return this._values.Remove(item);
        }
    }

#endif
}
