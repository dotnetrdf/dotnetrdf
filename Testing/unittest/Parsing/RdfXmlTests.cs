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
using System.Linq;
using NUnit.Framework;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Parsing
{
    [TestFixture]
	public class RdfXmlTests
	{
        [Test]
        public void ParsingRdfXmlAmpersands()
        {
            List<IRdfWriter> writers = new List<IRdfWriter>()
            {
                new RdfXmlWriter(),
                new PrettyRdfXmlWriter()
            };
            IRdfReader parser = new RdfXmlParser();

            Graph g = new Graph();
            g.BaseUri = new Uri("http://example.org/ampersandsInRdfXml");
            g.Assert(new Triple(g.CreateUriNode(), g.CreateUriNode(new Uri("http://example.org/property")), g.CreateUriNode(new Uri("http://example.org/a&b"))));
            g.Assert(new Triple(g.CreateUriNode(), g.CreateUriNode(new Uri("http://example.org/property")), g.CreateLiteralNode("A & B")));

            foreach (IRdfWriter writer in writers)
            {
                    Console.WriteLine(writer.GetType().ToString());
                    String temp = StringWriter.Write(g, writer);
                    Console.WriteLine(temp);
                    Graph h = new Graph();
                    StringParser.Parse(h, temp);
                    Assert.AreEqual(g, h, "Graphs should be equal");
                    Console.WriteLine();
            }
        }

#if !NO_XMLDOM
        [Test]
        public void ParsingRdfXmlEmptyStrings()
        {
            NTriplesFormatter formatter = new NTriplesFormatter();
            RdfXmlParser domParser = new RdfXmlParser(RdfXmlParserMode.DOM);
            Graph g = new Graph();
            domParser.Load(g, "resources\\empty-string-rdfxml.rdf");

            Console.WriteLine("DOM Parser parsed OK");

            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }
            Console.WriteLine();

            RdfXmlParser streamingParser = new RdfXmlParser(RdfXmlParserMode.Streaming);
            Graph h = new Graph();
            streamingParser.Load(h, "resources\\empty-string-rdfxml.rdf");

            Console.WriteLine("Streaming Parser parsed OK");

            foreach (Triple t in h.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }

            Assert.AreEqual(g, h, "Graphs should be equal");
        }
#endif

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

#if !NO_XMLDOM
        [TestCase(RdfXmlParserMode.DOM, "resources\\sequence.rdf")]
        [TestCase(RdfXmlParserMode.Streaming, "resources\\sequence.rdf")]
        [TestCase(RdfXmlParserMode.DOM, "resources\\sequence2.rdf")]
        [TestCase(RdfXmlParserMode.Streaming, "resources\\sequence2.rdf")]
        public void ParsingRdfXml(RdfXmlParserMode parsingMode, string path)
        {
            RdfXmlParser parser = new RdfXmlParser(parsingMode);
            this.TestRdfXmlSequence(parser, path);
        }
#endif

#if !NO_XMLDOM
        [Test]
        public void ParsingRdfXmlWithUrlEscapedNodes()
        {
            //Originally submitted by Rob Styles as part of CORE-251, modified somewhat during debugging process
            NTriplesFormatter formatter = new NTriplesFormatter();
            RdfXmlParser domParser = new RdfXmlParser(RdfXmlParserMode.DOM);
            Graph g = new Graph();
            domParser.Load(g, "resources\\urlencodes-in-rdfxml.rdf");

            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }

            Uri encoded = new Uri("http://example.com/some%40encoded%2FUri");
            Uri unencoded = new Uri("http://example.com/some@encoded/Uri");

            Assert.IsFalse(EqualityHelper.AreUrisEqual(encoded, unencoded), "URIs should not be equivalent because %40 encodes a reserved character and per RFC 3986 decoding this can change the meaning of the URI");

            IUriNode encodedNode = g.GetUriNode(encoded);
            Assert.IsNotNull(encodedNode, "The encoded node should be returned by its encoded URI");
            IUriNode unencodedNode = g.GetUriNode(unencoded);
            Assert.IsNotNull(unencodedNode, "The unencoded node should be returned by its unencoded URI");

            IUriNode pred = g.CreateUriNode(new Uri("http://example.org/schema/encoded"));
            Assert.IsTrue(g.ContainsTriple(new Triple(encodedNode, pred, g.CreateLiteralNode("true"))), "The encoded node should have the property 'true' from the file");
            Assert.IsTrue(g.ContainsTriple(new Triple(unencodedNode, pred, g.CreateLiteralNode("false"))), "The unencoded node should have the property 'false' from the file");
        }
