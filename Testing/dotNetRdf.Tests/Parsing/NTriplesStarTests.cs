using FluentAssertions;
using System.IO;
using Xunit;

namespace VDS.RDF.Parsing;

public class NTriplesStarTests
{
    public virtual IRdfReader GetParser()
    {
        return new NTriplesParser(NTriplesSyntax.Rdf11Star);
    }

    [Theory]
    [InlineData("<< <http://example.org/s> <http://example.org/p> <http://example.org/o> >> <http://example.org/p> <http://example.org/o> .", 1, 1)]
    [InlineData("<http://example.org/s> <http://example.org/p> << <http://example.org/s> <http://example.org/p> <http://example.org/o> >> .", 1, 1)]
    [InlineData("<< << <http://example.org/s> <http://example.org/p> <http://example.org/o> >> <http://example.org/p> <http://example.org/o> >> <http://example.org/p> <http://example.org/o> .", 1, 2)]
    public void NTriplesPositiveSyntaxTests(string inputString, int expectAssertCount, int expectQuoteCount)
    {
        IRdfReader parser = GetParser();
        var g = new Graph();
        parser.Load(g, new StringReader(inputString));
        g.Triples.Count.Should().Be(expectAssertCount);
        g.Triples.QuotedCount.Should().Be(expectQuoteCount);
    }
}
