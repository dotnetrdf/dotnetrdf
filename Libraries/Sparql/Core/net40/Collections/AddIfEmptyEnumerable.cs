using System;
using System.Collections.Generic;

namespace VDS.RDF.Collections
{
    public class AddIfEmptyEnumerable<T>
        : WrapperEnumerable<T>
    {
        public AddIfEmptyEnumerable(IEnumerable<T> enumerable, T item)
            : base(enumerable)
        {
            this.AdditionalItem = item;
        }

        public T AdditionalItem { get; private set; }

        public override IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public class AddIfEmptyEnumerator<T>
        : WrapperEnumerator<T>
    {
        public AddIfEmptyEnumerator(IEnumerator<T> enumerator, T item)
            : base(enumerator)
        {
            this.AdditionalItem = item;
        }

        public T AdditionalItem { get; private set; }

        private bool AnyItemsSeen { get; set; }

        private bool IsCurrentAdditionalItem { get; set; }

        protected override bool TryMoveNext(out T item)
        {
            item = default(T);
            if (this.InnerEnumerator.MoveNext())
            {
                this.AnyItemsSeen = true;
                item = this.InnerEnumerator.Current;
                return true;
            }
            if (this.AnyItemsSeen) return false;
            if (this.IsCurrentAdditionalItem) return false;

            this.IsCurrentAdditionalItem = true;
            item = this.AdditionalItem;
            return true;
        }

        protected override void ResetInternal()
        {
            this.AnyItemsSeen = false;
            this.IsCurrentAdditionalItem = false;
        }
    }
}
