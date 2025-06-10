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
using System.IO;
using System.Linq;
using FluentAssertions;
using Xunit;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Parsing.Handlers;

[Collection("RdfServer")]
public class GraphHandlerTests
{
    private readonly RdfServerFixture _serverFixture;

    public GraphHandlerTests(RdfServerFixture serverFixture)
    {
        _serverFixture = serverFixture;
    }

    [Fact]
    public void ParsingGraphHandlerImplicitTurtle()
    {
        var g = new Graph();
        EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");

        var formatter = new NTriplesFormatter();
        foreach (Triple t in g.Triples)
        {
            Console.WriteLine(t.ToString(formatter));
        }

        Assert.False(g.IsEmpty, "Graph should not be empty");
        Assert.True(g.NamespaceMap.HasNamespace("dnr"), "Graph should have the dnr: Namespace");
    }

    #region Explicit GraphHandler Usage

    protected void ParsingUsingGraphHandlerExplicitTest(String tempFile, IRdfReader parser, bool nsCheck)
    {
        var g = new Graph();
        EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");
        g.SaveToFile(tempFile);

        var h = new Graph();
        var handler = new GraphHandler(h);
        parser.Load(handler, tempFile);

        var formatter = new NTriplesFormatter();
        foreach (Triple t in h.Triples)
        {
            Console.WriteLine(t.ToString(formatter));
        }

        Assert.False(g.IsEmpty, "Graph should not be empty");
        Assert.True(g.NamespaceMap.HasNamespace("dnr"), "Graph should have the dnr: Namespace");
        Assert.False(h.IsEmpty, "Graph should not be empty");
        if (nsCheck) Assert.True(h.NamespaceMap.HasNamespace("dnr"), "Graph should have the dnr: Namespace");
        Assert.Equal(g, h);
    }

    [Fact]
    public void ParsingGraphHandlerExplicitNTriples()
    {
        ParsingUsingGraphHandlerExplicitTest("graph_handler_tests_temp.nt", new NTriplesParser(), false);
    }

    [Fact]
    public void ParsingGraphHandlerExplicitTurtle()
    {
        ParsingUsingGraphHandlerExplicitTest("graph_handler_tests_temp.ttl", new TurtleParser(), true);
    }

    [Fact]
    public void ParsingGraphHandlerExplicitNotation3()
    {
        ParsingUsingGraphHandlerExplicitTest("graph_handler_tests_temp.n3", new Notation3Parser(), true);
    }

    [Fact]
    public void ParsingGraphHandlerExplicitRdfA()
    {
        ParsingUsingGraphHandlerExplicitTest("graph_handler_tests_temp.html", new RdfAParser(), false);
    }

    [Fact]
    public void ParsingGraphHandlerExplicitRdfJson()
    {
        ParsingUsingGraphHandlerExplicitTest("graph_handler_tests_temp.json", new RdfJsonParser(), false);
    }

    #endregion

    [Fact]
    public void ParsingGraphHandlerExplicitMerging()
    {
        var g = new Graph();
        EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");
        g.SaveToFile("graph_handler_tests_temp.ttl");

        var h = new Graph();
        var handler = new GraphHandler(h);

        var parser = new TurtleParser();
        parser.Load(handler, "graph_handler_tests_temp.ttl");

        Assert.False(g.IsEmpty, "Graph should not be empty");
        Assert.True(g.NamespaceMap.HasNamespace("dnr"), "Graph should have the dnr: Namespace");
        Assert.False(h.IsEmpty, "Graph should not be empty");
        Assert.True(h.NamespaceMap.HasNamespace("dnr"), "Graph should have the dnr: Namespace");
        Assert.Equal(g, h);

        parser.Load(handler, "graph_handler_tests_temp.ttl");
        Assert.Equal(g.Triples.Count + 2, h.Triples.Count);
        Assert.NotEqual(g, h);

        var formatter = new NTriplesFormatter();
        foreach (Triple t in h.Triples.Where(x => !x.IsGroundTriple))
        {
            Console.WriteLine(t.ToString(formatter));
        }
    }

    [Fact]
    public void ParsingGraphHandlerImplicitMerging()
    {
        var g = new Graph();
        EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");
        g.SaveToFile("graph_handler_tests_temp.ttl");

        var h = new Graph();

        var parser = new TurtleParser();
        parser.Load(h, "graph_handler_tests_temp.ttl");

        Assert.False(g.IsEmpty, "Graph should not be empty");
        Assert.True(g.NamespaceMap.HasNamespace("dnr"), "Graph should have the dnr: Namespace");
        Assert.False(h.IsEmpty, "Graph should not be empty");
        Assert.True(h.NamespaceMap.HasNamespace("dnr"), "Graph should have the dnr: Namespace");
        Assert.Equal(g, h);

        parser.Load(h, "graph_handler_tests_temp.ttl");
        Assert.Equal(g.Triples.Count + 2, h.Triples.Count);
        Assert.NotEqual(g, h);

        var formatter = new NTriplesFormatter();
        foreach (Triple t in h.Triples.Where(x => !x.IsGroundTriple))
        {
            Console.WriteLine(t.ToString(formatter));
        }
    }

    [Fact]
    public void ParsingGraphHandlerImplicitInitialBaseUri()
    {
        var g = new Graph
        {
            BaseUri = new Uri("http://example.org/")
        };

        var fragment = "<subject> <predicate> <object> .";
        var parser = new TurtleParser();
        parser.Load(g, new StringReader(fragment));

        Assert.False(g.IsEmpty, "Graph should not be empty");
        Assert.Equal(1, g.Triples.Count);
    }

    [Fact]
    public void ParsingGraphHandlerExplicitInitialBaseUri()
    {
        var g = new Graph
        {
            BaseUri = new Uri("http://example.org/")
        };

        var fragment = "<subject> <predicate> <object> .";
        var parser = new TurtleParser();
        var handler = new GraphHandler(g);
        parser.Load(handler, new StringReader(fragment));

        Assert.False(g.IsEmpty, "Graph should not be empty");
        Assert.Equal(1, g.Triples.Count);
    }

    [Fact]
    public void ParsingGraphHandlerImplicitBaseUriPropagation()
    {
        var loader = new Loader(_serverFixture.Client);
        var g = new Graph();
        g.BaseUri.Should().BeNull();
        Uri targetUri = _serverFixture.UriFor("/one.ttl");
        loader.LoadGraph(g, targetUri);
        g.BaseUri.Should().Be(targetUri);
    }

    [Fact]
    public void ParsingGraphHandlerImplicitBaseUriPropogation2()
    {
        var loader = new Loader(_serverFixture.Client);
        var g = new Graph();
        Uri targetUri = _serverFixture.UriFor("/one.ttl");
        g.BaseUri.Should().BeNull();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        g.BaseUri.Should().NotBeNull("loading into a graph with no BaseUri should set the BaseUri");
        Uri baseUri = g.BaseUri;
        loader.LoadGraph(g, targetUri);
        g.BaseUri.Should().Be(baseUri, "merging a second graph should not overwrite the BaseUri");
    }

    [Fact]
    public void ParsingGraphHandlerExplicitRdfXml()
    {
        ParsingUsingGraphHandlerExplicitTest("graph_handler_tests_temp.rdf", new RdfXmlParser(), true);
    }
}
