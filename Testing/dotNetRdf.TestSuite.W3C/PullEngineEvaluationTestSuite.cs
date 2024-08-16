#if NET6_0_OR_GREATER
using dotNetRdf.Query.Pull;
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
        base.PerformTest(t);
    }

    [Theory]
    [MemberData(nameof(Sparql10QueryEvalTests))]
    public void RunSparql10TestSuite(ManifestTestData t)
    {
        base.PerformTest(t);
    }

    [Fact]
    public void RunSingleQueryEvaluation()
    {
        const string testUrl = "http://www.w3.org/2001/sw/DataAccess/tests/data-r2/open-world/manifest#open-eq-10";

        ManifestTestDataProvider provider = testUrl.Contains("data-r2") ? Sparql10QueryEvalTests : Sparql11QueryEvalTests;
        ManifestTestData t = provider.GetTestData(testUrl);
        base.PerformTest(t);
    }
    
    protected override async Task<object> ProcessQueryAsync(TripleStore tripleStore, SparqlQuery query)
    {
        var queryEngine = new PullQueryProcessor(tripleStore, (options => { options.QueryExecutionTimeout = 0; }));
            return await queryEngine.ProcessQueryAsync(query);
    }
}
#endif