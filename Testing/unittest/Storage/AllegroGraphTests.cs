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
using NUnit.Framework;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage;


namespace VDS.RDF.Storage
{
    [TestFixture]
    public class AllegroGraphTests
    {
        public static AllegroGraphConnector GetConnection()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseAllegroGraph))
            {
                Assert.Inconclusive("Test Config marks AllegroGraph as unavailable, cannot run this test");
            }

            return new AllegroGraphConnector(TestConfigManager.GetSetting(TestConfigManager.AllegroGraphServer), TestConfigManager.GetSetting(TestConfigManager.AllegroGraphCatalog), TestConfigManager.GetSetting(TestConfigManager.AllegroGraphRepository), TestConfigManager.GetSetting(TestConfigManager.AllegroGraphUser), TestConfigManager.GetSetting(TestConfigManager.AllegroGraphPassword));
        }

#if !NO_SYNC_HTTP
        // These tests are using the synchronous API

        [SetUp]
        public void Setup()
        {
            Options.HttpDebugging = true;
        }

        [TearDown]
        public void Teardown()
        {
            Options.HttpDebugging = false;
        }

        [Test]
        public void StorageAllegroGraphSaveLoad()
        {
            Graph g = new Graph();
            FileLoader.Load(g, "resources\\InferenceTest.ttl");
            g.BaseUri = new Uri("http://example.org/AllegroGraphTest");

            AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();
            agraph.SaveGraph(g);

            Graph h = new Graph();
            agraph.LoadGraph(h, "http://example.org/AllegroGraphTest");
            Assert.IsFalse(h.IsEmpty, "Graph should not be empty after loading");

            Assert.AreEqual(g, h, "Graphs should have been equal");
        }

        [Test]
        public void StorageAllegroGraphSaveEmptyGraph1()
        {
            Graph g = new Graph();
            g.BaseUri = new Uri("http://example.org/AllegroGraph/empty");

            AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();
            agraph.SaveGraph(g);

            Graph h = new Graph();
            agraph.LoadGraph(h, "http://example.org/AllegroGraph/empty");
            Assert.IsTrue(h.IsEmpty, "Graph should be empty after loading");

            Assert.AreEqual(g, h, "Graphs should have been equal");
        }

        [Test]
        public void StorageAllegroGraphSaveEmptyGraph2()
        {
            AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();
            Uri graphUri = new Uri("http://example.org/AllegroGraph/empty2");
            Console.WriteLine("Deleting any existing graph");
            agraph.DeleteGraph(graphUri);
            Console.WriteLine("Existing graph deleted");

            // First create a non-empty graph
            Graph g = new Graph();
            g.BaseUri = graphUri;
            g.Assert(g.CreateBlankNode(), g.CreateUriNode("rdf:type"), g.CreateUriNode(new Uri("http://example.org/BNode")));
            Console.WriteLine("Saving non-empty graph");
            agraph.SaveGraph(g);
            Console.WriteLine("Non-empty graph saved");

            Graph h = new Graph();
            agraph.LoadGraph(h, graphUri);
            Assert.IsFalse(h.IsEmpty, "Graph should not be empty after loading");

            Assert.AreEqual(g, h, "Graphs should be equal");

            // Now attempt to save an empty graph as well
            g = new Graph();
            g.BaseUri = graphUri;
            Console.WriteLine("Attempting to save empty graph with same name");
            agraph.SaveGraph(g);
            Console.WriteLine("Empty graph saved");

            h = new Graph();
            agraph.LoadGraph(h, graphUri);
            Assert.IsTrue(h.IsEmpty, "Graph should be empty after loading");

            Assert.AreEqual(g, h, "Graphs should have been equal");
        }

        [Test]
        public void StorageAllegroGraphSaveEmptyGraph3()
        {
            AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();
            Uri graphUri = null;
            Console.WriteLine("Deleting any existing graph");
            agraph.DeleteGraph(graphUri);
            Console.WriteLine("Existing graph deleted");

            // First create a non-empty graph
            Graph g = new Graph();
            g.BaseUri = graphUri;
            g.Assert(g.CreateBlankNode(), g.CreateUriNode("rdf:type"), g.CreateUriNode(new Uri("http://example.org/BNode")));
            Console.WriteLine("Saving non-empty graph");
            agraph.SaveGraph(g);
            Console.WriteLine("Non-empty graph saved");

            Graph h = new Graph();
            agraph.LoadGraph(h, graphUri);
            Assert.IsFalse(h.IsEmpty, "Graph should not be empty after loading");

            Assert.AreEqual(g, h, "Graphs should be equal");

            // Now attempt to overwrite with an empty graph
            g = new Graph();
            g.BaseUri = graphUri;
            Console.WriteLine("Attempting to save empty graph with same name");
            agraph.SaveGraph(g);
            Console.WriteLine("Empty graph saved");

            h = new Graph();
            agraph.LoadGraph(h, graphUri);

            // Since saving to default graph does not overwrite the graph we've just retrieved must contain the empty graph as a sub-graph
            Assert.IsTrue(h.HasSubGraph(g));
        }

        [Test]
        public void StorageAllegroGraphDeleteTriples()
        {
            Graph g = new Graph();
            FileLoader.Load(g, "resources\\InferenceTest.ttl");
            g.BaseUri = new Uri("http://example.org/AllegroGraphTest");

            AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();
            agraph.SaveGraph(g);

            Console.WriteLine("Graph before deletion");
            TestTools.ShowGraph(g);

            //Delete all Triples about the Ford Fiesta
            agraph.UpdateGraph(g.BaseUri, null, g.GetTriplesWithSubject(new Uri("http://example.org/vehicles/FordFiesta")));

            Graph h = new Graph();
            agraph.LoadGraph(h, g.BaseUri);

            Console.WriteLine("Graph after deletion");
            TestTools.ShowGraph(h);

            Assert.IsFalse(h.IsEmpty, "Graph should not be completely empty");
            Assert.IsTrue(g.HasSubGraph(h), "Graph retrieved with missing Triples should be a sub-graph of the original Graph");
            Assert.IsFalse(g.Equals(h), "Graph retrieved should not be equal to original Graph");

            Object results = agraph.Query("ASK WHERE { GRAPH <http://example.org/AllegroGraphTest> { <http://example.org/vehicles/FordFiesta> ?p ?o } }");
            if (results is SparqlResultSet)
            {
                Assert.IsFalse(((SparqlResultSet) results).Result, "There should no longer be any triples about the Ford Fiesta present");
            }
        }

        [Test]
        public void StorageAllegroGraphDeleteGraph1()
        {
            AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();
            Uri graphUri = new Uri("http://example.org/AllegroGraph/delete");

            Graph g = new Graph();
            FileLoader.Load(g, "resources\\InferenceTest.ttl");
            g.BaseUri = graphUri;

            agraph.SaveGraph(g);

            Graph h = new Graph();
            agraph.LoadGraph(h, graphUri);
            Assert.IsFalse(h.IsEmpty, "Graph should not be empty after loading");

            Assert.AreEqual(g, h, "Graphs should have been equal");

            agraph.DeleteGraph(graphUri);
            h = new Graph();
            agraph.LoadGraph(h, graphUri);
            Assert.IsTrue(h.IsEmpty, "Graph should be equal after deletion");
            Assert.AreNotEqual(g, h, "Graphs should not be equal after deletion");
        }

        [Test]
        public void StorageAllegroGraphDeleteGraph2()
        {
            AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();
            Uri graphUri = null;
            agraph.DeleteGraph(graphUri);

            Graph g = new Graph();
            FileLoader.Load(g, "resources\\InferenceTest.ttl");
            g.BaseUri = graphUri;

            agraph.SaveGraph(g);

            Graph h = new Graph();
            agraph.LoadGraph(h, graphUri);
            Assert.IsFalse(h.IsEmpty, "Graph should not be empty after loading");

            Assert.AreEqual(g, h, "Graphs should have been equal after loading");

            agraph.DeleteGraph(graphUri);
            h = new Graph();
            agraph.LoadGraph(h, graphUri);
            Assert.IsTrue(h.IsEmpty, "Graph should be equal after deletion");
            Assert.AreNotEqual(g, h, "Graphs should not be equal after deletion");
        }

        [Test]
        public void StorageAllegroGraphAsk()
        {
            AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();

            String ask = "ASK WHERE { ?s ?p ?o }";

            Object results = agraph.Query(ask);
            if (results is SparqlResultSet)
            {
                TestTools.ShowResults(results);
            }
            else
            {
                Assert.Fail("Failed to get a Result Set as expected");
            }
        }

        [Test]
        public void StorageAllegroGraphDescribe()
        {
            AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();

            String describe = "DESCRIBE <http://example.org/Vehicles/FordFiesta>";

            Object results = agraph.Query(describe);
            if (results is IGraph)
            {
                TestTools.ShowGraph((IGraph) results);
            }
            else
            {
                Assert.Fail("Failed to get a Graph as expected");
            }
        }

        [Test]
        public void StorageAllegroGraphSparqlUpdate()
        {
            AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();

            String updates = "INSERT DATA { GRAPH <http://example.org/new-graph> { <http://subject> <http://predicate> <http://object> } }";

            agraph.Update(updates);

            SparqlResultSet results = agraph.Query("SELECT * WHERE { GRAPH <http://example.org/new-graph> { ?s ?p ?o } }") as SparqlResultSet;
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Count);
        }
#endif
    }
}