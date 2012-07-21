/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using System.Diagnostics;

namespace dotNetRDFTest
{
    public class NTriplesTestSuite
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
            StreamWriter output = new StreamWriter("NTriplesTestSuite.txt");
            String[] wantTrace = { "test.nt" };
            String[] wantOutput = { };
            bool outputAll = true;
            String[] skipTests = { };

            Console.SetOut(output);

            try
            {
                int testsPassed = 0;
                int testsFailed = 0;
                String[] files = Directory.GetFiles("ntriples_tests");
                NTriplesParser parser = new NTriplesParser();
                bool passed, passDesired;
                Graph g = new Graph();

                foreach (String file in files)
                {
                    if (skipTests.Contains(Path.GetFileName(file)))
                    {
                        output.WriteLine("## Skipping Test of File " + Path.GetFileName(file));
                        output.WriteLine();
                        continue;
                    }

                    if (Path.GetExtension(file) != ".nt")
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
                        g.BaseUri = new Uri(TestSuiteBaseUri);
                        if (Path.GetFileNameWithoutExtension(file).StartsWith("bad"))
                        {
                            passDesired = false;
                            output.WriteLine("# Desired Result = Parsing Failed");
                        }
                        else
                        {
                            output.WriteLine("# Desired Result = Parses OK");
                        }

                        if (wantTrace.Contains(Path.GetFileName(file)))
                        {
                            parser.TraceTokeniser = true;
                        }
                        else
                        {
                            parser.TraceTokeniser = false;
                        }

                        parser.Load(g, file);

                        passed = true;
                        output.WriteLine("Parsed OK");

                        if (outputAll || wantOutput.Contains(Path.GetFileName(file)))
                        {
                            NTriplesWriter writer = new NTriplesWriter();
                            writer.Save(g, "ntriples_tests\\" + Path.GetFileNameWithoutExtension(file) + ".out");
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

                        output.WriteLine("# Triples Generated = " + g.Triples.Count());
                        output.WriteLine("# Test Ended at " + DateTime.Now.ToString(TestSuite.TestSuiteTimeFormat));
                    }

                    output.WriteLine();
                }

                output.WriteLine(testsPassed + " Tests Passed");
                output.WriteLine(testsFailed + " Tests Failed");

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
    }
}
