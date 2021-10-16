using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF.Parsing;
using VDS.RDF;
using Xunit;
using Xunit.Abstractions;

namespace VDS.RDF.TestSuite.RdfStar
{
    public class TurtleStarTestSuite : RdfTestSuite
    {
        public static ManifestTestDataProvider TurtleSyntaxTests = new ManifestTestDataProvider(
            new Uri("http://example/base/manifest.ttl"),
            Path.Combine("resources", "tests", "turtle", "syntax", "manifest.ttl"));

        public static ManifestTestDataProvider TurtleEvaluationTests = new ManifestTestDataProvider(
            new Uri("http://example/base/manifest.ttl"),
            Path.Combine("resources", "tests", "turtle", "eval", "manifest.ttl"));

        private readonly ITestOutputHelper _output;

        public TurtleStarTestSuite(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [MemberData(nameof(TurtleSyntaxTests))]
        [MemberData(nameof(TurtleEvaluationTests))]
        public void RunTest(ManifestTestData t)
        {
            _output.WriteLine($"{t.Id}: {t.Name} is a {t.Type}");
            InvokeTestRunner(t);
        }

        [ManifestTestRunner("http://www.w3.org/ns/rdftest#TestTurtlePositiveSyntax")]
        public void PositiveSyntaxTest(ManifestTestData t)
        {
            var parser = new TurtleParser(TurtleSyntax.Rdf11Star, false);
            var g = new Graph();
            parser.Load(g, t.Manifest.ResolveResourcePath(t.Action));
        }

        [ManifestTestRunner("http://www.w3.org/ns/rdftest#TestNTriplesPositiveSyntax")]
        public void NTriplesPositiveSyntaxTest(ManifestTestData t)
        {
            PositiveSyntaxTest(t);
        }

        [ManifestTestRunner("http://www.w3.org/ns/rdftest#TestTurtleNegativeSyntax")]
        public void NegativeSyntaxTest(ManifestTestData t)
        {
            var parser = new TurtleParser(TurtleSyntax.Rdf11Star, false);
            var g = new Graph();
            Assert.ThrowsAny<RdfException>(() => parser.Load(g, t.Manifest.ResolveResourcePath(t.Action)));
        }

        [ManifestTestRunner("http://www.w3.org/ns/rdftest#TestTurtleEval")]
        public void EvaluationTest(ManifestTestData t)
        {
            var ttlParser = new TurtleParser(TurtleSyntax.Rdf11Star, false);
            var g = new Graph();
            ttlParser.Load(g, t.Manifest.ResolveResourcePath(t.Action));
            var ntParser = new NTriplesParser(NTriplesSyntax.Rdf11Star);
            var h = new Graph();
            ntParser.Load(h, t.Manifest.ResolveResourcePath(t.Result));
            GraphDiffReport diffReport = g.Difference(h);
            if (!diffReport.AreEqual) TestTools.ShowDifferences(diffReport, _output);
            Assert.True(diffReport.AreEqual);
        }

        /* Helper method for debugging
        [Fact]
        public void TestSingle()
        {
            ManifestTestData testData = TurtleEvaluationTests.GetTestData(
                new Uri("https://w3c.github.io/rdf-star/tests/turtle/eval#turtle-star-bnode-2"));
            EvaluationTest(testData);
        }
        */
    }
}
