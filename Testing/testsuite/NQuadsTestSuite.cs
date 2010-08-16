using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Storage.Params;
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
                            parser.Load(store, new StreamParams(file));

                            Console.WriteLine("Parsed OK");

                            NQuadsWriter writer = new NQuadsWriter();
                            String outfile = "nquads_tests/" + Path.GetFileNameWithoutExtension(file) + ".nq";
                            writer.Save(store, new StreamParams(outfile));

                            Console.WriteLine("Serialized OK");

                            NQuadsParser nqparser = new NQuadsParser();
                            nqparser.TraceTokeniser = true;
                            TripleStore store2 = new TripleStore();
                            nqparser.Load(store2, new StreamParams(outfile));

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
