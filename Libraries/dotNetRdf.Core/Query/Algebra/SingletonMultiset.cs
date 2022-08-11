// unset

using System.Collections.Generic;

namespace VDS.RDF.Query.Algebra
{
    internal class SingletonMultiset : Multiset
    {
        public SingletonMultiset()
            : base()
        {
            Add(new Set());
        }

        public SingletonMultiset(IEnumerable<string> vars)
            : base(vars)
        {
            Add(new Set());
        }
    }
}