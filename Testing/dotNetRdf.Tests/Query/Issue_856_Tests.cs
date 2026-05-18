using System;
using System.IO;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Datasets;
using Xunit;

namespace VDS.RDF.Query;

public class Issue_856_Tests
{
    private static readonly String TestDataset = @"
        @prefix ex:   <http://example.org/> .
        @prefix foaf: <http://xmlns.com/foaf/0.1/> .

        ex:David a foaf:Person .
        ex:David foaf:knows ex:Zach .
        ex:David foaf:knows ex:Yara .

        ex:namedGraph {
            ex:Zach a ex:famousPerson .
        }";

    private static readonly String TestGraph = @"
        @prefix ex:   <http://example.org/> .
        @prefix foaf: <http://xmlns.com/foaf/0.1/> .

        ex:David a foaf:Person .
        ex:David foaf:knows ex:Zach .
        ex:David foaf:knows ex:Yara .
        ex:Zach a ex:famousPerson .";
    

    private TripleStore _store;
    private LeviathanQueryProcessor _processor;
    private LeviathanQueryProcessor _singleGraphProcessor;

    public Issue_856_Tests()
    {
        _store = new TripleStore();
        var parser = new TriGParser();
        parser.Load(_store, new StringReader(TestDataset));
        _processor = new LeviathanQueryProcessor(_store);
        var graph = new Graph();
        graph.LoadFromString(TestGraph);
        _singleGraphProcessor = new LeviathanQueryProcessor(new InMemoryDataset(graph));
    }

    [Fact]
    public void ApplyFilterWithNoNamedGraphPatternSingleGraph()
    {
        var parser = new SparqlQueryParser();
        SparqlQuery query = parser.ParseFromString(@"
            PREFIX ex:   <http://example.org/>
            PREFIX foaf: <http://xmlns.com/foaf/0.1/>

            SELECT *
            WHERE {
            ?this a foaf:Person .
            FILTER NOT EXISTS {
                ?someone a ex:famousPerson .
                ?this foaf:knows ?someone .
            }
            ?this foaf:knows ?someone .
            }");
        var results = _singleGraphProcessor.ProcessQuery(query) as SparqlResultSet;
        Assert.NotNull(results);
        Assert.Equal(1, results.Count);
        Assert.Equal("http://example.org/David", results[0]["this"].ToString());
        Assert.Equal("http://example.org/Yara", results[0]["someone"].ToString());
    }

    [Fact]
    public void ApplyFilterWithNoNamedGraphPattern()
    {
        var parser = new SparqlQueryParser();
        SparqlQuery query = parser.ParseFromString(@"
            PREFIX ex:   <http://example.org/>
            PREFIX foaf: <http://xmlns.com/foaf/0.1/>

            SELECT *
            WHERE {
            ?this a foaf:Person .
            FILTER NOT EXISTS {
                ?someone a ex:famousPerson .
                ?this foaf:knows ?someone .
            }
            ?this foaf:knows ?someone .
            }");
        var results = _processor.ProcessQuery(query) as SparqlResultSet;
        Assert.NotNull(results);
        Assert.Equal(2, results.Count);
        Assert.Equal("http://example.org/David", results[0]["this"].ToString());
        Assert.True(results[0]["someone"].ToString() == "http://example.org/Zach" || results[0]["someone"].ToString() == "http://example.org/Yara");
        Assert.Equal("http://example.org/David", results[1]["this"].ToString());
        Assert.True(results[1]["someone"].ToString() == "http://example.org/Zach" || results[1]["someone"].ToString() == "http://example.org/Yara");
    }

    [Fact]
    public void ApplyFilterWithNamedGraphPattern()
    {
        var parser = new SparqlQueryParser();
        SparqlQuery query = parser.ParseFromString(@"
            PREFIX ex:   <http://example.org/>
            PREFIX foaf: <http://xmlns.com/foaf/0.1/>

            SELECT *
            WHERE {
            ?this a foaf:Person .
            ?this foaf:knows ?someone .
            FILTER NOT EXISTS {
                GRAPH ex:namedGraph {
                    ?someone a ex:famousPerson .
                }
                ?this foaf:knows ?someone .
            }
            }");
        var results = _processor.ProcessQuery(query) as SparqlResultSet;
        Assert.NotNull(results);
        Assert.Equal(1, results.Count);
        Assert.Equal("http://example.org/David", results[0]["this"].ToString());
        Assert.Equal("http://example.org/Yara", results[0]["someone"].ToString());
    }
}