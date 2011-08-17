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
    public class UriLoaderHandlerTests
    {

        [TestMethod]
        public void ParsingUriLoaderGraphHandlerImplicit()
        {
            Graph g = new Graph();
            UriLoader.Load(g, new Uri("http://www.dotnetrdf.org/configuration#"));

            TestTools.ShowGraph(g);
            Assert.IsFalse(g.IsEmpty, "Graph should not be empty");
        }

        [TestMethod]
        public void ParsingUriLoaderGraphHandlerExplicit()
        {
            Graph g = new Graph();
            GraphHandler handler = new GraphHandler(g);
            UriLoader.Load(handler, new Uri("http://www.dotnetrdf.org/configuration#"));

            TestTools.ShowGraph(g);
            Assert.IsFalse(g.IsEmpty, "Graph should not be empty");
        }

        [TestMethod]
        public void ParsingUriLoaderCountHandler()
        {
            Graph orig = new Graph();
            orig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            CountHandler handler = new CountHandler();
            UriLoader.Load(handler, new Uri("http://www.dotnetrdf.org/configuration#"));

            Assert.AreEqual(orig.Triples.Count, handler.Count);
        }
    }
}
