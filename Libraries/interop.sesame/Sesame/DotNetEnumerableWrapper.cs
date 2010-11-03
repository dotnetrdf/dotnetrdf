using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using java.util;

namespace VDS.RDF.Interop.Sesame
{
    class DotNetEnumerableWrapper : Iterator
    {
        private IEnumerator _enumerator;
        private bool _ended = false;

        public DotNetEnumerableWrapper(IEnumerable enumerable)
        {
            this._enumerator = enumerable.GetEnumerator();
        }

        public bool hasNext()
        {
            this._ended = !this._enumerator.MoveNext();
            return !this._ended;
        }

        public object next()
        {
            if (this._enumerator == null) throw new InvalidOperationException("Cannot retrieve an Item from the Iterator as the Iterator is positioned before the start of the collection");
            return this._enumerator.Current;
        }

        public void remove()
        {
            throw new NotSupportedException("Cannot remove an item from a .Net IEnumerable");
        }

    }
}