#endif

#if !NO_XMLDOM
        [Test]
        public void ParsingRdfXmlWithUrlEscapedNodes2()
        {
            //Originally submitted by Rob Styles as part of CORE-251, modified somewhat during debugging process
            NTriplesFormatter formatter = new NTriplesFormatter();
            RdfXmlParser domParser = new RdfXmlParser(RdfXmlParserMode.DOM);
            Graph g = new Graph();
            domParser.Load(g, "resources\\urlencodes-in-rdfxml.rdf");

            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }

            Uri encoded = new Uri("http://example.com/some%20encoded%2FUri");
            Uri unencoded = new Uri("http://example.com/some encoded/Uri");

            Assert.IsTrue(EqualityHelper.AreUrisEqual(encoded, unencoded), "URIs should be equivalent");

            IUriNode encodedNode = g.GetUriNode(encoded);
            Assert.IsNotNull(encodedNode, "The encoded node should be returned by its encoded URI");
            IUriNode unencodedNode = g.GetUriNode(unencoded);
            Assert.IsNotNull(unencodedNode, "The unencoded node should be returned by its unencoded URI");

            IUriNode pred = g.CreateUriNode(new Uri("http://example.org/schema/encoded"));
            Assert.IsTrue(g.ContainsTriple(new Triple(encodedNode, pred, g.CreateLiteralNode("true"))), "The encoded node should have the property 'true' from the file");
            Assert.IsTrue(g.ContainsTriple(new Triple(unencodedNode, pred, g.CreateLiteralNode("false"))), "The unencoded node should have the property 'false' from the file");

        }
#endif

        [Test]
        public void ParsingRdfXmlPropertyInDefaultNamespaceBad()
        {
            Graph g = new Graph();
            RdfXmlParser parser = new RdfXmlParser();

            Assert.Throws<RdfParseException>(() => g.LoadFromFile("resources\\rdfxml-bad-property.rdf", parser));
        }

        [Test]
        public void ParsingRdfXmlPropertyInDefaultNamespaceGood()
        {
            Graph g = new Graph();
            RdfXmlParser parser = new RdfXmlParser();
            g.LoadFromFile("resources\\rdfxml-good-property.rdf", parser);

            Assert.IsFalse(g.IsEmpty);
            Assert.AreEqual(1, g.Triples.Count);

            IUriNode property = g.Triples.First().Predicate as IUriNode;
            Assert.AreEqual("default", property.Uri.Host);
            Assert.AreEqual("good", property.Uri.Segments[1]);
        }

#if !NO_XMLDOM
        [Test]
        public void ParsingRdfXmlElementUsesXmlNamespaceDom()
        {
            Graph g = new Graph();
            g.LoadFromFile("resources\\xml-prop.rdf", new RdfXmlParser(RdfXmlParserMode.DOM));
            Assert.IsFalse(g.IsEmpty);
            Assert.AreEqual(1, g.Triples.Count);
        }
#endif

        [Test]
        public void ParsingRdfXmlElementUsesXmlNamespaceStreaming()
        {
            Graph g = new Graph();
            g.LoadFromFile("resources\\xml-prop.rdf", new RdfXmlParser(RdfXmlParserMode.Streaming));
            Assert.IsFalse(g.IsEmpty);
            Assert.AreEqual(1, g.Triples.Count);
        }

