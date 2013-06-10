/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using VDS.RDF.Update;

namespace VDS.RDF.Storage
{
    [TestFixture]
    public class PersistentTripleStoreTests
    {
        private const String TestGraphUri1 = "http://example.org/persistence/graphs/1",
                             TestGraphUri2 = "http://example.org/persistence/graphs/2",
                             TestGraphUri3 = "http://example.org/persistence/graphs/3";

        private void EnsureTestDataset(IStorageProvider manager)
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            g.BaseUri = new Uri(TestGraphUri1);
            g.Retract(g.Triples.Where(t => !t.IsGroundTriple).ToList());
            manager.SaveGraph(g);

            g = new Graph();
            g.LoadFromFile("resources\\InferenceTest.ttl");
            g.BaseUri = new Uri(TestGraphUri2);
            g.Retract(g.Triples.Where(t => !t.IsGroundTriple).ToList());
            manager.SaveGraph(g);

            g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Query.Optimisation.OptimiserStats.ttl");
            g.BaseUri = new Uri(TestGraphUri3);
            g.Retract(g.Triples.Where(t => !t.IsGroundTriple).ToList());
            manager.SaveGraph(g);
        }

        private void EnsureGraphDeleted(IStorageProvider manager, Uri graphUri)
        {
            if (manager.DeleteSupported)
            {
                manager.DeleteGraph(graphUri);
            }
            else
            {
                Assert.Inconclusive("Unable to conduct this test as it requires ensuring a Graph is deleted from the underlying store which the IStorageProvider instance does not support");
            }
        }

        [Test,ExpectedException(typeof(ArgumentNullException))]
        public void StoragePersistentTripleStoreBadInstantiation()
        {
            PersistentTripleStore store = new PersistentTripleStore(null);
        }

        #region Contains Tests

        private void TestContains(IStorageProvider manager)
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

        [Test]
        public void StoragePersistentTripleStoreMemContains()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestContains(manager);
        }

#if !NO_SYNC_HTTP // Test requires synchronous APIs
        [Test]
        public void StoragePersistentTripleStoreFusekiContains()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestContains(fuseki);
        }
#endif

#if !PORTABLE // No VirutousoManager in PCL
        [Test]
        public void StoragePersistentTripleStoreVirtuosoContains()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestContains(virtuoso);
        }
#endif

        #endregion

        #region Get Graph Tests

        private void TestGetGraph(IStorageProvider manager)
        {
            this.EnsureTestDataset(manager);

            PersistentTripleStore store = new PersistentTripleStore(manager);
            try
            {
                Graph aExpected = new Graph();
                aExpected.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                aExpected.Retract(aExpected.Triples.Where(t => !t.IsGroundTriple).ToList());
                aExpected.BaseUri = new Uri(TestGraphUri1);
                IGraph aActual = store[aExpected.BaseUri];
                Assert.AreEqual(aExpected, aActual, "Graph 1 should be equal when retrieved using Graph()");
                aActual = store.Graphs[aExpected.BaseUri];
                Assert.AreEqual(aExpected, aActual, "Graph 1 should be equal when retrieved using Graphs[]");

                Graph bExpected = new Graph();
                bExpected.LoadFromFile("resources\\InferenceTest.ttl");
                bExpected.Retract(bExpected.Triples.Where(t => !t.IsGroundTriple).ToList());
                bExpected.BaseUri = new Uri(TestGraphUri2);
                IGraph bActual = store[bExpected.BaseUri];
                Assert.AreEqual(bExpected, bActual, "Graph 2 should be equal when retrieved using Graph()");
                bActual = store.Graphs[bExpected.BaseUri];
                Assert.AreEqual(bExpected, bActual, "Graph 2 should be equal when retrieved using Graphs[]");

                Graph cExpected = new Graph();
                cExpected.LoadFromEmbeddedResource("VDS.RDF.Query.Optimisation.OptimiserStats.ttl");
                cExpected.Retract(cExpected.Triples.Where(t => !t.IsGroundTriple).ToList());
                cExpected.BaseUri = new Uri(TestGraphUri3);
                IGraph cActual = store[cExpected.BaseUri];
                Assert.AreEqual(cExpected, cActual, "Graph 3 should be equal when retrieved using Graph()");
                cActual = store.Graphs[cExpected.BaseUri];
                Assert.AreEqual(cExpected, cActual, "Graph 3 should be equal when retrieved using Graphs[]");
            }
            finally
            {
                store.Dispose();
            }
        }

        [Test]
        public void StoragePersistentTripleStoreMemGetGraph()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestGetGraph(manager);
        }


        [Test]
