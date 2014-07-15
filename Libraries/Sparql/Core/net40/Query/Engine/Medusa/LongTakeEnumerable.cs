using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Engine.Medusa
{
    public class LongTakeEnumerable<T>
        : IEnumerable<T>
    {
        public LongTakeEnumerable(IEnumerable<T> enumerable, long toTake)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            if (toTake <= 0) throw new ArgumentException("toTake must be > 0", "toTake");
            this.ToTake = toTake;
            this.InnerEnumerable = enumerable;
        }

        private long ToTake { get; set; }

        private IEnumerable<T> InnerEnumerable { get; set; } 

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class LongTakeEnumerator<T>
        : IEnumerator<T>
    {
        public LongTakeEnumerator(IEnumerator<T> enumerator, long toTake)
        {
            if (enumerator == null) throw new ArgumentNullException("enumerator");
            if (toTake <= 0) throw new ArgumentException("toTake must be > 0", "toTake");
            this.ToTake = toTake;
            this.InnerEnumerator = enumerator;
            this.Taken = 0;
            this.Finished = false;
        }

        private long ToTake { get; set; }

        private long Taken { get; set; }

        private bool Finished { get; set; }

        private IEnumerator<T> InnerEnumerator { get; set; }

        public void Dispose()
        {
            this.InnerEnumerator.Dispose();
        }

        public bool MoveNext()
        {
            if (this.Taken >= this.ToTake) return false;
            if (!this.InnerEnumerator.MoveNext())
            {
                this.Finished = true;
                return false;
            }
            this.Taken++;
            return true;
        }

        public void Reset()
        {
            this.InnerEnumerator.Reset();
            this.Taken = 0;
            this.Finished = false;
        }

        public T Current
        {
            get
            {
                if (this.Finished || this.Taken >= this.ToTake) throw new InvalidOperationException("Past the end of the enumerator");
                if (this.Taken == 0) throw new InvalidOperationException("Before the start of the enumerator, MoveNext() must be called at least once before accessing this property");
                return this.InnerEnumerator.Current;
            }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }
    }
}
