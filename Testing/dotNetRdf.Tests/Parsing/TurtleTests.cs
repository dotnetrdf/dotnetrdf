using FluentAssertions;
using System.IO;
using Xunit;

namespace VDS.RDF.Parsing
{
    public class TurtleTests
    {
        [Fact]
        public void ItCanParseALiteralValueInABlankNode()
        {
            // Reproduce error reported in #525
            const string input = "@prefix ex: <http://example.org>. _:x ex:value [ex:value \"\"] .";
            var reader = new TurtleParser();
            var g = new Graph();
            reader.Load(g, new StringReader(input));
            g.Triples.Count.Should().Be(2);
        }

        [Fact]
        public void ItCanParseALiteralValueInAList()
        {
            // Reproduce error reported in #525
            const string input = "@prefix ex: <http://example.org>. _:x ex:value (\"a\" \"b\") .";
            var reader = new TurtleParser();
            var g = new Graph();
            reader.Load(g, new StringReader(input));
            g.Triples.Count.Should().Be(5);

        }
    }
}
