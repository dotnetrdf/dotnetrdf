using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using java.util;
using info.aduna.iteration;

namespace VDS.RDF.Interop.Sesame
{
    class DotNetEnumerableWrapper : Iterator
    {
        private IEnumerator _enumerator;

        public DotNetEnumerableWrapper(IEnumerable enumerable)
        {
            this._enumerator = enumerable.GetEnumerator();
        }

        public bool hasNext()
        {
            return this._enumerator.MoveNext();
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

    class DotNetAdunaIterationWrapper : CloseableIteration
    {
        private IEnumerator _enumerator;

        public DotNetAdunaIterationWrapper(IEnumerable enumerable)
        {
            this._enumerator = enumerable.GetEnumerator();
        }        

        #region CloseableIteration Members

        public void close()
        {
            if (this._enumerator != null) this._enumerator = null;
        }

        #endregion

        #region Iteration Members

        public bool hasNext()
        {
            return this._enumerator.MoveNext();
        }

        public object next()
        {
            if (this._enumerator == null) throw new InvalidOperationException("Cannot retrieve an Item from the Iterator as the Iterator is positioned before the start of the collection");
            return this._enumerator.Current;
        }

        public void remove()
        {
            throw new NotSupportedException("Cannot remove an item from a .Net Enumerable");
        }

        #endregion
    }
}
