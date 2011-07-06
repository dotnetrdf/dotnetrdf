using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
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
    public class GraphSerializationTests
    {
        [TestMethod]
        public void GraphXmlSerialization()
        {
            StringWriter writer = new StringWriter();
            XmlSerializer serializer = new XmlSerializer(typeof(Graph));

            Graph g = new Graph();
            EmbeddedResourceLoader.Load(new PagingHandler(new GraphHandler(g), 50), "VDS.RDF.Configuration.configuration.ttl");

            serializer.Serialize(writer, g);
            Console.WriteLine("Serialized Form:");
            Console.WriteLine(writer.ToString());
            Console.WriteLine();

            Graph h = serializer.Deserialize(new StringReader(writer.ToString())) as Graph;
            Assert.AreEqual(g, h, "Graphs should be equal");
        }

        [TestMethod]
        public void GraphBinarySerialization()
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter serializer = new BinaryFormatter();

            Graph g = new Graph();
            EmbeddedResourceLoader.Load(new PagingHandler(new GraphHandler(g), 50), "VDS.RDF.Configuration.configuration.ttl");

            serializer.Serialize(stream, g);
            Console.WriteLine("Serialized Form:");
            stream.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(stream);
            Console.WriteLine(reader.ReadToEnd());
            Console.WriteLine();

            stream.Seek(0, SeekOrigin.Begin);
            Graph h = serializer.Deserialize(stream) as Graph;
            reader.Close();
            Assert.AreEqual(g, h, "Graphs should be equal");

            stream.Dispose();
        }
    }
}
