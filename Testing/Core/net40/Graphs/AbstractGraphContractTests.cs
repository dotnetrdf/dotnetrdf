using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using NUnit.Framework;
using VDS.RDF.Collections;
using VDS.RDF.Nodes;

namespace VDS.RDF.Graphs
{
    /// <summary>
    /// Abstract tests for <see cref="IGraph"/> implementations
    /// </summary>
    [TestFixture]
    public abstract class AbstractGraphContractTests
        : BaseTest
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
            g.Assert(t);
            Assert.AreEqual(1, g.Count);
            Assert.IsTrue(g.ContainsTriple(t));
            Assert.IsTrue(g.Triples.Any());

            // Asserting same triple again should have no effect
            g.Assert(t);
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
            g.Assert(t);
            Assert.AreEqual(1, g.Count);
            Assert.IsTrue(g.ContainsTriple(t));
            Assert.IsTrue(g.Triples.Any());

            // Retract the triple
            g.Retract(t);
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
            g.Assert(t);
            Assert.AreEqual(1, g.Count);
            Assert.IsTrue(g.ContainsTriple(t));
            Assert.IsTrue(g.Triples.Any());

            IEnumerable<Triple> ts = g.Triples;
            Assert.IsTrue(ts.Any());
            Assert.AreEqual(1, ts.Count());
            Assert.IsTrue(ts.Contains(t));

            // Retract the triple
            g.Retract(t);
            Assert.AreEqual(0, g.Count);
            Assert.IsFalse(g.ContainsTriple(t));
            Assert.IsFalse(g.Triples.Any());

