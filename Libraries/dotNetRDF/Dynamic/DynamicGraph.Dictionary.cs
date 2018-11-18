namespace VDS.RDF.Dynamic
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public partial class DynamicGraph
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
                return UriNodes.Count();
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Cast<KeyValuePair<INode, object>>().GetEnumerator();
        }
    }
}
