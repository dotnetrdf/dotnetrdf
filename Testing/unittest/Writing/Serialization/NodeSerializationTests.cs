using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Test.Writing.Serialization
{
    [TestClass]
    public class NodeSerializationTests
    {
        private void TestNodeXmlSerialization(INode n, Type t, bool fullEquality)
        {
            Console.WriteLine("Input: " + n.ToString());

            StringWriter writer = new StringWriter();
            XmlSerializer serializer = new XmlSerializer(t);
            serializer.Serialize(writer, n);
            Console.WriteLine("Serialized Form:");
            Console.WriteLine(writer.ToString());

            INode m = serializer.Deserialize(new StringReader(writer.ToString())) as INode;
            Console.WriteLine("Deserialized Form: " + m.ToString());
            Console.WriteLine();

            if (fullEquality)
            {
                Assert.AreEqual(n, m, "Nodes should be equal");
            }
            else
            {
                Assert.AreEqual(n.ToString(), m.ToString(), "String forms should be equal");
            }
        }

        private void TestNodeXmlSerialization(IEnumerable<INode> nodes, Type t, bool fullEquality)
        {
            foreach (INode n in nodes)
            {
                this.TestNodeXmlSerialization(n, t, fullEquality);
            }
        }

        private void TestNodeBinarySerialization(INode n, bool fullEquality)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter serializer = new BinaryFormatter(null, new StreamingContext());
            serializer.Serialize(stream, n);

            stream.Seek(0, SeekOrigin.Begin);
            Console.WriteLine("Serialized Form:");
            StreamReader reader = new StreamReader(stream);
            Console.WriteLine(reader.ReadToEnd());

            stream.Seek(0, SeekOrigin.Begin);
            INode m = serializer.Deserialize(stream) as INode;

            reader.Close();

            if (fullEquality)
            {
                Assert.AreEqual(n, m, "Nodes should be equal");
            }
            else
            {
                Assert.AreEqual(n.ToString(), m.ToString(), "String forms should be equal");
            }

            stream.Dispose();
        }

        private void TestNodeBinarySerialization(IEnumerable<INode> nodes, bool fullEquality)
        {
            foreach (INode n in nodes)
            {
                this.TestNodeBinarySerialization(n, fullEquality);
            }
        }

        [TestMethod]
        public void NodeXmlSerializationBlankNodes()
        {
            Graph g = new Graph();
            INode b = g.CreateBlankNode();
            this.TestNodeXmlSerialization(b, typeof(BlankNode), false);
        }

        [TestMethod]
        public void NodeBinarySerializationBlankNodes()
        {
            Graph g = new Graph();
            INode b = g.CreateBlankNode();
            this.TestNodeBinarySerialization(b, false);
        }

        private IEnumerable<INode> GetLiteralNodes()
        {
            Graph g = new Graph();
            List<INode> nodes = new List<INode>()
            {
                g.CreateLiteralNode(String.Empty),
                g.CreateLiteralNode("simple literal"),
                g.CreateLiteralNode("literal with language", "en"),
                g.CreateLiteralNode("literal with different language", "fr"),
                (12345).ToLiteral(g),
                DateTime.Now.ToLiteral(g),
                (123.45).ToLiteral(g),
                (123.45m).ToLiteral(g)
            };
            return nodes;
        }

        [TestMethod]
        public void NodeXmlSerializationLiteralNodes()
        {
            this.TestNodeXmlSerialization(this.GetLiteralNodes(), typeof(LiteralNode), true);
        }

        [TestMethod]
        public void NodeBinarySerializationLiteralNodes()
        {
            this.TestNodeBinarySerialization(this.GetLiteralNodes(), true);
        }

        private IEnumerable<INode> GetUriNodes()
        {
            Graph g = new Graph();
            List<INode> nodes = new List<INode>()
            {
                g.CreateUriNode("rdf:type"),
                g.CreateUriNode("rdfs:label"),
                g.CreateUriNode("xsd:integer"),
                g.CreateUriNode(new Uri("http://example.org")),
                g.CreateUriNode(new Uri("mailto:example@example.org")),
                g.CreateUriNode(new Uri("ftp://ftp.example.org"))
            };
            return nodes;
        }

        [TestMethod]
        public void NodeXmlSerializationUriNodes()
        {
            this.TestNodeXmlSerialization(this.GetUriNodes(), typeof(UriNode), true);
        }

        [TestMethod]
        public void NodeBinarySerializationUriNodes()
        {
            this.TestNodeBinarySerialization(this.GetUriNodes(), true);
        }

        private IEnumerable<INode> GetGraphLiteralNodes()
        {
            Graph g = new Graph();
            Graph h = new Graph();
            EmbeddedResourceLoader.Load(new PagingHandler(new GraphHandler(h), 10), "VDS.RDF.Configuration.configuration.ttl");
            Graph i = new Graph();
            EmbeddedResourceLoader.Load(new PagingHandler(new GraphHandler(i), 5, 25), "VDS.RDF.Configuration.configuration.ttl");

            List<INode> nodes = new List<INode>()
            {
                g.CreateGraphLiteralNode(),
                g.CreateGraphLiteralNode(h),
                g.CreateGraphLiteralNode(i)

            };
            return nodes;
        }

        [TestMethod]
        public void NodeXmlSerializationGraphLiteralNodes()
        {
            this.TestNodeXmlSerialization(this.GetGraphLiteralNodes(), typeof(GraphLiteralNode), true);
        }

        [TestMethod]
        public void NodeBinarySerializationGraphLiteralNodes()
        {
            this.TestNodeBinarySerialization(this.GetGraphLiteralNodes(), true);
        }

        private IEnumerable<INode> GetVariableNodes()
        {
            Graph g = new Graph();
            List<INode> nodes = new List<INode>()
            {
                g.CreateVariableNode("a"),
                g.CreateVariableNode("b"),
                g.CreateVariableNode("c"),
                g.CreateVariableNode("variable"),
                g.CreateVariableNode("some123"),
                g.CreateVariableNode("this-that")
            };
            return nodes;
        }

        [TestMethod]
        public void NodeXmlSerializationVariableNodes()
        {
            this.TestNodeXmlSerialization(this.GetVariableNodes(), typeof(VariableNode), true);
        }

        [TestMethod]
        public void NodeBinarySerializationVariableNodes()
        {
            this.TestNodeBinarySerialization(this.GetVariableNodes(), true);
        }
    }
}
