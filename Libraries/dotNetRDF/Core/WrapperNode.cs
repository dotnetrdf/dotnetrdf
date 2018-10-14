namespace VDS.RDF
{
    using System;
    using VDS.RDF.Writing;
    using VDS.RDF.Writing.Formatting;

    public abstract partial class WrapperNode : INode
    {
        protected INode Node { get; private set; }

        protected WrapperNode(INode node)
        {
            this.Node = node ?? throw new ArgumentNullException(nameof(node));
        }

        public override bool Equals(object obj)
        {
            return this.Node.Equals(obj);
        }

        public override int GetHashCode()
        {
            return this.Node.GetHashCode();
        }

        public override string ToString()
        {
            return this.Node.ToString();
        }

        public NodeType NodeType => this.Node.NodeType;

        public IGraph Graph => this.Node.Graph;

        public Uri GraphUri { get => this.Node.GraphUri; set => this.Node.GraphUri = value; }

        public int CompareTo(INode other)
        {
            return this.Node.CompareTo(other);
        }

        public int CompareTo(IBlankNode other)
        {
            return this.Node.CompareTo(other);
        }

        public int CompareTo(IGraphLiteralNode other)
        {
            return this.Node.CompareTo(other);
        }

        public int CompareTo(ILiteralNode other)
        {
            return this.Node.CompareTo(other);
        }

        public int CompareTo(IUriNode other)
        {
            return this.Node.CompareTo(other);
        }

        public int CompareTo(IVariableNode other)
        {
            return this.Node.CompareTo(other);
        }

        public bool Equals(INode other)
        {
            return this.Node.Equals(other);
        }

        public bool Equals(IBlankNode other)
        {
            return this.Node.Equals(other);
        }

        public bool Equals(IGraphLiteralNode other)
        {
            return this.Node.Equals(other);
        }

        public bool Equals(ILiteralNode other)
        {
            return this.Node.Equals(other);
        }

        public bool Equals(IUriNode other)
        {
            return this.Node.Equals(other);
        }

        public bool Equals(IVariableNode other)
        {
            return this.Node.Equals(other);
        }

        public string ToString(INodeFormatter formatter)
        {
            return this.Node.ToString(formatter);
        }

        public string ToString(INodeFormatter formatter, TripleSegment segment)
        {
            return this.Node.ToString(formatter, segment);
        }

    }
}
