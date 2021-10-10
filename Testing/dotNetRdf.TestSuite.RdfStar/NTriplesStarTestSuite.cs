using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using VDS.RDF.Parsing;
using Xunit;
using Xunit.Abstractions;

namespace VDS.RDF.TestSuite.RdfStar
{

    public class NTriplesStarTestSuite
    {
        public static ManifestTestData NTriplesTests = new ManifestTestData(
            new Uri("http://example/base/manifest.ttl"),
            Path.Combine("resources", "tests", "nt", "syntax", "manifest.ttl"));
        private readonly ITestOutputHelper _output;

        private Dictionary<string, MethodInfo> _runners;
        public NTriplesStarTestSuite(ITestOutputHelper output)
        {
            _output = output;
            _runners = new Dictionary<string, MethodInfo>();
            foreach (var m in typeof(NTriplesStarTestSuite).GetMethods())
            {
                if (m.GetCustomAttribute(typeof(ManifestTestRunnerAttribute)) is ManifestTestRunnerAttribute runnerAttr)
                {
                    _runners[runnerAttr.TestType] = m;
                }
            }
        }

        [Theory]
        [MemberData(nameof(NTriplesTests))]
        public void RunTest(ManifestTest t)
        {
            _output.WriteLine($"{t.Id}: {t.Name} is a {t.Type}");
            _runners.Should().ContainKey(t.Type.AbsoluteUri,
                because: $"test class should provide a runner method for tests of type {t.Type.AbsoluteUri}");
            MethodInfo runner = _runners[t.Type.AbsoluteUri];
            runner.Invoke(this, new[] { t });
        }

        [ManifestTestRunner("http://www.w3.org/ns/rdftest#TestNTriplesPositiveSyntax")]
        public void PositiveSyntaxTest(ManifestTest t)
        {
            _output.WriteLine($"Load from {t.Manifest.ResolveResourcePath(t.Action)}");
            var parser = new NTriplesParser(NTriplesSyntax.Rdf11Star);
            var g = new Graph();
            parser.Load(g, t.Manifest.ResolveResourcePath(t.Action));
        }

        [ManifestTestRunner("http://www.w3.org/ns/rdftest#TestNTriplesNegativeSyntax")]
        public void NegativeSyntaxTest(ManifestTest t)
        {
            var parser = new NTriplesParser(NTriplesSyntax.Rdf11Star);
            var g = new Graph();
            Assert.ThrowsAny<RdfException>(() => parser.Load(g, t.Manifest.ResolveResourcePath(t.Action)));
        }
    }
}
