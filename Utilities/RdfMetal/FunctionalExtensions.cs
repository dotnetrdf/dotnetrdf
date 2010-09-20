using System;
using System.Collections.Generic;
using System.Linq;

namespace rdfMetal
{
    public static class FunctionalExtensions
    {
        public static IEnumerable<R> Map<T, R>(this IEnumerable<T> seq, Func<T, R> f)
        {
            foreach (T a in seq)
            {
                yield return f(a);
            }
        }

        /// <summary>
        /// Concatenates all of the sequences in <see cref="seqseq"/> and converts them into a single sequence of <see cref="T"/>.
        /// </summary>
        /// <typeparam name="T">the type of the inner sequences</typeparam>
        /// <param name="seqseq">The sequence of sequences.</param>
        /// <returns>a sequence of <see cref="T"/> containing allt he elements of the inner sequences</returns>
        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> seqseq)
        {
            IEnumerable<T> result = new T[] { };
            foreach (IEnumerable<T> ts in seqseq)
            {
                result = result.Concat(ts);
            }
            return result;
        }
    }
}