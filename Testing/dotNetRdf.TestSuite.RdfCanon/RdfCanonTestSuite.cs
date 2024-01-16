using dotNetRdf.TestSupport;
using FluentAssertions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF.Writing.Formatting;
using Xunit;
using Xunit.Abstractions;

namespace VDS.RDF.TestSuite.Rdf11;

public class RdfCanonTestSuite(ITestOutputHelper output) : RdfTestSuite
{
    public static ManifestTestDataProvider Rdfc10EvalTests = new(new Uri("http://example/base/manifest.ttl"),
        Path.Combine("resources", "manifest.ttl"));

    [Theory]
    [MemberData(nameof(Rdfc10EvalTests))]
    public void RunCanonEvalTests(ManifestTestData t)
    {
        output.WriteLine($"{t.Id}: {t.Name} is a {t.Type}");
        InvokeTestRunner(t);
    }

    [ManifestTestRunner("https://w3c.github.io/rdf-canon/tests/vocab#RDFC10EvalTest")]
    internal void EvalTest(ManifestTestData t)
    {
        var dataInputPath = t.Manifest.ResolveResourcePath(t.Action);
        var resultInputPath = t.Manifest.ResolveResourcePath(t.Result);
        var expectedResult = File.ReadAllText(resultInputPath);

        var inputStore = new TripleStore();
        inputStore.LoadFromFile(dataInputPath);

        TripleStore resultStore = new RdfCanonicalizer(t.HashAlgorithm ?? "SHA256").Canonicalize(inputStore.Graphs);

        var formatter = new NQuads11Formatter();
        var sb = new StringBuilder();
        resultStore.Graphs
            .SelectMany(graph => graph.Triples.Select(triple => formatter.Format(triple, graph.Name)))
            .OrderBy(p => p, StringComparer.Ordinal).ToList().ForEach(s => sb.AppendLine(s));
        var result = sb.ToString();

        result.Should().BeEquivalentTo(expectedResult);
    }

    [ManifestTestRunner("https://w3c.github.io/rdf-canon/tests/vocab#RDFC10NegativeEvalTest")]
    internal void NegativeEvalTest(ManifestTestData t)
    {
        string result = null;
        var dataInputPath = t.Manifest.ResolveResourcePath(t.Action);
        try
        {
            var inputStore = new TripleStore();
            inputStore.LoadFromFile(dataInputPath);

            TripleStore resultStore =
                new RdfCanonicalizer(t.HashAlgorithm ?? "SHA256").Canonicalize(inputStore.Graphs);

            var formatter = new NQuads11Formatter();
            var sb = new StringBuilder();
            resultStore.Graphs
                .SelectMany(graph => graph.Triples.Select(triple => formatter.Format(triple, graph.Name)))
                .OrderBy(p => p, StringComparer.Ordinal).ToList().ForEach(s => sb.AppendLine(s));
            result = sb.ToString();
        }
        catch
        {
            // ignored
        }

        result.Should().BeNull();
    }

    [ManifestTestRunner("https://w3c.github.io/rdf-canon/tests/vocab#RDFC10MapTest")]
    internal void MapTest(ManifestTestData t)
    {
        var dataInputPath = t.Manifest.ResolveResourcePath(t.Action);
        var resultInputPath = t.Manifest.ResolveResourcePath(t.Result);
        var expectedResultJson = File.ReadAllText(resultInputPath);
        var expectedResult = JsonConvert.DeserializeObject<Dictionary<string, string>>(expectedResultJson);

        var inputStore = new TripleStore();
        inputStore.LoadFromFile(dataInputPath);

        var canonicalizer = new RdfCanonicalizer(t.HashAlgorithm ?? "SHA256");
        canonicalizer.Canonicalize(inputStore.Graphs);
        var dict = canonicalizer.GetMappingDictionary().Select(p =>
            new KeyValuePair<string, string>(p.Key, p.Value.StartsWith("_:") ? p.Value.Substring(2) : p.Value));
        dict.Should().BeEquivalentTo(expectedResult);
    }
}