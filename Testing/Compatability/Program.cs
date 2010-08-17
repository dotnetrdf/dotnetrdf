using System;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing;

namespace Compatability
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Graph g = new Graph();
                FileLoader.Warning += WarningPrinter;

                //HTML Writing for Graphs Test
                //FileLoader.Load(g, "InferenceTest.ttl");
                //HtmlWriter writer = new HtmlWriter();
                //writer.Save(g, "test.html");

                //RDFa Parsing Test
                //FileLoader.Load(g, "gr1.xhtml");
                //foreach (Triple t in g.Triples)
                //{
                //    Console.WriteLine(t.ToString());
                //}
                //Console.WriteLine();
                //g = new Graph();
                //FileLoader.Load(g, "gr1b.xhtml");
                //foreach (Triple t in g.Triples)
                //{
                //    Console.WriteLine(t.ToString());
                //}
                //Console.WriteLine();
                //g = new Graph();
                //FileLoader.Load(g, "0072.xhtml");
                //foreach (Triple t in g.Triples)
                //{
                //    Console.WriteLine(t.ToString());
                //}

                //SPARQL HTML Writer Tests
                SparqlResultSet results = new SparqlResultSet();
                SparqlXmlParser parser = new SparqlXmlParser();
                parser.Load(results, "test001.srx");
                foreach (SparqlResult r in results)
                {
                    Console.WriteLine(r.ToString());
                }

                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
                Console.ReadLine();
            }
        }

        static void WarningPrinter(String message)
        {
            Console.Error.WriteLine("Warning: " + message);
        }
    }
}
