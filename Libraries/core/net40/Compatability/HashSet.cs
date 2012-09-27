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

//Uncomment when working on this code, don't forget to comment out afterwards
//#define WINDOWS_PHONE

//This code contributed by Peter Kahle as part of his Windows Phone 7 porting efforts

#if WINDOWS_PHONE 

using System;
using System.Linq;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using VDS.Common;

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
            throw new NotImplementedException();
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
