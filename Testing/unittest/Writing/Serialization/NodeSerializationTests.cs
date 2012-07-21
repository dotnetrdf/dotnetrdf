/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Test.Writing.Serialization
{
    [TestClass]
    public class NodeSerializationTests
    {
        #region Methods that perform the actual test logic

        private void TestSerializationXml(INode n, Type t, bool fullEquality)
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

        private void TestSerializationXml(IEnumerable<INode> nodes, Type t, bool fullEquality)
        {
            foreach (INode n in nodes)
            {
                this.TestSerializationXml(n, t, fullEquality);
            }
        }

        private void TestSerializationBinary(INode n, bool fullEquality)
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

        private void TestSerializationBinary(IEnumerable<INode> nodes, bool fullEquality)
        {
            foreach (INode n in nodes)
            {
                this.TestSerializationBinary(n, fullEquality);
            }
        }

        private void TestSerializationDataContract(INode n, Type t, bool fullEquality)
        {
            Console.WriteLine("Input: " + n.ToString());

            StringWriter writer = new StringWriter();
            DataContractSerializer serializer = new DataContractSerializer(t);
            serializer.WriteObject(new XmlTextWriter(writer), n);
            Console.WriteLine("Serialized Form:");
            Console.WriteLine(writer.ToString());

            INode m = serializer.ReadObject(XmlTextReader.Create(new StringReader(writer.ToString()))) as INode;
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

        private void TestSerializationDataContract(IEnumerable<INode> nodes, Type t, bool fullEquality)
        {
            foreach (INode n in nodes)
            {
                this.TestSerializationDataContract(n, t, fullEquality);
            }
        }

        #endregion

        #region Unit Tests for Blank Nodes

        [TestMethod]
        public void SerializationXmlBlankNodes()
        {
            Graph g = new Graph();
            INode b = g.CreateBlankNode();
            this.TestSerializationXml(b, typeof(BlankNode), false);
        }

        [TestMethod]
        public void SerializationBinaryBlankNodes()
        {
            Graph g = new Graph();
            INode b = g.CreateBlankNode();
            this.TestSerializationBinary(b, false);
        }

        [TestMethod]
        public void SerializationDataContractBlankNodes()
        {
            Graph g = new Graph();
            INode b = g.CreateBlankNode();
            this.TestSerializationDataContract(b, typeof(BlankNode), false);
        }

        #endregion

        #region Unit Tests for Literal Nodes

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
        public void SerializationXmlLiteralNodes()
        {
            this.TestSerializationXml(this.GetLiteralNodes(), typeof(LiteralNode), true);
        }

        [TestMethod]
        public void SerializationBinaryLiteralNodes()
        {
            this.TestSerializationBinary(this.GetLiteralNodes(), true);
        }

        [TestMethod]
        public void SerializationDataContractLiteralNodes()
        {
            this.TestSerializationDataContract(this.GetLiteralNodes(), typeof(LiteralNode), true);
        }

        #endregion

        #region Unit Tests for URI Nodes

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
        public void SerializationXmlUriNodes()
        {
            this.TestSerializationXml(this.GetUriNodes(), typeof(UriNode), true);
        }

        [TestMethod]
        public void SerializationBinaryUriNodes()
        {
            this.TestSerializationBinary(this.GetUriNodes(), true);
        }

        [TestMethod]
        public void SerializationDataContractUriNodes()
        {
            this.TestSerializationDataContract(this.GetUriNodes(), typeof(UriNode), true);
        }

        #endregion

        #region Unit Tests for Graph Literals

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
        public void SerializationXmlGraphLiteralNodes()
        {
            this.TestSerializationXml(this.GetGraphLiteralNodes(), typeof(GraphLiteralNode), true);
        }

        [TestMethod]
        public void SerializationBinaryGraphLiteralNodes()
        {
            this.TestSerializationBinary(this.GetGraphLiteralNodes(), true);
        }

        [TestMethod]
        public void SerializationDataContractGraphLiteralNodes()
        {
            this.TestSerializationDataContract(this.GetGraphLiteralNodes(), typeof(GraphLiteralNode), true);
        }

        #endregion

        #region Unit Tests for Variables

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
        public void SerializationXmlVariableNodes()
        {
            this.TestSerializationXml(this.GetVariableNodes(), typeof(VariableNode), true);
        }

        [TestMethod]
        public void SerializationBinaryVariableNodes()
        {
            this.TestSerializationBinary(this.GetVariableNodes(), true);
        }

        [TestMethod]
        public void SerializationDataContractVariableNodes()
        {
            this.TestSerializationDataContract(this.GetVariableNodes(), typeof(VariableNode), true);
        }

        #endregion
    }
}
