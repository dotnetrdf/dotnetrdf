using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;

namespace VDS.RDF.Test
{
    [TestClass]
    public class LoaderTests
    {
        [TestMethod]
        public void DataUri()
        {
            String rdfFragment = "@prefix : <http://example.org/> . :subject :predicate :object .";
            String rdfBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(rdfFragment));
            String rdfAscii = Uri.EscapeDataString(rdfFragment);
            List<String> uris = new List<string>()
            {
                "data:text/turtle;charset=UTF-8;base64," + rdfBase64,
                "data:text/turtle;base64," + rdfBase64,
                "data:;base64," + rdfBase64,
                "data:text/turtle;charset=UTF-8," + rdfAscii,
                "data:text/tutle," + rdfAscii,
                "data:," + rdfAscii
            };

            foreach (String uri in uris)
            {
                Uri u = new Uri(uri);

                Console.WriteLine("Testing URI " + u.ToString());

                Graph g = new Graph();
                UriLoader.Load(g, u);

                Assert.AreEqual(1, g.Triples.Count, "Expected 1 Triple to be produced");

                Console.WriteLine("Triples produced:");
                foreach (Triple t in g.Triples)
                {
                    Console.WriteLine(t.ToString());
                }
                Console.WriteLine();
            }
        }

        [TestMethod]
        public void DBPedia()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://dbpedia.org/resource/London");
            request.Accept = "application/rdf+xml";
            request.Method = "GET";

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Console.WriteLine("OK");
                    Console.WriteLine("Content Length: " + response.ContentLength);
                    Console.WriteLine("Content Type: " + response.ContentType);
                    Tools.HttpDebugRequest(request);
                    Tools.HttpDebugResponse(response);
                }
            }
            catch (WebException webEx)
            {
                Console.WriteLine("ERROR");
                Console.WriteLine(webEx.Message);
                Console.WriteLine(webEx.StackTrace);
                Assert.Fail();
            }

            request = (HttpWebRequest)WebRequest.Create("http://dbpedia.org/data/London");
            request.Accept = "application/rdf+xml";
            request.Method = "GET";

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Console.WriteLine("OK");
                    Console.WriteLine("Content Length: " + response.ContentLength);
                    Console.WriteLine("Content Type: " + response.ContentType);
                    Tools.HttpDebugRequest(request);
                    Tools.HttpDebugResponse(response);
                }
            }
            catch (WebException webEx)
            {
                Console.WriteLine("ERROR");
                Console.WriteLine(webEx.Message);
                Console.WriteLine(webEx.StackTrace);
                Assert.Fail();
            }

            try
            {
                Graph g = new Graph();
                Options.HttpDebugging = true;
                UriLoader.Load(g, new Uri("http://dbpedia.org/resource/London"));
                Console.WriteLine("OK");
                Console.WriteLine(g.Triples.Count + " Triples retrieved");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Assert.Fail();
            }
            finally
            {
                Options.HttpDebugging = false;
            }
        }

        [TestMethod]
        public void UriLoaderWithChunkedData()
        {
            try
            {
                Options.UriLoaderCaching = false;
                Options.HttpDebugging = true;
                Options.UriLoaderTimeout = 60000;
                //Options.HttpFullDebugging = true;

                Graph g = new Graph();
                UriLoader.Load(g, new Uri("http://cheminfov.informatics.indiana.edu:8080/medline/resource/medline/15760907"));

                foreach (Triple t in g.Triples)
                {
                    Console.WriteLine(t.ToString());
                }
            }
            catch (RdfParseException parseEx)
            {
                TestTools.ReportError("Parser Error", parseEx, true);
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Error", ex, true);
            }
            finally
            {
                //Options.HttpFullDebugging = false;
                Options.UriLoaderTimeout = 15000;
                Options.HttpDebugging = false;
                Options.UriLoaderCaching = true;
            }
        }
    }
}
