using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    }
}
