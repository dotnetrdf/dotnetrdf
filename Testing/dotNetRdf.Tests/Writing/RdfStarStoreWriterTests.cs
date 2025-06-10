using System.Collections.Generic;
using VDS.RDF.Parsing;
using Xunit;

namespace VDS.RDF.Writing;

public abstract class RdfStarStoreWriterTests
{
    protected ITestOutputHelper Output { get; }

    protected RdfStarStoreWriterTests(ITestOutputHelper output)
    {
        Output = output;
    }

    public abstract IStoreWriter GetWriter();
    public abstract IStoreReader GetReader();

    public static IEnumerable<TheoryDataRow<string, string>> RoundTripTestData = [
        new(
            "Triple Node Subject",
            "<< <http://example.org/s> <http://example.org/p> <http://example.org/o> >> <http://example.org/p> <http://example.org/o> <http://example.org/g> ."
        ),
        new(
            "Triple Node Object",
            "<http://example.org/s> <http://example.org/p> << <http://example.org/s> <http://example.org/p> <http://example.org/o> >> <http://example.org/g> ."
        ),
        new(
            "Annotated Triple",
            @"<< <http://example.org/s> <http://example.org/p> <http://example.org/o> >> <http://example.org/p> <http://example.org/o> <http://example.org/g> .
                  <http://example.org/s> <http://example.org/p> <http://example.org/o> <http://example.org/g> ."
        ),
        new(
            "Multiple Triple Annotations",
            @"<< <http://example.org/s> <http://example.org/p> <http://example.org/o> >> <http://example.org/p> <http://example.org/o> <http://example.org/g> .
                << <http://example.org/s> <http://example.org/p> <http://example.org/o> >> <http://example.org/p> <http://example.org/o2> <http://example.org/g> .
                << <http://example.org/s> <http://example.org/p> <http://example.org/o> >> <http://example.org/p2> <http://example.org/o> <http://example.org/g> .
                <http://example.org/s> <http://example.org/p> <http://example.org/o> <http://example.org/g> ."
        ),
        new(
            "Multiple Graphs",
            @"<< <http://example.org/s> <http://example.org/p> <http://example.org/o> >> <http://example.org/p> <http://example.org/o> <http://example.org/g1> .
                  <http://example.org/s> <http://example.org/p> << <http://example.org/s> <http://example.org/p> <http://example.org/o> >> <http://example.org/g2> .
                  << <http://example.org/s> <http://example.org/p> <http://example.org/o> >> <http://example.org/p> <http://example.org/o> <http://example.org/g3> .
                  <http://example.org/s> <http://example.org/p> <http://example.org/o> <http://example.org/g3> .
                  << <http://example.org/s> <http://example.org/p> <http://example.org/o> >> <http://example.org/p> <http://example.org/o> <http://example.org/g4> .
                  << <http://example.org/s> <http://example.org/p> <http://example.org/o> >> <http://example.org/p> <http://example.org/o2> <http://example.org/g4> .
                  << <http://example.org/s> <http://example.org/p> <http://example.org/o> >> <http://example.org/p2> <http://example.org/o> <http://example.org/g4> .
                  <http://example.org/s> <http://example.org/p> <http://example.org/o> <http://example.org/g4> ."
        )
    ];

    protected void RoundTrip(string input)
    {
        var store = new TripleStore();
        var stringWriter = new System.IO.StringWriter();
        IStoreWriter writer = GetWriter();
        IStoreReader reader = GetReader();

        store.LoadFromString(input, new NQuadsParser(NQuadsSyntax.Rdf11Star));
        writer.Save(store, stringWriter);

        var loadStore = new TripleStore();
        loadStore.LoadFromString(stringWriter.ToString(), reader);
        TestTools.AssertEqual(store, loadStore, Output);
    }
}

public class TriGMinimalCompressionWriterTests : RdfStarStoreWriterTests
{
    public TriGMinimalCompressionWriterTests(ITestOutputHelper output):base(output){}

    public override IStoreReader GetReader()
    {
        return new TriGParser(TriGSyntax.Rdf11Star);
    }

    public override IStoreWriter GetWriter()
    {
        return new TriGWriter()
        {
            CompressionLevel = WriterCompressionLevel.None, Syntax = TriGSyntax.Rdf11Star
        };
    }

    [Theory]
    [MemberData(nameof(RoundTripTestData))]
    public void TestRoundTrip(string _, string input)
    {
        RoundTrip(input);
    }

}

public class TriGThreadedMinimalCompressionWriterTests : RdfStarStoreWriterTests
{
    public TriGThreadedMinimalCompressionWriterTests(ITestOutputHelper output) : base(output) { }

    public override IStoreReader GetReader()
    {
        return new TriGParser(TriGSyntax.Rdf11Star);
    }

    public override IStoreWriter GetWriter()
    {
        return new TriGWriter()
        {
            CompressionLevel = WriterCompressionLevel.None,
            UseMultiThreadedWriting = true,
            Syntax = TriGSyntax.Rdf11Star
        };
    }

    [Theory]
    [MemberData(nameof(RoundTripTestData))]
    public void TestRoundTrip(string _, string input)
    {
        RoundTrip(input);
    }

}

public class TriGHighCompressionWriterTests : RdfStarStoreWriterTests
{
    public TriGHighCompressionWriterTests(ITestOutputHelper output) : base(output)
    {
    }

    public override IStoreWriter GetWriter()
    {
        return new TriGWriter
        {
            CompressionLevel = WriterCompressionLevel.High,
            HighSpeedModePermitted = false,
            Syntax = TriGSyntax.Rdf11Star,
        };
    }

    public override IStoreReader GetReader()
    {
        return new TriGParser(TriGSyntax.Rdf11Star);
    }

    [Theory]
    [MemberData(nameof(RoundTripTestData))]
    public void TestRoundTrip(string _, string input)
    {
        RoundTrip(input);
    }

}

public class TriGThreadedHighCompressionWriterTests : RdfStarStoreWriterTests
{
    public TriGThreadedHighCompressionWriterTests(ITestOutputHelper output) : base(output)
    {
    }

    public override IStoreWriter GetWriter()
    {
        return new TriGWriter
        {
            CompressionLevel = WriterCompressionLevel.High,
            UseMultiThreadedWriting = true,
            HighSpeedModePermitted = false,
            Syntax = TriGSyntax.Rdf11Star,
        };
    }

    public override IStoreReader GetReader()
    {
        return new TriGParser(TriGSyntax.Rdf11Star);
    }

    [Theory]
    [MemberData(nameof(RoundTripTestData))]
    public void TestRoundTrip(string _, string input)
    {
        RoundTrip(input);
    }

}
