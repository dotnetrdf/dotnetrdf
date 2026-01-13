using System;
using Xunit;
using FluentAssertions;
namespace VDS.RDF.Parsing.Handlers;

public class HandlerExtensionsTests
{
    [Fact]
    public void GetBaseUriFromCustomHandler()
    {
        var expected = new Uri("http://example.org/");
        var testHandler = new CustomHandler(expected);
        testHandler.GetBaseUri().Should().Be(expected);
    }

    [Fact]
    public void GetBaseUriFromWrappingHandler()
    {
        var expected = new Uri("http://example.org/");
        var innerHandler1 = new CustomHandler(null);
        var innerHandler2 = new CustomHandler(expected);
        var wrappingHandler = new ChainedHandler([innerHandler1, innerHandler2]);
        wrappingHandler.GetBaseUri().Should().Be(expected);
    }

    public class CustomHandler : BaseRdfHandler
    {
        public CustomHandler(Uri baseUri)
        {
            AcceptsAll = true;
            BaseUri = baseUri;
        }
        protected override bool HandleTripleInternal(Triple t)
        {
            throw new System.NotImplementedException();
        }

        protected override bool HandleQuadInternal(Triple t, IRefNode graph)
        {
            throw new System.NotImplementedException();
        }

        public override bool AcceptsAll { get; }
    }
}