#if !NO_SYNC_HTTP
        public void StoragePersistentTripleStoreFusekiGetGraph()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestGetGraph(fuseki);
        }
#endif

#if !PORTABLE
        [Test]
        public void StoragePersistentTripleStoreVirtuosoGetGraph()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestGetGraph(virtuoso);
        }
#endif
        #endregion

        #region Add Triples Tests

        private void TestAddTriplesFlushed(IStorageProvider manager)
        {
            this.EnsureGraphDeleted(manager, new Uri(TestGraphUri1));
            this.EnsureTestDataset(manager);

            PersistentTripleStore store = new PersistentTripleStore(manager);
            try
            {
                IGraph g = store[new Uri(TestGraphUri1)];

                Triple toAdd = new Triple(g.CreateUriNode(new Uri("http://example.org/subject")), g.CreateUriNode(new Uri("http://example.org/predicate")), g.CreateUriNode(new Uri("http://example.org/object")));
                g.Assert(toAdd);

                Assert.IsTrue(g.ContainsTriple(toAdd), "Added triple should be present in in-memory view prior to Flush/Discard");
                Graph h = new Graph();
                manager.LoadGraph(h, g.BaseUri);
                Assert.IsFalse(h.ContainsTriple(toAdd), "Added triple should not be present in underlying store prior to Flush/Discard");

                store.Flush();

                Assert.IsTrue(g.ContainsTriple(toAdd), "Added triple should be present in in-memory view after Flush");
                h = new Graph();
                manager.LoadGraph(h, g.BaseUri);
                Assert.IsTrue(h.ContainsTriple(toAdd), "Added triple should be present in underlying store after Flush");
            }
            finally
            {
                store.Dispose();
            }
        }

        [Test]
        public void StoragePersistentTripleStoreMemAddTriplesFlushed()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestAddTriplesFlushed(manager);
        }

#if !NO_SYNC_HTTP
        [Test]
        public void StoragePersistentTripleStoreFusekiAddTriplesFlushed()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestAddTriplesFlushed(fuseki);
        }
#endif

#if !PORTABLE
        [Test]
        public void StoragePersistentTripleStoreVirtuosoAddTriplesFlushed()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestAddTriplesFlushed(virtuoso);
        }
#endif

        private void TestAddTriplesDiscarded(IStorageProvider manager)
        {
            this.EnsureGraphDeleted(manager, new Uri(TestGraphUri1));
            this.EnsureTestDataset(manager);

            PersistentTripleStore store = new PersistentTripleStore(manager);
            try
            {
                IGraph g = store[new Uri(TestGraphUri1)];

                Triple toAdd = new Triple(g.CreateUriNode(new Uri("http://example.org/subject")), g.CreateUriNode(new Uri("http://example.org/predicate")), g.CreateUriNode(new Uri("http://example.org/object")));
                g.Assert(toAdd);

                Assert.IsTrue(g.ContainsTriple(toAdd), "Added triple should be present in in-memory view prior to Flush/Discard");
                Graph h = new Graph();
                manager.LoadGraph(h, g.BaseUri);
                Assert.IsFalse(h.ContainsTriple(toAdd), "Added triple should not be present in underlying store prior to Flush/Discard");

                store.Discard();

                Assert.IsFalse(g.ContainsTriple(toAdd), "Added triple should not be present in in-memory view after Discard");
                h = new Graph();
                manager.LoadGraph(h, g.BaseUri);
                Assert.IsFalse(h.ContainsTriple(toAdd), "Added triple should not be present in underlying store after Discard");
            }
            finally
            {
                store.Dispose();
            }
        }

        [Test]
        public void StoragePersistentTripleStoreMemAddTriplesDiscarded()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestAddTriplesDiscarded(manager);
        }

