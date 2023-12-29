using VDS.RDF;
using VDS.RDF.Query.Algebra;

namespace dotNetRdf.Query.PullEvaluation.Tests;

public class JoinEnumeratorTests : EnumeratorTestBase
{
    [Fact]
    public async void TestIntegerSequence()
    {
        var seq = new IntegerSequence(_nodeFactory, "x", 0, 6, 3);
        Assert.Throws<InvalidOperationException>(() => seq.Current);
        Assert.True(await seq.MoveNextAsync());
        Assert.Equal("0", (seq.Current["x"] as ILiteralNode)?.Value);
        Assert.True(await seq.MoveNextAsync());
        Assert.Equal("3", (seq.Current["x"] as ILiteralNode)?.Value);
        Assert.True(await seq.MoveNextAsync());
        Assert.Equal("6", (seq.Current["x"] as ILiteralNode)?.Value);
        Assert.False(await seq.MoveNextAsync());
    }

    [Fact]
    public async void SimpleJoin()
    {
        var lhs = new IntegerSequence(_nodeFactory, "x", 0, 60, 3, wait:100);
        var rhs = new IntegerSequence(_nodeFactory, "x", 0, 60, 5);
        var join = new JoinEnumerator(lhs, rhs, new[] { "x" });
        List<ISet> results = await ReadAllAsync(join);
        Assert.Equal(5, results.Count);
        var resultValues = results.Select(r => r["x"]).OfType<ILiteralNode>().Select(n => n.Value).ToArray();
        Assert.Contains("0", resultValues);
        Assert.Equivalent(new []{"0", "15", "30", "60", "45"}, resultValues);
    }
}