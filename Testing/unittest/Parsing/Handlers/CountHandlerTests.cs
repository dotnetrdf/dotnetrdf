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
    public class CountHandlerTests
    {
        private void ParsingUsingCountHandler(String tempFile, IRdfReader parser)
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");
            g.SaveToFile(tempFile);

            CountHandler handler = new CountHandler();
            parser.Load(handler, tempFile);

            Console.WriteLine("Counted " + handler.Count + " Triples");
            Assert.AreEqual(g.Triples.Count, handler.Count, "Counts should have been equal");
        }

        [TestMethod]
        public void ParsingCountHandlerNTriples()
        {
            this.ParsingUsingCountHandler("test.nt", new NTriplesParser());
        }

        [TestMethod]
        public void ParsingCountHandlerTurtle()
        {
            this.ParsingUsingCountHandler("test.ttl", new TurtleParser());
        }

        [TestMethod]
        public void ParsingCountHandlerNotation3()
        {
            this.ParsingUsingCountHandler("temp.n3", new Notation3Parser());
        }

        [TestMethod]
        public void ParsingCountHandlerRdfXml()
        {
            this.ParsingUsingCountHandler("test.rdf", new RdfXmlParser());
        }

        [TestMethod]
        public void ParsingCountHandlerRdfA()
        {
            this.ParsingUsingCountHandler("test.html", new RdfAParser());
        }

        [TestMethod]
        public void ParsingCountHandlerRdfJson()
        {
            this.ParsingUsingCountHandler("test.json", new RdfJsonParser());
        }
    }
}
