using System;
using Xunit;

namespace VDS.RDF.Core
{
    public class NamespaceTests
    {
        [Fact]
        public void CanCreateCustomNamespace()
        {
            dynamic ns = new Namespace("http://example.com/");
        }

        [Fact]
        public void BaseUriMustBeNonNull()
        {
            Assert.Throws<ArgumentNullException>("baseUri", () => new Namespace(null));
        }

        [Fact]
        public void BaseUriMustBeNonEmpty()
        {
            Assert.Throws<ArgumentException>("baseUri", () => new Namespace(string.Empty));
        }

        [Fact]
        public void ItProvidesRdfNamespace()
        {
            Assert.Equal("http://www.w3.org/1999/02/22-rdf-syntax-ns#type", Namespace.Rdf["type"]);
        }

        [Fact]
        public void CanUseIndexingExpression()
        {
            Assert.Equal("http://www.w3.org/1999/02/22-rdf-syntax-ns#123-456", Namespace.Rdf["123-456"]);
        }

    }
}
