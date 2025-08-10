using System;
using System.IO;
using VDS.RDF.Query.Inference;
using Xunit;

namespace VDS.RDF.Parsing;

public class N3ParsingReasonerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public N3ParsingReasonerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void ParsingN3Reasoner()
    {
        var rules = "@prefix rdfs: <" + NamespaceMapper.RDFS + "> . { ?s rdfs:subClassOf ?class } => { ?s a ?class } .";

        var rulesGraph = new Graph();
        StringParser.Parse(rulesGraph, rules, new Notation3Parser());

        var data = new Graph();
        FileLoader.Load(data, Path.Combine("resources", "InferenceTest.ttl"));

        _testOutputHelper.WriteLine("Original Graph - " + data.Triples.Count + " Triples");
        var origCount = data.Triples.Count;
        foreach (Triple t in data.Triples)
        {
            Console.WriteLine(t.ToString());
        }

        var reasoner = new SimpleN3RulesReasoner();
        reasoner.Initialise(rulesGraph);

        reasoner.Apply(data);

        _testOutputHelper.WriteLine("Graph after Reasoner application - " + data.Triples.Count + " Triples");
        foreach (Triple t in data.Triples)
        {
            _testOutputHelper.WriteLine(t.ToString());
        }

        Assert.True(data.Triples.Count > origCount, "Number of Triples should have increased after the reasoner was run");
    }

    [Fact]
    public void ParsingN3ReasonerWithForAll()
    {
        var rules = "@prefix rdfs: <" + NamespaceMapper.RDFS + "> . @forAll :x . { :x rdfs:subClassOf ?class } => { :x a ?class } .";

        var rulesGraph = new Graph
        {
            BaseUri = new Uri("http://example.org/rules")
        };
        StringParser.Parse(rulesGraph, rules, new Notation3Parser());

        var data = new Graph();
        FileLoader.Load(data, Path.Combine("resources", "InferenceTest.ttl"));

        _testOutputHelper.WriteLine("Original Graph - " + data.Triples.Count + " Triples");
        var origCount = data.Triples.Count;
        foreach (Triple t in data.Triples)
        {
            _testOutputHelper.WriteLine(t.ToString());
        }

        var reasoner = new SimpleN3RulesReasoner();
        reasoner.Initialise(rulesGraph);

        reasoner.Apply(data);

        _testOutputHelper.WriteLine("Graph after Reasoner application - " + data.Triples.Count + " Triples");
        foreach (Triple t in data.Triples)
        {
            _testOutputHelper.WriteLine(t.ToString());
        }

        Assert.True(data.Triples.Count > origCount, "Number of Triples should have increased after the reasoner was run");
    }
}