#if !NO_SYNC_HTTP
        [Test]
        public void StoragePersistentTripleStoreFusekiAddTriplesDiscarded()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestAddTriplesDiscarded(fuseki);
        }
#endif

#if !PORTABLE
        [Test]
        public void StoragePersistentTripleStoreVirtuosoAddTriplesDiscarded()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestAddTriplesDiscarded(virtuoso);
        }
#endif

        #endregion

        #region Remove Triples Tests

        private void TestRemoveTriplesFlushed(IStorageProvider manager)
        {
            this.EnsureTestDataset(manager);

            PersistentTripleStore store = new PersistentTripleStore(manager);
            try
            {
                IGraph g = store[new Uri(TestGraphUri1)];

                INode rdfType = g.CreateUriNode(new Uri(NamespaceMapper.RDF + "type"));
                g.Retract(g.GetTriplesWithPredicate(rdfType).ToList());

                Assert.IsFalse(g.GetTriplesWithPredicate(rdfType).Any(), "Removed triples should not be present in in-memory view prior to Flush/Discard");
                Graph h = new Graph();
                manager.LoadGraph(h, g.BaseUri);
                Assert.IsTrue(h.GetTriplesWithPredicate(rdfType).Any(), "Removed triples should still be present in underlying store prior to Flush/Discard");

                store.Flush();

                Assert.IsFalse(g.GetTriplesWithPredicate(rdfType).Any(), "Removed triples should not be present in in-memory view after Flush");
                h = new Graph();
                manager.LoadGraph(h, g.BaseUri);
                Assert.IsFalse(h.GetTriplesWithPredicate(rdfType).Any(), "Removed triples should no longer be present in underlying store after Flush");

            }
            finally
            {
                store.Dispose();
            }
        }

        [Test]
        public void StoragePersistentTripleStoreMemRemoveTriplesFlushed()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestRemoveTriplesFlushed(manager);
        }

#if !NO_SYNC_HTTP
        [Test]
        public void StoragePersistentTripleStoreFusekiRemoveTriplesFlushed()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestRemoveTriplesFlushed(fuseki);
        }
#endif

#if !PORTABLE
        [Test]
        public void StoragePersistentTripleStoreVirtuosoRemoveTriplesFlushed()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestRemoveTriplesFlushed(virtuoso);
        }
#endif
        private void TestRemoveTriplesDiscarded(IStorageProvider manager)
        {
            this.EnsureTestDataset(manager);

            PersistentTripleStore store = new PersistentTripleStore(manager);
            try
            {
                IGraph g = store[new Uri(TestGraphUri1)];

                INode rdfType = g.CreateUriNode(new Uri(NamespaceMapper.RDF + "type"));
                g.Retract(g.GetTriplesWithPredicate(rdfType).ToList());

                Assert.IsFalse(g.GetTriplesWithPredicate(rdfType).Any(), "Removed triples should not be present in in-memory view prior to Flush/Discard");
                Graph h = new Graph();
                manager.LoadGraph(h, g.BaseUri);
                Assert.IsTrue(h.GetTriplesWithPredicate(rdfType).Any(), "Removed triples should still be present in underlying store prior to Flush/Discard");

                store.Discard();

                Assert.IsTrue(g.GetTriplesWithPredicate(rdfType).Any(), "Removed triples should now be present in in-memory view after Discard");
                h = new Graph();
                manager.LoadGraph(h, g.BaseUri);
                Assert.IsTrue(h.GetTriplesWithPredicate(rdfType).Any(), "Removed triples should still be present in underlying store after Discard");

            }
            finally
            {
                store.Dispose();
            }
        }

        [Test]
        public void StoragePersistentTripleStoreMemRemoveTriplesDiscarded()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestRemoveTriplesDiscarded(manager);
        }

#if !NO_SYNC_HTTP
        [Test]
        public void StoragePersistentTripleStoreFusekiRemoveTriplesDiscarded()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestRemoveTriplesDiscarded(fuseki);
        }
