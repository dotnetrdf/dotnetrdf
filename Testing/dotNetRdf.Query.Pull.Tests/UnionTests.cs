using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Pull;
using VDS.RDF.Query.Pull.Algebra;

namespace dotNetRdf.Query.Pull.Tests;

public class UnionTests : EnumeratorTestBase
{
    [Fact]
    public void Union()
    {
        var lhs = new IntegerEnumeration(_nodeFactory, "x", 0, 60, 5);
        var rhs = new IntegerEnumeration(_nodeFactory, "x", 0, 60, 3);
        var union = new UnionEvaluation(lhs, rhs);
        var resultSets = union.Evaluate(new PullEvaluationContext(new TripleStore()), null, null, TestContext.Current.CancellationToken).ToList();
        var results = resultSets.Select(s => s["x"].AsValuedNode().AsInteger()).ToList();
        Assert.Equal(34, results.Count);
    }
}