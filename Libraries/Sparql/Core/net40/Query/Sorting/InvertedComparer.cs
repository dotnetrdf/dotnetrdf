using System;
using System.Collections.Generic;

namespace VDS.RDF.Query.Sorting
{
    public class InvertedComparer<T>
        : IComparer<T>
    {
        public InvertedComparer()
            : this(Comparer<T>.Default) { }

        public InvertedComparer(IComparer<T> comparer)
        {
            if (comparer == null) throw new ArgumentNullException();
            this.InnerComparer = comparer;
        }

        private IComparer<T> InnerComparer { get; set; }

        public int Compare(T x, T y)
        {
            return -1*this.InnerComparer.Compare(x, y);
        }
    }
}
