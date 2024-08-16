using dotNetRdf.Query.Pull.Algebra;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Algebra;

namespace dotNetRdf.Query.Pull.Tests;

public class LeftJoinEnumeratorTests : EnumeratorTestBase
{
    [Fact]
    public async void WhenRhsCompletesFirst()
    {
        var lhs = new AsyncIntegerEnumeration(_nodeFactory, "x", 0, 60, 3, 100);
        var rhs = new AsyncIntegerEnumeration(_nodeFactory, "x", 0, 60, 5);
        var join = new AsyncLeftJoinEvaluation(lhs, rhs, new[] { "x" }, new []{"x"}, null);
        List<ISet> results = await join.Evaluate(null, null).ToListAsync();
        Assert.Equal(21, results.Count);
        var resultValues = results.Select(r => r["x"]).OfType<ILiteralNode>().Select(n => n.AsValuedNode().AsInteger())
            .ToArray();
        Assert.Equivalent(new long [] { 0, 3, 6, 9, 12, 15, 18, 21, 24, 27, 30, 33, 36, 39, 42, 45, 48, 51, 54, 57, 60 }, resultValues);
    }

    [Fact]
    public async void WhenLhsCompletesFirst()
    {
        var lhs = new AsyncIntegerEnumeration(_nodeFactory, "x", 0, 60, 3);
        var rhs = new AsyncIntegerEnumeration(_nodeFactory, "x", 0, 60, 5, 100);
        var join = new AsyncLeftJoinEvaluation(lhs, rhs, new[] { "x" }, new[] { "x" }, null);
        List<ISet> results = await join.Evaluate(null, null).ToListAsync();
        var resultValues = results.Select(r => r["x"]).OfType<ILiteralNode>().Select(n => n.AsValuedNode().AsInteger())
            .ToArray();
        Assert.Equal(21, resultValues.Length);
        Assert.Equivalent(new long [] { 0, 3, 6, 9, 12, 15, 18, 21, 24, 27, 30, 33, 36, 39, 42, 45, 48, 51, 54, 57, 60 }, resultValues);
    }

    [Fact]
    public async void WhenInterleaved()
    {
        var lhs = new AsyncIntegerEnumeration(_nodeFactory, "x", 0, 60, 3, 5);
        var rhs = new AsyncIntegerEnumeration(_nodeFactory, "x", 0, 100, 5, 5);
        var join = new AsyncLeftJoinEvaluation(lhs, rhs, new[] { "x" },  new[] { "x" }, null);
        List<ISet> results = await join.Evaluate(null, null).ToListAsync();
        var resultValues = results.Select(r => r["x"]).OfType<ILiteralNode>().Select(n => n.AsValuedNode().AsInteger())
            .ToArray();
        Assert.Equal(21, resultValues.Length);
        Assert.Equivalent(new long [] { 0, 3, 6, 9, 12, 15, 18, 21, 24, 27, 30, 33, 36, 39, 42, 45, 48, 51, 54, 57, 60 }, resultValues);
    }
}