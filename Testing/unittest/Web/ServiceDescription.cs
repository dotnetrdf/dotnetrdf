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
                IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                Graph g = new Graph();
                parser.Load(g, new StreamReader(response.GetResponseStream()));

                TestTools.ShowGraph(g);

                Assert.IsFalse(g.IsEmpty, "A non-empty Service Description Graph should have been returned");

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
                IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                Graph g = new Graph();
                parser.Load(g, new StreamReader(response.GetResponseStream()));

                TestTools.ShowGraph(g);

                Assert.IsFalse(g.IsEmpty, "A non-empty Service Description Graph should have been returned");

                response.Close();
            }
        }

        [TestMethod]
        public void ServiceDescriptionDescriptionUriSparqlServer()
        {
            Console.WriteLine("Making an request for the Service Description from the web demos SPARQL Server at http://localhost/demos/server/description");
            Console.WriteLine();

            NTriplesFormatter formatter = new NTriplesFormatter();
            Graph g = new Graph();
            UriLoader.Load(g, new Uri("http://localhost/demos/server/description"));
            TestTools.ShowGraph(g);

            Assert.IsFalse(g.IsEmpty, "A non-empty Service Description Graph should have been returned");
        }

        [TestMethod]
        public void ServiceDescriptionOptionsRequestOnQueryHandler()
        {
            Console.WriteLine("Making an OPTIONS request to the web demos Query Handler at http://localhost/demos/leviathan/");
            Console.WriteLine();

            NTriplesFormatter formatter = new NTriplesFormatter();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost/demos/leviathan/");
            request.Method = "OPTIONS";
            request.Accept = MimeTypesHelper.HttpAcceptHeader;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                Graph g = new Graph();
                parser.Load(g, new StreamReader(response.GetResponseStream()));

                TestTools.ShowGraph(g);

                Assert.IsFalse(g.IsEmpty, "A non-empty Service Description Graph should have been returned");

                response.Close();
            }
        }

        [TestMethod]
        public void ServiceDescriptionOptionsRequestOnSparqlServer3()
        {
            Console.WriteLine("Making an OPTIONS request to the web demos SPARQL Server Query Endpoint at http://localhost/demos/server/query");
            Console.WriteLine();

            NTriplesFormatter formatter = new NTriplesFormatter();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost/demos/server/query");
            request.Method = "OPTIONS";
            request.Accept = MimeTypesHelper.HttpAcceptHeader;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                Graph g = new Graph();
                parser.Load(g, new StreamReader(response.GetResponseStream()));

                TestTools.ShowGraph(g);

                Assert.IsFalse(g.IsEmpty, "A non-empty Service Description Graph should have been returned");

                response.Close();
            }
        }

        [TestMethod]
        public void ServiceDescriptionOptionsRequestOnSparqlServer4()
        {
            Console.WriteLine("Making an OPTIONS request to the web demos SPARQL Server Update Endpoint at http://localhost/demos/server/update");
            Console.WriteLine();

            NTriplesFormatter formatter = new NTriplesFormatter();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost/demos/server/update");
            request.Method = "OPTIONS";
            request.Accept = MimeTypesHelper.HttpAcceptHeader;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                Graph g = new Graph();
                parser.Load(g, new StreamReader(response.GetResponseStream()));

                TestTools.ShowGraph(g);

                Assert.IsFalse(g.IsEmpty, "A non-empty Service Description Graph should have been returned");

                response.Close();
            }
        }
    }

}
