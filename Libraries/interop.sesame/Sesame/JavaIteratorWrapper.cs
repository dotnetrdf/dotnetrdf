using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using java.util;

namespace VDS.RDF.Interop
{
    public class JavaIteratorWrapper<T> : IEnumerable<T>
    {
        private Iterator _iter;

        public JavaIteratorWrapper(Iterator iter)
        {
            this._iter = iter;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new JavaIteratorEnumerator<T>(this._iter);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    class JavaIteratorEnumerator<T> : IEnumerator<T>
    {
        private Iterator _iter;
        private T _current;
        private bool _atStart = true;

        public JavaIteratorEnumerator(Iterator iter)
        {
            this._iter = iter;
        }

        public T Current
        {
            get 
            { 
                if (this._atStart) throw new InvalidOperationException("Enumerator is positioned before the start of the collection");
                if (this._current == null) throw new InvalidOperationException("Enumerator is positioned after the end of the collection");
                return this._current;
            }
        }

        public void Dispose()
        {
            this._atStart = true;
            this._current = default(T);
        }

        object System.Collections.IEnumerator.Current
        {
            get 
            { 
                return this.Current; 
            }
        }

        public bool MoveNext()
        {
            if (this._atStart) this._atStart = false;

            if (this._iter.hasNext())
            {
                Object obj = this._iter.next();
                if (obj is T)
                {
                    this._current = (T)this._iter.next();
                    return true;
                }
                else
                {
                    throw new InvalidOperationException("The underlying Java enumerator contains objects which cannot be converted to the type " + typeof(T).Name + " which is the generic type of this enumerator");
                }
            } 
            else 
            {
                this._current = default(T);
                return false;
            }
        }

        public void Reset()
        {
            throw new NotSupportedException("The Java Iterator Enumerator does not support the Reset() operation");
        }
    }
}
