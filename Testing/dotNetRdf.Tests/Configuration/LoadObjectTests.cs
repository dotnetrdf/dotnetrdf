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

using FluentAssertions;
using System;
using Xunit;
using VDS.RDF.Query.PropertyFunctions;

namespace VDS.RDF.Configuration;


public class LoadObjectTests
{
    [Fact]
    public void ConfigurationLoadObjectPropertyFunctionFactory()
    {
        var graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:SparqlPropertyFunctionFactory ;
  dnr:type """ + typeof(MockPropertyFunctionFactory).AssemblyQualifiedName + @""" .";

        var g = new Graph();
        g.LoadFromString(graph);

        var factory = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as IPropertyFunctionFactory;
        Assert.NotNull(factory);
        Assert.Equal(typeof(MockPropertyFunctionFactory), factory.GetType());
    }

    [Fact]
    public void ConfigurationLoadObjectTripleCollection1()
    {
        var graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:TripleCollection ;
  dnr:type ""VDS.RDF.TreeIndexedTripleCollection"" .";

        var g = new Graph();
        g.LoadFromString(graph);

        var collection = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as BaseTripleCollection;
        Assert.NotNull(collection);
        Assert.Equal(typeof(TreeIndexedTripleCollection), collection.GetType());
    }

    [Fact]
    public void ConfigurationLoadObjectTripleCollection2()
    {
        var graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:TripleCollection ;
  dnr:type ""VDS.RDF.ThreadSafeTripleCollection"" ;
  dnr:usingTripleCollection _:b .
_:b a dnr:TripleCollection ;
  dnr:type ""VDS.RDF.TreeIndexedTripleCollection"" .";

        var g = new Graph();
        g.LoadFromString(graph);

        var collection = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as BaseTripleCollection;
        Assert.NotNull(collection);
        Assert.Equal(typeof(ThreadSafeTripleCollection), collection.GetType());
    }

    [Fact]
    public void ConfigurationLoadObjectGraphCollection1()
    {
        var graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:GraphCollection ;
  dnr:type ""VDS.RDF.GraphCollection"" .";

        var g = new Graph();
        g.LoadFromString(graph);

        var collection = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as BaseGraphCollection;
        Assert.NotNull(collection);
        Assert.Equal(typeof(GraphCollection), collection.GetType());
    }

    [Fact]
    public void ConfigurationLoadObjectGraphCollection2()
    {
        var graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:GraphCollection ;
  dnr:type ""VDS.RDF.ThreadSafeGraphCollection"" ;
  dnr:usingGraphCollection _:b .
_:b a dnr:GraphCollection ;
  dnr:type ""VDS.RDF.GraphCollection"" .";

        var g = new Graph();
        g.LoadFromString(graph);

        var collection = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as BaseGraphCollection;
        Assert.NotNull(collection);
        Assert.Equal(typeof(ThreadSafeGraphCollection), collection.GetType());
    }

    [Fact]
    public void ConfigurationLoadObjectGraphCollection3()
    {
        var graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:GraphCollection ;
  dnr:type ""VDS.RDF.DiskDemandGraphCollection"" ;
  dnr:usingGraphCollection _:b .
_:b a dnr:GraphCollection ;
  dnr:type ""VDS.RDF.GraphCollection"" .";

        var g = new Graph();
        g.LoadFromString(graph);

        var collection = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as BaseGraphCollection;
        Assert.NotNull(collection);
        Assert.Equal(typeof(DiskDemandGraphCollection), collection.GetType());
    }

    [Fact]
    public void ConfigurationLoadObjectGraphCollection4()
    {
        var graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:GraphCollection ;
  dnr:type ""VDS.RDF.WebDemandGraphCollection"" ;
  dnr:usingGraphCollection _:b .
_:b a dnr:GraphCollection ;
  dnr:type ""VDS.RDF.GraphCollection"" .";

        var g = new Graph();
        g.LoadFromString(graph);

        var collection = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as BaseGraphCollection;
        Assert.NotNull(collection);
        Assert.Equal(typeof(WebDemandGraphCollection), collection.GetType());
    }

    [Fact]
    public void ConfigurationLoadObjectGraphCollection5()
    {
        var graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:GraphCollection ;
  dnr:type ""VDS.RDF.WebDemandGraphCollection"" ;
  dnr:usingGraphCollection _:b .
_:b a dnr:GraphCollection ;
  dnr:type ""VDS.RDF.ThreadSafeGraphCollection"" ;
  dnr:usingGraphCollection _:c .
_:c a dnr:GraphCollection ;
  dnr:type ""VDS.RDF.GraphCollection"" .";

        var g = new Graph();
        g.LoadFromString(graph);

        var collection = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as BaseGraphCollection;
        Assert.NotNull(collection);
        Assert.Equal(typeof(WebDemandGraphCollection), collection.GetType());
    }

    [Fact]
    public void ConfigurationLoadObjectGraphEmpty1()
    {
        var graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:Graph ;
  dnr:type ""VDS.RDF.Graph"" .";

        var g = new Graph();
        g.LoadFromString(graph);

        var result = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as IGraph;
        Assert.NotNull(result);
        Assert.Equal(typeof(Graph), result.GetType());
    }

    [Fact]
    public void ConfigurationLoadObjectGraphEmpty2()
    {
        var graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:Graph ;
  dnr:type ""VDS.RDF.ThreadSafeGraph"" .";

        var g = new Graph();
        g.LoadFromString(graph);

        var result = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as IGraph;
        Assert.NotNull(result);
        Assert.Equal(typeof(ThreadSafeGraph), result.GetType());
    }

