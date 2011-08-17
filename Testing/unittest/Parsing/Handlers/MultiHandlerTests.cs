using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Parsing.Handlers
{
    [TestClass]
    public class MultiHandlerTests
    {
        private void EnsureTestData()
        {
            if (!System.IO.File.Exists("temp.ttl"))
            {
                Graph g = new Graph();
                EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");
                g.SaveToFile("temp.ttl");
            }
        }
        
        [TestMethod,ExpectedException(typeof(ArgumentException))]
        public void ParsingMultiHandlerBadInstantiation()
        {
            MultiHandler handler = new MultiHandler(Enumerable.Empty<IRdfHandler>());
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void ParsingMultiHandlerBadInstantiation2()
        {
            MultiHandler handler = new MultiHandler(null);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void ParsingMultiHandlerBadInstantiation3()
        {
            GraphHandler h = new GraphHandler(new Graph());
            MultiHandler handler = new MultiHandler(new IRdfHandler[] { h, h });
        }

        [TestMethod]
        public void ParsingMultiHandlerTwoGraphs()
        {
            EnsureTestData();

            Graph g = new Graph();
            Graph h = new Graph();

            GraphHandler handler1 = new GraphHandler(g);
            GraphHandler handler2 = new GraphHandler(h);

            MultiHandler handler = new MultiHandler(new IRdfHandler[] { handler1, handler2 });

            TurtleParser parser = new TurtleParser();
            parser.Load(handler, "temp.ttl");

            Assert.AreEqual(g.Triples.Count, h.Triples.Count, "Expected same number of Triples");
            Assert.AreEqual(g, h, "Expected Graphs to be equal");
        }

        [TestMethod]
        public void ParsingMultiHandlerGraphAndPaging()
        {
            EnsureTestData();

            Graph g = new Graph();
            Graph h = new Graph();

            GraphHandler handler1 = new GraphHandler(g);
            PagingHandler handler2 = new PagingHandler(new GraphHandler(h), 100);

            MultiHandler handler = new MultiHandler(new IRdfHandler[] { handler1, handler2 });

            TurtleParser parser = new TurtleParser();
            parser.Load(handler, "temp.ttl");

            Assert.AreEqual(101, g.Triples.Count, "Triples should have been limited to 101 (1st Graph)");
            Assert.AreEqual(100, h.Triples.Count, "Triples should have been limited to 100 (2nd Graph)");
            Assert.AreNotEqual(g.Triples.Count, h.Triples.Count, "Expected different number of Triples");
            Assert.AreNotEqual(g, h, "Expected Graphs to not be equal");
        }

        [TestMethod]
        public void ParsingMultiHandlerGraphAndPaging2()
        {
            EnsureTestData();

            Graph g = new Graph();
            Graph h = new Graph();

            GraphHandler handler1 = new GraphHandler(g);
            PagingHandler handler2 = new PagingHandler(new GraphHandler(h), 100);

            MultiHandler handler = new MultiHandler(new IRdfHandler[] { handler2, handler1 });

            TurtleParser parser = new TurtleParser();
            parser.Load(handler, "temp.ttl");

            Assert.AreEqual(101, g.Triples.Count, "Triples should have been limited to 101 (1st Graph)");
            Assert.AreEqual(100, h.Triples.Count, "Triples should have been limited to 100 (2nd Graph)");
            Assert.AreNotEqual(g.Triples.Count, h.Triples.Count, "Expected different number of Triples");
            Assert.AreNotEqual(g, h, "Expected Graphs to not be equal");
        }
        
        [TestMethod]
        public void ParsingMultiHandlerGraphAndCount()
        {
            EnsureTestData();

            Graph g = new Graph();
            GraphHandler handler1 = new GraphHandler(g);

            CountHandler handler2 = new CountHandler();

            MultiHandler handler = new MultiHandler(new IRdfHandler[] { handler1, handler2 });

            TurtleParser parser = new TurtleParser();
            parser.Load(handler, "temp.ttl");

            Assert.AreEqual(g.Triples.Count, handler2.Count, "Expected Counts to be the same");
 
        }

        [TestMethod]
        public void ParsingMultiHandlerGraphAndNull()
        {
            EnsureTestData();

            Graph g = new Graph();
            GraphHandler handler1 = new GraphHandler(g);

            NullHandler handler2 = new NullHandler();

            MultiHandler handler = new MultiHandler(new IRdfHandler[] { handler1, handler2 });

            TurtleParser parser = new TurtleParser();
            parser.Load(handler, "temp.ttl");
        }
    }
}
