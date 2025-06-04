using System;
using FluentAssertions;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Formatting;
using Xunit;

namespace VDS.RDF.Writing;

public class NTriplesFormatterTests
{
    [Fact]
    public void SpacesInIrisMustBeEscaped()
    {
        var formatter = new NTriplesFormatter(NTriplesSyntax.Rdf11);
        var formatOutput = formatter.FormatUri(new Uri("http://example.org/foo bar"));
        formatOutput.Should().Be("http://example.org/foo%20bar");
    }

    [Fact]
    public void EscapedSpacesInIrisMustNotBeDoubleEscaped()
    {
        var formatter = new NTriplesFormatter(NTriplesSyntax.Rdf11);
        var formatOutput = formatter.FormatUri(new Uri("http://example.org/foo%20bar"));
        formatOutput.Should().Be("http://example.org/foo%20bar");
    }

    [Fact]
    public void EscapedSpacesInIrisMustNotBeDoubleEscaped2()
    {
        var formatter = new NTriplesFormatter(NTriplesSyntax.Rdf11);
        var formatOutput = formatter.FormatUri(new Uri("http://example.org/foo+bar"));
        formatOutput.Should().Be("http://example.org/foo+bar");
    }

    [Fact]
    public void NonAsciiCharactersMustNotBeEscaped()
    {
        var formatter = new NTriplesFormatter(NTriplesSyntax.Rdf11);
        var formatOutput = formatter.FormatUri(new Uri("http://example.org/渋谷駅"));
        formatOutput.Should().Be("http://example.org/渋谷駅");
    }

    [Fact]
    public void EscapeSequenceInOriginalStringMustNotBeDoubleEscaped()
    {
        var formatter = new NTriplesFormatter(NTriplesSyntax.Rdf11);
        var formatOutput = formatter.FormatUri(new Uri("http://example.org/September%2C 2020"));
        formatOutput.Should().Be("http://example.org/September%2C%202020");
    }

    [Theory]
    [InlineData("http://example.org/September%2", "http://example.org/September%252")]
    [InlineData("http://example.org/September%", "http://example.org/September%25")]
    public void IncompleteEscapeSequenceGetsEscaped(string input, string expected)
    {
        var formatter = new NTriplesFormatter(NTriplesSyntax.Rdf11);
        var formatOutput = formatter.FormatUri(new Uri(input));
        formatOutput.Should().Be(expected);
    }

    [Fact(Skip="Fails because the .NET URI constructor always unescapes %66 to f")]
    public void PercentCharactersArePreservedInUriFormatting()
    {
        var formatter = new NTriplesFormatter(NTriplesSyntax.Rdf11);
        var u = new Uri("http://a.example/%66oo-bar");
        var formatOutput = formatter.FormatUri(u);
        formatOutput.Should().Be("http://a.example/%66oo-bar");
    }
}
