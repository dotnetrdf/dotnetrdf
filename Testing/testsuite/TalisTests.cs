using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing;
using VDS.RDF.Storage;

namespace dotNetRDFTest
{
    public class TalisTests
    {
        private const String TestUri = "http://example.org/vehicles/FordFiesta";

        public static void Main(String[] args)
        {
            StreamWriter output = new StreamWriter("TalisTests.txt");
            try
            {
                Console.SetOut(output);
                Console.WriteLine("## Talis Platform Tests");
                Console.WriteLine();

                //Read in our Test Graph
                Graph g = new Graph();
                TurtleParser ttlparser = new TurtleParser();
                ttlparser.Load(g, "TalisTest.ttl");
                Console.WriteLine("Loaded Test Graph OK");
                Console.WriteLine();

                //Get the Talis Store
                TalisPlatformConnector talis = new TalisPlatformConnector("rvesse-dev1", "rvesse", "4kn478wj");

                //Attempt to add a Graph
                Console.WriteLine("# Attempting to add this Graph to the Talis Store");
                talis.Add(g);
                Console.WriteLine("Added the Graph OK");
                Console.WriteLine();

                //Attempt to get this some data from this Graph back again
                Console.WriteLine("# Attempting to retrieve some data from this Graph from the Talis Store");

                Graph h = new Graph();
                talis.Describe(h, TestUri);

                Console.WriteLine("Retrieved OK");
                Console.WriteLine();

                foreach (Triple t in h.Triples)
                {
                    Console.WriteLine(t.ToString());
                }
                Console.WriteLine();

                //Make a Sparql Query for the same stuff
                Console.WriteLine("# Attempting to retrieve the same data from this Graph from the Talis Store using SPARQL");

                Object results = talis.Query("SELECT * {?s ?p ?o . FILTER(?s = <" + TestUri + ">)}");
                Console.WriteLine("Retrieved OK");
                Console.WriteLine();

                //Output the Result
                if (results is SparqlResultSet)
                {
                    foreach (SparqlResult result in (SparqlResultSet)results)
                    {
                        Console.WriteLine(result.ToString());
                    }
                }
                else if (results is Graph)
                {
                    foreach (Triple t in ((Graph)results).Triples)
                    {
                        Console.WriteLine(t.ToString());
                    }
                }
                else
                {
                    Console.WriteLine("Unexpected Results Object '" + results.GetType().ToString() + "'");
                }
                Console.WriteLine();

                //Use a TalisGraph object to do the same thing
                Console.WriteLine("# Attempting to retrieve the same data again using a TalisGraph object");
                TalisGraph i = new TalisGraph(TestUri, talis);
                Console.WriteLine("Retrieved OK");
                Console.WriteLine();

                foreach (Triple t in i.Triples)
                {
                    Console.WriteLine(t.ToString());
                }
                Console.WriteLine();

                //Use the same object to Update the Store
                Console.WriteLine("# Attempting to Update the Talis Store using the TalisGraph object");
                if (!i.NamespaceMap.HasNamespace("eg")) {
                    i.NamespaceMap.AddNamespace("eg", new Uri("http://example.org/vehicles/"));
                }
                IUriNode spaceVehicle = i.CreateUriNode("eg:SpaceVehicle");
                IUriNode subClass = i.CreateUriNode("rdfs:subClassOf");
                IUriNode vehicle = i.CreateUriNode("eg:Vehicle");
                i.Assert(new Triple(spaceVehicle, subClass, vehicle));
                Console.WriteLine("Updated OK");
                Console.WriteLine();

                //Retrieve that Graph again
                Console.WriteLine("# Retrieving the same data again to check that the Update was persisted OK");
                TalisGraph j = new TalisGraph(new Uri("http://example.org/vehicles/SpaceVehicle"), talis);
                Console.WriteLine("Retrieved OK");
                Console.WriteLine();

                foreach (Triple t in j.Triples)
                {
                    Console.WriteLine(t.ToString());
                }
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
            finally
            {
                output.Close();
            }
        }

        private static void HandleError(Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);

            if (ex.InnerException != null)
            {
                Console.WriteLine();
                Console.WriteLine(ex.InnerException.Message);
                Console.WriteLine(ex.InnerException.StackTrace);
            }
        }
    }
}
