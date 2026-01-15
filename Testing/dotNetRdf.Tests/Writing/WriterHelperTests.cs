using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Contexts;
using Xunit;

namespace VDS.RDF.Writing;

public class WriterHelperTests
{
    private Dictionary<INode, OutputRdfCollection> FindImplicitCollections(IGraph g)
    {
        var sw = new System.IO.StringWriter();
        var context = new CompressingTurtleWriterContext(g, sw);
        WriterHelper.FindCollections(context, CollectionSearchMode.ImplicitOnly);
        return context.Collections;
    }

    [Fact]
    public void FindCollectionsIgnoresNodeWithNoRdfFirst()
    {
        var g= new Graph();
        var rdfFirst = g.CreateUriNode("rdf:first");
        var rdfRest = g.CreateUriNode("rdf:rest");
        var rdfNil = g.CreateUriNode("rdf:nil");
        var listRoot = g.CreateBlankNode();
        var listNode = g.CreateBlankNode();
        g.Assert(listRoot, rdfFirst, g.CreateLiteralNode("first"));
        g.Assert(listRoot, rdfRest, listNode);
        g.Assert(listNode, rdfRest, rdfNil);

        var collections = FindImplicitCollections(g);
        Assert.Empty(collections);
    }

    [Fact]
    public void FindCollectionsIgnoresRootWithNoRdfFirst()
    {
        var g = new Graph();
        var rdfFirst = g.CreateUriNode("rdf:first");
        var rdfRest = g.CreateUriNode("rdf:rest");
        var rdfNil = g.CreateUriNode("rdf:nil");
        var listRoot = g.CreateBlankNode("root");
        var listNode = g.CreateBlankNode("node");
        g.Assert(listRoot, rdfRest, listNode);
        g.Assert(listNode, rdfFirst, g.CreateLiteralNode("first"));
        g.Assert(listNode, rdfRest, rdfNil);

        var collections = FindImplicitCollections(g);
        Assert.Empty(collections);
    }

    [Fact]
    public void FindCollectionsIgnoresRootWithMultipleRdfFirst()
    {
        var g = new Graph();
        g.NamespaceMap.AddNamespace("ex", UriFactory.Create("http://example.org/"));
        var rdfFirst = g.CreateUriNode("rdf:first");
        var rdfRest = g.CreateUriNode("rdf:rest");
        var rdfNil = g.CreateUriNode("rdf:nil");
        var listRoot = g.CreateBlankNode("root");
        var listNode = g.CreateBlankNode("node");

        g.Assert(g.CreateUriNode("ex:s"), g.CreateUriNode("ex:p"), listRoot);
        g.Assert(listRoot, rdfFirst, g.CreateLiteralNode("first"));
        g.Assert(listRoot, rdfFirst, g.CreateLiteralNode("another first"));
        g.Assert(listRoot, rdfRest, listNode);
        g.Assert(listNode, rdfFirst, g.CreateLiteralNode("second"));
        g.Assert(listNode, rdfRest, rdfNil);

        var collections = FindImplicitCollections(g);
        Assert.Empty(collections);
    }

    [Fact]
    public void FindCollectionsIgnoresNodeWithMultipleRdfFirst()
    {
        var g = new Graph();
        g.NamespaceMap.AddNamespace("ex", UriFactory.Create("http://example.org/"));
        var rdfFirst = g.CreateUriNode("rdf:first");
        var rdfRest = g.CreateUriNode("rdf:rest");
        var rdfNil = g.CreateUriNode("rdf:nil");
        var listRoot = g.CreateBlankNode("root");
        var listNode = g.CreateBlankNode("node");

        g.Assert(g.CreateUriNode("ex:s"), g.CreateUriNode("ex:p"), listRoot);
        g.Assert(listRoot, rdfFirst, g.CreateLiteralNode("first"));
        g.Assert(listRoot, rdfRest, listNode);
        g.Assert(listNode, rdfFirst, g.CreateLiteralNode("second"));
        g.Assert(listNode, rdfFirst, g.CreateLiteralNode("another second"));
        g.Assert(listNode, rdfRest, rdfNil);

        var collections = FindImplicitCollections(g);
        Assert.Empty(collections);
    }

    [Fact]
    public void FindCollectionsIgnoresUnterminatedList()
    {
        var g = new Graph();
        var rdfFirst = g.CreateUriNode("rdf:first");
        var rdfRest = g.CreateUriNode("rdf:rest");
        var l = g.CreateBlankNode();
        var m = g.CreateBlankNode();
        g.Assert(l, rdfFirst, g.CreateLiteralNode("first"));
        g.Assert(l, rdfRest, m);
        g.Assert(m, rdfFirst, g.CreateLiteralNode("second"));

        var collections = FindImplicitCollections(g);
        Assert.Empty(collections);
    }

    [Fact]
    public void Issue519ListCompression()
    {
        var g = new Graph();
        g.LoadFromString("""
                         @prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .
                         @prefix sh: <http://www.w3.org/ns/shacl#> .
                         _:autos1 rdf:first <urn:a>;
                           rdf:rest _:autos2.
                         _:autos2 rdf:first <urn:b>;
                            rdf:rest _:autos3.
                         _:autos3 rdf:first <urn:c>;
                            rdf:rest rdf:nil.
                         <urn:X> a sh:PropertyShape;
                            sh:in _:autos1.
                         """, new TurtleParser(TurtleSyntax.W3C, false));
        var collections = FindImplicitCollections(g);
        Assert.Single(collections);
        var collection = collections.Values.First();
        Assert.False(collection.IsExplicit);
        Assert.Equal(3, collection.Triples.Count);
    }
}
