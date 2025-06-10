using FluentAssertions;
using System.IO;
using Xunit;

namespace VDS.RDF.Parsing;

public class TurtleStarTests: NTriplesStarTests
{
    public override IRdfReader GetParser()
    {
        return new TurtleParser(TurtleSyntax.Rdf11Star, false);
    }

    [Theory]
    [InlineData("ex:s ex:p ex:o {| ex:a ex:b |} .", 2, 1)]
    public void TurtlePositiveSyntaxTests(string inputString, int expectAssertCount, int expectQuoteCount)
    {
        IRdfReader parser = GetParser();
        var g = new Graph();
        inputString = "@prefix ex: <http://example.org/> .\n" + inputString;
        parser.Load(g, new StringReader(inputString));
        g.Triples.Count.Should().Be(expectAssertCount);
        g.Triples.QuotedCount.Should().Be(expectQuoteCount);
    }

    [Fact]
    public void SingleTest()
    {
        var inputString = @"PREFIX :       <http://example/>
PREFIX xsd:     <http://www.w3.org/2001/XMLSchema#>

:s :p :o {| :source [ :graph <http://host1/> ;
                      :date ""2020-01-20""^^xsd:date
                    ] ;
            :source [ :graph <http://host2/> ;
                      :date ""2020-12-31""^^xsd:date
                    ]
          |} .
";
        IRdfReader parser = GetParser();
        var g = new Graph();
        parser.Load(g, new StringReader(inputString));
        g.Triples.Count.Should().Be(7);
        g.Triples.QuotedCount.Should().Be(1);
    }
}
