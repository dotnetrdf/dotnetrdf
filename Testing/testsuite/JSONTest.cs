using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace dotNetRDFTest
{
    public class JsonTest
    {
        public static void Main(String[] args)
        {
            try
            {
                Console.WriteLine("Reading a RDF/XML format Test Graph");
                RdfXmlParser parser = new RdfXmlParser();
                Graph g = new Graph();
                parser.Load(g, "JSONTest.rdf");

                Console.WriteLine("Serializing back to RDF/JSON");
                RdfJsonWriter writer = new RdfJsonWriter();
                writer.Save(g, "JSONTest.rdf.json");

                Console.WriteLine("Reading back from the RDF/JSON");
                RdfJsonParser jsonparser = new RdfJsonParser();
                Graph h = new Graph();
                jsonparser.Load(h, "JSONTest.rdf.json");

                Console.WriteLine("Outputting to NTriples");
                NTriplesWriter ntwriter = new NTriplesWriter();
                ntwriter.Save(h, "JSONTest.rdf.json.out");

                Console.WriteLine("Reading a RDF/JSON format Test Graph with tons of Comments in it");
                Graph i = new Graph();
                jsonparser.Load(i, "JSONTest.json");

                Console.WriteLine("Outputting to NTriples");
                ntwriter.Save(i, "JSONTest.json.out");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException.Message);
                    Console.WriteLine(ex.InnerException.StackTrace);
                }
            }
        }
    }
}
