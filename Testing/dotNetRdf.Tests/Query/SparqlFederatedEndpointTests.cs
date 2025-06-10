using System;
using FluentAssertions;
using Xunit;

namespace VDS.RDF.Query;

[Obsolete("Tests for obsolete classes")]
public class SparqlFederatedEndpointTests : IClassFixture<FederatedEndpointFixture>
{
    private readonly FederatedEndpointFixture _fixture;

    public SparqlFederatedEndpointTests(FederatedEndpointFixture fixture)
    {
        _fixture = fixture;
        _fixture.Server1.ResetLogEntries();
        _fixture.Server2.ResetLogEntries();
    }

    [Fact]
    public void ItCombinesResultsFromFederatedEndpoints()
    {
        var endpoint = new FederatedSparqlRemoteEndpoint(
            new[]
            {
                new SparqlRemoteEndpoint(new Uri(_fixture.Server1.Urls[0] + "/query")),
                new SparqlRemoteEndpoint(new Uri(_fixture.Server2.Urls[0] + "/query")),
            });
        SparqlResultSet results = endpoint.QueryWithResultSet("SELECT * WHERE {?s ?p ?o}");
        results.Should().NotBeNull().And.HaveCount(2);
    }

    [Fact]
    public void ItCombinesGraphsFromFederatedEndpoints()
    {
        var endpoint = new FederatedSparqlRemoteEndpoint(new []
        {
            new SparqlRemoteEndpoint(new Uri(_fixture.Server1.Urls[0] + "/query2")), 
            new SparqlRemoteEndpoint(new Uri(_fixture.Server2.Urls[0] + "/query2")), 
        });
        IGraph resultGraph = endpoint.QueryWithResultGraph("CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }");
        resultGraph.Should().NotBeNull();
        resultGraph.Triples.Should().HaveCount(2);
    }

    [Fact]
    public void ItThrowsAnExceptionIfOneEndpointFails()
    {
        var endpoint = new FederatedSparqlRemoteEndpoint(new []
        {
            new SparqlRemoteEndpoint(new Uri(_fixture.Server1.Urls[0] + "/query")), 
            new SparqlRemoteEndpoint(new Uri(_fixture.Server2.Urls[0] + "/fail")), 
        });
        Assert.Throws<RdfQueryException>(() => endpoint.QueryWithResultSet("SELECT * WHERE { ?s ?p ?o }")).InnerException.Should().BeOfType<RdfQueryException>();
    }

    [Fact]
    public void ItAllowsEndpointErrorsToBeIgnored()
    {
        var endpoint = new FederatedSparqlRemoteEndpoint(new[]
            {
                new SparqlRemoteEndpoint(new Uri(_fixture.Server1.Urls[0] + "/query")),
                new SparqlRemoteEndpoint(new Uri(_fixture.Server2.Urls[0] + "/fail")),
            })
            {IgnoreFailedRequests = true};
        SparqlResultSet results = endpoint.QueryWithResultSet("SELECT * WHERE {?s ?p ?o}");
        results.Should().NotBeNull().And.HaveCount(1);
    }

    [Fact]
    public void ItThrowsAnExceptionIfOneEndpointTimesOut()
    {
        var endpoint = new FederatedSparqlRemoteEndpoint(new[]
            {
                new SparqlRemoteEndpoint(new Uri(_fixture.Server1.Urls[0] + "/query")),
                new SparqlRemoteEndpoint(new Uri(_fixture.Server2.Urls[0] + "/timeout")),
            })
            {Timeout = 2000};
        Assert.Throws<RdfQueryTimeoutException>(() => endpoint.QueryWithResultSet("SELECT * WHERE { ?s ?p ?o }"));

    }

    [Fact]
    public void ItAllowsTimeoutsToBeIgnored()
    {
        var endpoint = new FederatedSparqlRemoteEndpoint(new[]
            {
                new SparqlRemoteEndpoint(new Uri(_fixture.Server1.Urls[0] + "/query")),
                new SparqlRemoteEndpoint(new Uri(_fixture.Server2.Urls[0] + "/timeout")),
            })
            { Timeout = 3000, IgnoreFailedRequests = true};
        SparqlResultSet results = endpoint.QueryWithResultSet("SELECT * WHERE {?s ?p ?o}");
        results.Should().NotBeNull().And.HaveCount(1);
    }
}
