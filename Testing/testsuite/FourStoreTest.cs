using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage;

namespace dotNetRDFTest
{
    public class FourStoreTest
    {
        public static void Main(String[] args)
        {
            StreamWriter output = new StreamWriter("4StoreTest.txt");
            try
            {
                Console.SetOut(output);
                Console.WriteLine("## 4store Test");
                Console.WriteLine();

                //Load the Graph we want to use as a Test
                Graph g = new Graph();
                TurtleParser ttlparser = new TurtleParser();
                ttlparser.Load(g, "InferenceTest.ttl");
                Console.WriteLine("Test Graph contains the following Triples:");
                foreach (Triple t in g.Triples)
                {
                    Console.WriteLine(t.ToString());
                }
                Console.WriteLine();

                //Create the 4store Connector
                FourStoreConnector fourstore = new FourStoreConnector("http://nottm-virtual.ecs.soton.ac.uk:8080/");

                //Try to save a Graph to the Store
                Console.WriteLine("Attempting to save a Graph to the Store");
                g.BaseUri = new Uri("http://example.org/test");
                fourstore.SaveGraph(g);
                Console.WriteLine("Graph saved OK");
                Console.WriteLine();

                //Try to retrieve the Graph from the Store
                Console.WriteLine("Attempting to load the Graph back from the Store");
                Graph h = new Graph();
                fourstore.LoadGraph(h, g.BaseUri);
                Console.WriteLine("Graph loaded OK");
                Console.WriteLine("Contains the following Triples:");
                foreach (Triple t in h.Triples)
                {
                    Console.WriteLine(t.ToString());
                }
                Console.WriteLine();

                //Make a couple of queries against the store
                Console.WriteLine("Attempting to make an ASK query against the Store");
                Object results;
                results = fourstore.Query("ASK WHERE {<http://example.org/vehicles/FordFiesta> ?p ?o}");
                ShowResults(results);

                Console.WriteLine();
                Console.WriteLine("Attempting to make a SELECT query against the Store");
                results = fourstore.Query("SELECT * WHERE {<http://example.org/vehicles/FordFiesta> ?p ?o}");
                ShowResults(results);

                Console.WriteLine();
                Console.WriteLine("Attempting to make a DESCRIBE query against the Store");
                results = fourstore.Query("DESCRIBE <http://example.org/vehicles/FordFiesta>");
                ShowResults(results);

                Console.WriteLine();
                Console.WriteLine("Attempting to make a CONSTRUCT query against the Store");
                results = fourstore.Query("CONSTRUCT {<http://example.org/vehicles/FordFiesta> ?p ?o} WHERE {<http://example.org/vehicles/FordFiesta> ?p ?o}");
                ShowResults(results);

                //Now delete the data we added
                Console.WriteLine();
                Console.WriteLine("Attempting to delete the Graph from the Store");
                fourstore.DeleteGraph(g.BaseUri);
                Console.WriteLine("Graph deleted Ok");

            }
            catch (IOException ioEx)
            {
                reportError(output, "IO Exception", ioEx);
            }
            catch (RdfParseException parseEx)
            {
                reportError(output, "Parsing Exception", parseEx);
            }
            catch (RdfStorageException storeEx)
            {
                reportError(output, "Storage Exception", storeEx);
            }
            catch (RdfException rdfEx)
            {
                reportError(output, "RDF Exception", rdfEx);
            }
            catch (WebException webEx)
            {
                reportError(output, "HTTP Exception", webEx);
            }
            catch (Exception ex)
            {
                reportError(output, "Other Exception", ex);
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
