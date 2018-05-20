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

        public override int GetHashCode() => this.node.GetHashCode();

        public override string ToString() => this.node.ToString();

        public NodeType NodeType => node.NodeType;

        public IGraph Graph => node.Graph;

        public Uri GraphUri { get => node.GraphUri; set => node.GraphUri = value; }

        public int CompareTo(INode other) => node.CompareTo(other);

        public int CompareTo(IBlankNode other) => node.CompareTo(other);

        public int CompareTo(IGraphLiteralNode other) => node.CompareTo(other);

        public int CompareTo(ILiteralNode other) => node.CompareTo(other);

        public int CompareTo(IUriNode other) => node.CompareTo(other);

        public int CompareTo(IVariableNode other) => node.CompareTo(other);

        public bool Equals(INode other) => node.Equals(other);

        public bool Equals(IBlankNode other) => node.Equals(other);

        public bool Equals(IGraphLiteralNode other) => node.Equals(other);

        public bool Equals(ILiteralNode other) => node.Equals(other);

        public bool Equals(IUriNode other) => node.Equals(other);

        public bool Equals(IVariableNode other) => node.Equals(other);

        public void GetObjectData(SerializationInfo info, StreamingContext context) => node.GetObjectData(info, context);

        public XmlSchema GetSchema() => node.GetSchema();

        public void ReadXml(XmlReader reader) => node.ReadXml(reader);

        public string ToString(INodeFormatter formatter) => node.ToString(formatter);

        public string ToString(INodeFormatter formatter, TripleSegment segment) => node.ToString(formatter, segment);

        public void WriteXml(XmlWriter writer) => node.WriteXml(writer);
    }
}
