using FluentAssertions;
using System.Collections.Generic;
using VDS.RDF.Parsing;
using Xunit;

namespace VDS.RDF.Writing;

public abstract class RdfStarWriterTests
{
    protected ITestOutputHelper Output { get; }

    protected RdfStarWriterTests(ITestOutputHelper output)
    {
        Output = output;
    }

    public abstract IRdfWriter GetWriter();
    public abstract IRdfReader GetReader();

    public static IEnumerable<TheoryDataRow<string, string>> RoundTripTestData = [
        new(
            "Triple Node Subject",
            "<< <http://example.org/s> <http://example.org/p> <http://example.org/o> >> <http://example.org/p> <http://example.org/o> ."
        ),
        new(
            "Triple Node Object",
            "<http://example.org/s> <http://example.org/p> << <http://example.org/s> <http://example.org/p> <http://example.org/o> >> ."
        ),
        new(
            "Annotated Triple",
            @"<< <http://example.org/s> <http://example.org/p> <http://example.org/o> >> <http://example.org/p> <http://example.org/o> .
                  <http://example.org/s> <http://example.org/p> <http://example.org/o> ."
        ),
        new (
            "Multiple Triple Annotations",
            @"<< <http://example.org/s> <http://example.org/p> <http://example.org/o> >> <http://example.org/p> <http://example.org/o> .
                << <http://example.org/s> <http://example.org/p> <http://example.org/o> >> <http://example.org/p> <http://example.org/o2> .
                << <http://example.org/s> <http://example.org/p> <http://example.org/o> >> <http://example.org/p2> <http://example.org/o> .
                <http://example.org/s> <http://example.org/p> <http://example.org/o> ."
        )
    ];

    protected void RoundTrip(string input)
    {
        var g = new Graph();
        var stringWriter = new System.IO.StringWriter();
        IRdfWriter writer = GetWriter();
        IRdfReader reader = GetReader();

        g.LoadFromString(input, new NTriplesParser(NTriplesSyntax.Rdf11Star));
        g.NamespaceMap.AddNamespace("ex", UriFactory.Root.Create("http://example.org/"));
        writer.Save(g, stringWriter);

        Output.WriteLine(stringWriter.ToString());

        var h = new Graph();
        h.LoadFromString(stringWriter.ToString(), reader);
        
        var comparer = new GraphDiff();
        GraphDiffReport report = comparer.Difference(g, h);
        TestTools.ShowDifferences(report);
        report.AreEqual.Should().Be(true);
    }

}

public class TurtleStarWriterMinimalCompressionTests : RdfStarWriterTests
{
    public TurtleStarWriterMinimalCompressionTests(ITestOutputHelper output) : base(output){}

    public override IRdfReader GetReader()
    {
        return new TurtleParser(TurtleSyntax.Rdf11Star, false);

    }

    public override IRdfWriter GetWriter()
    {
        return new CompressingTurtleWriter(WriterCompressionLevel.Minimal, TurtleSyntax.Rdf11Star);
    }

    [Theory]
    [MemberData(nameof(RoundTripTestData))]
    public void TestRoundTrip(string _, string input)
    {
        RoundTrip(input);
    }
}

public class TurtleStarWriterHighCompressionTests : RdfStarWriterTests
{
    public TurtleStarWriterHighCompressionTests(ITestOutputHelper output):base(output){}

    public override IRdfReader GetReader()
    {
        return new TurtleParser(TurtleSyntax.Rdf11Star, false);
        
    }

    public override IRdfWriter GetWriter()
    {
        return new CompressingTurtleWriter(WriterCompressionLevel.High, TurtleSyntax.Rdf11Star)
        {
            HighSpeedModePermitted = false
        };
    }

    [Theory]
    [MemberData(nameof(RoundTripTestData))]
    public void TestRoundTrip(string _, string input)
    {
        RoundTrip(input);
    }
}