#endif

#if !PORTABLE
        [Test]
        public void StoragePersistentTripleStoreVirtuosoRemoveTriplesDiscarded()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestRemoveTriplesDiscarded(virtuoso);
        }
#endif

        #endregion

        #region Add Graph Tests

        private void TestAddGraphFlushed(IStorageProvider manager)
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

        [Test]
        public void StoragePersistentTripleStoreMemAddGraphFlushed()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestAddGraphFlushed(manager);
        }

#if !NO_SYNC_HTTP
        [Test]
        public void StoragePersistentTripleStoreFusekiAddGraphFlushed()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestAddGraphFlushed(fuseki);
        }
#endif

#if !PORTABLE
        [Test]
        public void StoragePersistentTripleStoreVirtuosoAddGraphFlushed()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestAddGraphFlushed(virtuoso);
        }
#endif
        private void TestAddGraphDiscarded(IStorageProvider manager)
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

        [Test]
        public void StoragePersistentTripleStoreMemAddGraphDiscarded()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestAddGraphDiscarded(manager);
        }

#if !NO_SYNC_HTTP
        [Test]
        public void StoragePersistentTripleStoreFusekiAddGraphDiscarded()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestAddGraphDiscarded(fuseki);
        }
#endif

#if !PORTABLE // No VirtuosoManager in PCL
        [Test]
        public void StoragePersistentTripleStoreVirtuosoAddGraphDiscarded()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestAddGraphDiscarded(virtuoso);
        }
#endif

        #endregion

        #region Remove Graph Tests

        private void TestRemoveGraphFlushed(IStorageProvider manager)
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
                try
                {
                    manager.LoadGraph(handler, toRemove);
                }
                catch
                {

                }
                Assert.IsFalse(handler.Any, "Attempting to load Graph from underlying store should return nothing after the Flush() operation");
            }
            finally
            {
                store.Dispose();
            }
        }

        [Test]
        public void StoragePersistentTripleStoreMemRemoveGraphFlushed()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestRemoveGraphFlushed(manager);
        }

#if !NO_SYNC_HTTP
        [Test]
        public void StoragePersistentTripleStoreFusekiRemoveGraphFlushed()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestRemoveGraphFlushed(fuseki);
        }
#endif

#if !PORTABLE // No VirtuosoManager in PCL
        [Test]
        public void StoragePersistentTripleStoreVirtuosoRemoveGraphFlushed()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestRemoveGraphFlushed(virtuoso);
        }
#endif

        private void TestRemoveGraphDiscarded(IStorageProvider manager)
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

        [Test]
        public void StoragePersistentTripleStoreMemRemoveGraphDiscarded()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestRemoveGraphDiscarded(manager);
        }

#if !NO_SYNC_HTTP
        [Test]
        public void StoragePersistentTripleStoreFusekiRemoveGraphDiscarded()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestRemoveGraphDiscarded(fuseki);
        }
#endif

#if !PORTABLE // No VirtuosoManager in PCL
        [Test]
        public void StoragePersistentTripleStoreVirtuosoRemoveGraphDiscarded()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestRemoveGraphDiscarded(virtuoso);
        }
#endif
        #endregion

        #region Add then Remove Graph Sequencing Tests

        private void TestAddThenRemoveGraphFlushed(IStorageProvider manager)
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

        [Test]
        public void StoragePersistentTripleStoreMemAddThenRemoveGraphFlushed()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestAddThenRemoveGraphFlushed(manager);
        }

#if !NO_SYNC_HTTP
        [Test]
        public void StoragePersistentTripleStoreFusekiAddThenRemoveGraphFlushed()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestAddThenRemoveGraphFlushed(fuseki);
        }
#endif

#if !PORTABLE // No VirtuosoManager in PCL
        [Test]
        public void StoragePersistentTripleStoreVirtuosoAddThenRemoveGraphFlushed()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestAddThenRemoveGraphFlushed(virtuoso);
        }
#endif

        private void TestAddThenRemoveGraphDiscarded(IStorageProvider manager)
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

        [Test]
        public void StoragePersistentTripleStoreMemAddThenRemoveGraphDiscarded()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestAddThenRemoveGraphDiscarded(manager);
        }

