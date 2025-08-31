using dotNetRdf.Query.Pull.Tests;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Pull;
using VDS.RDF.Query.PullAsync;
using VDS.RDF.Query.PullAsync.Algebra;

namespace dotNetRdf.Query.PullAsync.Tests;

public class AsyncUnionTests : EnumeratorTestBase
{
    [Fact]
    public async Task AsyncUnion()
    {
        var lhs = new AsyncIntegerEnumeration(NodeFactory, "x", 0, 60, 5);
        var rhs = new AsyncIntegerEnumeration(NodeFactory, "x", 0, 60, 3);
        var union = new AsyncUnionEvaluation(lhs, rhs);
        List<ISet> resultSets = await GetBatchResultsAsync(union.EvaluateBatch(
            new PullEvaluationContext(new TripleStore()),
            [null],
            null,
            TestContext.Current.CancellationToken));
        var results = resultSets.Select(s => s["x"].AsValuedNode().AsInteger()).ToList();
        Assert.Equal(34, results.Count);
    }
}