using dotNetRdf.TestSupport;
using FluentAssertions;
using System;
using System.IO;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Update;
using Xunit;

namespace VDS.RDF.TestSuite.RdfStar;

public class SparqlStarEvaluationTestSuite : RdfTestSuite
{
    public static ManifestTestDataProvider SparqlStarEvalTests = new ManifestTestDataProvider(
        new Uri("http://example/base/manifest.ttl"),
        Path.Combine("resources", "tests", "sparql", "eval", "manifest.ttl"));

    private readonly ITestOutputHelper _output;

    public SparqlStarEvaluationTestSuite(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory]
    [MemberData(nameof(SparqlStarEvalTests))]
    public void RunTest(ManifestTestData t)
    {
        _output.WriteLine($"{t.Id}: {t.Name} is a {t.Type}");
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
            resultGraph.LoadFromFile(resultInputPath, new TurtleParser(TurtleSyntax.Rdf11Star, false));
        }

        var tripleStore = new TripleStore();
        if (dataInputPath.EndsWith(".trig"))
        {
            tripleStore.LoadFromFile(dataInputPath, new TriGParser(TriGSyntax.Rdf11Star));
        }
        else
        {
            var g = new Graph();
            g.LoadFromFile(dataInputPath, new TurtleParser(TurtleSyntax.Rdf11Star, false));
            tripleStore.Add(g);
        }

        var queryParser = new SparqlQueryParser(SparqlQuerySyntax.Sparql_Star_1_1);
        SparqlQuery query = queryParser.ParseFromFile(queryInputPath);

        var queryEngine = new LeviathanQueryProcessor(tripleStore, options =>
        {
            options.UsePLinqEvaluation = false;
#if DEBUG
            options.QueryExecutionTimeout = -1;
#endif
        });
        var results = queryEngine.ProcessQuery(query);

        if (expectGraphResult)
        {
            results.Should().BeAssignableTo<IGraph>();
            var graphDiff = new GraphDiff();
            GraphDiffReport diffReport = graphDiff.Difference(resultGraph, results as IGraph);
            TestTools.ShowDifferences(diffReport, "Expected Results", "Actual Results", _output);
            diffReport.AreEqual.Should().BeTrue();
        }
        else
        {
            results.Should().BeAssignableTo<SparqlResultSet>().Which.Should().BeEquivalentTo(resultSet);
        }
    }

    [ManifestTestRunner("http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#UpdateEvaluationTest")]
    internal void UpdateEvaluationTest(ManifestTestData t)
    {
        var updateRequestPath = t.Manifest.ResolveResourcePath(t.UpdateRequest);
        var dataInputPath = t.Manifest.ResolveResourcePath(t.UpdateData);
        var resultInputPath = t.Manifest.ResolveResourcePath(t.UpdateResult);

        var resultStore = new TripleStore();
        resultStore.LoadFromFile(resultInputPath, new TriGParser(TriGSyntax.Rdf11Star));

        var tripleStore = new TripleStore();
        if (dataInputPath.EndsWith(".nq"))
        {
            tripleStore.LoadFromFile(dataInputPath, new NQuadsParser(NQuadsSyntax.Rdf11Star));
        } else if (dataInputPath.EndsWith(".trig"))
        {
            tripleStore.LoadFromFile(dataInputPath, new TriGParser(TriGSyntax.Rdf11Star));
        }
        else
        {
            Assert.Fail($"Unexpected data input path: {dataInputPath}");
        }

        var updateEngine = new LeviathanUpdateProcessor(tripleStore, options =>
        {
            options.UsePLinqEvaluation = false;
#if DEBUG
            options.QueryExecutionTimeout = -1;
#endif
        });
        var updateParser = new SparqlUpdateParser(SparqlQuerySyntax.Sparql_Star_1_1);
        SparqlUpdateCommandSet updateCommandSet = updateParser.ParseFromFile(updateRequestPath);
        updateEngine.ProcessCommandSet(updateCommandSet);
        TestTools.AssertEqual(resultStore, tripleStore, _output);
    }

    [Fact]
    public void RunSingle()
    {
        ManifestTestData testData =
            SparqlStarEvalTests.GetTestData(
                new Uri("https://w3c.github.io/rdf-star/tests/sparql/eval#sparql-star-expr-1"));
        InvokeTestRunner(testData);
    }
}
