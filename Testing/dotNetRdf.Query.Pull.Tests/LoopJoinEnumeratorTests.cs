using VDS.RDF.Nodes;
using VDS.RDF.Query.Pull.Algebra;

namespace dotNetRdf.Query.Pull.Tests;

public class LoopJoinEnumeratorTests : EnumeratorTestBase
{
    [Fact]
    public void ItRunsALoopInnerJoin()
    {
        var lhs = new IntegerEnumeration(_nodeFactory, "x", 0, 60, 3);
        var rhs = new IntegerEnumeration(_nodeFactory, "x", 0, 60, 5);
        var join = new LoopJoinEvaluation(lhs, rhs, false);
        var results = join.Evaluate(_context, null, null, TestContext.Current.CancellationToken).Select(s => s["x"].AsValuedNode().AsInteger()).ToArray();
        Assert.Equal(new long[]{0,15, 30, 45,60}, results);
    }

    [Fact]
    public void ItRunsALoopLeftJoin()
    {
        var lhs = new IntegerEnumeration(_nodeFactory, "x", 0, 60, 5);
        var rhs = new IntegerEnumeration(_nodeFactory, "x", 0, 60, 3);
        var join = new LoopJoinEvaluation(lhs, rhs, true);
        var results = join.Evaluate(_context, null, null, TestContext.Current.CancellationToken).Select(s => s["x"].AsValuedNode().AsInteger()).ToArray();
        Assert.Equal(new long[]{0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60}, results);
    }
}