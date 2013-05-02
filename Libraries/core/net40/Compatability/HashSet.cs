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

//This code contributed by Peter Kahle as part of his Windows Phone 7 porting efforts

#if WINDOWS_PHONE 

using System;
using System.Linq;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using VDS.Common.Collections;

namespace VDS.RDF
{
    /// <summary>
    /// Emulates the HashSet class for Windows Phone since Windows Phone doesn't have the HashSet class in its Silverlight based profile
    /// </summary>
    /// <remarks>
    /// Code taken from http://social.msdn.microsoft.com/Forums/en-US/windowsphone7series/thread/e1dd3571-dfb8-4abe-b63a-62106d6a4965/
    /// </remarks>
    public class HashSet<T> 
        : ICollection<T>
    {
        private MultiDictionary<T, List<T>> _dictionary;

        public HashSet()
        {
            this._dictionary = new MultiDictionary<T, List<T>>();
        }

        public HashSet(IEnumerable<T> items)
            : this()
        {
            foreach (T item in items)
            {
                this.Add(item);
            }
        }
        // Methods
        public void Add(T item)
        {
            if (!this.Contains(item))
            {
                this._dictionary.Add(item, new List<T> { item });
            }
        }

        public void Clear()
        {
            this._dictionary.Clear();
        }

        public bool Contains(T item)
        {
            List<T> values;
            if (this._dictionary.TryGetValue(item, out values))
            {
                return values.Contains(item);
            }
            else
            {
                return false;
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this._dictionary.Keys.CopyTo(array, arrayIndex);
            //throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            List<T> values;
            if (this._dictionary.TryGetValue(item, out values))
            {
                bool result = values.Remove(item);
                if (result && values.Count == 0) this._dictionary.Remove(item);
                return result;
            }
            else
            {
                return false;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return (from items in this._dictionary.Values
                    from item in items
                    select item).GetEnumerator();
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

        // Properties
        public int Count
        {
            get
            {
                return (from items in this._dictionary.Values
                        select items.Count).Sum();
            }
        }

        public bool IsReadOnly
        {
            get
            { 
                return false;
            }
        }
    }

}

#endif
