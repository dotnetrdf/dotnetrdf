using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Storage;

namespace VDS.RDF.Test.Storage
{
    [TestClass]
    public class PersistentTripleStoreTests
    {
        private const String TestGraphUri1 = "http://example.org/persistence/graphs/1",
                             TestGraphUri2 = "http://example.org/persistence/graphs/2",
                             TestGraphUri3 = "http://example.org/persistence/graphs/3";

        private void EnsureTestDataset(IGenericIOManager manager)
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            g.BaseUri = new Uri(TestGraphUri1);
            g.Retract(g.Triples.Where(t => !t.IsGroundTriple));
            manager.SaveGraph(g);

            g = new Graph();
            g.LoadFromFile("InferenceTest.ttl");
            g.BaseUri = new Uri(TestGraphUri2);
            g.Retract(g.Triples.Where(t => !t.IsGroundTriple));
            manager.SaveGraph(g);

            g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Query.Optimisation.OptimiserStats.ttl");
            g.BaseUri = new Uri(TestGraphUri3);
            g.Retract(g.Triples.Where(t => !t.IsGroundTriple));
            manager.SaveGraph(g);
        }

        [TestMethod,ExpectedException(typeof(ArgumentNullException))]
        public void StoragePersistentTripleStoreBadInstantiation()
        {
            PersistentTripleStore store = new PersistentTripleStore(null);
        }

        [TestMethod]
        public void StoragePersistentTripleStoreContains()
        {
            InMemoryManager manager = new InMemoryManager();
            this.EnsureTestDataset(manager);

            PersistentTripleStore store = new PersistentTripleStore(manager);
            try
            {
                Assert.IsTrue(store.HasGraph(new Uri(TestGraphUri1)), "URI 1 should return true for HasGraph()");
                Assert.IsTrue(store.Graphs.Contains(new Uri(TestGraphUri1)), "URI 1 should return true for Graphs.Contains()");
                Assert.IsTrue(store.HasGraph(new Uri(TestGraphUri2)), "URI 2 should return true for HasGraph()");
                Assert.IsTrue(store.Graphs.Contains(new Uri(TestGraphUri2)), "URI 2 should return true for Graphs.Contains()");
                Assert.IsTrue(store.HasGraph(new Uri(TestGraphUri3)), "URI 3 should return true for HasGraph()");
                Assert.IsTrue(store.Graphs.Contains(new Uri(TestGraphUri3)), "URI 3 should return true for Graphs.Contains()");

                Uri noSuchThing = new Uri("http://example.org/persistence/graphs/noSuchGraph");
                Assert.IsFalse(store.HasGraph(noSuchThing), "Bad URI should return false for HasGraph()");
                Assert.IsFalse(store.Graphs.Contains(noSuchThing), "Bad URI should return false for Graphs.Contains()");

            }
            finally
            {
                store.Dispose();
            }
        }

        [TestMethod]
        public void StoragePersistentTripleStoreAddGraphFlushed()
        {
            InMemoryManager manager = new InMemoryManager();
            this.EnsureTestDataset(manager);

            PersistentTripleStore store = new PersistentTripleStore(manager);
            try
            {
                Graph g = new Graph();
                g.BaseUri = new Uri("http://example.org/persistence/graphs/added");
                g.Assert(g.CreateUriNode("rdf:subject"), g.CreateUriNode("rdf:predicate"), g.CreateUriNode("rdf:object"));
                store.Add(g);

                Assert.IsTrue(store.HasGraph(g.BaseUri), "Newly added graph should exist in in-memory view of store");
                Assert.IsFalse(manager.ListGraphs().Contains(g.BaseUri), "Newly added graph should not yet exist in underlying store");

                store.Flush();

                Assert.IsTrue(manager.ListGraphs().Contains(g.BaseUri), "After Flush() is called added graph should exist in underlying store");
            }
            finally
            {
                store.Dispose();
            }
        }

        [TestMethod]
        public void StoragePersistentTripleStoreAddGraphDiscarded()
        {
            InMemoryManager manager = new InMemoryManager();
            this.EnsureTestDataset(manager);

            PersistentTripleStore store = new PersistentTripleStore(manager);
            try
            {
                Graph g = new Graph();
                g.BaseUri = new Uri("http://example.org/persistence/graphs/added");
                g.Assert(g.CreateUriNode("rdf:subject"), g.CreateUriNode("rdf:predicate"), g.CreateUriNode("rdf:object"));
                store.Add(g);

                Assert.IsTrue(store.HasGraph(g.BaseUri), "Newly added graph should exist in in-memory view of store");
                Assert.IsFalse(manager.ListGraphs().Contains(g.BaseUri), "Newly added graph should not yet exist in underlying store");

                store.Discard();

                Graph h = new Graph();
                manager.LoadGraph(h, g.BaseUri);
                Assert.IsTrue(h.IsEmpty, "After Discard() is called a graph may exist in the underlying store but it MUST be empty");
            }
            finally
            {
                store.Dispose();
            }
        }
    }
}
