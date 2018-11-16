namespace VDS.RDF.Dynamic
{
    using System.Collections.Generic;
    using System.Linq;

    public class DynamicSubjectCollection<T> : DynamicSubjectCollection, ICollection<T> where T : INode
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
            this.Add(item);
        }

        public bool Contains(T item)
        {
            return base.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            base.CopyTo(array.Cast<INode>().ToArray(), arrayIndex);
        }

        public bool Remove(T item)
        {
            return base.Remove(item);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this.Subjects.Select(Convert).GetEnumerator();
        }

        private T Convert(INode value)
        {
            var type = typeof(T);

            if (type.IsSubclassOf(typeof(DynamicNode)))
            {
                var ctor = type.GetConstructor(new[] { typeof(INode) });
                value = ctor.Invoke(new[] { value }) as DynamicNode;
            }

            return (T)value;
        }
    }
}
