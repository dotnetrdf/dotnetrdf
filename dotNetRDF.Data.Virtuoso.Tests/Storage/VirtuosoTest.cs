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
using System.Diagnostics;
using System.Linq;
using System.Text;
using Xunit;
using OpenLink.Data.Virtuoso;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Storage
{

    public class VirtuosoTest : BaseTest
    {
        public static VirtuosoManager GetConnection()
        {
            Skip.IfNot(TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseVirtuoso), "Test Config marks Virtuoso as unavailable, test cannot be run");
            return new VirtuosoManager(TestConfigManager.GetSetting(TestConfigManager.VirtuosoServer), TestConfigManager.GetSettingAsInt(TestConfigManager.VirtuosoPort), TestConfigManager.GetSetting(TestConfigManager.VirtuosoDatabase), TestConfigManager.GetSetting(TestConfigManager.VirtuosoUser), TestConfigManager.GetSetting(TestConfigManager.VirtuosoPassword));
        }

        [SkippableFact]
        public void StorageVirtuosoLoadGraph()
        {
            NTriplesFormatter formatter = new NTriplesFormatter();
            VirtuosoManager manager = VirtuosoTest.GetConnection();
            try
            {
                Assert.NotNull(manager);

                Console.WriteLine("Got the Virtuoso Manager OK");

                //Add the Test Date to Virtuoso
                Graph testData = new Graph();
                FileLoader.Load(testData, @"..\\resources\MergePart1.ttl");
                testData.BaseUri = new Uri("http://localhost/VirtuosoTest");
                manager.SaveGraph(testData);
                testData = new Graph();
                FileLoader.Load(testData, @"..\\resources\Turtle.ttl");
                testData.BaseUri = new Uri("http://localhost/TurtleImportTest");
                manager.SaveGraph(testData);
                Console.WriteLine("Saved the Test Data to Virtuoso");

                //Try loading it back again
                Graph g = new Graph();
                manager.LoadGraph(g, "http://localhost/VirtuosoTest");

                Console.WriteLine("Load Operation completed without errors");

                Assert.NotEqual(0, g.Triples.Count);

                foreach (Triple t in g.Triples)
                {
                    Console.WriteLine(t.ToString(formatter));
                }

                Console.WriteLine();
                Console.WriteLine("Loading a larger Graph");
                Graph h = new Graph();
                manager.LoadGraph(h, "http://localhost/TurtleImportTest");

                Console.WriteLine("Load operation completed without errors");

                Assert.NotEqual(0, h.Triples.Count);

                foreach (Triple t in h.Triples)
                {
                    Console.WriteLine(t.ToString(formatter));
                }

                Console.WriteLine();
                Console.WriteLine("Loading same Graph again to ensure loading is repeatable");
                Graph i = new Graph();
                manager.LoadGraph(i, "http://localhost/TurtleImportTest");

                Console.WriteLine("Load operation completed without errors");

                Assert.Equal(h.Triples.Count, i.Triples.Count);
                Assert.Equal(h, i);
            }
            finally
            {
                if (manager != null) manager.Dispose();
            }
        }

        [SkippableFact]
        public void StorageVirtuosoLoadGraphWithNullHandler()
        {
            NTriplesFormatter formatter = new NTriplesFormatter();
            VirtuosoManager manager = VirtuosoTest.GetConnection();
            try
            {
                Assert.NotNull(manager);

                Console.WriteLine("Got the Virtuoso Manager OK");

                //Add the Test Date to Virtuoso
                Graph testData = new Graph();
                FileLoader.Load(testData, "resources\\Turtle.ttl");
                testData.BaseUri = new Uri("http://example.org/virtuoso/tests/null");
                manager.SaveGraph(testData);
                Console.WriteLine("Saved the Test Data to Virtuoso");

                NullHandler handler = new NullHandler();
                manager.LoadGraph(handler, testData.BaseUri);
            }
            finally
            {
                if (manager != null) manager.Dispose();
            }
        }

        [SkippableFact]
        public void StorageVirtuosoSaveGraph()
        {
            NTriplesFormatter formatter = new NTriplesFormatter();
            VirtuosoManager manager = VirtuosoTest.GetConnection();
            try
            {
                Assert.NotNull(manager);

                Console.WriteLine("Got the Virtuoso Manager OK");

                //Load in our Test Graph
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                g.BaseUri = new Uri("http://example.org/storage/virtuoso/save");

                Console.WriteLine();
                Console.WriteLine("Loaded Test Graph OK");
                Console.WriteLine("Test Graph contains:");

                Assert.False(g.IsEmpty, "Test Graph should be non-empty");

                foreach (Triple t in g.Triples)
                {
                    Console.WriteLine(t.ToString(formatter));
                }
                Console.WriteLine();

                //Try to save to Virtuoso
                manager.SaveGraph(g);
                Console.WriteLine("Saved OK");
                Console.WriteLine();

                //Try to retrieve
                Graph h = new Graph();
                manager.LoadGraph(h, g.BaseUri);

                Assert.False(h.IsEmpty, "Retrieved Graph should be non-empty");

                Console.WriteLine("Retrieved the Graph from Virtuoso OK");
                Console.WriteLine("Retrieved Graph contains:");
                foreach (Triple t in h.Triples)
                {
                    Console.WriteLine(t.ToString(formatter));
                }

                Assert.Equal(g.Triples.Count, h.Triples.Count);

                GraphDiffReport diff = h.Difference(g);
                if (!diff.AreEqual)
                {
                    TestTools.ShowDifferences(diff);
                }

                Console.WriteLine();
                Assert.Equal(g, h);
            }
            finally
            {
                if (manager != null) manager.Dispose();
            }
        }

        [SkippableFact]
        public void StorageVirtuosoDeleteGraph()
        {
            VirtuosoManager manager = VirtuosoTest.GetConnection();
            try
            {
                Assert.NotNull(manager);

                Console.WriteLine("Got the Virtuoso Manager OK");

                //Load in our Test Graph
                TurtleParser ttlparser = new TurtleParser();
                Graph g = new Graph();
                ttlparser.Load(g, "resources\\Turtle.ttl");
                g.BaseUri = new Uri("http://example.org/deleteMe");

                Console.WriteLine();
                Console.WriteLine("Loaded Test Graph OK");
                Console.WriteLine("Test Graph contains:");

                Assert.False(g.IsEmpty, "Test Graph should be non-empty");

                foreach (Triple t in g.Triples)
                {
                    Console.WriteLine(t.ToString());
                }
                Console.WriteLine();

                //Try to save to Virtuoso
                manager.SaveGraph(g);
                Console.WriteLine("Saved OK");
                Console.WriteLine();

                //Try to retrieve
                Graph h = new Graph();
                manager.LoadGraph(h, "http://example.org/deleteMe");

                Assert.False(h.IsEmpty, "Retrieved Graph should be non-empty");

                Console.WriteLine("Retrieved the Graph from Virtuoso OK");
                Console.WriteLine("Retrieved Graph contains:");
                foreach (Triple t in h.Triples)
                {
                    Console.WriteLine(t.ToString());
                }

                //Try to delete
                manager.DeleteGraph("http://example.org/deleteMe");
                Graph i = new Graph();
                manager.LoadGraph(i, "http://example.org/deleteMe");

                Assert.True(i.IsEmpty, "Retrieved Graph should be empty as it should have been deleted from the Store");
            }
            finally
            {
                if (manager != null) manager.Dispose();
            }
        }

        [SkippableFact]
        public void StorageVirtuosoBlankNodePersistence()
        {
            //Create our Test Graph
            Graph g = new Graph();
            g.BaseUri = new Uri("http://example.org/bnodes/");
            g.NamespaceMap.AddNamespace(String.Empty, g.BaseUri);

            IBlankNode b = g.CreateBlankNode("blank");
            IUriNode rdfType = g.CreateUriNode("rdf:type");
            IUriNode bnode = g.CreateUriNode(":BlankNode");

            g.Assert(new Triple(b, rdfType, bnode));

            Assert.Equal(1, g.Triples.Count);

            //Connect to Virtuoso
            VirtuosoManager manager = VirtuosoTest.GetConnection();

            try
            {
                //Save Graph
                manager.SaveGraph(g);

                //Retrieve
                Graph h = new Graph();
                Graph i = new Graph();
                manager.LoadGraph(h, g.BaseUri);
                manager.LoadGraph(i, g.BaseUri);

                Assert.Equal(1, h.Triples.Count);
                Assert.Equal(1, i.Triples.Count);

                Console.WriteLine("Contents of Retrieved Graph:");
                foreach (Triple t in h.Triples)
                {
                    Console.WriteLine(t.ToString());
                }
                Console.WriteLine();

                TestTools.CompareGraphs(h, i, true);

                //Save back again
                manager.SaveGraph(h);

                //Retrieve again
                Graph j = new Graph();
                manager.LoadGraph(j, g.BaseUri);

                Console.WriteLine("Contents of Retrieved Graph (Retrieved after saving original Retrieval):");
                foreach (Triple t in j.Triples)
                {
                    Console.WriteLine(t.ToString());
                }
                Console.WriteLine();

                TestTools.CompareGraphs(h, j, false);
                TestTools.CompareGraphs(i, j, false);

                //Save back yet again
                manager.SaveGraph(j);

                //Retrieve yet again
                Graph k = new Graph();
                manager.LoadGraph(k, g.BaseUri);

                Console.WriteLine("Contents of Retrieved Graph (Retrieved after saving second Retrieval):");
                foreach (Triple t in k.Triples)
                {
                    Console.WriteLine(t.ToString());
                }
                Console.WriteLine();

                TestTools.CompareGraphs(j, k, false);
            }
            finally
            {
                if (manager != null) manager.Dispose();
            }
        }

        [SkippableFact]
        public void StorageVirtuosoUpdateGraph()
        {
            VirtuosoManager manager = VirtuosoTest.GetConnection();
            try
            {
                Assert.NotNull(manager);

                Console.WriteLine("Got the Virtuoso Manager OK");

                //Make some Triples to add to the Graph
                Graph g = new Graph();
                g.BaseUri = new Uri("http://example.org/");
                g.NamespaceMap.AddNamespace(String.Empty, g.BaseUri);

                List<Triple> additions = new List<Triple>();
                additions.Add(new Triple(g.CreateUriNode(":seven"), g.CreateUriNode("rdf:type"), g.CreateUriNode(":addedTriple")));
                List<Triple> removals = new List<Triple>();
                removals.Add(new Triple(g.CreateUriNode(":five"), g.CreateUriNode(":property"), g.CreateUriNode(":value")));

                //Try to Update
                manager.UpdateGraph(g.BaseUri, additions, removals);

                Console.WriteLine("Updated OK");

                //Retrieve to see what's in the Graph
                Graph h = new Graph();
                manager.LoadGraph(h, g.BaseUri);

                Console.WriteLine("Retrieved OK");
                Console.WriteLine("Graph contents are:");
                foreach (Triple t in h.Triples)
                {
                    Console.WriteLine(t.ToString());
                }

                Assert.True(h.Triples.Contains(additions[0]), "Added Triple should be in the retrieved Graph");
                Assert.False(h.Triples.Contains(removals[0]), "Removed Triple should not be in the retrieved Graph");
            }
            finally
            {
                if (manager != null) manager.Dispose();
            }
        }

        [SkippableFact]
        public void StorageVirtuosoNativeQueryAndUpdate()
        {
            NTriplesFormatter formatter = new NTriplesFormatter();
            VirtuosoManager manager = VirtuosoTest.GetConnection();
            try
            {
                Assert.NotNull(manager);

                Console.WriteLine("Got the Virtuoso Manager OK");

                Object result = null;

                Console.WriteLine("ASKing if there's any Triples");

                //Try an ASK query
                result = manager.Query("ASK {?s ?p ?o}");
                CheckQueryResult(result, true);

                Console.WriteLine("CONSTRUCTing a Graph");

                //Try a CONSTRUCT
                result = manager.Query("CONSTRUCT {?s ?p ?o} FROM <http://example.org/> WHERE {?s ?p ?o}");
                CheckQueryResult(result, false);

                Console.WriteLine("DESCRIBEing a resource");

                //Try a DESCRIBE
                result = manager.Query("DESCRIBE <http://example.org/one>");
                CheckQueryResult(result, false);

                //Try another DESCRIBE
                result = manager.Query("DESCRIBE <http://example.org/noSuchThing>");
                CheckQueryResult(result, false);

                Console.WriteLine("SELECTing the same Graph");

                //Try a SELECT query
                result = manager.Query("SELECT * FROM <http://example.org/> WHERE {?s ?p ?o}");
                CheckQueryResult(result, true);

                Console.WriteLine("SELECTing the same Graph using Project Expressions");

                //Try a SELECT query
                result = manager.Query("SELECT (?s AS ?Subject) (?P as ?Predicate) (?o AS ?Object) FROM <http://example.org/> WHERE {?s ?p ?o}");
                CheckQueryResult(result, true);

                Console.WriteLine("Now we'll delete a Triple");

                //Try a DELETE DATA
                manager.Update("DELETE DATA { GRAPH <http://example.org/> { <http://example.org/seven> <http://example.org/property> \"extra triple\".} }");
                Console.WriteLine("Triple with subject <http://example.org/seven> should be missing - ASKing for it...");

                //Try a SELECT query
                result = manager.Query("ASK FROM <http://example.org/> WHERE {<http://example.org/seven> ?p ?o}");
                CheckQueryResult(result, true);

                Console.WriteLine("Now we'll add the Triple back again");

                //Try a INSERT DATA
                manager.Update("INSERT DATA INTO <http://example.org/> { <http://example.org/seven> <http://example.org/property> \"extra triple\".}");
                Console.WriteLine("Triple with subject <http://example.org/seven> should be restored - ASKing for it again...");

                //Try a SELECT query
                result = manager.Query("ASK FROM <http://example.org/> WHERE {<http://example.org/seven> ?p ?o}");
                CheckQueryResult(result, true);

                Console.WriteLine("DESCRIBE things which are classes");

                //Try a DESCRIBE
                result = manager.Query("PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#> DESCRIBE ?s WHERE {?s a rdfs:Class} LIMIT 1");
                CheckQueryResult(result, false);

                Console.WriteLine("Another CONSTRUCT");

                //Try a CONSTRUCT
                result = manager.Query("CONSTRUCT {?s ?p ?o} FROM <http://example.org/> WHERE {?s ?p ?o}");
                CheckQueryResult(result, false);

                Console.WriteLine("COUNT subjects");

                //Try a SELECT using an aggregate function
                result = manager.Query("SELECT COUNT(?s) FROM <http://example.org/> WHERE {?s ?p ?o}");
                CheckQueryResult(result, true);

                Console.WriteLine("AVG integer objects");

                //Try another SELECT using an aggregate function
                result = manager.Query("SELECT AVG(?o) FROM <http://example.org/> WHERE {?s ?p ?o. FILTER(DATATYPE(?o) = <http://www.w3.org/2001/XMLSchema#integer>)}");
                CheckQueryResult(result, true);

                Console.WriteLine("AVG decimal objects");

                //Try yet another SELECT using an aggregate function
                result = manager.Query("SELECT AVG(?o) FROM <http://example.org/> WHERE {?s ?p ?o. FILTER(DATATYPE(?o) = <http://www.w3c.org/2001/XMLSchema#decimal>)}");
                CheckQueryResult(result, true);
            }
            finally
            {
                if (manager != null) manager.Dispose();
            }
        }

        [SkippableFact]
        public void StorageVirtuosoNativeQueryBifContains1()
        {
            VirtuosoManager manager = VirtuosoTest.GetConnection();
            try
            {
                Assert.NotNull(manager);

                Console.WriteLine("Got the Virtuoso Manager OK");

                Object result = null;

                //Try an ASK query
                result = manager.Query("ASK { ?s ?p ?o . ?o bif:contains 'example' }");
                CheckQueryResult(result, true);

            }
            finally
            {
                if (manager != null) manager.Dispose();
            }
        }

        [SkippableFact]
        public void StorageVirtuosoNativeQueryBifContains2()
        {
            VirtuosoManager manager = VirtuosoTest.GetConnection();
            try
            {
                Assert.NotNull(manager);

                Console.WriteLine("Got the Virtuoso Manager OK");

                Object result = null;

                //Try a CONSTRUCT query
                result = manager.Query("CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o . ?o bif:contains 'example' }");
                CheckQueryResult(result, false);
            }
            finally
            {
                if (manager != null) manager.Dispose();
            }
        }

        [SkippableFact]
        public void StorageVirtuosoNativeUpdate()
        {
            VirtuosoManager manager = VirtuosoTest.GetConnection();
            try
            {
                Assert.NotNull(manager);

                Console.WriteLine("Got the Virtuoso Manager OK");

                Object result = null;
                Console.WriteLine("Now we'll delete a Triple");

                //Try a DELETE DATA
                manager.Update("DELETE DATA FROM <http://example.org/> { <http://example.org/seven> <http://example.org/property> \"extra triple\".}");

                Console.WriteLine("Triple with subject <http://example.org/seven> should be missing - ASKing for it...");

                //Try a SELECT query
                result = manager.Query("ASK FROM <http://example.org/> WHERE {<http://example.org/seven> ?p ?o}");
                CheckQueryResult(result, true);

                Console.WriteLine("Now we'll add the Triple back again");

                //Try a INSERT DATA
                manager.Update("INSERT DATA INTO <http://example.org/> { <http://example.org/seven> <http://example.org/property> \"extra triple\".}");

                Console.WriteLine("Triple with subject <http://example.org/seven> should be restored - ASKing for it again...");

                //Try a SELECT query
                result = manager.Query("ASK FROM <http://example.org/> WHERE {<http://example.org/seven> ?p ?o}");
                CheckQueryResult(result, true);
            }
            finally
            {
                if (manager != null) manager.Dispose();
            }
        }

        [SkippableFact]
        public void StorageVirtuosoEncoding()
        {
            //Get the Virtuoso Manager
            VirtuosoManager manager = VirtuosoTest.GetConnection();

            try
            {

                //Make the Test Graph
                Graph g = new Graph();
                g.BaseUri = new Uri("http://example.org/VirtuosoEncodingTest");
                IUriNode encodedString = g.CreateUriNode(new Uri("http://example.org/encodedString"));
                ILiteralNode encodedText = g.CreateLiteralNode("William Jørgensen");
                g.Assert(new Triple(g.CreateUriNode(), encodedString, encodedText));

                Console.WriteLine("Test Graph created OK");
                TestTools.ShowGraph(g);

                //Save to Virtuoso
                Console.WriteLine();
                Console.WriteLine("Saving to Virtuoso");
                manager.SaveGraph(g);

                //Load back from Virtuoso
                Console.WriteLine();
                Console.WriteLine("Retrieving from Virtuoso");
                Graph h = new Graph();
                manager.LoadGraph(h, new Uri("http://example.org/VirtuosoEncodingTest"));
                TestTools.ShowGraph(h);

                Assert.Equal(g, h);
            }
            finally
            {
                if (manager != null) manager.Dispose();
            }
        }

        [SkippableFact]
        public void StorageVirtuosoConfigSerialization()
        {
            NTriplesFormatter formatter = new NTriplesFormatter();
            VirtuosoManager manager = VirtuosoTest.GetConnection();
            try
            {
                Assert.NotNull(manager);

                Graph g = new Graph();
                INode rdfType = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
                INode dnrType = g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyType));
                INode objFactory = g.CreateUriNode(UriFactory.Create(ConfigurationLoader.ClassObjectFactory));
                INode virtFactory = g.CreateLiteralNode("VDS.RDF.Configuration.VirtuosoObjectFactory, dotNetRDF.Data.Virtuoso");
                INode genericManager = g.CreateUriNode(UriFactory.Create(ConfigurationLoader.ClassStorageProvider));
                INode virtManager = g.CreateLiteralNode("VDS.RDF.Storage.VirtuosoManager, dotNetRDF.Data.Virtuoso");

                //Serialize Configuration
                ConfigurationSerializationContext context = new ConfigurationSerializationContext(g);
                manager.SerializeConfiguration(context);

                Console.WriteLine("Serialized Configuration");
                foreach (Triple t in g.Triples)
                {
                    Console.WriteLine(t.ToString(formatter));
                }
                Console.WriteLine();

                //Ensure that it was serialized
                INode factory = g.GetTriplesWithPredicateObject(rdfType, objFactory).Select(t => t.Subject).FirstOrDefault();
                Assert.NotNull(factory);
                Assert.True(g.ContainsTriple(new Triple(factory, dnrType, virtFactory)), "Should contain a Triple declaring the dnr:type to be the Virtuoso Object factory type");
                INode objNode = g.GetTriplesWithPredicateObject(rdfType, genericManager).Select(t => t.Subject).FirstOrDefault();
                Assert.NotNull(objNode);
                Assert.True(g.ContainsTriple(new Triple(objNode, dnrType, virtManager)), "Should contain a Triple declaring the dnr:type to be the Virtuoso Manager type");

                //Serialize again
                manager.SerializeConfiguration(context);

                Console.WriteLine("Serialized Configuration (after 2nd pass)");
                foreach (Triple t in g.Triples)
                {
                    Console.WriteLine(t.ToString(formatter));
                }
                Console.WriteLine();

                //Ensure that object factory has not been serialized again 
                Assert.Single(g.GetTriplesWithPredicateObject(rdfType, objFactory));

                //Now try to load the object
                ConfigurationLoader.AutoConfigureObjectFactories(g);
                Object loadedObj = ConfigurationLoader.LoadObject(g, objNode);
                if (loadedObj is VirtuosoManager)
                {
                    Assert.Equal(manager.ToString(), loadedObj.ToString());
                }
                else
                {
                    Assert.True(false, "Returned an object of type '" + loadedObj.GetType().FullName + "' when deserializing");
                }
            }
            finally
            {
                if (manager != null) manager.Dispose();
            }
        }

        [SkippableFact]
        public void StorageVirtuosoQueryRegex()
        {
            VirtuosoManager manager = VirtuosoTest.GetConnection();

            try
            {

                //Create the Test Graph
                Graph g = new Graph();
                g.BaseUri = new Uri("http://example.org/VirtuosoRegexTest");
                INode subj1 = g.CreateUriNode(new Uri("http://example.org/one"));
                INode subj2 = g.CreateUriNode(new Uri("http://example.org/two"));
                INode pred = g.CreateUriNode(new Uri("http://example.org/predicate"));
                INode obj1 = g.CreateLiteralNode("search term");
                INode obj2 = g.CreateLiteralNode("no term");
                g.Assert(subj1, pred, obj1);
                g.Assert(subj2, pred, obj2);
                manager.SaveGraph(g);

                Graph h = new Graph();
                manager.LoadGraph(h, g.BaseUri);
                Assert.Equal(g, h);

                String query = "SELECT * FROM <" + g.BaseUri.ToString() + "> WHERE { ?s ?p ?o . FILTER(REGEX(STR(?o), 'search', 'i')) }";
                SparqlResultSet results = manager.Query(query) as SparqlResultSet;
                if (results == null) Assert.True(false, "Did not get a Result Set as expected");
                Console.WriteLine("Results obtained via VirtuosoManager.Query()");
                TestTools.ShowResults(results);
                Assert.Equal(1, results.Count);

                SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri("http://localhost:8890/sparql"));
                SparqlResultSet results2 = endpoint.QueryWithResultSet(query);
                Console.WriteLine("Results obtained via SparqlRemoteEndpoint.QueryWithResultSet()");
                TestTools.ShowResults(results2);
                Assert.Equal(1, results2.Count);

            }
            finally
            {
                if (manager != null) manager.Dispose();
            }
        }

        [SkippableFact]
        public void StorageVirtuosoBlankNodeInsert()
        {
            VirtuosoManager manager = VirtuosoTest.GetConnection();
            try
            {
                Graph g = new Graph();
                Triple t = new Triple(g.CreateBlankNode(), g.CreateUriNode("rdf:type"), g.CreateUriNode(UriFactory.Create("http://example.org/object")));

                manager.UpdateGraph("http://localhost/insertBNodeTest", t.AsEnumerable(), null);

                Object results = manager.Query("ASK WHERE { GRAPH <http://localhost/insertBNodeTest> { ?s a <http:///example.org/object> } }");
                if (results is SparqlResultSet)
                {
                    TestTools.ShowResults(results);
                    Assert.True(((SparqlResultSet)results).Result, "Expected a true result");
                }
                else
                {
                    Assert.True(false, "Didn't get a SPARQL Result Set as expected");
                }
            }
            finally
            {
                if (manager != null) manager.Dispose();
            }
        }

        [SkippableFact]
        public void StorageVirtuosoBlankNodeDelete()
        {
            //First ensure data is present in the store
            VirtuosoManager manager = VirtuosoTest.GetConnection();
            try
            {
                manager.DeleteGraph("http://localhost/deleteBNodeTest");
                Graph g = new Graph();
                Triple t = new Triple(g.CreateBlankNode(), g.CreateUriNode("rdf:type"), g.CreateUriNode(UriFactory.Create("http://example.org/object")));
                g.Assert(t);

                manager.UpdateGraph("http://localhost/deleteBNodeTest", t.AsEnumerable(), null);

                Object results = manager.Query("ASK WHERE { GRAPH <http://localhost/deleteBNodeTest> { ?s a <http://example.org/object> } }");
                if (results is SparqlResultSet)
                {
                    TestTools.ShowResults(results);
                    Assert.True(((SparqlResultSet)results).Result, "Expected a true result");

                    //Now we've ensured data is present we can first load the graph and then try to delete the given triple
                    Graph h = new Graph();
                    manager.LoadGraph(h, "http://localhost/deleteBNodeTest");
                    Assert.Equal(g, h);

                    //Then we can go ahead and delete the triples from this graph
                    manager.UpdateGraph("http://localhost/deleteBNodeTest", null, h.Triples);
                    Graph i = new Graph();
                    manager.LoadGraph(i, "http://localhost/deleteBNodeTest");
                    Assert.True(i.IsEmpty, "Graph should be empty");
                    Assert.NotEqual(h, i);
                    Assert.NotEqual(g, i);
                }
                else
                {
                    Assert.True(false, "Didn't get a SPARQL Result Set as expected");
                }
            }
            finally
            {
                if (manager != null) manager.Dispose();
            }
        }

        [SkippableFact]
        public void StorageVirtuosoRelativeUris()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            try
            {
                //Load in our Test Graph
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                g.BaseUri = new Uri("/storage/virtuoso/relative", UriKind.Relative);

                virtuoso.SaveGraph(g);

                // Load it back out again
                Graph h = new Graph();
                virtuoso.LoadGraph(h, g.BaseUri);
                Assert.False(h.IsEmpty, "Graph loaded via relative URI should not be empty");
                Assert.Equal(g, h);

                Console.WriteLine("Loading with relative URI works OK");
                Console.WriteLine();

                // Load it back out using marshalled URI
                Uri u = new Uri("virtuoso-relative:/storage/virtuoso/relative");
                h = new Graph();
                virtuoso.LoadGraph(h, u);
                Assert.False(h.IsEmpty, "Graph loaded via marshalled URI should not be empty");
                Assert.Equal(g, h);

                Console.WriteLine("Loading with marshalled URI works OK");
            }
            finally
            {
                virtuoso.Dispose();
            }
        }

        [SkippableFact]
        public void StorageVirtuosoBadUpdate()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            try
            {
                virtuoso.Update("Bad update");
                Assert.True(false, "Expected an error");
            }
            catch
            {
                //Expected so can be ignored
                Assert.False(virtuoso.HasOpenConnection, "Connection should be closed");
                Assert.False(virtuoso.HasActiveTransaction, "Should be no active transaction");
            }
            finally
            {
                virtuoso.Dispose();
            }
        }

        [SkippableFact]
        public void StorageVirtuosoBadQuery()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            try
            {
                virtuoso.Query("Bad query");
                Assert.True(false, "Expected an error");
            }
            catch
            {
                //Expected so can be ignored
                Assert.False(virtuoso.HasOpenConnection, "Connection should be closed");
                Assert.False(virtuoso.HasActiveTransaction, "Should be no active transaction");
            }
            finally
            {
                virtuoso.Dispose();
            }
        }

        [SkippableFact]
        public void StorageVirtuosoTightSaveLoop()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            try
            {
                for (int i = 0; i < 1000; i++)
                {
                    IGraph g = new Graph();
                    g.Assert(g.CreateBlankNode(), g.CreateUriNode("rdf:type"), g.CreateUriNode(new Uri("http://example.org/Type")));
                    g.BaseUri = new Uri("http://example.org/graphs/" + i);

                    virtuoso.SaveGraph(g);
                }
            }
            finally
            {
                virtuoso.Dispose();
            }
        }

        private static void CheckQueryResult(Object results, bool expectResultSet)
        {
            NTriplesFormatter formatter = new NTriplesFormatter();
            if (results == null) 
            {
                Assert.True(false, "Got a Null Result from a Query");
            }
            else if (results is SparqlResultSet)
            {
                if (expectResultSet)
                {
                    Console.WriteLine("Got a SPARQL Result Set as expected");

                    SparqlResultSet rset = (SparqlResultSet)results;
                    Console.WriteLine("Result = " + rset.Result);
                    foreach (SparqlResult r in rset)
                    {
                        Console.WriteLine(r.ToString(formatter));
                    }
                    Console.WriteLine();
                }
                else
                {
                    Assert.True(false, "Expected a Graph but got a '" + results.GetType().ToString() + "'");
                }
            }
            else if (results is Graph)
            {
                if (!expectResultSet)
                {
                    Console.WriteLine("Got a Graph as expected");
                    Graph g = (Graph)results;
                    Console.WriteLine(g.Triples.Count + " Triples");
                    foreach (Triple t in g.Triples)
                    {
                        Console.WriteLine(t.ToString(formatter));
                    }
                    Console.WriteLine();
                }
                else
                {
                    Assert.True(false, "Expected a Result Set but got a '" + results.GetType().ToString() + "'");
                }
            }
        }
    }
}