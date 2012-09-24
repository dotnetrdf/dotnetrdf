/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

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
                    Console.WriteLine(writer.GetType().ToString());
                    String temp = StringWriter.Write(g, writer);
                    Console.WriteLine(temp);
                    Graph h = new Graph();
                    StringParser.Parse(h, temp);
                    Assert.AreEqual(g, h, "Graphs should be equal");
                    Console.WriteLine();
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

        [TestMethod]
        public void ParsingRdfXmlWithUrlEscapedNodes()
        {
            //Originally submitted by Rob Styles as part of CORE-251, modified somewhat during debugging process
            NTriplesFormatter formatter = new NTriplesFormatter();
            RdfXmlParser domParser = new RdfXmlParser(RdfXmlParserMode.DOM);
            Graph g = new Graph();
            domParser.Load(g, "urlencodes-in-rdfxml.rdf");

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

        [TestMethod]
        public void ParsingRdfXmlWithUrlEscapedNodes2()
        {
            //Originally submitted by Rob Styles as part of CORE-251, modified somewhat during debugging process
            NTriplesFormatter formatter = new NTriplesFormatter();
            RdfXmlParser domParser = new RdfXmlParser(RdfXmlParserMode.DOM);
            Graph g = new Graph();
            domParser.Load(g, "urlencodes-in-rdfxml.rdf");

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

        [TestMethod, ExpectedException(typeof(RdfParseException))]
        public void ParsingRdfXmlPropertyInDefaultNamespaceBad()
        {
            Graph g = new Graph();
            RdfXmlParser parser = new RdfXmlParser();
            g.LoadFromFile("rdfxml-bad-property.rdf", parser);
        }

        [TestMethod]
        public void ParsingRdfXmlPropertyInDefaultNamespaceGood()
        {
            Graph g = new Graph();
            RdfXmlParser parser = new RdfXmlParser();
            g.LoadFromFile("rdfxml-good-property.rdf", parser);

            Assert.IsFalse(g.IsEmpty);
            Assert.AreEqual(1, g.Triples.Count);

            IUriNode property = g.Triples.First().Predicate as IUriNode;
            Assert.AreEqual("default", property.Uri.Host);
            Assert.AreEqual("good", property.Uri.Segments[1]);
        }
	}
}
