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
using System.Linq;
using Xunit;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Update;
using VDS.RDF.Writing;
using StringWriter = System.IO.StringWriter;

namespace VDS.RDF.Storage;

public abstract class BasePersistentTripleStoreTests
{
    private const string TestGraphUri1 = "http://example.org/persistence/graphs/1",
                         TestGraphUri2 = "http://example.org/persistence/graphs/2",
                         TestGraphUri3 = "http://example.org/persistence/graphs/3";

    private readonly RdfServerFixture _serverFixture;

    protected BasePersistentTripleStoreTests(RdfServerFixture serverFixture)
    {
        _serverFixture = serverFixture;
    }

    private void EnsureTestDataset(IStorageProvider manager)
    {
        var g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        g.BaseUri = new Uri(TestGraphUri1);
        g.Retract(g.Triples.Where(t => !t.IsGroundTriple).ToList());
        manager.SaveGraph(g);

        g = new Graph();
        g.LoadFromFile(System.IO.Path.Combine("resources", "InferenceTest.ttl"));
        g.BaseUri = new Uri(TestGraphUri2);
        g.Retract(g.Triples.Where(t => !t.IsGroundTriple).ToList());
        manager.SaveGraph(g);

        g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Query.Optimisation.OptimiserStats.ttl, dotNetRdf");
        g.BaseUri = new Uri(TestGraphUri3);
        g.Retract(g.Triples.Where(t => !t.IsGroundTriple).ToList());
        manager.SaveGraph(g);
    }

    private void EnsureGraphDeleted(IStorageProvider manager,  IRefNode graphUri)
    {
        Assert.SkipUnless(manager.DeleteSupported, "Unable to conduct this test as it requires ensuring a Graph is deleted from the underlying store which the IStorageProvider instance does not support");
        manager.DeleteGraph(graphUri.ToString());
    }

    [Fact]
    public void StoragePersistentTripleStoreBadInstantiation()
    {
        Assert.Throws<ArgumentNullException>(() => new PersistentTripleStore(null));
    }

    protected abstract IStorageProvider GetConnection();

    #region Contains Tests

    private void TestContains(IStorageProvider manager)
    {
        EnsureTestDataset(manager);

        var store = new PersistentTripleStore(manager);
        var nodeFactory = new NodeFactory(new NodeFactoryOptions());
        IUriNode testGraph1 = nodeFactory.CreateUriNode(new Uri(TestGraphUri1));
        IUriNode testGraph2 = nodeFactory.CreateUriNode(new Uri(TestGraphUri2));
        IUriNode testGraph3 = nodeFactory.CreateUriNode(new Uri(TestGraphUri1));
        try
        {
            Assert.True(store.HasGraph(testGraph1), "URI 1 should return true for HasGraph()");
            Assert.True(store.Graphs.Contains(testGraph1), "URI 1 should return true for Graphs.Contains()");
            Assert.True(store.HasGraph(testGraph2), "URI 2 should return true for HasGraph()");
            Assert.True(store.Graphs.Contains(testGraph2), "URI 2 should return true for Graphs.Contains()");
            Assert.True(store.HasGraph(testGraph3), "URI 3 should return true for HasGraph()");
            Assert.True(store.Graphs.Contains(testGraph3), "URI 3 should return true for Graphs.Contains()");

            IUriNode noSuchThing = nodeFactory.CreateUriNode(new Uri("http://example.org/persistence/graphs/noSuchGraph"));
            Assert.False(store.HasGraph(noSuchThing), "Bad URI should return false for HasGraph()");
            Assert.False(store.Graphs.Contains(noSuchThing), "Bad URI should return false for Graphs.Contains()");

        }
        finally
        {
            store.Dispose();
        }
    }

    #endregion

    #region Get Graph Tests

