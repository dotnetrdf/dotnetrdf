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
using VDS.RDF.Query;

namespace VDS.RDF.Test.Writing.Serialization
{
    [TestClass]
    public class ResultSerializationTests
    {
        private void TestXmlSerialization(SparqlResult r, bool fullEquality)
        {
            Console.WriteLine("Input: " + r.ToString());

            StringWriter writer = new StringWriter();
            XmlSerializer serializer = new XmlSerializer(typeof(SparqlResult));
            serializer.Serialize(writer, r);
            Console.WriteLine("Serialized Form:");
            Console.WriteLine(writer.ToString());

            SparqlResult s = serializer.Deserialize(new StringReader(writer.ToString())) as SparqlResult;
            Console.WriteLine("Deserialized Form: " + s.ToString());
            Console.WriteLine();

            if (fullEquality)
            {
                Assert.AreEqual(r, s, "Results should be equal");
            }
            else
            {
                Assert.AreEqual(r.ToString(), s.ToString(), "String forms should be equal");
            }
        }

        private void TestXmlSerialization(SparqlResultSet results, bool fullEquality)
        {
            foreach (SparqlResult r in results)
            {
                this.TestXmlSerialization(r, fullEquality);
            }
        }

        private void TestBinarySerialization(SparqlResult r, bool fullEquality)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter serializer = new BinaryFormatter(null, new StreamingContext());
            serializer.Serialize(stream, r);

            stream.Seek(0, SeekOrigin.Begin);
            Console.WriteLine("Serialized Form:");
            StreamReader reader = new StreamReader(stream);
            Console.WriteLine(reader.ReadToEnd());

            stream.Seek(0, SeekOrigin.Begin);
            SparqlResult s = serializer.Deserialize(stream) as SparqlResult;

            reader.Close();

            if (fullEquality)
            {
                Assert.AreEqual(r, s, "Results should be equal");
            }
            else
            {
                Assert.AreEqual(r.ToString(), s.ToString(), "String forms should be equal");
            }

            stream.Dispose();
        }

        private void TestBinarySerialization(SparqlResultSet results, bool fullEquality)
        {
            foreach (SparqlResult r in results)
            {
                this.TestBinarySerialization(r, fullEquality);
            }
        }

        private void TestBinarySerialization(SparqlResultSet results)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter serializer = new BinaryFormatter(null, new StreamingContext());
            serializer.Serialize(stream, results);

            stream.Seek(0, SeekOrigin.Begin);
            Console.WriteLine("Serialized Form:");
            StreamReader reader = new StreamReader(stream);
            Console.WriteLine(reader.ReadToEnd());

            stream.Seek(0, SeekOrigin.Begin);
            SparqlResultSet results2 = serializer.Deserialize(stream) as SparqlResultSet;

            reader.Close();

            Assert.AreEqual(results, results2, "Expected Result Sets to be equal");
        }

        private SparqlResultSet GetResults(String query)
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            return g.ExecuteQuery(query) as SparqlResultSet;
        }

        private SparqlResultSet GetResults()
        {
            return this.GetResults("SELECT * WHERE { ?s a ?type } LIMIT 20");
        }

        [TestMethod]
        public void SerializationBinarySparqlResult()
        {
            this.TestBinarySerialization(this.GetResults(), true);
        }

        [TestMethod]
        public void SerializationBinarySparqlResultSet()
        {
            this.TestBinarySerialization(this.GetResults());
        }

        [TestMethod]
        public void SerializationXmlSparqlResult()
        {
            this.TestXmlSerialization(this.GetResults(), true);
        }
    }
}
