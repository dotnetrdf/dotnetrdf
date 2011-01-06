/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

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

#define WINDOWS_PHONE

//This code contributed by Peter Kahle as part of his Windows Phone 7 porting efforts

#if WINDOWS_PHONE 

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections;
using System.Collections.Generic;

namespace VDS.RDF
{
    /// <summary>
    /// Emulates the HashSet class for Windows Phone since Windows Phone doesn't have the HashSet class in its Silverlight based profile
    /// </summary>
    /// <remarks>
    /// Code taken from http://social.msdn.microsoft.com/Forums/en-US/windowsphone7series/thread/e1dd3571-dfb8-4abe-b63a-62106d6a4965/
    /// </remarks>
    public class HashSet<T> : ICollection<T>
    {
        private Dictionary<T, short> MyDict;

        public HashSet()
        {
            MyDict = new Dictionary<T, short>();
        }

        public HashSet(IEnumerable enumer)
        {
            MyDict = new Dictionary<T, short>();
            foreach (T item in enumer)
            {
                MyDict.Add(item,0);
            }
        }
        // Methods
        public void Add(T item)
        {
            // We don't care for the value in dictionary, Keys matter.
            MyDict.Add(item, 0);
        }

        public void Clear()
        {
            MyDict.Clear();
        }

        public bool Contains(T item)
        {
            return MyDict.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            return MyDict.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void UnionWith(IEnumerable<T> other)
        {
            foreach (T item in other)
            {
                try
                {
                    MyDict.Add(item, 0);
                }
                catch (ArgumentException) { }
            }
        }

        // Properties
        public int Count
        {
            get { return MyDict.Keys.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }
    }

}

#endif