            // Enumerable should reflect current state of graph
            Assert.IsFalse(ts.Any());
            Assert.AreEqual(0, ts.Count());
            Assert.IsFalse(ts.Contains(t));
        }

        [Test]
        public void GraphContractQuads1()
        {
            IGraph g = this.GetInstance();
            Assert.AreEqual(0, g.Count);
            Assert.IsFalse(g.Quads.Any());

            // Assert the triple
            Triple t = new Triple(g.CreateUriNode(new Uri("http://subject")), g.CreateUriNode(new Uri("http://predicate")), g.CreateBlankNode());
            g.Assert(t);
            Assert.AreEqual(1, g.Count);
            Assert.IsTrue(g.ContainsTriple(t));
            Assert.IsTrue(g.Triples.Any());

            IEnumerable<Quad> qs = g.Quads;
            Assert.IsTrue(qs.Any());
            Assert.AreEqual(1, qs.Count());
            Assert.IsTrue(qs.Contains(t.AsQuad(Quad.DefaultGraphNode)));

            // Retract the triple
            g.Retract(t);
            Assert.AreEqual(0, g.Count);
            Assert.IsFalse(g.ContainsTriple(t));
            Assert.IsFalse(g.Triples.Any());

            // Enumerable should reflect current state of graph
            Assert.IsFalse(qs.Any());
            Assert.AreEqual(0, qs.Count());
            Assert.IsFalse(qs.Contains(t.AsQuad(Quad.DefaultGraphNode)));
        }

        /// <summary>
        /// Performs some basic checks on the Find() implementation of a given graph
        /// </summary>
        /// <param name="g">Graph</param>
        protected static void FindBasicChecks(IGraph g)
        {
            // Find everything
            List<Triple> ts = g.Find(null, null, null).ToList();
            Assert.AreEqual(g.Count, ts.Count);

            // Find nothing
            ts = g.Find(g.CreateUriNode(new Uri("http://nosuchthing")), null, null).ToList();
            Assert.AreEqual(0, ts.Count);

            // Find each specific triple
            foreach (Triple t in g.Triples)
            {
                ts = g.Find(t.Subject, t.Predicate, t.Object).ToList();
                Assert.AreEqual(1, ts.Count);
            }
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

            // Perform basic checks
            FindBasicChecks(g);

            // Find by subject
            List<Triple> ts = g.Find(s1, null, null).ToList();
            Assert.AreEqual(2, ts.Count);
            Assert.IsTrue(ts.Contains(t1));
            Assert.IsTrue(ts.Contains(t2));

            // Find by predicate
            ts = g.Find(null, p, null).ToList();
            Assert.AreEqual(3, ts.Count);

            // Find by object
            ts = g.Find(null, null, o1).ToList();
            Assert.AreEqual(1, ts.Count);
            Assert.IsTrue(ts.Contains(t1));

            // Find by subject and object
            ts = g.Find(s2, null, o2).ToList();
            Assert.AreEqual(1, ts.Count);
            Assert.IsTrue(ts.Contains(t3));
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

            // Perform basic checks
            FindBasicChecks(g);

            // Find by subject
            List<Triple> ts = g.Find(s1, null, null).ToList();
            Assert.AreEqual(2, ts.Count);
            Assert.IsTrue(ts.Contains(t1));
            Assert.IsTrue(ts.Contains(t2));

            // Find by predicate
            ts = g.Find(null, p, null).ToList();
            Assert.AreEqual(3, ts.Count);

            // Find by object
            ts = g.Find(null, null, o2).ToList();
            Assert.AreEqual(2, ts.Count);
            Assert.IsTrue(ts.Contains(t2));
            Assert.IsTrue(ts.Contains(t3));

            // Find by subject and object
            ts = g.Find(s2, null, o2).ToList();
            Assert.AreEqual(1, ts.Count);
            Assert.IsTrue(ts.Contains(t3));

            ts = g.Find(s1, null, o1).ToList();
            Assert.AreEqual(1, ts.Count);
            Assert.IsTrue(ts.Contains(t1));

            ts = g.Find(s1, null, o2).ToList();
            Assert.AreEqual(1, ts.Count);
            Assert.IsTrue(ts.Contains(t2));

            ts = g.Find(s2, null, o1).ToList();
            Assert.AreEqual(0, ts.Count);
        }

        [Test]
        public void GraphContractFind3()
        {
            IGraph g = this.GetInstance();
            Assert.AreEqual(0, g.Count);
            Assert.IsTrue(g.IsEmpty);

            INode s = g.CreateBlankNode();
            INode s2 = g.CreateBlankNode();
            INode p1 = g.CreateUriNode(new Uri("http://p1"));
            INode p2 = g.CreateUriNode(new Uri("http://p2"));
            INode o = g.CreateLiteralNode("value");
            INode o2 = g.CreateUriNode(new Uri("http://o"));

            Triple t1 = new Triple(s, p1, o);
            g.Assert(t1);
            Triple t2 = new Triple(s2, p1, o);
            g.Assert(t2);
            Triple t3 = new Triple(s, p2, o2);
            g.Assert(t3);
            Assert.AreEqual(3, g.Count);

            // Perform basic checks
            FindBasicChecks(g);

            // Find by subject
            List<Triple> ts = g.Find(s, null, null).ToList();
            Assert.AreEqual(2, ts.Count);
            Assert.IsTrue(ts.Contains(t1));
            Assert.IsTrue(ts.Contains(t3));

            // Find by predicate
            ts = g.Find(null, p1, null).ToList();
            Assert.AreEqual(2, ts.Count);

            // Find by predicate and object
            ts = g.Find(null, p1, o).ToList();
            Assert.AreEqual(2, ts.Count);
            Assert.IsTrue(ts.Contains(t1));
            Assert.IsTrue(ts.Contains(t2));

            ts = g.Find(null, p2, o2).ToList();
            Assert.AreEqual(1, ts.Count);
            Assert.IsTrue(ts.Contains(t3));

            ts = g.Find(null, p2, o).ToList();
            Assert.AreEqual(0, ts.Count);
        }

        [Test]
        public void GraphContractStructure1()
        {
            IGraph g = this.GetInstance();
            Assert.AreEqual(0, g.Count);
            Assert.IsTrue(g.IsEmpty);

            INode s1 = g.CreateUriNode(new Uri("http://s1"));
            INode s2 = g.CreateUriNode(new Uri("http://s2"));
            INode p = g.CreateUriNode(new Uri("http://p"));
            INode o1 = g.CreateLiteralNode("value");
            INode o2 = g.CreateUriNode(new Uri("http://o"));

            INode[] vs = new INode[] {s1, s2, o1, o2};
            INode[] es = new INode[] {p};

            g.Assert(new Triple(s1, p, o1));
            g.Assert(new Triple(s1, p, o2));
            g.Assert(new Triple(s2, p, o2));
            Assert.AreEqual(3, g.Count);

            // Vertices
            List<INode> actual = g.Vertices.ToList();
            Assert.AreEqual(vs.Length, actual.Count);
            Assert.IsTrue(vs.All(v => actual.Remove(v)));

            // Edges
            actual = g.Edges.ToList();
            Assert.AreEqual(es.Length, actual.Count);
            Assert.IsTrue(es.All(e => actual.Remove(e)));
        }

        [Test]
        public void GraphContractStructure2()
        {
            IGraph g = this.GetInstance();
            Assert.AreEqual(0, g.Count);
            Assert.IsTrue(g.IsEmpty);

            INode s1 = g.CreateBlankNode();
            INode s2 = g.CreateGraphLiteralNode();
            INode p = g.CreateUriNode(new Uri("http://p"));
            INode o1 = g.CreateVariableNode("var");
            INode o2 = g.CreateUriNode(new Uri("http://o"));

            INode[] vs = new INode[] {s1, s2, o1, o2};
            INode[] es = new INode[] {p};

            g.Assert(new Triple(s1, p, o1));
            g.Assert(new Triple(s1, p, o2));
            g.Assert(new Triple(s2, p, o2));
            Assert.AreEqual(3, g.Count);

            // Vertices
            List<INode> actual = g.Vertices.ToList();
            Assert.AreEqual(vs.Length, actual.Count);
            Assert.IsTrue(vs.All(v => actual.Remove(v)));

            // Edges
            actual = g.Edges.ToList();
            Assert.AreEqual(es.Length, actual.Count);
            Assert.IsTrue(es.All(e => actual.Remove(e)));
        }

        [Test]
        public void GraphContractUsage1()
        {
            //Create a new Empty Graph
            IGraph g = this.GetInstance();
            Assert.IsNotNull(g);

            //Define Namespaces
            g.Namespaces.AddNamespace("vds", new Uri("http://www.vdesign-studios.com/dotNetRDF#"));
            g.Namespaces.AddNamespace("ecs", new Uri("http://id.ecs.soton.ac.uk/person/"));

            //Check we set the Namespace OK
            Assert.IsTrue(g.Namespaces.HasNamespace("vds"), "Failed to set a Namespace");

            //Create Uri Nodes
            INode rav08r, wh, lac, hcd;
            rav08r = g.CreateUriNode("ecs:11471");
            wh = g.CreateUriNode("ecs:1650");
            hcd = g.CreateUriNode("ecs:46");
            lac = g.CreateUriNode("ecs:60");

            //Create Uri Nodes for some Predicates
            INode supervises, collaborates, advises, has;
            supervises = g.CreateUriNode("vds:supervises");
            collaborates = g.CreateUriNode("vds:collaborates");
            advises = g.CreateUriNode("vds:advises");
            has = g.CreateUriNode("vds:has");

            //Create some Literal Nodes
            INode singleLine = g.CreateLiteralNode("Some string");
            INode multiLine = g.CreateLiteralNode("This goes over\n\nseveral\n\nlines");
            INode french = g.CreateLiteralNode("Bonjour", "fr");
            INode number = g.CreateLiteralNode("12", new Uri(g.Namespaces.GetNamespaceUri("xsd") + "integer"));

            g.Assert(new Triple(wh, supervises, rav08r));
            g.Assert(new Triple(lac, supervises, rav08r));
            g.Assert(new Triple(hcd, advises, rav08r));
            g.Assert(new Triple(wh, collaborates, lac));
            g.Assert(new Triple(wh, collaborates, hcd));
            g.Assert(new Triple(lac, collaborates, hcd));
            g.Assert(new Triple(rav08r, has, singleLine));
            g.Assert(new Triple(rav08r, has, multiLine));
            g.Assert(new Triple(rav08r, has, french));
            g.Assert(new Triple(rav08r, has, number));

            //Now print all the Statements
            Console.WriteLine("All Statements");
            Assert.AreEqual(10, g.Count);

            //Get statements about Rob Vesse
            Console.WriteLine();
            Console.WriteLine("Statements about Rob Vesse");
            Assert.AreEqual(7, g.GetTriples(rav08r).Count());

            //Get Statements about Collaboration
            Console.WriteLine();
            Console.WriteLine("Statements about Collaboration");
            Assert.AreEqual(3, g.GetTriples(collaborates).Count());
        }

        [Test]
        public void GraphContractUsage2()
        {
            //Create a new Empty Graph
            IGraph g = this.GetInstance();
            Assert.IsNotNull(g);

            //Define Namespaces
            g.Namespaces.AddNamespace("pets", new Uri("http://example.org/pets"));
            Assert.IsTrue(g.Namespaces.HasNamespace("pets"));

            //Create Uri Nodes
            INode dog, fido, rob, owner, name, species, breed, lab;
            dog = g.CreateUriNode("pets:Dog");
            fido = g.CreateUriNode("pets:abc123");
            rob = g.CreateUriNode("pets:def456");
            owner = g.CreateUriNode("pets:hasOwner");
            name = g.CreateUriNode("pets:hasName");
            species = g.CreateUriNode("pets:isAnimal");
            breed = g.CreateUriNode("pets:isBreed");
            lab = g.CreateUriNode("pets:Labrador");

            //Assert Triples
            g.Assert(new Triple(fido, species, dog));
            g.Assert(new Triple(fido, owner, rob));
            g.Assert(new Triple(fido, name, g.CreateLiteralNode("Fido")));
            g.Assert(new Triple(rob, name, g.CreateLiteralNode("Rob")));
            g.Assert(new Triple(fido, breed, lab));

            Assert.AreEqual(5, g.Count);
        }

        [Test]
        public void GraphContractEvents1()
        {
            IEventedGraph g = this.GetInstance() as IEventedGraph;
            if (g == null || !g.HasEvents) Assert.Ignore("Graph instance does not support events");

            // Attach event handler
            int events = 0;
            g.CollectionChanged += (sender, args) => {
                Assert.AreEqual(NotifyCollectionChangedAction.Add, args.Action);
                events++; 
            };

            // Assert should generate an event
            Triple t = new Triple(g.CreateUriNode(new Uri("http://subject")), g.CreateUriNode(new Uri("http://predicate")), g.CreateBlankNode());
            g.Assert(t);
            Assert.AreEqual(1, events);

            // Adding the same triple again should not fire the event
            g.Assert(t);
            Assert.AreEqual(1, events);
        }

        [Test]
        public void GraphContractEvents2()
        {
            IEventedGraph g = this.GetInstance() as IEventedGraph;
            if (g == null || !g.HasEvents) Assert.Ignore("Graph instance does not support events");

            // Attach event handler
            int events = 0;
            NotifyCollectionChangedAction action = NotifyCollectionChangedAction.Add;
            g.CollectionChanged += (sender, args) =>
            {
                Assert.AreEqual(action, args.Action);
                events++;
            };

            // Assert should generate an event
            Triple t = new Triple(g.CreateUriNode(new Uri("http://subject")), g.CreateUriNode(new Uri("http://predicate")), g.CreateBlankNode());
            g.Assert(t);
            Assert.AreEqual(1, events);

            // Adding the same triple again should not fire the event
            g.Assert(t);
            Assert.AreEqual(1, events);

            // Retracting the triple should fire an event
            action = NotifyCollectionChangedAction.Remove;
            g.Retract(t);
            Assert.AreEqual(2, events);
        }

        [Test]
        public void GraphContractEvents3()
        {
            IEventedGraph g = this.GetInstance() as IEventedGraph;
            if (g == null || !g.HasEvents) Assert.Ignore("Graph instance does not support events");

            // Attach event handler
            int events = 0;
            g.CollectionChanged += (sender, args) => {
                Assert.AreEqual(NotifyCollectionChangedAction.Reset, args.Action);
                events++;
            };

            // Clear 
            g.Clear();

            // Expect one event to have fired
            Assert.AreEqual(1, events);
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
    public class SubTreeIndexedGraphContractTests
        : AbstractGraphContractTests
    {
        protected override IGraph GetInstance()
        {
            return new Graph(new SubTreeIndexedTripleCollection());
        }
    }

    [TestFixture]
    public class TrieIndexedGraphContractTests
        : AbstractGraphContractTests
    {
        protected override IGraph GetInstance()
        {
            return new Graph(new TrieIndexedTripleCollection());
        }
    }

    [TestFixture]
    public class UnindexedGraphContractTests
        : AbstractGraphContractTests
    {

        protected override IGraph GetInstance()
        {
            return new Graph(new TripleCollection());
        }
    }

    [TestFixture]
    public class ThreadSafeGraphContractTests
        : AbstractGraphContractTests
    {
        protected override IGraph GetInstance()
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
            : WrapperGraph {}
    }
}