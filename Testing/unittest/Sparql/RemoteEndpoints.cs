using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Update;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class RemoteEndpoints
    {
        const String TestQueryUri = "http://localhost/demos/leviathan/";
        const String TestServerUpdateUri = "http://localhost/demos/server/update";
        const String TestServerQueryUri = "http://localhost/demos/server/query";

        const int AsyncTimeout = 10000;

        [TestMethod]
        public void SparqlRemoteEndpointLongQuery()
        {
            try
            {
                Options.HttpDebugging = true;

                StringBuilder input = new StringBuilder();
                input.AppendLine("SELECT * WHERE {?s ?p ?o}");
                input.AppendLine(new String('#', 2048));

                SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri(TestQueryUri));
                Object results = endpoint.QueryWithResultSet(input.ToString());
                if (results is SparqlResultSet)
                {
                    TestTools.ShowResults(results);
                }
                else
                {
                    Assert.Fail("Should have returned a SPARQL Result Set");
                }

            }
            finally
            {
                Options.HttpDebugging = false;
            }
        }

        [TestMethod]
        public void SparqlRemoteEndpointLongUpdate()
        {
            try
            {
                Options.HttpDebugging = true;

                StringBuilder input = new StringBuilder();
                input.AppendLine("LOAD <http://dbpedia.org/resource/Ilkeston>");
                input.AppendLine(new String('#', 2048));

                SparqlRemoteUpdateEndpoint endpoint = new SparqlRemoteUpdateEndpoint(new Uri(TestServerUpdateUri));
                endpoint.Update(input.ToString());
            }
            finally
            {
                Options.HttpDebugging = false;
            }
        }

        [TestMethod]
        public void SparqlRemoteEndpointCountHandler()
        {
            SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri(TestQueryUri));
            CountHandler handler = new CountHandler();
            endpoint.QueryWithResultGraph(handler, "CONSTRUCT WHERE { ?s ?p ?o }");

            Console.WriteLine("Triple Count: " + handler.Count);
            Assert.AreNotEqual(0, handler.Count, "Count should not be zero");
        }

        [TestMethod]
        public void SparqlRemoteEndpointResultCountHandler()
        {
            SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri(TestQueryUri));
            ResultCountHandler handler = new ResultCountHandler();
            endpoint.QueryWithResultSet(handler, "SELECT * WHERE { ?s ?p ?o }");

            Console.WriteLine("Result Count: " + handler.Count);
            Assert.AreNotEqual(0, handler.Count, "Count should not be zero");
        }

        [TestMethod]
        public void SparqlRemoteEndpointWriteThroughHandler()
        {
            SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri(TestQueryUri));
            WriteThroughHandler handler = new WriteThroughHandler(typeof(NTriplesFormatter), Console.Out, false);
            endpoint.QueryWithResultGraph(handler, "CONSTRUCT WHERE { ?s ?p ?o }");
        }

        [TestMethod]
        public void SparqlRemoteEndpointAsyncApiQueryWithResultSet()
        {
            SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri(TestQueryUri));
            ManualResetEvent signal = new ManualResetEvent(false);
            endpoint.QueryWithResultSet("SELECT * WHERE { ?s ?p ?o }", (r, s) =>
            {
                TestTools.ShowResults(r);
                signal.Set();
                signal.Close();
            }, null);

            Thread.Sleep(AsyncTimeout);
            Assert.IsTrue(signal.SafeWaitHandle.IsClosed, "WaitHandle should be closed");
        }

        [TestMethod]
        public void SparqlRemoteEndpointAsyncApiQueryWithResultGraph()
        {
            SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri(TestQueryUri));
            ManualResetEvent signal = new ManualResetEvent(false);
            endpoint.QueryWithResultGraph("CONSTRUCT WHERE { ?s ?p ?o }", (r, s) =>
            {
                TestTools.ShowResults(r);
                signal.Set();
                signal.Close();
            }, null);

            Thread.Sleep(AsyncTimeout);
            Assert.IsTrue(signal.SafeWaitHandle.IsClosed, "Wait Handle should be closed");
        }

        [TestMethod]
        public void SparqlRemoteEndpointAsyncApiUpdate()
        {
            SparqlRemoteUpdateEndpoint endpoint = new SparqlRemoteUpdateEndpoint(new Uri(TestServerUpdateUri));
            ManualResetEvent signal = new ManualResetEvent(false);
            endpoint.Update("LOAD <http://dbpedia.org/resource/Ilkeston> INTO GRAPH <http://example.org/async/graph>", s =>
            {
                signal.Set();
                signal.Close();
            }, null);

            Thread.Sleep(AsyncTimeout);
            Assert.IsTrue(signal.SafeWaitHandle.IsClosed, "Wait Handle should be closed");

            //Check that the Graph was really loaded
            SparqlRemoteEndpoint queryEndpoint = new SparqlRemoteEndpoint(new Uri(TestServerQueryUri));
            IGraph g = queryEndpoint.QueryWithResultGraph("CONSTRUCT FROM <http://example.org/async/graph> WHERE { ?s ?p ?o }");
            Assert.IsFalse(g.IsEmpty, "Graph should not be empty");
        }

        [TestMethod]
        public void SparqlRemoteEndpointSyncVsAsyncTimeDBPedia()
        {
            String query;
            using (StreamReader reader = new StreamReader("dbpedia-query-time.rq"))
            {
                query = reader.ReadToEnd();
                reader.Close();
            }

            SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri("http://dbpedia.org/sparql"), "http://dbpedia.org");
            Stopwatch timer = new Stopwatch();
            timer.Start();
            SparqlResultSet syncGetResults = endpoint.QueryWithResultSet(query) as SparqlResultSet;
            timer.Stop();
            Console.WriteLine("Sync Query (GET): " + timer.Elapsed);
            TestTools.ShowResults(syncGetResults);
            Console.WriteLine();
            timer.Reset();

            timer.Start();
            endpoint.HttpMode = "POST";
            SparqlResultSet syncPostResults = endpoint.QueryWithResultSet(query) as SparqlResultSet;
            timer.Stop();
            Console.WriteLine("Sync Query (POST): " + timer.Elapsed);
            TestTools.ShowResults(syncPostResults);
            Console.WriteLine();
            timer.Reset();

            ManualResetEvent signal = new ManualResetEvent(false);
            SparqlResultSet asyncResults = null;
            //DateTime start = DateTime.Now;
            //DateTime end = start;
            timer.Start();
            endpoint.QueryWithResultSet(query, (r, s) =>
            {
                //end = DateTime.Now;
                timer.Stop();
                asyncResults = r;
                signal.Set();
                signal.Close();
            }, null);

            Thread.Sleep(AsyncTimeout);
            Assert.IsTrue(signal.SafeWaitHandle.IsClosed, "Wait Handle should be closed");

            Console.WriteLine("Async Query: " + timer.Elapsed);//(end - start));
            TestTools.ShowResults(asyncResults);

            Assert.AreEqual(syncGetResults, asyncResults, "Result Sets should be equal");
        }

        [TestMethod]
        public void SparqlRemoteEndpointSyncVsAsyncTimeOpenLinkLOD()
        {
            String query;
            using (StreamReader reader = new StreamReader("dbpedia-query-time.rq"))
            {
                query = reader.ReadToEnd();
                reader.Close();
            }

            SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri("http://lod.openlinksw.com/sparql"), "http://dbpedia.org");
            Stopwatch timer = new Stopwatch();
            timer.Start();
            SparqlResultSet syncGetResults = endpoint.QueryWithResultSet(query) as SparqlResultSet;
            timer.Stop();
            Console.WriteLine("Sync Query (GET): " + timer.Elapsed);
            TestTools.ShowResults(syncGetResults);
            Console.WriteLine();
            timer.Reset();

            timer.Start();
            endpoint.HttpMode = "POST";
            SparqlResultSet syncPostResults = endpoint.QueryWithResultSet(query) as SparqlResultSet;
            timer.Stop();
            Console.WriteLine("Sync Query (POST): " + timer.Elapsed);
            TestTools.ShowResults(syncPostResults);
            Console.WriteLine();
            timer.Reset();

            ManualResetEvent signal = new ManualResetEvent(false);
            SparqlResultSet asyncResults = null;
            //DateTime start = DateTime.Now;
            //DateTime end = start;
            timer.Start();
            endpoint.QueryWithResultSet(query, (r, s) =>
            {
                //end = DateTime.Now;
                timer.Stop();
                asyncResults = r;
                signal.Set();
                signal.Close();
            }, null);

            Thread.Sleep(AsyncTimeout*2);
            Assert.IsTrue(signal.SafeWaitHandle.IsClosed, "Wait Handle should be closed");

            Console.WriteLine("Async Query: " + timer.Elapsed);//(end - start));
            TestTools.ShowResults(asyncResults);

            Assert.AreEqual(syncGetResults, asyncResults, "Result Sets should be equal");
        }

        [TestMethod]
        public void SparqlRemoteEndpointSyncVsAsyncTimeFactforge()
        {
            String query;
            using (StreamReader reader = new StreamReader("dbpedia-query-time.rq"))
            {
                query = reader.ReadToEnd();
                reader.Close();
            }

            SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri("http://factforge.net/sparql"));
            Stopwatch timer = new Stopwatch();
            timer.Start();
            SparqlResultSet syncGetResults = endpoint.QueryWithResultSet(query) as SparqlResultSet;
            timer.Stop();
            Console.WriteLine("Sync Query (GET): " + timer.Elapsed);
            TestTools.ShowResults(syncGetResults);
            Console.WriteLine();
            timer.Reset();

            timer.Start();
            endpoint.HttpMode = "POST";
            SparqlResultSet syncPostResults = endpoint.QueryWithResultSet(query) as SparqlResultSet;
            timer.Stop();
            Console.WriteLine("Sync Query (POST): " + timer.Elapsed);
            TestTools.ShowResults(syncPostResults);
            Console.WriteLine();
            timer.Reset();

            ManualResetEvent signal = new ManualResetEvent(false);
            SparqlResultSet asyncResults = null;
            //DateTime start = DateTime.Now;
            //DateTime end = start;
            timer.Start();
            endpoint.QueryWithResultSet(query, (r, s) =>
            {
                //end = DateTime.Now;
                timer.Stop();
                asyncResults = r;
                signal.Set();
                signal.Close();
            }, null);

            Thread.Sleep(AsyncTimeout);
            Assert.IsTrue(signal.SafeWaitHandle.IsClosed, "Wait Handle should be closed");

            Console.WriteLine("Async Query: " + timer.Elapsed);//(end - start));
            TestTools.ShowResults(asyncResults);

            Assert.AreEqual(syncGetResults, asyncResults, "Result Sets should be equal");
        }

        [TestMethod]
        public void SparqlRemoteEndpointSyncVsAsyncTimeLocal()
        {
            String query;
            using (StreamReader reader = new StreamReader("dbpedia-query-time.rq"))
            {
                query = reader.ReadToEnd();
                reader.Close();
            }

            SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri(TestQueryUri));
            Stopwatch timer = new Stopwatch();
            timer.Start();
            SparqlResultSet syncGetResults = endpoint.QueryWithResultSet(query) as SparqlResultSet;
            timer.Stop();
            Console.WriteLine("Sync Query (GET): " + timer.Elapsed);
            TestTools.ShowResults(syncGetResults);
            Console.WriteLine();
            timer.Reset();

            timer.Start();
            endpoint.HttpMode = "POST";
            SparqlResultSet syncPostResults = endpoint.QueryWithResultSet(query) as SparqlResultSet;
            timer.Stop();
            Console.WriteLine("Sync Query (POST): " + timer.Elapsed);
            TestTools.ShowResults(syncPostResults);
            Console.WriteLine();
            timer.Reset();

            ManualResetEvent signal = new ManualResetEvent(false);
            SparqlResultSet asyncResults = null;
            //DateTime start = DateTime.Now;
            //DateTime end = start;
            timer.Start();
            endpoint.QueryWithResultSet(query, (r, s) =>
            {
                //end = DateTime.Now;
                timer.Stop();
                asyncResults = r;
                signal.Set();
                signal.Close();
            }, null);

            Thread.Sleep(AsyncTimeout);
            Assert.IsTrue(signal.SafeWaitHandle.IsClosed, "Wait Handle should be closed");

            Console.WriteLine("Async Query: " + timer.Elapsed);//(end - start));
            TestTools.ShowResults(asyncResults);

            Assert.AreEqual(syncGetResults, asyncResults, "Result Sets should be equal");
        }

        [TestMethod]
        public void SparqlRemoteEndpointSyncVsAsyncTimeLocalVirtuoso()
        {
            String query;
            using (StreamReader reader = new StreamReader("dbpedia-query-time.rq"))
            {
                query = reader.ReadToEnd();
                reader.Close();
            }

            SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri("http://localhost:8890/sparql"));
            Stopwatch timer = new Stopwatch();
            timer.Start();
            SparqlResultSet syncGetResults = endpoint.QueryWithResultSet(query) as SparqlResultSet;
            timer.Stop();
            Console.WriteLine("Sync Query (GET): " + timer.Elapsed);
            TestTools.ShowResults(syncGetResults);
            Console.WriteLine();
            timer.Reset();

            timer.Start();
            endpoint.HttpMode = "POST";
            SparqlResultSet syncPostResults = endpoint.QueryWithResultSet(query) as SparqlResultSet;
            timer.Stop();
            Console.WriteLine("Sync Query (POST): " + timer.Elapsed);
            TestTools.ShowResults(syncPostResults);
            Console.WriteLine();
            timer.Reset();

            ManualResetEvent signal = new ManualResetEvent(false);
            SparqlResultSet asyncResults = null;
            //DateTime start = DateTime.Now;
            //DateTime end = start;
            timer.Start();
            endpoint.QueryWithResultSet(query, (r, s) =>
            {
                //end = DateTime.Now;
                timer.Stop();
                asyncResults = r;
                signal.Set();
                signal.Close();
            }, null);

            Thread.Sleep(AsyncTimeout);
            Assert.IsTrue(signal.SafeWaitHandle.IsClosed, "Wait Handle should be closed");

            Console.WriteLine("Async Query: " + timer.Elapsed);//(end - start));
            TestTools.ShowResults(asyncResults);

            Assert.AreEqual(syncGetResults, asyncResults, "Result Sets should be equal");
        }
    }
}
