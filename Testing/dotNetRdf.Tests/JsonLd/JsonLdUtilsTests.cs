using FluentAssertions;
using VDS.RDF.JsonLd.Processors;
using Xunit;

namespace VDS.RDF.JsonLd;

public class JsonLdUtilsTests
{
    [Theory]
    [InlineData("http://example.org/", true)]
    [InlineData("http://example.org/a/b", true)]
    [InlineData("http://example.org/a/b#c", true)]
    [InlineData("http://example.org/a/b#c:d", true)]
    [InlineData("http://example.org/a/b?c=d", true)]
    [InlineData("/", true)]
    [InlineData("/a/b", true)]
    [InlineData("/a/b#c", true)]
    [InlineData("/a/b?c=d", true)]
    [InlineData("a/b", true)]
    [InlineData("a/b#c", true)]
    [InlineData("a/b#c:d", true)]
    [InlineData("a/b?c=d", true)]
    [InlineData("http://example.org/http%3A%2F%2Flocalhost%3A8080%2Fknowledge-graph%2Fengine_01", true)]
    [InlineData("foo#bar", true)]
    [InlineData("foo%bar", true)] // %ba is a valid escape code
    [InlineData("bar%foo", false)] // %fo is not a valid escape code
    [InlineData("_:foo", false)]
    [InlineData("./rel#", true)]
    [InlineData("#f", false)]
    [InlineData("http:/example.org", true)]
    public void ItValidatesIriStrings(string iriString, bool expectValid)
    {
        JsonLdUtils.IsIri(iriString).Should().Be(expectValid);
    }
}
