namespace VDS.RDF
{
    using System;
    using VDS.RDF.Writing;
    using VDS.RDF.Writing.Formatting;

    public abstract partial class WrapperNode : INode
    {
        protected INode Node { get; private set; }

        protected WrapperNode(INode node) => Node = node ?? throw new ArgumentNullException(nameof(node));

        public override bool Equals(object obj) => Node.Equals(obj);

        public override int GetHashCode() => Node.GetHashCode();

        public override string ToString() => Node.ToString();

        public NodeType NodeType => Node.NodeType;

        public IGraph Graph => Node.Graph;

        public Uri GraphUri { get => Node.GraphUri; set => Node.GraphUri = value; }

        public int CompareTo(INode other) => Node.CompareTo(other);

        public int CompareTo(IBlankNode other) => Node.CompareTo(other);

        public int CompareTo(IGraphLiteralNode other) => Node.CompareTo(other);

        public int CompareTo(ILiteralNode other) => Node.CompareTo(other);

        public int CompareTo(IUriNode other) => Node.CompareTo(other);

        public int CompareTo(IVariableNode other) => Node.CompareTo(other);

        public bool Equals(INode other) => Node.Equals(other);

        public bool Equals(IBlankNode other) => Node.Equals(other);

        public bool Equals(IGraphLiteralNode other) => Node.Equals(other);

        public bool Equals(ILiteralNode other) => Node.Equals(other);

        public bool Equals(IUriNode other) => Node.Equals(other);

        public bool Equals(IVariableNode other) => Node.Equals(other);

        public string ToString(INodeFormatter formatter) => Node.ToString(formatter);

        public string ToString(INodeFormatter formatter, TripleSegment segment) => Node.ToString(formatter, segment);
    }
}