#if !NO_SYNC_HTTP
        [Test]
        public void StoragePersistentTripleStoreFusekiAddThenRemoveGraphDiscarded()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestAddThenRemoveGraphDiscarded(fuseki);
        }
#endif

#if !PORTABLE // No VirtuosoManager in PCL
        [Test]
        public void StoragePersistentTripleStoreVirtuosoAddThenRemoveGraphDiscarded()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestAddThenRemoveGraphDiscarded(virtuoso);
        }
#endif
        #endregion

        #region Remove then Add Graph Sequencing Tests

        private void TestRemoveThenAddGraphFlushed(IStorageProvider manager)
        {
            this.EnsureTestDataset(manager);

            PersistentTripleStore store = new PersistentTripleStore(manager);
            try
            {
                Uri toRemove = new Uri(TestGraphUri1);
                IGraph g = store[toRemove];
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

        [Test]
        public void StoragePersistentTripleStoreMemRemoveThenAddGraphFlushed()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestRemoveThenAddGraphFlushed(manager);
        }

#if !NO_SYNC_HTTP
        [Test]
        public void StoragePersistentTripleStoreFusekiRemoveThenAddGraphFlushed()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestRemoveThenAddGraphFlushed(fuseki);
        }
#endif

#if !PORTABLE // No VirtuosoManager in PCL
        [Test]
        public void StoragePersistentTripleStoreVirtuosoRemoveThenAddGraphFlushed()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestRemoveThenAddGraphFlushed(virtuoso);
        }
#endif

        private void TestRemoveThenAddGraphDiscarded(IStorageProvider manager)
        {
            this.EnsureTestDataset(manager);

            PersistentTripleStore store = new PersistentTripleStore(manager);
            try
            {
                Uri toRemove = new Uri(TestGraphUri1);
                IGraph g = store[toRemove];
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

        [Test]
        public void StoragePersistentTripleStoreMemRemoveThenAddGraphDiscarded()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestRemoveThenAddGraphDiscarded(manager);
        }

#if !NO_SYNC_HTTP
        [Test]
        public void StoragePersistentTripleStoreFusekiRemoveThenAddGraphDiscarded()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestRemoveThenAddGraphDiscarded(fuseki);
        }
#endif

#if !PORTABLE // No VirtuosoManager in PCL
        [Test]
        public void StoragePersistentTripleStoreVirtuosoRemoveThenAddGraphDiscarded()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestRemoveThenAddGraphDiscarded(virtuoso);
        }
#endif

        #endregion

        #region SPARQL Query Tests

        private void TestQueryUnsynced(IStorageProvider manager)
        {
            this.EnsureTestDataset(manager);

            PersistentTripleStore store = new PersistentTripleStore(manager);
            try
            {
                store.Remove(new Uri(TestGraphUri1));

                store.ExecuteQuery("SELECT * WHERE { ?s ?p ?o }");
            }
            finally
            {
                store.Discard();
                store.Dispose();
            }
        }

        private void TestQuerySelect(IStorageProvider manager, String query)
        {
            this.EnsureTestDataset(manager);

            PersistentTripleStore store = new PersistentTripleStore(manager);
            try
            {
                SparqlResultSet results = store.ExecuteQuery(query) as SparqlResultSet;
                if (results == null) Assert.Fail("Did not get a SPARQL Result Set as expected");

                TestTools.ShowResults(results);
            }
            finally
            {
                store.Dispose();
            }
        }

        private void TestQueryAsk(IStorageProvider manager, String query, bool expected)
        {
            this.EnsureTestDataset(manager);

            PersistentTripleStore store = new PersistentTripleStore(manager);
            try
            {
                SparqlResultSet results = store.ExecuteQuery(query) as SparqlResultSet;
                if (results == null) Assert.Fail("Did not get a SPARQL Result Set as expected");

                TestTools.ShowResults(results);

                Assert.AreEqual(SparqlResultsType.Boolean, results.ResultsType, "Did not get Boolean result as expected");
                Assert.AreEqual(expected, results.Result, "Boolean Result failed to match");
            }
            finally
            {
                store.Dispose();
            }
        }

        private void TestQueryConstruct(IStorageProvider manager, String query)
        {
            this.EnsureTestDataset(manager);

            PersistentTripleStore store = new PersistentTripleStore(manager);
            try
            {
                IGraph g = store.ExecuteQuery(query) as IGraph;
                if (g == null) Assert.Fail("Did not get a Graph as expected");

                TestTools.ShowResults(g);
            }
            finally
            {
                store.Dispose();
            }
        }

        private void TestQueryDescribe(IStorageProvider manager, String query)
        {
            this.EnsureTestDataset(manager);

            PersistentTripleStore store = new PersistentTripleStore(manager);
            try
            {
                IGraph g = store.ExecuteQuery(query) as IGraph;
                if (g == null) Assert.Fail("Did not get a Graph as expected");

                TestTools.ShowResults(g);
            }
            finally
            {
                store.Dispose();
            }
        }

        [Test,ExpectedException(typeof(RdfQueryException))]
        public void StoragePersistentTripleStoreMemQueryUnsynced()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestQueryUnsynced(manager);
        }

