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
    /// Possible Triple Index types
    /// </summary>
    /// <remarks>
    /// <para>
    /// Index types are given Integer values with the lowest being the least useful index and the highest being most useful index.  Non-Index based Patterns are given arbitrary high values since these will typically never be used as these items are usually inserted into a Graph Pattern after the ordering step
    /// </para>
    /// <para>
    /// When used to sort Patterns as part of query optimisation the patterns are partially ordered on the usefullness of their index since more useful indexes are considered more likely to return fewer results which will help restrict the query space earlier in the execution process.
    /// </para>
    /// </remarks>
    public enum TripleIndexType : int
    {
        /// <summary>
        /// No Index should be used as the Pattern does not use Variables
        /// </summary>
        NoVariables = -2,
        /// <summary>
        /// No Index should be used as the Pattern is three Variables
        /// </summary>
        None = -1,
        /// <summary>
        /// Subject Index should be used
        /// </summary>
        Subject = 2,
        /// <summary>
        /// Predicate Index should be used
        /// </summary>
        Predicate = 1,
        /// <summary>
        /// Object Index should be used
        /// </summary>
        Object = 0,
        /// <summary>
        /// Subject-Predicate Index should be used
        /// </summary>
        SubjectPredicate = 5,
        /// <summary>
        /// Predicate-Object Index should be used
        /// </summary>
        PredicateObject = 3,
        /// <summary>
        /// Subject-Object Index should be used
        /// </summary>
        SubjectObject = 4
    }

    /// <summary>
    /// A Comparer which sorts based on Triple Index Type
    /// </summary>
    class TripleIndexSorter
        : IComparer<TripleIndexType>
    {
        /// <summary>
        /// Compares two Triple Index types to see which is greater
        /// </summary>
        /// <param name="x">First Index type</param>
        /// <param name="y">Second Index type</param>
        /// <returns></returns>
        /// <remarks>
        /// Implemented by converting to Integers and then using the Integer comparison function
        /// </remarks>
        public int Compare(TripleIndexType x, TripleIndexType y)
        {
            int a, b;
            a = (int)x;
            b = (int)y;

            return a.CompareTo(b);
        }
    }

    /// <summary>
    /// Helper Class for indexing related operations
    /// </summary>
    [Obsolete("This helper pertains to obsoleted code and will be removed in future releases", true)]
    public static class IndexHelper
    {
        /// <summary>
        /// Searches an Index using the given Comparer
        /// </summary>
        /// <typeparam name="T">Indexed Object Type</typeparam>
        /// <param name="index">Index</param>
        /// <param name="comparer">Comparer to use for binary search</param>
        /// <param name="search">Item to search for</param>
        /// <returns></returns>
        [Obsolete("This helper pertains to obsoleted code and will be removed in future releases", true)]
        public static IEnumerable<T> SearchIndex<T>(this List<T> index, IComparer<T> comparer, T search)
        {
            // If Index is empty then there are no results
            if (index.Count == 0) return Enumerable.Empty<T>();

            int lower = 0;
            int upper = index.Count - 1;
            int middle, c;
            int start = upper + 1, end = upper;

            // Find the First point at which the Search Triple occurs
            do
            {
                if (lower > upper) return Enumerable.Empty<T>();
                middle = (lower + upper) / 2;

                c = comparer.Compare(index[middle], search);
                if (c < 0)
                {
                    // Increment lower bound
                    lower = middle + 1;
                }
                else
                {
                    // If equal record possible start point
                    if (c == 0) start = middle;

                    // Decrement upper bound and end
                    upper = middle - 1;
                    if (c != 0) end = middle;
                }
            } while (lower < start);

            if (start >= index.Count) return Enumerable.Empty<T>();

            // Find the Last point at which the Search Triple occurs
            lower = start;
            upper = end;
            do
            {
                if (lower > upper) break;
                middle = (lower + upper) / 2;

                c = comparer.Compare(index[middle], search);
                if (c > 0)
                {
                    // Decrement upper bound
                    upper = middle - 1;
                }
                else
                {
                    // If equal record possible end point
                    if (c == 0) end = middle;

                    // Increment lower bound
                    lower = middle + 1;
                }
            } while (true);

            end++;
            return (from i in Enumerable.Range(start, end - start)
                    select index[i]);
        }
    }
}