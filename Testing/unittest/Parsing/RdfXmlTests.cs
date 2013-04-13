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
using System.Text;
using NUnit.Framework;
using VDS.RDF.Parsing;
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

        [TestCase(RdfXmlParserMode.DOM, "resources\\sequence.rdf")]
        [TestCase(RdfXmlParserMode.Streaming, "resources\\sequence.rdf")]
        [TestCase(RdfXmlParserMode.DOM, "resources\\sequence2.rdf")]
        [TestCase(RdfXmlParserMode.Streaming, "resources\\sequence2.rdf")]
        public void ParsingRdfXml(RdfXmlParserMode parsingMode, string path)
        {
            RdfXmlParser parser = new RdfXmlParser(parsingMode);
            this.TestRdfXmlSequence(parser, path);
        }

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

        [Test, ExpectedException(typeof(RdfParseException))]
        public void ParsingRdfXmlPropertyInDefaultNamespaceBad()
        {
            Graph g = new Graph();
            RdfXmlParser parser = new RdfXmlParser();
            g.LoadFromFile("resources\\rdfxml-bad-property.rdf", parser);
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
	}
}
