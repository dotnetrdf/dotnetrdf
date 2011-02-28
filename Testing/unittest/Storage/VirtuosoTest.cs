using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Storage;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing.Formatting;
using OpenLink.Data.Virtuoso;

namespace VDS.RDF.Test.Storage
{
    [TestClass]
    public class VirtuosoTest : BaseTest
    {
        /// <summary>
        /// Test Account to use for Virtuoso testing - set to Virtuoso default DBA login by default
        /// </summary>
        public const String VirtuosoTestUsername = "dba",
                            VirtuosoTestPassword = "dba";

        [TestMethod]
        public void VirtuosoLoadGraph()
        {
            NTriplesFormatter formatter = new NTriplesFormatter();
            try
            {
                VirtuosoManager manager = new VirtuosoManager("DB", VirtuosoTestUsername, VirtuosoTestPassword);
                Assert.IsNotNull(manager);

                Console.WriteLine("Got the Virtuoso Manager OK");

                //Add the Test Date to Virtuoso
                Graph testData = new Graph();
                FileLoader.Load(testData, "MergePart1.ttl");
                testData.BaseUri = new Uri("http://localhost/VirtuosoTest");
                manager.SaveGraph(testData);
                testData = new Graph();
                FileLoader.Load(testData, "Turtle.ttl");
                testData.BaseUri = new Uri("http://localhost/TurtleImportTest");
                manager.SaveGraph(testData);
                Console.WriteLine("Saved the Test Data to Virtuoso");

                //Try loading it back again
                Graph g = new Graph();
                manager.LoadGraph(g, "http://localhost/VirtuosoTest");

                Console.WriteLine("Load Operation completed without errors");

                Assert.AreNotEqual(0, g.Triples.Count);

                foreach (Triple t in g.Triples)
                {
                    Console.WriteLine(t.ToString(formatter));
                }

                Console.WriteLine();
                Console.WriteLine("Loading a larger Graph");
                Graph h = new Graph();
                manager.LoadGraph(h, "http://localhost/TurtleImportTest");

                Console.WriteLine("Load operation completed without errors");

                Assert.AreNotEqual(0, h.Triples.Count);

                foreach (Triple t in h.Triples)
                {
                    Console.WriteLine(t.ToString(formatter));
                }

                Console.WriteLine();
                Console.WriteLine("Loading same Graph again to ensure loading is repeatable");
                Graph i = new Graph();
                manager.LoadGraph(i, "http://localhost/TurtleImportTest");

                Console.WriteLine("Load operation completed without errors");

                Assert.AreEqual(h.Triples.Count, i.Triples.Count);
                Assert.AreEqual(h, i);
            }
            catch (VirtuosoException virtEx)
            {
                TestTools.ReportError("Virtuoso Error", virtEx, true);
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Other Error", ex, true);
            }
        }

        [TestMethod]
        public void VirtuosoSaveGraph()
        {
            NTriplesFormatter formatter = new NTriplesFormatter();
            try
            {
                VirtuosoManager manager = new VirtuosoManager("DB", VirtuosoTestUsername, VirtuosoTestPassword);
                Assert.IsNotNull(manager);

                Console.WriteLine("Got the Virtuoso Manager OK");

                //Load in our Test Graph
                TurtleParser ttlparser = new TurtleParser();
                Graph g = new Graph();
                ttlparser.Load(g, "Turtle.ttl");

                Console.WriteLine();
                Console.WriteLine("Loaded Test Graph OK");
                Console.WriteLine("Test Graph contains:");

                Assert.IsFalse(g.IsEmpty, "Test Graph should be non-empty");

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
                manager.LoadGraph(h, "http://example.org");

                Assert.IsFalse(h.IsEmpty, "Retrieved Graph should be non-empty");

                Console.WriteLine("Retrieved the Graph from Virtuoso OK");
                Console.WriteLine("Retrieved Graph contains:");
                foreach (Triple t in h.Triples)
                {
                    Console.WriteLine(t.ToString(formatter));
                }

                Assert.Inconclusive("Virtuoso has known issues around not correctly returning datatypes on numeric literals and converts booleans to integers internally");

                Assert.AreEqual(g.Triples.Count, h.Triples.Count, "Graph should have same number of Triples before and after saving");
                Assert.AreEqual(g, h, "Graph should be equal before and after");
            }
            catch (VirtuosoException virtEx)
            {
                TestTools.ReportError("Virtuoso Error", virtEx, true);
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Other Error", ex, true);
            }
        }

