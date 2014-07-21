using System.Collections.Generic;

namespace VDS.RDF.Collections
{
    public class ReducedEnumerable<T>
        : WrapperEnumerable<T>
    {
        public ReducedEnumerable(IEnumerable<T> enumerable)
            : base(enumerable) { }

        public override IEnumerator<T> GetEnumerator()
        {
            return new ReducedEnumerator<T>(this.InnerEnumerable.GetEnumerator());
        }
    }

    public class ReducedEnumerator<T>
        : WrapperEnumerator<T>
    {
        public ReducedEnumerator(IEnumerator<T> enumerator)
            : base(enumerator)
        {
            this.First = true;
        }

        private bool First { get; set; }

        private T LastItem { get; set; }

        protected override bool TryMoveNext(out T item)
        {
            item = default(T);
            if (this.InnerEnumerator.MoveNext()) return false;
            item = this.InnerEnumerator.Current;

            if (this.First)
            {
                this.First = false;
                this.LastItem = item;
                return true;
            }

            while (true)
            {
                // Provided the next item is not the same as the previous return it
                if (!this.LastItem.Equals(item))
                {
                    this.LastItem = item;
                    return true;
                }

                if (!this.InnerEnumerator.MoveNext()) return false;
                item = this.InnerEnumerator.Current;
            }
        }

        protected override void ResetInternal()
        {
            this.First = true;
            this.LastItem = default(T);
        }
    }
}
