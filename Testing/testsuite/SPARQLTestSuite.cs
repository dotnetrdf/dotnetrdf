using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using VDS.RDF.Query;
using System.Diagnostics;

namespace dotNetRDFTest
{
    public class SparqlTestSuite
    {

        public static Uri TestSuiteURI = new Uri("http://www.dotnetrdf.org/tests/");

        public static void reportError(StreamWriter output, String header, Exception ex)
        {
            output.WriteLine(header);
            output.WriteLine(ex.Message);
            output.WriteLine(ex.StackTrace);

            if (!(ex.InnerException == null))
            {
                output.WriteLine(ex.InnerException.Message);
                output.WriteLine(ex.InnerException.StackTrace);
            }
        }

        public static void Main(string[] args)
        {
            StreamWriter output = new StreamWriter("SPARQLTestSuite.txt");
            Console.SetOut(output);

            try
            {
                int testsPassed = 0;
                int testsFailed = 0;
                bool passed, passDesired;

                SparqlQueryParser parser = new SparqlQueryParser();
                parser.TraceTokeniser = true;
                parser.DefaultBaseUri = TestSuiteURI;

                output.WriteLine("## SPARQL Test Suite");
                output.WriteLine();

                String[] dirs = Directory.GetDirectories("sparqlparser_tests");

                foreach (String dir in dirs)
                {
                    if (dir.Contains(".svn"))
                    {
                        continue;
                    }
                    String[] files = Directory.GetFiles(dir);

                    foreach (String file in files)
                    {
                        if (!Path.GetExtension(file).Equals(".rq"))
                        {
                            continue;
                        }

                        passed = false;
                        passDesired = true;

                        output.WriteLine("## Testing " + file);
                        output.WriteLine("# Test Started at " + DateTime.Now.ToString(TestSuite.TestSuiteTimeFormat));

                        if (Path.GetFileNameWithoutExtension(file).Contains("bad"))
                        {
                            passDesired = false;
                            output.WriteLine("# Desired Result = Parsing Failed");
                        }
                        else
                        {
                            output.WriteLine("# Desired Result = Parses OK");
                        }

                        try
                        {
                            Debug.WriteLine("Testing file " + file);
                            SparqlQuery q = parser.ParseFromFile(file);

                            passed = true;
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
                            if (passed && passDesired)
                            {
                                //Passed and we wanted to Pass
                                testsPassed++;
                                output.WriteLine("# Result = Test Passed");
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

                            output.WriteLine("# Test Ended at " + DateTime.Now.ToString(TestSuite.TestSuiteTimeFormat));

                        }

                        output.WriteLine();
                    }
                }

                output.WriteLine(testsPassed + " Tests Passed");
                output.WriteLine(testsFailed + " Tests Failed");
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
