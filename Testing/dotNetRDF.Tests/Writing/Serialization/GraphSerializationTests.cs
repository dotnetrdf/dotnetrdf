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
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Xunit;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.XunitExtensions;

namespace VDS.RDF.Writing.Serialization
{
    public class GraphSerializationTests
    {
        private void TestGraphSerializationXml<T>(T g)
            where T : class, IGraph
        {
            System.IO.StringWriter writer = new System.IO.StringWriter();
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            serializer.Serialize(writer, g);
            Console.WriteLine("Serialized Form:");
            Console.WriteLine(writer.ToString());
            Console.WriteLine();

            T h = serializer.Deserialize(new StringReader(writer.ToString())) as T;
            GraphDiffReport report = g.Difference(h);
            if (!report.AreEqual)
            {
                TestTools.ShowDifferences(report);
            }
            Assert.True(report.AreEqual, "Graphs should be equal");
        }

        private void TestGraphSerializationXml(IGraph g)
        {
            this.TestGraphSerializationXml<IGraph>(g);
        }

        private void TestGraphSerializationXml(IEnumerable<IGraph> gs)
        {
            foreach (IGraph g in gs)
            {
                this.TestGraphSerializationXml(g);
            }
        }

        private void TestGraphSerializationBinary<T>(T g)
            where T : class, IGraph
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
            T h = serializer.Deserialize(stream) as T;
            reader.Close();

            GraphDiffReport report = g.Difference(h);
            if (!report.AreEqual)
            {
                TestTools.ShowDifferences(report);
            }
            Assert.True(report.AreEqual, "Graphs should be equal");

            stream.Dispose();
        }

        private void TestGraphSerializationBinary(IGraph g)
        {
            this.TestGraphSerializationBinary<IGraph>(g);
        }

        private void TestGraphSerializationBinary(IEnumerable<IGraph> gs)
        {
            foreach (IGraph g in gs)
            {
                this.TestGraphSerializationBinary(g);
            }
        }

        private void TestGraphSerializationDataContract<T>(T g)
            where T : class, IGraph
        {
            System.IO.StringWriter writer = new System.IO.StringWriter();
            DataContractSerializer serializer = new DataContractSerializer(typeof(T));

            serializer.WriteObject(new XmlTextWriter(writer), g);
            Console.WriteLine("Serialized Form:");
            Console.WriteLine(writer.ToString());
            Console.WriteLine();

            T h = serializer.ReadObject(XmlReader.Create(new StringReader(writer.ToString()))) as T;
            GraphDiffReport report = g.Difference(h);
            if (!report.AreEqual)
            {
                TestTools.ShowDifferences(report);
            }
            Assert.True(report.AreEqual, "Graphs should be equal");
        }

        private void TestGraphSerializationDataContract(IGraph g)
        {
            this.TestGraphSerializationDataContract<IGraph>(g);
        }

        private void TestGraphSerializationJson<T>(T g)
            where T : class, IGraph
        {
            System.IO.StringWriter writer = new System.IO.StringWriter();
            JsonSerializer serializer = new JsonSerializer();
            serializer.TypeNameHandling = TypeNameHandling.All;

            serializer.Serialize(new JsonTextWriter(writer), g);
            Console.WriteLine("Serialized Form:");
            Console.WriteLine(writer.ToString());
            Console.WriteLine();

            Object h = serializer.Deserialize<T>(new JsonTextReader(new StringReader(writer.ToString())));
            Console.WriteLine(h.GetType().FullName);
            GraphDiffReport report = g.Difference((IGraph)h);
            if (!report.AreEqual)
            {
                TestTools.ShowDifferences(report);
            }
            Assert.True(report.AreEqual, "Graphs should be equal");
        }

        private void TestGraphSerializationJson(IGraph g)
        {
            this.TestGraphSerializationJson<IGraph>(g);
        }

        [Fact]
        public void SerializationXmlGraph()
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(new PagingHandler(new GraphHandler(g), 50), "VDS.RDF.Configuration.configuration.ttl");

