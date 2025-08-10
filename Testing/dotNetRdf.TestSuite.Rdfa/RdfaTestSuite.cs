using dotNetRdf.TestSupport;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace dotNetRdf.TestSuite.Rdfa;

public class RdfaTestSuite : RdfTestSuite
{
    private readonly ITestOutputHelper _output;
    private readonly string[] _testHostLanguages = { "html5", "html5-invalid", "html4", "xhtml5", "xhtml5-invalid", "xml" };

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
        RdfaTestData testData = Assert.IsType<RdfaTestData>(testParams[0]);
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
        var queryParser = new SparqlQueryParser(SparqlQuerySyntax.Sparql_1_1);
        var baseUri = new Uri(t.Id);
        var lastSegment = baseUri.Segments[baseUri.Segments.Length - 1];
        if (t.Versions.Contains("rdfa1.1"))
        {
            foreach (var hostLanguage in _testHostLanguages) {
                if (t.HostLangauges.Contains(hostLanguage))
                {
                    _output.WriteLine(hostLanguage);
                    var options = new RdfAParserOptions
                    {
                        Syntax = RdfASyntax.RDFa_1_1, Base = t.GetInputUrl("rdfa1.1", hostLanguage)
                    };
                    var parser = new RdfAParser(options);
                    var g = new Graph { BaseUri = t.GetInputUrl("rdfa1.1", hostLanguage) };
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
                    _output.WriteLine($"Host language {hostLanguage} not found in test host languages. SKIPPED.");
                }
            }
        }
    }

    private void RunTestInternal(RdfaTestData t, string[] hostLanguages)
    {
        var queryParser = new SparqlQueryParser(SparqlQuerySyntax.Sparql_1_1);
        if (t.Versions.Contains("rdfa1.1"))
        {
            foreach (var hostLanguage in hostLanguages)
            {
                if (t.HostLangauges.Contains(hostLanguage))
                {
                    _output.WriteLine($"Running test for {hostLanguage}");
                    var options = new RdfAParserOptions
                    {
                        Syntax = RdfASyntax.RDFa_1_1, Base = t.GetInputUrl("rdfa1.1", hostLanguage)
                    };
                    var parser = new RdfAParser(options);
                    var g = new Graph { BaseUri = options.Base };
                    var inputPath = t.GetInputPath("rdfa1.1", hostLanguage);
                    parser.Load(g, inputPath);
                    if (t.Results != null)
                    {
                        SparqlQuery q = queryParser.ParseFromFile(t.GetResultPath("rdfa1.1", hostLanguage));
                        var results = g.ExecuteQuery(q);
                        SparqlResultSet resultSet = Assert.IsType<SparqlResultSet>(results);
                        Assert.Equal(t.ExpectedResults, resultSet.Result);
                    }
                }
                else
                {
                    _output.WriteLine($"Host language {hostLanguage} not found in {t.HostLangauges}");
                }
            }
        }
    } 

    [Fact]
    public void RunSingleTest()
    {
        var testCase = "0326";
        RdfaTestData? testData = RdfaTests.Select(testParams => testParams[0]).OfType<RdfaTestData>()
            .FirstOrDefault(testData => testData.Id.EndsWith(testCase));
        Assert.NotNull(testData);
        RunTestInternal(testData, new []{"html5"});
    }
}