using dotNetRdf.TestSupport;
using System;
using System.IO;
using VDS.RDF.Parsing;
using Xunit;

namespace VDS.RDF.TestSuite.RdfStar;

public class TrigStarTestSuite : RdfTestSuite
{
    public static ManifestTestDataProvider TrigSyntaxTests = new ManifestTestDataProvider(
        new Uri("http://example/base/manifest.ttl"),
        Path.Combine("resources", "tests", "trig", "syntax", "manifest.ttl"));

    public static ManifestTestDataProvider TrigEvalTests = new ManifestTestDataProvider(
        new Uri("http://example/base/maifest.ttl"),
        Path.Combine("resources", "tests", "trig", "eval", "manifest.ttl"));

    private readonly ITestOutputHelper _output;

    public TrigStarTestSuite(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory]
    [MemberData(nameof(TrigSyntaxTests))]
    public void SyntaxTests(ManifestTestData t)
    {
        _output.WriteLine($"{t.Id}: {t.Name} is a {t.Type}");
        InvokeTestRunner(t);
    }

    [Theory]
    [MemberData(nameof(TrigEvalTests))]
    public void EvaluationTests(ManifestTestData t)
    {
        _output.WriteLine($"{t.Id}: {t.Name} is a {t.Type}");
        InvokeTestRunner(t);
    }

    [ManifestTestRunner("http://www.w3.org/ns/rdftest#TestTrigPositiveSyntax")]
    internal void PositiveSyntaxTest(ManifestTestData t)
    {
        var parser = new TriGParser(TriGSyntax.Rdf11Star);
        var store = new TripleStore();
        parser.Load(store, t.Manifest.ResolveResourcePath(t.Action));
    }

    [ManifestTestRunner("http://www.w3.org/ns/rdftest#TestTrigNegativeSyntax")]
    internal void NegativeSyntaxTest(ManifestTestData t)
    {
        var parser = new TriGParser(TriGSyntax.Rdf11Star);
        var store = new TripleStore();
        Assert.ThrowsAny<RdfException>(() => parser.Load(store, t.Manifest.ResolveResourcePath(t.Action)));
    }

    [ManifestTestRunner("http://www.w3.org/ns/rdftest#TestTrigEval")]
    internal void EvaluationTest(ManifestTestData t)
    {
        var trigParser = new TriGParser(TriGSyntax.Rdf11Star);
        var actual = new TripleStore();
        trigParser.Load(actual, t.Manifest.ResolveResourcePath(t.Action));
        var nqParser = new NQuadsParser(NQuadsSyntax.Rdf11Star);
        var expected = new TripleStore();
        nqParser.Load(expected, t.Manifest.ResolveResourcePath(t.Result));
        TestTools.AssertEqual(expected, actual, _output);
    }

    /* Helper method for debugging single tests
    [Fact]
    public void RunSingle()
    {
        InvokeTestRunner(new ManifestTestDataProvider(
            new Uri("http://example/base/manifest.ttl"),
            Path.Combine("resources", "tests", "trig", "syntax", "manifest.ttl"))
            .GetTestData(new Uri("https://w3c.github.io/rdf-star/tests/trig/syntax#trig-star-ann-2")));
    }
    */
}
