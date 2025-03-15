using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Pull;
using VDS.RDF.Query.Pull.Algebra;

namespace dotNetRdf.Query.Pull.Tests;

public class AsyncUnionTests : EnumeratorTestBase
{
    [Fact]
    public async void AsyncUnion()
    {
        var lhs = new AsyncIntegerEnumeration(_nodeFactory, "x", 0, 60, 5);
        var rhs = new AsyncIntegerEnumeration(_nodeFactory, "x", 0, 60, 3);
        var union = new AsyncUnionEvaluation(lhs, rhs);
        List<ISet> resultSets = await GetBatchResultsAsync(union.EvaluateBatch(
            new PullEvaluationContext(new TripleStore()),
            [null],
            null));
        List<long> results = resultSets.Select(s => s["x"].AsValuedNode().AsInteger()).ToList();
        Assert.Equal(34, results.Count);
    }
}