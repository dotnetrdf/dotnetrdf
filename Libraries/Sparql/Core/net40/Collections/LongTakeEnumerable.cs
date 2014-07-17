using System;
using System.Collections.Generic;

namespace VDS.RDF.Collections
{
    public class LongTakeEnumerable<T>
        : WrapperEnumerable<T>
    {
        public LongTakeEnumerable(IEnumerable<T> enumerable, long toTake)
            : base(enumerable)
        {
            if (toTake <= 0) throw new ArgumentException("toTake must be > 0", "toTake");
            this.ToTake = toTake;
        }

        private long ToTake { get; set; }

        public override IEnumerator<T> GetEnumerator()
        {
            return new LongTakeEnumerator<T>(this.InnerEnumerable.GetEnumerator(), this.ToTake);
        }
    }

    public class LongTakeEnumerator<T>
        : WrapperEnumerator<T>
    {
        public LongTakeEnumerator(IEnumerator<T> enumerator, long toTake)
            : base(enumerator)
        {
            if (toTake <= 0) throw new ArgumentException("toTake must be > 0", "toTake");
            this.ToTake = toTake;
            this.Taken = 0;
        }

        private long ToTake { get; set; }

        private long Taken { get; set; }

        protected override bool TryMoveNext(out T item)
        {
            item = default(T);
            if (this.Taken >= this.ToTake)
            {
                return false;
            }
            if (!this.InnerEnumerator.MoveNext())
            {
                return false;
            }
            this.Taken++;
            item = this.InnerEnumerator.Current;
            return true;
        }

        protected override void ResetInternal()
        {
            this.Taken = 0;
        }
    }
}