namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class DynamicObjectCollection : ICollection<object>
    {
        private readonly DynamicNode subject;
        private readonly INode predicate;

        public DynamicObjectCollection(DynamicNode subject, INode predicate)
        {
            if (subject is null)
            {
                throw new ArgumentNullException(nameof(subject));
            }

            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            this.subject = subject;
            this.predicate = predicate;
        }

        private IEnumerable<object> Objects
        {
            get
            {
                return
                    from triple
                    in subject.Graph.GetTriplesWithSubjectPredicate(subject, predicate)
                    select DynamicHelper.ConvertNode(triple.Object, subject.BaseUri);
            }
        }

        public int Count => Objects.Count();

        public bool IsReadOnly => false;

        public void Add(object item) => subject.Add(predicate, item);

        public void Clear() => subject.Remove(predicate);

        public bool Contains(object item) => ((IDictionary<INode, object>)subject).Contains(new KeyValuePair<INode, object>(predicate, item));

        public void CopyTo(object[] array, int index) => Objects.ToArray().CopyTo(array, index);

        public IEnumerator<object> GetEnumerator() => Objects.GetEnumerator();

        public bool Remove(object item) => ((IDictionary<INode, object>)subject).Remove(new KeyValuePair<INode, object>(predicate, item));

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
