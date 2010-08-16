using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Inference;

namespace dotNetRDFTest
{
    public class InferenceTest
    {

        public static void Main(string[] args)
        {
            StreamWriter output = new StreamWriter("InferenceTest.txt");
            try
            {
                //Set Output
                Console.SetOut(output);

                Console.WriteLine("## Inference Test Suite");

                //Load the Test RDF
                TurtleParser ttlparser = new TurtleParser();
                Graph g = new Graph();
                //ttlparser.TraceParsing = true;
                ttlparser.Load(g, "InferenceTest.ttl");

                Console.WriteLine("Inference Test Data Loaded OK");
                Console.WriteLine();

                Console.WriteLine("Graph contains the following Triples:");
                foreach (Triple t in g.Triples)
                {
                    Console.WriteLine(t.ToString());
                }
                Console.WriteLine();

                //Select everything that is exactly a Car
                Console.WriteLine("# Selecting things whose class is exactly Car");
                ExactClassSelector carsel = new ExactClassSelector(g, g.CreateUriNode("eg:Car"));
                Test(g, carsel);
                Console.WriteLine();

                //Select everything that is any sort of Car
                Console.WriteLine("# Selecting things whose class is Car or a subclass thereof");
                ClassSelector allcarsel = new ClassSelector(g, g.CreateUriNode("eg:Car"));
                Test(g, allcarsel);
                Console.WriteLine();

                //Select everything that is a SportsCar or superclass thereof
                Console.WriteLine("# Selecting things whose class is SportsCar or superclass thereof");
                Console.WriteLine("# Thus things which are Cars are selected but not PeopleCarriers since they are a sibling class and not a superclass");
                WideningClassSelector wcarsel = new WideningClassSelector(g, g.CreateUriNode("eg:SportsCar"));
                Test(g, wcarsel);
                Console.WriteLine();

                //Select everything that is exactly a Jumbo Jet
                Console.WriteLine("# Selecting things whose class is exactly JumboJet");
                ExactClassSelector jjsel = new ExactClassSelector(g, g.CreateUriNode("eg:JumboJet"));
                Test(g, jjsel);
                Console.WriteLine();

                //Select everything that is an Air Vehicle
                Console.WriteLine("# Selecting things which are Air Vehicles");
                ClassSelector avsel = new ClassSelector(g, g.CreateUriNode("eg:AirVehicle"));
                Test(g, avsel);
                Console.WriteLine();

                //Select everything that has a defined Speed
                Console.WriteLine("# Selecting things which have exactly Speed defined");
                HasExactPropertySelector spsel = new HasExactPropertySelector(g.CreateUriNode("eg:Speed"));
                Test(g, spsel);
                Console.WriteLine();

                //Select things with a limited Speed
                Console.WriteLine("# Selecting things which have a Limited Speed defined");
                HasExactPropertySelector lspsel = new HasExactPropertySelector(g.CreateUriNode("eg:LimitedSpeed"));
                Test(g, lspsel);
                Console.WriteLine();

                //Select things with any kinds of Speed
                Console.WriteLine("# Selecting things which have any kinds of Speed defined");
                HasPropertySelector allspsel = new HasPropertySelector(g, g.CreateUriNode("eg:Speed"));
                Test(g, allspsel);
                Console.WriteLine();

                //Now stick the Graph in a Triple Store with a Reasoner attached
                Console.WriteLine("# Using a Triple Store with an RDFS Reasoner attached");
                TripleStore store = new TripleStore();
                store.AddInferenceEngine(new RdfsReasoner());
                g.BaseUri = new Uri("http://example.org/Inference");
                //Add a couple of additional Triples, their types must be inferred
                g.Assert(new Triple(g.CreateUriNode("eg:AudiA8"), g.CreateUriNode("eg:Speed"), (180).ToLiteral(g)));
                g.Assert(new Triple(g.CreateUriNode("eg:SpaceShuttle"), g.CreateUriNode("eg:AirSpeed"), (17500).ToLiteral(g)));
                //Add the Graph to the store, this is when inference occurs
                store.Add(g);
                Console.WriteLine("Triple Store contains the following Triples");
                foreach (Triple t in store.Triples)
                {
                    Console.WriteLine(t.ToString());
                }
                Console.WriteLine();

                //Select things which are a Car
                Console.WriteLine("# Selecting things which are a Car (or subclass thereof)");
                HasPropertyValueSelector carsel2 = new HasPropertyValueSelector(g.CreateUriNode("rdf:type"), g.CreateUriNode("eg:Car"));
                Test(store, carsel2);
                Console.WriteLine();

                //Select things which are Air Vehicles
                Console.WriteLine("# Selecting things which are Air Vehicles");
                HasPropertyValueSelector avsel2 = new HasPropertyValueSelector(g.CreateUriNode("rdf:type"), g.CreateUriNode("eg:AirVehicle"));
                Test(store, avsel2);
                Console.WriteLine();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            output.Close();
        }

        private static void Test(IGraph g, ISelector<Triple> testsel)
        {
            foreach (Triple t in g.GetTriples(testsel))
            {
                Console.WriteLine(t.ToString());
            }
        }

        private static void Test(IInMemoryQueryableStore store, ISelector<Triple> testsel)
        {
            foreach (Triple t in store.GetTriples(testsel))
            {
                Console.WriteLine(t.ToString());
            }
        }
    }
}
