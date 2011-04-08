using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Storage.Params;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Writing
{
    [TestClass]
    public class TriGWriterTests
    {
        [TestMethod]
        public void WritingTriG()
        {
            TestTools.TestInMTAThread(new ThreadStart(this.WritingTriGActual));
        }

        [TestMethod]
        public void WritingTriGSingleThreaded()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            g.BaseUri = null;
            TripleStore store = new TripleStore();
            store.Add(g);

            TriGWriter writer = new TriGWriter();
            writer.UseMultiThreadedWriting = false;
            System.IO.StringWriter strWriter = new System.IO.StringWriter();
            writer.Save(store, new TextWriterParams(strWriter));

            Console.WriteLine(strWriter.ToString());

            Assert.IsFalse(strWriter.ToString().Equals(String.Empty));
        }

        private void WritingTriGActual()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            g.BaseUri = null;
            TripleStore store = new TripleStore();
            store.Add(g);

            TriGWriter writer = new TriGWriter();
            System.IO.StringWriter strWriter = new System.IO.StringWriter();
            writer.Save(store, new TextWriterParams(strWriter));

            Console.WriteLine(strWriter.ToString());

            Assert.IsFalse(strWriter.ToString().Equals(String.Empty));
        }

        [TestMethod]
        public void WritingTriGUncompressed()
        {
            TestTools.TestInMTAThread(new ThreadStart(this.WritingTriGUncompressedActual));
        }

        [TestMethod]
        public void WritingTriGUncompressedSingleThreaded()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            g.BaseUri = null;
            TripleStore store = new TripleStore();
            store.Add(g);

            TriGWriter writer = new TriGWriter();
            writer.UseMultiThreadedWriting = false;
            writer.CompressionLevel = WriterCompressionLevel.None;
            System.IO.StringWriter strWriter = new System.IO.StringWriter();
            writer.Save(store, new TextWriterParams(strWriter));

            Console.WriteLine(strWriter.ToString());

            Assert.IsFalse(strWriter.ToString().Equals(String.Empty));
        }

        private void WritingTriGUncompressedActual()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            g.BaseUri = null;
            TripleStore store = new TripleStore();
            store.Add(g);

            TriGWriter writer = new TriGWriter();
            writer.CompressionLevel = WriterCompressionLevel.None;
            System.IO.StringWriter strWriter = new System.IO.StringWriter();
            writer.Save(store, new TextWriterParams(strWriter));

            Console.WriteLine(strWriter.ToString());

            Assert.IsFalse(strWriter.ToString().Equals(String.Empty));
        }
    }
}
