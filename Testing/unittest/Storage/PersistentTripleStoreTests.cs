using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing.Handlers;
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

        private void EnsureGraphDeleted(IGenericIOManager manager, Uri graphUri)
        {
            if (manager.DeleteSupported)
            {
                manager.DeleteGraph(graphUri);
            }
            else
            {
                Assert.Inconclusive("Unable to conduct this test as it requires ensuring a Graph is deleted from the underlying store which the IGenericIOManager instance does not support");
            }
        }

        [TestMethod,ExpectedException(typeof(ArgumentNullException))]
        public void StoragePersistentTripleStoreBadInstantiation()
        {
            PersistentTripleStore store = new PersistentTripleStore(null);
        }

        #region Contains Tests

        private void TestContains(IGenericIOManager manager)
        {
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
        public void StoragePersistentTripleStoreMemContains()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestContains(manager);
        }

        [TestMethod]
        public void StoragePersistentTripleStoreFusekiContains()
        {
            FusekiConnector fuseki = new FusekiConnector("http://localhost:3030/dataset/data");
            this.TestContains(fuseki);
        }

        [TestMethod]
        public void StoragePersistentTripleStoreVirtuosoContains()
        {
            VirtuosoManager virtuoso = new VirtuosoManager("DB", VirtuosoTest.VirtuosoTestUsername, VirtuosoTest.VirtuosoTestPassword);
            this.TestContains(virtuoso);
        }

        #endregion

        #region Get Graph Tests

        private void TestGetGraph(IGenericIOManager manager)
        {
            this.EnsureTestDataset(manager);

            PersistentTripleStore store = new PersistentTripleStore(manager);
            try
            {
                Graph aExpected = new Graph();
                aExpected.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                aExpected.Retract(aExpected.Triples.Where(t => !t.IsGroundTriple));
                aExpected.BaseUri = new Uri(TestGraphUri1);
                IGraph aActual = store.Graph(aExpected.BaseUri);
                Assert.AreEqual(aExpected, aActual, "Graph 1 should be equal when retrieved using Graph()");
                aActual = store.Graphs[aExpected.BaseUri];
                Assert.AreEqual(aExpected, aActual, "Graph 1 should be equal when retrieved using Graphs[]");

                Graph bExpected = new Graph();
                bExpected.LoadFromFile("InferenceTest.ttl");
                bExpected.Retract(bExpected.Triples.Where(t => !t.IsGroundTriple));
                bExpected.BaseUri = new Uri(TestGraphUri2);
                IGraph bActual = store.Graph(bExpected.BaseUri);
                Assert.AreEqual(bExpected, bActual, "Graph 2 should be equal when retrieved using Graph()");
                bActual = store.Graphs[bExpected.BaseUri];
                Assert.AreEqual(bExpected, bActual, "Graph 2 should be equal when retrieved using Graphs[]");

                Graph cExpected = new Graph();
                cExpected.LoadFromEmbeddedResource("VDS.RDF.Query.Optimisation.OptimiserStats.ttl");
                cExpected.Retract(cExpected.Triples.Where(t => !t.IsGroundTriple));
                cExpected.BaseUri = new Uri(TestGraphUri3);
                IGraph cActual = store.Graph(cExpected.BaseUri);
                Assert.AreEqual(cExpected, cActual, "Graph 3 should be equal when retrieved using Graph()");
                cActual = store.Graphs[cExpected.BaseUri];
                Assert.AreEqual(cExpected, cActual, "Graph 3 should be equal when retrieved using Graphs[]");
            }
            finally
            {
                store.Dispose();
            }
        }

        [TestMethod]
        public void StoragePersistentTripleStoreMemGetGraph()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestGetGraph(manager);
        }

        [TestMethod]
        public void StoragePersistentTripleStoreFusekiGetGraph()
        {
            FusekiConnector fuseki = new FusekiConnector("http://localhost:3030/dataset/data");
            this.TestGetGraph(fuseki);
        }

        [TestMethod]
        public void StoragePersistentTripleStoreVirtuosoGetGraph()
        {
            VirtuosoManager virtuoso = new VirtuosoManager("DB", VirtuosoTest.VirtuosoTestUsername, VirtuosoTest.VirtuosoTestPassword);
            this.TestGetGraph(virtuoso);
        }

        #endregion

        #region Add Graph Tests

        private void TestAddGraphFlushed(IGenericIOManager manager)
        {
            this.EnsureTestDataset(manager);

            PersistentTripleStore store = new PersistentTripleStore(manager);
            try
            {
                Graph g = new Graph();
                g.BaseUri = new Uri("http://example.org/persistence/graphs/added/flushed");
                this.EnsureGraphDeleted(manager, g.BaseUri);
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
        public void StoragePersistentTripleStoreMemAddGraphFlushed()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestAddGraphFlushed(manager);
        }

        [TestMethod]
        public void StoragePersistentTripleStoreFusekiAddGraphFlushed()
        {
            FusekiConnector fuseki = new FusekiConnector("http://localhost:3030/dataset/data");
            this.TestAddGraphFlushed(fuseki);
        }

        [TestMethod]
        public void StoragePersistentTripleStoreVirtuosoAddGraphFlushed()
        {
            VirtuosoManager virtuoso = new VirtuosoManager("DB", VirtuosoTest.VirtuosoTestUsername, VirtuosoTest.VirtuosoTestPassword);
            this.TestAddGraphFlushed(virtuoso);
        }

        private void TestAddGraphDiscarded(IGenericIOManager manager)
        {
            this.EnsureTestDataset(manager);

            PersistentTripleStore store = new PersistentTripleStore(manager);
            try
            {
                Graph g = new Graph();
                g.BaseUri = new Uri("http://example.org/persistence/graphs/added/discarded");
                this.EnsureGraphDeleted(manager, g.BaseUri);
                g.Assert(g.CreateUriNode("rdf:subject"), g.CreateUriNode("rdf:predicate"), g.CreateUriNode("rdf:object"));
                store.Add(g);

                Assert.IsTrue(store.HasGraph(g.BaseUri), "Newly added graph should exist in in-memory view of store");
                Assert.IsFalse(manager.ListGraphs().Contains(g.BaseUri), "Newly added graph should not yet exist in underlying store");

                store.Discard();

                Graph h = new Graph();
                try
                {
                    manager.LoadGraph(h, g.BaseUri);
                }
                catch
                {
                    //No catch needed
                }
                Assert.IsTrue(h.IsEmpty, "After Discard() is called a graph may exist in the underlying store but it MUST be empty");
            }
            finally
            {
                store.Dispose();
            }
        }

        [TestMethod]
        public void StoragePersistentTripleStoreMemAddGraphDiscarded()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestAddGraphDiscarded(manager);
        }
        [TestMethod]
        public void StoragePersistentTripleStoreFusekiAddGraphDiscarded()
        {
            FusekiConnector fuseki = new FusekiConnector("http://localhost:3030/dataset/data");
            this.TestAddGraphDiscarded(fuseki);
        }

        [TestMethod]
        public void StoragePersistentTripleStoreVirtuosoAddGraphDiscarded()
        {
            VirtuosoManager virtuoso = new VirtuosoManager("DB", VirtuosoTest.VirtuosoTestUsername, VirtuosoTest.VirtuosoTestPassword);
            this.TestAddGraphDiscarded(virtuoso);
        }

        #endregion

        #region Remove Graph Tests

        private void TestRemoveGraphFlushed(IGenericIOManager manager)
        {
            this.EnsureTestDataset(manager);

            PersistentTripleStore store = new PersistentTripleStore(manager);
            try
            {
                Uri toRemove = new Uri(TestGraphUri1);
                Assert.IsTrue(store.HasGraph(toRemove), "In-memory view should contain the Graph we wish to remove");

                store.Remove(toRemove);
                Assert.IsFalse(store.HasGraph(toRemove), "In-memory view should no longer contain the Graph we removed prior to the Flush/Discard operation");
                store.Flush();

                Assert.IsFalse(store.HasGraph(toRemove), "In-Memory view should no longer contain the Graph we removed after Flushing");
                AnyHandler handler = new AnyHandler();
                manager.LoadGraph(handler, toRemove);
                Assert.IsFalse(handler.Any, "Attempting to load Graph from underlying store should return nothing after the Flush() operation");
            }
            finally
            {
                store.Dispose();
            }
        }

        [TestMethod]
        public void StoragePersistentTripleStoreMemRemoveGraphFlushed()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestRemoveGraphFlushed(manager);
        }

        private void TestRemoveGraphDiscarded(IGenericIOManager manager)
        {
            this.EnsureTestDataset(manager);

            PersistentTripleStore store = new PersistentTripleStore(manager);
            try
            {
                Uri toRemove = new Uri(TestGraphUri1);
                Assert.IsTrue(store.HasGraph(toRemove), "In-memory view should contain the Graph we wish to remove");

                store.Remove(toRemove);
                Assert.IsFalse(store.HasGraph(toRemove), "In-memory view should no longer contain the Graph we removed prior to the Flush/Discard operation");
                store.Discard();

                Assert.IsTrue(store.HasGraph(toRemove), "In-Memory view should still contain the Graph we removed as we Discarded that change");
                AnyHandler handler = new AnyHandler();
                manager.LoadGraph(handler, toRemove);
                Assert.IsTrue(handler.Any, "Attempting to load Graph from underlying store should return something as the Discard() prevented the removal being persisted");
            }
            finally
            {
                store.Dispose();
            }
        }

        [TestMethod]
        public void StoragePersistentTripleStoreMemRemoveGraphDiscarded()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestRemoveGraphDiscarded(manager);
        }

        #endregion

        #region Add then Remove Graph Sequencing Tests

        private void TestAddThenRemoveGraphFlushed(IGenericIOManager manager)
        {
            this.EnsureTestDataset(manager);

            PersistentTripleStore store = new PersistentTripleStore(manager);
            try
            {
                Graph g = new Graph();
                g.BaseUri = new Uri("http://example.org/persistence/graphs/added/flushed");
                this.EnsureGraphDeleted(manager, g.BaseUri);
                g.Assert(g.CreateUriNode("rdf:subject"), g.CreateUriNode("rdf:predicate"), g.CreateUriNode("rdf:object"));
                store.Add(g);

                Assert.IsTrue(store.HasGraph(g.BaseUri), "Newly added graph should exist in in-memory view of store");
                Assert.IsFalse(manager.ListGraphs().Contains(g.BaseUri), "Newly added graph should not yet exist in underlying store");

                store.Remove(g.BaseUri);
                Assert.IsFalse(store.HasGraph(g.BaseUri), "Graph then removed before Flush/Discard() should no longer exist in in-memory view of store");
                Assert.IsFalse(manager.ListGraphs().Contains(g.BaseUri), "Graph then removed should still not exist in underlying store");

                store.Flush();

                Assert.IsFalse(store.HasGraph(g.BaseUri), "After Flush() is called graph should not exist in in-memory view of store");
                Assert.IsFalse(manager.ListGraphs().Contains(g.BaseUri), "After Flush() is called added then removed graph should not exist in underlying store");
            }
            finally
            {
                store.Dispose();
            }
        }

        [TestMethod]
        public void StoragePersistentTripleStoreMemAddThenRemoveGraphFlushed()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestAddThenRemoveGraphFlushed(manager);
        }

        [TestMethod]
        public void StoragePersistentTripleStoreFusekiAddThenRemoveGraphFlushed()
        {
            FusekiConnector fuseki = new FusekiConnector("http://localhost:3030/dataset/data");
            this.TestAddThenRemoveGraphFlushed(fuseki);
        }

        [TestMethod]
        public void StoragePersistentTripleStoreVirtuosoAddThenRemoveGraphFlushed()
        {
            VirtuosoManager virtuoso = new VirtuosoManager("DB", VirtuosoTest.VirtuosoTestUsername, VirtuosoTest.VirtuosoTestPassword);
            this.TestAddThenRemoveGraphFlushed(virtuoso);
        }

        private void TestAddThenRemoveGraphDiscarded(IGenericIOManager manager)
        {
            this.EnsureTestDataset(manager);

            PersistentTripleStore store = new PersistentTripleStore(manager);
            try
            {
                Graph g = new Graph();
                g.BaseUri = new Uri("http://example.org/persistence/graphs/added/discarded");
                this.EnsureGraphDeleted(manager, g.BaseUri);
                g.Assert(g.CreateUriNode("rdf:subject"), g.CreateUriNode("rdf:predicate"), g.CreateUriNode("rdf:object"));
                store.Add(g);

                Assert.IsTrue(store.HasGraph(g.BaseUri), "Newly added graph should exist in in-memory view of store");
                Assert.IsFalse(manager.ListGraphs().Contains(g.BaseUri), "Newly added graph should not yet exist in underlying store");

                store.Remove(g.BaseUri);
                Assert.IsFalse(store.HasGraph(g.BaseUri), "Graph then removed before Flush/Discard() should no longer exist in in-memory view of store");
                Assert.IsFalse(manager.ListGraphs().Contains(g.BaseUri), "Graph then removed should still not exist in underlying store");

                store.Discard();

                Assert.IsFalse(store.HasGraph(g.BaseUri), "After Discard() is called graph should not exist in in-memory view of store");
                Assert.IsFalse(manager.ListGraphs().Contains(g.BaseUri), "After Discard() is called added then removed graph should not exist in underlying store");
            }
            finally
            {
                store.Dispose();
            }
        }

        [TestMethod]
        public void StoragePersistentTripleStoreMemAddThenRemoveGraphDiscarded()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestAddThenRemoveGraphDiscarded(manager);
        }

        [TestMethod]
        public void StoragePersistentTripleStoreFusekiAddThenRemoveGraphDiscarded()
        {
            FusekiConnector fuseki = new FusekiConnector("http://localhost:3030/dataset/data");
            this.TestAddThenRemoveGraphDiscarded(fuseki);
        }

        [TestMethod]
        public void StoragePersistentTripleStoreVirtuosoAddThenRemoveGraphDiscarded()
        {
            VirtuosoManager virtuoso = new VirtuosoManager("DB", VirtuosoTest.VirtuosoTestUsername, VirtuosoTest.VirtuosoTestPassword);
            this.TestAddThenRemoveGraphDiscarded(virtuoso);
        }

        #endregion

        #region Remove then Add Graph Sequencing Tests

        private void TestRemoveThenAddGraphFlushed(IGenericIOManager manager)
        {
            this.EnsureTestDataset(manager);

            PersistentTripleStore store = new PersistentTripleStore(manager);
            try
            {
                Uri toRemove = new Uri(TestGraphUri1);
                IGraph g = store.Graph(toRemove);
                Assert.IsTrue(store.HasGraph(toRemove), "In-memory view should contain the Graph we wish to remove");

                store.Remove(toRemove);
                Assert.IsFalse(store.HasGraph(toRemove), "In-memory view should no longer contain the Graph we removed prior to the Flush/Discard operation");

                store.Add(g);
                Assert.IsTrue(store.HasGraph(toRemove), "In-memory should now contain the Graph we added back");

                store.Flush();

                Assert.IsTrue(store.HasGraph(toRemove), "In-Memory view should still contain the Graph we added back after Flushing");
                AnyHandler handler = new AnyHandler();
                manager.LoadGraph(handler, toRemove);
                Assert.IsTrue(handler.Any, "Attempting to load Graph from underlying store should return something after the Flush() operation since we didn't remove the graph in the end");
            }
            finally
            {
                store.Dispose();
            }
        }

        [TestMethod]
        public void StoragePersistentTripleStoreMemRemoveThenAddGraphFlushed()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestRemoveThenAddGraphFlushed(manager);
        }

        [TestMethod]
        public void StoragePersistentTripleStoreFusekiRemoveThenAddGraphFlushed()
        {
            FusekiConnector fuseki = new FusekiConnector("http://localhost:3030/dataset/data");
            this.TestRemoveThenAddGraphFlushed(fuseki);
        }

        [TestMethod]
        public void StoragePersistentTripleStoreVirtuosoRemoveThenAddGraphFlushed()
        {
            VirtuosoManager virtuoso = new VirtuosoManager("DB", VirtuosoTest.VirtuosoTestUsername, VirtuosoTest.VirtuosoTestPassword);
            this.TestRemoveThenAddGraphFlushed(virtuoso);
        }

        private void TestRemoveThenAddGraphDiscarded(IGenericIOManager manager)
        {
            this.EnsureTestDataset(manager);

            PersistentTripleStore store = new PersistentTripleStore(manager);
            try
            {
                Uri toRemove = new Uri(TestGraphUri1);
                IGraph g = store.Graph(toRemove);
                Assert.IsTrue(store.HasGraph(toRemove), "In-memory view should contain the Graph we wish to remove");

                store.Remove(toRemove);
                Assert.IsFalse(store.HasGraph(toRemove), "In-memory view should no longer contain the Graph we removed prior to the Flush/Discard operation");

                store.Add(g);
                Assert.IsTrue(store.HasGraph(toRemove), "In-memory should now contain the Graph we added back");

                store.Discard();

                Assert.IsTrue(store.HasGraph(toRemove), "In-Memory view should still contain the Graph we removed and added back regardless as we Discarded that change");
                AnyHandler handler = new AnyHandler();
                manager.LoadGraph(handler, toRemove);
                Assert.IsTrue(handler.Any, "Attempting to load Graph from underlying store should return something as the Discard() prevented the removal and add back being persisted");
            }
            finally
            {
                store.Dispose();
            }
        }

        [TestMethod]
        public void StoragePersistentTripleStoreMemRemoveThenAddGraphDiscarded()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestRemoveThenAddGraphDiscarded(manager);
        }

        [TestMethod]
        public void StoragePersistentTripleStoreFusekiRemoveThenAddGraphDiscarded()
        {
            FusekiConnector fuseki = new FusekiConnector("http://localhost:3030/dataset/data");
            this.TestRemoveThenAddGraphDiscarded(fuseki);
        }

        [TestMethod]
        public void StoragePersistentTripleStoreVirtuosoRemoveThenAddGraphDiscarded()
        {
            VirtuosoManager virtuoso = new VirtuosoManager("DB", VirtuosoTest.VirtuosoTestUsername, VirtuosoTest.VirtuosoTestPassword);
            this.TestRemoveThenAddGraphDiscarded(virtuoso);
        }

        #endregion
    }
}
