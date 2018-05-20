namespace VDS.RDF
{
    public class WrapperBlankNode : WrapperNode, IBlankNode
    {
        public WrapperBlankNode(IBlankNode node) : base(node) { }

        public string InternalID => (this.node as IBlankNode).InternalID;

        public override bool Equals(object obj) => base.Equals(obj as IBlankNode);

        public override int GetHashCode() => base.GetHashCode();
    }
}
