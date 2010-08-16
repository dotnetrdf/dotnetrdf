using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing;

namespace dotNetRDFTest
{
    class RdfATestSuite
    {
        public static void reportError(StreamWriter output, String header, Exception ex)
        {
            output.WriteLine(header);
            output.WriteLine(ex.Message);
            output.WriteLine(ex.StackTrace);
        }

        public static void Main(string[] args)
        {
            StreamWriter output = new StreamWriter("RdfATestSuite.txt");
            String[] wantTrace = {  };
            String[] wantOutput = {  };
            bool outputAll = true;
            bool traceAll = false;
            String[] skipTests = { 
                                   "0002.xhtml",
                                   "0003.xhtml", 
                                   "0004.xhtml",
                                   "0005.xhtml",
                                   "0016.xhtml",
                                   "0022.xhtml",
                                   "0024.xhtml",
                                   "0028.xhtml",
                                   "0043.xhtml",
                                   "0044.xhtml",
                                   "0045.xhtml",
                                   "0095.xhtml",
                                   "0096.xhtml",
                                   "0097.xhtml",
                                   "0098.xhtml",
                                   "0122.xhtml",
                                   "0123.xhtml",
                                   "0124.xhtml",
                                   "0125.xhtml",
                                   "0126.xhtml"
                                 };

            String[] skipCheck = {
                                     "0011.xhtml",
                                     "0092.xhtml",
                                     "0094.xhtml",
                                     "0100.xhtml",
                                     "0101.xhtml",
                                     "0102.xhtml",
                                     "0103.xhtml",
                                 };

            String[] falseTests = {
                                    "0042.xhtml",
                                    "0086.xhtml",
                                    "0095.xhtml",
                                    "0096.xhtml",
                                    "0097.xhtml",
                                    "0107.xhtml",
                                    "0116.xhtml",
                                    "0122.xhtml",
                                    "0125.xhtml"
                                  };

            Console.SetOut(output);

            try
            {
                int testsPassed = 0;
                int testsFailed = 0;
                String[] files = Directory.GetFiles("rdfa_tests");
                RdfAParser parser = new RdfAParser(RdfASyntax.AutoDetectLegacy);
                parser.Warning += new RdfReaderWarning(parser_Warning);
                SparqlQueryParser queryparser = new SparqlQueryParser();
                bool passed, passDesired;
                Graph g = new Graph();
                Stopwatch timer = new Stopwatch();
                long totalTime = 0;
                long totalTriples = 0;

                foreach (String file in files)
                {
                    timer.Reset();

                    if (skipTests.Contains(Path.GetFileName(file)))
                    {
                        output.WriteLine("## Skipping Test of File " + Path.GetFileName(file));
                        output.WriteLine();
                        continue;
                    }

                    if (Path.GetExtension(file) != ".html" && Path.GetExtension(file) != ".xhtml")
                    {
                        continue;
                    }

                    Debug.WriteLine("Testing File " + Path.GetFileName(file));
                    output.WriteLine("## Testing File " + Path.GetFileName(file));
                    output.WriteLine("# Test Started at " + DateTime.Now.ToString(TestSuite.TestSuiteTimeFormat));

                    passed = false;
                    passDesired = true;

                    try
                    {
                        g = new Graph();
                        g.BaseUri = new Uri("http://www.w3.org/2006/07/SWD/RDFa/testsuite/xhtml1-testcases/" + Path.GetFileName(file));
                        if (Path.GetFileNameWithoutExtension(file).StartsWith("bad"))
                        {
                            passDesired = false;
                            output.WriteLine("# Desired Result = Parsing Failed");
                        }
                        else
                        {
                            output.WriteLine("# Desired Result = Parses OK");
                        }

                        //if (traceAll || wantTrace.Contains(Path.GetFileName(file)))
                        //{
                        //    parser.TraceTokeniser = true;
                        //}
                        //else
                        //{
                        //    parser.TraceTokeniser = false;
                        //}

                        timer.Start();
                        parser.Load(g, file);
                        timer.Stop();

                        Console.WriteLine("Parsing took " + timer.ElapsedMilliseconds + "ms");

                        passed = true;
                        output.WriteLine("Parsed OK");

                        if (outputAll || wantOutput.Contains(Path.GetFileName(file)))
                        {
                            NTriplesWriter writer = new NTriplesWriter();
                            writer.Save(g, "rdfa_tests\\" + Path.GetFileNameWithoutExtension(file) + ".out");
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
                    finally
                    {
                        timer.Stop();

                        //Write the Triples to the Output
                        foreach (Triple t in g.Triples)
                        {
                            Console.WriteLine(t.ToString());
                        }

                        //Now we run the Test SPARQL (if present)
                        if (File.Exists("rdfa_tests/" + Path.GetFileNameWithoutExtension(file) + ".sparql"))
                        {
                            if (skipCheck.Contains(Path.GetFileName(file)))
                            {
                                output.WriteLine("## Skipping Check of File " + Path.GetFileName(file));
                                output.WriteLine();
                            }
                            else
                            {
                                try
                                {
                                    SparqlQuery q = queryparser.ParseFromFile("rdfa_tests/" + Path.GetFileNameWithoutExtension(file) + ".sparql");
                                    Object results = g.ExecuteQuery(q);
                                    if (results is SparqlResultSet)
                                    {
                                        //The Result is the result of the ASK Query
                                        if (falseTests.Contains(Path.GetFileName(file)))
                                        {
                                            passed = !((SparqlResultSet)results).Result;
                                        }
                                        else
                                        {
                                            passed = ((SparqlResultSet)results).Result;
                                        }
                                    }
                                }
                                catch
                                {
                                    passed = false;
                                }
                            }
                        }

                        if (passed && passDesired)
                        {
                            //Passed and we wanted to Pass
                            testsPassed++;
                            output.WriteLine("# Result = Test Passed");
                            totalTime += timer.ElapsedMilliseconds;
                            totalTriples += g.Triples.Count;
                        }
                        else if (!passed && passDesired)
                        {
                            //Failed when we should have Passed
                            testsFailed++;
                            output.WriteLine("# Result = Test Failed");
                        }
                        else if (passed && !passDesired)
                        {
                            //Passed when we should have Failed
                            testsFailed++;
                            output.WriteLine("# Result = Test Failed");
                        }
                        else
                        {
                            //Failed and we wanted to Fail
                            testsPassed++;
                            output.WriteLine("# Result = Test Passed");
                        }

                        output.WriteLine("# Triples Generated = " + g.Triples.Count());
                        output.WriteLine("# Test Ended at " + DateTime.Now.ToString(TestSuite.TestSuiteTimeFormat));
                    }

                    output.WriteLine();
                }

                output.WriteLine(testsPassed + " Tests Passed");
                output.WriteLine(testsFailed + " Tests Failed");
                output.WriteLine();
                output.Write("Total Parsing Time was " + totalTime + " ms");
                if (totalTime > 1000)
                {
                    output.WriteLine(" (" + totalTime / 1000d + " seconds)");
                }
                output.WriteLine("Average Parsing Speed was " + totalTriples / (totalTime / 1000d) + " triples/second");

            }
            catch (Exception ex)
            {
                reportError(output, "Other Exception", ex);
            }

            Console.SetOut(Console.Out);
            Console.WriteLine("Done");
            Debug.WriteLine("Finished");

            output.Close();

        }

        static void parser_Warning(string warning)
        {
            Console.WriteLine("Warning: " + warning);
        }
    }
}
