using dotNetRdf.TestSupport;
using System;
using System.IO;
using VDS.RDF.Parsing;
using Xunit;

namespace VDS.RDF.TestSuite.RdfStar;

public class NTriplesStarTestSuite : RdfTestSuite
{
    public static ManifestTestDataProvider NTriplesTests = new ManifestTestDataProvider(
        new Uri("http://example/base/manifest.ttl"),
        Path.Combine("resources", "tests", "nt", "syntax", "manifest.ttl"));

    private readonly ITestOutputHelper _output;

    public NTriplesStarTestSuite(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory]
    [MemberData(nameof(NTriplesTests))]
    public void RunTest(ManifestTestData t)
    {
        _output.WriteLine($"{t.Id}: {t.Name} is a {t.Type}");
        InvokeTestRunner(t);
    }

    [ManifestTestRunner("http://www.w3.org/ns/rdftest#TestNTriplesPositiveSyntax")]
    internal void PositiveSyntaxTest(ManifestTestData t)
    {
        _output.WriteLine($"Load from {t.Manifest.ResolveResourcePath(t.Action)}");
        var parser = new NTriplesParser(NTriplesSyntax.Rdf11Star);
        var g = new Graph();
        parser.Load(g, t.Manifest.ResolveResourcePath(t.Action));
    }

    [ManifestTestRunner("http://www.w3.org/ns/rdftest#TestNTriplesNegativeSyntax")]
    internal void NegativeSyntaxTest(ManifestTestData t)
    {
        var parser = new NTriplesParser(NTriplesSyntax.Rdf11Star);
        var g = new Graph();
        Assert.ThrowsAny<RdfException>(() => parser.Load(g, t.Manifest.ResolveResourcePath(t.Action)));
    }
}
