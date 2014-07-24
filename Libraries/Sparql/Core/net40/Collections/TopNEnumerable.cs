using System;
using System.Collections.Generic;

namespace VDS.RDF.Collections
{
    public class TopNEnumerable<T>
        : WrapperEnumerable<T>
    {
        public TopNEnumerable(IEnumerable<T> enumerable, IComparer<T> comparer, long n)
            : base(enumerable)
        {
            if (comparer == null) throw new ArgumentNullException("comparer");
            this.Comparer = comparer;
            if (n < 1) throw new ArgumentException("N must be >= 1", "n");
            this.N = n;
        }

        public IComparer<T> Comparer { get; private set; }

        public long N { get; private set; }

        public override IEnumerator<T> GetEnumerator()
        {
            return new TopNEnumerator<T>(this.InnerEnumerable.GetEnumerator(), this.Comparer, this.N);
        }
    }

    public class TopNEnumerator<T>
        : WrapperEnumerator<T>
    {
        public TopNEnumerator(IEnumerator<T> enumerator, IComparer<T> comparer, long n)
            : base(enumerator)
        {
            if (comparer == null) throw new ArgumentNullException("comparer");
            if (n < 1) throw new ArgumentException("N must be >= 1", "n");
            this.N = n;
            this.TopItems = new SortedList<T, byte>(comparer);
        }

        public long N { get; private set; }

        private SortedList<T, byte> TopItems { get; set; }

        private IEnumerator<KeyValuePair<T, byte>> TopItemsEnumerator { get; set; } 

        protected override bool TryMoveNext(out T item)
        {
            // First time this is accessed need to populate the Top N items list
            if (this.TopItemsEnumerator == null)
            {
                while (this.InnerEnumerator.MoveNext())
                {
                    this.TopItems.Add(this.InnerEnumerator.Current, 0);
                    if (this.TopItems.Count > this.N) this.TopItems.RemoveAt(this.TopItems.Count - 1);
                }
                this.TopItemsEnumerator = this.TopItems.GetEnumerator();
            }

            // Afterwards we just pull items from that list
            item = default(T);
            if (!this.TopItemsEnumerator.MoveNext()) return false;
            item = this.TopItemsEnumerator.Current.Key;
            return true;
        }

        protected override void ResetInternal()
        {
            this.TopItems.Clear();
            this.TopItemsEnumerator.Dispose();
            this.TopItemsEnumerator = null;
        }
    }
}
