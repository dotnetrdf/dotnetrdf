using dotNetRdf.TestSupport;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using Xunit;
using Xunit.Abstractions;

namespace dotNetRdf.TestSuite.Rdfa
{
    public class RdfaTestSuite : RdfTestSuite
    {
        private readonly ITestOutputHelper _output;
        private readonly string[] TestHostLanguages = new[] { "html5", "html5-invalid", "html4", "xhtml5", "xhtml5-invalid", "xml" };

        public static RdfaTestDataProvider RdfaTests = new RdfaTestDataProvider(
            new Uri("http://rdfa.info/test-suite/manifest.jsonld"),
            Path.Combine("resources", "test-suite", "manifest.jsonld"));

        public RdfaTestSuite(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestStoreManifestProvider()
        {
            var provider = new RdfaTestDataProvider(new Uri("http://rdfa.info/test-suite/manifest.jsonld"),
            Path.Combine("resources", "test-suite", "manifest.jsonld"));
            var testParams = provider.FirstOrDefault();
            Assert.NotNull(testParams);
            Assert.Single(testParams);
            Assert.NotNull(testParams[0]);
            var testData = Assert.IsType<RdfaTestData>(testParams[0]);
            Assert.NotNull(testData.Description);
            Assert.NotNull(testData.Input);
            Assert.NotNull(testData.Results);
            Assert.NotEmpty(testData.Versions);
        }

        [Theory]
        [MemberData(nameof(RdfaTests))]
        public void RunTest(RdfaTestData t)
        {
            _output.WriteLine($"{t.Id}: {t.InputPath}");
            _output.WriteLine(string.Join(", ", t.Versions));
            SparqlQueryParser queryParser = new SparqlQueryParser(SparqlQuerySyntax.Sparql_1_1);
            if (t.Versions.Contains("rdfa1.1"))
            {
                foreach (var hostLanguage in TestHostLanguages) {
                    if (t.HostLangauges.Contains(hostLanguage))
                    {
                        _output.WriteLine(hostLanguage);
                        var parser = new RdfAParser(RdfASyntax.RDFa_1_1);
                        var g = new Graph { BaseUri = new Uri(t.Id) };
                        var inputPath = t.GetInputPath("rdfa1.1", hostLanguage);
                        parser.Load(g, inputPath);
                        if (t.Results != null) {
                            SparqlQuery q = queryParser.ParseFromFile(t.GetResultPath("rdfa1.1", hostLanguage));
                            var results = g.ExecuteQuery(q);
                            SparqlResultSet resultSet = Assert.IsType<SparqlResultSet>(results);
                            Assert.Equal(t.ExpectedResults, resultSet.Result);
                        }
                    } else
                    {
                        Console.WriteLine($"Host language {hostLanguage} not found in {t.HostLangauges}");
                    }
                }
            }
        }
    }
}