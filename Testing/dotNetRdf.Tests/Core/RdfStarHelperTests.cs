using System.IO;
using VDS.RDF.Parsing;
using Xunit;

namespace VDS.RDF;

public class RdfStarHelperTests
{
    private readonly ITestOutputHelper _output;
    public RdfStarHelperTests(ITestOutputHelper output) {
        _output = output;
    }

    [Theory]
    [InlineData("test1.ttl", "test1.unstar.ttl")]
    [InlineData("test2.ttl", "test2.unstar.ttl")]
    [InlineData("test3.ttl", "test3.unstar.ttl")]
    [InlineData("string-literal-in-tn.ttl", "string-literal-in-tn.unstar.ttl")]
    [InlineData("no-lexical-for-bnodes.ttl", "no-lexical-for-bnodes.unstar.ttl")]
    [InlineData("nested-triple-nodes.ttl", "nested-triple-nodes.unstar.ttl")]
    [InlineData("no-triple-nodes.ttl", "no-triple-nodes.ttl")]
    [InlineData("test1.unstar.ttl", "test1.unstar.unstar.ttl")]
    public void TestUnstarOperation(string inputPath, string outputPath)
    {
        IGraph inputGraph = new Graph();
        inputGraph.LoadFromFile(Path.Combine("resources", "rdfstar", inputPath), new TurtleParser(TurtleSyntax.Rdf11Star, true));
        IGraph expectGraph = new Graph();
        expectGraph.LoadFromFile(Path.Combine("resources", "rdfstar", outputPath), new TurtleParser(TurtleSyntax.W3C, true));
        inputGraph.Unstar();
        var graphDiff = new GraphDiff();
        GraphDiffReport diffReport = graphDiff.Difference(expectGraph, inputGraph);
        TestTools.ShowDifferences(diffReport, outputPath, $"unstar('{inputPath}')", _output);
        Assert.True(diffReport.AreEqual, $"Expected unstar('{inputPath}') to be the same as '{outputPath}', but differences where found.");
    }
}
