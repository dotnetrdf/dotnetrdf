/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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

using System.Collections.Generic;

namespace VDS.RDF.Collections
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Skip<T>(this IEnumerable<T> enumerable, long toSkip)
        {
            return new LongSkipEnumerable<T>(enumerable, toSkip);
        }

        public static IEnumerable<T> Take<T>(this IEnumerable<T> enumerable, long toTake)
        {
            return new LongTakeEnumerable<T>(enumerable, toTake);
        }

        public static IEnumerable<T> OmitAll<T>(this IEnumerable<T> enumerable, T item)
        {
            return new OmitAllEnumerable<T>(enumerable, item);
        }

        public static IEnumerable<T> AddDistinct<T>(this IEnumerable<T> enumerable, T item)
        {
            return new AddDistinctEnumerable<T>(enumerable, item);
        }

        public static IEnumerable<T> AddIfEmpty<T>(this IEnumerable<T> enumerable, T item)
        {
            return new AddIfEmptyEnumerable<T>(enumerable, item);
        }

        /// <summary>
        /// Returns an enumerable which eliminates adjacent duplicates from the given enumerable
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Enumerable which removes adjacent duplicates</returns>
        public static IEnumerable<T> Reduced<T>(this IEnumerable<T> enumerable)
        {
            return new ReducedEnumerable<T>(enumerable);
        }

        public static IEnumerable<T> Top<T>(this IEnumerable<T> enumerable, IComparer<T> comparer, long n)
        {
            return new TopNEnumerable<T>(enumerable, comparer, n);
        }

        public static IEnumerable<T> Top<T>(this IEnumerable<T> enumerable, long n)
        {
            return Top(enumerable, Comparer<T>.Default, n);
        }
    }
}
