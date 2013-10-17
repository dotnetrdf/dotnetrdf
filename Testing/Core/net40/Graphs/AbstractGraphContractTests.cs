using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using VDS.RDF.Nodes;

namespace VDS.RDF.Graphs
{
    /// <summary>
    /// Abstract tests for <see cref="IGraph"/> implementations
    /// </summary>
    [TestFixture]
    public abstract class AbstractGraphContractTests
    {
        /// <summary>
        /// Gets a new fresh instance of a graph for testing
        /// </summary>
        /// <returns></returns>
        protected abstract IGraph GetInstance();

        protected IEnumerable<Triple> GenerateTriples(int n)
        {
            for (int i = 0; i < n; i++)
            {
                yield return new Triple(new UriNode(new Uri("http://test/" + i)), new UriNode(new Uri("http://predicate")), new LiteralNode(i.ToString()));
            }
        }

        [Test]
        public void GraphContractCount1()
        {
            IGraph g = this.GetInstance();
            Assert.AreEqual(0, g.Count);
        }

        [Test]
        public void GraphContractCount2()
        {
            IGraph g = this.GetInstance();
            g.Assert(this.GenerateTriples(1));
            Assert.AreEqual(1, g.Count);
        }

        [Test]
        public void GraphContractCount3()
        {
            IGraph g = this.GetInstance();
            g.Assert(this.GenerateTriples(100));
            Assert.AreEqual(100, g.Count);
        }

        [Test]
        public void GraphContractIsEmpty1()
        {
            IGraph g = this.GetInstance();
            Assert.IsTrue(g.IsEmpty);
        }

        [Test]
        public void GraphContractIsEmpty2()
        {
            IGraph g = this.GetInstance();
            g.Assert(this.GenerateTriples(1));
            Assert.IsFalse(g.IsEmpty);
        }

        [Test]
        public void GraphContractNamespaces1()
        {
            IGraph g = this.GetInstance();
            Assert.IsNotNull(g.Namespaces);
        }

        [Test]
        public void GraphContractNamespaces2()
        {
            IGraph g = this.GetInstance();
            Assert.IsNotNull(g.Namespaces);
            g.Namespaces.AddNamespace("ex", new Uri("http://example.org"));
            Assert.IsTrue(g.Namespaces.HasNamespace("ex"));
            Assert.AreEqual(new Uri("http://example.org"), g.Namespaces.GetNamespaceUri("ex"));
        }

        [Test]
        public void GraphContractAssert1()
        {
            IGraph g = this.GetInstance();
            Assert.AreEqual(0, g.Count);
            Assert.IsFalse(g.Triples.Any());

            // Assert the triple
            Triple t = new Triple(g.CreateUriNode(new Uri("http://subject")), g.CreateUriNode(new Uri("http://predicate")), g.CreateBlankNode());
            Assert.IsTrue(g.Assert(t));
            Assert.AreEqual(1, g.Count);
            Assert.IsTrue(g.ContainsTriple(t));
            Assert.IsTrue(g.Triples.Any());

            // Asserting same triple again should have no effect
            Assert.IsFalse(g.Assert(t));
            Assert.AreEqual(1, g.Count);
            Assert.IsTrue(g.ContainsTriple(t));
            Assert.IsTrue(g.Triples.Any());
        }

        [Test]
        public void GraphContractRetract1()
        {
            IGraph g = this.GetInstance();
            Assert.AreEqual(0, g.Count);
            Assert.IsFalse(g.Triples.Any());

            // Assert the triple
            Triple t = new Triple(g.CreateUriNode(new Uri("http://subject")), g.CreateUriNode(new Uri("http://predicate")), g.CreateBlankNode());
            Assert.IsTrue(g.Assert(t));
            Assert.AreEqual(1, g.Count);
            Assert.IsTrue(g.ContainsTriple(t));
            Assert.IsTrue(g.Triples.Any());

            // Retract the triple
            Assert.IsTrue(g.Retract(t));
            Assert.AreEqual(0, g.Count);
            Assert.IsFalse(g.ContainsTriple(t));
            Assert.IsFalse(g.Triples.Any());
        }

