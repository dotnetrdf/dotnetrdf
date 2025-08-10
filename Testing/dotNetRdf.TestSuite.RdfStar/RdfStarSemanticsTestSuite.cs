using dotNetRdf.TestSupport;
using System;
using System.IO;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing.Formatting;
using Xunit;
using Xunit.Sdk;

namespace VDS.RDF.TestSuite.RdfStar;

public class RdfStarSemanticsTestSuite : RdfTestSuite
{
    private readonly ITestOutputHelper _output;

    public static ManifestTestDataProvider RdfStarSemantics = new ManifestTestDataProvider(new Uri("http://example/base/manifest.ttl"),
        Path.Combine("resources", "tests", "semantics", "manifest.ttl"));

    public RdfStarSemanticsTestSuite(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory]
    [MemberData(nameof(RdfStarSemantics))]
    public void RunTest(ManifestTestData t)
    {
        _output.WriteLine($"{t.Id}: {t.Name} is a {t.Type} with regime {t.EntailmentRegime}");
        InvokeTestRunner(t);
    }

    [ManifestTestRunner("http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#PositiveEntailmentTest")]
    internal void PositiveEntailmentTest(ManifestTestData t)
    {
        RunEntailmentTest(t, true);
    }

    [ManifestTestRunner("http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#NegativeEntailmentTest")]
    internal void NegativeEntailmentTest(ManifestTestData t)
    {
        RunEntailmentTest(t, false);
    }

    private void RunEntailmentTest(ManifestTestData t, bool expectEntailment)
    {
        if (!t.EntailmentRegime.Equals("simple"))
        {
            throw SkipException.ForSkip($"Entailment regime {t.EntailmentRegime} is not supported.");
        }
        var inputPath = t.Manifest.ResolveResourcePath(t.Action);
        var resultPath = t.Manifest.ResolveResourcePath(t.Result);
        IGraph inputGraph = new Graph();
        inputGraph.LoadFromFile(inputPath, new TurtleParser(TurtleSyntax.Rdf11Star, false));
        IGraph resultGraph = new Graph();
        resultGraph.LoadFromFile(resultPath, new TurtleParser(TurtleSyntax.Rdf11Star, false));

        SparqlQuery query = MakeEntailmentQuery(resultGraph);
        var queryResult = inputGraph.ExecuteQuery(query) as SparqlResultSet;
        Assert.NotNull(queryResult);
        if (expectEntailment)
        {
            Assert.True(queryResult.Result);
        }
        else
        {
            Assert.False(queryResult.Result);
        }
    }

    [Fact]
    public void RunSingle()
    {
        ManifestTestData testData =
            RdfStarSemantics.GetTestData(
                new Uri("https://w3c.github.io/rdf-star/tests/semantics#constrained-bnodes-in-quoted-object"));
        InvokeTestRunner(testData);
    }

    private SparqlQuery MakeEntailmentQuery(IGraph g)
    {
        var builder = new StringBuilder();
        var formatter = new SparqlFormatter();
        builder.Append("ASK WHERE {\n");
        foreach (Triple t in g.Triples)
        {
            builder.Append(formatter.Format(MapNode(t.Subject)));
            builder.Append(" ");
            builder.Append(formatter.Format(MapNode(t.Predicate)));
            builder.Append(" ");
            builder.Append(formatter.Format(t.Object));
            builder.Append(". \n");
        }

        builder.Append("}");
        var queryParser = new SparqlQueryParser(SparqlQuerySyntax.Sparql_Star_1_1);
        return queryParser.ParseFromString(builder.ToString());
    }

    private static INode MapNode(INode n)
    {
        switch (n)
        {
            case ITripleNode tn:
                return new TripleNode(new Triple(MapNode(tn.Triple.Subject), MapNode(tn.Triple.Predicate),
                    MapNode(tn.Triple.Object)));
            case IBlankNode bn:
                return new VariableNode("_" + bn.InternalID);
            default:
                return n;
        }
    }
}
