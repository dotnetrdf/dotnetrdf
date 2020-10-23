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
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage;

namespace VDS.RDF.Storage
{

    public class AllegroGraphTests
    {
        public static AllegroGraphConnector GetConnection()
        {
            Skip.IfNot(TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseAllegroGraph), "Test Config marks AllegroGraph as unavailable, cannot run this test");

            return new AllegroGraphConnector(TestConfigManager.GetSetting(TestConfigManager.AllegroGraphServer), TestConfigManager.GetSetting(TestConfigManager.AllegroGraphCatalog), TestConfigManager.GetSetting(TestConfigManager.AllegroGraphRepository), TestConfigManager.GetSetting(TestConfigManager.AllegroGraphUser), TestConfigManager.GetSetting(TestConfigManager.AllegroGraphPassword));
        }

        // These tests are using the synchronous API

        [SkippableFact]
        public void StorageAllegroGraphSaveLoad()
        {
            var g = new Graph();
            FileLoader.Load(g, "resources\\InferenceTest.ttl");
            g.BaseUri = new Uri("http://example.org/AllegroGraphTest");

            AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();
            agraph.SaveGraph(g);

            var h = new Graph();
            agraph.LoadGraph(h, "http://example.org/AllegroGraphTest");
            Assert.False(h.IsEmpty, "Graph should not be empty after loading");

            Assert.Equal(g, h);
        }

        [SkippableFact]
        public void StorageAllegroGraphSaveEmptyGraph1()
        {
            var g = new Graph
            {
                BaseUri = new Uri("http://example.org/AllegroGraph/empty")
            };

            AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();
            agraph.SaveGraph(g);

            var h = new Graph();
            agraph.LoadGraph(h, "http://example.org/AllegroGraph/empty");
            Assert.True(h.IsEmpty, "Graph should be empty after loading");

            Assert.Equal(g, h);
        }

        [SkippableFact]
        public void StorageAllegroGraphSaveEmptyGraph2()
        {
            AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();
            var graphUri = new Uri("http://example.org/AllegroGraph/empty2");
            Console.WriteLine("Deleting any existing graph");
            agraph.DeleteGraph(graphUri);
            Console.WriteLine("Existing graph deleted");

            // First create a non-empty graph
            var g = new Graph
            {
                BaseUri = graphUri
            };
            g.Assert(g.CreateBlankNode(), g.CreateUriNode("rdf:type"), g.CreateUriNode(new Uri("http://example.org/BNode")));
            Console.WriteLine("Saving non-empty graph");
            agraph.SaveGraph(g);
            Console.WriteLine("Non-empty graph saved");

            var h = new Graph();
            agraph.LoadGraph(h, graphUri);
            Assert.False(h.IsEmpty, "Graph should not be empty after loading");

            Assert.Equal(g, h);

            // Now attempt to save an empty graph as well
            g = new Graph
            {
                BaseUri = graphUri
            };
            Console.WriteLine("Attempting to save empty graph with same name");
            agraph.SaveGraph(g);
            Console.WriteLine("Empty graph saved");

            h = new Graph();
            agraph.LoadGraph(h, graphUri);
            Assert.True(h.IsEmpty, "Graph should be empty after loading");

            Assert.Equal(g, h);
        }

        [SkippableFact]
        public void StorageAllegroGraphSaveEmptyGraph3()
        {
            AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();
            Uri graphUri = null;
            Console.WriteLine("Deleting any existing graph");
            agraph.DeleteGraph(graphUri);
            Console.WriteLine("Existing graph deleted");

            // First create a non-empty graph
            var g = new Graph
            {
                BaseUri = graphUri
            };
            g.Assert(g.CreateBlankNode(), g.CreateUriNode("rdf:type"), g.CreateUriNode(new Uri("http://example.org/BNode")));
            Console.WriteLine("Saving non-empty graph");
            agraph.SaveGraph(g);
            Console.WriteLine("Non-empty graph saved");

            var h = new Graph();
            agraph.LoadGraph(h, graphUri);
            Assert.False(h.IsEmpty, "Graph should not be empty after loading");

            Assert.Equal(g, h);

            // Now attempt to overwrite with an empty graph
            g = new Graph
            {
                BaseUri = graphUri
            };
            Console.WriteLine("Attempting to save empty graph with same name");
            agraph.SaveGraph(g);
            Console.WriteLine("Empty graph saved");

            h = new Graph();
            agraph.LoadGraph(h, graphUri);

            // Since saving to default graph does not overwrite the graph we've just retrieved must contain the empty graph as a sub-graph
            Assert.True(h.HasSubGraph(g));
        }

