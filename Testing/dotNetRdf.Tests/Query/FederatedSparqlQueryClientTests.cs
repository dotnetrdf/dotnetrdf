using System;
using System.Net.Http;
using FluentAssertions;
using WireMock.Matchers;
using WireMock.Matchers.Request;
using Xunit;

namespace VDS.RDF.Query
{
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
        public async void QueryWithResultSetCombinesResultsFromFederatedEndpoints()
        {
            var endpoint = new FederatedSparqlQueryClient(
                HttpClient, new Uri(_fixture.Server1.Urls[0] + "/query"), new Uri(_fixture.Server2.Urls[0] + "/query"));
            SparqlResultSet results = await endpoint.QueryWithResultSetAsync("SELECT * WHERE {?s ?p ?o}");
            results.Should().NotBeNull().And.HaveCount(2);
        }

        [Fact]
        public async void QueryWithResultGraphCombinesGraphsFromFederatedEndpoints()
        {
            var endpoint = new FederatedSparqlQueryClient(
                HttpClient, new Uri(_fixture.Server1.Urls[0] + "/query2"), new Uri(_fixture.Server2.Urls[0] + "/query2"));
            IGraph resultGraph = await endpoint.QueryWithResultGraphAsync("CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }");
            resultGraph.Should().NotBeNull();
            resultGraph.Triples.Should().HaveCount(2);
        }

        [Fact]
        public void QueryWithResultSetThrowsAnExceptionIfOneEndpointFails()
        {
            var endpoint = new FederatedSparqlQueryClient(
                HttpClient, new Uri(_fixture.Server1.Urls[0] + "/query"), new Uri(_fixture.Server2.Urls[0] + "/fail"));
            Assert.ThrowsAsync<RdfQueryException>(() => endpoint.QueryWithResultSetAsync("SELECT * WHERE { ?s ?p ?o }"));
        }

        [Fact]
        public async void QueryWithResultGraphThrowsAnExceptionIfOneEndpointFails()
        {
            var endpoint = new FederatedSparqlQueryClient(
                HttpClient, new Uri(_fixture.Server1.Urls[0] + "/query2"), new Uri(_fixture.Server2.Urls[0] + "/fail"));
            await Assert.ThrowsAsync<RdfQueryException>( () => endpoint.QueryWithResultGraphAsync("CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }"));
        }

        [Fact]
        public async void QueryWithResultSetAllowsEndpointErrorsToBeIgnored()
        {
            var endpoint = new FederatedSparqlQueryClient(
                    HttpClient, new Uri(_fixture.Server1.Urls[0] + "/query"), new Uri(_fixture.Server2.Urls[0] + "/fail"))
            { IgnoreFailedRequests = true };
            SparqlResultSet results = await endpoint.QueryWithResultSetAsync("SELECT * WHERE {?s ?p ?o}");
            results.Should().NotBeNull().And.HaveCount(1);
        }

        [Fact]
        public async void QueryWithResultGraphAllowsEndpointErrorsToBeIgnored()
        {
            var endpoint = new FederatedSparqlQueryClient(
                    HttpClient, new Uri(_fixture.Server1.Urls[0] + "/query2"), new Uri(_fixture.Server2.Urls[0] + "/fail"))
                {IgnoreFailedRequests = true};
            IGraph results = await endpoint.QueryWithResultGraphAsync("CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }");
            results.Should().NotBeNull();
            results.Triples.Count.Should().Be(1);
        }

        [Fact]
        public async void QueryWithResultSetThrowsAnExceptionIfOneEndpointTimesOut()
        {
            var endpoint = new FederatedSparqlQueryClient(
                    HttpClient, new Uri(_fixture.Server1.Urls[0] + "/query"), new Uri(_fixture.Server2.Urls[0] + "/timeout"))
            { Timeout = 2000 };
            await Assert.ThrowsAsync<RdfQueryTimeoutException>(() => endpoint.QueryWithResultSetAsync("SELECT * WHERE { ?s ?p ?o }"));
        }

        [Fact]
        public async void QueryWithResultGraphThrowsAnExceptionIfOneEndpointTimesOut()
        {
            var endpoint = new FederatedSparqlQueryClient(
                    HttpClient, new Uri(_fixture.Server1.Urls[0] + "/query2"), new Uri(_fixture.Server2.Urls[0] + "/timeout"))
                { Timeout = 2000 };
            await Assert.ThrowsAsync<RdfQueryTimeoutException>(async () =>
                await endpoint.QueryWithResultGraphAsync("CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }"));

        }

        [Fact]
        public async void QueryWithResultSetAllowsTimeoutsToBeIgnored()
        {
            var endpoint = new FederatedSparqlQueryClient(
                    HttpClient, new Uri(_fixture.Server1.Urls[0] + "/query"), new Uri(_fixture.Server2.Urls[0] + "/timeout"))
            { Timeout = 3000, IgnoreFailedRequests = true };
            SparqlResultSet results = await endpoint.QueryWithResultSetAsync("SELECT * WHERE {?s ?p ?o}");
            results.Should().NotBeNull().And.HaveCount(1);
            _fixture.Server1.FindLogEntries(new RequestMessagePathMatcher(MatchBehaviour.AcceptOnMatch, "/query"))
                .Should().HaveCount(1).And.Contain(x =>
                    x.RequestMessage.Method.Equals("get", StringComparison.InvariantCultureIgnoreCase));
        }

        [Fact]
        public async void QueryWithResultGraphAllowsTimeoutsToBeIgnored()
        {
            var endpoint = new FederatedSparqlQueryClient(
                    HttpClient, new Uri(_fixture.Server1.Urls[0] + "/query2"), new Uri(_fixture.Server2.Urls[0] + "/timeout"))
                { Timeout = 3000, IgnoreFailedRequests = true };
            IGraph results = await endpoint.QueryWithResultGraphAsync("CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }");
            results.Should().NotBeNull();
            results.Triples.Count.Should().Be(1);
            _fixture.Server1.FindLogEntries(new RequestMessagePathMatcher(MatchBehaviour.AcceptOnMatch, "/query2"))
                .Should().HaveCount(1).And.Contain(x =>
                    x.RequestMessage.Method.Equals("get", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
