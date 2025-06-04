using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using VDS.RDF.Parsing.Handlers;
using Xunit;

namespace VDS.RDF.Query;

public class SparqlQueryClientTests : IClassFixture<MockRemoteSparqlEndpointFixture>
{
    private static readonly HttpClient HttpClient = new HttpClient();
    private readonly MockRemoteSparqlEndpointFixture _fixture;

    public SparqlQueryClientTests(MockRemoteSparqlEndpointFixture fixture)
    {
        _fixture = fixture;
    }

    private SparqlQueryClient GetQueryClient()
    {
        return new SparqlQueryClient(HttpClient, new Uri(_fixture.Server.Urls[0] + "/sparql"));
    }


    [Fact]
    public async Task SelectWithResultSet()
    {
        SparqlQueryClient client = GetQueryClient();
        SparqlResultSet resultSet = await client.QueryWithResultSetAsync(_fixture.SelectQuery, TestContext.Current.CancellationToken);
        resultSet.Count.Should().Be(1);
    }

    [Fact]
    public async Task SelectWithCountHandler()
    {
        SparqlQueryClient client = GetQueryClient();
        var handler = new ResultCountHandler();
        await client.QueryWithResultSetAsync(_fixture.SelectQuery, handler, TestContext.Current.CancellationToken);
        handler.Count.Should().Be(1);
    }

    [Fact]
    public async Task SelectRaisesExceptionOnServerError()
    {
        SparqlQueryClient client = GetQueryClient();
        RdfQueryException ex = await Assert.ThrowsAsync<RdfQueryException>(async () => await client.QueryWithResultSetAsync(_fixture.ErrorSelectQuery, TestContext.Current.CancellationToken));
        ex.Message.Should().Contain("400");
    }

    [Fact]
    public async Task ConstructWithResultGraph()
    {
        SparqlQueryClient client = GetQueryClient();
        IGraph resultGraph = await client.QueryWithResultGraphAsync(_fixture.ConstructQuery, TestContext.Current.CancellationToken);
        resultGraph.Triples.Count.Should().Be(1);
    }

    [Fact]
    public async Task ConstructWithCountHandler()
    {
        SparqlQueryClient client = GetQueryClient();
        var handler = new CountHandler();
        await client.QueryWithResultGraphAsync(_fixture.ConstructQuery, handler, TestContext.Current.CancellationToken);
        handler.Count.Should().Be(1);
    }

    [Fact]
    public async Task ConstructRaisesExceptionOnServerError()
    {
        SparqlQueryClient client = GetQueryClient();
        RdfQueryException ex = await Assert.ThrowsAsync<RdfQueryException>(async () => await client.QueryWithResultGraphAsync(_fixture.ErrorConstructQuery, TestContext.Current.CancellationToken));
        ex.Message.Should().Contain("400");
    }
}
