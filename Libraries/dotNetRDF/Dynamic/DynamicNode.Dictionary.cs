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
                return NodePairs.Values;
            }
        }

        public int Count
        {
            get
            {
                return PredicateNodes.Count();
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
            return this.Cast<KeyValuePair<INode, object>>().GetEnumerator();
        }
    }
}
