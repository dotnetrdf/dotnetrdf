using System;
using System.Collections.Generic;

namespace VDS.RDF.Collections
{
    public class LongSkipEnumerable<T>
        : WrapperEnumerable<T>
    {
        public LongSkipEnumerable(IEnumerable<T> enumerable, long toSkip)
            : base(enumerable)
        {
            if (toSkip <= 0) throw new ArgumentException("toSkip must be > 0", "toSkip");
            this.ToSkip = toSkip;
        }

        private long ToSkip { get; set; }

        public override IEnumerator<T> GetEnumerator()
        {
            return new LongSkipEnumerator<T>(this.InnerEnumerable.GetEnumerator(), this.ToSkip);
        }
    }

    public class LongSkipEnumerator<T>
        : WrapperEnumerator<T>
    {
        public LongSkipEnumerator(IEnumerator<T> enumerator, long toSkip)
            : base(enumerator)
        {
            if (toSkip <= 0) throw new ArgumentException("toSkip must be > 0", "toSkip");
            this.ToSkip = toSkip;
            this.Skipped = 0;
        }

        private long ToSkip { get; set; }

        private long Skipped { get; set; }

        private bool TrySkip()
        {
            while (this.Skipped < this.ToSkip)
            {
                if (!this.InnerEnumerator.MoveNext()) return false;
                this.Skipped++;
            }
            return true;
        }

        protected override bool TryMoveNext(out T item)
        {
            item = default(T);

            // If we've previously done the skipping so can just defer to inner enumerator
            if (this.Skipped == this.ToSkip)
            {
                if (!this.InnerEnumerator.MoveNext()) return false;
                item = this.InnerEnumerator.Current;
                return true;
            }

            // First time being accessed so attempt to skip if possible
            if (!this.TrySkip()) return false;
            item = this.InnerEnumerator.Current;
            return true;
        }

        protected override void ResetInternal()
        {
            this.Skipped = 0;
        }
    }
}
