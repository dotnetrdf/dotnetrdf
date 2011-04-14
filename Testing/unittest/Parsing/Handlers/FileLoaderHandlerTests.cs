using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Test.Parsing.Handlers
{
    [TestClass]
    public class FileLoaderHandlerTests
    {
        private void EnsureTestData(String testFile)
        {
            if (!File.Exists(testFile))
            {
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                g.SaveToFile(testFile);
            }
        }

        [TestMethod]
        public void ParsingFileLoaderGraphHandlerImplicitTurtle()
        {
            EnsureTestData("temp.ttl");

            Graph g = new Graph();
            FileLoader.Load(g, "temp.ttl");

            TestTools.ShowGraph(g);
            Assert.IsFalse(g.IsEmpty, "Graph should not be empty");
        }

        [TestMethod]
        public void ParsingFileLoaderGraphHandlerExplicitTurtle()
        {
            EnsureTestData("temp.ttl");
            Graph g = new Graph();
            GraphHandler handler = new GraphHandler(g);
            FileLoader.Load(handler, "temp.ttl");

            TestTools.ShowGraph(g);
            Assert.IsFalse(g.IsEmpty, "Graph should not be empty");
        }

        [TestMethod]
        public void ParsingFileLoaderCountHandlerTurtle()
        {
            EnsureTestData("temp.ttl");
            Graph orig = new Graph();
            orig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            CountHandler handler = new CountHandler();
            FileLoader.Load(handler, "temp.ttl");

            Assert.AreEqual(orig.Triples.Count, handler.Count);
        }
    }
}
