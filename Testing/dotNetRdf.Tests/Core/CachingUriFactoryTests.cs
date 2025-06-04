using FluentAssertions;
using System;
using Xunit;

namespace VDS.RDF;

public class CachingUriFactoryTests
{
    [Fact]
    public void WhenInterningEnabledACachedValueIsReturned()
    {
        var factory = new CachingUriFactory(null);
        Uri uri1 = factory.Create("http://example.org/foo");
        Uri uri2 = factory.Create("http://example.org/foo");
        Uri uri3 = factory.Create("http://example.org/bar");
        Uri uri4 = factory.Create(uri1, "bar");

        uri2.Should().BeSameAs(uri1);
        uri4.Should().BeSameAs(uri3);
    }

    [Fact]
    public void WhenInterningDisabledNewValueIsReturned()
    {
        var factory = new CachingUriFactory(null) {InternUris = false};
        Uri uri1 = factory.Create("http://example.org/foo");
        Uri uri2 = factory.Create("http://example.org/foo");
        Uri uri3 = factory.Create("http://example.org/bar");
        Uri uri4 = factory.Create(uri1, "bar");

        uri2.Should().NotBeSameAs(uri1);
        uri4.Should().NotBeSameAs(uri3);
    }

    [Fact]
    public void WhenFactoryIsClearedNewValueIsReturned()
    {
        // Interning is enabled
        var factory = new CachingUriFactory(null);
        Uri uri1 = factory.Create("http://example.org/foo");
        Uri uri3 = factory.Create("http://example.org/bar");
        factory.Clear();

        // As the factory cache has been cleared, these calls should result in new URI instances
        Uri uri2 = factory.Create("http://example.org/foo");
        Uri uri4 = factory.Create(uri1, "bar");

        uri2.Should().NotBeSameAs(uri1);
        uri4.Should().NotBeSameAs(uri3);
    }

    [Fact]
    public void FactoryParentDefaultsToTheRootFactory()
    {
        Uri uri1 = UriFactory.Root.Create("http://example.org/foo");
        Uri uri3 = UriFactory.Root.Create("http://example.org/bar");

        var factory = new CachingUriFactory();

        // As the factory has the root factory as its parent, it should return interned values
        Uri uri2 = factory.Create("http://example.org/foo");
        Uri uri4 = factory.Create(uri1, "bar");

        uri2.Should().BeSameAs(uri1);
        uri4.Should().BeSameAs(uri3);
    }

    [Fact]
    public void ParentIsIgnoredWhenInterningIsDisabled()
    {
        Uri uri1 = UriFactory.Root.Create("http://example.org/foo");
        Uri uri3 = UriFactory.Root.Create("http://example.org/bar");

        var factory = new CachingUriFactory{InternUris = false};

        // As the factory has interning disabled, the parent factory (in this case the root factory) will not be consulted
        Uri uri2 = factory.Create("http://example.org/foo");
        Uri uri4 = factory.Create(uri1, "bar");

        uri2.Should().NotBeSameAs(uri1);
        uri4.Should().NotBeSameAs(uri3);
    }

    [Fact]
    public void ParentFactoriesCanBeChained()
    {
        var factory1 = new CachingUriFactory();
        var factory2 = new CachingUriFactory(factory1);

        Uri uri1 = UriFactory.Root.Create("http://example.org/foo");
        Uri uri2 = factory1.Create("http://example.org/bar");

        Uri uri3 = factory2.Create("http://example.org/foo");
        uri3.Should().BeSameAs(uri1);

        Uri uri4 = factory2.Create(uri3, "bar");
        uri4.Should().BeSameAs(uri2);
    }

    [Fact]
    public void InternedUriIsNotAccessibleToTheParent()
    {
        var factory1 = new CachingUriFactory(null);
        var factory2 = new CachingUriFactory(factory1);

        Uri uri1 = factory2.Create("http://example.org/foo");
        Uri uri2 = factory1.Create("http://example.org/foo");

        // Although the URI values are the same, because factory1 is the parent of factory2 it doesn't have the URI interned.
        uri2.Should().NotBeSameAs(uri1);
    }
}
