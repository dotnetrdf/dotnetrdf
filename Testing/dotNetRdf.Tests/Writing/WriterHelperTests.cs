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
        INode rdfFirst = g.CreateUriNode("rdf:first");
        INode rdfRest = g.CreateUriNode("rdf:rest");
        INode rdfNil = g.CreateUriNode("rdf:nil");
        INode listRoot = g.CreateBlankNode();
        INode listNode = g.CreateBlankNode();
        g.Assert(listRoot, rdfFirst, g.CreateLiteralNode("first"));
        g.Assert(listRoot, rdfRest, listNode);
        g.Assert(listNode, rdfRest, rdfNil);

        Dictionary<INode, OutputRdfCollection> collections = FindImplicitCollections(g);
        Assert.Empty(collections);
    }

    [Fact]
    public void FindCollectionsIgnoresRootWithNoRdfFirst()
    {
        var g = new Graph();
        INode rdfFirst = g.CreateUriNode("rdf:first");
        INode rdfRest = g.CreateUriNode("rdf:rest");
        INode rdfNil = g.CreateUriNode("rdf:nil");
        INode listRoot = g.CreateBlankNode("root");
        INode listNode = g.CreateBlankNode("node");
        g.Assert(listRoot, rdfRest, listNode);
        g.Assert(listNode, rdfFirst, g.CreateLiteralNode("first"));
        g.Assert(listNode, rdfRest, rdfNil);

        Dictionary<INode, OutputRdfCollection> collections = FindImplicitCollections(g);
        Assert.Empty(collections);
    }

    [Fact]
    public void FindCollectionsIgnoresRootWithMultipleRdfFirst()
    {
        var g = new Graph();
        g.NamespaceMap.AddNamespace("ex", UriFactory.Create("http://example.org/"));
        INode rdfFirst = g.CreateUriNode("rdf:first");
        INode rdfRest = g.CreateUriNode("rdf:rest");
        INode rdfNil = g.CreateUriNode("rdf:nil");
        INode listRoot = g.CreateBlankNode("root");
        INode listNode = g.CreateBlankNode("node");

        g.Assert(g.CreateUriNode("ex:s"), g.CreateUriNode("ex:p"), listRoot);
        g.Assert(listRoot, rdfFirst, g.CreateLiteralNode("first"));
        g.Assert(listRoot, rdfFirst, g.CreateLiteralNode("another first"));
        g.Assert(listRoot, rdfRest, listNode);
        g.Assert(listNode, rdfFirst, g.CreateLiteralNode("second"));
        g.Assert(listNode, rdfRest, rdfNil);

        Dictionary<INode, OutputRdfCollection> collections = FindImplicitCollections(g);
        Assert.Empty(collections);
    }

    [Fact]
    public void FindCollectionsIgnoresNodeWithMultipleRdfFirst()
    {
        var g = new Graph();
        g.NamespaceMap.AddNamespace("ex", UriFactory.Create("http://example.org/"));
        INode rdfFirst = g.CreateUriNode("rdf:first");
        INode rdfRest = g.CreateUriNode("rdf:rest");
        INode rdfNil = g.CreateUriNode("rdf:nil");
        INode listRoot = g.CreateBlankNode("root");
        INode listNode = g.CreateBlankNode("node");

        g.Assert(g.CreateUriNode("ex:s"), g.CreateUriNode("ex:p"), listRoot);
        g.Assert(listRoot, rdfFirst, g.CreateLiteralNode("first"));
        g.Assert(listRoot, rdfRest, listNode);
        g.Assert(listNode, rdfFirst, g.CreateLiteralNode("second"));
        g.Assert(listNode, rdfFirst, g.CreateLiteralNode("another second"));
        g.Assert(listNode, rdfRest, rdfNil);

        Dictionary<INode, OutputRdfCollection> collections = FindImplicitCollections(g);
        Assert.Empty(collections);
    }

    [Fact]
    public void FindCollectionsIgnoresUnterminatedList()
    {
        var g = new Graph();
        INode rdfFirst = g.CreateUriNode("rdf:first");
        INode rdfRest = g.CreateUriNode("rdf:rest");
        INode l = g.CreateBlankNode();
        INode m = g.CreateBlankNode();
        g.Assert(l, rdfFirst, g.CreateLiteralNode("first"));
        g.Assert(l, rdfRest, m);
        g.Assert(m, rdfFirst, g.CreateLiteralNode("second"));

        Dictionary<INode, OutputRdfCollection> collections = FindImplicitCollections(g);
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
        Dictionary<INode, OutputRdfCollection> collections = FindImplicitCollections(g);
        Assert.Single(collections);
        OutputRdfCollection collection = collections.Values.First();
        Assert.False(collection.IsExplicit);
        Assert.Equal(3, collection.Triples.Count);
    }
}
