/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

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