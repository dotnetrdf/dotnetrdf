using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF
{

    class SComparer : IComparer<Triple>
    {
        public int Compare(Triple x, Triple y)
        {
            return x.Subject.CompareTo(y.Subject);
        }
    }

    class PComparer : IComparer<Triple>
    {
        public int Compare(Triple x, Triple y)
        {
            return x.Predicate.CompareTo(y.Predicate);
        }
    }

    class OComparer : IComparer<Triple>
    {
        public int Compare(Triple x, Triple y)
        {
            return x.Object.CompareTo(y.Object);
        }
    }

    class SPComparer : IComparer<Triple>
    {
        public int Compare(Triple x, Triple y)
        {
            int c = x.Subject.CompareTo(y.Subject);
            if (c == 0)
            {
                c = x.Predicate.CompareTo(y.Predicate);
            }
            return c;
        }
    }

    class POComparer : IComparer<Triple>
    {
        public int Compare(Triple x, Triple y)
        {
            int c = x.Predicate.CompareTo(y.Predicate);
            if (c == 0)
            {
                c = x.Object.CompareTo(y.Object);
            }
            return c;
        }
    }

    class OSComparer : IComparer<Triple>
    {
        public int Compare(Triple x, Triple y)
        {
            int c = x.Object.CompareTo(y.Object);
            if (c == 0)
            {
                c = x.Subject.CompareTo(y.Subject);
            }
            return c;
        }
    }
}
