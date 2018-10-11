namespace Dynamic
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Xml;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using VDS.RDF;
    using VDS.RDF.Writing;
    using VDS.RDF.Writing.Formatting;

    [TestClass]
    public class WrapperNodeTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Requires_node()
        {
            new MockWrapperNode(null);
        }

        [TestMethod]
        public void Delegates_Equals_object()
        {
            var node = new NodeFactory().CreateBlankNode();
            var nodeObject = node as object;
            var wrapper = new MockWrapperNode(node);

            var expected = node.Equals(nodeObject);
            var actual = wrapper.Equals(nodeObject);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Delegates_GetHashCode()
        {
            var node = new NodeFactory().CreateBlankNode();
            var wrapper = new MockWrapperNode(node);

            var expected = node.GetHashCode();
            var actual = wrapper.GetHashCode();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Delegates_to_string()
        {
            var node = new NodeFactory().CreateBlankNode();
            var wrapper = new MockWrapperNode(node);

            var expected = node.ToString();
            var actual = wrapper.ToString();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Delegates_NodeType()
        {
            var node = new NodeFactory().CreateBlankNode();
            var wrapper = new MockWrapperNode(node);

            var expected = node.NodeType;
            var actual = wrapper.NodeType;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Delegates_Graph()
        {

            var node = new Graph().CreateBlankNode();
            var wrapper = new MockWrapperNode(node);

            var expected = node.Graph;
            var actual = wrapper.Graph;

            Assert.AreSame(expected, actual);
        }

        [TestMethod]
        public void Delegates_GraphUri()
        {
            var node = new NodeFactory().CreateBlankNode();
            var wrapper = new MockWrapperNode(node);

            wrapper.GraphUri = new Uri("http://example.com/");

            var expected = node.GraphUri;
            var actual = wrapper.GraphUri;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Delegates_CompareTo_node()
        {
            var node = new NodeFactory().CreateBlankNode() as INode;
            var wrapper = new MockWrapperNode(node);

            var expected = node.CompareTo(node);
            var actual = wrapper.CompareTo(node);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Delegates__CompareTo_blank()
        {
            var node = new NodeFactory().CreateBlankNode();
            var wrapper = new MockWrapperNode(node);

            var expected = node.CompareTo(node);
            var actual = wrapper.CompareTo(node);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Delegates__CompareTo_graphliteral()
        {
            var node = new NodeFactory().CreateGraphLiteralNode();
            var wrapper = new MockWrapperNode(node);

            var expected = node.CompareTo(node);
            var actual = wrapper.CompareTo(node);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Delegates__CompareTo_literal()
        {
            var node = new NodeFactory().CreateLiteralNode(string.Empty);
            var wrapper = new MockWrapperNode(node);

            var expected = node.CompareTo(node);
            var actual = wrapper.CompareTo(node);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Delegates__CompareTo_uri()
        {
            var node = new NodeFactory().CreateUriNode(new Uri("http://example.com/"));
            var wrapper = new MockWrapperNode(node);

            var expected = node.CompareTo(node);
            var actual = wrapper.CompareTo(node);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Delegates__CompareTo_variable()
        {
            var node = new NodeFactory().CreateVariableNode(string.Empty);
            var wrapper = new MockWrapperNode(node);

            var expected = node.CompareTo(node);
            var actual = wrapper.CompareTo(node);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Delegates_Equal_node()
        {
            var node = new NodeFactory().CreateBlankNode() as INode;
            var wrapper = new MockWrapperNode(node);

            var expected = node.Equals(node);
            var actual = wrapper.Equals(node);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Delegates_Equal_blank()
        {
            var node = new NodeFactory().CreateBlankNode();
            var wrapper = new MockWrapperNode(node);

            var expected = node.Equals(node);
            var actual = wrapper.Equals(node);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Delegates_Equal_graphliteral()
        {
            var node = new NodeFactory().CreateGraphLiteralNode();
            var wrapper = new MockWrapperNode(node);

            var expected = node.Equals(node);
            var actual = wrapper.Equals(node);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Delegates_Equal_literal()
        {
            var node = new NodeFactory().CreateLiteralNode(string.Empty);
            var wrapper = new MockWrapperNode(node);

            var expected = node.Equals(node);
            var actual = wrapper.Equals(node);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Delegates_Equal_uri()
        {
            var node = new NodeFactory().CreateUriNode(new Uri("http://example.com/"));
            var wrapper = new MockWrapperNode(node);

            var expected = node.Equals(node);
            var actual = wrapper.Equals(node);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Delegates_Equal_variable()
        {
            var node = new NodeFactory().CreateVariableNode(string.Empty);
            var wrapper = new MockWrapperNode(node);

            var expected = node.Equals(node);
            var actual = wrapper.Equals(node);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void Doesnt_implement_GetObjectData()
        {
            var node = new NodeFactory().CreateBlankNode();
            var wrapper = new MockWrapperNode(node);
            var serializer = new BinaryFormatter(null, new StreamingContext());

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, wrapper);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void Doesnt_implement_GetSchema()
        {
            var node = new NodeFactory().CreateBlankNode();
            var wrapper = new MockWrapperNode(node);

            wrapper.GetSchema();
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void Doesnt_implement_ReadXml()
        {
            var node = new NodeFactory().CreateBlankNode();
            var wrapper = new MockWrapperNode(node);

            wrapper.ReadXml(XmlReader.Create(Stream.Null));
        }

        [TestMethod]
        public void Delegates_ToString_formatter()
        {
            var node = new NodeFactory().CreateBlankNode();
            var wrapper = new MockWrapperNode(node);

            var expected = node.ToString(new CsvFormatter());
            var actual = wrapper.ToString(new CsvFormatter());

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Delegates_ToString_formatter_segment()
        {
            var node = new NodeFactory().CreateBlankNode();
            var wrapper = new MockWrapperNode(node);
            var formatter = new CsvFormatter();

            var expected = node.ToString(formatter, TripleSegment.Subject);
            var actual = wrapper.ToString(formatter, TripleSegment.Subject);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void Doesnt_implement_WriteXml()
        {
            var node = new NodeFactory().CreateBlankNode();
            var wrapper = new MockWrapperNode(node);

            wrapper.WriteXml(XmlWriter.Create(Stream.Null));
        }

        [Serializable]
        public class MockWrapperNode : WrapperNode
        {
            public MockWrapperNode(INode node) : base(node) { }
        }
    }
}