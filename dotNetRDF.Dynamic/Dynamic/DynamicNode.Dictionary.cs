namespace Dynamic
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    partial class DynamicNode
    {
        public ICollection<object> Values
        {
            get
            {
                // TODO: This should be a collection of CynamicObjectCollections (1 per predicate), no?
                return this.Graph.GetTriplesWithSubject(this).Select(t => t.Object).Distinct().Select(o => DynamicHelper.ConvertToObject(o, this.BaseUri)).ToArray();
            }
        }

        public int Count
        {
            get
            {
                // TODO: This should be the count of predicates, no?
                return this.Graph.GetTriplesWithSubject(this).Count();
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public void Clear()
        {
            this.Graph.Retract(this.Graph.GetTriplesWithSubject(this).ToArray());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            // TODO: Which one should this actually be?
            return this.GetEnumerator();
        }
    }
}
