using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace dotNetRDFTest
{
    public class DiskIOSpeedTests
    {
        public const String TestSuiteBaseUri = "http://www.w3.org/2001/sw/DataAccess/df1/tests/";

        public static void reportError(StreamWriter output, String header, Exception ex)
        {
            output.WriteLine(header);
            output.WriteLine(ex.Message);
            output.WriteLine(ex.StackTrace);
        }

        public static void Main(string[] args)
        {
            //Read in Parameters
            if (args.Length < 3) args = new string[] { "both","30","true" };
            String mode = args[0].ToLower();
            if (mode != "read" && mode != "write" && mode != "both") mode = "both";
            int runs = Int32.Parse(args[1]);
            if (runs < 1) runs = 1;
            bool allowHiSpeed = Boolean.Parse(args[2]);

            StreamWriter output = new StreamWriter("DiskIOSpeedTests.txt");
            Console.SetOut(output);

            Stopwatch timer = new Stopwatch();

            if (mode.Equals("read") || mode.Equals("both"))
            {
                Console.WriteLine("## Read Speed Tests");
                Console.WriteLine("# Compares the Speed of reading the same Graph n times");

                try
                {
                    List<IRdfReader> readers = new List<IRdfReader>() { 

                        new NTriplesParser(),
                        new TurtleParser(),
                        new Notation3Parser(), 
                        new RdfXmlParser(RdfXmlParserMode.DOM),
                        new RdfXmlParser(RdfXmlParserMode.Streaming),
                        new RdfJsonParser()/*,
                        new RdfAParser()*/
                    };
                    List<String> files = new List<string>() { 
                        "test.nt", 
                        "test.ttl", 
                        "test.n3", 
                        "test.rdf",
                        "test.rdf",
                        "test.json",
                        "test.nt.json"/*,
                        "test.html"*/
                    };
                    IRdfReader reader;

                    for (int i = 0; i < readers.Count; i++)
                    {
                        reader = readers[i];

                        long totalTime = 0;
                        int totalTriples = 0;
                        int triples;
                        DateTime start, finish;
                        double diff;
                        Console.WriteLine("# Profiling Reader '" + reader.GetType().ToString() + "' - " + reader.ToString());
                        Debug.WriteLine("# Profiling Reader '" + reader.GetType().ToString() + "' - " + reader.ToString());
                        Console.WriteLine("Test File is " + files[i]);
                        Console.WriteLine();

                        for (int j = 1; j <= runs; j++)
                        {
                            Console.WriteLine("Run #" + j);
                            Debug.WriteLine("Run #" + j);

                            timer.Reset();

                            //Start Time
                            start = DateTime.Now;
                            Console.WriteLine("Start @ " + start.ToString(TestSuite.TestSuiteTimeFormat));
                            Debug.WriteLine("Start @ " + start.ToString(TestSuite.TestSuiteTimeFormat));

                            //Load
                            try
                            {
                                Graph g = new Graph();
                                timer.Start();
                                reader.Load(g, "diskio_tests/" + files[i]);
                                timer.Stop();
                                triples = g.Triples.Count;
                            }
                            catch (Exception ex)
                            {
                                triples = 0;
                                reportError(output, "Exception while Reading", ex);
                            }

                            //End Time
                            finish = DateTime.Now;
                            Console.WriteLine("End @ " + finish.ToString(TestSuite.TestSuiteTimeFormat));
                            Debug.WriteLine("End @ " + finish.ToString(TestSuite.TestSuiteTimeFormat));

                            //Compute Load Rate
                            diff = (double)timer.ElapsedMilliseconds;
                            Console.WriteLine("Read Time was " + diff / 1000d + " seconds");
                            Console.WriteLine("Read Rate was " + (triples / diff) * 1000 + " triples/second");
                            Debug.WriteLine("Read Rate was " + (triples / diff) * 1000 + " triples/second");

                            //Running Totals
                            totalTriples += triples;
                            totalTime += timer.ElapsedMilliseconds;

                            Console.WriteLine();
                        }

                        //Compute Averages
                        double dtime = (double)totalTime;
                        Console.WriteLine("Average Read Time was " + totalTime / 1000 / runs + " seconds");
                        Console.WriteLine("Average Read Rate was " + totalTriples / (dtime / 1000d) + " triples/second");
                        Debug.WriteLine("Average Read Rate was " + totalTriples / (dtime / 1000d) + " triples/second");
                        Console.WriteLine();
                    }
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

                Console.WriteLine();
            }

            if (mode.Equals("write") || mode.Equals("both"))
            {
                Console.WriteLine("## Write Speed Tests");
                Console.WriteLine("# Compares the Speed of serializing the same Graph n times");

                try
                {
                    //Load up the Test Graph
                    Console.WriteLine();
                    Graph g = new Graph();
                    g.BaseUri = new Uri(TestSuiteBaseUri);
                    TurtleParser ttlparser = new TurtleParser();
                    ttlparser.Load(g, "diskio_tests/test.ttl");
                    Console.WriteLine("# Test Graph Loaded");
                    int triples = g.Triples.Count;
                    Console.WriteLine("# Test Graph contains " + triples + " Triples");
                    Console.WriteLine();
                    Debug.WriteLine("Test Graph Loaded");

                    List<IRdfWriter> writers = new List<IRdfWriter>()
                    {
                        new RdfXmlWriter(),
                        new FastRdfXmlWriter(),
                        new PrettyRdfXmlWriter(),
                        new NTriplesWriter(),
                        new TurtleWriter(),
                        new CompressingTurtleWriter(),
                        new Notation3Writer(),
                        new RdfJsonWriter(),
                        new HtmlWriter()
                    };

                    foreach (IRdfWriter writer in writers)
                    {
                        long totalTime = 0;
                        int totalTriples = 0;
                        DateTime start, finish;
                        long diff;
                        Console.WriteLine("# Profiling Writer '" + writer.GetType().ToString() + "' - " + writer.ToString());
                        Debug.WriteLine("# Profiling Writer '" + writer.GetType().ToString() + "' - " + writer.ToString());
                        if (writer is ICompressingWriter)
                        {
                            Console.WriteLine("Compression Level is " + ((ICompressingWriter)writer).CompressionLevel);
                        }
                        Console.WriteLine();

                        if (writer is IHighSpeedWriter)
                        {
                            ((IHighSpeedWriter)writer).HighSpeedModePermitted = allowHiSpeed;
                            if (allowHiSpeed)
                            {
                                Console.WriteLine("Using High Speed Mode");
                            }
                        }

                        //Disable Pretty Printing
                        if (writer is IPrettyPrintingWriter)
                        {
                            ((IPrettyPrintingWriter)writer).PrettyPrintMode = false;
                        }

                        for (int i = 1; i <= runs; i++)
                        {
                            Console.WriteLine("Run #" + i);
                            Debug.WriteLine("Run #" + i);

                            //Start Time
                            start = DateTime.Now;
                            Console.WriteLine("Start @ " + start.ToString(TestSuite.TestSuiteTimeFormat));
                            Debug.WriteLine("Start @ " + start.ToString(TestSuite.TestSuiteTimeFormat));

                            //Load
                            try
                            {
                                writer.Save(g, "diskio_tests/" + writer.GetType().ToString() + ".out");
                            }
                            catch (Exception ex)
                            {
                                reportError(output, "Exception while Writing", ex);
                            }

                            //End Time
                            finish = DateTime.Now;
                            Console.WriteLine("End @ " + finish.ToString(TestSuite.TestSuiteTimeFormat));
                            Debug.WriteLine("End @ " + finish.ToString(TestSuite.TestSuiteTimeFormat));

                            //Compute Load Rate
                            diff = Microsoft.VisualBasic.DateAndTime.DateDiff(Microsoft.VisualBasic.DateInterval.Second, start, finish, Microsoft.VisualBasic.FirstDayOfWeek.Monday, Microsoft.VisualBasic.FirstWeekOfYear.System);
                            if (diff == 0)
                            {
                                diff = finish.Millisecond - start.Millisecond;
                                if (diff < 0) diff += 1000;
                                double smalldiff = (double)diff;
                                Console.WriteLine("Write Time was " + smalldiff / 1000d + " seconds");
                                Console.WriteLine("Write Rate was " + (triples / smalldiff) * 1000 + " triples/second");
                                Debug.WriteLine("Write Rate was " + (triples / smalldiff) * 1000 + " triples/second");
                            }
                            else
                            {
                                Console.WriteLine("Write Time was " + diff + " seconds");
                                Console.WriteLine("Write Rate was " + triples / diff + " triples/second");
                                Debug.WriteLine("Write Rate was " + triples / diff + " triples/second");
                                diff = diff * 1000;
                            }

                            //Running Totals
                            totalTriples += triples;
                            totalTime += diff;

                            Console.WriteLine();
                        }

                        //Compute Averages
                        double dtime = (double)totalTime;
                        Console.WriteLine("Average Write Time was " + totalTime / 1000 / runs + " seconds");
                        Console.WriteLine("Average Write Rate was " + totalTriples / (dtime / 1000d) + " triples/second");
                        Debug.WriteLine("Average Write Rate was " + totalTriples / (dtime / 1000d) + " triples/second");
                        Console.WriteLine();
                    }
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
            }

            output.Close();
        }
    }
}
