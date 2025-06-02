using VDS.RDF;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Query.Pull;
using Graph = VDS.RDF.Graph;

namespace dotNetRdf.Query.Pull.Tests;

public class AlgebraEvaluationTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly IGraph _g;
    private readonly ITripleStore _dataset;
    private readonly INode _alice;
    private readonly IUriNode _bob;
    private readonly IUriNode _carol;
    private readonly IUriNode _dave;
    private readonly IUriNode _bobHome;
    private readonly INode _foafKnows;
    private readonly INode _foafName;
    private readonly INode _foafHomepage;
    
    public AlgebraEvaluationTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _g = new Graph();
        _g.LoadFromFile("resources/single_graph.ttl");
        _dataset = new TripleStore();
        _dataset.Add(_g);
        INodeFactory nodeFactory = _g;
        _alice = nodeFactory.CreateUriNode(nodeFactory.UriFactory.Create("http://example.org/alice"));
        _bob = nodeFactory.CreateUriNode(nodeFactory.UriFactory.Create("http://example.org/bob"));
        _carol = nodeFactory.CreateUriNode(nodeFactory.UriFactory.Create("http://example.org/carol"));
        _dave = nodeFactory.CreateUriNode(nodeFactory.UriFactory.Create("http://example.org/dave"));
        _bobHome = nodeFactory.CreateUriNode(nodeFactory.UriFactory.Create("http://example.org/pages/bob"));
        _foafKnows = nodeFactory.CreateUriNode(nodeFactory.UriFactory.Create("http://xmlns.com/foaf/0.1/knows"));
        _foafName = nodeFactory.CreateUriNode(nodeFactory.UriFactory.Create("http://xmlns.com/foaf/0.1/name"));
        _foafHomepage = nodeFactory.CreateUriNode(nodeFactory.UriFactory.Create("http://xmlns.com/foaf/0.1/homepage"));
    }

    [Fact]
    public async Task SingleTriplePatternMatch()
    {
        var algebra = new Bgp(new TriplePattern(new NodeMatchPattern(_alice), new NodeMatchPattern(_foafKnows), new VariablePattern("x")));
        var p = new PullQueryProcessor(_dataset);
        var results = new List<ISet>();
        await foreach (ISet result in p.Evaluate(algebra, cancellationToken: TestContext.Current.CancellationToken))
        {
            results.Add(result);
        }

        Assert.Single(results);
    }
    [Fact]
    public async Task SingleVarTriplePatternJoin()
    {
        var algebra = new Bgp(new[]
        {
            new TriplePattern(
                new NodeMatchPattern(_alice),
                new NodeMatchPattern(_foafKnows),
                new VariablePattern("x")),
            new TriplePattern(
                new VariablePattern("x"),
                new NodeMatchPattern(_foafName),
                new VariablePattern("xname"))
        });
        var processor = new PullQueryProcessor(_dataset);
        var results = new List<ISet>();
        await foreach (ISet result in processor.Evaluate(algebra, cancellationToken: TestContext.Current.CancellationToken))
        {
            results.Add(result);
        }
        Assert.Single(results);
        ISet actual = results.First();
        Assert.Equal(_bob, actual["x"]);
        Assert.Equal("Bob", ((ILiteralNode)actual["xname"]).Value);
    }

    [Fact]
    public async Task SingleVarTriplePatternExtend()
    {
        var algebra = new Bgp(new[]
        {
            new TriplePattern(
                new NodeMatchPattern(_alice),
                new NodeMatchPattern(_foafKnows),
                new VariablePattern("x")),
            new TriplePattern(
                new VariablePattern("x"),
                new NodeMatchPattern(_foafName),
                new VariablePattern("xname")),
            new TriplePattern(
                new VariablePattern("x"),
                new NodeMatchPattern(_foafHomepage),
                new VariablePattern("xhome")),
        });
        var processor = new PullQueryProcessor(_dataset);
        var results = new List<ISet>();
        await foreach (ISet result in processor.Evaluate(algebra, cancellationToken: TestContext.Current.CancellationToken))
        {
            results.Add(result);
        }
        Assert.Single(results);
        ISet actual = results.First();
        Assert.Equal(_bob, actual["x"]);
        Assert.Equal("Bob", ((ILiteralNode)actual["xname"]).Value);
        Assert.Equal(_bobHome, actual["xhome"]);
    }

    [Fact]
    public async Task TestOptional()
    {
        var algebra = new LeftJoin(
            new Bgp(
                new TriplePattern(
                    new NodeMatchPattern(_bob),
                    new NodeMatchPattern(_foafKnows),
                    new VariablePattern("x")
                )
            ),
            new Bgp(
                new TriplePattern(
                    new VariablePattern("x"),
                    new NodeMatchPattern(_foafName),
                    new VariablePattern("xname")))
        );
        var processor =  new PullQueryProcessor(_dataset);
        IList<ISet> results = await processor.Evaluate(algebra, cancellationToken: TestContext.Current.CancellationToken).ToListAsync(cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(2, results.Count);
        var expectResult1 = new Set();
        expectResult1.Add("x", _carol);
        expectResult1.Add("xname", _g.CreateLiteralNode("Carol"));
        var expectResult2 = new Set();
        expectResult2.Add("x", _dave);
        expectResult2.Add("xname", null);
        Assert.Contains(expectResult1, results);
        Assert.Contains(expectResult2, results);
    }
}