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
