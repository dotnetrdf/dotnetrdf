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
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query;


public class DefaultGraphTests2
{
    private readonly SparqlQueryParser _parser = new SparqlQueryParser();

    private ISparqlDataset GetDataset(IEnumerable<IGraph> gs, bool unionDefaultGraph)
    {
        var store = new TripleStore();
        foreach (IGraph g in gs)
        {
            store.Add(g, false);
        }

        return new InMemoryDataset(store, unionDefaultGraph);
    }

    private ISparqlDataset GetDataset(IEnumerable<IGraph> gs, Uri defaultGraphUri)
    {
        var store = new TripleStore();
        foreach (IGraph g in gs)
        {
            store.Add(g, false);
        }

        return new InMemoryDataset(store, defaultGraphUri == null ? null : new UriNode(defaultGraphUri));
    }

    [Fact]
    public void SparqlDatasetDefaultGraphUnion()
    {
        var gs = new List<IGraph>();
        var g = new Graph(new UriNode(new Uri("http://example.org/1")));
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        gs.Add(g);
        var h = new Graph(new UriNode(new Uri("http://example.org/2")));
        h.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
        gs.Add(h);

        ISparqlDataset dataset = GetDataset(gs, true);
        var query = "CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }";
        SparqlQuery q = _parser.ParseFromString(query);
        var processor = new LeviathanQueryProcessor(dataset);

        object results = processor.ProcessQuery(q);
        if (results is IGraph resultGraph)
        {
            Assert.Equal(g.Triples.Count + h.Triples.Count, resultGraph.Triples.Count);
            Assert.True(resultGraph.HasSubGraph(g), "g should be a sub-graph of the results");
            Assert.True(resultGraph.HasSubGraph(h), "h should be a sub-graph of the results");
        }
        else
        {
            Assert.Fail("Did not return a Graph as expected");
        }
    }

    [Fact]
    public void SparqlDatasetDefaultGraphUnionAndGraphClause()
    {
        var gs = new List<IGraph>();
        var g = new Graph(new UriNode(new Uri("http://example.org/1")));
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        gs.Add(g);
        var h = new Graph(new UriNode(new Uri("http://example.org/2")));
        h.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
        gs.Add(h);

        ISparqlDataset dataset = GetDataset(gs, true);
        var query = "CONSTRUCT { ?s ?p ?o } WHERE { GRAPH <http://example.org/unknown> { ?s ?p ?o } }";
        SparqlQuery q = _parser.ParseFromString(query);
        var processor = new LeviathanQueryProcessor(dataset);

        object results = processor.ProcessQuery(q);
        if (results is IGraph r)
        {
            Assert.Equal(0, r.Triples.Count);
            Assert.False(r.HasSubGraph(g), "g should not be a sub-graph of the results");
            Assert.False(r.HasSubGraph(h), "h should not be a sub-graph of the results");
        }
        else
        {
            Assert.Fail("Did not return a Graph as expected");
        }
    }

    [Fact]
    public void SparqlDatasetDefaultGraphNoUnion()
    {
        var gs = new List<IGraph>();
        var g = new Graph(new UriNode(new Uri("http://example.org/1")));
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        gs.Add(g);
        var h = new Graph(new UriNode(new Uri("http://example.org/2")));
        h.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
        gs.Add(h);

        ISparqlDataset dataset = GetDataset(gs, false);
        var query = "CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }";
        SparqlQuery q = _parser.ParseFromString(query);
        var processor = new LeviathanQueryProcessor(dataset);

        var results = processor.ProcessQuery(q);
        if (results is IGraph)
        {
            var r = (IGraph)results;
            Assert.Equal(0, r.Triples.Count);
            Assert.False(r.HasSubGraph(g), "g should not be a subgraph of the results");
            Assert.False(r.HasSubGraph(h), "h should not be a subgraph of the results");
        }
        else
        {
            Assert.Fail("Did not return a Graph as expected");
        }
    }

    [Fact]
    public void SparqlDatasetDefaultGraphNamed()
    {
        var gs = new List<IGraph>();
        var g = new Graph(new UriNode(new Uri("http://example.org/1")));
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        gs.Add(g);
        var h = new Graph(new UriNode(new Uri("http://example.org/2")));
        h.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
        gs.Add(h);

        ISparqlDataset dataset = GetDataset(gs, new Uri("http://example.org/1"));
        var query = "CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }";
        SparqlQuery q = _parser.ParseFromString(query);
        var processor = new LeviathanQueryProcessor(dataset);

        var results = processor.ProcessQuery(q);
        if (results is IGraph)
        {
            var r = (IGraph)results;
            Assert.Equal(g.Triples.Count, r.Triples.Count);
            Assert.Equal(g, r);
            Assert.NotEqual(h, r);
        }
        else
        {
            Assert.Fail("Did not return a Graph as expected");
        }
    }

