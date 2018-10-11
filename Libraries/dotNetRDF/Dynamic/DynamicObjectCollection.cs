namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal class DynamicObjectCollection : ICollection<object>
    {
        private readonly DynamicNode subject;
        private readonly INode predicate;

        internal DynamicObjectCollection(DynamicNode subject, INode predicate)
        {
            this.subject = subject;
            this.predicate = predicate;
        }

        public int Count => this.Get().Count();

        public bool IsReadOnly => false;

        public void Add(object item)
        {
            this.subject.Add(this.predicate, item);
        }

        public void Clear()
        {
            this.subject.Remove(this.predicate);
        }

        public bool Contains(object item)
        {
            return this.subject.Contains(this.predicate, item);
        }

        public void CopyTo(object[] array, int index)
        {
            this.Get().ToArray().CopyTo(array, index);
        }

        public IEnumerator<object> GetEnumerator()
        {
            return this.Get().GetEnumerator();
        }

        public bool Remove(object item)
        {
            return this.subject.Remove(new KeyValuePair<INode, object>(this.predicate, item));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private IEnumerable<object> Get()
        {
            if (this.subject.Graph is null)
            {
                throw new InvalidOperationException("graph missing");
            }

            return
                from triple
                in this.subject.Graph.GetTriplesWithSubjectPredicate(this.subject, this.predicate)
                select DynamicHelper.ConvertToObject(triple.Object, this.subject.BaseUri);
        }
    }
}
