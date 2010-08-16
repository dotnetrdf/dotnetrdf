using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace dotNetRDFTest
{
    public class QuerySpeedTest
    {
        public static void Main(string[] args)
        {
            if (args.Length < 1) args = new string[] { "100" };
            int runs = Int32.Parse(args[0]);

            Console.WriteLine("## Query Speed Test");
            Console.WriteLine("Designed to test the effiency of using different data structures for TripleCollection");

            List<IGraph> graphs = new List<IGraph>();
            
            //Load the Test Graph
            TurtleParser ttlparser = new TurtleParser();
            IGraph g = new Graph();
            ttlparser.Load(g, "turtle_tests/test-14.ttl");
            graphs.Add(g);
            g = new NonIndexedGraph();
            ttlparser.Load(g, "turtle_tests/test-14.ttl");
            graphs.Add(g);

            Console.WriteLine("Test Graphs loaded");
            Console.WriteLine();

            UriNode lookupSubj = g.CreateUriNode(":a999");
            UriNode lookupPred = g.CreateUriNode(":a500");
            UriNode lookupObj = g.CreateUriNode(":doesNotExist");
            int count = 0;
            Stopwatch timer = new Stopwatch();

            foreach (IGraph graph in graphs)
            {
                Console.WriteLine("# Profiling Class '" + graph.GetType().ToString() + "'");

                //Test a Basic Iteration
                timer.Start();
                for (int i = 0; i < runs; i++)
                {
                    foreach (Triple t in graph.Triples)
                    {
                        count++;
                    }
                }
                timer.Stop();

                Console.WriteLine("Iteration took average time of " + timer.ElapsedTicks / runs + " ticks (" + timer.ElapsedMilliseconds / runs + " ms)");
                Console.WriteLine(count + " Triples iterated over");

                timer.Reset();
                count = 0;

                //Test a Lookup that can be Indexed
                timer.Start();
                for (int i = 0; i < runs; i++)
                {
                    IEnumerable<Triple> ts = graph.GetTriplesWithSubject(lookupSubj);
                    count += ts.Count();
                }
                timer.Stop();

                Console.WriteLine("Subject Lookup took average time of " + timer.ElapsedTicks / runs + " ticks (" + timer.ElapsedMilliseconds / runs + " ms)");
                Console.WriteLine(count + " Triples found");

                timer.Reset();
                count = 0;

                //Test a Lookup that can be Indexed
                timer.Start();
                for (int i = 0; i < runs; i++)
                {
                    IEnumerable<Triple> ts = graph.GetTriplesWithPredicate(lookupPred);
                    count += ts.Count();
                }
                timer.Stop();

                Console.WriteLine("Predicate Lookup took average time of " + timer.ElapsedTicks / runs + " ticks (" + timer.ElapsedMilliseconds / runs + " ms)");
                Console.WriteLine(count + " Triples found");

                timer.Reset();
                count = 0;

                //Test a Lookup that can be Indexed
                timer.Start();
                for (int i = 0; i < runs; i++)
                {
                    IEnumerable<Triple> ts = graph.GetTriplesWithObject(lookupObj);
                    count += ts.Count();
                }
                timer.Stop();

                Console.WriteLine("Object Lookup took average time of " + timer.ElapsedTicks / runs + " ticks (" + timer.ElapsedMilliseconds / runs + " ms)");
                Console.WriteLine(count + " Triples found");

                timer.Reset();
                count = 0;

                //Test a Lookup that can't be indexed
                timer.Start();
                for (int i = 0; i < runs; i++)
                {
                    IEnumerable<Triple> ts = graph.GetTriples(new PredicateIsSelector(lookupPred));
                    count += ts.Count();
                }
                timer.Stop();

                Console.WriteLine("ISelector Lookup took average time of " + timer.ElapsedTicks / runs + " ticks (" + timer.ElapsedMilliseconds / runs + " ms)");
                Console.WriteLine(count + " Triples found");

                timer.Reset();
                Console.WriteLine();
                count = 0;
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