#if !NO_XMLDOM
        [Test]
        public void ParsingRdfXmlElementUsesUndeclaredNamespaceDom()
        {
            Graph g = new Graph();
            g.LoadFromFile(@"..\\resources\missing-namespace-declarations.rdf", new RdfXmlParser(RdfXmlParserMode.DOM));
            Assert.IsFalse(g.IsEmpty);
            Assert.AreEqual(9, g.Triples.Count);
        }
#endif

        [Test]
        public void ParsingRdfXmlElementUsesUndeclaredNamespaceStreaming()
        {
            Graph g = new Graph();
            g.LoadFromFile(@"..\\resources\missing-namespace-declarations.rdf", new RdfXmlParser(RdfXmlParserMode.Streaming));
            Assert.IsFalse(g.IsEmpty);
            Assert.AreEqual(9, g.Triples.Count);
        }

#if !NO_COMPRESSION

        [Test]
        public void ParsingRdfXmlStreamingDoesNotExhaustMemory()
        {
            IGraph g = new Graph();
            GraphHandler graphHandler = new GraphHandler(g);
            PagingHandler paging = new PagingHandler(graphHandler, 1000);
            CountHandler counter = new CountHandler();
            ChainedHandler handler = new ChainedHandler(new IRdfHandler[] { paging, counter });

            GZippedRdfXmlParser parser = new GZippedRdfXmlParser(RdfXmlParserMode.Streaming);
            parser.Load(handler, @"..\\resources\oom.rdf.gz");

            Assert.IsFalse(g.IsEmpty);
            Assert.AreEqual(1000, counter.Count);
            // Note that the source produces some duplicate triples so triples in the graph will be at most 1000
            Assert.IsTrue(g.Triples.Count <= 1000);
        }

#endif

        [Test]
        public void ParsingRdfXmlStackOverflow1()
        {
            IGraph g = new Graph();
            RdfXmlParser parser = new RdfXmlParser();
            parser.Load(g, @"..\\resources\cogapp.rdf");

            Assert.IsFalse(g.IsEmpty);
            Assert.AreEqual(9358, g.Triples.Count);
        }

        [Test]
        public void ParsingRdfXmlStackOverflow2()
        {
            TestTools.RunAtDepth(100, this.ParsingRdfXmlStackOverflow1);
        }

        [Test]
        public void ParsingRdfXmlStackOverflow3()
        {
            TestTools.RunAtDepth(1000, this.ParsingRdfXmlStackOverflow1);
        }

        [Test,Ignore("potentially risky test")]
        public void ParsingRdfXmlStackOverflow4()
        {
            TestTools.RunAtDepth(5000, this.ParsingRdfXmlStackOverflow1);
        }

        [Test]
        public void ParsingRdfXmlResetDefaultNamespace()
        {
            IGraph g = new Graph();
            var parser = new RdfXmlParser(RdfXmlParserMode.Streaming);
            parser.Load(g, @"..\\resources\rdfxml-defaultns-scope.xml");
            var resourceNode = g.CreateUriNode(UriFactory.Create("http://example.org/thing/1"));
            var p1Node = g.CreateUriNode(UriFactory.Create("http://example.org/ns/b#p1"));
            var p2Node = g.CreateUriNode(UriFactory.Create("http://example.org/ns/a#p2"));
            var triples = g.GetTriplesWithSubject(resourceNode).ToList();

            Assert.IsFalse(g.IsEmpty);
            Assert.AreEqual(3, triples.Count);
            Assert.IsNotNull(p2Node);
            Assert.AreEqual(1, g.GetTriplesWithSubjectPredicate(resourceNode, p1Node).Count());
            Assert.AreEqual(1, g.GetTriplesWithSubjectPredicate(resourceNode, p2Node).Count());
        }
	}
}
