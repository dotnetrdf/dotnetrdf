using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Params;

namespace dotNetRDFTest
{
    public class SqlStoreTest
    {
        public static void reportError(StreamWriter output, String header, Exception ex)
        {
            output.WriteLine(header);
            output.WriteLine(ex.Message);
            output.WriteLine(ex.StackTrace);
        }

        public static void Main(string[] args)
        {
            StreamWriter output = new StreamWriter("SQLStoreTest.txt");
            Console.SetOut(output);
            Console.WriteLine("## SQL Store Test");

            //Set default parameters if insufficient supplied
            if (args.Length < 5)
            {
                args = new string[] { "read", "10", "false", "false", "8" };
            }

            try
            {
                //Read in the Parameters
                String testMode = args[0].ToLower();
                if (testMode != "read" && testMode != "write")
                {
                    testMode = "read";
                }
                int runs = Int32.Parse(args[1]);
                if (runs < 1) runs = 1;
                bool reuseManager = Boolean.Parse(args[2]);
                bool useThreadedManager = Boolean.Parse(args[3]);
                int threads = Int32.Parse(args[4]);
                if (threads < 1) threads = 4;

                #region Basic Tests

                //Do some basic operations
                Console.WriteLine("# Basic Read and Write of normal Graphs");

                //Read in a Test Graph from a Turtle File
                Graph g = new Graph();
                g.BaseUri = new Uri("http://www.dotnetrdf.org/Tests/SQLStore/");
                TurtleParser ttlparser = new TurtleParser();
                ttlparser.Load(g, "InferenceTest.ttl");

                Console.WriteLine("Loaded the InferenceTest.ttl file as the Test Graph");
                Console.WriteLine("Attempting to save into the SQL Store");

                //Save to a Store using SqlWriter
                SqlWriter sqlwriter = new SqlWriter("dotnetrdf_experimental", "sa", "20sQl08");
                sqlwriter.Save(g, false);

                Console.WriteLine("Saved to the SQL Store");

                //Read back from the Store using SqlReader
                IGraph h = new Graph();
                Console.WriteLine("Trying to read the Graph back from the SQL Store");
                SqlReader sqlreader = new SqlReader("dotnetrdf_experimental", "sa", "20sQl08");
                h = sqlreader.Load("http://www.dotnetrdf.org/Tests/SQLStore/");

                Console.WriteLine("Read from SQL Store OK");

                foreach (String prefix in h.NamespaceMap.Prefixes)
                {
                    Console.WriteLine(prefix + ": <" + h.NamespaceMap.GetNamespaceUri(prefix).ToString() + ">");
                }
                Console.WriteLine();
                foreach (Triple t in h.Triples)
                {
                    Console.WriteLine(t.ToString());
                }

                Console.WriteLine("# Test Passed");
                Console.WriteLine();

                //Demonstrate that the SqlGraph persists stuff to the Store
                Console.WriteLine("# Advanced Read and Write with a SQLGraph");

                SqlGraph s = new SqlGraph(new Uri("http://www.dotnetrdf.org/Tests/SQLStore"), "dotnetrdf_experimental", "sa", "20sQl08");
                Console.WriteLine("Opened the SQL Graph OK");

                INode type = s.CreateUriNode("rdf:type");

                s.Assert(new Triple(type, type, type));

                Console.WriteLine("Asserted something");

                s.NamespaceMap.AddNamespace("ex", new Uri("http://www.example.org/"));
                Console.WriteLine("Added a Namespace");

                s.NamespaceMap.AddNamespace("ex", new Uri("http://www.example.org/changedNamespace/"));
                Console.WriteLine("Changed a Namespace");

                Console.WriteLine("Reopening to see if stuff gets loaded correctly");
                s = new SqlGraph(new Uri("http://www.dotnetrdf.org/Tests/SQLStore"), "dotnetrdf_experimental", "sa", "20sQl08");

                foreach (String prefix in s.NamespaceMap.Prefixes)
                {
                    Console.WriteLine(prefix + ": <" + s.NamespaceMap.GetNamespaceUri(prefix).ToString() + ">");
                }
                Console.WriteLine();
                foreach (Triple t in s.Triples)
                {
                    Console.WriteLine(t.ToString());
                }
                Console.WriteLine();

                s.Retract(new Triple(type, type, type));
                Console.WriteLine("Retracted something");

                foreach (Triple t in s.Triples)
                {
                    Console.WriteLine(t.ToString());
                }

                #endregion

                #region Benchmarking Tests

                Console.WriteLine();
                Console.WriteLine("# Performance Benchmarking for a Large TripleStore");
                Console.WriteLine("Performing " + runs + " Runs to gauge average performance");

                int totalTriples = 0;
                long totalTime = 0;
                long diff;
                DateTime start, finish;
                int triples;

                if (testMode.Equals("read"))
                {
                    //Read Benchmark
                    Console.WriteLine("Read Benchmarking");

                    if (reuseManager)
                    {
                        Console.WriteLine("Reusing ISQLIOManager which should improve performance");
                    }
                    if (useThreadedManager)
                    {
                        Console.WriteLine("Using IThreadedSQLIOManager which should improve perfomance");
                    }

                    //Create the Manager
                    IThreadedSqlIOManager manager;
                    manager = new MicrosoftSqlStoreManager("localhost", "bbcone", "sa", "20sQl08");
                    if (reuseManager)
                    {
                        manager.PreserveState = true;
                    }

                    //Perform the Runs
                    for (int i = 1; i <= runs; i++)
                    {
                        Console.WriteLine("Run #" + i);
                        Debug.WriteLine("Run #" + i);

                        //Start Profiling
                        start = DateTime.Now;
                        Console.WriteLine("Starting Loading @ " + start.ToString(TestSuite.TestSuiteTimeFormat));
                        Debug.WriteLine("Start @ " + start.ToString(TestSuite.TestSuiteTimeFormat));

                        //Set-up the Manager as required by the Test Options
                        ITripleStore bigstore;
                        if (reuseManager)
                        {
                            if (useThreadedManager)
                            {
                                bigstore = new ThreadedSqlTripleStore((IThreadedSqlIOManager)manager, threads);
                            }
                            else
                            {
                                bigstore = new SqlTripleStore(manager);
                            }
                        }
                        else
                        {
                            if (useThreadedManager)
                            {
                                bigstore = new ThreadedSqlTripleStore(manager, threads);
                            }
                            else
                            {
                                bigstore = new SqlTripleStore(manager);
                            }
                        }

                        //End Profiling
                        finish = DateTime.Now;
                        Console.WriteLine("Finished Loading @ " + finish.ToString(TestSuite.TestSuiteTimeFormat));
                        Debug.WriteLine("Finish @ " + finish.ToString(TestSuite.TestSuiteTimeFormat));

                        //Increment Totals for final average calculations later
                        triples = bigstore.Triples.Count();
                        totalTriples += triples;
                        Console.WriteLine(triples + " Triples loaded");

                        //Compute Load Rate
                        diff = Microsoft.VisualBasic.DateAndTime.DateDiff(Microsoft.VisualBasic.DateInterval.Second, start, finish, Microsoft.VisualBasic.FirstDayOfWeek.Monday, Microsoft.VisualBasic.FirstWeekOfYear.System);
                        totalTime += diff;
                        Console.WriteLine("Load took " + diff + " seconds");
                        Console.WriteLine("Load Rate was " + triples / diff + " Triples/Second");
                        Debug.WriteLine("Load Rate was " + triples / diff + " Triples/Second");
                        Console.WriteLine();

                        bigstore.Dispose();
                    }
                }
                else
                {
                    //Write Benchmark
                    Console.WriteLine("Write Benchmarking");

                    //Load in the Source Data from our Test Store
                    ITripleStore origstore;
                    IThreadedSqlIOManager readManager = new MicrosoftSqlStoreManager("localhost", "dotnetrdf_experimental", "sa", "20sQl08");
                    Console.WriteLine();
                    Console.WriteLine("Obtaining Test Data");

                    start = DateTime.Now;
                    Console.WriteLine("Starting Loading @ " + start.ToString(TestSuite.TestSuiteTimeFormat));
                    Debug.WriteLine("Start @ " + start.ToString(TestSuite.TestSuiteTimeFormat));

                    //Do the Load
                    origstore = new ThreadedSqlTripleStore(readManager,8);
                    finish = DateTime.Now;
                    triples = origstore.Triples.Count();

                    Console.WriteLine("Finished Loading @ " + finish.ToString(TestSuite.TestSuiteTimeFormat));
                    Debug.WriteLine("Finish @ " + finish.ToString(TestSuite.TestSuiteTimeFormat));

                    //Compute Load Rate
                    diff = Microsoft.VisualBasic.DateAndTime.DateDiff(Microsoft.VisualBasic.DateInterval.Second, start, finish, Microsoft.VisualBasic.FirstDayOfWeek.Monday, Microsoft.VisualBasic.FirstWeekOfYear.System);
                    Console.WriteLine("Load took " + diff + " seconds");
                    if (diff > 0)
                    {
                        Console.WriteLine("Load Rate was " + triples / diff + " Triples/Second");
                        Debug.WriteLine("Load Rate was " + triples / diff + " Triples/Second");
                    }
                    Console.WriteLine();

                    IThreadedSqlIOManager writeManager;

                    for (int i = 1; i <= runs; i++) 
                    {
                        Console.WriteLine("Run #" + i);
                        Debug.WriteLine("Run #" + i);

                        //Create a New Manager for every write
                        writeManager = new MicrosoftSqlStoreManager("localhost", "write_test", "sa", "20sQl08");

                        //Create SqlWriter
                        ThreadedSqlStoreWriter writer = new ThreadedSqlStoreWriter();

                        //Start Profiling
                        start = DateTime.Now;
                        Console.WriteLine("Starting Loading @ " + start.ToString(TestSuite.TestSuiteTimeFormat));
                        Debug.WriteLine("Start @ " + start.ToString(TestSuite.TestSuiteTimeFormat));

                        //Write the Store
                        writer.Save(origstore, new ThreadedSqlIOParams(writeManager,threads));
                        
                        //End Profiling
                        finish = DateTime.Now;
                        Console.WriteLine("Finished Loading @ " + finish.ToString(TestSuite.TestSuiteTimeFormat));
                        Debug.WriteLine("Finish @ " + finish.ToString(TestSuite.TestSuiteTimeFormat));

                        //Compute Load Rate
                        diff = Microsoft.VisualBasic.DateAndTime.DateDiff(Microsoft.VisualBasic.DateInterval.Second, start, finish, Microsoft.VisualBasic.FirstDayOfWeek.Monday, Microsoft.VisualBasic.FirstWeekOfYear.System);
                        totalTime += diff;
                        Console.WriteLine("Writing took " + diff + " seconds");
                        if (diff > 0)
                        {
                            Console.WriteLine("Write Rate was " + triples / diff + " Triples/Second");
                            Debug.WriteLine("Write Rate was " + triples / diff + " Triples/Second");
                        }
                        Console.WriteLine();

                        //Now need to clear the Database for the next Test
                        writeManager.Open(true);
                        writeManager.ExecuteNonQuery("DELETE FROM NODES");
                        writeManager.ExecuteNonQuery("DELETE FROM TRIPLES");
                        writeManager.ExecuteNonQuery("DELETE FROM GRAPH_TRIPLES");
                        writeManager.ExecuteNonQuery("DELETE FROM GRAPHS");
                        writeManager.ExecuteNonQuery("DELETE FROM NAMESPACES");
                        writeManager.ExecuteNonQuery("DELETE FROM NS_PREFIXES");
                        writeManager.ExecuteNonQuery("DELETE FROM NS_URIS");
                        writeManager.Close(true);
                    }
                }

                //Final Average Calculations
                Console.WriteLine();
                Console.WriteLine("Average Load Time was " + totalTime / runs + " seconds");
                Console.WriteLine("Average Load Rate was " + totalTriples / totalTime + " Triples/Second");

                #endregion

                Console.WriteLine("# Tests Passed");
                

            }
            catch (System.Data.SqlClient.SqlException sqlEx)
            {
                reportError(output, "SQL Exception", sqlEx);
            }
            catch (IOException ioEx)
            {
                reportError(output, "IO Exception", ioEx);
            }
            catch (RdfParseException parseEx)
            {
                reportError(output, "Parsing Exception", parseEx);
            }
            catch (RdfException rdfEx)
            {
                reportError(output, "RDF Exception", rdfEx);
            }
            catch (Exception ex)
            {
                reportError(output, "Other Exception", ex);
            }

            output.Close();
        }
    }
}
