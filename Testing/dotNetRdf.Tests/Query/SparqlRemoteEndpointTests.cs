/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2017 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Linq;
using System.Text;
using System.Threading;
using FluentAssertions;
using Moq;
using VDS.RDF.Parsing.Handlers;
using WireMock.Matchers;
using WireMock.Matchers.Request;
using Xunit;

namespace VDS.RDF.Query;

[Obsolete("Tests for obsolete classes")]
public class SparqlRemoteEndpointTests : IClassFixture<MockRemoteSparqlEndpointFixture>
{
    private readonly MockRemoteSparqlEndpointFixture _fixture;

    public SparqlRemoteEndpointTests(MockRemoteSparqlEndpointFixture fixture)
    {
        _fixture = fixture;
        _fixture.Server.ResetLogEntries();
    }

    private SparqlRemoteEndpoint GetQueryEndpoint()
    {
        return new SparqlRemoteEndpoint(new Uri(_fixture.Server.Urls[0] + "/sparql"));
    }

    [Fact]
    public void ItDefaultsToGetForShortQueries()
    {
        SparqlRemoteEndpoint endpoint = GetQueryEndpoint();
        SparqlResultSet results = endpoint.QueryWithResultSet("SELECT * WHERE { ?s ?p ?o . }");
        results.Should().NotBeNull().And.HaveCount(1);
        var sparqlLogEntries = _fixture.Server.FindLogEntries(new RequestMessagePathMatcher(MatchBehaviour.AcceptOnMatch, MatchOperator.Or, "/sparql")).ToList();
        sparqlLogEntries.Should().HaveCount(1);
        sparqlLogEntries[0].RequestMessage.Method.Should().BeEquivalentTo("get");
    }
    
    [Fact]
    public void ItDefaultsToPostForLongQueries()
    {
        var input = new StringBuilder();
        input.AppendLine("SELECT * WHERE {?s ?p ?o}");
        input.AppendLine(new string('#', 2048));

        SparqlRemoteEndpoint endpoint = GetQueryEndpoint();
        SparqlResultSet results = endpoint.QueryWithResultSet(input.ToString());
        results.Should().HaveCount(1);
        var sparqlLogEntries = _fixture.Server.FindLogEntries(new RequestMessagePathMatcher(MatchBehaviour.AcceptOnMatch, MatchOperator.Or, "/sparql")).ToList();
        sparqlLogEntries.Should().HaveCount(1);
        sparqlLogEntries[0].RequestMessage.Method.Should().BeEquivalentTo("post");
    }

    [Fact]
    public void ItAllowsLongQueriesToBeForcedToUseGet()
    {
        var queryString = "SELECT * WHERE {?s ?p ?o}" + new string('#', 2048);
        _fixture.RegisterSelectQueryGetHandler(queryString);
        SparqlRemoteEndpoint endpoint = GetQueryEndpoint();
        endpoint.HttpMode = "GET";

        SparqlResultSet results = endpoint.QueryWithResultSet(queryString);
        results.Should().NotBeNull().And.HaveCount(1);
        var sparqlLogEntries = _fixture.Server.FindLogEntries(new RequestMessagePathMatcher(MatchBehaviour.AcceptOnMatch, MatchOperator.Or, "/sparql")).ToList();
        sparqlLogEntries.Should().HaveCount(1);
        sparqlLogEntries[0].RequestMessage.Method.Should().BeEquivalentTo("get");
    }

    [Fact]
    public void ItAllowsNonAsciiCharactersToBeForcedToUseGet()
    {
        var queryString = "SELECT * WHERE {?s ?p \"\u6E0B\u8c37\u99c5\"}";
        _fixture.RegisterSelectQueryGetHandler(queryString);
        SparqlRemoteEndpoint endpoint = GetQueryEndpoint();
        endpoint.HttpMode = "GET";

        SparqlResultSet results = endpoint.QueryWithResultSet(queryString);
        results.Should().NotBeNull().And.HaveCount(1);
        var sparqlLogEntries = _fixture.Server.FindLogEntries(new RequestMessagePathMatcher(MatchBehaviour.AcceptOnMatch, MatchOperator.Or, "/sparql")).ToList();
        sparqlLogEntries.Should().HaveCount(1);
        sparqlLogEntries[0].RequestMessage.Method.Should().BeEquivalentTo("get");
    }

    [Fact]
    public void ItInvokesAnIRdfHandler()
    {
        SparqlRemoteEndpoint endpoint = GetQueryEndpoint();
        var handler= new CountHandler();
        endpoint.QueryWithResultGraph(handler, "CONSTRUCT {?s ?p ?o} WHERE {?s ?p ?o}");
        handler.Count.Should().Be(1);
    }
    

    [Fact]
    public void ItInvokesAnISparqlResultsHandler()
    {
        var resultsHandler = new Mock<ISparqlResultsHandler>();
        resultsHandler.Setup(x => x.HandleResult(It.IsAny<SparqlResult>())).Returns(true);
        resultsHandler.Setup(x => x.HandleVariable(It.IsAny<string>())).Returns(true);

        SparqlRemoteEndpoint endpoint = GetQueryEndpoint();
        endpoint.QueryWithResultSet(resultsHandler.Object, "SELECT * WHERE { ?s ?p ?o . }");

        resultsHandler.Verify(x => x.StartResults());
        resultsHandler.Verify(x => x.HandleVariable("s"), Times.Exactly(1));
        resultsHandler.Verify(x => x.HandleVariable("p"), Times.Exactly(1));
        resultsHandler.Verify(x => x.HandleVariable("o"), Times.Exactly(1));
        resultsHandler.Verify(x => x.HandleResult(It.IsAny<SparqlResult>()), Times.Exactly(1));
        resultsHandler.Verify(x => x.EndResults(true), Times.Exactly(1));
    }

    [Fact]
    public void SparqlRemoteEndpointAsyncApiQueryWithResultSet()
    {
        SparqlRemoteEndpoint endpoint = GetQueryEndpoint();
        var signal = new ManualResetEvent(false);
        var resultsCount = -1;
        endpoint.QueryWithResultSet("SELECT * WHERE {?s ?p ?o}", (r, _) =>
        {
            resultsCount = r.Count;
            signal.Set();
        }, null);

        var wasSet = signal.WaitOne(10000);
        wasSet.Should().BeTrue();
        resultsCount.Should().Be(1);
    }


    [Fact]
    public void SparqlRemoteEndpointAsyncApiQueryWithResultGraph()
    {
        SparqlRemoteEndpoint endpoint = GetQueryEndpoint();
        var signal = new ManualResetEvent(false);
        var resultsCount = -1;
        endpoint.QueryWithResultGraph(_fixture.ConstructQuery, (r, _) =>
        {
            if (r != null) resultsCount = r.Triples.Count;
            signal.Set();
        }, null);

        var wasSet = signal.WaitOne(10000);
        wasSet.Should().BeTrue();
        resultsCount.Should().Be(1);
    }


}
