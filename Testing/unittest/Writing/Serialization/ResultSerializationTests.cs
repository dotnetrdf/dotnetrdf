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

        private void TestXmlSerialization(SparqlResultSet results)
        {
            StringWriter writer = new StringWriter();
            XmlSerializer serializer = new XmlSerializer(typeof(SparqlResultSet));

            serializer.Serialize(writer, results);
            Console.WriteLine("Serialized Form:");
            Console.WriteLine(writer.ToString());
            Console.WriteLine();

            SparqlResultSet results2 = serializer.Deserialize(new StringReader(writer.ToString())) as SparqlResultSet;

            Assert.AreEqual(results, results2, "Expected Result Sets to be equal");
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

        private void TestDataContractSerialization(SparqlResult r, bool fullEquality)
        {
            Console.WriteLine("Input: " + r.ToString());

            StringWriter writer = new StringWriter();
            DataContractSerializer serializer = new DataContractSerializer(typeof(SparqlResult));
            serializer.WriteObject(new XmlTextWriter(writer), r);
            Console.WriteLine("Serialized Form:");
            Console.WriteLine(writer.ToString());

            SparqlResult s = serializer.ReadObject(XmlTextReader.Create(new StringReader(writer.ToString()))) as SparqlResult;
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

        private void TestDataContractSerialization(SparqlResultSet results, bool fullEquality)
        {
            foreach (SparqlResult r in results)
            {
                this.TestDataContractSerialization(r, fullEquality);
            }
        }

        private void TestDataContractSerialization(SparqlResultSet results)
        {
            StringWriter writer = new StringWriter();
            DataContractSerializer serializer = new DataContractSerializer(typeof(SparqlResultSet));

            serializer.WriteObject(new XmlTextWriter(writer), results);
            Console.WriteLine("Serialized Form:");
            Console.WriteLine(writer.ToString());
            Console.WriteLine();

            SparqlResultSet results2 = serializer.ReadObject(XmlTextReader.Create(new StringReader(writer.ToString()))) as SparqlResultSet;

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

        [TestMethod]
        public void SerializationXmlSparqlResultSet()
        {
            this.TestXmlSerialization(this.GetResults());
        }

        [TestMethod]
        public void SerializationDataContractSparqlResult()
        {
            this.TestDataContractSerialization(this.GetResults(), true);
        }

        [TestMethod]
        public void SerializationDataContractSparqlResultSet()
        {
            this.TestDataContractSerialization(this.GetResults());
        }
    }
}
