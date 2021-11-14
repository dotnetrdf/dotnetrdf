using System;
using dotNetRdf.TestSupport;
using System.Collections.Generic;
using System.IO;
using Xunit;
using Xunit.Abstractions;


namespace VDS.RDF.Parsing.Suites
{
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
                new Uri("https://www.w3.org/2013/TrigTests/"),
                Path.Combine("resources", "trig11", "manifest.ttl"));
        }

        [SkippableTheory]
        [MemberData(nameof(GetTestData))]
        public void RunTest(ManifestTestData t)
        {
            _output.WriteLine($"{t.Id}: {t.Name} is a {t.Type}");
            InvokeTestRunner(t);
        }

        [ManifestTestRunner("http://www.w3.org/ns/rdftest#TestTrigPositiveSyntax")]
        public void PositiveSyntaxTest(ManifestTestData t)
        {
            if (t.Id == "https://www.w3.org/2013/TrigTests/#trig-syntax-minimal-whitespace-01")
            {
                throw new SkipException(
                    "Turtle tokenizer does not currently handle backtracking when a DOT was meant to terminate a triple.");
            }
            _output.WriteLine($"Load from {t.Manifest.ResolveResourcePath(t.Action)}");
            var parser = new TriGParser(TriGSyntax.Rdf11);
            var store = new TripleStore();
            parser.Load(store, t.Manifest.ResolveResourcePath(t.Action));
        }

        [ManifestTestRunner("http://www.w3.org/ns/rdftest#TestTrigNegativeSyntax")]
        public void NegativeSyntaxTest(ManifestTestData t)
        {
            if (t.Id == "https://www.w3.org/2013/TrigTests/#trig-graph-bad-02")
            {
                throw new SkipException(
                    "Skipping dubious bad syntax test. Test asserts that a graph block must be followed by a DOT, but the specification does not require this.");
            }

            var parser = new TriGParser(TriGSyntax.Rdf11);
            var store = new TripleStore();
            Assert.ThrowsAny<RdfException>(() => parser.Load(store, t.Manifest.ResolveResourcePath(t.Action)));
        }

        [ManifestTestRunner("http://www.w3.org/ns/rdftest#TestTrigEval")]
        public void EvaluationTest(ManifestTestData t)
        {
            throw new SkipException("TriG Evaluation Tests not yet implemented");
        }

        [ManifestTestRunner("http://www.w3.org/ns/rdftest#TestTrigNegativeEval")]
        public void NegativeEvaluationTest(ManifestTestData t)
        {
            throw new SkipException("TriG Evaluation Tests not yet implemented");
        }

        [SkippableFact]
        public void RunSingle()
        {
            InvokeTestRunner(new ManifestTestDataProvider(
                new Uri("https://www.w3.org/2013/TrigTests/"),
                Path.Combine("resources", "trig11", "manifest.ttl")).GetTestData(new Uri("https://www.w3.org/2013/TrigTests/#trig-syntax-minimal-whitespace-01")));
        }
    }
}
