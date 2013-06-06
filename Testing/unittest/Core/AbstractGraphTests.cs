using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using VDS.RDF.Parsing;

namespace VDS.RDF
{
    /// <summary>
    /// Abstract set of tests that can be used to test any <see cref="IGraph" /> implementation
    /// </summary>
    [TestFixture]
    public abstract class AbstractGraphTests
    {
        /// <summary>
        /// Method which derived tests should implement to provide a fresh instance that can be used for testing
        /// </summary>
        /// <returns></returns>
        protected abstract IGraph GetInstance();

        [Test]
        public void GraphIsEmpty01()
        {
            IGraph g = this.GetInstance();
            Assert.IsTrue(g.IsEmpty);
        }

        [Test]
        public void GraphIsEmpty02()
        {
            IGraph g = this.GetInstance();

            g.Assert(new Triple(g.CreateBlankNode(), g.CreateBlankNode(), g.CreateBlankNode()));
            Assert.IsFalse(g.IsEmpty);
        }

        [Test]
        public void GraphAssert01()
        {
            IGraph g = this.GetInstance();
            g.NamespaceMap.AddNamespace(String.Empty, UriFactory.Create("http://example/"));

            Triple t = new Triple(g.CreateUriNode(":s"), g.CreateUriNode(":p"), g.CreateBlankNode(":o"));
            g.Assert(t);
            Assert.IsFalse(g.IsEmpty);
            Assert.IsTrue(g.ContainsTriple(t));
        }

        [Test]
        public void GraphRetract01()
        {
            IGraph g = this.GetInstance();
            g.NamespaceMap.AddNamespace(String.Empty, UriFactory.Create("http://example/"));

            Triple t = new Triple(g.CreateUriNode(":s"), g.CreateUriNode(":p"), g.CreateBlankNode(":o"));
            g.Assert(t);
            Assert.IsFalse(g.IsEmpty);
            Assert.IsTrue(g.ContainsTriple(t));

            g.Retract(t);
            Assert.IsTrue(g.IsEmpty);
            Assert.IsFalse(g.ContainsTriple(t));
        }

        [Test]
        public void GraphRetract02()
        {
            IGraph g = this.GetInstance();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            Assert.IsFalse(g.IsEmpty);

            INode rdfType = g.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
            Assert.IsTrue(g.GetTriplesWithPredicate(rdfType).Any());

            g.Retract(g.GetTriplesWithPredicate(rdfType).ToList());
            Assert.IsFalse(g.GetTriplesWithPredicate(rdfType).Any());
        }
    }

    [TestFixture]
    public class GraphTests
        : AbstractGraphTests
    {
        protected override IGraph GetInstance()
        {
            return new Graph();
        }
    }

    [TestFixture]
    public class ThreadSafeGraphTests
        : AbstractGraphTests
    {
        protected override IGraph GetInstance()
        {
            return new ThreadSafeGraph();
        }
    }

    [TestFixture]
    public class NonIndexedGraphTests
        : AbstractGraphTests
    {
        protected override IGraph GetInstance()
        {
            return new NonIndexedGraph();
        }
    }
}