        [Test]
        public void GraphContractTriples1()
        {
            IGraph g = this.GetInstance();
            Assert.AreEqual(0, g.Count);
            Assert.IsFalse(g.Triples.Any());

            // Assert the triple
            Triple t = new Triple(g.CreateUriNode(new Uri("http://subject")), g.CreateUriNode(new Uri("http://predicate")), g.CreateBlankNode());
            Assert.IsTrue(g.Assert(t));
            Assert.AreEqual(1, g.Count);
            Assert.IsTrue(g.ContainsTriple(t));
            Assert.IsTrue(g.Triples.Any());

            IEnumerable<Triple> ts = g.Triples;
            Assert.IsTrue(ts.Any());
            Assert.AreEqual(1, ts.Count());
            Assert.IsTrue(ts.Contains(t));

            // Retract the triple
            Assert.IsTrue(g.Retract(t));
            Assert.AreEqual(0, g.Count);
            Assert.IsFalse(g.ContainsTriple(t));
            Assert.IsFalse(g.Triples.Any());

            // Enumerable should reflect current state of graph
            Assert.IsFalse(ts.Any());
            Assert.AreEqual(0, ts.Count());
            Assert.IsFalse(ts.Contains(t));
        }

        [Test]
        public void GraphContractFind1()
        {
            IGraph g = this.GetInstance();
            Assert.AreEqual(0, g.Count);
            Assert.IsTrue(g.IsEmpty);

            INode s1 = g.CreateUriNode(new Uri("http://s1"));
            INode s2 = g.CreateUriNode(new Uri("http://s2"));
            INode p = g.CreateUriNode(new Uri("http://p"));
            INode o1 = g.CreateLiteralNode("value");
            INode o2 = g.CreateUriNode(new Uri("http://o"));

            Triple t1 = new Triple(s1, p, o1);
            g.Assert(t1);
            Triple t2 = new Triple(s1, p, o2);
            g.Assert(t2);
            Triple t3 = new Triple(s2, p, o2);
            g.Assert(t3);
            Assert.AreEqual(3, g.Count);

            // Find by subject
            List<Triple> ts = g.Find(s1, null, null).ToList();
            Assert.AreEqual(2, ts.Count);
            Assert.IsTrue(ts.Contains(t1));
            Assert.IsTrue(ts.Contains(t2));

            // Find by predicate
            ts = g.Find(null, p, null).ToList();
            Assert.AreEqual(3, ts.Count);

            // Find by subject and object
            ts = g.Find(s2, null, o2).ToList();
            Assert.AreEqual(1, ts.Count);
            Assert.IsTrue(ts.Contains(t3));

            // Find everything
            ts = g.Find(null, null, null).ToList();
            Assert.AreEqual(3, ts.Count);

            // Find nothing
            ts = g.Find(g.CreateUriNode(new Uri("http://s3")), null, null).ToList();
            Assert.AreEqual(0, ts.Count);
        }

        [Test]
        public void GraphContractFind2()
        {
            IGraph g = this.GetInstance();
            Assert.AreEqual(0, g.Count);
            Assert.IsTrue(g.IsEmpty);

            INode s1 = g.CreateBlankNode();
            INode s2 = g.CreateBlankNode();
            INode p = g.CreateUriNode(new Uri("http://p"));
            INode o1 = g.CreateLiteralNode("value");
            INode o2 = g.CreateUriNode(new Uri("http://o"));

            Triple t1 = new Triple(s1, p, o1);
            g.Assert(t1);
            Triple t2 = new Triple(s1, p, o2);
            g.Assert(t2);
            Triple t3 = new Triple(s2, p, o2);
            g.Assert(t3);
            Assert.AreEqual(3, g.Count);

            // Find by subject
            List<Triple> ts = g.Find(s1, null, null).ToList();
            Assert.AreEqual(2, ts.Count);
            Assert.IsTrue(ts.Contains(t1));
            Assert.IsTrue(ts.Contains(t2));

            // Find by predicate
            ts = g.Find(null, p, null).ToList();
            Assert.AreEqual(3, ts.Count);

            // Find by subject and object
            ts = g.Find(s2, null, o2).ToList();
            Assert.AreEqual(1, ts.Count);
            Assert.IsTrue(ts.Contains(t3));

            // Find everything
            ts = g.Find(null, null, null).ToList();
            Assert.AreEqual(3, ts.Count);

            // Find nothing
            ts = g.Find(g.CreateBlankNode(), null, null).ToList();
            Assert.AreEqual(0, ts.Count);
        }
    }

    [TestFixture]
    public class GraphContractTests
        : AbstractGraphContractTests
    {
        protected override IGraph GetInstance()
        {
            return new Graph();
        }
    }

    [TestFixture]
    public class ThreadSafeGraphContractTests
        : AbstractGraphContractTests
    {
        protected override IGraph  GetInstance()
{
 	return new ThreadSafeGraph();
}
    }

    [TestFixture]
    public class WrapperGraphContractTests
        : AbstractGraphContractTests
    {

        protected override IGraph GetInstance()
        {
            return new TestWrapperGraph();
        }

        private class TestWrapperGraph
            : WrapperGraph
        {
            
        }
    }
}
