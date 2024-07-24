#if NET6_0_OR_GREATER
using dotNetRdf.Query.PullEvaluation;
using System.Threading.Tasks;
using VDS.RDF.Query;
using Xunit;
using Xunit.Abstractions;

namespace VDS.RDF.TestSuite.W3C;

public class PullEngineEvaluationTestSuite(ITestOutputHelper output) : BaseAsyncSparqlEvaluationTestSuite(output)
{
    [Theory]
    [MemberData(nameof(Sparql11QueryEvalTests))]
    public void RunSparql11TestSuite(ManifestTestData t)
    {
        base.PerformQueryEvaluationTest(t);
    }

    [Theory]
    [MemberData(nameof(Sparql10QueryEvalTests))]
    public void RunSparql10TestSuite(ManifestTestData t)
    {
        base.PerformQueryEvaluationTest(t);
    }

    [Fact]
    public void RunSingleQueryEvaluation()
    {
        const string testUrl = "http://www.w3.org/2009/sparql/docs/tests/data-sparql11/functions/manifest#encode01";

        ManifestTestDataProvider provider = testUrl.Contains("data-r2") ? Sparql10QueryEvalTests : Sparql11QueryEvalTests;
        ManifestTestData t = provider.GetTestData(testUrl);
        base.PerformQueryEvaluationTest(t);
    }
    
    protected override async Task<object> ProcessQueryAsync(TripleStore tripleStore, SparqlQuery query)
    {
            var queryEngine = new PullQueryProcessor(tripleStore) { Timeout = 0 };
            return await queryEngine.ProcessQueryAsync(query);
    }
}
#endif