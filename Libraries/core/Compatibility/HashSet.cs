/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

//Uncomment when working on this code, don't forget to comment out afterwards
//#define WINDOWS_PHONE

//This code originally contributed by Peter Kahle as part of his Windows Phone 7 porting efforts

using System.Linq;
using VDS.RDF.Parsing.Tokens;
#if WINDOWS_PHONE
using System.Collections;
using System.Collections.Generic;

namespace VDS.RDF
{
    /// <summary>
    /// Emulates the HashSet class for Windows Phone since Windows Phone doesn't have the HashSet class in its Silverlight based profile
    /// </summary>
    /// <remarks>
    /// Does not handle null items
    /// </remarks>
    public class HashSet<T>
        : ICollection<T>, ISet<T>
    {
        private const byte Value = 0x0;
        private readonly IDictionary<T, byte> _dictionary;

        public HashSet()
        {
            this._dictionary = new Dictionary<T, byte>();
        }

        public HashSet(IEnumerable<T> items)
            : this()
        {
            foreach (T item in items)
            {
                this.Add(item);
            }
        }

        public bool Add(T item)
        {
            if (this.Contains(item)) return false;
            this._dictionary.Add(item, Value);
            return true;
        }

        void ICollection<T>.Add(T item)
        {
            ((ISet<T>)this).Add(item);
        }

        public void Clear()
        {
            this._dictionary.Clear();
        }

        public bool Contains(T item)
        {
            return this._dictionary.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this._dictionary.Keys.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return this._dictionary.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this._dictionary.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void UnionWith(IEnumerable<T> other)
        {
            foreach (T item in other)
            {
                this.Add(item);
            }
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            ISet<T> temp = new HashSet<T>(other);
            foreach (T key in this._dictionary.Keys.ToList())
            {
                if (!temp.Contains(key)) this._dictionary.Remove(key);
            }
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            foreach (T key in other)
            {
                this._dictionary.Remove(key);
            }
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            ISet<T> temp = new HashSet<T>(other);
            // Firstly remove things present in both
            foreach (T key in temp.ToList())
            {
                if (this.Remove(key))
                {
                    // Also in other set so remove
                    temp.Remove(key);
                }
                else
                {
                    // Not in current set so add
                    this.Add(key);
                }
            }
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            ISet<T> temp = new HashSet<T>(other);
            return this._dictionary.Keys.All(k => temp.Contains(k));
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            ISet<T> temp = new HashSet<T>(other);
            if (temp.Count > this.Count) return false;
            return temp.All(k => this.Contains(k));
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            ISet<T> temp = new HashSet<T>(other);
            if (temp.Count == 0 && this.Count != 0) return true;
            if (temp.Count > this.Count) return false;
            return temp.All(k => this.Contains(k));
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            ISet<T> temp = new HashSet<T>(other);
            if (this.Count == 0 && temp.Count != 0) return true;
            if (this.Count > temp.Count) return false;
            return this._dictionary.Keys.All(k => temp.Contains(k));
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return other.Any(k => this.Contains(k));
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            ISet<T> temp = new HashSet<T>(other);
            if (this.Count != temp.Count) return false;
            return this._dictionary.Keys.All(k => temp.Contains(k));
        }

        public int Count
        {
            get { return this._dictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }
    }
}

#endif