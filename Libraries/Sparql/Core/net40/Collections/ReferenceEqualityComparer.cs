using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace VDS.RDF.Collections
{
    /// <summary>
    /// An equality comparer based purely on Object reference
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    public sealed class ReferenceEqualityComparer<T>
        : IEqualityComparer<T>
    {
        public bool Equals(T x, T y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}
