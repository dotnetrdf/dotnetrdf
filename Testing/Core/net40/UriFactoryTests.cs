using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using VDS.RDF.Namespaces;

namespace VDS.RDF
{
    [TestFixture]
    public class UriFactoryTests
    {
        private void TestUriResolution(String uri, Uri baseUri, String expected, bool expectAbsolute)
        {
            Uri u = UriFactory.ResolveUri(uri, baseUri);
            if (ReferenceEquals(expected, null)) Assert.Fail("Expected URI resolution to fail");
            Assert.AreEqual(expected, expectAbsolute ? u.AbsoluteUri : u.ToString());
        }

        private void TestPrefixedNameResolution(string prefixedName, INamespaceMapper nsmap, Uri baseUri, string expected)
        {
            TestPrefixedNameResolution(prefixedName, nsmap, baseUri, false, expected);
        }

        private void TestPrefixedNameResolution(string prefixedName, INamespaceMapper nsmap, Uri baseUri, bool allowDefaultPrefixFallback, string expected)
        {
            Uri u = UriFactory.ResolvePrefixedName(prefixedName, nsmap, baseUri, allowDefaultPrefixFallback);
            if (ReferenceEquals(expected, null)) Assert.Fail("Expected URI resolution to fail");
            Assert.AreEqual(expected, u.AbsoluteUri);
        }

        [Test]
        public void UriResolution1()
        {
            this.TestUriResolution("file.ext", new Uri("http://example.org"), "http://example.org/file.ext", true);
        }

        [Test]
        public void UriResolution2()
        {
            this.TestUriResolution("file.ext", new Uri("http://example.org"), "http://example.org/file.ext", true);
        }

        [Test]
        public void UriResolution3()
        {
            this.TestUriResolution("file.ext", null, "file.ext", false);
        }

        [Test]
        public void UriResolution4()
        {
            this.TestUriResolution("file.ext", new Uri("/path", UriKind.Relative), "file.ext", false);
        }

        [Test, ExpectedException(typeof(RdfException))]
        public void UriResolutionBad1()
        {
            this.TestUriResolution("file:/file.ext", new Uri("http://example.org"), null, false);
        }

        [Test, ExpectedException(typeof(RdfException))]
        public void UriResolutionBad2()
        {
            this.TestUriResolution(null, new Uri("http://example.org"), null, false);
        }

        [Test]
        public void PrefixedNameResolution1()
        {
            INamespaceMapper nsmap = new NamespaceMapper();
            nsmap.AddNamespace("ex", new Uri("http://example.org"));

            // Namespace resolution
            this.TestPrefixedNameResolution("ex:test", nsmap, null, "http://example.org/test");
        }

        [Test]
        public void PrefixedNameResolution2()
        {
            INamespaceMapper nsmap = new NamespaceMapper();
            nsmap.AddNamespace(String.Empty, new Uri("http://example.org"));

            // Default namespace resolution
            this.TestPrefixedNameResolution(":test", nsmap, null, "http://example.org/test");
        }

        [Test]
        public void PrefixedNameResolution3()
        {
            INamespaceMapper nsmap = new NamespaceMapper();

            // Namespace not in scope and default namespace fallback possible
            this.TestPrefixedNameResolution(":test", nsmap, new Uri("http://example.org"), true, "http://example.org/#test");
        }

        [Test]
        public void PrefixedNameResolution4()
        {
            INamespaceMapper nsmap = new NamespaceMapper();

            // Namespace not in scope and default namespace fallback possible
            this.TestPrefixedNameResolution(":test", nsmap, new Uri("http://example.org/ns#"), true, "http://example.org/ns#test");
        }

        [Test, ExpectedException(typeof(RdfException))]
        public void PrefixedNameResolutionBad1()
        {
            INamespaceMapper nsmap = new NamespaceMapper();

            // Namespace not in scope
            this.TestPrefixedNameResolution("ex:test", nsmap, null, null);
        }

        [Test, ExpectedException(typeof(RdfException))]
        public void PrefixedNameResolutionBad2()
        {
            INamespaceMapper nsmap = new NamespaceMapper();

            // Namespace not in scope and default namespace fallback not applicable
            this.TestPrefixedNameResolution("ex:test", nsmap, new Uri("http://example.org"), null);
        }

        [Test, ExpectedException(typeof(RdfException))]
        public void PrefixedNameResolutionBad3()
        {
            INamespaceMapper nsmap = new NamespaceMapper();

            // Namespace not in scope and default namespace fallback not allowed
            this.TestPrefixedNameResolution(":test", nsmap, new Uri("http://example.org"), null);
        }

        [Test, ExpectedException(typeof(RdfException))]
        public void PrefixedNameResolutionBad4()
        {
            INamespaceMapper nsmap = new NamespaceMapper();

            // Namespace not in scope and default namespace fallback not possible due to relative base URI
            this.TestPrefixedNameResolution(":test", nsmap, new Uri("file.ext", UriKind.Relative), true, null);
        }

        [Test, ExpectedException(typeof(RdfException))]
        public void PrefixedNameResolutionBad5()
        {
            INamespaceMapper nsmap = new NamespaceMapper();

            // Namespace not in scope and default namespace fallback not possible due to null base URI
            this.TestPrefixedNameResolution(":test", nsmap, null, true, null);
        }
    }
}
