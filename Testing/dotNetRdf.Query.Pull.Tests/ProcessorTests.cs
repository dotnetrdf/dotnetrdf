using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Query.Pull;

namespace dotNetRdf.Query.Pull.Tests;

public class ProcessorTests
{
    private static TripleStore MakeTestTripleStore()
    {
        var store = new TripleStore();
        var nodeFactory = new NodeFactory();
        nodeFactory.NamespaceMap.AddNamespace("ex", store.UriFactory.Create("http://example.org/"));
        store.Assert(new Quad(
            nodeFactory.CreateUriNode("ex:s"),
            nodeFactory.CreateUriNode("ex:p"),
            nodeFactory.CreateUriNode("ex:o"),
            null));
        store.Assert(new Quad(
            nodeFactory.CreateUriNode("ex:s"),
            nodeFactory.CreateUriNode("ex:p"),
            nodeFactory.CreateUriNode("ex:o2"),
            nodeFactory.CreateUriNode("ex:g1")));
        store.Assert(new Quad(
            nodeFactory.CreateUriNode("ex:s"),
            nodeFactory.CreateUriNode("ex:p"),
            nodeFactory.CreateUriNode("ex:o3"),
            nodeFactory.CreateUriNode("ex:g2")));
        return store;
    }

    [Fact]
    public void CanQueryUnionDefaultGraph()
    {
        TripleStore store = MakeTestTripleStore();
        var sparqlParser = new SparqlQueryParser();
        SparqlQuery? query = sparqlParser.ParseFromString("SELECT * WHERE {?s ?p ?o}");
        var processor = new PullQueryProcessor(store, options => { options.UnionDefaultGraph = true; });
        var resultsUnionDefault = processor.ProcessQuery(query);
        SparqlResultSet sparqlResultsUnionDefault = Assert.IsType<SparqlResultSet>(resultsUnionDefault);
        Assert.Equal(3, sparqlResultsUnionDefault.Count);
    }

    [Fact]
    public void DefaultsToUnnamedDefaultGraph()
    {
        TripleStore store = MakeTestTripleStore();
        var sparqlParser = new SparqlQueryParser();
        SparqlQuery? query = sparqlParser.ParseFromString("SELECT * WHERE {?s ?p ?o}");
        var processor = new PullQueryProcessor(store, options => { options.UnionDefaultGraph = false; });
        var resultsUnnamedDefault = processor.ProcessQuery(query);
        SparqlResultSet sparqlResultsUnnamedDefault = Assert.IsType<SparqlResultSet>(resultsUnnamedDefault);
        Assert.Equal(1, sparqlResultsUnnamedDefault.Count);
    }

    [Fact]
    public void CanSetExplicitDefaultGraph()
    {
        TripleStore store = MakeTestTripleStore();
        var sparqlParser = new SparqlQueryParser();
        SparqlQuery? query = sparqlParser.ParseFromString("SELECT * WHERE {?s ?p ?o}");
        var nameNodeFactory = new NodeFactory();
        var processor = new PullQueryProcessor(store, options =>
        {
            options.UnionDefaultGraph = false;
            options.DefaultGraphNames =
            [
                nameNodeFactory.CreateUriNode(new Uri("http://example.org/g1")),
                nameNodeFactory.CreateUriNode(new Uri("http://example.org/g2"))
            ];
        });
        var results = processor.ProcessQuery(query);
        SparqlResultSet resultSet = Assert.IsType<SparqlResultSet>(results);
        Assert.Equal(2, resultSet.Count);
    }

    [Fact]
    public async Task AsyncSelectQueryWithHandler()
    {
        TripleStore store = MakeTestTripleStore();
        var sparqlParser = new SparqlQueryParser();
        SparqlQuery? query = sparqlParser.ParseFromString("SELECT * WHERE {?s ?p ?o}");
        var processor = new PullQueryProcessor(store, options => {options.UnionDefaultGraph = true;});
        var handler = new ResultCountHandler();
        await processor.ProcessQueryAsync(null, handler, query);
        Assert.Equal(3, handler.Count);
    }
    
    [Fact]
    public async Task AsyncAskQueryWithHandler()
    {
        TripleStore store = MakeTestTripleStore();
        var sparqlParser = new SparqlQueryParser();
        SparqlQuery? query = sparqlParser.ParseFromString("ASK WHERE {?s ?p ?o}");
        var processor = new PullQueryProcessor(store, options => {options.UnionDefaultGraph = true;});
        var handler = new ResultCountHandler();
        await processor.ProcessQueryAsync(null, handler, query);
        Assert.Equal(1, handler.Count);
    }

    [Fact]
    public async Task AsyncSelectQueryWithoutHandler()
    {
        TripleStore store = MakeTestTripleStore();
        var sparqlParser = new SparqlQueryParser();
        SparqlQuery? query = sparqlParser.ParseFromString("SELECT * WHERE {?s ?p ?o}");
        var processor = new PullQueryProcessor(store, options => {options.UnionDefaultGraph = true;});
        await Assert.ThrowsAsync<ArgumentNullException>( () => processor.ProcessQueryAsync(null, null, query));
    }

    [Fact]
    public async Task DescribeQueryWithHandler()
    {
        TripleStore store = MakeTestTripleStore();
        var sparqlParser= new SparqlQueryParser();
        SparqlQuery? query = sparqlParser.ParseFromString("DESCRIBE ?s WHERE {?s <http://example.org/p> <http://example.org/o> }");
        var processor = new PullQueryProcessor(store, options => {options.UnionDefaultGraph = false;});
        var graph = new Graph();
        await processor.ProcessQueryAsync(new GraphHandler(graph), null, query);
        Assert.Equal(1, graph.Triples.Count);
    }
    
    [Fact]
    public async Task DescribeQueryUnionDefaultGraphWithHandler()
    {
        TripleStore store = MakeTestTripleStore();
        var sparqlParser= new SparqlQueryParser();
        SparqlQuery? query = sparqlParser.ParseFromString("DESCRIBE ?s WHERE {?s <http://example.org/p> <http://example.org/o> }");
        var processor = new PullQueryProcessor(store, options => {options.UnionDefaultGraph = true;});
        var graph = new Graph();
        await processor.ProcessQueryAsync(new GraphHandler(graph), null, query);
        Assert.Equal(3, graph.Triples.Count);
    }

    [Fact]
    public async Task DescribeAllQueryWithHandler()
    {
        TripleStore store = MakeTestTripleStore();
        var sparqlParser= new SparqlQueryParser();
        SparqlQuery? query = sparqlParser.ParseFromString("DESCRIBE * WHERE {?s <http://example.org/p> <http://example.org/o> }");
        var processor = new PullQueryProcessor(store, options => {options.UnionDefaultGraph = false;});
        var graph = new Graph();
        await processor.ProcessQueryAsync(new GraphHandler(graph), null, query);
        Assert.Equal(1, graph.Triples.Count);
    }

}