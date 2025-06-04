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
using Xunit;

namespace VDS.RDF.Parsing.Handlers;


public class StoreHandlerTests
{

    private void EnsureTestData(string testFile)
    {
        var store = new TripleStore();
        var g = new Graph(new UriNode(new Uri("http://graphs/1")));
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        store.Add(g);
        var h = new Graph(new UriNode(new Uri("http://graphs/2")));
        h.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
        store.Add(h);
        store.SaveToFile(testFile);
    }

    [Fact]
    public void ParsingStoreHandlerBadInstantiation()
    {
        Assert.Throws<ArgumentNullException>(() => new StoreHandler(null));
    }

    #region NQuads Tests

    [Fact]
    public void ParsingStoreHandlerNQuadsImplicit()
    {
        EnsureTestData("test.nq");

        var store = new TripleStore();

        var parser = new NQuadsParser();
        parser.Load(store, "test.nq");

        var expectGraph1 = new UriNode(new Uri("http://graphs/1"));
        var expectGraph2 = new UriNode(new Uri("http://graphs/2"));
        Assert.True(store.HasGraph(expectGraph1), "Configuration Vocab Graph should have been parsed from Dataset");
        var configOrig = new Graph();
        configOrig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        IGraph config = store[expectGraph1];
        Assert.Equal(configOrig, config);

        Assert.True(store.HasGraph(expectGraph2), "Leviathan Function Library Graph should have been parsed from Dataset");
        var lvnOrig = new Graph();
        lvnOrig.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
        IGraph lvn = store[expectGraph2];
        Assert.Equal(lvnOrig, lvn);

    }

    [Fact]
    public void ParsingStoreHandlerNQuadsExplicit()
    {
        EnsureTestData("test.nq");

        var store = new TripleStore();
        var parser = new NQuadsParser();
        parser.Load(new StoreHandler(store), "test.nq");
        var graph1 = new UriNode(new Uri("http://graphs/1"));
        var graph2 = new UriNode(new Uri("http://graphs/2"));

        Assert.True(store.HasGraph(graph1), "Configuration Vocab Graph should have been parsed from Dataset");
        var configOrig = new Graph();
        configOrig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        IGraph config = store[graph1];
        Assert.Equal(configOrig, config);

        Assert.True(store.HasGraph(graph2), "Leviathan Function Library Graph should have been parsed from Dataset");
        var lvnOrig = new Graph();
        lvnOrig.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
        IGraph lvn = store[graph2];
        Assert.Equal(lvnOrig, lvn);

    }

    [Fact]
    public void ParsingStoreHandlerNQuadsCounting()
    {
        EnsureTestData("test.nq");

        var parser = new NQuadsParser();
        var counter = new StoreCountHandler();
        parser.Load(counter, "test.nq");

        var configOrig = new Graph();
        configOrig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        var lvnOrig = new Graph();
        lvnOrig.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");

        Assert.Equal(2, counter.GraphCount);
        Assert.Equal(configOrig.Triples.Count + lvnOrig.Triples.Count, counter.TripleCount);
    }

    [Fact]
    public void ParsingFileLoaderStoreHandlerCounting()
    {
        EnsureTestData("test.nq");

        var counter = new StoreCountHandler();
        FileLoader.LoadDataset(counter, "test.nq");

        var configOrig = new Graph();
        configOrig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        var lvnOrig = new Graph();
        lvnOrig.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");

        Assert.Equal(2, counter.GraphCount);
        Assert.Equal(configOrig.Triples.Count + lvnOrig.Triples.Count, counter.TripleCount);
    }

    [Fact]
    public void ParsingFileLoaderStoreHandlerExplicit()
    {
        EnsureTestData("test.nq");

        var store = new TripleStore();
        FileLoader.LoadDataset(new StoreHandler(store), "test.nq");
        var graph1 = new UriNode(new Uri("http://graphs/1"));
        var graph2 = new UriNode(new Uri("http://graphs/2"));

        Assert.True(store.HasGraph(graph1), "Configuration Vocab Graph should have been parsed from Dataset");
        var configOrig = new Graph();
        configOrig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        IGraph config = store[graph1];
        Assert.Equal(configOrig, config);

        Assert.True(store.HasGraph(graph2), "Leviathan Function Library Graph should have been parsed from Dataset");
        var lvnOrig = new Graph();
        lvnOrig.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
        IGraph lvn = store[graph2];
        Assert.Equal(lvnOrig, lvn);

    }
    #endregion

    #region TriG Tests

