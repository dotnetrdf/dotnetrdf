using System;
using System.Collections;
using System.Collections.Generic;

namespace VDS.RDF.Query.Engine.Medusa
{
    public class LongSkipEnumerable<T>
        : IEnumerable<T>
    {
        public LongSkipEnumerable(IEnumerable<T> enumerable, long toSkip)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            if (toSkip <= 0) throw new ArgumentException("toSkip must be > 0", "toSkip");
            this.ToSkip = toSkip;
            this.InnerEnumerable = enumerable;
        }

        private long ToSkip { get; set; }

        private IEnumerable<T> InnerEnumerable { get; set; } 

        public IEnumerator<T> GetEnumerator()
        {
            return new LongSkipEnumerator<T>(this.InnerEnumerable.GetEnumerator(), this.ToSkip);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class LongSkipEnumerator<T>
        : IEnumerator<T>
    {
        public LongSkipEnumerator(IEnumerator<T> enumerator, long toSkip)
        {
            if (enumerator == null) throw new ArgumentNullException("enumerator");
            if (toSkip <= 0) throw new ArgumentException("toSkip must be > 0", "toSkip");
            this.ToSkip = toSkip;
            this.InnerEnumerator = enumerator;
            this.Skipped = 0;
        }

        private long ToSkip { get; set; }

        private IEnumerator<T> InnerEnumerator { get; set; }

        private long Skipped { get; set; }

        public void Dispose()
        {
            this.InnerEnumerator.Dispose();
        }

        private bool TrySkip()
        {
            while (this.Skipped < this.ToSkip)
            {
                if (!this.InnerEnumerator.MoveNext()) return false;
                this.Skipped++;
            }
            return true;
        }

        public bool MoveNext()
        {
            // We've previously done the skipping so can just defer to inner enumerator
            if (this.Skipped == this.ToSkip) return this.InnerEnumerator.MoveNext();

            // First time being accessed so attempt to skip if possible
            return this.TrySkip();
        }

        public void Reset()
        {
            this.InnerEnumerator.Reset();
            this.Skipped = 0;
        }

        public T Current
        {
            get
            {
                if (this.Skipped == this.ToSkip) return this.InnerEnumerator.Current;
                if (this.Skipped == 0) throw new InvalidOperationException("Currently before the first element in the enumerator, MoveNext() must be called at least once before accessing this property");
                throw new InvalidOperationException("No element at current position");
            }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }
    }
}
