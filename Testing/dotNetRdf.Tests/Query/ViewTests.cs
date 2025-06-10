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
using System.Threading;
using Xunit;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query;

public class ViewTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ViewTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void SparqlViewConstruct1()
    {
        var store = new TripleStore();
        var view = new SparqlView("CONSTRUCT { ?s ?p ?o } WHERE { GRAPH ?g { ?s ?p ?o . FILTER(IsLiteral(?o)) } }",
            store, new UriNode(new Uri("http://example.org/view")));
        store.Add(view);

        _testOutputHelper.WriteLine("SPARQL View Empty");
        TestTools.ShowGraph(view, _testOutputHelper);
        _testOutputHelper.WriteLine();

        //Load a Graph into the Store to cause the SPARQL View to update
        var g = new Graph(new UriNode(new Uri("http://example.org/data")));
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        store.Add(g);

        Thread.Sleep(1000);
        if (view.Triples.Count == 0) view.UpdateView();

        _testOutputHelper.WriteLine("SPARQL View Populated");
        TestTools.ShowGraph(view, _testOutputHelper);

        Assert.True(view.Triples.Count > 0, "View should have updated to contain some Triples");
    }

    [Fact(Skip = "If the test is not stable, fix the test")]
    public void SparqlViewConstruct2()
    {
        //Since the test has failed intermittently in the past run it a whole bunch of times to be on the safe side
        for (var i = 1; i <= 50; i++)
        {
            SparqlViewConstruct1();
        }
    }

    [Fact]
    public void SparqlViewDescribe1()
    {
        var store = new TripleStore();
        var view = new SparqlView("DESCRIBE <http://example.org/vehicles/FordFiesta>", store,
            new UriNode(new Uri("http://example.org/view")));
        store.Add(view);

        _testOutputHelper.WriteLine("SPARQL View Empty");
        TestTools.ShowGraph(view, _testOutputHelper);
        _testOutputHelper.WriteLine();

        //Load a Graph into the Store to cause the SPARQL View to update
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        store.Add(g, true);

        Thread.Sleep(500);
        if (view.Triples.Count == 0) view.UpdateView();

        _testOutputHelper.WriteLine("SPARQL View Populated");
        TestTools.ShowGraph(view, _testOutputHelper);

        Assert.True(view.Triples.Count > 0, "View should have updated to contain some Triples");
    }

    [Fact(Skip = "If the test is not stable, fix the test")]
    public void SparqlViewDescribe2()
    {
        //Since the test has failed intermittently in the past run it a whole bunch of times to be on the safe side
        for (var i = 1; i <= 50; i++)
        {
            SparqlViewDescribe1();
        }
    }

    [Fact]
    public void SparqlViewSelect1()
    {
        var store = new TripleStore();
        var view = new SparqlView(
            "SELECT ?s (<http://example.org/vehicles/TurbochargedSpeed>) AS ?p (?speed * 1.25) AS ?o  WHERE { GRAPH ?g { ?s <http://example.org/vehicles/Speed> ?speed } }",
            store, new UriNode(new Uri("http://example.org/view")));
        store.Add(view);

        _testOutputHelper.WriteLine("SPARQL View Empty");
        TestTools.ShowGraph(view, _testOutputHelper);
        _testOutputHelper.WriteLine();

        //Load a Graph into the Store to cause the SPARQL View to update
        var g = new Graph(new UriNode(new Uri("http://example.org/data")));
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        store.Add(g);

        Thread.Sleep(500);
        if (view.Triples.Count == 0) view.UpdateView();

        _testOutputHelper.WriteLine("SPARQL View Populated");
        TestTools.ShowGraph(view, _testOutputHelper);

        Assert.True(view.Triples.Count > 0, "View should have updated to contain some Triples");
    }

    [Fact(Skip = "If the test is not stable, fix the test")]
    public void SparqlViewSelect2()
    {
        //Since the test has failed intermittently in the past run it a whole bunch of times to be on the safe side
        for (var i = 1; i <= 50; i++)
        {
            SparqlViewSelect1();
        }
    }

    [Fact]
    public void SparqlViewGraphScope1()
    {
        var store = new TripleStore();
        var view = new SparqlView(
            "CONSTRUCT { ?s ?p ?o } FROM <http://example.org/data> WHERE { ?s ?p ?o . FILTER(IsLiteral(?o)) }",
            store, new UriNode(new Uri("http://example.org/view")));
        store.Add(view);

        Assert.Empty(view.Triples);

        //Load a Graph into the Store to cause the SPARQL View to update
        var g = new Graph(new UriNode(new Uri("http://example.org/data")));
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        store.Add(g);

        Thread.Sleep(500);
        if (view.Triples.Count == 0) view.UpdateView();
        var lastCount = view.Triples.Count;

        Assert.True(view.Triples.Count > 0, "View should have updated to contain some Triples");

        //Load another Graph with a different URI into the Store
        var h = new Graph(new UriNode(new Uri("http://example.org/2")));
        FileLoader.Load(h, Path.Combine("resources", "Turtle.ttl"));
        store.Add(h);

        Thread.Sleep(500);
        view.UpdateView();

        Assert.True(view.Triples.Count == lastCount, "View should not have changed since the added Graph is not in the set of Graphs over which the query operates");

        //Remove this Graph and check the View still doesn't change
        store.Remove(h.Name);

        Thread.Sleep(500);
        view.UpdateView();

        Assert.True(view.Triples.Count == lastCount, "View should not have changed since the removed Graph is not in the set of Graphs over which the query operates");
    }

}
