using System;
using dotNetRdf.TestSupport;
using System.Collections.Generic;
using System.IO;
using Xunit;
using Xunit.Sdk;

namespace VDS.RDF.Parsing.Suites;

public class Trig11 : RdfTestSuite
{
    private readonly ITestOutputHelper _output;

    public Trig11(ITestOutputHelper output)
    {
        _output = output;
    }

    public static IEnumerable<object[]> GetTestData()
    {
        return new ManifestTestDataProvider(
            new Uri("http://www.w3.org/2013/TriGTests/"),
            Path.Combine("resources", "trig11", "manifest.ttl"));
    }

    [Theory]
    [MemberData(nameof(GetTestData))]
    public void RunTest(ManifestTestData t)
    {
        _output.WriteLine($"{t.Id}: {t.Name} is a {t.Type}");
        InvokeTestRunner(t);
    }

    [ManifestTestRunner("http://www.w3.org/ns/rdftest#TestTrigPositiveSyntax")]
    internal void PositiveSyntaxTest(ManifestTestData t)
    {
        if (t.Id == "http://www.w3.org/2013/TriGTests/#trig-syntax-minimal-whitespace-01")
        {
            throw SkipException.ForSkip(
                "Turtle tokenizer does not currently handle backtracking when a DOT was meant to terminate a triple.");
        }
        _output.WriteLine($"Load from {t.Manifest.ResolveResourcePath(t.Action)}");
        var parser = new TriGParser(TriGSyntax.Rdf11);
        var store = new TripleStore();
        parser.Load(store, t.Manifest.ResolveResourcePath(t.Action));
    }

    [ManifestTestRunner("http://www.w3.org/ns/rdftest#TestTrigNegativeSyntax")]
    internal void NegativeSyntaxTest(ManifestTestData t)
    {
        if (t.Id == "http://www.w3.org/2013/TriGTests/#trig-graph-bad-02")
        {
            throw SkipException.ForSkip(
                "Skipping dubious bad syntax test. Test asserts that a graph block must be followed by a DOT, but the specification does not require this.");
        }

        var parser = new TriGParser(TriGSyntax.Rdf11);
        var store = new TripleStore();
        Assert.ThrowsAny<RdfException>(() => parser.Load(store, t.Manifest.ResolveResourcePath(t.Action)));
    }

    [ManifestTestRunner("http://www.w3.org/ns/rdftest#TestTrigEval")]
    internal void EvaluationTest(ManifestTestData t)
    {
        var trigParser = new TriGParser(TriGSyntax.Rdf11Star);
        var actual = new TripleStore();
        using (var reader = new StreamReader(t.Manifest.ResolveResourcePath(t.Action)))
        {
            trigParser.Load(actual, reader, t.Action);
        }

        var nqParser = new NQuadsParser(NQuadsSyntax.Rdf11Star);
        var expected = new TripleStore();
        nqParser.Load(expected, t.Manifest.ResolveResourcePath(t.Result));
        TestTools.AssertEqual(expected, actual, _output);
    }

    [ManifestTestRunner("http://www.w3.org/ns/rdftest#TestTrigNegativeEval")]
    internal void NegativeEvaluationTest(ManifestTestData t)
    {
        var trigParser = new TriGParser(TriGSyntax.Rdf11Star) { ValidateIris = true };
        var actual = new TripleStore();
        using var reader = new StreamReader(t.Manifest.ResolveResourcePath(t.Action));
        Assert.ThrowsAny<RdfException> (()=>trigParser.Load(actual, reader, t.Action));
    }


    [Fact]
    public void RunSingle()
    {
        InvokeTestRunner(new ManifestTestDataProvider(
            new Uri("http://www.w3.org/2013/TriGTests/"),
            Path.Combine("resources", "trig11", "manifest.ttl"))
            .GetTestData(new Uri("http://www.w3.org/2013/TriGTests/#trig-eval-bad-01")));
    }
}
