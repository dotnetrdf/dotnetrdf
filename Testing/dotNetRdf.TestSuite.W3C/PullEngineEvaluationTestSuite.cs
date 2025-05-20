#if NET6_0_OR_GREATER
using System.Threading.Tasks;
using VDS.RDF.Query;
using VDS.RDF.Query.Pull;
using Xunit;

namespace VDS.RDF.TestSuite.W3C;

public class PullEngineEvaluationTestSuite : BaseAsyncSparqlEvaluationTestSuite
{
    public PullEngineEvaluationTestSuite(ITestOutputHelper output) : base(output)
    {
        SkipTests["http://www.w3.org/2001/sw/DataAccess/tests/data-r2/i18n/manifest#normalization-2"] =
            "SPARQL URI Normalization is not implemented. URIs are normalized using standard .NET URI functionality.";
        SkipTests["http://www.w3.org/2001/sw/DataAccess/tests/data-r2/i18n/manifest#normalization-3"] =
            "SPARQL URI Normalization is not implemented. URIs are normalized using standard .NET URI functionality.";
    }

    [Theory]
    [MemberData(nameof(Sparql11QueryEvalTests))]
    public async Task RunSparql11TestSuite(ManifestTestData t)
    {
        await base.PerformTest(t);
    }

    [Theory]
    [MemberData(nameof(Sparql10QueryEvalTests))]
    public async Task RunSparql10TestSuite(ManifestTestData t)
    {
        await base.PerformTest(t);
    }

    [Fact]
    public async Task RunSingleQueryEvaluation()
    {
        const string testUrl = "http://www.w3.org/2001/sw/DataAccess/tests/data-r2/solution-seq/manifest#offset-4";

        ManifestTestDataProvider provider = testUrl.Contains("data-r2") ? Sparql10QueryEvalTests : Sparql11QueryEvalTests;
        ManifestTestData t = provider.GetTestData(testUrl);
        await base.PerformTest(t);
    }
    
    protected override async Task<object> ProcessQueryAsync(TripleStore tripleStore, SparqlQuery query)
    {
        var queryEngine = new PullQueryProcessor(tripleStore, (options => { options.QueryExecutionTimeout = 0; }));
            return await queryEngine.ProcessQueryAsync(query);
    }
}
#endif