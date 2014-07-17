using System;
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
    }
}
