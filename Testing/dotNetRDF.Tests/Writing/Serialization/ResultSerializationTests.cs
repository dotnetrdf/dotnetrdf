/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
using Xunit;
using VDS.RDF.Query;

namespace VDS.RDF.Writing.Serialization
{
    public class ResultSerializationTests
    {
        private void TestXmlSerialization(SparqlResult r, bool fullEquality)
        {
            Console.WriteLine("Input: " + r.ToString());

            System.IO.StringWriter writer = new System.IO.StringWriter();
            XmlSerializer serializer = new XmlSerializer(typeof(SparqlResult));
            serializer.Serialize(writer, r);
            Console.WriteLine("Serialized Form:");
            Console.WriteLine(writer.ToString());

            SparqlResult s = serializer.Deserialize(new StringReader(writer.ToString())) as SparqlResult;
            Console.WriteLine("Deserialized Form: " + s.ToString());
            Console.WriteLine();

            if (fullEquality)
            {
                Assert.Equal(r, s);
            }
            else
            {
                Assert.Equal(r.ToString(), s.ToString());
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
            System.IO.StringWriter writer = new System.IO.StringWriter();
            XmlSerializer serializer = new XmlSerializer(typeof(SparqlResultSet));

            serializer.Serialize(writer, results);
            Console.WriteLine("Serialized Form:");
            Console.WriteLine(writer.ToString());
            Console.WriteLine();

            SparqlResultSet results2 = serializer.Deserialize(new StringReader(writer.ToString())) as SparqlResultSet;

            Assert.Equal(results, results2);
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
                Assert.Equal(r, s);
            }
            else
            {
                Assert.Equal(r.ToString(), s.ToString());
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

            Assert.Equal(results, results2);
        }

        private void TestDataContractSerialization(SparqlResult r, bool fullEquality)
        {
            Console.WriteLine("Input: " + r.ToString());

            System.IO.StringWriter writer = new System.IO.StringWriter();
            DataContractSerializer serializer = new DataContractSerializer(typeof(SparqlResult));
            serializer.WriteObject(new XmlTextWriter(writer), r);
            Console.WriteLine("Serialized Form:");
            Console.WriteLine(writer.ToString());

            SparqlResult s = serializer.ReadObject(XmlTextReader.Create(new StringReader(writer.ToString()))) as SparqlResult;
            Console.WriteLine("Deserialized Form: " + s.ToString());
            Console.WriteLine();

            if (fullEquality)
            {
                Assert.Equal(r, s);
            }
            else
            {
                Assert.Equal(r.ToString(), s.ToString());
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
            System.IO.StringWriter writer = new System.IO.StringWriter();
            DataContractSerializer serializer = new DataContractSerializer(typeof(SparqlResultSet));

            serializer.WriteObject(new XmlTextWriter(writer), results);
            Console.WriteLine("Serialized Form:");
            Console.WriteLine(writer.ToString());
            Console.WriteLine();

            SparqlResultSet results2 = serializer.ReadObject(XmlTextReader.Create(new StringReader(writer.ToString()))) as SparqlResultSet;

            Assert.Equal(results, results2);
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

        [Fact]
        public void SerializationBinarySparqlResult()
        {
            this.TestBinarySerialization(this.GetResults(), true);
        }

        [Fact]
        public void SerializationBinarySparqlResultSet()
        {
            this.TestBinarySerialization(this.GetResults());
        }

        [Fact]
        public void SerializationXmlSparqlResult()
        {
            this.TestXmlSerialization(this.GetResults(), true);
        }

        [Fact]
        public void SerializationXmlSparqlResultSet()
        {
            this.TestXmlSerialization(this.GetResults());
        }

        [Fact]
        public void SerializationDataContractSparqlResult()
        {
            this.TestDataContractSerialization(this.GetResults(), true);
        }

        [Fact]
        public void SerializationDataContractSparqlResultSet()
        {
            this.TestDataContractSerialization(this.GetResults());
        }
    }
}
