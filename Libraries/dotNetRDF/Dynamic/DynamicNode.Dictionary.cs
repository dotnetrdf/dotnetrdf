namespace VDS.RDF.Dynamic
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public partial class DynamicNode
    {
        public ICollection<object> Values
        {
            get
            {
                return Graph
                    .GetTriplesWithSubject(this)
                    .Select(t => t.Predicate)
                    .Distinct()
                    .Select(p => new DynamicObjectCollection(this, p))
                    .ToArray();
            }
        }

        public int Count
        {
            get
            {
                return Graph
                    .GetTriplesWithSubject(this)
                    .Select(t => t.Predicate)
                    .Distinct()
                    .Count();
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
            new DynamicGraph(Graph).Remove(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return NodeDictionary.GetEnumerator();
        }
    }
}
