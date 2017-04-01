/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace VDS.RDF
{
    /// <summary>
    /// Extension methods for the Windows Phone builds to avoid having to do lots of #if WINDOWS_PHONE blocks
    /// </summary>
    public static class WindowsPhoneCompatibility
    {
        /// <summary>
        /// Determines whether any key value pairs meet the given criteria
        /// </summary>
        /// <typeparam name="TKey">Key Type</typeparam>
        /// <typeparam name="TValue">Value Type</typeparam>
        /// <param name="source">Dictionary to operator over</param>
        /// <param name="predicate">Criteria</param>
        /// <returns>True if any key value pair fulfils the criteria, false otherwise</returns>
        public static bool Any<TKey, TValue>(this IDictionary<TKey, TValue> source, Func<KeyValuePair<TKey, TValue>, bool> predicate)
        {
            foreach (KeyValuePair<TKey, TValue> kvp in source)
            {
                if (predicate(kvp)) return true;
            }
            return false;
        }

        /// <summary>
        /// Removes all elements that match a given criteria
        /// </summary>
        /// <typeparam name="T">Element Type</typeparam>
        /// <param name="source">Hash Set</param>
        /// <param name="predicate">Criteria</param>
        public static void RemoveWhere<T>(this HashSet<T> source, Func<T, bool> predicate)
        {
            foreach (T item in source.ToList())
            {
                if (predicate(item)) source.Remove(item);
            }
        }
    }
}
