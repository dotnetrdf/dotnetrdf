using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace dotNetRDFTest
{
    public class TripleStoreTest
    {
        public static string[] TestIDs = { "11471", "1650", "11475", "60", "46", "21" };
        public const string TestBaseURI = "http://rdf.ecs.soton.ac.uk/person/";
        public const string TestBaseOntology = "http://rdf.ecs.soton.ac.uk/ontology/ecs#";

        public static void Main(string[] args)
        {
            StreamWriter output = new StreamWriter("TripleStoreTest.txt");
            try
            {
                //Set Output
                Console.SetOut(output);

                Console.WriteLine("## Triple Store Test Suite");

                //Get a Triple Store
                IInMemoryQueryableStore store = new SqlTripleStore("dotnetrdf_experimental","sa","20sQl08");

                //Load it with Data
                foreach (String id in TestIDs) {
                    Console.WriteLine("# Attempting to load Graph from URI '" + TestBaseURI + id + "'");
                    try
                    {
                        store.AddFromUri(new Uri(TestBaseURI + id),true);
                        Console.WriteLine("Loaded OK");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Load Failed");
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                        if (ex.InnerException != null)
                        {
                            Console.WriteLine(ex.InnerException.Message);
                            Console.WriteLine(ex.InnerException.StackTrace);
                        }
                    }
                }

                Console.WriteLine("# Triple Store Loading Done");
                Console.WriteLine();

                //Show the Triples
                Console.WriteLine("# Following Triples are in the Store");
                foreach (Triple t in store.Triples)
                {
                    Console.WriteLine(t.ToString(true) + " from Graph " + t.GraphUri.ToString());
                }
                
                Console.WriteLine();

                //Do some selection
                //Need a Blank Graph to do the Node Creation
                Graph g = new Graph();
                g.NamespaceMap.AddNamespace("ecs", new Uri(TestBaseOntology));

                PredicateIsSelector selInterests = new PredicateIsSelector(g.CreateUriNode("ecs:hasInterest"));
                Console.WriteLine("# All Triples about Interests");
                foreach (Triple t in store.GetTriples(selInterests))
                {
                    Console.WriteLine(t.ToString(true) + " from Graph " + t.GraphUri.ToString());
                }
                Console.WriteLine();
                Console.WriteLine("# All Triples about Interests from the last two Graphs only");
                foreach (Triple t in store.GetTriples(store.Graphs.GraphUris.Reverse().Take(2).ToList(),selInterests)) {
                    Console.WriteLine(t.ToString(true) + " from Graph " + t.GraphUri.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            output.Close();
        }
    }
}
