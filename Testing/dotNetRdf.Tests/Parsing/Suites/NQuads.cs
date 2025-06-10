using System.IO;
using Xunit;

namespace VDS.RDF.Parsing.Suites;


public class NQuads
    : BaseDatasetParserSuite
{
    private readonly ITestOutputHelper _testOutputHelper;

    public NQuads(ITestOutputHelper testOutputHelper)
        : base(new NQuadsParser(), new NQuadsParser(), "nquads11")
    {
        _testOutputHelper = testOutputHelper;
        CheckResults = false;
        Parser.Warning += TestTools.WarningPrinter;
    }

    [Fact]
    public void ParsingSuitesNQuads11()
    {
        //Nodes for positive and negative tests
        var g = new Graph();
        g.NamespaceMap.AddNamespace("rdft", UriFactory.Root.Create("http://www.w3.org/ns/rdftest#"));
        INode posSyntaxTest = g.CreateUriNode("rdft:TestNQuadsPositiveSyntax");
        INode negSyntaxTest = g.CreateUriNode("rdft:TestNQuadsNegativeSyntax");

        //Run manifests
        RunManifest(Path.Combine("resources", "nquads11", "manifest.ttl"), posSyntaxTest, negSyntaxTest);

        if (Count == 0) Assert.Fail("No tests found");

        _testOutputHelper.WriteLine(Count + " Tests - " + Passed + " Passed - " + Failed + " Failed");
        _testOutputHelper.WriteLine(((Passed / (double)Count) * 100) + "% Passed");

        if (Failed > 0) Assert.Fail(Failed + " Tests failed");
        Assert.SkipWhen(Indeterminate > 0, Indeterminate + " Tests are indeterminate");
    }
}
