using FluentAssertions;
using System.IO;
using VDS.RDF.Parsing;
using Xunit;

namespace VDS.RDF.Query;

public class QNameDigitsSupportTests
{
    [Fact]
    public void QName__ShouldAllow__LeadingDigitInLocalName()
    {
        var trigParser = new TriGParser();
        var tStore = new TripleStore();
        var loadQnameWithLeadingDigit = Record.Exception(() =>
            trigParser.Load(
                tStore, 
                new StringReader("@prefix exdata: <https://example.com/data/>. exdata:test1 { exdata:1test a <http://example.com/Thing> .}")
            )
        );

        loadQnameWithLeadingDigit.Should().BeNull();
    }
}

