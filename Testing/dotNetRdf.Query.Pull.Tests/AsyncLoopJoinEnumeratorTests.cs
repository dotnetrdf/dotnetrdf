using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Pull;
using VDS.RDF.Query.Pull.Algebra;

namespace dotNetRdf.Query.Pull.Tests;

public class AsyncLoopJoinEnumeratorTests : EnumeratorTestBase
{
    private readonly TripleStore _store = new ();
    private readonly PullEvaluationContext _context;

    public AsyncLoopJoinEnumeratorTests()
    {
        _store.Add(new Graph());
        _context = new PullEvaluationContext(_store);
    }
    [Fact]
    public async Task ItRunsALoopInnerJoin()
    {
        var lhs = new AsyncIntegerEnumeration(_nodeFactory, "x", 0, 60, 3);
        var rhs = new AsyncIntegerEnumeration(_nodeFactory, "x", 0, 60, 5);
        var join = new AsyncLoopJoinEvaluation(lhs, rhs, false);
        var results = await join.Evaluate(_context, null, null, TestContext.Current.CancellationToken).Select(s => s["x"].AsValuedNode().AsInteger()).ToArrayAsync(cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal([0,15, 30, 45,60], results);
    }
    
    [Fact]
    public async Task ItRunsALoopLeftJoin()
    {
        var lhs = new AsyncIntegerEnumeration(_nodeFactory, "x", 0, 60, 5);
        var rhs = new AsyncIntegerEnumeration(_nodeFactory, "x", 0, 60, 3);
        var join = new AsyncLoopJoinEvaluation(lhs, rhs, true);
        var results = await join.Evaluate(_context, null, null, TestContext.Current.CancellationToken).Select(s => s["x"].AsValuedNode().AsInteger()).ToArrayAsync(cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal([0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60], results);
    }

}