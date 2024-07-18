using VDS.RDF;
using VDS.RDF.Query.Algebra;

namespace dotNetRdf.Query.PullEvaluation.Tests;

public class JoinEnumeratorTests : EnumeratorTestBase
{
    [Fact]
    public async void TestIntegerSequence()
    {
        var enumeration = new AsyncIntegerEnumeration(_nodeFactory, "x", 0, 6, 3);
        IAsyncEnumerator<ISet> seq = enumeration.Evaluate(null, null, null).GetAsyncEnumerator();
        Assert.True(await seq.MoveNextAsync());
        Assert.Equal("0", (seq.Current["x"] as ILiteralNode)?.Value);
        Assert.True(await seq.MoveNextAsync());
        Assert.Equal("3", (seq.Current["x"] as ILiteralNode)?.Value);
        Assert.True(await seq.MoveNextAsync());
        Assert.Equal("6", (seq.Current["x"] as ILiteralNode)?.Value);
        Assert.False(await seq.MoveNextAsync());
    }

    [Fact]
    public async void SimpleJoinLhsCompletesFirst()
    {
        var lhs = new AsyncIntegerEnumeration(_nodeFactory, "x", 0, 60, 3);
        var rhs = new AsyncIntegerEnumeration(_nodeFactory, "x", 0, 60, 5, wait:100);
        var join = new AsyncJoinEvaluation(lhs, rhs, new[] { "x" });
        List<ISet> results = await join.Evaluate(null, null).ToListAsync();
        Assert.Equal(5, results.Count);
        var resultValues = results.Select(r => r["x"]).OfType<ILiteralNode>().Select(n => n.Value).ToArray();
        Assert.Contains("0", resultValues);
        Assert.Equivalent(new []{"0", "15", "30", "60", "45"}, resultValues);
    }

    [Fact]
    public async void SimpleJoinRhsCompletesFirst()
    {
        var lhs = new AsyncIntegerEnumeration(_nodeFactory, "x", 0, 60, 3, wait:100);
        var rhs = new AsyncIntegerEnumeration(_nodeFactory, "x", 0, 60, 5);
        var join = new AsyncJoinEvaluation(lhs, rhs, new[] { "x" });
        List<ISet> results = await join.Evaluate(null, null).ToListAsync();
        Assert.Equal(5, results.Count);
        var resultValues = results.Select(r => r["x"]).OfType<ILiteralNode>().Select(n => n.Value).ToArray();
        Assert.Contains("0", resultValues);
        Assert.Equivalent(new []{"0", "15", "30", "60", "45"}, resultValues);
    }
    
    [Fact]
    public async void SimpleJoinLhsInterleaved()
    {
        var lhs = new AsyncIntegerEnumeration(_nodeFactory, "x", 0, 60, 3);
        var rhs = new AsyncIntegerEnumeration(_nodeFactory, "x", 0, 60, 5);
        var join = new AsyncJoinEvaluation(lhs, rhs, new[] { "x" });
        List<ISet> results = await join.Evaluate(null, null).ToListAsync();
        Assert.Equal(5, results.Count);
        var resultValues = results.Select(r => r["x"]).OfType<ILiteralNode>().Select(n => n.Value).ToArray();
        Assert.Contains("0", resultValues);
        Assert.Equivalent(new []{"0", "15", "30", "60", "45"}, resultValues);
    }

}