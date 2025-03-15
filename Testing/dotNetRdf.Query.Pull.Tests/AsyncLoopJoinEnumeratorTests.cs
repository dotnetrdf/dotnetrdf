using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Pull;
using VDS.RDF.Query.Pull.Algebra;

namespace dotNetRdf.Query.Pull.Tests;

public class AsyncLoopJoinEnumeratorTests : EnumeratorTestBase
{
    [Fact]
    [Obsolete("Tests obsolete method")]
    public async void ItRunsALoopInnerJoin()
    {
        var lhs = new AsyncIntegerEnumeration(_nodeFactory, "x", 0, 60, 3);
        var rhs = new AsyncIntegerEnumeration(_nodeFactory, "x", 0, 60, 5);
        var join = new AsyncLoopJoinEvaluation(lhs, rhs, false);
        var results = await join.Evaluate(null, null, null).Select(s => s["x"].AsValuedNode().AsInteger()).ToArrayAsync();
        Assert.Equal(new long[]{0,15, 30, 45,60}, results);
    }

    [Fact]
    public async void ItRunsALoopInnerJoinBatch()
    {
        var lhs = new AsyncIntegerEnumeration(_nodeFactory, "x", 0, 60, 3);
        var rhs = new AsyncIntegerEnumeration(_nodeFactory, "x", 0, 60, 5);
        var join = new AsyncLoopJoinEvaluation(lhs, rhs, false);
        IEnumerable<ISet?> input = [null];
        var results = new List<long>();
        await foreach (IEnumerable<ISet> batch in join.EvaluateBatch(new PullEvaluationContext(new TripleStore()), input, null))
        {
            results.AddRange(batch.Select(s => s["x"].AsValuedNode().AsInteger()));
        }
        Assert.Equal([0, 15, 30, 45, 60], results);
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

    [Fact]
    public async void ItRunsALoopLeftJoinBatch()
    {
        var lhs = new AsyncIntegerEnumeration(_nodeFactory, "x", 0, 60, 5);
        var rhs = new AsyncIntegerEnumeration(_nodeFactory, "x", 0, 60, 3);
        var join = new AsyncLoopJoinEvaluation(lhs, rhs, true);
        var results = new List<long>();
        await foreach (IEnumerable<ISet> batch in join.EvaluateBatch(new PullEvaluationContext(new TripleStore()), [null], null))
        {
            results.AddRange(batch.Select(s => s["x"].AsValuedNode().AsInteger()));
        }

        Assert.Equal([0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60], results);
    }

}