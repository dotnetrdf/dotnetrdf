namespace VDS.RDF.Dynamic
{
    using System.Collections.Generic;
    using System.Linq;

    public class DynamicSubjectCollection<T> : DynamicSubjectCollection, ICollection<T>
    {
        public DynamicSubjectCollection(string predicate, DynamicNode subject) :
            base(
                DynamicHelper.ConvertPredicate(
                    DynamicHelper.ConvertPredicate(
                        predicate,
                        subject.Graph),
                    subject.Graph,
                    subject.BaseUri),
                subject)
        { }

        public void Add(T item)
        {
            this.Add(item as object);
        }

        public bool Contains(T item)
        {
            return base.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            base.CopyTo(array.Cast<object>().ToArray(), arrayIndex);
        }

        public bool Remove(T item)
        {
            return base.Remove(item);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this.Subjects.Select(Convert).GetEnumerator();
        }

        private T Convert(object value)
        {
            var type = typeof(T);

            if (type.IsSubclassOf(typeof(DynamicNode)))
            {
                var ctor = type.GetConstructor(new[] { typeof(INode) });
                value = ctor.Invoke(new[] { value });
            }

            return (T)value;
        }
    }
}
