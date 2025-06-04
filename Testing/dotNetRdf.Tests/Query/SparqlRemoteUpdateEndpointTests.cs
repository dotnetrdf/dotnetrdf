using System;
using System.Linq;
using System.Text;
using System.Threading;
using FluentAssertions;
using VDS.RDF.Update;
using WireMock.Matchers;
using WireMock.Matchers.Request;
using Xunit;

namespace VDS.RDF.Query;

[Obsolete("Tests for obsolete classes")]
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
        return new SparqlRemoteUpdateEndpoint(new Uri(_fixture.Server.Urls[0] + "/update"));
    }

    [Fact]
    public void ItDefaultsToPostForAShortUpdate()
    {
        const string input = "LOAD <http://dbpedia.org/resource/Ilkeston>";

        var endpoint = GetUpdateEndpoint();
        endpoint.Update(input);
        var logEntries = _fixture.Server.FindLogEntries(new RequestMessagePathMatcher(MatchBehaviour.AcceptOnMatch, MatchOperator.Or, "/update")).ToList();
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
        var logEntries = _fixture.Server.FindLogEntries(new RequestMessagePathMatcher(MatchBehaviour.AcceptOnMatch, MatchOperator.Or, "/update")).ToList();
        logEntries.Should().HaveCount(1);
        logEntries[0].RequestMessage.Method.Should().BeEquivalentTo("post");
    }

    [Fact]
    public void SparqlRemoteEndpointAsyncApiUpdate()
    {

        SparqlRemoteUpdateEndpoint endpoint = GetUpdateEndpoint();
        var signal = new ManualResetEvent(false);
        endpoint.Update("LOAD <http://dbpedia.org/resource/Ilkeston> INTO GRAPH <http://example.org/async/graph>", s =>
        {
            signal.Set();
        }, null);

        var wasSet = signal.WaitOne(TimeSpan.FromSeconds(15.0));
        Assert.True(wasSet);
    }

}
