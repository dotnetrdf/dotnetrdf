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
using System.Net.Http;
using System.Text;
using System.Threading;
using FluentAssertions;
using Moq;
using VDS.RDF;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using WireMock.Matchers;
using WireMock.Matchers.Request;
using Xunit;

namespace dotNetRDF.MockServerTests
{
    public class SparqlRemoteEndpointTests : SparqlRemoteTestsBase
    {

        private SparqlRemoteEndpoint GetQueryEndpoint()
        {
            return new SparqlRemoteEndpoint(new Uri(_server.Urls[0] + "/sparql"));
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

        [Fact]
        public void SparqlRemoteEndpointAsyncApiQueryWithResultSet()
        {
            RegisterSelectQueryPostHandler(); // Async methods always use POST
            SparqlRemoteEndpoint endpoint = GetQueryEndpoint();
            ManualResetEvent signal = new ManualResetEvent(false);
            int resultsCount = -1;
            endpoint.QueryWithResultSet("SELECT * WHERE {?s ?p ?o}", (r, s) =>
            {
                resultsCount = r.Count;
                signal.Set();
            }, null);

            bool wasSet = signal.WaitOne(10000);
            wasSet.Should().BeTrue();
            resultsCount.Should().Be(1);
        }


        [Fact]
        public void SparqlRemoteEndpointAsyncApiQueryWithResultGraph()
        {
            RegisterConstructQueryPostHandler(); // Async methods always use POST
            SparqlRemoteEndpoint endpoint = GetQueryEndpoint();
            ManualResetEvent signal = new ManualResetEvent(false);
            var resultsCount = -1;
            endpoint.QueryWithResultGraph(ConstructQuery, (r, s) =>
            {
                if (r != null) resultsCount = r.Triples.Count;
                signal.Set();
            }, null);

            bool wasSet = signal.WaitOne(10000);
            wasSet.Should().BeTrue();
            resultsCount.Should().Be(1);
        }


    }
}
