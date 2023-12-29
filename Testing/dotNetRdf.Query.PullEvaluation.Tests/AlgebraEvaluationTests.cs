using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Patterns;
using Xunit.Abstractions;
using Graph = VDS.RDF.Graph;

namespace dotNetRdf.Query.PullEvaluation.Tests;

public class AlgebraEvaluationTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly ISparqlDataset _dataset;
    private readonly INode _alice;
    private readonly IUriNode _bob;
    private readonly IUriNode _bobHome;
    private readonly INode _foafKnows;
    private readonly INode _foafName;
    private readonly INode _foafHomepage;
    
    public AlgebraEvaluationTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        var g = new Graph();
        g.LoadFromFile("resources/single_graph.ttl");
        _dataset = new InMemoryDataset(g);
        INodeFactory nodeFactory = g;
        _alice = nodeFactory.CreateUriNode(nodeFactory.UriFactory.Create("http://example.org/alice"));
        _bob = nodeFactory.CreateUriNode(nodeFactory.UriFactory.Create("http://example.org/bob"));
        _bobHome = nodeFactory.CreateUriNode(nodeFactory.UriFactory.Create("http://example.org/pages/bob"));
        _foafKnows = nodeFactory.CreateUriNode(nodeFactory.UriFactory.Create("http://xmlns.com/foaf/0.1/knows"));
        _foafName = nodeFactory.CreateUriNode(nodeFactory.UriFactory.Create("http://xmlns.com/foaf/0.1/name"));
        _foafHomepage = nodeFactory.CreateUriNode(nodeFactory.UriFactory.Create("http://xmlns.com/foaf/0.1/homepage"));
    }

    [Fact]
    public async void SingleTriplePatternMatch()
    {
        var algebra = new Bgp(new TriplePattern(new NodeMatchPattern(_alice), new NodeMatchPattern(_foafKnows), new VariablePattern("x")));
        var p = new PullQueryProcessor(algebra, _dataset);
        var results = new List<ISet>();
        await foreach (ISet result in p.Evaluate())
        {
            results.Add(result);
        }

        Assert.Single(results);
    }
    [Fact]
    public async void SingleVarTriplePatternJoin()
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
        var processor = new PullQueryProcessor(algebra, _dataset);
        var results = new List<ISet>();
        await foreach (ISet result in processor.Evaluate())
        {
            results.Add(result);
        }
        Assert.Single(results);
        ISet actual = results.First();
        Assert.Equal(_bob, actual["x"]);
        Assert.Equal("Bob", ((ILiteralNode)actual["xname"]).Value);
    }

    [Fact]
    public async void SingleVarTriplePatternExtend()
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
        var processor = new PullQueryProcessor(algebra, _dataset);
        var results = new List<ISet>();
        await foreach (ISet result in processor.Evaluate())
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
    public void TestOptional()
    {
        var query = "SELECT * WHERE { ?x a ?y OPTIONAL { ?x <http://example.org/p> ?o } }";
        var parser = new SparqlQueryParser();
        SparqlQuery sq = parser.ParseFromString(query);
        _testOutputHelper.WriteLine(sq.ToAlgebra().ToString());
        query = "SELECT * WHERE { ?x a ?y { ?x <http://example.org/p> ?o } }";
        sq = parser.ParseFromString(query);
        _testOutputHelper.WriteLine(sq.ToAlgebra().ToString());
        query = "SELECT * WHERE { ?x a ?y . ?x <http://example.org/p> ?o }";
        sq = parser.ParseFromString(query);
        _testOutputHelper.WriteLine(sq.ToAlgebra().ToString());
    }
}