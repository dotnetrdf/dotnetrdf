using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Engine.Medusa
{
    public static class MedusaExtensions
    {
        public static IEnumerable<T> Skip<T>(this IEnumerable<T> enumerable, long toSkip)
        {
            return new LongSkipEnumerable<T>(enumerable, toSkip);
        }

        public static IEnumerable<T> Take<T>(this IEnumerable<T> enumerable, long toTake)
        {
            return new LongTakeEnumerable<T>(enumerable, toTake);
        }
    }
}
