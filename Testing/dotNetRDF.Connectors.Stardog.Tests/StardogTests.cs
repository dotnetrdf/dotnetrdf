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
using System.Threading;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage.Management;
using VDS.RDF.Storage.Management.Provisioning;
using VDS.RDF.Storage.Management.Provisioning.Stardog;
using VDS.RDF.Update;

namespace VDS.RDF.Storage
{

    public class StardogTests
        //: GenericUpdateProcessorTests
    {
        public static StardogConnector GetConnection()
        {
            Skip.IfNot(TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseStardog), "Test Config marks Stardog as unavailable, test cannot be run");
            var connector = new StardogConnector(TestConfigManager.GetSetting(TestConfigManager.StardogServer),
                TestConfigManager.GetSetting(TestConfigManager.StardogDatabase),
                TestConfigManager.GetSetting(TestConfigManager.StardogUser),
                TestConfigManager.GetSetting(TestConfigManager.StardogPassword));
            var testStore = TestConfigManager.GetSetting(TestConfigManager.StardogDatabase);
            if (!connector.ParentServer.ListStores().Contains(testStore))
            {
                connector.ParentServer.CreateStore(new StardogMemTemplate(testStore));
            }

            return connector;
        }

        public static StardogServer GetServer()
        {
            Skip.IfNot(TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseStardog), "Test Config marks Stardog as unavailable, test cannot be run");
            return new StardogServer(TestConfigManager.GetSetting(TestConfigManager.StardogServer),
                TestConfigManager.GetSetting(TestConfigManager.StardogUser),
                TestConfigManager.GetSetting(TestConfigManager.StardogPassword));
        }

        /*
        protected override IStorageProvider GetManager()
        {
            return (IStorageProvider) StardogTests.GetConnection();
        }
        */

        // Many of these tests require a synchronous API
        [SkippableFact]
        public void StorageStardogLoadDefaultGraph()
        {
            StardogConnector stardog = StardogTests.GetConnection();
            var g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            g.BaseUri = null;
            stardog.SaveGraph(g);

            var h = new Graph();
            stardog.LoadGraph(h, (Uri) null);

            Assert.False(h.IsEmpty);
        }

        [SkippableFact]
        public void StorageStardogLoadNamedGraph()
        {
            StardogConnector stardog = StardogTests.GetConnection();

            // Ensure graph exists
            var g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            g.BaseUri = new Uri("http://example.org/graph");
            stardog.SaveGraph(g);

            // Load it back from the store
            var h = new Graph();
            stardog.LoadGraph(h, new Uri("http://example.org/graph"));

            Assert.False(h.IsEmpty);
            Assert.Equal(g, h);
        }

        [SkippableFact]
        public void StorageStardogSaveToDefaultGraph()
        {
            StardogConnector stardog = StardogTests.GetConnection();
            
            var g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            g.BaseUri = null;
            stardog.SaveGraph(g);

            var h = new Graph();
            stardog.LoadGraph(h, (Uri) null);

            if (g.Triples.Count == h.Triples.Count)
            {
                Assert.Equal(g, h);
            }
            else
            {
                Assert.True(h.HasSubGraph(g), "Retrieved Graph should have the Saved Graph as a subgraph");
            }
        }

        [SkippableFact]
        public void StorageStardogSaveToNamedGraph()
        {
            try
            {
                //Options.UseBomForUtf8 = false;

                StardogConnector stardog = StardogTests.GetConnection();
                
                var g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                g.BaseUri = new Uri("http://example.org/graph");
                stardog.SaveGraph(g);

                var h = new Graph();
                stardog.LoadGraph(h, new Uri("http://example.org/graph"));

                Assert.Equal(g, h);
            }
            finally
            {
                //Options.UseBomForUtf8 = true;
            }
        }

        [SkippableFact]
        public void StorageStardogSaveToNamedGraph2()
        {
            try
            {
                //Options.UseBomForUtf8 = false;

                StardogConnector stardog = StardogTests.GetConnection();
                
                var g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                var u = new Uri("http://example.org/graph/" + DateTime.Now.Ticks);
                g.BaseUri = u;
                stardog.SaveGraph(g);

                var h = new Graph();
                stardog.LoadGraph(h, u);

                Assert.Equal(g, h);
            }
            finally
            {
                //Options.UseBomForUtf8 = true;
            }
        }

        [SkippableFact]
        public void StorageStardogSaveToNamedGraphOverwrite()
        {
            StardogConnector stardog = StardogTests.GetConnection();
            
            var g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            g.BaseUri = new Uri("http://example.org/namedGraph");
            stardog.SaveGraph(g);

            var h = new Graph();
            stardog.LoadGraph(h, new Uri("http://example.org/namedGraph"));

            Assert.Equal(g, h);

            var i = new Graph();
            i.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
            i.BaseUri = new Uri("http://example.org/namedGraph");
            stardog.SaveGraph(i);

            var j = new Graph();
            stardog.LoadGraph(j, "http://example.org/namedGraph");

            Assert.NotEqual(g, j);
            Assert.Equal(i, j);
        }

        [SkippableFact]
        public void StorageStardogUpdateNamedGraphRemoveTriples()
        {
            try
            {
                //Options.UseBomForUtf8 = false;

                StardogConnector stardog = StardogTests.GetConnection();
                
                var g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                g.BaseUri = new Uri("http://example.org/graph");
                stardog.SaveGraph(g);

                INode rdfType = g.CreateUriNode(new Uri(VDS.RDF.Parsing.RdfSpecsHelper.RdfType));

                stardog.UpdateGraph(g.BaseUri, null, g.GetTriplesWithPredicate(rdfType));
                g.Retract(g.GetTriplesWithPredicate(rdfType).ToList());

                var h = new Graph();
                stardog.LoadGraph(h, new Uri("http://example.org/graph"));

                if (g.Triples.Count == h.Triples.Count)
                {
                    Assert.Equal(g, h);
                }
                else
                {
                    Assert.True(h.HasSubGraph(g), "Retrieved Graph should have the Saved Graph as a subgraph");
                }
                Assert.False(h.GetTriplesWithPredicate(rdfType).Any(),
                    "Retrieved Graph should not contain any rdf:type Triples");
            }
            finally
            {
                //Options.UseBomForUtf8 = true;
            }
        }

        [SkippableFact]
        public void StorageStardogUpdateNamedGraphAddTriples()
        {
            try
            {
                //Options.UseBomForUtf8 = false;

                StardogConnector stardog = StardogTests.GetConnection();
                
                var g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                g.BaseUri = new Uri("http://example.org/addGraph");

                INode rdfType = g.CreateUriNode(new Uri(VDS.RDF.Parsing.RdfSpecsHelper.RdfType));
                var types = new Graph();
                types.Assert(g.GetTriplesWithPredicate(rdfType));
                g.Retract(g.GetTriplesWithPredicate(rdfType).ToList());

                //Save the Graph without the rdf:type triples
                stardog.SaveGraph(g);
                //Then add back in the rdf:type triples
                stardog.UpdateGraph(g.BaseUri, types.Triples, null);

                var h = new Graph();
                stardog.LoadGraph(h, new Uri("http://example.org/addGraph"));

                if (g.Triples.Count == h.Triples.Count)
                {
                    Assert.Equal(g, h);
                }
                else
                {
                    Assert.True(h.HasSubGraph(g), "Retrieved Graph should have the Saved Graph as a subgraph");
                }
                Assert.True(h.GetTriplesWithPredicate(rdfType).Any(),
                    "Retrieved Graph should not contain any rdf:type Triples");
            }
            finally
            {
                //Options.UseBomForUtf8 = true;
            }
        }

        [SkippableFact]
        public void StorageStardogDeleteNamedGraph()
        {
            try
            {
                //Options.UseBomForUtf8 = false;

                StardogConnector stardog = StardogTests.GetConnection();
                
                var g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                g.BaseUri = new Uri("http://example.org/tempGraph");
                stardog.SaveGraph(g);

                var h = new Graph();
                stardog.LoadGraph(h, new Uri("http://example.org/tempGraph"));

                if (g.Triples.Count == h.Triples.Count)
                {
                    Assert.Equal(g, h);
                }
                else
                {
                    Assert.True(h.HasSubGraph(g), "Retrieved Graph should have the Saved Graph as a subgraph");
                }

                stardog.DeleteGraph("http://example.org/tempGraph");
                var i = new Graph();
                stardog.LoadGraph(i, new Uri("http://example.org/tempGraph"));

                Assert.True(i.IsEmpty, "Retrieved Graph should be empty since it has been deleted");
            }
            finally
            {
                //Options.UseBomForUtf8 = true;
            }
        }

        [SkippableFact]
        public void StorageStardogReasoningQL()
        {
            StardogConnector stardog = StardogTests.GetConnection();
            Skip.If(stardog.Reasoning == StardogReasoningMode.DatabaseControlled, 
                    "Version of Stardog being tested does not support configuring reasoning mode at connection level");

            var g = new Graph();
            g.LoadFromFile("resources\\InferenceTest.ttl");
            g.BaseUri = new Uri("http://example.org/reasoning");
            stardog.SaveGraph(g);

            var query = "PREFIX rdfs: <" + NamespaceMapper.RDFS +
                           "> SELECT * WHERE { { ?class rdfs:subClassOf <http://example.org/vehicles/Vehicle> } UNION { GRAPH <http://example.org/reasoning> { ?class rdfs:subClassOf <http://example.org/vehicles/Vehicle> } } }";

            var resultsNoReasoning = stardog.Query(query) as SparqlResultSet;
            Assert.NotNull(resultsNoReasoning);

            stardog.Reasoning = StardogReasoningMode.QL;
            var resultsWithReasoning = stardog.Query(query) as SparqlResultSet;
            Assert.NotNull(resultsWithReasoning);

            Assert.True(resultsWithReasoning.Count >= resultsNoReasoning.Count,
                "Reasoning should yield as many if not more results");
        }


        [SkippableFact]
        public void StorageStardogReasoningByQuery1()
        {
            StardogConnector stardog = StardogTests.GetConnection();
            Skip.If(stardog.Reasoning == StardogReasoningMode.DatabaseControlled, 
                    "Version of Stardog being tested does not support configuring reasoning mode at connection level");

            var g = new Graph();
            g.LoadFromFile("resources\\stardog-reasoning-test.rdf");
            g.BaseUri = new Uri("http://www.reasoningtest.com/");
            stardog.SaveGraph(g);

            var query = "Select ?building where { ?building <http://www.reasoningtest.com#hasLocation> ?room.}";

            var resultsWithReasoning = stardog.Query(query, true) as SparqlResultSet;
            Assert.NotNull(resultsWithReasoning);
        }


        [SkippableFact]
        public void StorageStardogReasoningByQuery2()
        {
            StardogConnector stardog = StardogTests.GetConnection();
            Skip.If(stardog.Reasoning == StardogReasoningMode.DatabaseControlled, 
                    "Version of Stardog being tested does not support configuring reasoning mode at connection level");

            var g = new Graph();
            g.LoadFromFile("resources\\stardog-reasoning-test.rdf");
            g.BaseUri = new Uri("http://www.reasoningtest.com/");
            stardog.SaveGraph(g);

            var query = "Select ?building where { ?building <http://www.reasoningtest.com#hasLocation> ?room.}"; 

            var resultsWithNoReasoning = stardog.Query(query, false) as SparqlResultSet;
            Assert.Null(resultsWithNoReasoning);
        }


        [SkippableFact]
        public void StorageStardogReasoningMode()
        {
            StardogConnector connector = StardogTests.GetConnection();

            if (connector.Reasoning != StardogReasoningMode.DatabaseControlled)
            {
                return;
            }
            else
            {
                Assert.Throws<RdfStorageException>(() =>
                    connector.Reasoning = StardogReasoningMode.DL
                    );
            }
        }

        [SkippableFact]
        public void StorageStardogTransactionTest()
        {
            StardogConnector stardog = StardogTests.GetConnection();
            
            stardog.Begin();
            stardog.Commit();
            stardog.Dispose();
        }

        [SkippableFact]
        public void StorageStardogAmpersandsInDataTest()
        {
            StardogConnector stardog = StardogTests.GetConnection();

            //Save the Graph
            var g = new Graph();
            const string fragment = "@prefix : <http://example.org/> . [] :string \"This has & ampersands in it\" .";
            g.LoadFromString(fragment);
            g.BaseUri = new Uri("http://example.org/ampersandGraph");

            stardog.SaveGraph(g);

            //Retrieve and check it round trips
            var h = new Graph();
            stardog.LoadGraph(h, g.BaseUri);

            Assert.Equal(g, h);

            //Now try to delete the data from this Graph
            var processor = new GenericUpdateProcessor(stardog);
            var parser = new SparqlUpdateParser();
            processor.ProcessCommandSet(
                parser.ParseFromString("DELETE WHERE { GRAPH <http://example.org/ampersandGraph> { ?s ?p ?o } }"));

            var i = new Graph();
            stardog.LoadGraph(i, g.BaseUri);

            Assert.NotEqual(g, i);
            Assert.NotEqual(h, i);
        }

        [SkippableFact]
        public void StorageStardogCreateNewStore()
        {
            Guid guid;
            do
            {
                guid = Guid.NewGuid();
            } while (guid.Equals(Guid.Empty) || !Char.IsLetter(guid.ToString()[0]));

            StardogServer stardog = StardogTests.GetServer();
            IStoreTemplate template = stardog.GetDefaultTemplate(guid.ToString());

            stardog.CreateStore(template);

            stardog.Dispose();
        }

        [SkippableFact]
        public void StorageStardogSparqlUpdate1()
        {
            StardogConnector stardog = StardogTests.GetConnection();
            IGraph g;

            g = new Graph();
            stardog.LoadGraph(g, "http://example.org/stardog/update/1");
            if (!g.IsEmpty)
            {
                stardog.Update("DROP SILENT GRAPH <http://example.org/stardog/update/1>");
                g = new Graph();
                stardog.LoadGraph(g, "http://example.org/stardog/update/1");
                Assert.True(g.IsEmpty, "Graph should be empty after DROP command");
            }

            stardog.Update(
                "INSERT DATA { GRAPH <http://example.org/stardog/update/1> { <http://x> <http://y> <http://z> } }");
            g = new Graph();
            stardog.LoadGraph(g, "http://example.org/stardog/update/1");
            Assert.False(g.IsEmpty, "Graph should not be empty");
            Assert.Equal(1, g.Triples.Count);
        }

        [SkippableFact]
        public void StorageStardogSparqlUpdate2()
        {
            StardogConnector stardog = StardogTests.GetConnection();
            IGraph g;

            stardog.Update("DROP SILENT GRAPH <http://example.org/stardog/update/2>");
            g = new Graph();
            stardog.LoadGraph(g, "http://example.org/stardog/update/2");
            Assert.True(g.IsEmpty, "Graph should be empty after DROP command");

            stardog.Update(
                "INSERT DATA { GRAPH <http://example.org/stardog/update/2> { <http://x> <http://y> <http://z> } }");
            g = new Graph();
            stardog.LoadGraph(g, "http://example.org/stardog/update/2");
            Assert.False(g.IsEmpty, "Graph should not be empty");
            Assert.Equal(1, g.Triples.Count);
        }

        [SkippableFact]
        public void StorageStardogSparqlUpdate3()
        {
            StardogConnector stardog = StardogTests.GetConnection();
            IGraph g;

            stardog.Update("DROP SILENT GRAPH <http://example.org/stardog/update/3>");
            g = new Graph();
            stardog.LoadGraph(g, "http://example.org/stardog/update/3");
            Assert.True(g.IsEmpty, "Graph should be empty after DROP command");

            IGraph newData = new Graph
            {
                BaseUri = new Uri("http://example.org/stardog/update/3")
            };
            newData.Assert(newData.CreateUriNode(new Uri("http://x")), newData.CreateUriNode(new Uri("http://y")),
                newData.CreateUriNode(new Uri("http://z")));
            stardog.SaveGraph(newData);
            g = new Graph();
            stardog.LoadGraph(g, "http://example.org/stardog/update/3");
            Assert.False(g.IsEmpty, "Graph should not be empty");
            Assert.Equal(1, g.Triples.Count);
        }

        [SkippableFact]
        public void StorageStardogSparqlUpdate4()
        {
            StardogConnector stardog = StardogTests.GetConnection();
            IGraph g;

            // Begin a transaction
            stardog.Begin();

            // Try to make an update
            stardog.Update(
                "DROP SILENT GRAPH <http://example.org/stardog/update/4>; INSERT DATA { GRAPH <http://example.org/stardog/update/4> { <http://x> <http://y> <http://z> } }");

            // Commit the transaction
            stardog.Commit();

            g = new Graph();
            stardog.LoadGraph(g, "http://example.org/stardog/update/4");
            Assert.False(g.IsEmpty, "Graph should not be empty after update");
            Assert.Equal(1, g.Triples.Count);

            stardog.Dispose();
        }

        [SkippableFact]
        public void StorageStardogSparqlUpdate5()
        {
            StardogConnector stardog = StardogTests.GetConnection();
            IGraph g;

            // Begin a transaction
            stardog.Begin();

            // Try to make an update
            stardog.Update(
                "DROP SILENT GRAPH <http://example.org/stardog/update/5>; INSERT DATA { GRAPH <http://example.org/stardog/update/5> { <http://x> <http://y> <http://z> } }");

            // Rollback the transaction
            stardog.Rollback();

            g = new Graph();
            stardog.LoadGraph(g, "http://example.org/stardog/update/5");
            Assert.False(g.IsEmpty, "Graph should not be empty after update");
            Assert.Equal(1, g.Triples.Count);

            stardog.Dispose();
        }

        [SkippableFact]
        public void StorageStardogIsReadyValidDb()
        {
            StardogConnector stardog = StardogTests.GetConnection();
            Assert.True(stardog.IsReady);

            stardog.Dispose();
        }

        [SkippableFact]
        public void StorageStardogIsReadyInvalidDb()
        {
            Skip.IfNot(TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseStardog), "Test Config marks Stardog as unavailable, test cannot be run");
            var stardog =  new StardogConnector(TestConfigManager.GetSetting(TestConfigManager.StardogServer),
                "i_dont_exist",
                TestConfigManager.GetSetting(TestConfigManager.StardogUser),
                TestConfigManager.GetSetting(TestConfigManager.StardogPassword));
            Assert.False(stardog.IsReady);

            stardog.Dispose();
        }
    }
}