    private void TestGetGraph(IStorageProvider manager)
    {
        EnsureTestDataset(manager);

        var store = new PersistentTripleStore(manager);
        var nodeFactory = new NodeFactory(new NodeFactoryOptions());
        try
        {
            var aExpected = new Graph(nodeFactory.CreateUriNode(new Uri(TestGraphUri1)));
            aExpected.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            aExpected.Retract(aExpected.Triples.Where(t => !t.IsGroundTriple).ToList());
            IGraph aActual = store[aExpected.Name];
            Assert.Equal(aExpected, aActual);
            aActual = store.Graphs[aExpected.Name];
            Assert.Equal(aExpected, aActual);

            var bExpected = new Graph(nodeFactory.CreateUriNode(new Uri(TestGraphUri2)));
            bExpected.LoadFromFile(System.IO.Path.Combine("resources", "InferenceTest.ttl"));
            bExpected.Retract(bExpected.Triples.Where(t => !t.IsGroundTriple).ToList());
            IGraph bActual = store[bExpected.Name];
            Assert.Equal(bExpected, bActual);
            bActual = store.Graphs[bExpected.Name];
            Assert.Equal(bExpected, bActual);

            var cExpected = new Graph(nodeFactory.CreateUriNode(new Uri(TestGraphUri3)));
            cExpected.LoadFromEmbeddedResource("VDS.RDF.Query.Optimisation.OptimiserStats.ttl, dotNetRdf");
            cExpected.Retract(cExpected.Triples.Where(t => !t.IsGroundTriple).ToList());
            cExpected.BaseUri = new Uri(TestGraphUri3);
            IGraph cActual = store[cExpected.Name];
            Assert.Equal(cExpected, cActual);
            cActual = store.Graphs[cExpected.Name];
            Assert.Equal(cExpected, cActual);
        }
        finally
        {
            store.Dispose();
        }
    }

    #endregion

    #region Add Triples Tests

