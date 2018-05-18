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
using System.Net;
using System.Text;
using System.Web;
using FluentAssertions;
using Moq;
using VDS.RDF;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using WireMock.Matchers;
using WireMock.Matchers.Request;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace dotNetRDF.MockServerTests
{
    public partial class SparqlRemoteEndpointTests : IDisposable
    {
        private readonly FluentMockServer _server;

        public SparqlRemoteEndpointTests()
        {
            _server = FluentMockServer.Start();
        }

        public void Dispose()
        {
            _server.Stop();
        }

        private const string SparqlResultsXml = @"<?xml version=""1.0""?>
<sparql xmlns='http://www.w3.org/2005/sparql-results#'>
    <head>
        <variable name='s'/>
        <variable name='p'/>
        <variable name='o'/>
    </head>
    <results>
        <result>
            <binding name='s'><uri>http://example.org/s</uri></binding>
            <binding name='p'><uri>http://example.org/s</uri></binding>
            <binding name='o'><literal>o</literal></binding>
        </result>
    </results>
</sparql>";

        private const string ConstructResults = "<http://example.org/s> <http://example.org/p> \"o\" .";

        private void RegisterSelectQueryGetHandler(string query = "SELECT * WHERE {?s ?p ?o}")
        {
            _server.Given(Request.Create()
                                 .WithPath("/sparql")
                                 .UsingGet()
                                 .WithParam(queryParams=>
                                                queryParams.ContainsKey("query") && 
                                                queryParams["query"].Any(q => HttpUtility.UrlDecode(q).StartsWith(query))))
                   .RespondWith(Response.Create()
                                        .WithBody(SparqlResultsXml, encoding:Encoding.UTF8)
                                        .WithHeader("Content-Type", MimeTypesHelper.SparqlResultsXml[0])
                                        .WithStatusCode(HttpStatusCode.OK));
        }

        private void RegisterConstructQueryGetHandler()
        {
            _server.Given(Request.Create()
                    .WithPath("/sparql")
                    .UsingGet()
                    .WithParam(queryParams =>
                        queryParams.ContainsKey("query") &&
                        queryParams["query"].Any(q => HttpUtility.UrlDecode(q).Equals("CONSTRUCT {?s ?p ?o} WHERE {?s ?p ?o}"))))
                .RespondWith(Response.Create()
                    .WithBody(ConstructResults, encoding: Encoding.UTF8)
                    .WithHeader("Content-Type", "application/n-triples")
                    .WithStatusCode(HttpStatusCode.OK));
        }

        private void RegisterSelectQueryPostHandler()
        {
            _server.Given(Request.Create()
                    .WithPath("/sparql")
                    .UsingPost()
                    .WithBody(x=>x.Contains("query=" + HttpUtility.UrlEncode("SELECT * WHERE {?s ?p ?o}"))))
                .RespondWith(Response.Create()
                    .WithBody(SparqlResultsXml, encoding: Encoding.UTF8)
                    .WithHeader("Content-Type", MimeTypesHelper.SparqlResultsXml[0])
                    .WithStatusCode(HttpStatusCode.OK));
        }
        private SparqlRemoteEndpoint GetQueryEndpoint()
        {
            return new SparqlRemoteEndpoint(new Uri(_server.Urls[0] + "sparql"));
        }

        [Fact]
        public void ItDefaultsToGetForShortQueries()
        {
            RegisterSelectQueryGetHandler();
            var endpoint = GetQueryEndpoint();
            var results = endpoint.QueryWithResultSet("SELECT * WHERE {?s ?p ?o}");
            results.Should().NotBeNull().And.HaveCount(1);
            var sparqlLogEntries = _server.FindLogEntries(new RequestMessagePathMatcher(MatchBehaviour.AcceptOnMatch, "/sparql")).ToList();
            sparqlLogEntries.Should().HaveCount(1);
            sparqlLogEntries[0].RequestMessage.Method.Should().BeEquivalentTo("get");
        }
        
        [Fact]
        public void ItDefaultsToPostForLongQueries()
        {
            RegisterSelectQueryPostHandler();

            var input = new StringBuilder();
            input.AppendLine("SELECT * WHERE {?s ?p ?o}");
            input.AppendLine(new string('#', 2048));

            var endpoint = GetQueryEndpoint();
            var results = endpoint.QueryWithResultSet(input.ToString());
            results.Should().HaveCount(1);
            var sparqlLogEntries = _server.FindLogEntries(new RequestMessagePathMatcher(MatchBehaviour.AcceptOnMatch, "/sparql")).ToList();
            sparqlLogEntries.Should().HaveCount(1);
            sparqlLogEntries[0].RequestMessage.Method.Should().BeEquivalentTo("post");
        }

        [Fact]
        public void ItAllowsLongQueriesToBeForcedToUseGet()
        {
            RegisterSelectQueryGetHandler();
            var endpoint = GetQueryEndpoint();
            endpoint.HttpMode = "GET";

            var input = new StringBuilder();
            input.AppendLine("SELECT * WHERE {?s ?p ?o}");
            input.AppendLine(new string('#', 2048));

            var results = endpoint.QueryWithResultSet(input.ToString());
            results.Should().NotBeNull().And.HaveCount(1);
            var sparqlLogEntries = _server.FindLogEntries(new RequestMessagePathMatcher(MatchBehaviour.AcceptOnMatch, "/sparql")).ToList();
            sparqlLogEntries.Should().HaveCount(1);
            sparqlLogEntries[0].RequestMessage.Method.Should().BeEquivalentTo("get");
        }

        [Fact]
        public void ItAllowsNonAsciCharactersToBeForcedToUseGet()
        {
            RegisterSelectQueryGetHandler("SELECT * WHERE {?s ?p \"\u6E0B\u8c37\u99c5\"}");
            var endpoint = GetQueryEndpoint();
            endpoint.HttpMode = "GET";

            var input = new StringBuilder();
            input.AppendLine("SELECT * WHERE {?s ?p \"\u6E0B\u8c37\u99c5\"}");

            var results = endpoint.QueryWithResultSet(input.ToString());
            results.Should().NotBeNull().And.HaveCount(1);
            var sparqlLogEntries = _server.FindLogEntries(new RequestMessagePathMatcher(MatchBehaviour.AcceptOnMatch, "/sparql")).ToList();
            sparqlLogEntries.Should().HaveCount(1);
            sparqlLogEntries[0].RequestMessage.Method.Should().BeEquivalentTo("get");
        }

        [Fact]
        public void ItInvokesAnIRdfHandler()
        {
            RegisterConstructQueryGetHandler();
            var endpoint = GetQueryEndpoint();
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
            RegisterSelectQueryGetHandler();

            var endpoint = GetQueryEndpoint();
            endpoint.QueryWithResultSet(resultsHandler.Object, "SELECT * WHERE {?s ?p ?o}");

            resultsHandler.Verify(x => x.StartResults());
            resultsHandler.Verify(x => x.HandleVariable("s"), Times.Exactly(1));
            resultsHandler.Verify(x => x.HandleVariable("p"), Times.Exactly(1));
            resultsHandler.Verify(x => x.HandleVariable("o"), Times.Exactly(1));
            resultsHandler.Verify(x => x.HandleResult(It.IsAny<SparqlResult>()), Times.Exactly(1));
            resultsHandler.Verify(x => x.EndResults(true), Times.Exactly(1));
        }

        
    }
}
