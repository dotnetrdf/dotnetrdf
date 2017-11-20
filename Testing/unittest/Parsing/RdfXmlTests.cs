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
using Xunit;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Parsing
{
	public partial class RdfXmlTests
	{
        [Fact]
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
                    Assert.Equal(g, h);
                    Console.WriteLine();
            }
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

        [Theory]
        [InlineData(RdfXmlParserMode.DOM, "resources\\sequence.rdf")]
        [InlineData(RdfXmlParserMode.Streaming, "resources\\sequence.rdf")]
        [InlineData(RdfXmlParserMode.DOM, "resources\\sequence2.rdf")]
        [InlineData(RdfXmlParserMode.Streaming, "resources\\sequence2.rdf")]
        public void ParsingRdfXml(RdfXmlParserMode parsingMode, string path)
        {
            RdfXmlParser parser = new RdfXmlParser(parsingMode);
            this.TestRdfXmlSequence(parser, path);
        }

        [Fact]
        public void ParsingRdfXmlPropertyInDefaultNamespaceBad()
        {
            Graph g = new Graph();
            RdfXmlParser parser = new RdfXmlParser();

            Assert.Throws<RdfParseException>(() => g.LoadFromFile("resources\\rdfxml-bad-property.rdf", parser));
        }

        [Fact]
        public void ParsingRdfXmlPropertyInDefaultNamespaceGood()
        {
            Graph g = new Graph();
            RdfXmlParser parser = new RdfXmlParser();
            g.LoadFromFile("resources\\rdfxml-good-property.rdf", parser);

            Assert.False(g.IsEmpty);
            Assert.Equal(1, g.Triples.Count);

            IUriNode property = g.Triples.First().Predicate as IUriNode;
            Assert.Equal("default", property.Uri.Host);
            Assert.Equal("good", property.Uri.Segments[1]);
        }

        [Fact]
        public void ParsingRdfXmlElementUsesXmlNamespaceDom()
        {
            Graph g = new Graph();
            g.LoadFromFile("resources\\xml-prop.rdf", new RdfXmlParser(RdfXmlParserMode.DOM));
            Assert.False(g.IsEmpty);
            Assert.Equal(1, g.Triples.Count);
        }

        [Fact]
        public void ParsingRdfXmlElementUsesXmlNamespaceStreaming()
        {
            Graph g = new Graph();
            g.LoadFromFile("resources\\xml-prop.rdf", new RdfXmlParser(RdfXmlParserMode.Streaming));
            Assert.False(g.IsEmpty);
            Assert.Equal(1, g.Triples.Count);
        }

        [Fact]
        public void ParsingRdfXmlElementUsesUndeclaredNamespaceStreaming()
        {
            Graph g = new Graph();
            g.LoadFromFile(@"resources\missing-namespace-declarations.rdf", new RdfXmlParser(RdfXmlParserMode.Streaming));
            Assert.False(g.IsEmpty);
            Assert.Equal(9, g.Triples.Count);
        }

        [Fact]
        public void ParsingRdfXmlStreamingDoesNotExhaustMemory()
        {
            IGraph g = new Graph();
            GraphHandler graphHandler = new GraphHandler(g);
            PagingHandler paging = new PagingHandler(graphHandler, 1000);
            CountHandler counter = new CountHandler();
            ChainedHandler handler = new ChainedHandler(new IRdfHandler[] { paging, counter });

            GZippedRdfXmlParser parser = new GZippedRdfXmlParser(RdfXmlParserMode.Streaming);
            parser.Load(handler, @"resources\oom.rdf.gz");

            Assert.False(g.IsEmpty);
            Assert.Equal(1000, counter.Count);
            // Note that the source produces some duplicate triples so triples in the graph will be at most 1000
            Assert.True(g.Triples.Count <= 1000);
        }

        [Fact]
        public void ParsingRdfXmlStackOverflow1()
        {
            IGraph g = new Graph();
            RdfXmlParser parser = new RdfXmlParser();
            parser.Load(g, @"resources\cogapp.rdf");

            Assert.False(g.IsEmpty);
            Assert.Equal(9358, g.Triples.Count);
        }

        [Fact]
        public void ParsingRdfXmlStackOverflow2()
        {
            TestTools.RunAtDepth(100, this.ParsingRdfXmlStackOverflow1);
        }

        [Fact]
        public void ParsingRdfXmlStackOverflow3()
        {
            TestTools.RunAtDepth(1000, this.ParsingRdfXmlStackOverflow1);
        }

        [Fact(Skip="potentially risky test")]
        public void ParsingRdfXmlStackOverflow4()
        {
            TestTools.RunAtDepth(5000, this.ParsingRdfXmlStackOverflow1);
        }

        [Fact]
        public void ParsingRdfXmlResetDefaultNamespace()
        {
            IGraph g = new Graph();
            var parser = new RdfXmlParser(RdfXmlParserMode.Streaming);
            parser.Load(g, @"resources\rdfxml-defaultns-scope.xml");
            var resourceNode = g.CreateUriNode(UriFactory.Create("http://example.org/thing/1"));
            var p1Node = g.CreateUriNode(UriFactory.Create("http://example.org/ns/b#p1"));
            var p2Node = g.CreateUriNode(UriFactory.Create("http://example.org/ns/a#p2"));
            var triples = g.GetTriplesWithSubject(resourceNode).ToList();

            Assert.False(g.IsEmpty);
            Assert.Equal(3, triples.Count);
            Assert.NotNull(p2Node);
            Assert.Single(g.GetTriplesWithSubjectPredicate(resourceNode, p1Node));
            Assert.Single(g.GetTriplesWithSubjectPredicate(resourceNode, p2Node));
        }
	}
}
