using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Ordering
{
    class TripleComparer : IComparer<Triple>
    {
        private IComparer<Triple> _child;
        private Func<Triple, Triple, int> _compareFunc;
        private int _modifier = 1;

        public TripleComparer(Func<Triple, Triple, int> compareFunc, bool descending, IComparer<Triple> child)
        {
            this._compareFunc = compareFunc;
            this._child = child;
            if (descending)
            {
                this._modifier = -1;
            }
        }

        public TripleComparer(Func<Triple, Triple, int> compareFunc, IComparer<Triple> child)
            : this(compareFunc, false, child) { }

        public TripleComparer(Func<Triple, Triple, int> compareFunc)
            : this(compareFunc, null) { }

        public int Compare(Triple x, Triple y)
        {
            if (this._compareFunc == null)
            {
                return 0;
            }
            else
            {
                int c = this._compareFunc(x, y);
                if (c == 0)
                {
                    if (this._child != null)
                    {
                        return this._modifier * this._child.Compare(x, y);
                    }
                    else
                    {
                        return this._modifier * c;
                    }
                }
                else
                {
                    return this._modifier * c;
                }
            }
        }
    }
}
