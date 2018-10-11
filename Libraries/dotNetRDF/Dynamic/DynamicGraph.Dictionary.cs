namespace VDS.RDF.Dynamic
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public partial class DynamicGraph
    {
        public ICollection<object> Values => this.Triples.SubjectNodes.UriNodes().Select(node => new DynamicNode(node, this.PredicateBaseUri)).ToArray();

        public int Count => (this as IEnumerable<KeyValuePair<INode, object>>).Count();

        public bool IsReadOnly => false;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<KeyValuePair<INode, object>>).GetEnumerator();
        }
    }
}
