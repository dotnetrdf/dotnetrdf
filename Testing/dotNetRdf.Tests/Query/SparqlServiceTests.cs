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

using FluentAssertions;
using System;
using System.Linq;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Writing;
using WireMock.Matchers;
using WireMock.Matchers.Request;
using Xunit;

namespace VDS.RDF.Query;

public class SparqlServiceTests : IClassFixture<MockRemoteSparqlEndpointFixture>
{
    private readonly MockRemoteSparqlEndpointFixture _serverFixture;

    public SparqlServiceTests(MockRemoteSparqlEndpointFixture serverFixture)
    {
        _serverFixture = serverFixture;
    }

    private string ServiceUri => _serverFixture.ServiceUri;

    private Predicate<string> SameAsQuery(string expectedQuery) =>
        actualQuery => actualQuery == new SparqlQueryParser().ParseFromString(expectedQuery).ToString();

    private Predicate<string> QueryWithInlineData(int expectedNumberOfInlineTuples) =>
        actualQuery =>
        {
            var parsedQuery = new SparqlQueryParser().ParseFromString(actualQuery);
            var actualNumberOfInlineTuples = parsedQuery.RootGraphPattern.InlineData.Tuples.Count();
            return actualNumberOfInlineTuples == expectedNumberOfInlineTuples;
        };

    [Fact]
    public void ItCallsServiceWithoutInlineData()
    {
        var dataset = CreateDataset(numberOfTriples: 0);
        var query = $"SELECT * WHERE {{ SERVICE <{ServiceUri}> {{?s ?p ?o}} }}";
        var expectedServiceQuery = "SELECT * WHERE { ?s ?p ?o }";
        var serviceResults = XmlFormat(CreateResults(1));
        _serverFixture.RegisterSelectQueryGetHandler(SameAsQuery(expectedServiceQuery), serviceResults);
        _serverFixture.Server.ResetLogEntries();

        var results = ProcessQuery(dataset, query);

        results.Should().NotBeNull().And.HaveCount(1);
        var sparqlLogEntries = _serverFixture.Server.FindLogEntries(new RequestMessagePathMatcher(MatchBehaviour.AcceptOnMatch, MatchOperator.Or, "/sparql")).ToList();
        sparqlLogEntries.Should().HaveCount(1);
        sparqlLogEntries[0].RequestMessage.Method.Should().BeEquivalentTo("get");
    }

    [Fact]
    public void ItCallsServiceWithInlineData()
    {
        var dataset = CreateDataset(numberOfTriples: 2);
        var query = $"SELECT * WHERE {{ ?s ?p ?o SERVICE <{ServiceUri}> {{?s ?p ?o}} }}";
        var serviceResults = XmlFormat(CreateResults(numberOfTriples: 2));
        _serverFixture.RegisterSelectQueryGetHandler(QueryWithInlineData(2), serviceResults);
        _serverFixture.Server.ResetLogEntries();

        var results = ProcessQuery(dataset, query);

        results.Should().NotBeNull().And.HaveCount(2);
        var sparqlLogEntries = _serverFixture.Server.FindLogEntries(new RequestMessagePathMatcher(MatchBehaviour.AcceptOnMatch, MatchOperator.Or, "/sparql")).ToList();
        sparqlLogEntries.Should().HaveCount(1);
        sparqlLogEntries[0].RequestMessage.Method.Should().BeEquivalentTo("get");
    }

    [Fact]
    public void ItCallsServiceWithInlineDataInChunks()
    {
        var dataset = CreateDataset(numberOfTriples: 102);
        var query = $"SELECT * WHERE {{ ?s ?p ?o SERVICE <{ServiceUri}> {{?s ?p ?o}} }}";
        var resultsOfPost = XmlFormat(CreateResults(numberOfTriples: 100)); 
        var resultsOfGet = XmlFormat(CreateResults(numberOfTriples: 2));
        _serverFixture.RegisterSelectQueryPostHandler(QueryWithInlineData(100), resultsOfPost);
        _serverFixture.RegisterSelectQueryGetHandler(QueryWithInlineData(2), resultsOfGet);
        _serverFixture.Server.ResetLogEntries();

        var results = ProcessQuery(dataset, query);

        results.Should().NotBeNull().And.HaveCount(102);
        var sparqlLogEntries = _serverFixture.Server.FindLogEntries(new RequestMessagePathMatcher(MatchBehaviour.AcceptOnMatch, MatchOperator.Or, "/sparql")).ToList();
        sparqlLogEntries.Should().HaveCount(2);
        sparqlLogEntries[0].RequestMessage.Method.Should().BeEquivalentTo("post");
        sparqlLogEntries[1].RequestMessage.Method.Should().BeEquivalentTo("get");
    }

    [Fact]
    public void ItResolvesEndpointFromVariable()
    {
        var dataset = CreateDataset(numberOfTriples: 1);
        var query = $"SELECT * WHERE {{ VALUES ?service {{<{ServiceUri}>}} SERVICE ?service {{?s ?p ?o}} }}";
        var serviceResults = XmlFormat(CreateResults(numberOfTriples: 1));
        _serverFixture.RegisterSelectQueryGetHandler(SameAsQuery("SELECT * WHERE {?s ?p ?o}"), serviceResults);
        _serverFixture.Server.ResetLogEntries();
        var results = ProcessQuery(dataset, query);

        results.Should().NotBeNull().And.HaveCount(1);
        var sparqlLogEntries = _serverFixture.Server.FindLogEntries(new RequestMessagePathMatcher(MatchBehaviour.AcceptOnMatch, MatchOperator.Or, "/sparql")).ToList();
        sparqlLogEntries.Should().HaveCount(1);
        sparqlLogEntries[0].RequestMessage.Method.Should().BeEquivalentTo("get");
    }

    [Fact]
    public void ItResolvesNoEndpointFromVariable()
    {
        var dataset = CreateDataset(numberOfTriples: 0);
        var query = $"SELECT * WHERE {{ VALUES ?service {{ 'noturi' }} SERVICE ?service {{?s ?p ?o}} }}";

        var results = ProcessQuery(dataset, query);

        results.Should().NotBeNull().And.HaveCount(0);
    }

    private static SparqlResultSet ProcessQuery(InMemoryDataset dataset, string query)
    {
        var parsedQuery = new SparqlQueryParser().ParseFromString(query);
        var processor = new LeviathanQueryProcessor(dataset);
        return (SparqlResultSet) processor.ProcessQuery(parsedQuery);
    }

    private static IGraph CreateGraph(int numberOfTriples)
    {
        var graph = new Graph();
        for (int i = 0; i < numberOfTriples; i++)
        {
            graph.Assert(
                graph.CreateUriNode(new Uri("http://example.org/s")),
                graph.CreateUriNode(new Uri("http://example.org/p")),
                graph.CreateLiteralNode(i.ToString()));
        }
        return graph;
    }

    private static InMemoryDataset CreateDataset(int numberOfTriples)
    {
        var dataset = new InMemoryDataset(new TripleStore(), true);
        dataset.AddGraph(CreateGraph(numberOfTriples));
        return dataset;
    }

    private static SparqlResultSet CreateResults(int numberOfTriples) => 
        (SparqlResultSet)CreateGraph(numberOfTriples).ExecuteQuery("SELECT * WHERE {?s ?p ?o}");

    private static string XmlFormat(SparqlResultSet results)
    {
        var stringWriter = new System.IO.StringWriter();
        new SparqlXmlWriter().Save(results, stringWriter);
        return stringWriter.ToString();
    }
}