        [TestMethod]
        public void VirtuosoDataTypes()
        {
            VirtuosoManager manager = new VirtuosoManager("DB", VirtuosoTestUsername, VirtuosoTestPassword);
            Assert.IsNotNull(manager);

            NTriplesFormatter formatter = new NTriplesFormatter();

            //Try to retrieve
            Graph g = new Graph();
            manager.LoadGraph(g, "http://localhost/TurtleImportTest");

            Assert.IsFalse(g.IsEmpty, "Retrieved Graph should be non-empty");

            Console.WriteLine("Retrieved the Graph from Virtuoso OK");
            Console.WriteLine("Retrieved Graph contains:");
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }
        }

        [TestMethod]
        public void VirtuosoDeleteGraph()
        {
            try
            {
                VirtuosoManager manager = new VirtuosoManager("DB", VirtuosoTestUsername, VirtuosoTestPassword);
                Assert.IsNotNull(manager);

                Console.WriteLine("Got the Virtuoso Manager OK");

                //Load in our Test Graph
                TurtleParser ttlparser = new TurtleParser();
                Graph g = new Graph();
                ttlparser.Load(g, "Turtle.ttl");
                g.BaseUri = new Uri("http://example.org/deleteMe");

                Console.WriteLine();
                Console.WriteLine("Loaded Test Graph OK");
                Console.WriteLine("Test Graph contains:");

                Assert.IsFalse(g.IsEmpty, "Test Graph should be non-empty");

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

                Assert.IsFalse(h.IsEmpty, "Retrieved Graph should be non-empty");

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

                Assert.IsTrue(i.IsEmpty, "Retrieved Graph should be empty as it should have been deleted from the Store");
            }
            catch (VirtuosoException virtEx)
            {
                TestTools.ReportError("Virtuoso Error", virtEx, true);
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Other Error", ex, true);
            }
        }

