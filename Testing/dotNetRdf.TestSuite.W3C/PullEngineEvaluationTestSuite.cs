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
        ManifestTestDataProvider provider = Sparql10QueryEvalTests;
        // ManifestTestDataProvider provider = Sparql11QueryEvalTests;
        
        ManifestTestData t = provider.GetTestData(
            "http://www.w3.org/2001/sw/DataAccess/tests/data-r2/graph/manifest#dawg-graph-09");
        base.PerformQueryEvaluationTest(t);
    }
    
    protected override async Task<object> ProcessQueryAsync(TripleStore tripleStore, SparqlQuery query)
    {
            var queryEngine = new PullQueryProcessor(tripleStore);
            return await queryEngine.ProcessQueryAsync(query);
    }
}
#endif