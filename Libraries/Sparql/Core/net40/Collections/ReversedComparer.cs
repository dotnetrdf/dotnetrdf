using System;
using System.Collections.Generic;

namespace VDS.RDF.Collections
{
    public class ReversedComparer<T>
        : IComparer<T>
    {
        public ReversedComparer()
            : this(Comparer<T>.Default) { }

        public ReversedComparer(IComparer<T> comparer)
        {
            if (comparer == null) throw new ArgumentNullException("comparer");
            this.InnerComparer = comparer;
        }

        public IComparer<T> InnerComparer { get; private set; }

        public int Compare(T x, T y)
        {
            return this.InnerComparer.Compare(y, x);
        }
    }
}
