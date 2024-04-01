using dotNetRdf.TestSupport;
using FluentAssertions;
using System;
using System.IO;
using System.Linq;
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
        var dataInputPath = t.Data != null ? t.Manifest.ResolveResourcePath(t.Data) : null;
        var resultInputPath = t.Manifest.ResolveResourcePath(t.Result);
        SparqlResultSet expectedResultSet = null;
        Graph expectedResultGraph = null;
        var expectGraphResult = false;

        if (resultInputPath.EndsWith(".srj"))
        {
            expectedResultSet = new SparqlResultSet();
            var resultParser = new SparqlJsonParser();
            resultParser.Load(expectedResultSet, resultInputPath);
        }
        else if (resultInputPath.EndsWith(".srx"))
        {
            expectedResultSet = new SparqlResultSet();
            var resultParser = new SparqlXmlParser();
            resultParser.Load(expectedResultSet, resultInputPath);
        }
        else if (resultInputPath.EndsWith(".ttl"))
        {
            expectedResultGraph = new Graph() { BaseUri = t.Manifest.BaseUri };
            expectedResultGraph.LoadFromFile(resultInputPath, new TurtleParser(TurtleSyntax.W3C, false));
        } 
        else if (resultInputPath.EndsWith(".rdf"))
        {
            expectedResultGraph = new Graph() { BaseUri = t.Manifest.BaseUri };
            expectedResultGraph.LoadFromFile(resultInputPath, new RdfXmlParser());
        }

        var tripleStore = new TripleStore(new ResolverDemandGraphCollection(uri =>
        {
            var filePath = t.Manifest.ResolveResourcePath(uri);
            var g = new Graph(new UriNode(uri));
            g.LoadFromFile(filePath);
            return g;
        }));
        if (dataInputPath != null)
        {
            var g = new Graph{BaseUri = t.Data};
            g.LoadFromFile(dataInputPath);
            tripleStore.Add(g);
        }

        foreach (Uri graphUri in t.GraphData)
        {
            var g = new Graph(new UriNode(graphUri)) { BaseUri = graphUri };
            g.LoadFromFile(t.Manifest.ResolveResourcePath(graphUri));
            tripleStore.Add(g);
        }
        var queryParser = new SparqlQueryParser(queryInputPath.Contains("data-r2") ? SparqlQuerySyntax.Sparql_1_0 : SparqlQuerySyntax.Sparql_1_1)
        {
            DefaultBaseUri = t.Manifest.BaseUri
        };
        SparqlQuery query = queryParser.ParseFromFile(queryInputPath);
        expectGraphResult = query.QueryType is SparqlQueryType.Construct or SparqlQueryType.Describe;
        var results = await ProcessQueryAsync(tripleStore, query);
        if (!expectGraphResult && expectedResultGraph != null)
        {
            expectedResultSet = new SparqlResultSet();
            var reader = new SparqlRdfParser { PadUnboundVariables = false };
            reader.Load(expectedResultSet, expectedResultGraph);
        }

        if (expectGraphResult)
        {
            results.Should().BeAssignableTo<IGraph>();
            var graphDiff = new GraphDiff();
            GraphDiffReport diffReport = graphDiff.Difference(expectedResultGraph, results as IGraph);
            TestTools.ShowDifferences(diffReport, "Expected Results", "Actual Results", output);
            diffReport.AreEqual.Should().BeTrue();
        }
        else
        {
            if (t.LaxCardinality)
            {
                // With lax cardinality expect only that each result in the actual result set should be in expectedResultSet
                results.Should().BeAssignableTo<SparqlResultSet>().Which.Should().OnlyContain(actualResult =>
                    expectedResultSet.Any(expectedResult => expectedResult.Equals(actualResult)));
            }
            else
            {
                results.Should().BeAssignableTo<SparqlResultSet>().Which.Should().BeEquivalentTo(expectedResultSet);
            }
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