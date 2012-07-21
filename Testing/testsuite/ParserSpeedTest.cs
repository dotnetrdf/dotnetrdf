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
using System.Diagnostics;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Tokens;

namespace dotNetRDFTest
{
    public class ParserSpeedTest
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
            StreamWriter output = new StreamWriter("ParserSpeedTest.txt");
            Notation3Parser n3parser = new Notation3Parser();
            TurtleParser ttlparser = new TurtleParser();
            ITokenQueue queue;
            DateTime start, end;
            long diff;
            bool tokeniseOnly = false;

            Console.SetOut(output);

            String[] files = { "turtle_tests/test-14.ttl", "turtle_tests/test-15.ttl", "turtle_tests/test-16.ttl" };
            foreach (String file in files)
            {
                Console.WriteLine("## Testing File " + file);

                //Use Turtle Tokeniser
                Debug.WriteLine("Testing file " + file + " using Turtle Tokeniser");
                Console.WriteLine("# Using Turtle Tokeniser Only");
                queue = new TokenQueue();
                queue.Tokeniser = new TurtleTokeniser(new StreamReader(file));
                Console.WriteLine("# Test Started at " + DateTime.Now.ToString(TestSuite.TestSuiteTimeFormat));
                queue.InitialiseBuffer();
                Console.WriteLine("# Test Finished at " + DateTime.Now.ToString(TestSuite.TestSuiteTimeFormat));
                
                //Use Turtle Parser
                if (!tokeniseOnly)
                {
                    Debug.WriteLine("Testing file " + file + " using Turtle Parser");
                    Console.WriteLine("# Using Turtle Parser");
                    Console.WriteLine("# Test Started at " + DateTime.Now.ToString(TestSuite.TestSuiteTimeFormat));
                    start = DateTime.Now;
                    Debug.WriteLine("Start " + start.ToString(TestSuite.TestSuiteTimeFormat));
                    Test(file, ttlparser, output);
                    Console.WriteLine("# Test Finished at " + DateTime.Now.ToString(TestSuite.TestSuiteTimeFormat));
                    end = DateTime.Now;
                    diff = Microsoft.VisualBasic.DateAndTime.DateDiff(Microsoft.VisualBasic.DateInterval.Second, start, end, Microsoft.VisualBasic.FirstDayOfWeek.Saturday, Microsoft.VisualBasic.FirstWeekOfYear.System);
                    Console.WriteLine("Read Time was " + diff + " seconds");
                    Debug.WriteLine("End " + end.ToString(TestSuite.TestSuiteTimeFormat));
                    Debug.WriteLine("Read Time was " + diff + " seconds");
                    Console.WriteLine();
                }

                //Use Notation 3 Tokeniser
                Debug.WriteLine("Testing file " + file + " using Notation 3 Tokeniser");
                Console.WriteLine("# Using Notation 3 Tokeniser Only");
                queue = new TokenQueue();
                queue.Tokeniser = new Notation3Tokeniser(new StreamReader(file));
                Console.WriteLine("# Test Started at " + DateTime.Now.ToString(TestSuite.TestSuiteTimeFormat));
                queue.InitialiseBuffer();
                Console.WriteLine("# Test Finished at " + DateTime.Now.ToString(TestSuite.TestSuiteTimeFormat));


                //Use Notation 3 Parser
                if (!tokeniseOnly)
                {
                    Debug.WriteLine("Testing file " + file + " using Notation 3 Parser");
                    Console.WriteLine("# Using Notation 3 Parser");
                    Console.WriteLine("# Test Started at " + DateTime.Now.ToString(TestSuite.TestSuiteTimeFormat));
                    start = DateTime.Now;
                    Debug.WriteLine("Start " + start.ToString(TestSuite.TestSuiteTimeFormat));
                    Test(file, n3parser, output);
                    Console.WriteLine("# Test Finished at " + DateTime.Now.ToString(TestSuite.TestSuiteTimeFormat));
                    end = DateTime.Now;
                    diff = Microsoft.VisualBasic.DateAndTime.DateDiff(Microsoft.VisualBasic.DateInterval.Second, start, end, Microsoft.VisualBasic.FirstDayOfWeek.Saturday, Microsoft.VisualBasic.FirstWeekOfYear.System);
                    Console.WriteLine("Read Time was " + diff + " seconds");
                    Debug.WriteLine("End " + end.ToString(TestSuite.TestSuiteTimeFormat));
                    Debug.WriteLine("Read Time was " + diff + " seconds");
                    Console.WriteLine();
                }
            }

            output.Close();

        }

        public static void Test(String file, IRdfReader parser, StreamWriter output)
        {
            try
            {
                Graph g = new Graph();
                g.BaseUri = new Uri(TestSuiteBaseUri);

                parser.Load(g, file);

                Console.WriteLine(g.Triples.Count + " Triples produced");
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
    }
}