    [Fact]
    public void SparqlDatasetDefaultGraphNamedAndGraphClause()
    {
        var gs = new List<IGraph>();
        var g = new Graph(new UriNode(new Uri("http://example.org/1")));
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        gs.Add(g);
        var h = new Graph(new UriNode(new Uri("http://example.org/2")));
        h.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
        gs.Add(h);

        ISparqlDataset dataset = GetDataset(gs, new Uri("http://example.org/1"));
        var query = "CONSTRUCT { ?s ?p ?o } WHERE { GRAPH <http://example.org/2> { ?s ?p ?o } }";
        SparqlQuery q = _parser.ParseFromString(query);
        var processor = new LeviathanQueryProcessor(dataset);

        var results = processor.ProcessQuery(q);
        if (results is IGraph)
        {
            var r = (IGraph)results;
            Assert.Equal(h.Triples.Count, r.Triples.Count);
            Assert.NotEqual(g, r);
            Assert.Equal(h, r);
        }
        else
        {
            Assert.Fail("Did not return a Graph as expected");
        }
    }

    [Fact]
    public void SparqlDatasetDefaultGraphNamed2()
    {
        var gs = new List<IGraph>();
        var g = new Graph(new UriNode(new Uri("http://example.org/1")));
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        gs.Add(g);
        var h = new Graph(new UriNode(new Uri("http://example.org/2")));
        h.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
        gs.Add(h);

        ISparqlDataset dataset = GetDataset(gs, new Uri("http://example.org/2"));
        var query = "CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }";
        SparqlQuery q = _parser.ParseFromString(query);
        var processor = new LeviathanQueryProcessor(dataset);

        var results = processor.ProcessQuery(q);
        if (results is IGraph)
        {
            var r = (IGraph)results;
            Assert.Equal(h.Triples.Count, r.Triples.Count);
            Assert.NotEqual(g, r);
            Assert.Equal(h, r);
        }
        else
        {
            Assert.Fail("Did not return a Graph as expected");
        }
    }

    [Fact]
    public void SparqlDatasetDefaultGraphNamedAndGraphClause2()
    {
        var gs = new List<IGraph>();
        var g = new Graph(new UriNode(new Uri("http://example.org/1")));
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        gs.Add(g);
        var h = new Graph(new UriNode(new Uri("http://example.org/2")));
        h.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
        gs.Add(h);

        ISparqlDataset dataset = GetDataset(gs, new Uri("http://example.org/2"));
        var query = "CONSTRUCT { ?s ?p ?o } WHERE { GRAPH <http://example.org/1> { ?s ?p ?o } }";
        SparqlQuery q = _parser.ParseFromString(query);
        var processor = new LeviathanQueryProcessor(dataset);

        var results = processor.ProcessQuery(q);
        if (results is IGraph)
        {
            var r = (IGraph)results;
            Assert.Equal(g.Triples.Count, r.Triples.Count);
            Assert.Equal(g, r);
            Assert.NotEqual(h, r);
        }
        else
        {
            Assert.Fail("Did not return a Graph as expected");
        }
    }

    [Fact]
    public void SparqlDatasetDefaultGraphUnknownName()
    {
        var gs = new List<IGraph>();
        var g = new Graph(new UriNode(new Uri("http://example.org/1")));
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        gs.Add(g);
        var h = new Graph(new UriNode(new Uri("http://example.org/2")));
        h.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
        gs.Add(h);

        ISparqlDataset dataset = GetDataset(gs, new Uri("http://example.org/unknown"));
        var query = "CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }";
        SparqlQuery q = _parser.ParseFromString(query);
        var processor = new LeviathanQueryProcessor(dataset);

        var results = processor.ProcessQuery(q);
        if (results is IGraph)
        {
            var r = (IGraph)results;
            Assert.Equal(0, r.Triples.Count);
            Assert.False(r.HasSubGraph(g), "g should not be a subgraph of the results");
            Assert.False(r.HasSubGraph(h), "h should not be a subgraph of the results");
        }
        else
        {
            Assert.Fail("Did not return a Graph as expected");
        }
    }

    [Fact]
    public void SparqlDatasetDefaultGraphUnnamed()
    {
        var gs = new List<IGraph>();
        var g = new Graph(new UriNode(new Uri("http://example.org/1")));
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        gs.Add(g);
        var h = new Graph(new UriNode(new Uri("http://example.org/2")));
        h.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
        gs.Add(h);

        ISparqlDataset dataset = GetDataset(gs, null);
        var query = "CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }";
        SparqlQuery q = _parser.ParseFromString(query);
        var processor = new LeviathanQueryProcessor(dataset);

        var results = processor.ProcessQuery(q);
        if (results is IGraph)
        {
            var r = (IGraph)results;
            Assert.Equal(0, r.Triples.Count);
            Assert.False(r.HasSubGraph(g), "g should not be a subgraph of the results");
            Assert.False(r.HasSubGraph(h), "h should not be a subgraph of the results");
        }
        else
        {
            Assert.Fail("Did not return a Graph as expected");
        }
    }
}
