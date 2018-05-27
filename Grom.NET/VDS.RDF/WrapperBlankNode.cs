namespace VDS.RDF
{
    public class WrapperBlankNode : WrapperNode, IBlankNode
    {
        public WrapperBlankNode(IBlankNode node) : base(node) { }

        public string InternalID => (this.node as IBlankNode).InternalID;
    }
}
