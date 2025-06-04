using FluentAssertions;
using System;
using VDS.RDF.Parsing;
using Xunit;

namespace VDS.RDF.Writing;

public class NQuadsTests
{
    [Fact]
    public void WritingNQuadsStar1()
    {
        var g = new Graph(new UriNode(new Uri("http://example.org/g"))); 
        g.NamespaceMap.AddNamespace("", new Uri("http://example.org/"));
        g.Assert(new Triple(
            g.CreateUriNode(":s"),
            g.CreateUriNode(":p"),
            g.CreateTripleNode(
                new Triple(g.CreateUriNode(":a"),
                    g.CreateUriNode(":b"),
                    g.CreateUriNode(":c")))));
        var store = new TripleStore();
        store.Add(g);
        var writer = new NQuadsWriter(NQuadsSyntax.Rdf11Star);
        var strWriter = new System.IO.StringWriter();
        writer.Save(store, strWriter);
        strWriter.ToString().Should().Be(
            "<http://example.org/s> <http://example.org/p> << <http://example.org/a> <http://example.org/b> <http://example.org/c> >> <http://example.org/g> ." + Environment.NewLine);
    }

    [Fact]
    public void WritingNQuadsStar2()
    {
        var g = new Graph(new UriNode(new Uri("http://example.org/g")));
        g.NamespaceMap.AddNamespace("", new Uri("http://example.org/"));
        g.Assert(new Triple(
            g.CreateTripleNode(
                new Triple(g.CreateUriNode(":a"),
                    g.CreateUriNode(":b"),
                    g.CreateUriNode(":c"))),
            g.CreateUriNode(":p"),
            g.CreateUriNode(":o")
        ));
        var store = new TripleStore();
        store.Add(g);
        var writer = new NQuadsWriter(NQuadsSyntax.Rdf11Star);
        var strWriter = new System.IO.StringWriter();
        writer.Save(store, strWriter);
        strWriter.ToString().Should().Be(
            "<< <http://example.org/a> <http://example.org/b> <http://example.org/c> >> <http://example.org/p> <http://example.org/o> <http://example.org/g> ." + Environment.NewLine);
    }

    [Fact]
    public void WritingNQuadsStar3()
    {
        var g = new Graph(new UriNode(new Uri("http://example.org/g")));
        g.NamespaceMap.AddNamespace("", new Uri("http://example.org/"));
        g.Assert(new Triple(
            g.CreateTripleNode(
                new Triple(g.CreateUriNode(":a"),
                    g.CreateUriNode(":b"),
                    g.CreateUriNode(":c"))),
            g.CreateUriNode(":p"),
            g.CreateUriNode(":o")
        ));
        var store = new TripleStore();
        store.Add(g);
        var writer = new NQuadsWriter(NQuadsSyntax.Rdf11Star);
        var strWriter = new System.IO.StringWriter();
        writer.Save(store, strWriter);
        strWriter.ToString().Should().Be(
            "<< <http://example.org/a> <http://example.org/b> <http://example.org/c> >> <http://example.org/p> <http://example.org/o> <http://example.org/g> ." + Environment.NewLine);
    }

    [Fact]
    public void WritingNQuadsStar4()
    {
        var g = new Graph(new UriNode(new Uri("http://example.org/g")));
        g.NamespaceMap.AddNamespace("", new Uri("http://example.org/"));
        g.Assert(new Triple(
            g.CreateTripleNode(
                new Triple(g.CreateUriNode(":a"),
                    g.CreateUriNode(":b"),
                    g.CreateTripleNode(
                        new Triple(
                            g.CreateUriNode(":c"),
                            g.CreateUriNode(":d"),
                            g.CreateUriNode(":e")))
                    )),
            g.CreateUriNode(":p"),
            g.CreateUriNode(":o")
        ));
        var store = new TripleStore();
        store.Add(g);
        var writer = new NQuadsWriter(NQuadsSyntax.Rdf11Star);
        var strWriter = new System.IO.StringWriter();
        writer.Save(store, strWriter);
        strWriter.ToString().Should().Be(
            "<< <http://example.org/a> <http://example.org/b> << <http://example.org/c> <http://example.org/d> <http://example.org/e> >> >> <http://example.org/p> <http://example.org/o> <http://example.org/g> ." + Environment.NewLine);
    }

    [Fact]
    public void WritingNQuadsStar5()
    {
        var g = new Graph(new UriNode(new Uri("http://example.org/g")));
        g.NamespaceMap.AddNamespace("", new Uri("http://example.org/"));
        g.Assert(new Triple(
            g.CreateUriNode(":s"),
            g.CreateUriNode(":p"),
            g.CreateTripleNode(
                new Triple(g.CreateUriNode(":a"),
                    g.CreateUriNode(":b"),
                    g.CreateUriNode(":c")))));
        var store = new TripleStore();
        store.Add(g);
        var writer = new NQuadsWriter(NQuadsSyntax.Rdf11);
        var strWriter = new System.IO.StringWriter();
        RdfOutputException ex = Assert.Throws<RdfOutputException>(() => writer.Save(store, strWriter));
        ex.Message.Should().Be(WriterErrorMessages.TripleNodesUnserializable("NQuads (RDF 1.1)"));
    }
}
