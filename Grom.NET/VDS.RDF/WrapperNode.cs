namespace VDS.RDF
{
    using System;
    using System.Runtime.Serialization;
    using System.Xml;
    using System.Xml.Schema;
    using VDS.RDF.Writing;
    using VDS.RDF.Writing.Formatting;

    public class WrapperNode : INode
    {
        protected readonly INode node;

        public WrapperNode(INode node) => this.node = node ?? throw new ArgumentNullException(nameof(node));

        public override bool Equals(object obj)
        {
            return this.node.Equals(obj);
        }

        public override int GetHashCode() => this.node.GetHashCode();

        public override string ToString() => this.node.ToString();

        public NodeType NodeType => this.node.NodeType;

        public IGraph Graph => this.node.Graph;

        public Uri GraphUri { get => this.node.GraphUri; set => this.node.GraphUri = value; }

        public int CompareTo(INode other) => this.node.CompareTo(other);

        public int CompareTo(IBlankNode other) => this.node.CompareTo(other);

        public int CompareTo(IGraphLiteralNode other) => this.node.CompareTo(other);

        public int CompareTo(ILiteralNode other) => this.node.CompareTo(other);

        public int CompareTo(IUriNode other) => this.node.CompareTo(other);

        public int CompareTo(IVariableNode other) => this.node.CompareTo(other);

        public bool Equals(INode other) => this.node.Equals(other);

        public bool Equals(IBlankNode other) => this.node.Equals(other);

        public bool Equals(IGraphLiteralNode other) => this.node.Equals(other);

        public bool Equals(ILiteralNode other) => this.node.Equals(other);

        public bool Equals(IUriNode other) => this.node.Equals(other);

        public bool Equals(IVariableNode other) => this.node.Equals(other);

        public void GetObjectData(SerializationInfo info, StreamingContext context) => this.node.GetObjectData(info, context);

        public XmlSchema GetSchema() => this.node.GetSchema();

        public void ReadXml(XmlReader reader) => this.node.ReadXml(reader);

        public string ToString(INodeFormatter formatter) => this.node.ToString(formatter);

        public string ToString(INodeFormatter formatter, TripleSegment segment) => this.node.ToString(formatter, segment);

        public void WriteXml(XmlWriter writer) => this.node.WriteXml(writer);
    }
}
