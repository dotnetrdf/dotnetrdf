using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using VDS.RDF.Update;
using WireMock.Matchers;
using WireMock.Matchers.Request;
using Xunit;

namespace VDS.RDF.Query;

public class SparqlUpdateClientTests : IClassFixture<MockRemoteUpdateEndpointFixture>
{
    private static readonly HttpClient HttpClient = new HttpClient();
    private readonly MockRemoteUpdateEndpointFixture _fixture;

    public SparqlUpdateClientTests(MockRemoteUpdateEndpointFixture fixture)
    {
        _fixture = fixture;
        _fixture.Server.ResetLogEntries();
    }

    private SparqlUpdateClient GetUpdateClient()
    {
        return new SparqlUpdateClient(HttpClient, new Uri(_fixture.Server.Urls[0] + "/update"));
    }

    [Fact]
    public async Task ItDefaultsToPostForAShortUpdate()
    {
        const string input = "LOAD <http://dbpedia.org/resource/Ilkeston>";

        var client = GetUpdateClient();
        await client.UpdateAsync(input, TestContext.Current.CancellationToken);
        var logEntries = _fixture.Server.FindLogEntries(new RequestMessagePathMatcher(MatchBehaviour.AcceptOnMatch, MatchOperator.Or, "/update")).ToList();
        logEntries.Should().HaveCount(1);
        logEntries[0].RequestMessage.Method.Should().BeEquivalentTo("post");
    }

    [Fact]
    public async Task ItDefaultsToPostForALongUpdate()
    {
        var input = new StringBuilder();
        input.AppendLine("LOAD <http://dbpedia.org/resource/Ilkeston>");
        input.AppendLine(new string('#', 2048));

        var client = GetUpdateClient();
        await client.UpdateAsync(input.ToString(), TestContext.Current.CancellationToken);
        var logEntries = _fixture.Server.FindLogEntries(new RequestMessagePathMatcher(MatchBehaviour.AcceptOnMatch, MatchOperator.Or, "/update")).ToList();
        logEntries.Should().HaveCount(1);
        logEntries[0].RequestMessage.Method.Should().BeEquivalentTo("post");
    }

    [Fact]
    public async Task ItPropagatesHttpErrorsAsExceptions()
    {
        const string input = "DELETE <http://dbpedia.org/resource/Ilkeston>";
        var client = GetUpdateClient();
        var ex = await Assert.ThrowsAsync<SparqlUpdateException>(async () => { await client.UpdateAsync(input, TestContext.Current.CancellationToken); });
        ex.Message.Should().Contain("403");
    }
}
