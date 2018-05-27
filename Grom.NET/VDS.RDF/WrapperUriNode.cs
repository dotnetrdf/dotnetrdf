namespace VDS.RDF
{
    using System;

    public class WrapperUriNode : WrapperNode, IUriNode
    {
        public WrapperUriNode(IUriNode node) : base(node) { }

        public Uri Uri => (this.node as IUriNode).Uri;
    }
}
