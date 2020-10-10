using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using FluentAssertions;
using VDS.RDF.Query;
using VDS.RDF.Update;
using WireMock.Matchers;
using WireMock.Matchers.Request;
using Xunit;

namespace dotNetRDF.MockServerTests
{
    public class SparqlRemoteUpdateClientTests : IClassFixture<MockRemoteUpdateEndpointFixture>
    {
        private MockRemoteUpdateEndpointFixture _fixture;

        public SparqlRemoteUpdateClientTests(MockRemoteUpdateEndpointFixture fixture)
        {
            _fixture = fixture;
            _fixture.Server.ResetLogEntries();
        }

        private SparqlRemoteUpdateClient GetUpdateClient()
        {
            return new SparqlRemoteUpdateClient(new HttpClient(), new Uri(_fixture.Server.Urls[0] + "/update"));
        }

        [Fact]
        public async void ItDefaultsToPostForAShortUpdate()
        {
            const string input = "LOAD <http://dbpedia.org/resource/Ilkeston>";

            var client = GetUpdateClient();
            await client.UpdateAsync(input);
            var logEntries = _fixture.Server.FindLogEntries(new RequestMessagePathMatcher(MatchBehaviour.AcceptOnMatch, "/update")).ToList();
            logEntries.Should().HaveCount(1);
            logEntries[0].RequestMessage.Method.Should().BeEquivalentTo("post");
        }

        [Fact]
        public async void ItDefaultsToPostForALongUpdate()
        {
            var input = new StringBuilder();
            input.AppendLine("LOAD <http://dbpedia.org/resource/Ilkeston>");
            input.AppendLine(new string('#', 2048));

            var client = GetUpdateClient();
            await client.UpdateAsync(input.ToString());
            var logEntries = _fixture.Server.FindLogEntries(new RequestMessagePathMatcher(MatchBehaviour.AcceptOnMatch, "/update")).ToList();
            logEntries.Should().HaveCount(1);
            logEntries[0].RequestMessage.Method.Should().BeEquivalentTo("post");
        }

        [Fact]
        public async void ItPropagatesHttpErrorsAsExceptions()
        {
            const string input = "DELETE <http://dbpedia.org/resource/Ilkeston>";
            var client = GetUpdateClient();
            var ex = await Assert.ThrowsAsync<SparqlUpdateException>(async () => { await client.UpdateAsync(input); });
            ex.Message.Should().Contain("403");
        }
    }
}
