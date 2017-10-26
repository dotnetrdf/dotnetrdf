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
using Xunit;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using VDS.RDF.Update;
using VDS.RDF.Writing;
using StringWriter = System.IO.StringWriter;
using VDS.RDF.XunitExtensions;

namespace VDS.RDF.Storage
{
    public partial class PersistentTripleStoreTests
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
                throw new SkipTestException("Unable to conduct this test as it requires ensuring a Graph is deleted from the underlying store which the IStorageProvider instance does not support");
            }
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreBadInstantiation()
        {
            Assert.Throws<ArgumentNullException>(() => new PersistentTripleStore(null));
        }

        #region Contains Tests

        private void TestContains(IStorageProvider manager)
        {
            this.EnsureTestDataset(manager);

            PersistentTripleStore store = new PersistentTripleStore(manager);
            try
            {
                Assert.True(store.HasGraph(new Uri(TestGraphUri1)), "URI 1 should return true for HasGraph()");
                Assert.True(store.Graphs.Contains(new Uri(TestGraphUri1)), "URI 1 should return true for Graphs.Contains()");
                Assert.True(store.HasGraph(new Uri(TestGraphUri2)), "URI 2 should return true for HasGraph()");
                Assert.True(store.Graphs.Contains(new Uri(TestGraphUri2)), "URI 2 should return true for Graphs.Contains()");
                Assert.True(store.HasGraph(new Uri(TestGraphUri3)), "URI 3 should return true for HasGraph()");
                Assert.True(store.Graphs.Contains(new Uri(TestGraphUri3)), "URI 3 should return true for Graphs.Contains()");

                Uri noSuchThing = new Uri("http://example.org/persistence/graphs/noSuchGraph");
                Assert.False(store.HasGraph(noSuchThing), "Bad URI should return false for HasGraph()");
                Assert.False(store.Graphs.Contains(noSuchThing), "Bad URI should return false for Graphs.Contains()");

            }
            finally
            {
                store.Dispose();
            }
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreMemContains()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestContains(manager);
        }

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
                Assert.Equal(aExpected, aActual);
                aActual = store.Graphs[aExpected.BaseUri];
                Assert.Equal(aExpected, aActual);

                Graph bExpected = new Graph();
                bExpected.LoadFromFile("resources\\InferenceTest.ttl");
                bExpected.Retract(bExpected.Triples.Where(t => !t.IsGroundTriple).ToList());
                bExpected.BaseUri = new Uri(TestGraphUri2);
                IGraph bActual = store[bExpected.BaseUri];
                Assert.Equal(bExpected, bActual);
                bActual = store.Graphs[bExpected.BaseUri];
                Assert.Equal(bExpected, bActual);

                Graph cExpected = new Graph();
                cExpected.LoadFromEmbeddedResource("VDS.RDF.Query.Optimisation.OptimiserStats.ttl");
                cExpected.Retract(cExpected.Triples.Where(t => !t.IsGroundTriple).ToList());
                cExpected.BaseUri = new Uri(TestGraphUri3);
                IGraph cActual = store[cExpected.BaseUri];
                Assert.Equal(cExpected, cActual);
                cActual = store.Graphs[cExpected.BaseUri];
                Assert.Equal(cExpected, cActual);
            }
            finally
            {
                store.Dispose();
            }
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreMemGetGraph()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestGetGraph(manager);
        }

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

                Assert.True(g.ContainsTriple(toAdd), "Added triple should be present in in-memory view prior to Flush/Discard");
                Graph h = new Graph();
                manager.LoadGraph(h, g.BaseUri);
                Assert.False(h.ContainsTriple(toAdd), "Added triple should not be present in underlying store prior to Flush/Discard");

                store.Flush();

                Assert.True(g.ContainsTriple(toAdd), "Added triple should be present in in-memory view after Flush");
                h = new Graph();
                manager.LoadGraph(h, g.BaseUri);
                Assert.True(h.ContainsTriple(toAdd), "Added triple should be present in underlying store after Flush");
            }
            finally
            {
                store.Dispose();
            }
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreMemAddTriplesFlushed()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestAddTriplesFlushed(manager);
        }

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

                Assert.True(g.ContainsTriple(toAdd), "Added triple should be present in in-memory view prior to Flush/Discard");
                Graph h = new Graph();
                manager.LoadGraph(h, g.BaseUri);
                Assert.False(h.ContainsTriple(toAdd), "Added triple should not be present in underlying store prior to Flush/Discard");

                store.Discard();

                Assert.False(g.ContainsTriple(toAdd), "Added triple should not be present in in-memory view after Discard");
                h = new Graph();
                manager.LoadGraph(h, g.BaseUri);
                Assert.False(h.ContainsTriple(toAdd), "Added triple should not be present in underlying store after Discard");
            }
            finally
            {
                store.Dispose();
            }
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreMemAddTriplesDiscarded()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestAddTriplesDiscarded(manager);
        }

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

                Assert.False(g.GetTriplesWithPredicate(rdfType).Any(), "Removed triples should not be present in in-memory view prior to Flush/Discard");
                Graph h = new Graph();
                manager.LoadGraph(h, g.BaseUri);
                Assert.True(h.GetTriplesWithPredicate(rdfType).Any(), "Removed triples should still be present in underlying store prior to Flush/Discard");

                store.Flush();

                Assert.False(g.GetTriplesWithPredicate(rdfType).Any(), "Removed triples should not be present in in-memory view after Flush");
                h = new Graph();
                manager.LoadGraph(h, g.BaseUri);
                Assert.False(h.GetTriplesWithPredicate(rdfType).Any(), "Removed triples should no longer be present in underlying store after Flush");

            }
            finally
            {
                store.Dispose();
            }
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreMemRemoveTriplesFlushed()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestRemoveTriplesFlushed(manager);
        }

        private void TestRemoveTriplesDiscarded(IStorageProvider manager)
        {
            this.EnsureTestDataset(manager);

            PersistentTripleStore store = new PersistentTripleStore(manager);
            try
            {
                IGraph g = store[new Uri(TestGraphUri1)];

                INode rdfType = g.CreateUriNode(new Uri(NamespaceMapper.RDF + "type"));
                g.Retract(g.GetTriplesWithPredicate(rdfType).ToList());

                Assert.False(g.GetTriplesWithPredicate(rdfType).Any(), "Removed triples should not be present in in-memory view prior to Flush/Discard");
                Graph h = new Graph();
                manager.LoadGraph(h, g.BaseUri);
                Assert.True(h.GetTriplesWithPredicate(rdfType).Any(), "Removed triples should still be present in underlying store prior to Flush/Discard");

                store.Discard();

                Assert.True(g.GetTriplesWithPredicate(rdfType).Any(), "Removed triples should now be present in in-memory view after Discard");
                h = new Graph();
                manager.LoadGraph(h, g.BaseUri);
                Assert.True(h.GetTriplesWithPredicate(rdfType).Any(), "Removed triples should still be present in underlying store after Discard");

            }
            finally
            {
                store.Dispose();
            }
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreMemRemoveTriplesDiscarded()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestRemoveTriplesDiscarded(manager);
        }

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

                Assert.True(store.HasGraph(g.BaseUri), "Newly added graph should exist in in-memory view of store");
                Assert.False(manager.ListGraphs().Contains(g.BaseUri), "Newly added graph should not yet exist in underlying store");

                store.Flush();

                Assert.True(manager.ListGraphs().Contains(g.BaseUri), "After Flush() is called added graph should exist in underlying store");
            }
            finally
            {
                store.Dispose();
            }
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreMemAddGraphFlushed()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestAddGraphFlushed(manager);
        }

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

                Assert.True(store.HasGraph(g.BaseUri), "Newly added graph should exist in in-memory view of store");
                Assert.False(manager.ListGraphs().Contains(g.BaseUri), "Newly added graph should not yet exist in underlying store");

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
                Assert.True(h.IsEmpty, "After Discard() is called a graph may exist in the underlying store but it MUST be empty");
            }
            finally
            {
                store.Dispose();
            }
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreMemAddGraphDiscarded()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestAddGraphDiscarded(manager);
        }

        #endregion

        #region Remove Graph Tests

        private void TestRemoveGraphFlushed(IStorageProvider manager)
        {
            this.EnsureTestDataset(manager);

            PersistentTripleStore store = new PersistentTripleStore(manager);
            try
            {
                Uri toRemove = new Uri(TestGraphUri1);
                Assert.True(store.HasGraph(toRemove), "In-memory view should contain the Graph we wish to remove");

                store.Remove(toRemove);
                Assert.False(store.HasGraph(toRemove), "In-memory view should no longer contain the Graph we removed prior to the Flush/Discard operation");
                store.Flush();

                Assert.False(store.HasGraph(toRemove), "In-Memory view should no longer contain the Graph we removed after Flushing");
                AnyHandler handler = new AnyHandler();
                try
                {
                    manager.LoadGraph(handler, toRemove);
                }
                catch
                {

                }
                Assert.False(handler.Any, "Attempting to load Graph from underlying store should return nothing after the Flush() operation");
            }
            finally
            {
                store.Dispose();
            }
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreMemRemoveGraphFlushed()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestRemoveGraphFlushed(manager);
        }

        private void TestRemoveGraphDiscarded(IStorageProvider manager)
        {
            this.EnsureTestDataset(manager);

            PersistentTripleStore store = new PersistentTripleStore(manager);
            try
            {
                Uri toRemove = new Uri(TestGraphUri1);
                Assert.True(store.HasGraph(toRemove), "In-memory view should contain the Graph we wish to remove");

                store.Remove(toRemove);
                Assert.False(store.HasGraph(toRemove), "In-memory view should no longer contain the Graph we removed prior to the Flush/Discard operation");
                store.Discard();

                Assert.True(store.HasGraph(toRemove), "In-Memory view should still contain the Graph we removed as we Discarded that change");
                AnyHandler handler = new AnyHandler();
                manager.LoadGraph(handler, toRemove);
                Assert.True(handler.Any, "Attempting to load Graph from underlying store should return something as the Discard() prevented the removal being persisted");
            }
            finally
            {
                store.Dispose();
            }
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreMemRemoveGraphDiscarded()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestRemoveGraphDiscarded(manager);
        }

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

                Assert.True(store.HasGraph(g.BaseUri), "Newly added graph should exist in in-memory view of store");
                Assert.False(manager.ListGraphs().Contains(g.BaseUri), "Newly added graph should not yet exist in underlying store");

                store.Remove(g.BaseUri);
                Assert.False(store.HasGraph(g.BaseUri), "Graph then removed before Flush/Discard() should no longer exist in in-memory view of store");
                Assert.False(manager.ListGraphs().Contains(g.BaseUri), "Graph then removed should still not exist in underlying store");

                store.Flush();

                Assert.False(store.HasGraph(g.BaseUri), "After Flush() is called graph should not exist in in-memory view of store");
                Assert.False(manager.ListGraphs().Contains(g.BaseUri), "After Flush() is called added then removed graph should not exist in underlying store");
            }
            finally
            {
                store.Dispose();
            }
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreMemAddThenRemoveGraphFlushed()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestAddThenRemoveGraphFlushed(manager);
        }

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

                Assert.True(store.HasGraph(g.BaseUri), "Newly added graph should exist in in-memory view of store");
                Assert.False(manager.ListGraphs().Contains(g.BaseUri), "Newly added graph should not yet exist in underlying store");

                store.Remove(g.BaseUri);
                Assert.False(store.HasGraph(g.BaseUri), "Graph then removed before Flush/Discard() should no longer exist in in-memory view of store");
                Assert.False(manager.ListGraphs().Contains(g.BaseUri), "Graph then removed should still not exist in underlying store");

                store.Discard();

                Assert.False(store.HasGraph(g.BaseUri), "After Discard() is called graph should not exist in in-memory view of store");
                Assert.False(manager.ListGraphs().Contains(g.BaseUri), "After Discard() is called added then removed graph should not exist in underlying store");
            }
            finally
            {
                store.Dispose();
            }
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreMemAddThenRemoveGraphDiscarded()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestAddThenRemoveGraphDiscarded(manager);
        }

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
                Assert.True(store.HasGraph(toRemove), "In-memory view should contain the Graph we wish to remove");

                store.Remove(toRemove);
                Assert.False(store.HasGraph(toRemove), "In-memory view should no longer contain the Graph we removed prior to the Flush/Discard operation");

                store.Add(g);
                Assert.True(store.HasGraph(toRemove), "In-memory should now contain the Graph we added back");

                store.Flush();

                Assert.True(store.HasGraph(toRemove), "In-Memory view should still contain the Graph we added back after Flushing");
                AnyHandler handler = new AnyHandler();
                manager.LoadGraph(handler, toRemove);
                Assert.True(handler.Any, "Attempting to load Graph from underlying store should return something after the Flush() operation since we didn't remove the graph in the end");
            }
            finally
            {
                store.Dispose();
            }
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreMemRemoveThenAddGraphFlushed()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestRemoveThenAddGraphFlushed(manager);
        }

        private void TestRemoveThenAddGraphDiscarded(IStorageProvider manager)
        {
            this.EnsureTestDataset(manager);

            PersistentTripleStore store = new PersistentTripleStore(manager);
            try
            {
                Uri toRemove = new Uri(TestGraphUri1);
                IGraph g = store[toRemove];
                Assert.True(store.HasGraph(toRemove), "In-memory view should contain the Graph we wish to remove");

                store.Remove(toRemove);
                Assert.False(store.HasGraph(toRemove), "In-memory view should no longer contain the Graph we removed prior to the Flush/Discard operation");

                store.Add(g);
                Assert.True(store.HasGraph(toRemove), "In-memory should now contain the Graph we added back");

                store.Discard();

                Assert.True(store.HasGraph(toRemove), "In-Memory view should still contain the Graph we removed and added back regardless as we Discarded that change");
                AnyHandler handler = new AnyHandler();
                manager.LoadGraph(handler, toRemove);
                Assert.True(handler.Any, "Attempting to load Graph from underlying store should return something as the Discard() prevented the removal and add back being persisted");
            }
            finally
            {
                store.Dispose();
            }
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreMemRemoveThenAddGraphDiscarded()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestRemoveThenAddGraphDiscarded(manager);
        }

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
                if (results == null) Assert.True(false, "Did not get a SPARQL Result Set as expected");

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
                if (results == null) Assert.True(false, "Did not get a SPARQL Result Set as expected");

                TestTools.ShowResults(results);

                Assert.Equal(SparqlResultsType.Boolean, results.ResultsType);
                Assert.Equal(expected, results.Result);
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
                if (g == null) Assert.True(false, "Did not get a Graph as expected");

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
                if (g == null) Assert.True(false, "Did not get a Graph as expected");

                TestTools.ShowResults(g);
            }
            finally
            {
                store.Dispose();
            }
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreMemQueryUnsynced()
        {
            InMemoryManager manager = new InMemoryManager();
            Assert.Throws<RdfQueryException>(() => this.TestQueryUnsynced(manager));
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreMemQuerySelect()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestQuerySelect(manager, "SELECT * WHERE { ?s a ?type }");
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreMemQueryAsk()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestQueryAsk(manager, "ASK WHERE { GRAPH ?g { ?s a ?type } }", true);
            this.TestQueryAsk(manager, "ASK WHERE { GRAPH ?g { ?s <http://example.org/noSuchThing> ?o } }", false);
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreMemQueryConstruct()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestQueryConstruct(manager, "CONSTRUCT { ?s a ?type } WHERE { ?s a ?type }");
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreMemQueryDescribe()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestQueryDescribe(manager, "DESCRIBE ?type WHERE { ?s a ?type } LIMIT 5");
        }

        #endregion

        #region SPARQL Update Tests

        private void TestUpdateUnsynced(IStorageProvider manager)
        {
            this.EnsureTestDataset(manager);

            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseRemoteParsing))
            {
                throw new SkipTestException("Test Config marks Remote Parsing as unavailable, test cannot be run");
            }

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
                Assert.False(store.HasGraph(updateUri), "Prior to SPARQL Update our target graph should not exist using HasGraph()");
                Assert.False(store.Graphs.Contains(updateUri), "Prior to SPARQL Update out target graph should not exist using Graphs.Contains()");
                Assert.False(manager.ListGraphs().Contains(updateUri), "Prior to SPARQL Update our target graph should not exist in the underlying store");

                store.ExecuteUpdate("LOAD <http://dbpedia.org/resource/Ilkeston> INTO GRAPH <" + updateUri.ToString() + ">");

                Assert.True(store.HasGraph(updateUri), "SPARQL Update should have loaded into our target graph so that HasGraph() returns true");
                Assert.True(store.Graphs.Contains(updateUri), "SPARQL Update should have loaded into out target graph so that Graphs.Contains() returns true");

                //Note that SPARQL Updates go directly to the underlying store so the change is persisted immediately
                Assert.True(manager.ListGraphs().Contains(updateUri), "SPARQL Update should loaded into our target graph directly in the underlying store");
            }
            finally
            {
                store.Dispose();
            }
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreMemUpdateUnsynced()
        {
            InMemoryManager manager = new InMemoryManager();
            Assert.Throws<SparqlUpdateException>(() => this.TestUpdateUnsynced(manager));
        }

        #endregion

        #region Dump persistent store tests
        
        private void TestDumpStoreEmpty(IStorageProvider manager)
        {
            this.EnsureTestDataset(manager);

            PersistentTripleStore store = new PersistentTripleStore(manager);
            try
            {
                // Try and dump
                StringWriter strWriter = new StringWriter();
                TriGWriter writer = new TriGWriter();
                writer.UseMultiThreadedWriting = false;

                writer.Save(store, strWriter);
                Console.WriteLine("TriG output:");
                Console.WriteLine(strWriter.ToString());
                Assert.True(String.IsNullOrEmpty(strWriter.ToString()));
            }
            finally
            {
                store.Dispose();
            }
        }

        private void TestDumpStorePrimed(IStorageProvider manager)
        {
            this.EnsureTestDataset(manager);

            PersistentTripleStore store = new PersistentTripleStore(manager);
            try
            {
                // First prime the persistent store by loading a bunch of stuff
                Assert.True(store.HasGraph(new Uri(TestGraphUri1)), "URI 1 should return true for HasGraph()");
                Assert.True(store.Graphs.Contains(new Uri(TestGraphUri1)), "URI 1 should return true for Graphs.Contains()");
                Assert.True(store.HasGraph(new Uri(TestGraphUri2)), "URI 2 should return true for HasGraph()");
                Assert.True(store.Graphs.Contains(new Uri(TestGraphUri2)), "URI 2 should return true for Graphs.Contains()");
                Assert.True(store.HasGraph(new Uri(TestGraphUri3)), "URI 3 should return true for HasGraph()");
                Assert.True(store.Graphs.Contains(new Uri(TestGraphUri3)), "URI 3 should return true for Graphs.Contains()");

                Uri noSuchThing = new Uri("http://example.org/persistence/graphs/noSuchGraph");
                Assert.False(store.HasGraph(noSuchThing), "Bad URI should return false for HasGraph()");
                Assert.False(store.Graphs.Contains(noSuchThing), "Bad URI should return false for Graphs.Contains()");

                // Then try and dump
                StringWriter strWriter = new StringWriter();
                TriGWriter writer = new TriGWriter();
                writer.UseMultiThreadedWriting = false;

                writer.Save(store, strWriter);
                Console.WriteLine("TriG output:");
                Console.WriteLine(strWriter.ToString());
                Assert.False(String.IsNullOrEmpty(strWriter.ToString()));
            }
            finally
            {
                store.Dispose();
            }
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreMemDump1()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestDumpStoreEmpty(manager);
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreMemDump2()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestDumpStorePrimed(manager);
        }

        #endregion
    }
}

