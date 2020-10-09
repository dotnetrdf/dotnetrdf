using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using FluentAssertions;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using Xunit;

namespace dotNetRDF.MockServerTests
{
    public class SparqlRemoteClientTests : SparqlRemoteTestsBase
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        private SparqlRemoteClient GetQueryClient()
        {
            return new SparqlRemoteClient(_httpClient, new Uri(_server.Urls[0] + "/sparql"));
        }


        [Fact]
        public async void SelectWithResultSet()
        {
            RegisterSelectQueryGetHandler();
            var client = GetQueryClient();
            var resultSet = await client.QueryWithResultSetAsync(SelectQuery);
            resultSet.Count.Should().Be(1);
        }

        [Fact]
        public async void SelectWithCountHandler()
        {
            RegisterSelectQueryGetHandler();
            var client = GetQueryClient();
            var handler = new ResultCountHandler();
            await client.QueryWithResultSetAsync(SelectQuery, handler);
            handler.Count.Should().Be(1);
        }

        [Fact]
        public async void SelectRaisesExceptionOnServerError()
        {
            RegisterErrorHandler();
            var client = GetQueryClient();
            try
            {
                var resultSet = await client.QueryWithResultSetAsync(SelectQuery);
                Assert.True(false, "Expected an exception to be raised");
            }
            catch (RdfQueryException)
            {
                // Expected
            }
        }

        [Fact]
        public async void ConstructWithResultGraph()
        {
            RegisterConstructQueryGetHandler();
            var client = GetQueryClient();
            var resultGraph = await client.QueryWithResultGraphAsync(ConstructQuery);
            resultGraph.Triples.Count.Should().Be(1);
        }

        [Fact]
        public async void ConstructWithCountHandler()
        {
            RegisterConstructQueryGetHandler();
            var client = GetQueryClient();
            var handler = new CountHandler();
            await client.QueryWithResultGraphAsync(ConstructQuery, handler);
            handler.Count.Should().Be(1);
        }

        [Fact]
        public async void ConstructRaisesExceptionOnServerError()
        {
            RegisterErrorHandler();
            var client = GetQueryClient();
            try
            {
                var resultSet = await client.QueryWithResultGraphAsync(ConstructQuery);
                Assert.True(false, "Expected an exception to be raised");
            }
            catch (RdfQueryException)
            {
                // Expected
            }
        }
    }
}
