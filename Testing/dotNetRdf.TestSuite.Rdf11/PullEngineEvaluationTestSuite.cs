#if NET6_0_OR_GREATER
using dotNetRdf.Query.PullEvaluation;
using System;
using System.Threading.Tasks;
using VDS.RDF.Query;
using Xunit;
using Xunit.Abstractions;

namespace VDS.RDF.TestSuite.Rdf11;

public class PullEngineEvaluationTestSuite : BaseAsyncSparqlEvaluationTestSuite
{
    public PullEngineEvaluationTestSuite(ITestOutputHelper output) : base(output) {}

    [Theory]
    [MemberData(nameof(SparqlQueryEvalTests))]
    public void RunQueryEvaluationTest(ManifestTestData t)
    {
        base.PerformQueryEvaluationTest(t);
    }

    [Theory]
    [MemberData(nameof(DawgQueryEvalTests))]
    public void RunDawgEvaluationTest(ManifestTestData t)
    {
        base.PerformQueryEvaluationTest(t);
    }

    [Fact]
    public void RunSingle()
    {
        // ManifestTestData t = DawgQueryEvalTests.GetTestData(
        //     "http://www.w3.org/2001/sw/DataAccess/tests/data-r2/optional-filter/manifest#dawg-optional-filter-001");
        // base.PerformQueryEvaluationTest(t);

        ManifestTestData t = SparqlQueryEvalTests.GetTestData(
            "http://www.w3.org/2009/sparql/docs/tests/data-sparql11/subquery/manifest#subquery02");
        base.PerformQueryEvaluationTest(t);
    }
    
    protected override async Task<object> ProcessQueryAsync(TripleStore tripleStore, SparqlQuery query)
    {
            var queryEngine = new PullQueryProcessor(tripleStore);
            return await queryEngine.ProcessQueryAsync(query);
    }
}
#endif