using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace VDS.RDF.Test.Writing.Serialization
{
    [TestClass]
    public class TripleSerializationTests
    {
        [TestMethod]
        public void SerializationXmlTriple()
        {
            StringWriter writer = new StringWriter();
            XmlSerializer serializer = new XmlSerializer(typeof(Triple));

            Graph g = new Graph();
            INode s = g.CreateBlankNode();
            INode p = g.CreateUriNode(new Uri("http://example.org/predicate"));
            INode o = g.CreateLiteralNode("object");
            Triple t = new Triple(s, p, o);

            Console.WriteLine("Input: " + t.ToString());

            serializer.Serialize(writer, t);
            Console.WriteLine("Serialized Form:");
            Console.WriteLine(writer.ToString());

            Triple t2 = serializer.Deserialize(new StringReader(writer.ToString())) as Triple;
            Assert.IsNotNull(t2, "Triple should not be null");
            Console.WriteLine("Deserialized Form: " + t2.ToString());
            Assert.AreEqual(t, t2, "Triples should be equal");
        }

        [TestMethod]
        public void SerializationBinaryTriple()
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter serializer = new BinaryFormatter();

            Graph g = new Graph();
            INode s = g.CreateBlankNode();
            INode p = g.CreateUriNode(new Uri("http://example.org/predicate"));
            INode o = g.CreateLiteralNode("object");
            Triple t = new Triple(s, p, o);

            Console.WriteLine("Input: " + t.ToString());

            serializer.Serialize(stream, t);
            Console.WriteLine("Serialized Form:");
            stream.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(stream);
            Console.WriteLine(reader.ReadToEnd());

            stream.Seek(0, SeekOrigin.Begin);
            Triple t2 = serializer.Deserialize(stream) as Triple;

            reader.Close();

            Assert.IsNotNull(t2, "Triple should not be null");
            Console.WriteLine("Deserialized Form: " + t2.ToString());
            Assert.AreEqual(t, t2, "Triples should be equal");

            stream.Dispose();
        }
    }
}