#if !NO_SYNC_HTTP
        [Test, ExpectedException(typeof(RdfQueryException))]
        public void StoragePersistentTripleStoreFusekiQueryUnsynced()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestQueryUnsynced(fuseki);
        }
#endif

#if !PORTABLE // No VirtuosoManager in PCL
        [Test, ExpectedException(typeof(RdfQueryException))]
        public void StoragePersistentTripleStoreVirtuosoQueryUnsynced()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestQueryUnsynced(virtuoso);
        }
#endif

        [Test]
        public void StoragePersistentTripleStoreMemQuerySelect()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestQuerySelect(manager, "SELECT * WHERE { ?s a ?type }");
        }

#if !NO_SYNC_HTTP
        [Test]
        public void StoragePersistentTripleStoreFusekiQuerySelect()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestQuerySelect(fuseki, "SELECT * WHERE { ?s a ?type }");
        }
#endif

#if !PORTABLE // No VirtuosoManager in PCL
        [Test]
        public void StoragePersistentTripleStoreVirtuosoQuerySelect()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestQuerySelect(virtuoso, "SELECT * WHERE { ?s a ?type }");
        }
#endif

        [Test]
        public void StoragePersistentTripleStoreMemQueryAsk()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestQueryAsk(manager, "ASK WHERE { GRAPH ?g { ?s a ?type } }", true);
            this.TestQueryAsk(manager, "ASK WHERE { GRAPH ?g { ?s <http://example.org/noSuchThing> ?o } }", false);
        }

#if !NO_SYNC_HTTP
        [Test]
        public void StoragePersistentTripleStoreFusekiQueryAsk()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestQueryAsk(fuseki, "ASK WHERE { GRAPH ?g { ?s a ?type } }", true);
            this.TestQueryAsk(fuseki, "ASK WHERE { GRAPH ?g { ?s <http://example.org/noSuchThing> ?o } }", false);
        }
#endif

#if !PORTABLE // No VirtuosoManager in PCL
        [Test]
        public void StoragePersistentTripleStoreVirtuosoQueryAsk()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestQueryAsk(virtuoso, "ASK WHERE { ?s a ?type }", true);
            this.TestQueryAsk(virtuoso, "ASK WHERE { ?s <http://example.org/noSuchThing> ?o }", false);
        }
#endif

        [Test]
        public void StoragePersistentTripleStoreMemQueryConstruct()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestQueryConstruct(manager, "CONSTRUCT { ?s a ?type } WHERE { ?s a ?type }");
        }

#if !NO_SYNC_HTTP
        [Test]
        public void StoragePersistentTripleStoreFusekiQueryConstruct()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestQueryConstruct(fuseki, "CONSTRUCT { ?s a ?type } WHERE { ?s a ?type }");
        }
#endif

