using Xunit;

namespace VDS.RDF.JsonLd;

public class WebLinkTests
{
    [Fact]
    public void ParseSimpleLinkHeader()
    {
        var isValid = WebLink.TryParse("<http://example.org/foo.json>", out var link);
        Assert.True(isValid);
        Assert.Equal("http://example.org/foo.json", link.LinkValue);
    }

    [Fact]
    public void ItCanParseTheRelField()
    {
        var isValid = WebLink.TryParse("<http://example.org/foo.json>;rel=rel1", out var link);
        Assert.True(isValid);
        Assert.Equal(new [] {"rel1"}, link.RelationTypes);
    }

    [Fact]
    public void ItCanParseMultipleValuesFromTheRelField()
    {
        var isValid = WebLink.TryParse("<http://example.org/foo.json>;rel=\"rel1 rel2\"", out var link);
        Assert.True(isValid);
        Assert.Equal(new[] { "rel1", "rel2" }, link.RelationTypes);
    }

    [Fact]
    public void ItCanParseTheTypeField()
    {
        var isValid = WebLink.TryParse("<http://example.org/foo.json>;type=application/ld+json", out var link);
        Assert.True(isValid);
        Assert.Equal(new[] { "application/ld+json" }, link.MediaTypes);
    }

    [Fact]
    public void ItCanParseAQuotedMediaType()
    {
        var isValid = WebLink.TryParse("<http://example.org/foo.json>;type=\"application/ld+json\"", out var link);
        Assert.True(isValid);
        Assert.Equal(new[] { "application/ld+json" }, link.MediaTypes);
    }

    [Fact]
    public void ItCanParseMultipleFields()
    {
        var isValid = WebLink.TryParse("<http://example.org/foo.json>;type=\"application/ld+json\";rel=alternate",
            out var link);
        Assert.True(isValid);
        Assert.Equal(new[] { "application/ld+json" }, link.MediaTypes);
        Assert.Equal(new string[] {"alternate"}, link.RelationTypes);
        Assert.Equal("http://example.org/foo.json", link.LinkValue);
    }
}