    [Fact]
    public void ParsingStoreHandlerTriGImplicit()
    {
        EnsureTestData("test.trig");

        var store = new TripleStore();

        var parser = new TriGParser();
        parser.Load(store, "test.trig");
        var nodeFactory = new NodeFactory();
        IUriNode graph1 = nodeFactory.CreateUriNode(new Uri("http://graphs/1"));
        IUriNode graph2 = nodeFactory.CreateUriNode(new Uri("http://graphs/2"));


        Assert.True(store.HasGraph(graph1), "Configuration Vocab Graph should have been parsed from Dataset");
        var configOrig = new Graph();
        configOrig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        IGraph config = store[graph1];
        Assert.Equal(configOrig, config);

        Assert.True(store.HasGraph(graph2), "Leviathan Function Library Graph should have been parsed from Dataset");
        var lvnOrig = new Graph();
        lvnOrig.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
        IGraph lvn = store[graph2];
        Assert.Equal(lvnOrig, lvn);

    }

    [Fact]
    public void ParsingStoreHandlerTriGExplicit()
    {
        EnsureTestData("test.trig");

        var store = new TripleStore();
        var parser = new TriGParser();
        parser.Load(new StoreHandler(store), "test.trig");
        var nodeFactory = new NodeFactory();
        IUriNode graph1 = nodeFactory.CreateUriNode(new Uri("http://graphs/1"));
        IUriNode graph2 = nodeFactory.CreateUriNode(new Uri("http://graphs/2"));

        Assert.True(store.HasGraph(graph1), "Configuration Vocab Graph should have been parsed from Dataset");
        var configOrig = new Graph();
        configOrig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        IGraph config = store[graph1];
        Assert.Equal(configOrig, config);

        Assert.True(store.HasGraph(graph2), "Leviathan Function Library Graph should have been parsed from Dataset");
        var lvnOrig = new Graph();
        lvnOrig.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
        IGraph lvn = store[graph2];
        Assert.Equal(lvnOrig, lvn);

    }

    [Fact]
    public void ParsingStoreHandlerTriGCounting()
    {
        EnsureTestData("test.trig");

        var parser = new TriGParser();
        var counter = new StoreCountHandler();
        parser.Load(counter, "test.trig");

        var configOrig = new Graph();
        configOrig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        var lvnOrig = new Graph();
        lvnOrig.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");

        Assert.Equal(2, counter.GraphCount);
        Assert.Equal(configOrig.Triples.Count + lvnOrig.Triples.Count, counter.TripleCount);
    }

    #endregion

    #region TriX Tests

    [Fact]
    public void ParsingStoreHandlerTriXImplicit()
    {
        EnsureTestData("test.xml");

        var store = new TripleStore();
        var parser = new TriXParser();
        parser.Load(store, "test.xml");
        var nodeFactory = new NodeFactory();
        IUriNode graph1 = nodeFactory.CreateUriNode(new Uri("http://graphs/1"));
        IUriNode graph2 = nodeFactory.CreateUriNode(new Uri("http://graphs/2"));

        Assert.True(store.HasGraph(graph1), "Configuration Vocab Graph should have been parsed from Dataset");
        var configOrig = new Graph();
        configOrig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        IGraph config = store[graph1];
        Assert.Equal(configOrig, config);

        Assert.True(store.HasGraph(graph2), "Leviathan Function Library Graph should have been parsed from Dataset");
        var lvnOrig = new Graph();
        lvnOrig.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
        IGraph lvn = store[graph2];
        Assert.Equal(lvnOrig, lvn);

    }

    [Fact]
    public void ParsingStoreHandlerTriXExplicit()
    {
        EnsureTestData("test.xml");

        var store = new TripleStore();
        var parser = new TriXParser();
        parser.Load(new StoreHandler(store), "test.xml");
        var nodeFactory = new NodeFactory();
        IUriNode graph1 = nodeFactory.CreateUriNode(new Uri("http://graphs/1"));
        IUriNode graph2 = nodeFactory.CreateUriNode(new Uri("http://graphs/2"));

        Assert.True(store.HasGraph(graph1), "Configuration Vocab Graph should have been parsed from Dataset");
        var configOrig = new Graph();
        configOrig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        IGraph config = store[graph1];
        Assert.Equal(configOrig, config);

        Assert.True(store.HasGraph(graph2), "Leviathan Function Library Graph should have been parsed from Dataset");
        var lvnOrig = new Graph();
        lvnOrig.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
        IGraph lvn = store[graph2];
        Assert.Equal(lvnOrig, lvn);

    }

    [Fact]
    public void ParsingStoreHandlerTriXCounting()
    {
        EnsureTestData("test.xml");

        var parser = new TriXParser();
        var counter = new StoreCountHandler();
        parser.Load(counter, "test.xml");

        var configOrig = new Graph();
        configOrig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        var lvnOrig = new Graph();
        lvnOrig.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");

        Assert.Equal(2, counter.GraphCount);
        Assert.Equal(configOrig.Triples.Count + lvnOrig.Triples.Count, counter.TripleCount);
    }

    #endregion
}
