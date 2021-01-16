using FluentAssertions;
using System;
using Xunit;

namespace VDS.RDF
{
    public class CachingUriFactoryTests
    {
        [Fact]
        public void WhenInterningEnabledACachedValueIsReturned()
        {
            var factory = new CachingUriFactory();
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
            var factory = new CachingUriFactory {InternUris = false};
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
            var factory = new CachingUriFactory();
            Uri uri1 = factory.Create("http://example.org/foo");
            Uri uri3 = factory.Create("http://example.org/bar");
            factory.Clear();

            // As the factory cache has been cleared, these calls should result in new URI instances
            Uri uri2 = factory.Create("http://example.org/foo");
            Uri uri4 = factory.Create(uri1, "bar");

            uri2.Should().NotBeSameAs(uri1);
            uri4.Should().NotBeSameAs(uri3);
        }
    }
}
