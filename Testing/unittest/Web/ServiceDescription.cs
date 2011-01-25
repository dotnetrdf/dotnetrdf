using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Web
{
    /// <summary>
    /// Summary description for ServiceDescription
    /// </summary>
    [TestClass]
    public class ServiceDescription
    {
        [TestMethod]
        public void ServiceDescriptionOptionsRequestOnSparqlServer()
        {
            Console.WriteLine("Making an OPTIONS request to the web demos SPARQL Server at http://localhost/demos/server/");
            Console.WriteLine();

            NTriplesFormatter formatter = new NTriplesFormatter();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost/demos/server/");
            request.Method = "OPTIONS";
            request.Accept = MimeTypesHelper.HttpAcceptHeader;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                if (response.Headers["Location"] != null)
                {
                    Console.WriteLine("Server returned Location: " + response.Headers["Location"]);
                    Console.WriteLine();

                    Graph g = new Graph();
                    UriLoader.Load(g, new Uri(response.Headers["Location"]));

                    foreach (Triple t in g.Triples)
                    {
                        Console.WriteLine(t.ToString(formatter));
                    }
                }
                else
                {
                    Assert.Fail("Expected the SPARQL Server to return a 200 OK with a Location: Header for an OPTIONS request");
                }
                response.Close();
            }
        }

        [TestMethod]
        public void ServiceDescriptionOptionsRequestOnSparqlServer2()
        {
            Console.WriteLine("Making an OPTIONS request to the web demos SPARQL Server at http://localhost/demos/server/some/long/path/elsewhere");
            Console.WriteLine("This Test tries to ensure that the URI resolution works correctly");
            Console.WriteLine();

            NTriplesFormatter formatter = new NTriplesFormatter();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost/demos/server/some/long/path/elsewhere");
            request.Method = "OPTIONS";
            request.Accept = MimeTypesHelper.HttpAcceptHeader;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                if (response.Headers["Location"] != null)
                {
                    Console.WriteLine("Server returned Location: " + response.Headers["Location"]);
                    Console.WriteLine();

                    Graph g = new Graph();
                    UriLoader.Load(g, new Uri(response.Headers["Location"]));

                    foreach (Triple t in g.Triples)
                    {
                        Console.WriteLine(t.ToString(formatter));
                    }
                }
                else
                {
                    Assert.Fail("Expected the SPARQL Server to return a 200 OK with a Location: Header for an OPTIONS request");
                }
                response.Close();
            }
        }
    }
}
