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
        var resultInputPath = t.Manifest.ResolveResourcePath(t.Result);
        string expectedResult;
        using (var sr = new StreamReader(new FileStream(resultInputPath, FileMode.Open), Encoding.UTF8))
        {
            expectedResult = sr.ReadToEnd();
        }
        RdfCanonicalizer.CanonicalizedRdfDataset result = RunCanonicalize(t);

        result.SerializedNQuads.Should().BeEquivalentTo(expectedResult);
    }

    [ManifestTestRunner("https://w3c.github.io/rdf-canon/tests/vocab#RDFC10NegativeEvalTest")]
    internal void NegativeEvalTest(ManifestTestData t)
    {
        Assert.ThrowsAny<Exception>(() => RunCanonicalize(t));
    }

    [ManifestTestRunner("https://w3c.github.io/rdf-canon/tests/vocab#RDFC10MapTest")]
    internal void MapTest(ManifestTestData t)
    {
        var resultInputPath = t.Manifest.ResolveResourcePath(t.Result);
        var expectedResultJson = File.ReadAllText(resultInputPath);
        var expectedResult = JsonConvert.DeserializeObject<Dictionary<string, string>>(expectedResultJson);
        
        RdfCanonicalizer.CanonicalizedRdfDataset result = RunCanonicalize(t);

        var dict = result.IssuedIdentifiersMap.Select(p =>
            new KeyValuePair<string, string>(p.Key, p.Value.StartsWith("_:") ? p.Value.Substring(2) : p.Value));
        dict.Should().BeEquivalentTo(expectedResult);
    }

    private static RdfCanonicalizer.CanonicalizedRdfDataset RunCanonicalize(ManifestTestData t)
    {
        var inputStore = new TripleStore();
        inputStore.LoadFromFile(t.Manifest.ResolveResourcePath(t.Action));
        return new RdfCanonicalizer(t.HashAlgorithm ?? "SHA256").Canonicalize(inputStore);
    }
}