using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Collections
{
    public class OmitAllEnumerable<T>
        : WrapperEnumerable<T>
    {
        public OmitAllEnumerable(IEnumerable<T> enumerable, T item)
            : base(enumerable)
        {
            this.OmittedItem = item;
        }

        private T OmittedItem { get; set; }

        public override IEnumerator<T> GetEnumerator()
        {
            return new OmitAllEnumerator<T>(this.InnerEnumerable.GetEnumerator(), this.OmittedItem);
        }
    }

    public class OmitAllEnumerator<T>
        : WrapperEnumerator<T>
    {
        public OmitAllEnumerator(IEnumerator<T> enumerator, T item)
            : base(enumerator)
        {
            this.OmittedItem = item;
        }

        private T OmittedItem { get; set; }

        protected override bool TryMoveNext(out T item)
        {
            item = default(T);
            while (this.InnerEnumerator.MoveNext())
            {
                item = this.InnerEnumerator.Current;
                if (item.Equals(this.OmittedItem)) continue;

                return true;
            }
            return false;
        }
    }
}
