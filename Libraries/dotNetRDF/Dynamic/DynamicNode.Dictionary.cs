namespace VDS.RDF.Dynamic
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public partial class DynamicNode
    {
        // TODO: This should be a collection of CynamicObjectCollections (1 per predicate), no?
        public ICollection<object> Values => this.Graph.GetTriplesWithSubject(this).Select(t => t.Object).Distinct().Select(o => DynamicHelper.ConvertToObject(o, this.BaseUri)).ToArray();

        public int Count => this.Graph.GetTriplesWithSubject(this).Select(t => t.Predicate).Distinct().Count();

        public bool IsReadOnly => false;

        public void Clear()
        {
            new DynamicGraph(this.Graph).Remove(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            // TODO: Which one should this actually be?
            return this.GetEnumerator();
        }
    }
}