    [Fact]
    public void ConfigurationLoadObjectGraphEmpty3()
    {
        var graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:Graph ;
  dnr:type ""VDS.RDF.Graph"" ;
  dnr:usingTripleCollection _:b .
_:b a dnr:TripleCollection ;
  dnr:type ""VDS.RDF.ThreadSafeTripleCollection"" .";

        var g = new Graph();
        g.LoadFromString(graph);

        var result = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as IGraph;
        Assert.NotNull(result);
        Assert.Equal(typeof(Graph), result.GetType());
        Assert.Equal(typeof(ThreadSafeTripleCollection), result.Triples.GetType());
    }

    [Fact]
    public void ConfigurationLoadNamedGraphEmpty()
    {
        var graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:Graph ;
  dnr:type ""VDS.RDF.Graph"" ;
  dnr:withName <http://example.org/graph> ;
  dnr:usingNodeFactory _:b ;
  dnr:usingUriFactory _:c .
_:b a dnr:NodeFactory ;
  dnr:type ""VDS.RDF.NodeFactory"" .
_:c a dnr:UriFactory ;
  dnr:type ""VDS.RDF.CachingUriFactory"" .";
        var g = new Graph();
        g.LoadFromString(graph);
        var result = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as IGraph;
        result.Should().BeAssignableTo<Graph>();
        Assert.NotNull(result);
        Assert.NotNull(result.Name);
        result.Name.Should().BeAssignableTo<IUriNode>().Which.Uri.ToString().Should()
            .Be("http://example.org/graph");
    }
    
    [Fact]
    public void ConfigurationLoadNamedThreadSafeGraphEmpty()
    {
        var graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:Graph ;
  dnr:type ""VDS.RDF.ThreadSafeGraph"" ;
  dnr:withName <http://example.org/graph> .";
        var g = new Graph();
        g.LoadFromString(graph);
        var result = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as IGraph;
        result.Should().BeAssignableTo<ThreadSafeGraph>();
        Assert.NotNull(result);
        Assert.NotNull(result.Name);
        result.Name.Should().BeAssignableTo<IUriNode>().Which.Uri.ToString().Should()
            .Be("http://example.org/graph");
    }

    [Fact]
    public void ConfigurationLoadNamedGraphThreadSafeGraphWithCollection()
    {
        var graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:Graph ;
  dnr:type ""VDS.RDF.ThreadSafeGraph"" ;
  dnr:withName <http://example.org/graph> ;
  dnr:usingTripleCollection _:b .
_:b a dnr:TripleCollection ;
  dnr:type ""VDS.RDF.ThreadSafeTripleCollection"" .";
        var g = new Graph();
        g.LoadFromString(graph);
        var result = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as IGraph;
        result.Should().BeAssignableTo<ThreadSafeGraph>();
        result.Triples.Should().BeAssignableTo<ThreadSafeTripleCollection>();
        Assert.NotNull(result);
        Assert.NotNull(result.Name);
        result.Name.Should().BeAssignableTo<IUriNode>().Which.Uri.ToString().Should()
            .Be("http://example.org/graph");
    }
    [Fact]
    public void ConfigurationLoadObjectTripleStoreEmpty1()
    {
        var graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:TripleStore ;
  dnr:type ""VDS.RDF.TripleStore"" .";

        var g = new Graph();
        g.LoadFromString(graph);

        var result = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as ITripleStore;
        Assert.NotNull(result);
        Assert.Equal(typeof(TripleStore), result.GetType());
    }

    [Fact]
    public void ConfigurationLoadObjectTripleStoreEmpty2()
    {
        var graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:TripleStore ;
  dnr:type ""VDS.RDF.WebDemandTripleStore"" .";

        var g = new Graph();
        g.LoadFromString(graph);

        var result = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as ITripleStore;
        Assert.NotNull(result);
        Assert.Equal(typeof(WebDemandTripleStore), result.GetType());
    }

    [Fact]
    public void ConfigurationLoadObjectTripleStoreEmpty3()
    {
        var graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:TripleStore ;
  dnr:type ""VDS.RDF.TripleStore"" ;
  dnr:usingGraphCollection _:b .
_:b a dnr:GraphCollection ;
  dnr:type ""VDS.RDF.ThreadSafeGraphCollection"" .";

        var g = new Graph();
        g.LoadFromString(graph);

        var result = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as ITripleStore;
        Assert.NotNull(result);
        Assert.Equal(typeof(TripleStore), result.GetType());
        Assert.Equal(typeof(ThreadSafeGraphCollection), result.Graphs.GetType());
    }

    [Fact]
    public void ConfigurationLoadThreadSafeTripleStoreEmpty()
    {
        var graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:TripleStore ;
    dnr:type ""VDS.RDF.ThreadSafeTripleStore"" ;
.";
        var g = new Graph();
        g.LoadFromString(graph);
        var result = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as ITripleStore;
        Assert.NotNull(result);
        result.Should().NotBeNull().And.BeAssignableTo<ThreadSafeTripleStore>();
        result.Graphs.Should().BeAssignableTo<ThreadSafeGraphCollection>();
    }
}

class MockPropertyFunctionFactory
    : IPropertyFunctionFactory
{
    public bool IsPropertyFunction(Uri u)
    {
        throw new NotImplementedException();
    }

    public bool TryCreatePropertyFunction(PropertyFunctionInfo info, out RDF.Query.Patterns.IPropertyFunctionPattern function)
    {
        throw new NotImplementedException();
    }
}
