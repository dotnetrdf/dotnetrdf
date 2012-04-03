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