            this.TestGraphSerializationXml(g);
        }

        [Fact]
        public void SerializationXmlGraph2()
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");

            this.TestGraphSerializationXml(g);
        }

        [SkippableFact]
        public void SerializationXmlGraph3()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseRemoteParsing))
            {
                throw new SkipTestException("Test Config marks Remote Parsing as unavailable, test cannot be run");
            }

            Graph g = new Graph();
            UriLoader.Load(g, new Uri("http://dbpedia.org/resource/Ilkeston"));

            this.TestGraphSerializationXml(g);
        }

        [Fact]
        public void SerializationXmlGraph4()
        {
            Graph g = new Graph();
            g.LoadFromFile("resources\\complex-collections.nt");

            this.TestGraphSerializationXml(g);
        }

        [Fact]
        public void SerializationXmlGraph5()
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(new PagingHandler(new GraphHandler(g), 50), "VDS.RDF.Configuration.configuration.ttl");

            this.TestGraphSerializationXml<MockWrapperGraph>(new MockWrapperGraph(g));
        }

        [Fact]
        public void SerializationBinaryGraph()
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(new PagingHandler(new GraphHandler(g), 50), "VDS.RDF.Configuration.configuration.ttl");

            this.TestGraphSerializationBinary(g);
        }

        [Fact]
        public void SerializationBinaryGraph2()
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");

            this.TestGraphSerializationBinary(g);
        }

        [SkippableFact]
        public void SerializationBinaryGraph3()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseRemoteParsing))
            {
                throw new SkipTestException("Test Config marks Remote Parsing as unavailable, test cannot be run");
            }

            Graph g = new Graph();
            UriLoader.Load(g, new Uri("http://dbpedia.org/resource/Ilkeston"));

            this.TestGraphSerializationBinary(g);
        }

        [Fact]
        public void SerializationBinaryGraph4()
        {
            Graph g = new Graph();
            g.LoadFromFile("resources\\complex-collections.nt");

            this.TestGraphSerializationBinary(g);
        }

        [Fact]
        public void SerializationBinaryGraph5()
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(new PagingHandler(new GraphHandler(g), 50), "VDS.RDF.Configuration.configuration.ttl");

            this.TestGraphSerializationBinary<MockWrapperGraph>(new MockWrapperGraph(g));
        }

        [Fact]
        public void SerializationDataContractGraph()
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(new PagingHandler(new GraphHandler(g), 50), "VDS.RDF.Configuration.configuration.ttl");

            this.TestGraphSerializationDataContract(g);
        }

        [Fact]
        public void SerializationDataContractGraph2()
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");

            this.TestGraphSerializationDataContract(g);
        }

        [SkippableFact]
        public void SerializationDataContractGraph3()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseRemoteParsing))
            {
                throw new SkipTestException("Test Config marks Remote Parsing as unavailable, test cannot be run");
            }

            Graph g = new Graph();
            UriLoader.Load(g, new Uri("http://dbpedia.org/resource/Ilkeston"));

            this.TestGraphSerializationDataContract(g);
        }

        [Fact]
        public void SerializationDataContractGraph4()
        {
            Graph g = new Graph();
            g.LoadFromFile("resources\\complex-collections.nt");

            this.TestGraphSerializationDataContract(g);
        }

        [Fact]
        public void SerializationDataContractGraph5()
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(new PagingHandler(new GraphHandler(g), 50), "VDS.RDF.Configuration.configuration.ttl");

            this.TestGraphSerializationDataContract(new MockWrapperGraph(g));
        }

        [Fact]
        public void SerializationJsonGraph()
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(new PagingHandler(new GraphHandler(g), 50), "VDS.RDF.Configuration.configuration.ttl");

            this.TestGraphSerializationJson(g);
        }

        [Fact]
        public void SerializationJsonGraph2()
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");

            this.TestGraphSerializationJson(g);
        }

        [SkippableFact]
        public void SerializationJsonGraph3()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseRemoteParsing))
            {
                throw new SkipTestException("Test Config marks Remote Parsing as unavailable, test cannot be run");
            }

            Graph g = new Graph();
            UriLoader.Load(g, new Uri("http://dbpedia.org/resource/Ilkeston"));

            this.TestGraphSerializationJson(g);
        }

        [Fact]
        public void SerializationJsonGraph4()
        {
            Graph g = new Graph();
            g.LoadFromFile("resources\\complex-collections.nt");

            this.TestGraphSerializationJson(g);
        }

        //[Fact]
        //public void SerializationJsonGraph5()
        //{
        //    Graph g = new Graph();
        //    EmbeddedResourceLoader.Load(new PagingHandler(new GraphHandler(g), 50), "VDS.RDF.Configuration.configuration.ttl");

        //    this.TestGraphSerializationJson(new MockWrapperGraph(g));
        //}

    }

    [Serializable]
    public class MockWrapperGraph
        : WrapperGraph
        , ISerializable
    {
        protected MockWrapperGraph()
            : base() { }

        public MockWrapperGraph(IGraph g)
            : base(g) { }

        protected MockWrapperGraph(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
