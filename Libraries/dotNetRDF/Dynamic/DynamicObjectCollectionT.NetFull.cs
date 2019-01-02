namespace VDS.RDF.Dynamic
{
    using System.Collections.Generic;
    using System.Linq;

    public class DynamicObjectCollection<T> : DynamicObjectCollection, ICollection<T>
    {
        public DynamicObjectCollection(DynamicNode subject, string predicate) :
            base(
                subject,
                DynamicHelper.ConvertPredicate(
                    DynamicHelper.ConvertPredicate(
                        predicate,
                        subject.Graph),
                    subject.Graph,
                    subject.BaseUri))
        { }

        public void Add(T item)
        {
            base.Add(item);
        }

        public bool Contains(T item)
        {
            return base.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.Objects.Select(Convert).ToArray().CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return base.Remove(item);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this.Objects.Select(Convert).GetEnumerator();
        }

        private T Convert(object value)
        {
            var type = typeof(T);

            if (type.IsSubclassOf(typeof(DynamicNode)))
            {
                // TODO: Exception handling
                var ctor = type.GetConstructor(new[] { typeof(INode) });
                value = ctor.Invoke(new[] { value });
            }

            return (T)value;
        }
    }
}
