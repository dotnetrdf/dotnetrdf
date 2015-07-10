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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Management;
using VDS.RDF.Storage.Management.Provisioning;
using VDS.RDF.Update;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Storage
{
    [TestFixture]
    public class StardogTests
        : GenericUpdateProcessorTests
    {
        public static StardogConnector GetConnection()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseStardog))
            {
                Assert.Inconclusive("Test Config marks Stardog as unavailable, test cannot be run");
            }
            return new StardogConnector(TestConfigManager.GetSetting(TestConfigManager.StardogServer), TestConfigManager.GetSetting(TestConfigManager.StardogDatabase), TestConfigManager.GetSetting(TestConfigManager.StardogUser), TestConfigManager.GetSetting(TestConfigManager.StardogPassword));
        }

        public static StardogServer GetServer()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseStardog))
            {
                Assert.Inconclusive("Test Config marks Stardog as unavailable, test cannot be run");
            }
            return new StardogServer(TestConfigManager.GetSetting(TestConfigManager.StardogServer), TestConfigManager.GetSetting(TestConfigManager.StardogUser), TestConfigManager.GetSetting(TestConfigManager.StardogPassword));
        }

        protected override IStorageProvider GetManager()
        {
            return (IStorageProvider) StardogTests.GetConnection();
        }

        [SetUp]
        public void Setup()
        {
            Options.HttpDebugging = true;
        }

        [TearDown]
        public void CleanUp()
        {
            Options.HttpDebugging = false;
        }

