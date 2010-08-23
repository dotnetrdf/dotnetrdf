using System;
using System.IO;
using System.Text;
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
            Console.WriteLine(Console.Out.Encoding.WebName);
            try
            {
                Graph g = new Graph();
                FileLoader.Warning += WarningPrinter;

                //HTML Writing for Graphs Test
                //FileLoader.Load(g, "InferenceTest.ttl");
                //HtmlWriter writer = new HtmlWriter();
                //writer.Save(g, "test.html");

                //RDF/XML Writing for Graphs
                //FileLoader.Load(g, "InferenceTest.ttl");
                //NTriplesWriter ntwriter = new NTriplesWriter();
                //ntwriter.SortTriples = true;
                //FileLoader.Load(g, "test.n3");
                //ntwriter.Save(g, "test.input.nt");
                //RdfXmlWriter writer = new RdfXmlWriter();
                ////g.NamespaceMap.RemoveNamespace("eg");
                ////g.Assert(new Triple(g.CreateUriNode(new Uri("http://example.org/subject")), g.CreateUriNode(new Uri("http://example.org/predicate")), g.CreateUriNode(new Uri("http://example.org/object"))));
                //writer.Save(g, "test.rdf");
                //RdfXmlParser parser = new RdfXmlParser();
                //Graph h = new Graph();
                //parser.Load(h, "test.rdf");
                //ntwriter.Save(h, "test.output.nt");
                //if (!g.Equals(h))
                //{
                //    Console.WriteLine("Graphs were not the same before and after serialization as expected");
                //}
                //else
                //{
                //    Console.WriteLine("Graphs were same before and after serialization");
                //}

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
                //SparqlResultSet results = new SparqlResultSet();
                //SparqlXmlParser parser = new SparqlXmlParser();
                //parser.Load(results, "test001.srx");
                //foreach (SparqlResult r in results)
                //{
                //    Console.WriteLine(r.ToString());
                //}
                //SparqlHtmlWriter writer = new SparqlHtmlWriter();
                //writer.Save(results, "test.html");

                //URL Encoding Test
                //String test = "The following String needs URL Encoding <node>Test</node> 100% not a percent encode";
                String test = "This string contains UTF-8 納豆 characters";
                //String test = "This string contains UTF-8 ç´è± characters";
                //String test = "This string contains character 256 " + (char)256 + " in it";
                Console.WriteLine(test);
                Console.WriteLine();
                Console.WriteLine(HttpUtility.UrlEncode(test));
                Console.WriteLine();
                Console.WriteLine(HttpUtility.UrlEncode(HttpUtility.UrlEncode(test)));
                Console.WriteLine();
                Console.WriteLine(HttpUtility.UrlDecode(HttpUtility.UrlEncode(test)));
                Console.WriteLine();
                Console.WriteLine(HttpUtility.UrlDecode(HttpUtility.UrlDecode(HttpUtility.UrlEncode(test))));

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