    private void TestAddTriplesFlushed(IStorageProvider manager)
    {
        var nodeFactory = new NodeFactory(new NodeFactoryOptions());
        IUriNode testGraphName = nodeFactory.CreateUriNode(new Uri(TestGraphUri1));
        EnsureGraphDeleted(manager, testGraphName);
        EnsureTestDataset(manager);

        var store = new PersistentTripleStore(manager);
        try
        {
            IGraph g = store[testGraphName];

            var toAdd = new Triple(g.CreateUriNode(new Uri("http://example.org/subject")), g.CreateUriNode(new Uri("http://example.org/predicate")), g.CreateUriNode(new Uri("http://example.org/object")));
            g.Assert(toAdd);

            Assert.True(g.ContainsTriple(toAdd), "Added triple should be present in in-memory view prior to Flush/Discard");
            var h = new Graph();
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

    private void TestAddTriplesDiscarded(IStorageProvider manager)
    {
        var nodeFactory = new NodeFactory(new NodeFactoryOptions());
        IUriNode testGraphName = nodeFactory.CreateUriNode(new Uri(TestGraphUri1));
        EnsureGraphDeleted(manager, testGraphName);
        EnsureTestDataset(manager);

        var store = new PersistentTripleStore(manager);
        try
        {
            IGraph g = store[testGraphName];

            var toAdd = new Triple(g.CreateUriNode(new Uri("http://example.org/subject")), g.CreateUriNode(new Uri("http://example.org/predicate")), g.CreateUriNode(new Uri("http://example.org/object")));
            g.Assert(toAdd);

            Assert.True(g.ContainsTriple(toAdd), "Added triple should be present in in-memory view prior to Flush/Discard");
            var h = new Graph();
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

    #endregion

    #region Remove Triples Tests

    private void TestRemoveTriplesFlushed(IStorageProvider manager)
    {
        EnsureTestDataset(manager);

        var store = new PersistentTripleStore(manager);
        var nodeFactory = new NodeFactory(new NodeFactoryOptions());
        try
        {
            IGraph g = store[nodeFactory.CreateUriNode(new Uri(TestGraphUri1))];

            INode rdfType = g.CreateUriNode(new Uri(NamespaceMapper.RDF + "type"));
            g.Retract(g.GetTriplesWithPredicate(rdfType).ToList());

            Assert.False(g.GetTriplesWithPredicate(rdfType).Any(), "Removed triples should not be present in in-memory view prior to Flush/Discard");
            var h = new Graph();
            manager.LoadGraph(h, g.Name.ToString());
            Assert.True(h.GetTriplesWithPredicate(rdfType).Any(), "Removed triples should still be present in underlying store prior to Flush/Discard");

            store.Flush();

            Assert.False(g.GetTriplesWithPredicate(rdfType).Any(), "Removed triples should not be present in in-memory view after Flush");
            h = new Graph();
            manager.LoadGraph(h, g.Name.ToString());
            Assert.False(h.GetTriplesWithPredicate(rdfType).Any(), "Removed triples should no longer be present in underlying store after Flush");

        }
        finally
        {
            store.Dispose();
        }
    }

    private void TestRemoveTriplesDiscarded(IStorageProvider manager)
    {
        EnsureTestDataset(manager);

        var store = new PersistentTripleStore(manager);
        var nodeFactory =new NodeFactory(new NodeFactoryOptions());
        try
        {
            IGraph g = store[nodeFactory.CreateUriNode(new Uri(TestGraphUri1))];

            INode rdfType = g.CreateUriNode(new Uri(NamespaceMapper.RDF + "type"));
            g.Retract(g.GetTriplesWithPredicate(rdfType).ToList());

            Assert.False(g.GetTriplesWithPredicate(rdfType).Any(), "Removed triples should not be present in in-memory view prior to Flush/Discard");
            var h = new Graph();
            manager.LoadGraph(h, g.Name.ToString());
            Assert.True(h.GetTriplesWithPredicate(rdfType).Any(), "Removed triples should still be present in underlying store prior to Flush/Discard");

            store.Discard();

            Assert.True(g.GetTriplesWithPredicate(rdfType).Any(), "Removed triples should now be present in in-memory view after Discard");
            h = new Graph();
            manager.LoadGraph(h, g.Name.ToString());
            Assert.True(h.GetTriplesWithPredicate(rdfType).Any(), "Removed triples should still be present in underlying store after Discard");

        }
        finally
        {
            store.Dispose();
        }
    }

    #endregion

    #region Add Graph Tests

    private void TestAddGraphFlushed(IStorageProvider manager)
    {
        EnsureTestDataset(manager);

        var store = new PersistentTripleStore(manager);
        var nodeFactory = new NodeFactory(new NodeFactoryOptions());
        try
        {
            var g = new Graph(
                nodeFactory.CreateUriNode(new Uri("http://example.org/persistence/graphs/added/flushed")));
            EnsureGraphDeleted(manager, g.Name);
            g.Assert(g.CreateUriNode("rdf:subject"), g.CreateUriNode("rdf:predicate"), g.CreateUriNode("rdf:object"));
            store.Add(g);

            Assert.True(store.HasGraph(g.Name), "Newly added graph should exist in in-memory view of store");
            Assert.False(manager.ListGraphNames().Contains(g.Name.ToString()), "Newly added graph should not yet exist in underlying store");

            store.Flush();

            Assert.True(manager.ListGraphNames().Contains(g.Name.ToString()), "After Flush() is called added graph should exist in underlying store");
        }
        finally
        {
            store.Dispose();
        }
    }

    private void TestAddGraphDiscarded(IStorageProvider manager)
    {
        EnsureTestDataset(manager);

        var store = new PersistentTripleStore(manager);
        var nodeFactory = new NodeFactory(new NodeFactoryOptions());
        try
        {
            var g = new Graph(nodeFactory.CreateUriNode(new Uri("http://example.org/persistence/graphs/added/discarded")));
            EnsureGraphDeleted(manager, g.Name);
            g.Assert(g.CreateUriNode("rdf:subject"), g.CreateUriNode("rdf:predicate"), g.CreateUriNode("rdf:object"));
            store.Add(g);

            Assert.True(store.HasGraph(g.Name), "Newly added graph should exist in in-memory view of store");
            Assert.False(manager.ListGraphNames().Contains(g.Name.ToString()), "Newly added graph should not yet exist in underlying store");

            store.Discard();

            var h = new Graph();
            try
            {
                manager.LoadGraph(h, g.Name.ToString());
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

    #endregion

    #region Remove Graph Tests

    private void TestRemoveGraphFlushed(IStorageProvider manager)
    {
        EnsureTestDataset(manager);

        var store = new PersistentTripleStore(manager);
        var nodeFactory = new NodeFactory(new NodeFactoryOptions());
        try
        {
            IUriNode toRemove = nodeFactory.CreateUriNode(new Uri(TestGraphUri1));
            Assert.True(store.HasGraph(toRemove), "In-memory view should contain the Graph we wish to remove");

            store.Remove(toRemove);
            Assert.False(store.HasGraph(toRemove), "In-memory view should no longer contain the Graph we removed prior to the Flush/Discard operation");
            store.Flush();

            Assert.False(store.HasGraph(toRemove), "In-Memory view should no longer contain the Graph we removed after Flushing");
            var handler = new AnyHandler();
            try
            {
                manager.LoadGraph(handler, toRemove.ToString());
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

    private void TestRemoveGraphDiscarded(IStorageProvider manager)
    {
        EnsureTestDataset(manager);

        var store = new PersistentTripleStore(manager);
        var nodeFactory = new NodeFactory(new NodeFactoryOptions());
        try
        {
            IUriNode toRemove = nodeFactory.CreateUriNode(new Uri(TestGraphUri1));
            Assert.True(store.HasGraph(toRemove), "In-memory view should contain the Graph we wish to remove");

            store.Remove(toRemove);
            Assert.False(store.HasGraph(toRemove), "In-memory view should no longer contain the Graph we removed prior to the Flush/Discard operation");
            store.Discard();

            Assert.True(store.HasGraph(toRemove), "In-Memory view should still contain the Graph we removed as we Discarded that change");
            var handler = new AnyHandler();
            manager.LoadGraph(handler, toRemove.ToString());
            Assert.True(handler.Any, "Attempting to load Graph from underlying store should return something as the Discard() prevented the removal being persisted");
        }
        finally
        {
            store.Dispose();
        }
    }

    #endregion

    #region Add then Remove Graph Sequencing Tests

    private void TestAddThenRemoveGraphFlushed(IStorageProvider manager)
    {
        EnsureTestDataset(manager);

        var store = new PersistentTripleStore(manager);
        var nodeFactory = new NodeFactory(new NodeFactoryOptions());
        try
        {
            var g = new Graph(
                nodeFactory.CreateUriNode(new Uri("http://example.org/persistence/graphs/added/flushed")));
            EnsureGraphDeleted(manager, g.Name);
            g.Assert(g.CreateUriNode("rdf:subject"), g.CreateUriNode("rdf:predicate"), g.CreateUriNode("rdf:object"));
            store.Add(g);

            Assert.True(store.HasGraph(g.Name), "Newly added graph should exist in in-memory view of store");
            Assert.False(manager.ListGraphNames().Contains(g.Name.ToString()), "Newly added graph should not yet exist in underlying store");

            store.Remove(g.Name);
            Assert.False(store.HasGraph(g.Name), "Graph then removed before Flush/Discard() should no longer exist in in-memory view of store");
            Assert.False(manager.ListGraphNames().Contains(g.Name.ToString()), "Graph then removed should still not exist in underlying store");

            store.Flush();

            Assert.False(store.HasGraph(g.Name), "After Flush() is called graph should not exist in in-memory view of store");
            Assert.False(manager.ListGraphNames().Contains(g.Name.ToString()), "After Flush() is called added then removed graph should not exist in underlying store");
        }
        finally
        {
            store.Dispose();
        }
    }

    private void TestAddThenRemoveGraphDiscarded(IStorageProvider manager)
    {
        EnsureTestDataset(manager);

        var store = new PersistentTripleStore(manager);
        var nodeFactory = new NodeFactory(new NodeFactoryOptions());
        try
        {
            var g = new Graph(
                nodeFactory.CreateUriNode(new Uri("http://example.org/persistence/graphs/added/discarded")));
            EnsureGraphDeleted(manager, g.Name);
            g.Assert(g.CreateUriNode("rdf:subject"), g.CreateUriNode("rdf:predicate"), g.CreateUriNode("rdf:object"));
            store.Add(g);

            Assert.True(store.HasGraph(g.Name), "Newly added graph should exist in in-memory view of store");
            Assert.False(manager.ListGraphNames().Contains(g.Name.ToString()), "Newly added graph should not yet exist in underlying store");

            store.Remove(g.Name);
            Assert.False(store.HasGraph(g.Name), "Graph then removed before Flush/Discard() should no longer exist in in-memory view of store");
            Assert.False(manager.ListGraphNames().Contains(g.Name.ToString()), "Graph then removed should still not exist in underlying store");

            store.Discard();

            Assert.False(store.HasGraph(g.Name), "After Discard() is called graph should not exist in in-memory view of store");
            Assert.False(manager.ListGraphNames().Contains(g.Name.ToString()), "After Discard() is called added then removed graph should not exist in underlying store");
        }
        finally
        {
            store.Dispose();
        }
    }

    #endregion

    #region Remove then Add Graph Sequencing Tests

    private void TestRemoveThenAddGraphFlushed(IStorageProvider manager)
    {
        EnsureTestDataset(manager);

        var store = new PersistentTripleStore(manager);
        var nodeFactory = new NodeFactory(new NodeFactoryOptions());
        try
        {
            IUriNode toRemove = nodeFactory.CreateUriNode(new Uri(TestGraphUri1));
            IGraph g = store[toRemove];
            Assert.True(store.HasGraph(toRemove), "In-memory view should contain the Graph we wish to remove");

            store.Remove(toRemove);
            Assert.False(store.HasGraph(toRemove), "In-memory view should no longer contain the Graph we removed prior to the Flush/Discard operation");

            store.Add(g);
            Assert.True(store.HasGraph(toRemove), "In-memory should now contain the Graph we added back");

            store.Flush();

            Assert.True(store.HasGraph(toRemove), "In-Memory view should still contain the Graph we added back after Flushing");
            var handler = new AnyHandler();
            manager.LoadGraph(handler, toRemove.ToString());
            Assert.True(handler.Any, "Attempting to load Graph from underlying store should return something after the Flush() operation since we didn't remove the graph in the end");
        }
        finally
        {
            store.Dispose();
        }
    }

    private void TestRemoveThenAddGraphDiscarded(IStorageProvider manager)
    {
        EnsureTestDataset(manager);

        var store = new PersistentTripleStore(manager);
        var nodeFactory = new NodeFactory(new NodeFactoryOptions());
        try
        {
            IUriNode toRemove = nodeFactory.CreateUriNode(new Uri(TestGraphUri1));
            IGraph g = store[toRemove];
            Assert.True(store.HasGraph(toRemove), "In-memory view should contain the Graph we wish to remove");

            store.Remove(toRemove);
            Assert.False(store.HasGraph(toRemove), "In-memory view should no longer contain the Graph we removed prior to the Flush/Discard operation");

            store.Add(g);
            Assert.True(store.HasGraph(toRemove), "In-memory should now contain the Graph we added back");

            store.Discard();

            Assert.True(store.HasGraph(toRemove), "In-Memory view should still contain the Graph we removed and added back regardless as we Discarded that change");
            var handler = new AnyHandler();
            manager.LoadGraph(handler, toRemove.ToString());
            Assert.True(handler.Any, "Attempting to load Graph from underlying store should return something as the Discard() prevented the removal and add back being persisted");
        }
        finally
        {
            store.Dispose();
        }
    }

    #endregion

    #region SPARQL Query Tests

    private void TestQueryUnsynced(IStorageProvider manager)
    {
        EnsureTestDataset(manager);

        var store = new PersistentTripleStore(manager);
        var nodeFactory = new NodeFactory(new NodeFactoryOptions());
        try
        {
            store.Remove(nodeFactory.CreateUriNode(new Uri(TestGraphUri1)));

            store.ExecuteQuery("SELECT * WHERE { ?s ?p ?o }");
        }
        finally
        {
            store.Discard();
            store.Dispose();
        }
    }

    private void TestQuerySelect(IStorageProvider manager, string query)
    {
        EnsureTestDataset(manager);

        var store = new PersistentTripleStore(manager);
        try
        {
            var results = store.ExecuteQuery(query) as SparqlResultSet;
            if (results == null) Assert.Fail("Did not get a SPARQL Result Set as expected");

            //TestTools.ShowResults(results);
        }
        finally
        {
            store.Dispose();
        }
    }

    private void TestQueryAsk(IStorageProvider manager, String query, bool expected)
    {
        EnsureTestDataset(manager);

        var store = new PersistentTripleStore(manager);
        try
        {
            var results = store.ExecuteQuery(query) as SparqlResultSet;
            if (results == null) Assert.Fail("Did not get a SPARQL Result Set as expected");

            //TestTools.ShowResults(results);

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
        EnsureTestDataset(manager);

        var store = new PersistentTripleStore(manager);
        try
        {
            var g = store.ExecuteQuery(query) as IGraph;
            if (g == null) Assert.Fail("Did not get a Graph as expected");

            //TestTools.ShowResults(g);
        }
        finally
        {
            store.Dispose();
        }
    }

    private void TestQueryDescribe(IStorageProvider manager, String query)
    {
        EnsureTestDataset(manager);

        var store = new PersistentTripleStore(manager);
        try
        {
            var g = store.ExecuteQuery(query) as IGraph;
            if (g == null) Assert.Fail("Did not get a Graph as expected");

            //TestTools.ShowResults(g);
        }
        finally
        {
            store.Dispose();
        }
    }

    #endregion

    #region SPARQL Update Tests

    private void TestUpdateUnsynced(IStorageProvider manager)
    {
        EnsureTestDataset(manager);

        var store = new PersistentTripleStore(manager);
        var nodeFactory = new NodeFactory(new NodeFactoryOptions());
        try
        {
            store.Remove(nodeFactory.CreateUriNode(new Uri(TestGraphUri1)));
            store.ExecuteUpdate($"LOAD <{_serverFixture.UriFor("/resource/Southampton")}>");
        }
        finally
        {
            store.Discard();
            store.Dispose();
        }
    }

    private void TestUpdate(IStorageProvider manager)
    {
        EnsureTestDataset(manager);
        var nodeFactory = new NodeFactory(new NodeFactoryOptions());
        IUriNode updateUri = nodeFactory.CreateUriNode(new Uri("http://example.org/persistence/update/temp"));
        EnsureGraphDeleted(manager, updateUri);

        var store = new PersistentTripleStore(manager);
        try
        {
            Assert.False(store.HasGraph(updateUri), "Prior to SPARQL Update our target graph should not exist using HasGraph()");
            Assert.False(store.Graphs.Contains(updateUri), "Prior to SPARQL Update out target graph should not exist using Graphs.Contains()");
            Assert.False(manager.ListGraphNames().Contains(updateUri.ToString()), "Prior to SPARQL Update our target graph should not exist in the underlying store");

            store.ExecuteUpdate("LOAD <http://dbpedia.org/resource/Ilkeston> INTO GRAPH <" + updateUri.ToString() + ">");

            Assert.True(store.HasGraph(updateUri), "SPARQL Update should have loaded into our target graph so that HasGraph() returns true");
            Assert.True(store.Graphs.Contains(updateUri), "SPARQL Update should have loaded into out target graph so that Graphs.Contains() returns true");

            //Note that SPARQL Updates go directly to the underlying store so the change is persisted immediately
            Assert.True(manager.ListGraphNames().Contains(updateUri.ToString()), "SPARQL Update should loaded into our target graph directly in the underlying store");
        }
        finally
        {
            store.Dispose();
        }
    }

    #endregion

    #region Dump persistent store tests
    
    private void TestDumpStoreEmpty(IStorageProvider manager)
    {
        EnsureTestDataset(manager);

        var store = new PersistentTripleStore(manager);
        try
        {
            // Try and dump
            var strWriter = new StringWriter();
            var writer = new TriGWriter
            {
                UseMultiThreadedWriting = false
            };

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
        EnsureTestDataset(manager);

        var store = new PersistentTripleStore(manager);
        var nodeFactory = new NodeFactory(new NodeFactoryOptions());
        try
        {
            // First prime the persistent store by loading a bunch of stuff
            IUriNode testGraphName1 = nodeFactory.CreateUriNode(new Uri(TestGraphUri1));
            IUriNode testGraphName2 = nodeFactory.CreateUriNode(new Uri(TestGraphUri2));
            IUriNode testGraphName3 = nodeFactory.CreateUriNode(new Uri(TestGraphUri3));
            Assert.True(store.HasGraph(testGraphName1), "URI 1 should return true for HasGraph()");
            Assert.True(store.Graphs.Contains(testGraphName1), "URI 1 should return true for Graphs.Contains()");
            Assert.True(store.HasGraph(testGraphName2), "URI 2 should return true for HasGraph()");
            Assert.True(store.Graphs.Contains(testGraphName2), "URI 2 should return true for Graphs.Contains()");
            Assert.True(store.HasGraph(testGraphName3), "URI 3 should return true for HasGraph()");
            Assert.True(store.Graphs.Contains(testGraphName3), "URI 3 should return true for Graphs.Contains()");

            IRefNode noSuchThing = nodeFactory.CreateUriNode(new Uri("http://example.org/persistence/graphs/noSuchGraph"));
            Assert.False(store.HasGraph(noSuchThing), "Bad URI should return false for HasGraph()");
            Assert.False(store.Graphs.Contains(noSuchThing), "Bad URI should return false for Graphs.Contains()");

            // Then try and dump
            var strWriter = new StringWriter();
            var writer = new TriGWriter
            {
                UseMultiThreadedWriting = false
            };

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

    #endregion

    [Fact]
    public void StoragePersistentTripleStoreFusekiContains()
    {
        IStorageProvider fuseki = GetConnection();
        TestContains(fuseki);
    }

    [Fact]
    public void StoragePersistentTripleStoreFusekiGetGraph()
    {
        IStorageProvider fuseki = GetConnection();
        TestGetGraph(fuseki);
    }

    [Fact]
    public void StoragePersistentTripleStoreFusekiAddTriplesFlushed()
    {
        IStorageProvider fuseki = GetConnection();
        TestAddTriplesFlushed(fuseki);
    }

    [Fact]
    public void StoragePersistentTripleStoreFusekiAddTriplesDiscarded()
    {
        IStorageProvider fuseki = GetConnection();
        TestAddTriplesDiscarded(fuseki);
    }


    [Fact]
    public void StoragePersistentTripleStoreFusekiRemoveTriplesFlushed()
    {
        IStorageProvider fuseki = GetConnection();
        TestRemoveTriplesFlushed(fuseki);
    }

    [Fact]
    public void StoragePersistentTripleStoreFusekiRemoveTriplesDiscarded()
    {
        IStorageProvider fuseki = GetConnection();
        TestRemoveTriplesDiscarded(fuseki);
    }


    [Fact]
    public void StoragePersistentTripleStoreFusekiAddGraphFlushed()
    {
        IStorageProvider fuseki = GetConnection();
        TestAddGraphFlushed(fuseki);
    }

    [Fact]
    public void StoragePersistentTripleStoreFusekiAddGraphDiscarded()
    {
        IStorageProvider fuseki = GetConnection();
        TestAddGraphDiscarded(fuseki);
    }


    [Fact]
    public void StoragePersistentTripleStoreFusekiRemoveGraphFlushed()
    {
        IStorageProvider fuseki = GetConnection();
        TestRemoveGraphFlushed(fuseki);
    }

    [Fact]
    public void StoragePersistentTripleStoreFusekiRemoveGraphDiscarded()
    {
        IStorageProvider fuseki = GetConnection();
        TestRemoveGraphDiscarded(fuseki);
    }

    [Fact]
    public void StoragePersistentTripleStoreFusekiAddThenRemoveGraphFlushed()
    {
        IStorageProvider fuseki = GetConnection();
        TestAddThenRemoveGraphFlushed(fuseki);
    }


    [Fact]
    public void StoragePersistentTripleStoreFusekiAddThenRemoveGraphDiscarded()
    {
        IStorageProvider fuseki = GetConnection();
        TestAddThenRemoveGraphDiscarded(fuseki);
    }

    [Fact]
    public void StoragePersistentTripleStoreFusekiRemoveThenAddGraphFlushed()
    {
        IStorageProvider fuseki = GetConnection();
        TestRemoveThenAddGraphFlushed(fuseki);
    }

    [Fact]
    public void StoragePersistentTripleStoreFusekiRemoveThenAddGraphDiscarded()
    {
        IStorageProvider fuseki = GetConnection();
        TestRemoveThenAddGraphDiscarded(fuseki);
    }

    [Fact]
    public void StoragePersistentTripleStoreFusekiQueryUnsynced()
    {
        IStorageProvider fuseki = GetConnection();
        Assert.Throws<RdfQueryException>(() => TestQueryUnsynced(fuseki));
    }

    [Fact]
    public void StoragePersistentTripleStoreFusekiQuerySelect()
    {
        IStorageProvider fuseki = GetConnection();
        TestQuerySelect(fuseki, "SELECT * WHERE { ?s a ?type }");
    }

    [Fact]
    public void StoragePersistentTripleStoreFusekiQueryAsk()
    {
        IStorageProvider fuseki = GetConnection();
        TestQueryAsk(fuseki, "ASK WHERE { GRAPH ?g { ?s a ?type } }", true);
        TestQueryAsk(fuseki, "ASK WHERE { GRAPH ?g { ?s <http://example.org/noSuchThing> ?o } }", false);
    }

    [Fact]
    public void StoragePersistentTripleStoreFusekiQueryConstruct()
    {
        IStorageProvider fuseki = GetConnection();
        TestQueryConstruct(fuseki, "CONSTRUCT { ?s a ?type } WHERE { ?s a ?type }");
    }

    [Fact]
    public void StoragePersistentTripleStoreFusekiQueryDescribe()
    {
        IStorageProvider fuseki = GetConnection();
        TestQueryDescribe(fuseki, "DESCRIBE ?type WHERE { GRAPH ?g { ?s a ?type } } LIMIT 5");
    }

    [Fact]
    public void StoragePersistentTripleStoreFusekiUpdateUnsynced()
    {
        IStorageProvider fuseki = GetConnection();
        Assert.Throws<SparqlUpdateException>(() => TestUpdateUnsynced(fuseki));
    }

    [Fact]
    public void StoragePersistentTripleStoreFusekiUpdate()
    {
        IStorageProvider fuseki = GetConnection();
        TestUpdate(fuseki);
    }


}