        [SkippableFact]
        public void StorageAllegroGraphDeleteTriples()
        {
            var g = new Graph();
            FileLoader.Load(g, "resources\\InferenceTest.ttl");
            g.BaseUri = new Uri("http://example.org/AllegroGraphTest");

            AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();
            agraph.SaveGraph(g);

            Console.WriteLine("Graph before deletion");
            TestTools.ShowGraph(g);

            //Delete all Triples about the Ford Fiesta
            agraph.UpdateGraph(g.BaseUri, null, g.GetTriplesWithSubject(new Uri("http://example.org/vehicles/FordFiesta")));

            var h = new Graph();
            agraph.LoadGraph(h, g.BaseUri);

            Console.WriteLine("Graph after deletion");
            TestTools.ShowGraph(h);

            Assert.False(h.IsEmpty, "Graph should not be completely empty");
            Assert.True(g.HasSubGraph(h), "Graph retrieved with missing Triples should be a sub-graph of the original Graph");
            Assert.False(g.Equals(h), "Graph retrieved should not be equal to original Graph");

            var results = agraph.Query("ASK WHERE { GRAPH <http://example.org/AllegroGraphTest> { <http://example.org/vehicles/FordFiesta> ?p ?o } }");
            if (results is SparqlResultSet)
            {
                Assert.False(((SparqlResultSet) results).Result, "There should no longer be any triples about the Ford Fiesta present");
            }
        }

        [SkippableFact]
        public void StorageAllegroGraphDeleteGraph1()
        {
            AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();
            var graphUri = new Uri("http://example.org/AllegroGraph/delete");

            var g = new Graph();
            FileLoader.Load(g, "resources\\InferenceTest.ttl");
            g.BaseUri = graphUri;

            agraph.SaveGraph(g);

            var h = new Graph();
            agraph.LoadGraph(h, graphUri);
            Assert.False(h.IsEmpty, "Graph should not be empty after loading");

            Assert.Equal(g, h);

            agraph.DeleteGraph(graphUri);
            h = new Graph();
            agraph.LoadGraph(h, graphUri);
            Assert.True(h.IsEmpty, "Graph should be equal after deletion");
            Assert.NotEqual(g, h);
        }

        [SkippableFact]
        public void StorageAllegroGraphDeleteGraph2()
        {
            AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();
            Uri graphUri = null;
            agraph.DeleteGraph(graphUri);

            var g = new Graph();
            FileLoader.Load(g, "resources\\InferenceTest.ttl");
            g.BaseUri = graphUri;

            agraph.SaveGraph(g);

            var h = new Graph();
            agraph.LoadGraph(h, graphUri);
            Assert.False(h.IsEmpty, "Graph should not be empty after loading");

            Assert.Equal(g, h);

            agraph.DeleteGraph(graphUri);
            h = new Graph();
            agraph.LoadGraph(h, graphUri);
            Assert.True(h.IsEmpty, "Graph should be equal after deletion");
            Assert.NotEqual(g, h);
        }

        [SkippableFact]
        public void StorageAllegroGraphAsk()
        {
            AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();

            var ask = "ASK WHERE { ?s ?p ?o }";

            var results = agraph.Query(ask);
            Assert.IsAssignableFrom<SparqlResultSet>(results);
            if (results is SparqlResultSet)
            {
                TestTools.ShowResults(results);
            }
        }

        [SkippableFact]
        public void StorageAllegroGraphDescribe()
        {
            AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();

            var describe = "DESCRIBE <http://example.org/Vehicles/FordFiesta>";

            var results = agraph.Query(describe);
            Assert.IsAssignableFrom<IGraph>(results);
            if (results is IGraph)
            {
                TestTools.ShowGraph((IGraph) results);
            }
        }

        [SkippableFact]
        public void StorageAllegroGraphSparqlUpdate()
        {
            AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();

            var updates = "INSERT DATA { GRAPH <http://example.org/new-graph> { <http://subject> <http://predicate> <http://object> } }";

            agraph.Update(updates);

            var results = agraph.Query("SELECT * WHERE { GRAPH <http://example.org/new-graph> { ?s ?p ?o } }") as SparqlResultSet;
            Assert.NotNull(results);
            Assert.Equal(1, results.Count);
        }
    }
}
