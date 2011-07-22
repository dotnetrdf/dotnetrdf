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
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Test.Writing.Serialization
{
    [TestClass]
    public class GraphSerializationTests
    {
        private void TestGraphSerializationXml(IGraph g)
        {
            StringWriter writer = new StringWriter();
            XmlSerializer serializer = new XmlSerializer(typeof(Graph));

            serializer.Serialize(writer, g);
            Console.WriteLine("Serialized Form:");
            Console.WriteLine(writer.ToString());
            Console.WriteLine();

            Graph h = serializer.Deserialize(new StringReader(writer.ToString())) as Graph;
            Assert.AreEqual(g, h, "Graphs should be equal");
        }

        private void TestGraphSerializationXml(IEnumerable<IGraph> gs)
        {
            foreach (IGraph g in gs)
            {
                this.TestGraphSerializationXml(g);
            }
        }

        private void TestGraphSerializationBinary(IGraph g)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter serializer = new BinaryFormatter();

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

        private void TestGraphSerializationBinary(IEnumerable<IGraph> gs)
        {
            foreach (IGraph g in gs)
            {
                this.TestGraphSerializationBinary(g);
            }
        }

        private void TestGraphSerializationDataContract(IGraph g)
        {
            StringWriter writer = new StringWriter();
            DataContractSerializer serializer = new DataContractSerializer(typeof(Graph));

            serializer.WriteObject(new XmlTextWriter(writer), g);
            Console.WriteLine("Serialized Form:");
            Console.WriteLine(writer.ToString());
            Console.WriteLine();

            Graph h = serializer.ReadObject(XmlReader.Create(new StringReader(writer.ToString()))) as Graph;
            Assert.AreEqual(g, h, "Graphs should be equal");
        }

        private void TestGraphSerializationJson(IGraph g)
        {
            StringWriter writer = new StringWriter();
            JsonSerializer serializer = new JsonSerializer();
            serializer.TypeNameHandling = TypeNameHandling.All;

            serializer.Serialize(new JsonTextWriter(writer), g);
            Console.WriteLine("Serialized Form:");
            Console.WriteLine(writer.ToString());
            Console.WriteLine();

            Object h = serializer.Deserialize<Graph>(new JsonTextReader(new StringReader(writer.ToString())));// as Graph;
            Console.WriteLine(h.GetType().FullName);
            Assert.AreEqual(g, h, "Graphs should be equal");
        }

        [TestMethod]
        public void SerializationXmlGraph()
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(new PagingHandler(new GraphHandler(g), 50), "VDS.RDF.Configuration.configuration.ttl");

            this.TestGraphSerializationXml(g);
        }

        [TestMethod]
        public void SerializationXmlGraph2()
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");

            this.TestGraphSerializationXml(g);
        }

        [TestMethod]
        public void SerializationXmlGraph3()
        {
            Graph g = new Graph();
            UriLoader.Load(g, new Uri("http://dbpedia.org/resource/Ilkeston"));

            this.TestGraphSerializationXml(g);
        }

        [TestMethod]
        public void SerializationBinaryGraph()
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(new PagingHandler(new GraphHandler(g), 50), "VDS.RDF.Configuration.configuration.ttl");

            this.TestGraphSerializationBinary(g);
        }

        [TestMethod]
        public void SerializationBinaryGraph2()
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");

            this.TestGraphSerializationBinary(g);
        }

        [TestMethod]
        public void SerializationBinaryGraph3()
        {
            Graph g = new Graph();
            UriLoader.Load(g, new Uri("http://dbpedia.org/resource/Ilkeston"));

            this.TestGraphSerializationBinary(g);
        }

        [TestMethod]
        public void SerializationDataContractGraph()
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(new PagingHandler(new GraphHandler(g), 50), "VDS.RDF.Configuration.configuration.ttl");

            this.TestGraphSerializationDataContract(g);
        }

        [TestMethod]
        public void SerializationDataContractGraph2()
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");

            this.TestGraphSerializationDataContract(g);
        }

        [TestMethod]
        public void SerializationDataContractGraph3()
        {
            Graph g = new Graph();
            UriLoader.Load(g, new Uri("http://dbpedia.org/resource/Ilkeston"));

            this.TestGraphSerializationDataContract(g);
        }

        [TestMethod]
        public void SerializationJsonGraph()
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(new PagingHandler(new GraphHandler(g), 50), "VDS.RDF.Configuration.configuration.ttl");

            this.TestGraphSerializationJson(g);
        }

    }
}
