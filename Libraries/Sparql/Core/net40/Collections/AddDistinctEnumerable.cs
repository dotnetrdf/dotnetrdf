using System;
using System.Collections.Generic;

namespace VDS.RDF.Collections
{
    public class AddDistinctEnumerable<T>
        : WrapperEnumerable<T>
    {
        public AddDistinctEnumerable(IEnumerable<T> enumerable, T item)
            : base(enumerable)
        {
            this.AdditionalItem = item;
        }

        private T AdditionalItem { get; set; }

        public override IEnumerator<T> GetEnumerator()
        {
            return new AddDistinctEnumerator<T>(this.InnerEnumerable.GetEnumerator(), this.AdditionalItem);
        }
    }

    public class AddDistinctEnumerator<T>
        : WrapperEnumerator<T>
    {

        public AddDistinctEnumerator(IEnumerator<T> enumerator, T item)
            : base(enumerator)
        {
            this.AdditionalItem = item;
            this.AdditionalItemSeen = false;
        }

        private bool AdditionalItemSeen { get; set; }

        private bool IsCurrentAdditionalItem { get; set; }

        private T AdditionalItem { get; set; }

        protected override bool TryMoveNext(out T item)
        {
            item = default(T);
            if (this.InnerEnumerator.MoveNext())
            {
                item = this.InnerEnumerator.Current;
                if (item.Equals(this.AdditionalItem)) this.AdditionalItemSeen = true;
                return true;
            }
            if (this.AdditionalItemSeen) return false;
            if (this.IsCurrentAdditionalItem) return false;
            item = this.AdditionalItem;
            this.IsCurrentAdditionalItem = true;
            return true;
        }

        protected override void ResetInternal()
        {
            this.AdditionalItemSeen = false;
            this.IsCurrentAdditionalItem = false;
        }
    }
}
