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
    public class NQuadsTestSuite
    {
        public static void Main(String[] args)
        {
            StreamWriter output = new StreamWriter("NQuadsTestSuite.txt");
            Console.SetOut(output);

            Console.WriteLine("## NQuads Test Suite");
            Console.WriteLine();

            try
            {
                //Make sure Test Directory exists
                if (!Directory.Exists("nquads_tests"))
                {
                    Directory.CreateDirectory("nquads_tests");
                }

                //First read in the TriG tests and serialize to NQuads
                Console.WriteLine("# Reading in TriG files and serializing to NQuads");
                foreach (String file in Directory.GetFiles("trig_tests"))
                {
                    if (Path.GetExtension(file) == ".trig")
                    {
                        //Skip the Bad Syntax tests
                        if (Path.GetFileName(file).StartsWith("bad")) continue;

                        Console.WriteLine("Reading File " + Path.GetFileName(file));

                        try
                        {
                            TriGParser parser = new TriGParser();
                            TripleStore store = new TripleStore();
                            parser.Load(store, file);

                            Console.WriteLine("Parsed OK");

                            NQuadsWriter writer = new NQuadsWriter();
                            String outfile = "nquads_tests/" + Path.GetFileNameWithoutExtension(file) + ".nq";
                            writer.Save(store, outfile);

                            Console.WriteLine("Serialized OK");

                            NQuadsParser nqparser = new NQuadsParser();
                            nqparser.TraceTokeniser = true;
                            TripleStore store2 = new TripleStore();
                            nqparser.Load(store2, outfile);

                            Console.WriteLine("Parsed serialized NQuads OK");

                            if (store.Graphs.Count != store2.Graphs.Count)
                            {
                                Console.WriteLine("ERROR: Wrong number of Graphs when parsed back in");
                            }
                            else if (store.Triples.Count() != store2.Triples.Count())
                            {
                                Console.WriteLine("ERROR: Wrong number of Triples when parsed back in");
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
