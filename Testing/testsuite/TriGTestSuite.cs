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
using System.Linq;
using System.IO;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace dotNetRDFTest
{
    public class TriGTestSuite
    {
        public static void Main(String[] args)
        {
            StreamWriter output = new StreamWriter("TriGTestSuite.txt");
            Console.SetOut(output);

            Console.WriteLine("## TriG Test Suite");
            Console.WriteLine();

            try
            {

                foreach (String file in Directory.GetFiles("trig_tests"))
                {
                    if (Path.GetExtension(file) == ".trig")
                    {
                        Console.WriteLine("## Testing File " + Path.GetFileName(file));

                        try
                        {
                            //Parse in
                            TriGParser parser = new TriGParser();
                            TripleStore store = new TripleStore();
                            //parser.TraceTokeniser = true;
                            parser.Load(store, file);

                            //Output the Triples
                            Console.WriteLine("# Parsed OK");
                            Console.WriteLine();
                            foreach (Triple t in store.Triples)
                            {
                                Console.WriteLine(t.ToString() + " from Graph <" + t.GraphUri.ToString() + ">");
                            }
                            Console.WriteLine();

                            //Write out
                            TriGWriter writer = new TriGWriter();
                            writer.HighSpeedModePermitted = false;
                            writer.Save(store, file + ".out");

                            //Parse back in again
                            TripleStore store2 = new TripleStore();
                            parser.Load(store2, file + ".out");

                            Console.WriteLine("# Parsed back in again");
                            if (store.Graphs.Count == store2.Graphs.Count)
                            {
                                Console.WriteLine("Correct number of Graphs");
                            }
                            else
                            {
                                Console.WriteLine("Incorrect number of Graphs - Expected " + store.Graphs.Count + " - Actual " + store2.Graphs.Count);
                            }
                            if (store.Triples.Count() == store2.Triples.Count())
                            {
                                Console.WriteLine("Correct number of Triples");
                            }
                            else
                            {
                                Console.WriteLine("Incorrect number of Triples - Expected " + store.Triples.Count() + " - Actual " + store2.Triples.Count());
                            }
                        }
                        catch (RdfParseException parseEx)
                        {
                            HandleError("Parser Error", parseEx);
                        }
                        catch (RdfException rdfEx)
                        {
                            HandleError("RDF Error", rdfEx);
                        }
                        catch (Exception ex)
                        {
                            HandleError("Other Error", ex);
                        }
                        finally
                        {
                            Console.WriteLine();
                        }
                    }
                }
            }
            catch (RdfParseException parseEx)
            {
                HandleError("Parser Error", parseEx);
            }
            catch (Exception ex)
            {
                HandleError("Other Error", ex);
            }
            finally
            {
                output.Close();
            }
        }

        private static void HandleError(String title, Exception ex)
        {
            Console.WriteLine(title);
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);

            if (ex.InnerException != null)
            {
                Console.WriteLine();
                Console.WriteLine(ex.InnerException.Message);
                Console.WriteLine(ex.InnerException.StackTrace);
            }
        }
    }
}
