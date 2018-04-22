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

	    [Fact]
	    public void EmptySameDocumentReferenceResolvesAgainstUriPartOfBaseUri()
	    {
	        var rdfXml1 = "<rdf:RDF xml:base='http://example.org#fragment' xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:eg='http://example.org/'><eg:type rdf:about='' /></rdf:RDF>";
	        var rdfXml2 = "<rdf:RDF xml:base='http://example.org' xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:eg='http://example.org/'><eg:type rdf:about='' /></rdf:RDF>";

	        var parser = new RdfXmlParser();

	        var graph1 = new Graph();
	        graph1.LoadFromString(rdfXml1, parser);

	        var graph2 = new Graph();
	        graph2.LoadFromString(rdfXml2, parser);

	        var diff = graph1.Difference(graph2);

	        Assert.True(diff.AreEqual);
        }

	    [Fact]
	    public void ItExpandsRdfListElementsRegardlessOfNamespacePrefix()
	    {
	        var rdfXml1 = "<RDF xmlns='http://www.w3.org/1999/02/22-rdf-syntax-ns#'><Seq><li>bar</li></Seq></RDF>";
	        var rdfXml2 = "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#'><rdf:Seq><rdf:li>bar</rdf:li></rdf:Seq></rdf:RDF>";
	        var rdfXml3 =
	            "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#'><rdf:Seq><foo:li xmlns:foo='http://www.w3.org/1999/02/22-rdf-syntax-ns#'>bar</foo:li></rdf:Seq></rdf:RDF>";

            var parser = new RdfXmlParser();
	        var graph1 = new Graph();
	        graph1.LoadFromString(rdfXml1, parser);

	        var graph2 = new Graph();
	        graph2.LoadFromString(rdfXml2, parser);

	        var graph3 = new Graph();
            graph3.LoadFromString(rdfXml3, parser);

	        var diff12 = graph1.Difference(graph2);
	        var diff13 = graph1.Difference(graph3);
            
            Assert.True(diff12.AreEqual);
            Assert.True(diff13.AreEqual);
        }

        [Fact]
	    public void ItHandlesRdfTypeAttributesRegardlessOfNamespacePrefix()
	    {
	        var rdfXml1 = "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:eg='http://example.org/'><eg:Example rdf:about='http://example.org/#us'><rdf:type rdf:Resource='http://example.org/SomeType'/></eg:Example></rdf:RDF>";
	        var rdfXml2 = "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:eg='http://example.org/'><eg:Example rdf:about='http://example.org/#us'><eg:type  rdf:Resource='http://example.org/SomeType' xmlns:eg='http://www.w3.org/1999/02/22-rdf-syntax-ns#'/></eg:Example></rdf:RDF>";
	        var rdfXml3 = "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:eg='http://example.org/'><eg:Example rdf:about='http://example.org/#us'><type     rdf:Resource='http://example.org/SomeType' xmlns='http://www.w3.org/1999/02/22-rdf-syntax-ns#' /></eg:Example></rdf:RDF>";

            var parser = new RdfXmlParser();

	        var graph1 = new Graph();
	        graph1.LoadFromString(rdfXml1, parser);

	        var graph2 = new Graph();
	        graph2.LoadFromString(rdfXml2, parser);

	        var graph3 = new Graph();
            graph3.LoadFromString(rdfXml3, parser);

	        var diff12 = graph1.Difference(graph2);
	        var diff13 = graph1.Difference(graph3);

	        Assert.True(diff12.AreEqual);
	        Assert.True(diff13.AreEqual);
        }

        [Fact]
        public void ItHandlesRdfParseTypeRegardlessOfNamespacePrefix()
        {
            var rdfXml1 = "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:eg='http://example.org/'><eg:Example rdf:about='http://example.org/#us'><eg:prop rdf:parseType='Literal'/><eg:Value /></eg:Example></rdf:RDF>";
            var rdfXml2 = "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:eg='http://example.org/'><eg:Example rdf:about='http://example.org/#us'><eg:prop  xx:parseType='Literal' xmlns:xx='http://www.w3.org/1999/02/22-rdf-syntax-ns#'/><eg:Value /></eg:Example></rdf:RDF>";

            var parser = new RdfXmlParser();

            var graph1 = new Graph();
            graph1.LoadFromString(rdfXml1, parser);

            var graph2 = new Graph();
            graph2.LoadFromString(rdfXml2, parser);

            var diff12 = graph1.Difference(graph2);

            Assert.True(diff12.AreEqual);
        }

	    [Fact]
	    public void ItHandlesRdfDescriptionRegardessOfNamespacePrefix()
	    {
	        var rdfXml1 = "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:ex='http://example.org/'><rdf:Description rdf:about='http://example.org/#us'><ex:property rdf:Resource='http://example.org/object'/></rdf:Description></rdf:RDF>";
	        var rdfXml2 = "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:ex='http://example.org/'><foo:Description rdf:about='http://example.org/#us' xmlns:foo='http://www.w3.org/1999/02/22-rdf-syntax-ns#'><ex:property rdf:Resource='http://example.org/object' xmlns:ex='http://example.org/'/></foo:Description></rdf:RDF>";
	        var rdfXml3 = "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:ex='http://example.org/'><Description     rdf:about='http://example.org/#us' xmlns='http://www.w3.org/1999/02/22-rdf-syntax-ns#'><ex:property rdf:Resource='http://example.org/object'/></Description></rdf:RDF>";

	        var parser = new RdfXmlParser();

	        var graph1 = new Graph();
	        graph1.LoadFromString(rdfXml1, parser);

	        var graph2 = new Graph();
	        graph2.LoadFromString(rdfXml2, parser);

	        var graph3 = new Graph();
	        graph3.LoadFromString(rdfXml3, parser);

	        var diff12 = graph1.Difference(graph2);
	        var diff13 = graph1.Difference(graph3);

	        Assert.True(diff12.AreEqual);
	        Assert.True(diff13.AreEqual);

        }
    }
}
