namespace Dynamic
{
    using System;
    using VDS.RDF;

    public class DynamicUriNode : DynamicNode, IUriNode
    {
        public DynamicUriNode(IUriNode node, Uri baseUri = null, bool collapseSingularArrays = false) : base(node, baseUri, collapseSingularArrays) { }

        public Uri Uri => (this.node as IUriNode).Uri;
    }
}
