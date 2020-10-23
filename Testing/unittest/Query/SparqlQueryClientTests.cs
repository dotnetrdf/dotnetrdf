using System;
using System.Net.Http;
using FluentAssertions;
using VDS.RDF.Parsing.Handlers;
using Xunit;

namespace VDS.RDF.Query
{
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
        public async void SelectWithResultSet()
        {
            var client = GetQueryClient();
            var resultSet = await client.QueryWithResultSetAsync(_fixture.SelectQuery);
            resultSet.Count.Should().Be(1);
        }

        [Fact]
        public async void SelectWithCountHandler()
        {
            var client = GetQueryClient();
            var handler = new ResultCountHandler();
            await client.QueryWithResultSetAsync(_fixture.SelectQuery, handler);
            handler.Count.Should().Be(1);
        }

        [Fact]
        public async void SelectRaisesExceptionOnServerError()
        {
            var client = GetQueryClient();
            var ex = await Assert.ThrowsAsync<RdfQueryException>(async () => await client.QueryWithResultSetAsync(_fixture.ErrorSelectQuery));
            ex.Message.Should().Contain("400");
        }

        [Fact]
        public async void ConstructWithResultGraph()
        {
            var client = GetQueryClient();
            var resultGraph = await client.QueryWithResultGraphAsync(_fixture.ConstructQuery);
            resultGraph.Triples.Count.Should().Be(1);
        }

        [Fact]
        public async void ConstructWithCountHandler()
        {
            var client = GetQueryClient();
            var handler = new CountHandler();
            await client.QueryWithResultGraphAsync(_fixture.ConstructQuery, handler);
            handler.Count.Should().Be(1);
        }

        [Fact]
        public async void ConstructRaisesExceptionOnServerError()
        {
            var client = GetQueryClient();
            var ex = await Assert.ThrowsAsync<RdfQueryException>(async () => await client.QueryWithResultGraphAsync(_fixture.ErrorConstructQuery));
            ex.Message.Should().Contain("400");
        }
    }
}
