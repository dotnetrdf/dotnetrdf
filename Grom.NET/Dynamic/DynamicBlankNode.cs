namespace Dynamic
{
    using System;
    using VDS.RDF;

    public class DynamicBlankNode : DynamicNode, IBlankNode
    {
        public DynamicBlankNode(IBlankNode node, Uri baseUri = null, bool collapseSingularArrays = false) : base(node, baseUri, collapseSingularArrays) { }

        public string InternalID => (this.node as IBlankNode).InternalID;
    }
}