#if !PORTABLE // No VirtuosoManager in PCL
        [Test]
        public void StoragePersistentTripleStoreVirtuosoQueryConstruct()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestQueryConstruct(virtuoso, "CONSTRUCT { ?s a ?type } WHERE { ?s a ?type }");
        }
#endif

        [Test]
        public void StoragePersistentTripleStoreMemQueryDescribe()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestQueryDescribe(manager, "DESCRIBE ?type WHERE { ?s a ?type } LIMIT 5");
        }

#if !NO_SYNC_HTTP
        [Test]
        public void StoragePersistentTripleStoreFusekiQueryDescribe()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestQueryDescribe(fuseki, "DESCRIBE ?type WHERE { GRAPH ?g { ?s a ?type } } LIMIT 5");
        }
#endif

#if !PORTABLE // No VirtuosoManager in PCL
        [Test]
        public void StoragePersistentTripleStoreVirtuosoQueryDescribe()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestQueryDescribe(virtuoso, "DESCRIBE ?type WHERE { ?s a ?type } LIMIT 5");
        }
#endif

        #endregion

        #region SPARQL Update Tests

        private void TestUpdateUnsynced(IStorageProvider manager)
        {
            this.EnsureTestDataset(manager);

            PersistentTripleStore store = new PersistentTripleStore(manager);
            try
            {
                store.Remove(new Uri(TestGraphUri1));

                store.ExecuteUpdate("LOAD <http://dbpedia.org/resource/Ilkeston>");
            }
            finally
            {
                store.Discard();
                store.Dispose();
            }
        }

        private void TestUpdate(IStorageProvider manager)
        {
            this.EnsureTestDataset(manager);
            Uri updateUri = new Uri("http://example.org/persistence/update/temp");
            this.EnsureGraphDeleted(manager, updateUri);

            PersistentTripleStore store = new PersistentTripleStore(manager);
            try
            {
                Assert.IsFalse(store.HasGraph(updateUri), "Prior to SPARQL Update our target graph should not exist using HasGraph()");
                Assert.IsFalse(store.Graphs.Contains(updateUri), "Prior to SPARQL Update out target graph should not exist using Graphs.Contains()");
                Assert.IsFalse(manager.ListGraphs().Contains(updateUri), "Prior to SPARQL Update our target graph should not exist in the underlying store");

                store.ExecuteUpdate("LOAD <http://dbpedia.org/resource/Ilkeston> INTO GRAPH <" + updateUri.ToString() + ">");

                Assert.IsTrue(store.HasGraph(updateUri), "SPARQL Update should have loaded into our target graph so that HasGraph() returns true");
                Assert.IsTrue(store.Graphs.Contains(updateUri), "SPARQL Update should have loaded into out target graph so that Graphs.Contains() returns true");

                //Note that SPARQL Updates go directly to the underlying store so the change is persisted immediately
                Assert.IsTrue(manager.ListGraphs().Contains(updateUri), "SPARQL Update should loaded into our target graph directly in the underlying store");
            }
            finally
            {
                store.Dispose();
            }
        }

        [Test, ExpectedException(typeof(SparqlUpdateException))]
        public void StoragePersistentTripleStoreMemUpdateUnsynced()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestUpdateUnsynced(manager);
        }

#if !NO_SYNC_HTTP
        [Test, ExpectedException(typeof(SparqlUpdateException))]
        public void StoragePersistentTripleStoreFusekiUpdateUnsynced()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestUpdateUnsynced(fuseki);
        }
#endif

#if !PORTABLE // No VirtuosoManager in PCL
        [Test, ExpectedException(typeof(SparqlUpdateException))]
        public void StoragePersistentTripleStoreVirtuosoUpdateUnsynced()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestUpdateUnsynced(virtuoso);
        }
#endif

        [Test]
        public void StoragePersistentTripleStoreMemUpdate()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestUpdate(manager);
        }

#if !NO_SYNC_HTTP
        [Test]
        public void StoragePersistentTripleStoreFusekiUpdate()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestUpdate(fuseki);
        }
#endif

#if !PORTABLE
        [Test]
        public void StoragePersistentTripleStoreVirtuosoUpdate()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestUpdate(virtuoso);
        }
#endif

        #endregion
    }
}
