namespace Dynamic
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public partial class DynamicNode
    {
        public ICollection<object> Values =>
                // TODO: This should be a collection of CynamicObjectCollections (1 per predicate), no?
                this.Graph.GetTriplesWithSubject(this).Select(t => t.Object).Distinct().Select(o => DynamicHelper.ConvertToObject(o, this.BaseUri)).ToArray();

        public int Count =>
                // TODO: This should be the count of predicates, no?
                this.Graph.GetTriplesWithSubject(this).Count();

        public bool IsReadOnly => false;

        public void Clear() => this.Graph.Retract(this.Graph.GetTriplesWithSubject(this).ToArray());

        IEnumerator IEnumerable.GetEnumerator()
        {
            // TODO: Which one should this actually be?
            return this.GetEnumerator();
        }
    }
}