        [TestMethod]
        public void VirtuosoBlankNodePersistence()
        {
            try
            {
                //Create our Test Graph
                Graph g = new Graph();
                g.BaseUri = new Uri("http://example.org/bnodes/");

                BlankNode b = g.CreateBlankNode("blank");
                UriNode rdfType = g.CreateUriNode("rdf:type");
                UriNode bnode = g.CreateUriNode(":BlankNode");

                g.Assert(new Triple(b, rdfType, bnode));

                Assert.AreEqual(1, g.Triples.Count, "Should only be 1 Triple in the Test Graph");

                //Connect to Virtuoso
                VirtuosoManager manager = new VirtuosoManager("DB", VirtuosoTestUsername, VirtuosoTestPassword);

                //Save Graph
                manager.SaveGraph(g);

                //Retrieve
                Graph h = new Graph();
                Graph i = new Graph();
                manager.LoadGraph(h, g.BaseUri);
                manager.LoadGraph(i, g.BaseUri);

                Assert.AreEqual(1, h.Triples.Count, "Retrieved Graph should only contain 1 Triple");
                Assert.AreEqual(1, i.Triples.Count, "Retrieved Graph should only contain 1 Triple");

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
            catch (VirtuosoException virtEx)
            {
                TestTools.ReportError("Virtuoso Error", virtEx, true);
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Other Error", ex, true);
            }
        }

        /// <summary>
        /// Tests retrieving the Default Graph
        /// </summary>
        /// <remarks>Excluded from Unit Tests because while this works it takes an extremely long time since the Default Graph will potentially have a very large number of Triples and the Virtuoso Manager only reads in a single threaded manner</remarks>
        //[TestMethod]
        public void VirtuosoDefaultGraph()
        {
            Stopwatch timer = new Stopwatch();
            try
            {                
                VirtuosoManager manager = new VirtuosoManager("DB", VirtuosoTestUsername, VirtuosoTestPassword);
                Assert.IsNotNull(manager);

                Console.WriteLine("Got the Virtuoso Manager OK");

                //Try to get the Default Graph
                Graph g = new Graph();
                timer.Start();
                manager.LoadGraph(g, (Uri)null);
                timer.Stop();

                Console.WriteLine();
                Console.WriteLine("Default Graph Contains:");
                foreach (Triple t in g.Triples)
                {
                    Console.WriteLine(t.ToString());
                }
                Console.WriteLine();
            }
            catch (VirtuosoException virtEx)
            {
                timer.Stop();
                Console.WriteLine(timer.ElapsedMilliseconds + "ms Elapsed");
                TestTools.ReportError("Virtuoso Error", virtEx, true);
            }
            catch (Exception ex)
            {
                timer.Stop();
                Console.WriteLine(timer.ElapsedMilliseconds + "ms Elapsed");
                TestTools.ReportError("Other Error", ex, true);
            }
        }

        [TestMethod]
        public void VirtuosoUpdateGraph()
        {
            try
            {
                VirtuosoManager manager = new VirtuosoManager("DB", VirtuosoTestUsername, VirtuosoTestPassword);
                Assert.IsNotNull(manager);

                Console.WriteLine("Got the Virtuoso Manager OK");

                //Make some Triples to add to the Graph
                Graph g = new Graph();
                g.BaseUri = new Uri("http://example.org/");
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

                Assert.IsTrue(h.Triples.Contains(additions[0]), "Added Triple should be in the retrieved Graph");
                Assert.IsFalse(h.Triples.Contains(removals[0]), "Removed Triple should not be in the retrieved Graph");
            }
            catch (VirtuosoException virtEx)
            {
                TestTools.ReportError("Virtuoso Error", virtEx, true);
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Other Error", ex, true);
            }
        }

        [TestMethod]
        public void VirtuosoNativeQuery()
        {
            NTriplesFormatter formatter = new NTriplesFormatter();
            try
            {
                VirtuosoManager manager = new VirtuosoManager("DB", VirtuosoTestUsername, VirtuosoTestPassword);
                Assert.IsNotNull(manager);

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

                Console.WriteLine("SELECTing the same Graph");

                //Try a SELECT query
                result = manager.Query("SELECT * FROM <http://example.org/> WHERE {?s ?p ?o}");
                CheckQueryResult(result, true);

                Console.WriteLine("Now we'll delete a Triple");

                //Try a DELETE DATA
                result = manager.Query("DELETE DATA FROM <http://example.org/> { <http://example.org/seven> <http://example.org/property> \"extra triple\".}");
                CheckQueryResult(result, true);

                Console.WriteLine("Triple with subject <http://example.org/seven> should be missing - ASKing for it...");

                //Try a SELECT query
                result = manager.Query("ASK FROM <http://example.org/> WHERE {<http://example.org/seven> ?p ?o}");
                CheckQueryResult(result, true);

                Console.WriteLine("Now we'll add the Triple back again");

                //Try a INSERT DATA
                result = manager.Query("INSERT DATA INTO <http://example.org/> { <http://example.org/seven> <http://example.org/property> \"extra triple\".}");
                CheckQueryResult(result, true);

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
            catch (RdfQueryException queryEx)
            {
                TestTools.ReportError("RDF Query Error", queryEx, true);
            }
            catch (VirtuosoException virtEx)
            {
                TestTools.ReportError("Virtuoso Error", virtEx, true);
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Other Error", ex, true);
            }
        }

        [TestMethod]
        public void VirtuosoNativeUpdate()
        {
            try
            {
                VirtuosoManager manager = new VirtuosoManager("DB", VirtuosoTestUsername, VirtuosoTestPassword);
                Assert.IsNotNull(manager);

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
            catch (RdfQueryException queryEx)
            {
                TestTools.ReportError("RDF Query Error", queryEx, true);
            }
            catch (VirtuosoException virtEx)
            {
                TestTools.ReportError("Virtuoso Error", virtEx, true);
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Other Error", ex, true);
            }
        }

        [TestMethod]
        public void VirtuosoEncoding()
        {
            //Get the Virtuoso Manager
            VirtuosoManager manager = new VirtuosoManager("DB", VirtuosoTestUsername, VirtuosoTestPassword);

            //Make the Test Graph
            Graph g = new Graph();
            g.BaseUri = new Uri("http://example.org/VirtuosoEncodingTest");
            UriNode encodedString = g.CreateUriNode(new Uri("http://example.org/encodedString"));
            LiteralNode encodedText = g.CreateLiteralNode("William Jørgensen");
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

            Assert.AreEqual(g, h, "Graphs should be equal");
        }

        private static void CheckQueryResult(Object results, bool expectResultSet)
        {
            NTriplesFormatter formatter = new NTriplesFormatter();
            if (results == null) 
            {
                Assert.Fail("Got a Null Result from a Query");
            }
            else if (results is SparqlResultSet)
            {
                if (expectResultSet)
                {
                    Console.WriteLine("Got a SPARQLResultSet as expected");

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
                    Assert.Fail("Expected a SPARQLResultSet but got a '" + results.GetType().ToString() + "'");
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
                    Assert.Fail("Expected a Graph but got a '" + results.GetType().ToString() + "'");
                }
            }
        }
    }
}
