using FluentAssertions;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using WireMock.Matchers;
using WireMock.Matchers.Request;
using Xunit;

namespace VDS.RDF.Query;

[Collection("FederatedSparqlQuery")]
public class FederatedSparqlQueryClientTests : IClassFixture<FederatedEndpointFixture>
{
    private readonly FederatedEndpointFixture _fixture;
    private static readonly HttpClient HttpClient = new HttpClient();

    public FederatedSparqlQueryClientTests(FederatedEndpointFixture fixture)
    {
        _fixture = fixture;
        _fixture.Server1.ResetLogEntries();
        _fixture.Server2.ResetLogEntries();
    }

    [Fact]
    public async Task QueryWithResultSetCombinesResultsFromFederatedEndpoints()
    {
        var endpoint = new FederatedSparqlQueryClient(
            HttpClient, new Uri(_fixture.Server1.Urls[0] + "/query"), new Uri(_fixture.Server2.Urls[0] + "/query"));
        SparqlResultSet results = await endpoint.QueryWithResultSetAsync("SELECT * WHERE {?s ?p ?o}", TestContext.Current.CancellationToken);
        results.Should().NotBeNull().And.HaveCount(2);
    }

    [Fact]
    public async Task QueryWithResultGraphCombinesGraphsFromFederatedEndpoints()
    {
        var server1Path = "/query2/" + Guid.NewGuid().ToString("D").ToLowerInvariant();
        var server2Path = "/query2/" + Guid.NewGuid().ToString("D").ToLowerInvariant();
        var endpoint = new FederatedSparqlQueryClient(
            HttpClient, new Uri(_fixture.Server1.Urls[0] + server1Path), new Uri(_fixture.Server2.Urls[0] + server2Path));
        IGraph resultGraph = await endpoint.QueryWithResultGraphAsync("CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }", TestContext.Current.CancellationToken);
        resultGraph.Should().NotBeNull();
        resultGraph.Triples.Should().HaveCount(2);
    }

    [Fact]
    public async Task QueryWithResultSetThrowsAnExceptionIfOneEndpointFails()
    {
        var endpoint = new FederatedSparqlQueryClient(
            HttpClient, new Uri(_fixture.Server1.Urls[0] + "/query"), new Uri(_fixture.Server2.Urls[0] + "/fail"));
        await Assert.ThrowsAsync<RdfQueryException>(() => endpoint.QueryWithResultSetAsync("SELECT * WHERE { ?s ?p ?o }", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task QueryWithResultGraphThrowsAnExceptionIfOneEndpointFails()
    {
        var endpoint = new FederatedSparqlQueryClient(
            HttpClient, new Uri(_fixture.Server1.Urls[0] + "/query2/" + Guid.NewGuid().ToString("D").ToLowerInvariant()), new Uri(_fixture.Server2.Urls[0] + "/fail"));
        await Assert.ThrowsAsync<RdfQueryException>( () => endpoint.QueryWithResultGraphAsync("CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task QueryWithResultSetAllowsEndpointErrorsToBeIgnored()
    {
        var endpoint = new FederatedSparqlQueryClient(
                HttpClient, new Uri(_fixture.Server1.Urls[0] + "/query"), new Uri(_fixture.Server2.Urls[0] + "/fail"))
        { IgnoreFailedRequests = true };
        SparqlResultSet results = await endpoint.QueryWithResultSetAsync("SELECT * WHERE {?s ?p ?o}", TestContext.Current.CancellationToken);
        results.Should().NotBeNull().And.HaveCount(1);
    }

    [Fact]
    public async Task QueryWithResultGraphAllowsEndpointErrorsToBeIgnored()
    {
        var endpoint = new FederatedSparqlQueryClient(
                HttpClient, new Uri(_fixture.Server1.Urls[0] + "/query2/" + Guid.NewGuid().ToString("D").ToLowerInvariant()), new Uri(_fixture.Server2.Urls[0] + "/fail"))
            {IgnoreFailedRequests = true};
        IGraph results = await endpoint.QueryWithResultGraphAsync("CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }", TestContext.Current.CancellationToken);
        results.Should().NotBeNull();
        results.Triples.Count.Should().Be(1);
    }

    [Fact]
    public async Task QueryWithResultSetThrowsAnExceptionIfOneEndpointTimesOut()
    {
        var endpoint = new FederatedSparqlQueryClient(
                HttpClient, new Uri(_fixture.Server1.Urls[0] + "/query"), new Uri(_fixture.Server2.Urls[0] + "/timeout"))
        { Timeout = 2000 };
        await Assert.ThrowsAsync<RdfQueryTimeoutException>(() => endpoint.QueryWithResultSetAsync("SELECT * WHERE { ?s ?p ?o }", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task QueryWithResultGraphThrowsAnExceptionIfOneEndpointTimesOut()
    {
        var endpoint = new FederatedSparqlQueryClient(
                HttpClient, new Uri(_fixture.Server1.Urls[0] + "/query2/" + Guid.NewGuid().ToString("D").ToLowerInvariant()), new Uri(_fixture.Server2.Urls[0] + "/timeout"))
            { Timeout = 2000 };
        await Assert.ThrowsAsync<RdfQueryTimeoutException>(async () =>
            await endpoint.QueryWithResultGraphAsync("CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }", TestContext.Current.CancellationToken));

    }

    [Fact]
    public async Task QueryWithResultSetAllowsTimeoutsToBeIgnored()
    {
        var endpoint = new FederatedSparqlQueryClient(
                HttpClient, new Uri(_fixture.Server1.Urls[0] + "/query"), new Uri(_fixture.Server2.Urls[0] + "/timeout"))
        { Timeout = 3000, IgnoreFailedRequests = true };
        SparqlResultSet results = await endpoint.QueryWithResultSetAsync("SELECT * WHERE {?s ?p ?o}", TestContext.Current.CancellationToken);
        results.Should().NotBeNull().And.HaveCount(1);
        _fixture.Server1.FindLogEntries(new RequestMessagePathMatcher(MatchBehaviour.AcceptOnMatch, MatchOperator.Or, "/query"))
            .Should().HaveCount(1).And.Contain(x =>
                x.RequestMessage.Method.Equals("get", StringComparison.InvariantCultureIgnoreCase));
    }

    [Fact]
    public async Task QueryWithResultGraphAllowsTimeoutsToBeIgnored()
    {
        var server1Path = "/query2/" + Guid.NewGuid().ToString("D").ToLowerInvariant();
        var endpoint = new FederatedSparqlQueryClient(
                HttpClient, new Uri(_fixture.Server1.Urls[0] + server1Path), new Uri(_fixture.Server2.Urls[0] + "/timeout"))
            { Timeout = 3000, IgnoreFailedRequests = true };
        IGraph results = await endpoint.QueryWithResultGraphAsync("CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }", TestContext.Current.CancellationToken);
        results.Should().NotBeNull();
        results.Triples.Count.Should().Be(1);
        _fixture.Server1.FindLogEntries(new RequestMessagePathMatcher(MatchBehaviour.AcceptOnMatch, MatchOperator.Or, server1Path))
            .Should().HaveCount(1).And.Contain(x =>
                x.RequestMessage.Method.Equals("get", StringComparison.InvariantCultureIgnoreCase));
    }
}
