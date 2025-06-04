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
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage.Management;
using VDS.RDF.Storage.Management.Provisioning;

namespace VDS.RDF.Storage;

[Collection("AllegroGraph Test Collection")]
public class AllegroGraphTests
{
    public static AllegroGraphConnector GetConnection()
    {
        Assert.SkipUnless(TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseAllegroGraph), "Test Config marks AllegroGraph as unavailable, cannot run this test");
        EnsureRepository(TestConfigManager.GetSetting(TestConfigManager.AllegroGraphServer),
            TestConfigManager.GetSetting(TestConfigManager.AllegroGraphCatalog),
            TestConfigManager.GetSetting(TestConfigManager.AllegroGraphRepository),
            TestConfigManager.GetSetting(TestConfigManager.AllegroGraphUser),
            TestConfigManager.GetSetting(TestConfigManager.AllegroGraphPassword));
        return new AllegroGraphConnector(TestConfigManager.GetSetting(TestConfigManager.AllegroGraphServer), TestConfigManager.GetSetting(TestConfigManager.AllegroGraphCatalog), TestConfigManager.GetSetting(TestConfigManager.AllegroGraphRepository), TestConfigManager.GetSetting(TestConfigManager.AllegroGraphUser), TestConfigManager.GetSetting(TestConfigManager.AllegroGraphPassword));
    }

    private static void EnsureRepository(string baseUri, string catalog, string repo, string user, string password)
    {
        var server = new AllegroGraphServer(baseUri, catalog, user, password);
        server.CreateStore(new StoreTemplate(repo, "Unit Test", "Unit Test repository"));
    }

    // These tests are using the synchronous API

    [Fact]
    public void StorageAllegroGraphSaveLoad()
    {
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources","InferenceTest.ttl"));
        g.BaseUri = new Uri("http://example.org/AllegroGraphTest");

        AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();
        agraph.SaveGraph(g);

        var h = new Graph();
        agraph.LoadGraph(h, "http://example.org/AllegroGraphTest");
        Assert.False(h.IsEmpty, "Graph should not be empty after loading");

        Assert.Equal(g, h);
    }

    [Fact]
    public void StorageAllegroGraphSaveEmptyGraph1()
    {
        var g = new Graph
        {
            BaseUri = new Uri("http://example.org/AllegroGraph/empty")
        };

        AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();
        agraph.SaveGraph(g);

        var h = new Graph();
        agraph.LoadGraph(h, "http://example.org/AllegroGraph/empty");
        Assert.True(h.IsEmpty, "Graph should be empty after loading");

        Assert.Equal(g, h);
    }

    [Fact]
    public void StorageAllegroGraphSaveEmptyGraph2()
    {
        AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();
        var graphUri = new Uri("http://example.org/AllegroGraph/empty2");
        // Delete any existing graph
        agraph.DeleteGraph(graphUri);

        // First create a non-empty graph
        var g = new Graph
        {
            BaseUri = graphUri
        };
        g.Assert(g.CreateBlankNode(), g.CreateUriNode("rdf:type"), g.CreateUriNode(new Uri("http://example.org/BNode")));
        agraph.SaveGraph(g);

        var h = new Graph();
        agraph.LoadGraph(h, graphUri);
        Assert.False(h.IsEmpty, "Graph should not be empty after loading");

        Assert.Equal(g, h);

        // Now attempt to save an empty graph as well
        g = new Graph
        {
            BaseUri = graphUri
        };
        agraph.SaveGraph(g);

        h = new Graph();
        agraph.LoadGraph(h, graphUri);
        Assert.True(h.IsEmpty, "Graph should be empty after loading");

        Assert.Equal(g, h);
    }

    [Fact]
    public void StorageAllegroGraphSaveEmptyGraph3()
    {
        AllegroGraphConnector agraph = GetConnection();
        Uri graphUri = null;
        // Delete existing graph
        agraph.DeleteGraph(graphUri);

        // First create a non-empty graph
        var g = new Graph
        {
            BaseUri = graphUri
        };
        g.Assert(g.CreateBlankNode(), g.CreateUriNode("rdf:type"), g.CreateUriNode(new Uri("http://example.org/BNode")));
        agraph.SaveGraph(g);

        var h = new Graph();
        agraph.LoadGraph(h, graphUri);
        Assert.False(h.IsEmpty, "Graph should not be empty after loading");

        Assert.Equal(g, h);

        // Now attempt to overwrite with an empty graph
        g = new Graph
        {
            BaseUri = graphUri
        };
        agraph.SaveGraph(g);

        h = new Graph();
        agraph.LoadGraph(h, graphUri);

        // Since saving to default graph does not overwrite the graph we've just retrieved must contain the empty graph as a sub-graph
        Assert.True(h.HasSubGraph(g));
    }

    [Fact]
    public void StorageAllegroGraphDeleteTriples()
    {
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources","InferenceTest.ttl"));
        g.BaseUri = new Uri("http://example.org/AllegroGraphTest");

        AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();
        agraph.SaveGraph(g);

        //Delete all Triples about the Ford Fiesta
        agraph.UpdateGraph(g.BaseUri, null, g.GetTriplesWithSubject(new Uri("http://example.org/vehicles/FordFiesta")));

        var h = new Graph();
        agraph.LoadGraph(h, g.BaseUri);

        Assert.False(h.IsEmpty, "Graph should not be completely empty");
        Assert.True(g.HasSubGraph(h), "Graph retrieved with missing Triples should be a sub-graph of the original Graph");
        Assert.False(g.Equals(h), "Graph retrieved should not be equal to original Graph");

        var results = agraph.Query("ASK WHERE { GRAPH <http://example.org/AllegroGraphTest> { <http://example.org/vehicles/FordFiesta> ?p ?o } }");
        if (results is SparqlResultSet resultSet)
        {
            Assert.False(resultSet.Result, "There should no longer be any triples about the Ford Fiesta present");
        }
    }

    [Fact]
    public void StorageAllegroGraphDeleteGraph1()
    {
        AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();
        var graphUri = new Uri("http://example.org/AllegroGraph/delete");

        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources","InferenceTest.ttl"));
        g.BaseUri = graphUri;

        agraph.SaveGraph(g);

        var h = new Graph();
        agraph.LoadGraph(h, graphUri);
        Assert.False(h.IsEmpty, "Graph should not be empty after loading");

        Assert.Equal(g, h);

        agraph.DeleteGraph(graphUri);
        h = new Graph();
        agraph.LoadGraph(h, graphUri);
        Assert.True(h.IsEmpty, "Graph should be equal after deletion");
        Assert.NotEqual(g, h);
    }

    [Fact]
    public void StorageAllegroGraphDeleteGraph2()
    {
        AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();
        Uri graphUri = null;
        agraph.DeleteGraph(graphUri);

        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources","InferenceTest.ttl"));
        g.BaseUri = graphUri;

        agraph.SaveGraph(g);

        var h = new Graph();
        agraph.LoadGraph(h, graphUri);
        Assert.False(h.IsEmpty, "Graph should not be empty after loading");

        Assert.Equal(g, h);

        agraph.DeleteGraph(graphUri);
        h = new Graph();
        agraph.LoadGraph(h, graphUri);
        Assert.True(h.IsEmpty, "Graph should be equal after deletion");
        Assert.NotEqual(g, h);
    }

    [Fact]
    public void StorageAllegroGraphAsk()
    {
        AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();

        var ask = "ASK WHERE { ?s ?p ?o }";

        var results = agraph.Query(ask);
        Assert.IsType<SparqlResultSet>(results, exactMatch: false);
    }

    [Fact]
    public void StorageAllegroGraphDescribe()
    {
        AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();

        var describe = "DESCRIBE <http://example.org/Vehicles/FordFiesta>";

        var results = agraph.Query(describe);
        Assert.IsType<IGraph>(results, exactMatch: false);
    }

    [Fact]
    public void StorageAllegroGraphSparqlUpdate()
    {
        AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();
        agraph.DeleteGraph("http://example.org/new-graph");

        var updates = "INSERT DATA { GRAPH <http://example.org/new-graph> { <http://subject> <http://predicate> <http://object> } }";

        agraph.Update(updates);

        var results = agraph.Query("SELECT * WHERE { GRAPH <http://example.org/new-graph> { ?s ?p ?o } }") as SparqlResultSet;
        Assert.NotNull(results);
        Assert.Equal(1, results.Count);
    }
}
