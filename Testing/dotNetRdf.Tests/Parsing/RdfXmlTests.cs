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
using System.IO;
using System.Linq;
using Xunit;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;
using System.Xml;
using StringWriter = VDS.RDF.Writing.StringWriter;

namespace VDS.RDF.Parsing;

	public class RdfXmlTests
	{
    [Fact]
    public void ParsingRdfXmlAmpersands()
    {
        var writers = new List<IRdfWriter>()
        {
            new RdfXmlWriter(),
            new PrettyRdfXmlWriter()
        };
        IRdfReader parser = new RdfXmlParser();

        var g = new Graph
        {
            BaseUri = new Uri("http://example.org/ampersandsInRdfXml")
        };
        g.Assert(new Triple(g.CreateUriNode(), g.CreateUriNode(new Uri("http://example.org/property")), g.CreateUriNode(new Uri("http://example.org/a&b"))));
        g.Assert(new Triple(g.CreateUriNode(), g.CreateUriNode(new Uri("http://example.org/property")), g.CreateLiteralNode("A & B")));

        foreach (IRdfWriter writer in writers)
        {
                Console.WriteLine(writer.GetType().ToString());
                var temp = StringWriter.Write(g, writer);
                Console.WriteLine(temp);
                var h = new Graph();
                StringParser.Parse(h, temp);
                Assert.Equal(g, h);
                Console.WriteLine();
        }
    }

    private void TestRdfXmlSequence(IRdfReader parser, String file)
    {
        var g = new Graph();
        if (parser is ITraceableParser)
        {
            ((ITraceableParser)parser).TraceParsing = true;
        }
        parser.Load(g, file);

        TestTools.ShowGraph(g);
    }

    [Theory]
    [InlineData(RdfXmlParserMode.DOM, "sequence.rdf")]
    [InlineData(RdfXmlParserMode.Streaming, "sequence.rdf")]
    [InlineData(RdfXmlParserMode.DOM, "sequence2.rdf")]
    [InlineData(RdfXmlParserMode.Streaming, "sequence2.rdf")]
    public void ParsingRdfXml(RdfXmlParserMode parsingMode, string path)
    {
        path = Path.Combine("resources", path);
        var parser = new RdfXmlParser(parsingMode);
        TestRdfXmlSequence(parser, path);
    }

    [Fact]
    public void ParsingRdfXmlPropertyInDefaultNamespaceBad()
    {
        var g = new Graph();
        var parser = new RdfXmlParser();

        Assert.Throws<RdfParseException>(() => g.LoadFromFile(Path.Combine("resources", "rdfxml-bad-property.rdf"), parser));
    }

    [Fact]
    public void ParsingRdfXmlPropertyInDefaultNamespaceGood()
    {
        var g = new Graph();
        var parser = new RdfXmlParser();
        g.LoadFromFile(Path.Combine("resources", "rdfxml-good-property.rdf"), parser);

        Assert.False(g.IsEmpty);
        Assert.Equal(1, g.Triples.Count);

        var property = g.Triples.First().Predicate as IUriNode;
        Assert.Equal("default", property.Uri.Host);
        Assert.Equal("good", property.Uri.Segments[1]);
    }

    [Fact]
    public void ParsingRdfXmlElementUsesXmlNamespaceDom()
    {
        var g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "xml-prop.rdf"), new RdfXmlParser(RdfXmlParserMode.DOM));
        Assert.False(g.IsEmpty);
        Assert.Equal(1, g.Triples.Count);
    }

    [Fact]
    public void ParsingRdfXmlElementUsesXmlNamespaceStreaming()
    {
        var g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "xml-prop.rdf"), new RdfXmlParser(RdfXmlParserMode.Streaming));
        Assert.False(g.IsEmpty);
        Assert.Equal(1, g.Triples.Count);
    }

    [Fact]
    public void ParsingRdfXmlElementUsesUndeclaredNamespaceStreaming()
    {
        var g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "missing-namespace-declarations.rdf"), new RdfXmlParser(RdfXmlParserMode.Streaming));
        Assert.False(g.IsEmpty);
        Assert.Equal(9, g.Triples.Count);
    }

    [Fact]
    public void ParsingRdfXmlStreamingDoesNotExhaustMemory()
    {
        IGraph g = new Graph();
        var graphHandler = new GraphHandler(g);
        var paging = new PagingHandler(graphHandler, 1000);
        var counter = new CountHandler();
        var handler = new ChainedHandler(new IRdfHandler[] { paging, counter });

        var parser = new GZippedRdfXmlParser(RdfXmlParserMode.Streaming);
        parser.Load(handler, Path.Combine("resources", "oom.rdf.gz"));

        Assert.False(g.IsEmpty);
        Assert.Equal(1000, counter.Count);
        // Note that the source produces some duplicate triples so triples in the graph will be at most 1000
        Assert.True(g.Triples.Count <= 1000);
    }

    [Fact]
    public void ParsingRdfXmlStackOverflow1()
    {
        IGraph g = new Graph();
        var parser = new RdfXmlParser();
        parser.Load(g, Path.Combine("resources", "cogapp.rdf"));

        Assert.False(g.IsEmpty);
        Assert.Equal(9358, g.Triples.Count);
    }

    [Fact]
    public void ParsingRdfXmlStackOverflow2()
    {
        TestTools.RunAtDepth(100, ParsingRdfXmlStackOverflow1);
    }

    [Fact]
    public void ParsingRdfXmlStackOverflow3()
    {
        TestTools.RunAtDepth(1000, ParsingRdfXmlStackOverflow1);
    }

    [Fact(Skip="potentially risky test")]
    public void ParsingRdfXmlStackOverflow4()
    {
        TestTools.RunAtDepth(5000, ParsingRdfXmlStackOverflow1);
    }

    [Fact]
    public void ParsingRdfXmlResetDefaultNamespace()
    {
        IGraph g = new Graph();
        var parser = new RdfXmlParser(RdfXmlParserMode.Streaming);
        parser.Load(g, Path.Combine("resources", "rdfxml-defaultns-scope.xml"));
        var resourceNode = g.CreateUriNode(UriFactory.Root.Create("http://example.org/thing/1"));
        var p1Node = g.CreateUriNode(UriFactory.Root.Create("http://example.org/ns/b#p1"));
        var p2Node = g.CreateUriNode(UriFactory.Root.Create("http://example.org/ns/a#p2"));
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
	        const string rdfXml1 = "<rdf:RDF xml:base='http://example.org#fragment' xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:eg='http://example.org/'><eg:type rdf:about='' /></rdf:RDF>";
	        const string rdfXml2 = "<rdf:RDF xml:base='http://example.org' xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:eg='http://example.org/'><eg:type rdf:about='' /></rdf:RDF>";

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
	        const string rdfXml1 = "<RDF xmlns='http://www.w3.org/1999/02/22-rdf-syntax-ns#'><Seq><li>bar</li></Seq></RDF>";
	        const string rdfXml2 = "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#'><rdf:Seq><rdf:li>bar</rdf:li></rdf:Seq></rdf:RDF>";
	        const string rdfXml3 = "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#'><rdf:Seq><foo:li xmlns:foo='http://www.w3.org/1999/02/22-rdf-syntax-ns#'>bar</foo:li></rdf:Seq></rdf:RDF>";

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
    public void ParsingRdfXmlEmptyStrings()
    {
        var formatter = new NTriplesFormatter();
        var domParser = new RdfXmlParser(RdfXmlParserMode.DOM);
        var g = new Graph();
        domParser.Load(g, Path.Combine("resources", "empty-string-rdfxml.rdf"));

        Console.WriteLine("DOM Parser parsed OK");

        foreach (Triple t in g.Triples)
        {
            Console.WriteLine(t.ToString(formatter));
        }
        Console.WriteLine();

        var streamingParser = new RdfXmlParser(RdfXmlParserMode.Streaming);
        var h = new Graph();
        streamingParser.Load(h, Path.Combine("resources", "empty-string-rdfxml.rdf"));

        Console.WriteLine("Streaming Parser parsed OK");

        foreach (Triple t in h.Triples)
        {
            Console.WriteLine(t.ToString(formatter));
        }

        Assert.Equal(g, h);
    }

    [Fact]
    public void ParsingRdfXmlWithUrlEscapedNodes()
    {
        //Originally submitted by Rob Styles as part of CORE-251, modified somewhat during debugging process
        var formatter = new NTriplesFormatter();
        var domParser = new RdfXmlParser(RdfXmlParserMode.DOM);
        var g = new Graph();
        domParser.Load(g, Path.Combine("resources", "urlencodes-in-rdfxml.rdf"));

        foreach (Triple t in g.Triples)
        {
            Console.WriteLine(t.ToString(formatter));
        }

        var encoded = new Uri("http://example.com/some%40encoded%2FUri");
        var unencoded = new Uri("http://example.com/some@encoded/Uri");

        Assert.False(EqualityHelper.AreUrisEqual(encoded, unencoded), "URIs should not be equivalent because %40 encodes a reserved character and per RFC 3986 decoding this can change the meaning of the URI");

        IUriNode encodedNode = g.GetUriNode(encoded);
        Assert.NotNull(encodedNode);
        IUriNode unencodedNode = g.GetUriNode(unencoded);
        Assert.NotNull(unencodedNode);

        IUriNode pred = g.CreateUriNode(new Uri("http://example.org/schema/encoded"));
        Assert.True(g.ContainsTriple(new Triple(encodedNode, pred, g.CreateLiteralNode("true"))), "The encoded node should have the property 'true' from the file");
        Assert.True(g.ContainsTriple(new Triple(unencodedNode, pred, g.CreateLiteralNode("false"))), "The unencoded node should have the property 'false' from the file");
    }

    [Fact]
    public void ParsingRdfXmlWithUrlEscapedNodes2()
    {
        //Originally submitted by Rob Styles as part of CORE-251, modified somewhat during debugging process
        var formatter = new NTriplesFormatter();
        var domParser = new RdfXmlParser(RdfXmlParserMode.DOM);
        var g = new Graph();
        domParser.Load(g, Path.Combine("resources", "urlencodes-in-rdfxml.rdf"));

        var encoded = new Uri("http://example.com/some%20encoded%2FUri");
        var unencoded = new Uri("http://example.com/some encoded/Uri");

        IUriNode encodedNode = g.GetUriNode(encoded);
        Assert.NotNull(encodedNode);
        IUriNode unencodedNode = g.GetUriNode(unencoded);
        Assert.NotNull(unencodedNode);

        IUriNode pred = g.CreateUriNode(new Uri("http://example.org/schema/encoded"));
        Assert.True(g.ContainsTriple(new Triple(encodedNode, pred, g.CreateLiteralNode("true"))), "The encoded node should have the property 'true' from the file");
        Assert.True(g.ContainsTriple(new Triple(unencodedNode, pred, g.CreateLiteralNode("false"))), "The unencoded node should have the property 'false' from the file");

    }

    [Fact]
    public void ParsingRdfXmlElementUsesUndeclaredNamespaceDom()
    {
        var g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "missing-namespace-declarations.rdf"), new RdfXmlParser(RdfXmlParserMode.DOM));
        Assert.False(g.IsEmpty);
        Assert.Equal(9, g.Triples.Count);
    }


    [Theory]
    [InlineData(RdfXmlParserMode.Streaming)]
    [InlineData(RdfXmlParserMode.DOM)]
	    public void ItHandlesRdfTypeAttributesRegardlessOfNamespacePrefix(RdfXmlParserMode parserMode)
	    {
	        const string rdfXml1 = "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:eg='http://example.org/'><eg:Example rdf:about='http://example.org/#us'><rdf:type rdf:Resource='http://example.org/SomeType'/></eg:Example></rdf:RDF>";
	        const string rdfXml2 = "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:eg='http://example.org/'><eg:Example rdf:about='http://example.org/#us'><eg:type  rdf:Resource='http://example.org/SomeType' xmlns:eg='http://www.w3.org/1999/02/22-rdf-syntax-ns#'/></eg:Example></rdf:RDF>";
	        const string rdfXml3 = "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:eg='http://example.org/'><eg:Example rdf:about='http://example.org/#us'><type     rdf:Resource='http://example.org/SomeType' xmlns='http://www.w3.org/1999/02/22-rdf-syntax-ns#' /></eg:Example></rdf:RDF>";

        var parser = new RdfXmlParser(parserMode);

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

	    [Theory]
	    [InlineData(RdfXmlParserMode.Streaming)]
	    [InlineData(RdfXmlParserMode.DOM)]
    public void ItHandlesRdfParseTypeLiteralRegardlessOfNamespacePrefix(RdfXmlParserMode parserMode)
    {
        const string rdfXml1 = "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:eg='http://example.org/'><eg:Example rdf:about='http://example.org/#us'><eg:prop rdf:parseType='Literal'/><eg:Value /></eg:Example></rdf:RDF>";
        const string rdfXml2 = "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:eg='http://example.org/'><eg:Example rdf:about='http://example.org/#us'><eg:prop  xx:parseType='Literal' xmlns:xx='http://www.w3.org/1999/02/22-rdf-syntax-ns#'/><eg:Value /></eg:Example></rdf:RDF>";

        var parser = new RdfXmlParser(parserMode);

        var graph1 = new Graph();
        graph1.LoadFromString(rdfXml1, parser);

        var graph2 = new Graph();
        graph2.LoadFromString(rdfXml2, parser);

        var diff12 = graph1.Difference(graph2);

        Assert.True(diff12.AreEqual);
    }

	    [Theory]
	    [InlineData(RdfXmlParserMode.Streaming)]
	    [InlineData(RdfXmlParserMode.DOM)]
	    public void ItHandlesRdfParseTypeResourceRegardlessOfNamespacePrefix(RdfXmlParserMode parserMode)
	    {
	        const string rdfXml1 = "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:eg='http://example.org/'><rdf:Description rdf:about='http://example.org/#us'><eg:prop rdf:parseType='Resource'><eg:value>ABC</eg:value></eg:prop></rdf:Description></rdf:RDF>";
	        const string rdfXml2 = "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:eg='http://example.org/'><rdf:Description rdf:about='http://example.org/#us'><eg:prop  xx:parseType='Resource' xmlns:xx='http://www.w3.org/1999/02/22-rdf-syntax-ns#'><eg:value>ABC</eg:value></eg:prop></rdf:Description></rdf:RDF>";
	        var expectGraph = new Graph();
	        var bnode = expectGraph.CreateBlankNode();
	        expectGraph.Assert(
	            expectGraph.CreateUriNode(new Uri("http://example.org/#us")),
	            expectGraph.CreateUriNode(new Uri("http://example.org/prop")),
	            bnode);
	        expectGraph.Assert(
	            bnode,
	            expectGraph.CreateUriNode(new Uri("http://example.org/value")),
	            expectGraph.CreateLiteralNode("ABC"));
        AssertGraphsAreEquivalent(parserMode, expectGraph, rdfXml1, rdfXml2);
	    }


	    [Theory]
	    [InlineData(RdfXmlParserMode.Streaming)]
	    [InlineData(RdfXmlParserMode.DOM)]
	    public void ItHandlesRdfParseTypeCollectionRegardlessOfNamespacePrefix(RdfXmlParserMode parserMode)
	    {
	        const string rdfXml1 = "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:eg='http://example.org/'><rdf:Description rdf:about='http://example.org/#us'><eg:prop rdf:parseType='Collection'><rdf:Description rdf:about='http://example.org/1'/><rdf:Description rdf:about='http://example.org/2'/></eg:prop></rdf:Description></rdf:RDF>";
	        const string rdfXml2 = "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:eg='http://example.org/'><rdf:Description rdf:about='http://example.org/#us'><eg:prop  xx:parseType='Collection' xmlns:xx='http://www.w3.org/1999/02/22-rdf-syntax-ns#'><rdf:Description rdf:about='http://example.org/1'/><rdf:Description rdf:about='http://example.org/2'/></eg:prop></rdf:Description></rdf:RDF>";
	        var expectGraph = new Graph();
	        var listNode1 = expectGraph.CreateBlankNode();
	        var listNode2 = expectGraph.CreateBlankNode();
	        var first = expectGraph.CreateUriNode(new Uri(NamespaceMapper.RDF + "first"));
	        var rest = expectGraph.CreateUriNode(new Uri(NamespaceMapper.RDF + "rest"));
	        var nil = expectGraph.CreateUriNode(new Uri(NamespaceMapper.RDF + "nil"));
        expectGraph.Assert(
	            expectGraph.CreateUriNode(new Uri("http://example.org/#us")),
	            expectGraph.CreateUriNode(new Uri("http://example.org/prop")),
	            listNode1);
	        expectGraph.Assert(listNode1, first, expectGraph.CreateUriNode(new Uri("http://example.org/1")));
        expectGraph.Assert(listNode1, rest, listNode2);
	        expectGraph.Assert(listNode2, first, expectGraph.CreateUriNode(new Uri("http://example.org/2")));
	        expectGraph.Assert(listNode2, rest, nil);
	        AssertGraphsAreEquivalent(parserMode, expectGraph, rdfXml1, rdfXml2);
	    }

    [Theory]
    [InlineData(RdfXmlParserMode.Streaming)]
    [InlineData(RdfXmlParserMode.DOM)]
    public void ItHandlesRdfLiRegardlessOfNamespacePrefix(RdfXmlParserMode parserMode)
    {
        const string rdfXml1 = "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:eg='http://example.org/'><rdf:Seq rdf:about='http://example.org/#us'><rdf:li rdf:resource='http://example.org/1'/><rdf:li rdf:resource='http://example.org/2'/></rdf:Seq></rdf:RDF>";
        const string rdfXml2 = "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:eg='http://example.org/'><xx:Seq rdf:about='http://example.org/#us' xmlns:xx='http://www.w3.org/1999/02/22-rdf-syntax-ns#'><xx:li rdf:resource='http://example.org/1'/><xx:li rdf:resource='http://example.org/2'/></xx:Seq></rdf:RDF>";
        const string rdfXml3 = "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:eg='http://example.org/'><Seq rdf:about='http://example.org/#us' xmlns='http://www.w3.org/1999/02/22-rdf-syntax-ns#'><li rdf:resource='http://example.org/1'/><li rdf:resource='http://example.org/2'/></Seq></rdf:RDF>";
        var expectGraph = new Graph();
        var type = expectGraph.CreateUriNode(new Uri(NamespaceMapper.RDF + "type"));
        var first = expectGraph.CreateUriNode(new Uri(NamespaceMapper.RDF + "_1"));
        var second = expectGraph.CreateUriNode(new Uri(NamespaceMapper.RDF + "_2"));
        var seq = expectGraph.CreateUriNode(new Uri(NamespaceMapper.RDF + "Seq"));
        var coll = expectGraph.CreateUriNode(new Uri("http://example.org/#us"));
        expectGraph.Assert(coll, type, seq);
        expectGraph.Assert(coll, first, expectGraph.CreateUriNode(new Uri("http://example.org/1")));
        expectGraph.Assert(coll, second, expectGraph.CreateUriNode(new Uri("http://example.org/2")));
        AssertGraphsAreEquivalent(parserMode, expectGraph, rdfXml1, rdfXml2, rdfXml3);
    }

    [Theory]
	    [InlineData(RdfXmlParserMode.Streaming)]
	    [InlineData(RdfXmlParserMode.DOM)]
    public void ItHandlesRdfDescriptionAndRdfAboutRegardlessOfNamespacePrefix(RdfXmlParserMode parserMode)
	    {
	        const string rdfXml1 = "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:ex='http://example.org/'><rdf:Description rdf:about='http://example.org/#us'><ex:property rdf:resource='http://example.org/object'/></rdf:Description></rdf:RDF>";
	        const string rdfXml2 = "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:ex='http://example.org/'><foo:Description foo:about='http://example.org/#us' xmlns:foo='http://www.w3.org/1999/02/22-rdf-syntax-ns#'><ex:property rdf:resource='http://example.org/object' xmlns:ex='http://example.org/'/></foo:Description></rdf:RDF>";
	        var expectGraph = new Graph();
	        expectGraph.Assert(
	            expectGraph.CreateUriNode(new Uri("http://example.org/#us")),
	            expectGraph.CreateUriNode(new Uri("http://example.org/property")),
	            expectGraph.CreateUriNode(new Uri("http://example.org/object")));
        AssertGraphsAreEquivalent(parserMode, expectGraph, rdfXml1, rdfXml2);
    }

	    [Theory]
	    [InlineData(RdfXmlParserMode.Streaming)]
	    [InlineData(RdfXmlParserMode.DOM)]
	    public void ItHandlesRdfDatatypeAttributeRegardlessOfNamespacePrefix(RdfXmlParserMode parserMode)
	    {
	        const string rdfXml1 = "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:ex='http://example.org/'><rdf:Description rdf:about='http://example.org/#us'><ex:property rdf:datatype='http://www.w3.org/2001/XMLSchema#int'>123</ex:property></rdf:Description></rdf:RDF>";
	        const string rdfXml2 = "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:ex='http://example.org/'><rdf:Description rdf:about='http://example.org/#us'><ex:property foo:datatype='http://www.w3.org/2001/XMLSchema#int' xmlns:foo='http://www.w3.org/1999/02/22-rdf-syntax-ns#'>123</ex:property></rdf:Description></rdf:RDF>";
	        const string rdfXml3 = "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:ex='http://example.org/'><rdf:Description rdf:about='http://example.org/#us'><ex:property datatype='http://www.w3.org/2001/XMLSchema#int' xmlns='http://www.w3.org/1999/02/22-rdf-syntax-ns#'>123</ex:property></rdf:Description></rdf:RDF>";
	        var expectGraph = new Graph();
	        expectGraph.Assert(
	            expectGraph.CreateUriNode(new Uri("http://example.org/#us")),
	            expectGraph.CreateUriNode(new Uri("http://example.org/property")),
	            expectGraph.CreateLiteralNode("123", new Uri("http://www.w3.org/2001/XMLSchema#int")));
	        AssertGraphsAreEquivalent(parserMode, expectGraph, rdfXml1, rdfXml2, rdfXml3);
	    }

	    [Theory]
	    [InlineData(RdfXmlParserMode.Streaming)]
	    [InlineData(RdfXmlParserMode.DOM)]
	    public void ItHandlesRdfNodeIdAttributeRegardlessOfNamespacePrefix(RdfXmlParserMode parserMode)
	    {
	        const string rdfXml1 =
	            "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:ex='http://example.org/'><rdf:Description rdf:nodeID='abc' ex:fullName='John Smith'/></rdf:RDF>";
	        const string rdfXml2 =
            "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:ex='http://example.org/'><foo:Description foo:nodeID='abc' ex:fullName='John Smith' xmlns:foo='http://www.w3.org/1999/02/22-rdf-syntax-ns#'/></rdf:RDF>";
	        const string rdfXml3 =
	            "<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:ex='http://example.org/'><Description nodeID='abc' ex:fullName='John Smith' xmlns='http://www.w3.org/1999/02/22-rdf-syntax-ns#'/></rdf:RDF>";

	        var expectGraph = new Graph();
	        expectGraph.Assert(
	            expectGraph.CreateBlankNode("abc"),
	            expectGraph.CreateUriNode(new Uri("http://example.org/fullName")),
	            expectGraph.CreateLiteralNode("John Smith"));
        AssertGraphsAreEquivalent(parserMode, expectGraph, rdfXml1, rdfXml2, rdfXml3);
	    }


    private void AssertGraphsAreEquivalent(RdfXmlParserMode parserMode, IGraph expectGraph, params string[] testRdfXmlGraphs )
	    {
	        var parser = new RdfXmlParser(parserMode);
	        for (var i = 0; i < testRdfXmlGraphs.Length; i++)
	        {
            var testGraph = new Graph();
            testGraph.LoadFromString(testRdfXmlGraphs[i], parser);
	            var diff = expectGraph.Difference(testGraph);
            Assert.True(diff.AreEqual, "Expected test graph #" + i + " to match expect graph but found differences.");
	        }
	    }

    [Theory]
    [InlineData(RdfXmlParserMode.Streaming)]
    [InlineData(RdfXmlParserMode.DOM)]
    public void EntityParsingCanBeDisabled(RdfXmlParserMode mode)
    {
        var parser = new RdfXmlParser(mode);
        const string rdfWithEntities = "<!DOCTYPE rdf:RDF [ <!ENTITY foo 'abc'> ]><rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:ex='http://example.org/'><rdf:Description rdf:nodeID='&foo;' ex:fullName='John Smith'/></rdf:RDF>";
        parser.XmlReaderSettings.DtdProcessing = System.Xml.DtdProcessing.Prohibit;
        var thrown = Assert.Throws<RdfParseException>(() =>
        {
            var g = new Graph();
            g.LoadFromString(rdfWithEntities, parser);
        });
        Assert.IsType<XmlException>(thrown.InnerException);
    }
}
