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
using Xunit;

namespace VDS.RDF;


public class GraphCollectionTests
{
    [Fact]
    public void GraphCollectionBasic1()
    {
        var store = new GraphCollection();
        var g = new Graph();
        store.Add(g, false);

        Assert.True(store.Contains(g.Name), "Graph Collection should contain the Graph");
        Assert.Equal(g, store[g.Name]);
    }

    [Fact]
    public void GraphCollectionAddUnnamedGraph()
    {
        var collection = new GraphCollection();
        var g = new Graph();
        collection.Add(g, false);
        Assert.True(collection.Contains((IRefNode)null));
    }

    [Fact]
    public void GraphCollectionBasic2()
    {
        var store = new TripleStore();
        var g = new Graph(new UriNode(new Uri("http://example.org/graph")));
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        store.Add(g);

        Assert.True(store.HasGraph(g.Name), "Graph Collection should contain the Graph");
        Assert.Equal(g, store[g.Name]);
    }

    [Fact]
    public void GraphCollectionBasic3()
    {
        var collection = new GraphCollection();
        var g = new Graph();
        collection.Add(g, true);

        Assert.True(collection.Contains(g.Name));
    }

    [Fact]
    public void GraphCollectionBasic4()
    {
        var collection = new GraphCollection();
        var g = new Graph();
        collection.Add(g, true);

        Assert.True(collection.Contains(g.Name));
        Assert.Contains(null, collection.GraphNames);
    }

    [Fact]
    public void GraphCollectionDiskDemand1()
    {
        var store = new TripleStore(new DiskDemandGraphCollection());
        var g = new Graph(new UriNode(new Uri("file://" + Path.GetFullPath(Path.Combine("resources", "InferenceTest.ttl")))));
        g.LoadFromFile(Path.Combine("resources", "InferenceTest.ttl"));

        Assert.True(store.HasGraph(g.Name), "Graph Collection should contain the Graph");
        Assert.Equal(g, store[g.Name]);
    }

    [Fact]
    public void GraphCollectionDiskDemand2()
    {
        //Test that on-demand loading does not kick in for pre-existing graphs
        var store = new TripleStore(new DiskDemandGraphCollection());

        var g = new Graph(new UriNode(new Uri("file:///" + Path.GetFullPath(Path.Combine("resources", "InferenceTest.ttl")))));
        g.LoadFromFile(Path.Combine("resources", "InferenceTest.ttl"));

        var empty = new Graph(g.Name)
        {
            BaseUri = g.BaseUri
        };
        store.Add(empty);

        Assert.True(store.HasGraph(g.Name), "Graph Collection should contain the Graph");
        Assert.NotEqual(g, store[g.Name]);
    }

    [Fact(Skip="Remote configuration file is not currently available")]
    public void GraphCollectionWebDemand1()
    {
        var store = new TripleStore(new WebDemandGraphCollection());
        var u = new Uri("http://www.dotnetrdf.org/configuration#");
        var nodeFactory = new NodeFactory(new NodeFactoryOptions {BaseUri = u});
        var g = new Graph(nodeFactory.CreateUriNode(u));
        g.LoadFromUri(u);

        Assert.True(store.HasGraph(g.Name), "Graph Collection should contain the Graph");
        Assert.Equal(g, store[g.Name]);
    }

    [Fact(Skip = "Remote configuration file is not currently available")]
    public void GraphCollectionWebDemand2()
    {
        //Test that on-demand loading does not kick in for pre-existing graphs
        var store = new TripleStore(new WebDemandGraphCollection());
        var nodeFactory = new NodeFactory(new NodeFactoryOptions());
        var u = new Uri("http://www.dotnetrdf.org/configuration#");
        var n = nodeFactory.CreateUriNode(u);
        var g = new Graph(n);
        g.LoadFromUri(u);

        var empty = new Graph(n);
        store.Add(empty);

        Assert.True(store.HasGraph(g.Name), "Graph Collection should contain the Graph");
        Assert.NotEqual(g, store[g.Name]);
    }
}
