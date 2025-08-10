using dotNetRdf.TestSupport;
using System;
using System.IO;
using VDS.RDF.Parsing;
using Xunit;

namespace VDS.RDF.TestSuite.RdfStar;

public class SparqlStarSyntaxTestSuite  : RdfTestSuite
{
    public static ManifestTestDataProvider SparqlStarSyntaxTests = new ManifestTestDataProvider(
        new Uri("http://example/base/manifest.ttl"),
        Path.Combine("resources", "tests", "sparql", "syntax", "manifest.ttl"));

    private readonly ITestOutputHelper _output;

    public SparqlStarSyntaxTestSuite(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory]
    [MemberData(nameof(SparqlStarSyntaxTests))]
    public void RunTest(ManifestTestData t)
    {
        _output.WriteLine($"{t.Id}: {t.Name} is a {t.Type}");
        InvokeTestRunner(t);
    }

    [ManifestTestRunner("http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#PositiveSyntaxTest11")]
    internal void PositiveSyntaxTest(ManifestTestData t)
    {
        var inputPath = t.Manifest.ResolveResourcePath(t.Action);
        LoadQuery(inputPath);
    }

    [ManifestTestRunner("http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#NegativeSyntaxTest11")]
    internal void NegativeSyntaxTest(ManifestTestData t)
    {
        var inputPath = t.Manifest.ResolveResourcePath(t.Action);
        Assert.ThrowsAny<RdfException>(() =>LoadQuery(inputPath));
    }

    [ManifestTestRunner("http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#PositiveUpdateSyntaxTest11")]
    internal void PositiveUpdateSyntaxTest(ManifestTestData t)
    {
        var inputPath = t.Manifest.ResolveResourcePath(t.Action);
        LoadUpdate(inputPath);
    }

    [ManifestTestRunner("http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#NegativeUpdateSyntaxTest11")]
    internal void NegativeUpdateSyntaxTest(ManifestTestData t)
    {
        var inputPath = t.Manifest.ResolveResourcePath(t.Action);
        Assert.ThrowsAny<RdfException>(() => LoadUpdate(inputPath));
    }

    private void LoadQuery(string inputPath)
    {
        _output.WriteLine($"Load SPARQL-Star QUERY from {inputPath}");
            var parser = new SparqlQueryParser(SparqlQuerySyntax.Sparql_Star_1_1);
            parser.ParseFromFile(inputPath);
    }

    private void LoadUpdate(string inputPath)
    {
        _output.WriteLine($"Load SPARQL-Star UPDATE from {inputPath}");
        var parser = new SparqlUpdateParser(SparqlQuerySyntax.Sparql_Star_1_1);
        parser.ParseFromFile(inputPath);
    }

    [Fact]
    public void RunSingle()
    {
        ManifestTestData testData =
            SparqlStarSyntaxTests.GetTestData(
                new Uri("https://w3c.github.io/rdf-star/tests/sparql/syntax#sparql-star-update-1"));
        InvokeTestRunner(testData);
    }
}
