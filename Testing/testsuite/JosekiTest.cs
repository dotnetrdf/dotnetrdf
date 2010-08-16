using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage;

namespace dotNetRDFTest
{
    public class JosekiTest
    {
        public static void Main(String[] args)
        {
            StreamWriter output  = new StreamWriter("JosekiTest.txt");
            Console.SetOut(output);
            try
            {
                Console.WriteLine("## Joseki Test");
                Console.WriteLine();

                //Load the Graph we want to use as a Test
                Graph g = new Graph();
                TurtleParser ttlparser = new TurtleParser();
                ttlparser.Load(g, "InferenceTest.ttl");
                Console.WriteLine("Test Graph contains the following Triples:");
                ShowGraph(g);
                Console.WriteLine();

                //Load another Graph
                Graph h = new Graph();
                h.BaseUri = new Uri("http://example.org/test");
                Notation3Parser n3parser = new Notation3Parser();
                n3parser.Load(h, "test.n3");
                Console.WriteLine("Second Test Graph contains the following Triples:");
                ShowGraph(h);
                Console.WriteLine();

                Console.WriteLine("Trying to create a connection to the Store");
                JosekiConnector joseki = new JosekiConnector("http://nottm-virtual.ecs.soton.ac.uk:2020/", "sparql", "update");
                Console.WriteLine("Connection created OK");
                Console.WriteLine();

                //Options.HttpDebugging = true;
                //Options.HttpFullDebugging = true;

                Console.WriteLine("Trying to add data to the Store");
                joseki.SaveGraph(g);
                //joseki.SaveGraph(h);
                Console.WriteLine("Saved OK");
                Console.WriteLine();

                Console.WriteLine("Trying to load data from the Store");
                Graph i = new Graph();
                joseki.LoadGraph(i, String.Empty);
                ShowGraph(i);
                Console.WriteLine();
                //i = new Graph();
                //agraph.LoadGraph(i, new Uri("http://example.org/test"));
                //ShowGraph(i);
                Console.WriteLine("Loaded OK");
                Console.WriteLine();

                //Console.WriteLine("Trying to update data in the Store");
                //List<Triple> toRemove = g.GetTriplesWithPredicate(g.CreateUriNode("rdf:type")).ToList();
                //Triple toAdd = new Triple(g.CreateUriNode(new Uri("http://example.org/")), g.CreateUriNode("rdf:type"), g.CreateLiteralNode("Added Triple Test"));
                //agraph.UpdateGraph(String.Empty, new List<Triple>() { toAdd }, toRemove);
                //Console.WriteLine("Updated OK");
                //Console.WriteLine();

                //Console.WriteLine("Trying a Sparql ASK query against the store");
                //Object results = agraph.Query("ASK WHERE {?s ?p ?o}");
                //Console.WriteLine("Got results OK");
                //ShowResults(results);
                //Console.WriteLine();

                //Console.WriteLine("Trying a Sparql SELECT query against the store");
                //results = agraph.Query("SELECT * WHERE {?s ?p ?o}");
                //Console.WriteLine("Got results OK");
                //ShowResults(results);
                //Console.WriteLine();

            }
            catch (RdfStorageException storeEx)
            {
                reportError(output, "RDF Storage Error", storeEx);
            }
            catch (RdfParseException parseEx)
            {
                reportError(output, "RDF Parsing Error", parseEx);
            }
            catch (RdfQueryException queryEx)
            {
                reportError(output, "RDF Query Error", queryEx);
            }
            catch (RdfException rdfEx)
            {
                reportError(output, "RDF Error", rdfEx);
            }
            catch (WebException webEx)
            {
                reportError(output, "HTTP Error", webEx);
            }
            catch (Exception ex)
            {
                reportError(output, "Error", ex);
            }
            finally
            {
                output.Close();
            }
        }

        public static void reportError(StreamWriter output, String header, Exception ex)
        {
            output.WriteLine(header);
            output.WriteLine(ex.Message);
            output.WriteLine(ex.StackTrace);

            Exception innerEx = ex.InnerException;
            while (innerEx != null)
            {
                output.WriteLine();
                output.WriteLine(innerEx.Message);
                output.WriteLine(innerEx.StackTrace);
                innerEx = innerEx.InnerException;
            }
        }


        public static void ShowResults(Object results)
        {
            if (results is Graph)
            {
                ShowGraph((Graph)results);
            }
            else if (results is SparqlResultSet)
            {
                SparqlResultSet resultSet = (SparqlResultSet)results;
                Console.WriteLine("Result: " + resultSet.Result);
                Console.WriteLine(resultSet.Results.Count + " Results");
                foreach (SparqlResult r in resultSet.Results)
                {
                    Console.WriteLine(r.ToString());
                }
            }
            else
            {
                throw new ArgumentException("Expected a Graph or a SPARQLResultSet");
            }
        }

        public static void ShowGraph(IGraph g)
        {
            Console.Write("Graph URI: ");
            if (g.BaseUri != null)
            {
                Console.WriteLine(g.BaseUri.ToString());
            }
            else
            {
                Console.WriteLine("NULL");
            }
            Console.WriteLine(g.Triples.Count + " Triples");
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString());
            }
        }
    }
}
