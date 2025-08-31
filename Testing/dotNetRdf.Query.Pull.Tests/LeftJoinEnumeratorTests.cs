using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Pull.Algebra;

namespace dotNetRdf.Query.Pull.Tests;

public class LeftJoinEnumeratorTests : EnumeratorTestBase
{
    [Fact]
    public void WhenRhsShorter()
    {
        var lhs = new IntegerEnumeration(_nodeFactory, "x", 0, 60, 3);
        var rhs = new IntegerEnumeration(_nodeFactory, "x", 0, 60, 5);
        var join = new LeftJoinEvaluation(lhs, rhs, new[] { "x" }, new []{"x"}, null);
        var results = join.Evaluate(_context, null, cancellationToken: TestContext.Current.CancellationToken).ToList();
        Assert.Equal(21, results.Count);
        var resultValues = results.Select(r => r["x"]).OfType<ILiteralNode>().Select(n => n.AsValuedNode().AsInteger())
            .ToArray();
        Assert.Equivalent(new long [] { 0, 3, 6, 9, 12, 15, 18, 21, 24, 27, 30, 33, 36, 39, 42, 45, 48, 51, 54, 57, 60 }, resultValues);
    }

    [Fact]
    public void WhenLhsShorter()
    {
        var lhs = new IntegerEnumeration(_nodeFactory, "x", 0, 60, 5);
        var rhs = new IntegerEnumeration(_nodeFactory, "x", 0, 60, 3);
        var join = new LeftJoinEvaluation(lhs, rhs, new[] { "x" }, new[] { "x" }, null);
        var results = join.Evaluate(_context, null, cancellationToken: TestContext.Current.CancellationToken)
            .ToList();
        var resultValues = results.Select(r => r["x"]).OfType<ILiteralNode>().Select(n => n.AsValuedNode().AsInteger())
            .ToArray();
        Assert.Equal(13, resultValues.Length);
        Assert.Equivalent(new long [] { 0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60 }, resultValues);
    }

    [Fact]
    public void WhenBothSidesEqualLength()
    {
        var lhs = new IntegerEnumeration(_nodeFactory, "x", 0, 60, 3);
        var rhs = new IntegerEnumeration(_nodeFactory, "x", 0, 100, 5);
        var join = new LeftJoinEvaluation(lhs, rhs, ["x"], ["x"], null);
        var results = join.Evaluate(_context, null, cancellationToken: TestContext.Current.CancellationToken)
            .ToList();
        var resultValues = results.Select(r => r["x"]).OfType<ILiteralNode>().Select(n => n.AsValuedNode().AsInteger())
            .ToArray();
        Assert.Equal(21, resultValues.Length);
        Assert.Equivalent(new long [] { 0, 3, 6, 9, 12, 15, 18, 21, 24, 27, 30, 33, 36, 39, 42, 45, 48, 51, 54, 57, 60 }, resultValues);
    }
}