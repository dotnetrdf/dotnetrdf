using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using FluentAssertions;
using VDS.RDF.Update;
using WireMock.Matchers;
using WireMock.Matchers.Request;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace dotNetRDF.MockServerTests
{
    public partial class SparqlRemoteUpdateEndpointTests : IClassFixture<MockRemoteUpdateEndpointFixture>
    {
        private readonly MockRemoteUpdateEndpointFixture _fixture;
        public SparqlRemoteUpdateEndpointTests(MockRemoteUpdateEndpointFixture fixture)
        {
            _fixture = fixture;
            _fixture.Server.ResetLogEntries();
        }

        private SparqlRemoteUpdateEndpoint GetUpdateEndpoint()
        {
            return new SparqlRemoteUpdateEndpoint(new Uri(_fixture.Server.Urls[0] + "update"));
        }

        [Fact]
        public void ItDefaultsToPostForAShortUpdate()
        {
            const string input = "LOAD <http://dbpedia.org/resource/Ilkeston>";

            var endpoint = GetUpdateEndpoint();
            endpoint.Update(input);
            var logEntries = _fixture.Server.FindLogEntries(new RequestMessagePathMatcher(MatchBehaviour.AcceptOnMatch, "/update")).ToList();
            logEntries.Should().HaveCount(1);
            logEntries[0].RequestMessage.Method.Should().BeEquivalentTo("post");
        }

        [Fact]
        public void ItDefaultsToPostForALongUpdate()
        {
            var input = new StringBuilder();
            input.AppendLine("LOAD <http://dbpedia.org/resource/Ilkeston>");
            input.AppendLine(new string('#', 2048));

            var endpoint = GetUpdateEndpoint();
            endpoint.Update(input.ToString());
            var logEntries = _fixture.Server.FindLogEntries(new RequestMessagePathMatcher(MatchBehaviour.AcceptOnMatch, "/update")).ToList();
            logEntries.Should().HaveCount(1);
            logEntries[0].RequestMessage.Method.Should().BeEquivalentTo("post");
        }
        
    }

    public class MockRemoteUpdateEndpointFixture : IDisposable
    {
        public readonly FluentMockServer Server;

        public MockRemoteUpdateEndpointFixture()
        {
            Server = FluentMockServer.Start();
            Server.Given(Request.Create()
                                .WithPath("/update")
                                .UsingPost()
                                .WithBody(body=>HttpUtility.UrlDecode(body).StartsWith("update=LOAD <http://dbpedia.org/resource/Ilkeston>")))
                  .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.OK));
        }

        public void Dispose()
        {
            Server.Stop();
        }

        
    }
}
