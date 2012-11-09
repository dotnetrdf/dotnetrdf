/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Events;
using VDS.RDF.Writing;

namespace dotNetRDFTest
{
    public class RdfXmlTestSuite
    {

        public const String XMLTestSuiteBaseURI = "http://www.w3.org/2000/10/rdf-tests/rdfcore";

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
            StreamWriter output = new StreamWriter("RdfXmlTestSuite.txt");
            Console.SetOut(output);

            try
            {
                int testsPassed = 0;
                int testsFailed = 0;
                bool passed, passDesired;

                //RdfXmlParser parser = new RdfXmlParser(RdfXmlParserMode.Streaming);
                RdfXmlParser parser = new RdfXmlParser(RdfXmlParserMode.DOM);
                NTriplesWriter ntwriter = new NTriplesWriter();
                parser.TraceParsing = true;
                Graph g = new Graph();

                output.WriteLine("## RDF/XML Test Suite");
                output.WriteLine();

                String[] dirs = Directory.GetDirectories("xmlrdf_tests");

                foreach (String dir in dirs)
                {
                    if (dir.Contains(".svn"))
                    {
                        continue;
                    }
                    String[] files = Directory.GetFiles(dir);

                    foreach (String file in files)
                    {
                        if (!Path.GetExtension(file).Equals(".rdf"))
                        {
                            continue;
                        }

                        passed = false;
                        passDesired = true;

                        output.WriteLine("## Testing " + file);
                        output.WriteLine("# Test Started at " + DateTime.Now.ToString(TestSuite.TestSuiteTimeFormat));

                        if (Path.GetFileNameWithoutExtension(file).StartsWith("error"))
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
                            g = new Graph();
                            g.BaseUri = new Uri(XMLTestSuiteBaseURI);
                            parser.Load(g, file);

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

                                //Try to write output
                                ntwriter.Save(g, Path.ChangeExtension(file, "out"));
                            }
                            else if (!passed && passDesired)
                            {
                                //Failed when we should have Passed
                                testsFailed++;
                                output.WriteLine("# Result = Test Failed");

                                //Show output
                                output.WriteLine("Generated the following Triples before failing:");
                                foreach (Triple t in g.Triples)
                                {
                                    Console.WriteLine(t.ToString());
                                }
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

                            output.WriteLine("# " + g.Triples.Count() + " Triples generated");
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
