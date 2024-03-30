using dotNetRdf.TestSupport;
using FluentAssertions;
using System;
using System.IO;
using System.Threading.Tasks;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using Xunit;
using Xunit.Abstractions;

namespace VDS.RDF.TestSuite.Rdf11;

public abstract class BaseAsyncSparqlEvaluationTestSuite(ITestOutputHelper output) : RdfTestSuite
{
    public static ManifestTestDataProvider SparqlQueryEvalTests = new ManifestTestDataProvider(
        new Uri("http://example/base/manifest.ttl"),
        Path.Combine("resources", "sparql11", "data-sparql11", "manifest-sparql11-query.ttl"));

    public static ManifestTestDataProvider DawgQueryEvalTests = new(
        new Uri("http://example/base/manifest.ttl"),
        Path.Combine("resources", "sparql11", "data-r2", "manifest-evaluation.ttl"));

    protected abstract Task<object> ProcessQueryAsync(TripleStore tripleStore, SparqlQuery query);

    protected async void PerformQueryEvaluationTest(ManifestTestData t)
    {
        output.WriteLine($"{t.Id}: {t.Name} is a {t.Type}");
        await InvokeTestRunnerAsync(t);
    }

    protected void PerformQueryEvaluationTest(Uri testUri)
    {
        PerformQueryEvaluationTest(SparqlQueryEvalTests.GetTestData(testUri));
    }

    private async Task InvokeTestRunnerAsync(ManifestTestData t)
    {
        switch (t.Type.AbsoluteUri)
        {
            case "http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#QueryEvaluationTest":
                await QueryEvaluationTest(t);
                break;
            case "http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#PositiveSyntaxTest11":
                PositiveSyntaxTest(t);
                break;
            case "http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#NegativeSyntaxTest11":
                NegativeSyntaxTest(t);
                break;
            default:
                Assert.Fail("Unrecognised test type " + t.Type.AbsoluteUri);
                break;
        }
    }

    [ManifestTestRunner("http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#QueryEvaluationTest")]
    private async Task QueryEvaluationTest(ManifestTestData t)
    {
        var queryInputPath = t.Manifest.ResolveResourcePath(t.Query);
        var dataInputPath = t.Manifest.ResolveResourcePath(t.Data);
        var resultInputPath = t.Manifest.ResolveResourcePath(t.Result);
        SparqlResultSet resultSet = null;
        Graph resultGraph = null;
        var expectGraphResult = false;

        if (resultInputPath.EndsWith(".srj"))
        {
            resultSet = new SparqlResultSet();
            var resultParser = new SparqlJsonParser();
            resultParser.Load(resultSet, resultInputPath);
        }
        else if (resultInputPath.EndsWith(".srx"))
        {
            resultSet = new SparqlResultSet();
            var resultParser = new SparqlXmlParser();
            resultParser.Load(resultSet, resultInputPath);
        }
        else if (resultInputPath.EndsWith(".ttl"))
        {
            resultGraph = new Graph();
            resultGraph.LoadFromFile(resultInputPath, new TurtleParser(TurtleSyntax.W3C, false));
        }

        var tripleStore = new TripleStore();
        tripleStore.LoadFromFile(dataInputPath);
        if (t.GraphData != null)
        {
            var graphDataInputPath = t.Manifest.ResolveResourcePath(t.GraphData);
            var g = new Graph(new UriNode(t.GraphData));
            g.LoadFromFile(graphDataInputPath);
            tripleStore.Add(g);
        }
        var queryParser = new SparqlQueryParser(queryInputPath.Contains("data-r2") ? SparqlQuerySyntax.Sparql_1_0 : SparqlQuerySyntax.Sparql_1_1);
        SparqlQuery query = queryParser.ParseFromFile(queryInputPath);
        expectGraphResult = query.QueryType is SparqlQueryType.Construct or SparqlQueryType.Describe;
        var results = await ProcessQueryAsync(tripleStore, query);
        if (!expectGraphResult && resultGraph != null)
        {
            resultSet = new SparqlResultSet();
            var reader = new SparqlRdfParser();
            reader.Load(resultSet, resultGraph);
        }

        if (expectGraphResult)
        {
            results.Should().BeAssignableTo<IGraph>();
            var graphDiff = new GraphDiff();
            GraphDiffReport diffReport = graphDiff.Difference(resultGraph, results as IGraph);
            TestTools.ShowDifferences(diffReport, "Expected Results", "Actual Results", output);
            diffReport.AreEqual.Should().BeTrue();
        }
        else
        {
            results.Should().BeAssignableTo<SparqlResultSet>().Which.Should().BeEquivalentTo(resultSet);
        }
    }

    //[ManifestTestRunner("http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#QueryEvaluationTest")]
    //public void UpdateEvaluationTest(ManifestTestData t)
    //{

    //}

    [ManifestTestRunner("http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#PositiveSyntaxTest11")]
    private void PositiveSyntaxTest(ManifestTestData t)
    {
        var queryInputPath = t.Manifest.ResolveResourcePath(t.Action);
        var queryParser = new SparqlQueryParser(SparqlQuerySyntax.Sparql_1_1) { DefaultBaseUri = t.Action };
        queryParser.ParseFromFile(queryInputPath);
    }

    [ManifestTestRunner("http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#NegativeSyntaxTest11")]
    private void NegativeSyntaxTest(ManifestTestData t)
    {
        var queryInputPath = t.Manifest.ResolveResourcePath(t.Action);
        var queryParser = new SparqlQueryParser(SparqlQuerySyntax.Sparql_1_1) { DefaultBaseUri = t.Action };
        Assert.ThrowsAny<RdfParseException>(() => { queryParser.ParseFromFile(queryInputPath); });
    }
}