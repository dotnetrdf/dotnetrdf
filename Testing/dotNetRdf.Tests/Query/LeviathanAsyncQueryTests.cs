using System.Threading;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query.Datasets;
using Xunit;

namespace VDS.RDF.Query;

public class LeviathanAsyncQueryTests
{
    private readonly ITestOutputHelper _output;

    public LeviathanAsyncQueryTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void TestAsyncQueryResultSetCallback()
    {
        var store = new TripleStore();
        var g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        store.Add(g);
        var dataset = new InMemoryDataset(store, g.Name);

        var processor = new LeviathanQueryProcessor(dataset);

        var wait = new AutoResetEvent(false);
        var parser = new SparqlQueryParser();
        var query = parser.ParseFromString(
            "SELECT * WHERE { ?instance a ?class }");
        var callbackInvoked = false;
        var resultCount = 0;
        var syncResultSet = processor.ProcessQuery(query) as SparqlResultSet;
        Assert.NotNull(syncResultSet);
        var syncResultCount = syncResultSet.Count;

        processor.ProcessQuery(query, null, (resultSet, state) =>
        {
            try
            {
                Assert.IsNotType<AsyncError>(state);
                Assert.Equal("some state", state);
                Assert.NotNull(resultSet);
                Assert.NotEmpty(resultSet.Results);
            }
            finally
            {
                resultCount = resultSet.Count;
                callbackInvoked = true;
                wait.Set();
            }
        }, "some state");
        wait.WaitOne();
        Assert.True(callbackInvoked);
        Assert.True(resultCount > 0);
        Assert.Equal(syncResultCount, resultCount);
    }

    [Fact]
    public void TestAsyncQueryWithQueryCallback()
    {
        var store = new TripleStore();
        var g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        store.Add(g);
        var dataset = new InMemoryDataset(store, g.Name);

        var processor = new LeviathanQueryProcessor(dataset);

        var wait = new AutoResetEvent(false);
        var parser = new SparqlQueryParser();
        var query = parser.ParseFromString(
            "SELECT * WHERE { ?instance a ?class }");
        var callbackInvoked = false;
        var syncResultSet = processor.ProcessQuery(query) as SparqlResultSet;
        Assert.NotNull(syncResultSet);
        var syncResultCount = syncResultSet.Count;
        var resultSet = new SparqlResultSet();
        var resultHandler = new ResultSetHandler(resultSet);
        processor.ProcessQuery(null, resultHandler, query, (rdfHandler, rsetHandler, state)=>
        {
            try
            {
                Assert.IsNotType<AsyncError>(state);
                Assert.Equal("some state", state);
            }
            finally
            {
                callbackInvoked = true;
                wait.Set();
            }
        }, "some state");
        wait.WaitOne();
        var resultCount = resultSet.Count;
        Assert.True(callbackInvoked);
        Assert.True(resultCount > 0);
        Assert.Equal(syncResultCount, resultCount);
    }
}
