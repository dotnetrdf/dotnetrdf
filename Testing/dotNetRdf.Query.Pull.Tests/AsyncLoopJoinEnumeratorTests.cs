using VDS.RDF.Nodes;
using VDS.RDF.Query.Pull.Algebra;

namespace dotNetRdf.Query.Pull.Tests;

public class AsyncLoopJoinEnumeratorTests : EnumeratorTestBase
{
    [Fact]
    public async void ItRunsALoopInnerJoin()
    {
        var lhs = new AsyncIntegerEnumeration(_nodeFactory, "x", 0, 60, 3);
        var rhs = new AsyncIntegerEnumeration(_nodeFactory, "x", 0, 60, 5);
        var join = new AsyncLoopJoinEvaluation(lhs, rhs, false);
        var results = await join.Evaluate(null, null, null).Select(s => s["x"].AsValuedNode().AsInteger()).ToArrayAsync();
        Assert.Equal(new long[]{0,15, 30, 45,60}, results);
    }
    
    [Fact]
    public async void ItRunsALoopLeftJoin()
    {
        var lhs = new AsyncIntegerEnumeration(_nodeFactory, "x", 0, 60, 5);
        var rhs = new AsyncIntegerEnumeration(_nodeFactory, "x", 0, 60, 3);
        var join = new AsyncLoopJoinEvaluation(lhs, rhs, true);
        var results = await join.Evaluate(null, null, null).Select(s => s["x"].AsValuedNode().AsInteger()).ToArrayAsync();
        Assert.Equal(new long[]{0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60}, results);
    }

}