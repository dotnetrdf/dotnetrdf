using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Parsing
{
    [TestClass]
	public class RdfXmlTests
	{
        [TestMethod]
        public void ParsingRdfXmlAmpersands()
        {
            List<IRdfWriter> writers = new List<IRdfWriter>()
            {
                new RdfXmlWriter(),
                new FastRdfXmlWriter()
            };
            IRdfReader parser = new RdfXmlParser();

            Graph g = new Graph();
            g.BaseUri = new Uri("http://example.org/ampersandsInRdfXml");
            g.Assert(new Triple(g.CreateUriNode(), g.CreateUriNode(new Uri("http://example.org/property")), g.CreateUriNode(new Uri("http://example.org/a&b"))));
            g.Assert(new Triple(g.CreateUriNode(), g.CreateUriNode(new Uri("http://example.org/property")), g.CreateLiteralNode("A & B")));

            foreach (IRdfWriter writer in writers)
            {
                try
                {
                    Console.WriteLine(writer.GetType().ToString());
                    String temp = StringWriter.Write(g, writer);
                    Console.WriteLine(temp);
                    Graph h = new Graph();
                    StringParser.Parse(h, temp);
                    Assert.AreEqual(g, h, "Graphs should be equal");
                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    TestTools.ReportError("Error", ex, true);
                }
            }
        }

        [TestMethod]
        public void ParsingRdfXmlEmptyStrings()
        {
            NTriplesFormatter formatter = new NTriplesFormatter();
            RdfXmlParser domParser = new RdfXmlParser(RdfXmlParserMode.DOM);
            Graph g = new Graph();
            domParser.Load(g, "empty-string-rdfxml.rdf");

            Console.WriteLine("DOM Parser parsed OK");

            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }
            Console.WriteLine();

            RdfXmlParser streamingParser = new RdfXmlParser(RdfXmlParserMode.Streaming);
            Graph h = new Graph();
            streamingParser.Load(h, "empty-string-rdfxml.rdf");

            Console.WriteLine("Streaming Parser parsed OK");

            foreach (Triple t in h.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }

            Assert.AreEqual(g, h, "Graphs should be equal");
        }

        private void TestRdfXmlSequence(IRdfReader parser, String file)
        {
            Graph g = new Graph();
            if (parser is ITraceableParser)
            {
                ((ITraceableParser)parser).TraceParsing = true;
            }
            parser.Load(g, file);

            TestTools.ShowGraph(g);
        }

        [TestMethod]
        public void ParsingRdfXmlSequenceStreaming()
        {
            RdfXmlParser parser = new RdfXmlParser(RdfXmlParserMode.Streaming);
            this.TestRdfXmlSequence(parser, "sequence.rdf");
        }

        [TestMethod]
        public void ParsingRdfXmlSequenceDom()
        {
            RdfXmlParser parser = new RdfXmlParser(RdfXmlParserMode.DOM);
            this.TestRdfXmlSequence(parser, "sequence.rdf");
        }

        [TestMethod]
        public void ParsingRdfXmlSequenceStreaming2()
        {
            RdfXmlParser parser = new RdfXmlParser(RdfXmlParserMode.Streaming);
            this.TestRdfXmlSequence(parser, "sequence2.rdf");
        }

        [TestMethod]
        public void ParsingRdfXmlSequenceDom2()
        {
            RdfXmlParser parser = new RdfXmlParser(RdfXmlParserMode.DOM);
            this.TestRdfXmlSequence(parser, "sequence2.rdf");
        }
	}
}
