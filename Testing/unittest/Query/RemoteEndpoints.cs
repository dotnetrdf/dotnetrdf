/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

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

namespace VDS.RDF.Query
{
    [TestClass]
    public class RemoteEndpoints
    {
        const int AsyncTimeout = 10000;

        public static SparqlRemoteEndpoint GetQueryEndpoint()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseIIS))
            {
                Assert.Inconclusive("Test Config marks IIS as unavailable, cannot run test");
            }
            return new SparqlRemoteEndpoint(new Uri(TestConfigManager.GetSetting(TestConfigManager.LocalGraphStoreQueryUri)));
        }

        public static SparqlRemoteUpdateEndpoint GetUpdateEndpoint()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseIIS))
            {
                Assert.Inconclusive("Test Config marks IIS as unavailable, cannot run test");
            }
            return new SparqlRemoteUpdateEndpoint(new Uri(TestConfigManager.GetSetting(TestConfigManager.LocalGraphStoreUpdateUri)));
        }

        [TestMethod]
        public void SparqlRemoteEndpointLongQuery()
        {
            try
            {
                Options.HttpDebugging = true;

                StringBuilder input = new StringBuilder();
                input.AppendLine("SELECT * WHERE {?s ?p ?o}");
                input.AppendLine(new String('#', 2048));

                SparqlRemoteEndpoint endpoint = RemoteEndpoints.GetQueryEndpoint();
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

                SparqlRemoteUpdateEndpoint endpoint = RemoteEndpoints.GetUpdateEndpoint();
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
            SparqlRemoteEndpoint endpoint = RemoteEndpoints.GetQueryEndpoint();
            CountHandler handler = new CountHandler();
            endpoint.QueryWithResultGraph(handler, "CONSTRUCT { ?s ?p ?o } WHERE { { ?s ?p ?o } UNION { GRAPH ?g { ?s ?p ?o } } }");

            Console.WriteLine("Triple Count: " + handler.Count);
            Assert.AreNotEqual(0, handler.Count, "Count should not be zero");
        }

        [TestMethod]
        public void SparqlRemoteEndpointResultCountHandler()
        {
            SparqlRemoteEndpoint endpoint = RemoteEndpoints.GetQueryEndpoint();
            ResultCountHandler handler = new ResultCountHandler();
            endpoint.QueryWithResultSet(handler, "SELECT * WHERE { { ?s ?p ?o } UNION { GRAPH ?g { ?s ?p ?o } } }");

            Console.WriteLine("Result Count: " + handler.Count);
            Assert.AreNotEqual(0, handler.Count, "Count should not be zero");
        }

        [TestMethod]
        public void SparqlRemoteEndpointWriteThroughHandler()
        {
            SparqlRemoteEndpoint endpoint = RemoteEndpoints.GetQueryEndpoint();
            WriteThroughHandler handler = new WriteThroughHandler(typeof(NTriplesFormatter), Console.Out, false);
            endpoint.QueryWithResultGraph(handler, "CONSTRUCT WHERE { ?s ?p ?o }");
        }

        [TestMethod]
        public void SparqlRemoteEndpointAsyncApiQueryWithResultSet()
        {
            SparqlRemoteEndpoint endpoint = RemoteEndpoints.GetQueryEndpoint();
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
            SparqlRemoteEndpoint endpoint = RemoteEndpoints.GetQueryEndpoint();
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
            SparqlRemoteUpdateEndpoint endpoint = RemoteEndpoints.GetUpdateEndpoint();
            ManualResetEvent signal = new ManualResetEvent(false);
            endpoint.Update("LOAD <http://dbpedia.org/resource/Ilkeston> INTO GRAPH <http://example.org/async/graph>", s =>
            {
                signal.Set();
                signal.Close();
            }, null);

            Thread.Sleep(AsyncTimeout);
            Assert.IsTrue(signal.SafeWaitHandle.IsClosed, "Wait Handle should be closed");

            //Check that the Graph was really loaded
            SparqlRemoteEndpoint queryEndpoint = RemoteEndpoints.GetQueryEndpoint();
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

            SparqlRemoteEndpoint endpoint = RemoteEndpoints.GetQueryEndpoint();
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
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseVirtuoso))
            {
                Assert.Inconclusive("Test Config marks Virtuoso as unavailable, test cannot be run");
            }

            String query;
            using (StreamReader reader = new StreamReader("dbpedia-query-time.rq"))
            {
                query = reader.ReadToEnd();
                reader.Close();
            }

            SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri(TestConfigManager.GetSetting(TestConfigManager.VirtuosoEndpoint)));
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
