using System;
using System.Collections;
using System.Collections.Generic;

namespace VDS.RDF.Collections
{
    public abstract class WrapperEnumerator<T>
        : IEnumerator<T>
    {
        private T _current;

        protected WrapperEnumerator(IEnumerator<T> enumerator)
        {
            if (enumerator == null) throw new ArgumentNullException("enumerator");
            this.InnerEnumerator = enumerator;
        }

        protected IEnumerator<T> InnerEnumerator { get; private set; }

        public virtual void Dispose()
        {
            this.InnerEnumerator.Dispose();
        }

        private bool Started { get; set; }

        private bool Finished { get; set; }

        public bool MoveNext()
        {
            this.Started = true;
            if (this.Finished) return false;
            T item;
            if (this.TryMoveNext(out item))
            {
                this.Current = item;
                return true;
            }
            this.Finished = true;
            return false;
        }

        protected abstract bool TryMoveNext(out T item);

        public void Reset()
        {
            this.Started = false;
            this.Finished = false;
            this.InnerEnumerator.Reset();
        }

        protected virtual void ResetInternal() {}

        public T Current
        {
            get
            {
                if (!this.Started) throw new InvalidOperationException("Currently before the start of the enumerator, call MoveNext() before accessing Current");
                if (this.Finished) throw new InvalidOperationException("Currently after end of the enumerator");
                return this._current;
            }
            private set { this._current = value; }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }
    }
}