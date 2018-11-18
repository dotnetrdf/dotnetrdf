namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;

    public class DynamicObjectCollection : ICollection<object>, IDynamicMetaObjectProvider
    {
        protected readonly DynamicNode subject;
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

        protected IEnumerable<object> Objects
        {
            get
            {
                return
                    from triple
                    in subject.Graph.GetTriplesWithSubjectPredicate(subject, predicate)
                    select DynamicHelper.ConvertNode(triple.Object, subject.BaseUri);
            }
        }

        public int Count
        {
            get
            {
                return Objects.Count();
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public void Add(object @object)
        {
            subject.Add(predicate, @object);
        }

        public void Clear()
        {
            subject.Remove(predicate);
        }

        public bool Contains(object @object)
        {
            return Objects.Contains(@object);
        }

        public void CopyTo(object[] array, int index)
        {
            Objects.ToArray().CopyTo(array, index);
        }

        public IEnumerator<object> GetEnumerator()
        {
            return Objects.GetEnumerator();
        }

        public bool Remove(object @object)
        {
            return subject.Remove(predicate, @object);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
        {
            return new EnumerableMetaObject(parameter, this);
        }
    }
}
