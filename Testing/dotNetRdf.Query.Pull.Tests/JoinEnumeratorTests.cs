using VDS.RDF;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Pull;
using VDS.RDF.Query.Pull.Algebra;

namespace dotNetRdf.Query.Pull.Tests;

public class JoinEnumeratorTests : EnumeratorTestBase
{
    [Fact]
    public void TestIntegerSequence()
    {
        var enumeration = new IntegerEnumeration(_nodeFactory, "x", 0, 6, 3);
        using IEnumerator<ISet> seq = enumeration.Evaluate(_context, null, null, TestContext.Current.CancellationToken).GetEnumerator();
        Assert.True(seq.MoveNext());
        Assert.Equal("0", (seq.Current["x"] as ILiteralNode)?.Value);
        Assert.True(seq.MoveNext());
        Assert.Equal("3", (seq.Current["x"] as ILiteralNode)?.Value);
        Assert.True(seq.MoveNext());
        Assert.Equal("6", (seq.Current["x"] as ILiteralNode)?.Value);
        Assert.False(seq.MoveNext());
    }

    [Fact]
    public void SimpleJoinLhsShorter()
    {
        var lhs = new IntegerEnumeration(_nodeFactory, "x", 0, 60, 5);
        var rhs = new IntegerEnumeration(_nodeFactory, "x", 0, 60, 3);
        var join = new JoinEvaluation(lhs, rhs, ["x"]);
        var results = join.Evaluate(_context, null, cancellationToken: TestContext.Current.CancellationToken).ToList();
        Assert.Equal(5, results.Count);
        var resultValues = results.Select(r => r["x"]).OfType<ILiteralNode>().Select(n => n.Value).ToArray();
        Assert.Contains("0", resultValues);
        Assert.Equivalent(new []{"0", "15", "30", "60", "45"}, resultValues);
    }

    [Fact]
    public void SimpleJoinRhsShorter()
    {
        var lhs = new IntegerEnumeration(_nodeFactory, "x", 0, 60, 3);
        var rhs = new IntegerEnumeration(_nodeFactory, "x", 0, 60, 5);
        var join = new JoinEvaluation(lhs, rhs, ["x"]);
        var results = join.Evaluate(_context, null, cancellationToken: TestContext.Current.CancellationToken).ToList();
        Assert.Equal(5, results.Count);
        var resultValues = results.Select(r => r["x"]).OfType<ILiteralNode>().Select(n => n.Value).ToArray();
        Assert.Contains("0", resultValues);
        Assert.Equivalent(new []{"0", "15", "30", "60", "45"}, resultValues);
    }
    
}