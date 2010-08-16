using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace dotNetRDFTest
{
    public class UriResolutionTest
    {

        public static void Main(string[] args)
        {
            StreamWriter output = new StreamWriter("URIResolutionTest.txt");
            try
            {
                //Ask User for a Uri
                Console.WriteLine("Enter a URI to retrieve: ");
                String uri = Console.ReadLine();

                //Set Output
                Console.SetOut(output);

                Console.WriteLine("## URI Resolution Test Suite");

                //Load the Test RDF
                Graph g = new Graph();
                UriLoader.Load(g, new Uri(uri));

                Console.WriteLine();

                Console.WriteLine("Following Triples were generated");
                foreach (Triple t in g.Triples)
                {
                    Console.WriteLine(t.ToString());
                }
                Console.WriteLine();

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

            output.Close();
        }
    }
}