#if !NO_SYNC_HTTP
        // Many of these tests require a synchronous API
        [Test]
        public void StorageStardogLoadDefaultGraph()
        {
            StardogConnector stardog = StardogTests.GetConnection();
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            g.BaseUri = null;
            stardog.SaveGraph(g);

            Graph h = new Graph();
            stardog.LoadGraph(h, (Uri) null);

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (Triple t in h.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }

            Assert.IsFalse(h.IsEmpty);
        }

        [Test]
        public void StorageStardogLoadNamedGraph()
        {
            StardogConnector stardog = StardogTests.GetConnection();

            // Ensure graph exists
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            g.BaseUri = new Uri("http://example.org/graph");
            stardog.SaveGraph(g);

            // Load it back from the store
            Graph h = new Graph();
            stardog.LoadGraph(h, new Uri("http://example.org/graph"));

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (Triple t in h.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }

            Assert.IsFalse(h.IsEmpty);
            Assert.AreEqual(g, h);
        }

        [Test]
        public void StorageStardogSaveToDefaultGraph()
        {
            StardogConnector stardog = StardogTests.GetConnection();
            ;
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            g.BaseUri = null;
            stardog.SaveGraph(g);

            Graph h = new Graph();
            stardog.LoadGraph(h, (Uri) null);
            Console.WriteLine("Retrieved " + h.Triples.Count + " Triple(s) from Stardog");

            if (g.Triples.Count == h.Triples.Count)
            {
                Assert.AreEqual(g, h, "Retrieved Graph should be equal to the Saved Graph");
            }
            else
            {
                Assert.IsTrue(h.HasSubGraph(g), "Retrieved Graph should have the Saved Graph as a subgraph");
            }
        }

        [Test]
        public void StorageStardogSaveToNamedGraph()
        {
            try
            {
                //Options.UseBomForUtf8 = false;

                StardogConnector stardog = StardogTests.GetConnection();
                ;
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                g.BaseUri = new Uri("http://example.org/graph");
                stardog.SaveGraph(g);

                Graph h = new Graph();
                stardog.LoadGraph(h, new Uri("http://example.org/graph"));

                Assert.AreEqual(g, h, "Retrieved Graph should be equal to the Saved Graph");
            }
            finally
            {
                //Options.UseBomForUtf8 = true;
            }
        }

        [Test]
        public void StorageStardogSaveToNamedGraph2()
        {
            try
            {
                //Options.UseBomForUtf8 = false;

                StardogConnector stardog = StardogTests.GetConnection();
                ;
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                Uri u = new Uri("http://example.org/graph/" + DateTime.Now.Ticks);
                g.BaseUri = u;
                stardog.SaveGraph(g);

                Graph h = new Graph();
                stardog.LoadGraph(h, u);

                Assert.AreEqual(g, h, "Retrieved Graph should be equal to the Saved Graph");
            }
            finally
            {
                //Options.UseBomForUtf8 = true;
            }
        }

        [Test]
        public void StorageStardogSaveToNamedGraphOverwrite()
        {
            StardogConnector stardog = StardogTests.GetConnection();
            ;
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            g.BaseUri = new Uri("http://example.org/namedGraph");
            stardog.SaveGraph(g);

            Graph h = new Graph();
            stardog.LoadGraph(h, new Uri("http://example.org/namedGraph"));

            Assert.AreEqual(g, h, "Retrieved Graph should be equal to the Saved Graph");

            Graph i = new Graph();
            i.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
            i.BaseUri = new Uri("http://example.org/namedGraph");
            stardog.SaveGraph(i);

            Graph j = new Graph();
            stardog.LoadGraph(j, "http://example.org/namedGraph");

            Assert.AreNotEqual(g, j, "Retrieved Graph should not be equal to overwritten Graph");
            Assert.AreEqual(i, j, "Retrieved Graph should be equal to Saved Graph");
        }

        [Test]
        public void StorageStardogUpdateNamedGraphRemoveTriples()
        {
            try
            {
                //Options.UseBomForUtf8 = false;

                StardogConnector stardog = StardogTests.GetConnection();
                ;
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                g.BaseUri = new Uri("http://example.org/graph");
                stardog.SaveGraph(g);

                INode rdfType = g.CreateUriNode(new Uri(VDS.RDF.Parsing.RdfSpecsHelper.RdfType));

                stardog.UpdateGraph(g.BaseUri, null, g.GetTriplesWithPredicate(rdfType));
                g.Retract(g.GetTriplesWithPredicate(rdfType).ToList());

                Graph h = new Graph();
                stardog.LoadGraph(h, new Uri("http://example.org/graph"));

                if (g.Triples.Count == h.Triples.Count)
                {
                    Assert.AreEqual(g, h, "Retrieved Graph should be equal to the Saved Graph");
                }
                else
                {
                    Assert.IsTrue(h.HasSubGraph(g), "Retrieved Graph should have the Saved Graph as a subgraph");
                }
                Assert.IsFalse(h.GetTriplesWithPredicate(rdfType).Any(), "Retrieved Graph should not contain any rdf:type Triples");
            }
            finally
            {
                //Options.UseBomForUtf8 = true;
            }
        }

        [Test]
        public void StorageStardogUpdateNamedGraphAddTriples()
        {
            try
            {
                //Options.UseBomForUtf8 = false;

                StardogConnector stardog = StardogTests.GetConnection();
                ;
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                g.BaseUri = new Uri("http://example.org/addGraph");

                INode rdfType = g.CreateUriNode(new Uri(VDS.RDF.Parsing.RdfSpecsHelper.RdfType));
                Graph types = new Graph();
                types.Assert(g.GetTriplesWithPredicate(rdfType));
                g.Retract(g.GetTriplesWithPredicate(rdfType).ToList());

                //Save the Graph without the rdf:type triples
                stardog.SaveGraph(g);
                //Then add back in the rdf:type triples
                stardog.UpdateGraph(g.BaseUri, types.Triples, null);

                Graph h = new Graph();
                stardog.LoadGraph(h, new Uri("http://example.org/addGraph"));

                if (g.Triples.Count == h.Triples.Count)
                {
                    Assert.AreEqual(g, h, "Retrieved Graph should be equal to the Saved Graph");
                }
                else
                {
                    Assert.IsTrue(h.HasSubGraph(g), "Retrieved Graph should have the Saved Graph as a subgraph");
                }
                Assert.IsTrue(h.GetTriplesWithPredicate(rdfType).Any(), "Retrieved Graph should not contain any rdf:type Triples");
            }
            finally
            {
                //Options.UseBomForUtf8 = true;
            }
        }

        [Test]
        public void StorageStardogDeleteNamedGraph()
        {
            try
            {
                //Options.UseBomForUtf8 = false;

                StardogConnector stardog = StardogTests.GetConnection();
                ;
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                g.BaseUri = new Uri("http://example.org/tempGraph");
                stardog.SaveGraph(g);

                Graph h = new Graph();
                stardog.LoadGraph(h, new Uri("http://example.org/tempGraph"));

                if (g.Triples.Count == h.Triples.Count)
                {
                    Assert.AreEqual(g, h, "Retrieved Graph should be equal to the Saved Graph");
                }
                else
                {
                    Assert.IsTrue(h.HasSubGraph(g), "Retrieved Graph should have the Saved Graph as a subgraph");
                }

                stardog.DeleteGraph("http://example.org/tempGraph");
                Graph i = new Graph();
                stardog.LoadGraph(i, new Uri("http://example.org/tempGraph"));

                Assert.IsTrue(i.IsEmpty, "Retrieved Graph should be empty since it has been deleted");
            }
            finally
            {
                //Options.UseBomForUtf8 = true;
            }
        }

        [Test]
        public void StorageStardogReasoningQL()
        {
            StardogConnector stardog = StardogTests.GetConnection();
            if (stardog.Reasoning == StardogReasoningMode.DatabaseControlled)
            {
                Assert.Inconclusive("Version of Stardog being tested does not support configuring reasoning mode at connection level");
            }

            Graph g = new Graph();
            g.LoadFromFile("resources\\InferenceTest.ttl");
            g.BaseUri = new Uri("http://example.org/reasoning");
            stardog.SaveGraph(g);

            String query = "PREFIX rdfs: <" + NamespaceMapper.RDFS + "> SELECT * WHERE { { ?class rdfs:subClassOf <http://example.org/vehicles/Vehicle> } UNION { GRAPH <http://example.org/reasoning> { ?class rdfs:subClassOf <http://example.org/vehicles/Vehicle> } } }";
            Console.WriteLine(query);
            Console.WriteLine();

            SparqlResultSet resultsNoReasoning = stardog.Query(query) as SparqlResultSet;
            if (resultsNoReasoning != null)
            {
                Console.WriteLine("Results without Reasoning");
                TestTools.ShowResults(resultsNoReasoning);
            }
            else
            {
                Assert.Fail("Did not get a SPARQL Result Set as expected");
            }

            stardog.Reasoning = StardogReasoningMode.QL;
            SparqlResultSet resultsWithReasoning = stardog.Query(query) as SparqlResultSet;
            if (resultsWithReasoning != null)
            {
                Console.WriteLine("Results with Reasoning");
                TestTools.ShowResults(resultsWithReasoning);
            }
            else
            {
                Assert.Fail("Did not get a SPARQL Result Set as expected");
            }

            Assert.IsTrue(resultsWithReasoning.Count >= resultsNoReasoning.Count, "Reasoning should yield as many if not more results");
        }

        [Test]
        public void StorageStardogTransactionTest()
        {
            StardogConnector stardog = StardogTests.GetConnection();
            ;
            stardog.Begin();
            stardog.Commit();
            stardog.Dispose();
        }

        [Test]
        public void StorageStardogAmpersandsInDataTest()
        {
            StardogConnector stardog = StardogTests.GetConnection();

            //Save the Graph
            Graph g = new Graph();
            const string fragment = "@prefix : <http://example.org/> . [] :string \"This has & ampersands in it\" .";
            g.LoadFromString(fragment);
            g.BaseUri = new Uri("http://example.org/ampersandGraph");

            Console.WriteLine("Original Graph:");
            TestTools.ShowGraph(g);

            stardog.SaveGraph(g);

            //Retrieve and check it round trips
            Graph h = new Graph();
            stardog.LoadGraph(h, g.BaseUri);

            Console.WriteLine("Graph as retrieved from Stardog:");
            TestTools.ShowGraph(h);

            Assert.AreEqual(g, h, "Graphs should be equal");

            //Now try to delete the data from this Graph
            GenericUpdateProcessor processor = new GenericUpdateProcessor(stardog);
            SparqlUpdateParser parser = new SparqlUpdateParser();
            processor.ProcessCommandSet(parser.ParseFromString("DELETE WHERE { GRAPH <http://example.org/ampersandGraph> { ?s ?p ?o } }"));

            Graph i = new Graph();
            stardog.LoadGraph(i, g.BaseUri);

            Console.WriteLine("Graph as retrieved after the DELETE WHERE:");
            TestTools.ShowGraph(i);

            Assert.AreNotEqual(g, i, "Graphs should not be equal");
            Assert.AreNotEqual(h, i, "Graphs should not be equal");
        }

        [Test]
        public void StorageStardogCreateNewStore()
        {
            Guid guid;
            do
            {
                guid = Guid.NewGuid();
            } while (guid.Equals(Guid.Empty) || !Char.IsLetter(guid.ToString()[0]));

            StardogServer stardog = StardogTests.GetServer();
            IStoreTemplate template = stardog.GetDefaultTemplate(guid.ToString());
            Console.WriteLine("Template ID " + template.ID);

            stardog.CreateStore(template);

            stardog.Dispose();
        }

        [Test]
        public void StorageStardogSparqlUpdate1()
        {
            StardogConnector stardog = StardogTests.GetConnection();
            IGraph g;

            g = new Graph();
            stardog.LoadGraph(g, "http://example.org/stardog/update/1");
            if (!g.IsEmpty)
            {
                Console.WriteLine("Dropping graph");
                stardog.Update("DROP SILENT GRAPH <http://example.org/stardog/update/1>");
                Console.WriteLine("Dropped graph");
                Thread.Sleep(2500);
                g = new Graph();
                stardog.LoadGraph(g, "http://example.org/stardog/update/1");
                Assert.IsTrue(g.IsEmpty, "Graph should be empty after DROP command");
            }

            Console.WriteLine("Inserting data");
            stardog.Update("INSERT DATA { GRAPH <http://example.org/stardog/update/1> { <http://x> <http://y> <http://z> } }");
            Console.WriteLine("Inserted data");
            g = new Graph();
            stardog.LoadGraph(g, "http://example.org/stardog/update/1");
            Assert.IsFalse(g.IsEmpty, "Graph should not be empty");
            Assert.AreEqual(1, g.Triples.Count, "Should be 1 triple in the graph");
        }

        [Test]
        public void StorageStardogSparqlUpdate2()
        {
            StardogConnector stardog = StardogTests.GetConnection();
            IGraph g;

            Console.WriteLine("Dropping graph");
            stardog.Update("DROP SILENT GRAPH <http://example.org/stardog/update/2>");
            Console.WriteLine("Dropped graph");
            Thread.Sleep(2500);
            g = new Graph();
            stardog.LoadGraph(g, "http://example.org/stardog/update/2");
            Assert.IsTrue(g.IsEmpty, "Graph should be empty after DROP command");

            Console.WriteLine("Inserting data");
            stardog.Update("INSERT DATA { GRAPH <http://example.org/stardog/update/2> { <http://x> <http://y> <http://z> } }");
            Console.WriteLine("Inserted data");
            g = new Graph();
            stardog.LoadGraph(g, "http://example.org/stardog/update/2");
            Assert.IsFalse(g.IsEmpty, "Graph should not be empty");
            Assert.AreEqual(1, g.Triples.Count, "Should be 1 triple in the graph");
        }

        [Test]
        public void StorageStardogSparqlUpdate3()
        {
            StardogConnector stardog = StardogTests.GetConnection();
            IGraph g;

            Console.WriteLine("Dropping graph");
            stardog.Update("DROP SILENT GRAPH <http://example.org/stardog/update/3>");
            Console.WriteLine("Dropped graph");
            Thread.Sleep(2500);
            g = new Graph();
            stardog.LoadGraph(g, "http://example.org/stardog/update/3");
            Assert.IsTrue(g.IsEmpty, "Graph should be empty after DROP command");

            Console.WriteLine("Inserting data");
            IGraph newData = new Graph();
            newData.BaseUri = new Uri("http://example.org/stardog/update/3");
            newData.Assert(newData.CreateUriNode(new Uri("http://x")), newData.CreateUriNode(new Uri("http://y")), newData.CreateUriNode(new Uri("http://z")));
            stardog.SaveGraph(newData);
            Console.WriteLine("Inserted data");
            g = new Graph();
            stardog.LoadGraph(g, "http://example.org/stardog/update/3");
            Assert.IsFalse(g.IsEmpty, "Graph should not be empty");
            Assert.AreEqual(1, g.Triples.Count, "Should be 1 triple in the graph");
        }

        [Test, Timeout(30000)]
        public void StorageStardogSparqlUpdate4()
        {
            StardogConnector stardog = StardogTests.GetConnection();
            IGraph g;

            // Begin a transaction
            stardog.Begin();

            // Try to make an update
            stardog.Update("DROP SILENT GRAPH <http://example.org/stardog/update/4>; INSERT DATA { GRAPH <http://example.org/stardog/update/4> { <http://x> <http://y> <http://z> } }");

            // Commit the transaction
            stardog.Commit();

            g = new Graph();
            stardog.LoadGraph(g, "http://example.org/stardog/update/4");
            Assert.IsFalse(g.IsEmpty, "Graph should not be empty after update");
            Assert.AreEqual(1, g.Triples.Count, "Graph should contain 1 triple");

            stardog.Dispose();
        }

        [Test, Timeout(30000)]
        public void StorageStardogSparqlUpdate5()
        {
            StardogConnector stardog = StardogTests.GetConnection();
            IGraph g;

            // Begin a transaction
            stardog.Begin();

            // Try to make an update
            stardog.Update("DROP SILENT GRAPH <http://example.org/stardog/update/5>; INSERT DATA { GRAPH <http://example.org/stardog/update/5> { <http://x> <http://y> <http://z> } }");

            // Rollback the transaction
            stardog.Rollback();

            g = new Graph();
            stardog.LoadGraph(g, "http://example.org/stardog/update/5");
            Assert.IsFalse(g.IsEmpty, "Graph should not be empty after update");
            Assert.AreEqual(1, g.Triples.Count, "Graph should contain 1 triple");

            stardog.Dispose();
        }
#endif
    }
}