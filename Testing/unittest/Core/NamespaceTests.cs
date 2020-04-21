using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
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
        public void ExpansionCanBeExplicitlyRetrievedAsAString()
        {
            dynamic ns = new Namespace("http://example.com/");
            Assert.Equal("http://example.com/foo", (string)ns.foo.AsString());
        }

        [Fact]
        public void ExpansionCanBeExplicitlyRetrievedAsUri()
        {
            dynamic ns = new Namespace("http://example.com/");
            Assert.Equal(new Uri("http://example.com/foo"), ns.foo.AsUri());
        }
        [Fact]
        public void ExpansionCanBeCastToAString()
        {
            dynamic ns = new Namespace("http://example.com/");
            Assert.Equal("http://example.com/foo", (string)ns.foo);
        }

        [Fact]
        public void ExpansionCanBeCaseToAUri()
        {
            dynamic ns = new Namespace("http://example.com/");
            Assert.Equal(new Uri("http://example.com/foo"), (Uri)ns.foo);
        }

        [Fact]
        public void ExpansionCanBeImplicitlyCastToAUri()
        {
            dynamic ns = new Namespace("http://example.com/");
            Assert.True(MethodAcceptingUriArgument(ns.foo));
        }

        [Fact]
        public void ExpansionCanBeImplicitlyCastToAString()
        {
            dynamic ns = new Namespace("http://example.com/");
            Assert.True(MethodAcceptingStringArgument(ns.foo));
        }

        [Fact]
        public void ItProvidesRdfNamespace()
        {
            Assert.Equal("http://www.w3.org/1999/02/22-rdf-syntax-ns#type", Namespace.Rdf.type);
        }

        [Fact]
        public void CanUseIndexingExpression()
        {
            Assert.Equal("http://www.w3.org/1999/02/22-rdf-syntax-ns#123-456", Namespace.Rdf["123-456"]);
        }

        private bool MethodAcceptingUriArgument(Uri u)
        {
            return u != null;
        }

        private bool MethodAcceptingStringArgument(string s)
        {
            return s != null;
        }
    }
}
