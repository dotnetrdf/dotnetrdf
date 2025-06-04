using dotNetRdf.TestSupport;
using FluentAssertions;
using System;
using System.IO;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using Xunit;

namespace VDS.RDF.TestSuite.W3C;

public class LeviathanEngineTestSuites(ITestOutputHelper output) : RdfTestSuite
{
    public static ManifestTestDataProvider Sparql11QueryEvalTests = new ManifestTestDataProvider(
        new Uri("http://example/base/manifest.ttl"),
        Path.Combine("resources", "rdf-tests", "sparql", "sparql11", "manifest-sparql11-query.ttl"));

    public static ManifestTestDataProvider Sparql10QueryEvalTests = new(
        new Uri("http://example/base/manifest.ttl"),
        Path.Combine("resources", "rdf-tests", "sparql", "sparql10", "manifest-evaluation.ttl"));

    [Trait("Category", "explicit")]
    [Theory]
    [MemberData(nameof(Sparql10QueryEvalTests))]
    public void RunSparql10EvaluationTests(ManifestTestData t)
    {
        output.WriteLine($"{t.Id}: {t.Name} is a {t.Type}");
        InvokeTestRunner(t);
    }

    [Trait("Category", "explicit")]
    [Theory]
    [MemberData(nameof(Sparql11QueryEvalTests))]
    public void RunSparql11EvaluationTests(ManifestTestData t)
    {
        output.WriteLine($"{t.Id}: {t.Name} is a {t.Type}");
        InvokeTestRunner(t);
    }

    [ManifestTestRunner("http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#QueryEvaluationTest")]
    internal void QueryEvaluationTest(ManifestTestData t)
    {
        var queryInputPath = t.Manifest.ResolveResourcePath(t.Query);
        var dataInputPath = t.Manifest.ResolveResourcePath(t.Data);
        var resultInputPath = t.Manifest.ResolveResourcePath(t.Result);
        var resultSet = new SparqlResultSet();
        var resultGraph = new Graph();
        var expectGraphResult = false;

        if (resultInputPath.EndsWith(".srj"))
        {
            var resultParser = new SparqlJsonParser();
            resultParser.Load(resultSet, resultInputPath);
        }
        else if (resultInputPath.EndsWith(".srx"))
        {
            var resultParser = new SparqlXmlParser();
            resultParser.Load(resultSet, resultInputPath);
        }
        else if (resultInputPath.EndsWith(".ttl"))
        {
            expectGraphResult = true;
            resultGraph.LoadFromFile(resultInputPath, new TurtleParser(TurtleSyntax.W3C, false));
        }

        var tripleStore = new TripleStore();
        tripleStore.LoadFromFile(dataInputPath);

        var queryParser = new SparqlQueryParser(SparqlQuerySyntax.Sparql_1_1);
        SparqlQuery query = queryParser.ParseFromFile(queryInputPath);

        var queryEngine = new LeviathanQueryProcessor(tripleStore);
        var results = queryEngine.ProcessQuery(query);

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
    internal void PositiveSyntaxTest(ManifestTestData t)
    {
        var queryInputPath = t.Manifest.ResolveResourcePath(t.Action);
        var queryParser = new SparqlQueryParser(SparqlQuerySyntax.Sparql_1_1) { DefaultBaseUri = t.Action };
        queryParser.ParseFromFile(queryInputPath);
    }

    [ManifestTestRunner("http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#NegativeSyntaxTest11")]
    internal void NegativeSyntaxTest(ManifestTestData t)
    {
        var queryInputPath = t.Manifest.ResolveResourcePath(t.Action);
        var queryParser = new SparqlQueryParser(SparqlQuerySyntax.Sparql_1_1) { DefaultBaseUri = t.Action };
        Assert.ThrowsAny<RdfParseException>(() => { queryParser.ParseFromFile(queryInputPath); });
    }
}
