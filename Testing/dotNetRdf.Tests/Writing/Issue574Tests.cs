using System;
using System.IO;
using VDS.RDF.Parsing;
using Xunit;
using Xunit.Abstractions;

namespace VDS.RDF.Writing;

public class Issue574Tests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Issue574Tests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void CompressingTurtleWriterHandlesAListWithAnEmptyNode()
    {
        var parser = new RdfJsonParser();
        var writer = new CompressingTurtleWriter();
        var stringWriter = new System.IO.StringWriter();
        IGraph g = new Graph();
        parser.Load(g, Path.Combine("resources", "issue-574", "data.json"));
        writer.Save(g, stringWriter);
        _testOutputHelper.WriteLine(stringWriter.ToString());
    }
}