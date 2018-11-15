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
                var values =
                    from node in UriNodes
                    select new DynamicNode(
                        node,
                        this.PredicateBaseUri);

                return values.ToArray();
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
            return (this as IEnumerable<KeyValuePair<INode, object>>).GetEnumerator();
        }
    }
}
