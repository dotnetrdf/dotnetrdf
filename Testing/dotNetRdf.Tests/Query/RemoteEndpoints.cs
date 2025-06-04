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
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using Xunit;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Update;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query;

[Obsolete("Tests for obsolete classes")]
public class RemoteEndpoints
{
    const int AsyncTimeout = 45000;
    public static readonly HttpClient HttpClient = new();

    public static SparqlRemoteEndpoint GetQueryEndpoint()
    {
        Assert.SkipUnless(TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseIIS), "Test Config marks IIS as unavailable, cannot run test");
        return new SparqlRemoteEndpoint(new Uri(TestConfigManager.GetSetting(TestConfigManager.LocalGraphStoreQueryUri)));
    }

    public static SparqlRemoteUpdateEndpoint GetUpdateEndpoint()
    {
        Assert.SkipUnless(TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseIIS), "Test Config marks IIS as unavailable, cannot run test");
        return new SparqlRemoteUpdateEndpoint(new Uri(TestConfigManager.GetSetting(TestConfigManager.LocalGraphStoreUpdateUri)));
    }

    [Fact]
    public void SparqlRemoteEndpointLongQuery()
    {
        var input = new StringBuilder();
        input.AppendLine("SELECT * WHERE {?s ?p ?o}");
        input.AppendLine(new String('#', 2048));

        SparqlRemoteEndpoint endpoint = RemoteEndpoints.GetQueryEndpoint();
        Object results = endpoint.QueryWithResultSet(input.ToString());
        Assert.IsType<SparqlResultSet>(results, exactMatch: false);
        if (results is SparqlResultSet)
        {
            TestTools.ShowResults(results);
        }
    }

    [Fact]
    public void SparqlRemoteEndpointLongUpdate()
    {
        Assert.SkipUnless(TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseRemoteParsing),
            "Test Config marks Remote Parsing as unavailable, test cannot be run");

        var input = new StringBuilder();
        input.AppendLine("LOAD <http://dbpedia.org/resource/Ilkeston>");
        input.AppendLine(new String('#', 2048));

        SparqlRemoteUpdateEndpoint endpoint = RemoteEndpoints.GetUpdateEndpoint();
        endpoint.Update(input.ToString());
    }

    [Fact]
    public void SparqlRemoteEndpointCountHandler()
    {
        SparqlRemoteEndpoint endpoint = RemoteEndpoints.GetQueryEndpoint();
        var handler = new CountHandler();
        endpoint.QueryWithResultGraph(handler, "CONSTRUCT { ?s ?p ?o } WHERE { { ?s ?p ?o } UNION { GRAPH ?g { ?s ?p ?o } } }");

        Console.WriteLine("Triple Count: " + handler.Count);
        Assert.NotEqual(0, handler.Count);
    }

    [Fact]
    public void SparqlRemoteEndpointResultCountHandler()
    {
        SparqlRemoteEndpoint endpoint = RemoteEndpoints.GetQueryEndpoint();
        var handler = new ResultCountHandler();
        endpoint.QueryWithResultSet(handler, "SELECT * WHERE { { ?s ?p ?o } UNION { GRAPH ?g { ?s ?p ?o } } }");

        Console.WriteLine("Result Count: " + handler.Count);
        Assert.NotEqual(0, handler.Count);
    }

    [Fact]
    public void SparqlRemoteEndpointMemoryLeak1()
    {
        /*
        Dim endpoint = New SparqlRemoteEndpoint(New Uri("http://localhost:8080/sesame/repositories/my_repo"))
Dim queryString As SparqlParameterizedString = New SparqlParameterizedString()
queryString.Namespaces.AddNamespace("annot", New Uri(oAppSettingsReader.GetValue("BaseUriSite", GetType(System.String)) & "/annotations.owl#"))
queryString.CommandText = "SELECT DISTINCT ?text WHERE {?annotation annot:onContent <" & _uriDocument & "> ; annot:onContentPart """ & ContentPart & """ ; annot:text ?text ; annot:isValid ""false""^^xsd:boolean . }"
Dim results As SparqlResultSet = endpoint.QueryWithResultSet(queryString.ToString)
For Each result As SparqlResult In results
Console.WriteLine(DirectCast(result.Value("text"), LiteralNode).Value)
Next
results.Dispose()
         */

        //Do a GC before attempting the test
        GC.GetTotalMemory(true);

        //First off make sure to load some data into the some
        SparqlRemoteUpdateEndpoint updateEndpoint = RemoteEndpoints.GetUpdateEndpoint();
        updateEndpoint.Update("DROP ALL; INSERT DATA { <http://subject> <http://predicate> <http://object> . }");

        var totalRuns = 10000;

        //Loop over making queries to try and reproduce the memory leak
        for (var i = 1; i <= totalRuns; i++)
        {
            SparqlRemoteEndpoint endpoint = RemoteEndpoints.GetQueryEndpoint();
            var queryString = new SparqlParameterizedString
            {
                CommandText = "SELECT * WHERE { ?s ?p ?o }"
            };

            SparqlResultSet results = endpoint.QueryWithResultSet(queryString.ToString());
            Assert.Equal(1, results.Count);
            foreach (SparqlResult result in results)
            {
                //We're just iterating to make sure we touch the whole of the results
            }
            results.Dispose();

            if (i % 500 == 0)
            {
                Debug.WriteLine("Memory Usage after " + i + " Iterations: " + Process.GetCurrentProcess().PrivateMemorySize64);
            }
        }
        Debug.WriteLine("Memory Usage after " + totalRuns + " Iterations: " + Process.GetCurrentProcess().PrivateMemorySize64);
    }

    [Fact]
    public void SparqlRemoteEndpointMemoryLeak2()
    {
        //Do a GC before attempting the test
        GC.GetTotalMemory(true);

        //First off make sure to load some data into the some
        SparqlRemoteUpdateEndpoint updateEndpoint = RemoteEndpoints.GetUpdateEndpoint();
        updateEndpoint.Update("DROP ALL");

        var totalRuns = 10000;
        var subjects = 1000;
        var predicates = 10;

        //Loop over making queries to try and reproduce the memory leak
        for (var i = 1; i <= totalRuns; i++)
        {
            //Add new data each time around
            updateEndpoint.Update("INSERT DATA { <http://subject/" + (i % subjects) + "> <http://predicate/" + (i % predicates) + "> <http://object/" + i + "> . }");

            SparqlRemoteEndpoint endpoint = RemoteEndpoints.GetQueryEndpoint();
            var queryString = new SparqlParameterizedString
            {
                CommandText = "SELECT * WHERE { <http://subject/" + (i % 1000) + "> ?p ?o }"
            };

            var handler = new ResultCountHandler();
            endpoint.QueryWithResultSet(handler, queryString.ToString());
            Assert.True(handler.Count >= 1 && handler.Count <= subjects, "Result Count " + handler.Count + " is not in expected range 1 <= x < " + (i % 1000));

            if (i % 500 == 0)
            {
                Debug.WriteLine("Memory Usage after " + i + " Iterations: " + Process.GetCurrentProcess().PrivateMemorySize64);
            }
        }
        Debug.WriteLine("Memory Usage after " + totalRuns + " Iterations: " + Process.GetCurrentProcess().PrivateMemorySize64);
    }

    [Fact]
    public void SparqlRemoteEndpointWriteThroughHandler()
    {
        SparqlRemoteEndpoint endpoint = RemoteEndpoints.GetQueryEndpoint();
        var handler = new WriteThroughHandler(typeof(NTriplesFormatter), Console.Out, false);
        endpoint.QueryWithResultGraph(handler, "CONSTRUCT WHERE { ?s ?p ?o }");
    }

    [Fact]
    public void SparqlRemoteEndpointAsyncApiQueryWithResultSet()
    {
        SparqlRemoteEndpoint endpoint = RemoteEndpoints.GetQueryEndpoint();
        var signal = new ManualResetEvent(false);
        endpoint.QueryWithResultSet("SELECT * WHERE { ?s ?p ?o }", (r, s) =>
        {
            TestTools.ShowResults(r);
            signal.Set();
            signal.Close();
        }, null);

        Thread.Sleep(AsyncTimeout);
        Assert.True(signal.SafeWaitHandle.IsClosed, "WaitHandle should be closed");
    }

    [Fact]
    public void SparqlRemoteEndpointAsyncApiQueryWithResultGraph()
    {
        SparqlRemoteEndpoint endpoint = RemoteEndpoints.GetQueryEndpoint();
        var signal = new ManualResetEvent(false);
        endpoint.QueryWithResultGraph("CONSTRUCT WHERE { ?s ?p ?o }", (r, s) =>
        {
            TestTools.ShowResults(r);
            signal.Set();
            signal.Close();
        }, null);

        Thread.Sleep(AsyncTimeout);
        Assert.True(signal.SafeWaitHandle.IsClosed, "Wait Handle should be closed");
    }

    [Fact]
    public void SparqlRemoteEndpointAsyncApiUpdate()
    {
        Assert.SkipUnless(TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseRemoteParsing),
            "Test Config marks Remote Parsing as unavailable, test cannot be run");

        SparqlRemoteUpdateEndpoint endpoint = RemoteEndpoints.GetUpdateEndpoint();
        var signal = new ManualResetEvent(false);
        endpoint.Update("LOAD <http://dbpedia.org/resource/Ilkeston> INTO GRAPH <http://example.org/async/graph>", s =>
        {
            signal.Set();
            signal.Close();
        }, null);

        Thread.Sleep(AsyncTimeout);
        Assert.True(signal.SafeWaitHandle.IsClosed, "Wait Handle should be closed");

        //Check that the Graph was really loaded
        SparqlRemoteEndpoint queryEndpoint = RemoteEndpoints.GetQueryEndpoint();
        IGraph g = queryEndpoint.QueryWithResultGraph("CONSTRUCT FROM <http://example.org/async/graph> WHERE { ?s ?p ?o }");
        Assert.False(g.IsEmpty, "Graph should not be empty");
    }

    [Fact]
    public void SparqlRemoteEndpointSyncVsAsyncTimeDBPedia()
    {
        Assert.SkipUnless(TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseRemoteParsing),
            "Test Config marks Remote Parsing as unavailable, test cannot be run");

        String query;
        using (StreamReader reader = File.OpenText("resources\\dbpedia-query-time.rq"))
        {
            query = reader.ReadToEnd();
            reader.Close();
        }

        var endpoint = new SparqlRemoteEndpoint(new Uri("http://dbpedia.org/sparql"), "http://dbpedia.org");
        var timer = new Stopwatch();
        timer.Start();
        var syncGetResults = endpoint.QueryWithResultSet(query) as SparqlResultSet;
        timer.Stop();
        Console.WriteLine("Sync Query (GET): " + timer.Elapsed);
        TestTools.ShowResults(syncGetResults);
        Console.WriteLine();
        timer.Reset();

        timer.Start();
        endpoint.HttpMode = "POST";
        var syncPostResults = endpoint.QueryWithResultSet(query) as SparqlResultSet;
        timer.Stop();
        Console.WriteLine("Sync Query (POST): " + timer.Elapsed);
        TestTools.ShowResults(syncPostResults);
        Console.WriteLine();
        timer.Reset();

        var signal = new ManualResetEvent(false);
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
        Assert.True(signal.SafeWaitHandle.IsClosed, "Wait Handle should be closed");

        Console.WriteLine("Async Query: " + timer.Elapsed);//(end - start));
        TestTools.ShowResults(asyncResults);

        Assert.Equal(syncGetResults, asyncResults);
    }

    [Fact(Skip = "TP: I think that we should either ignore or remove redundant tests")]
    public void SparqlRemoteEndpointSyncVsAsyncTimeOpenLinkLOD()
    {
        String query;
        using (StreamReader reader = File.OpenText("resources\\dbpedia-query-time.rq"))
        {
            query = reader.ReadToEnd();
            reader.Close();
        }

        var endpoint = new SparqlRemoteEndpoint(new Uri("http://lod.openlinksw.com/sparql"), "http://dbpedia.org")
        {
            Timeout = AsyncTimeout
        };
        var timer = new Stopwatch();
        timer.Start();
        var syncGetResults = endpoint.QueryWithResultSet(query) as SparqlResultSet;
        timer.Stop();
        Console.WriteLine("Sync Query (GET): " + timer.Elapsed);
        TestTools.ShowResults(syncGetResults);
        Console.WriteLine();
        timer.Reset();

        timer.Start();
        endpoint.HttpMode = "POST";
        var syncPostResults = endpoint.QueryWithResultSet(query) as SparqlResultSet;
        timer.Stop();
        Console.WriteLine("Sync Query (POST): " + timer.Elapsed);
        TestTools.ShowResults(syncPostResults);
        Console.WriteLine();
        timer.Reset();

        var signal = new ManualResetEvent(false);
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

        Thread.Sleep(AsyncTimeout * 2);
        Assert.True(signal.SafeWaitHandle.IsClosed, "Wait Handle should be closed");

        Console.WriteLine("Async Query: " + timer.Elapsed);//(end - start));
        TestTools.ShowResults(asyncResults);

        Assert.Equal(syncGetResults, asyncResults);
    }

    [Fact(Skip = "TP: I think that we should either ignore or remove redundant tests")]
    public void SparqlRemoteEndpointSyncVsAsyncTimeFactforge()
    {
        String query;
        using (StreamReader reader = File.OpenText("resources\\dbpedia-query-time.rq"))
        {
            query = reader.ReadToEnd();
            reader.Close();
        }

        var endpoint = new SparqlRemoteEndpoint(new Uri("http://factforge.net/sparql"))
        {
            Timeout = AsyncTimeout
        };
        var timer = new Stopwatch();
        timer.Start();
        var syncGetResults = endpoint.QueryWithResultSet(query) as SparqlResultSet;
        timer.Stop();
        Console.WriteLine("Sync Query (GET): " + timer.Elapsed);
        TestTools.ShowResults(syncGetResults);
        Console.WriteLine();
        timer.Reset();

        timer.Start();
        endpoint.HttpMode = "POST";
        var syncPostResults = endpoint.QueryWithResultSet(query) as SparqlResultSet;
        timer.Stop();
        Console.WriteLine("Sync Query (POST): " + timer.Elapsed);
        TestTools.ShowResults(syncPostResults);
        Console.WriteLine();
        timer.Reset();

        var signal = new ManualResetEvent(false);
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
        Assert.True(signal.SafeWaitHandle.IsClosed, "Wait Handle should be closed");

        Console.WriteLine("Async Query: " + timer.Elapsed);//(end - start));
        TestTools.ShowResults(asyncResults);

        Assert.Equal(syncGetResults, asyncResults);
    }

    [Fact]
    public void SparqlRemoteEndpointSyncVsAsyncTimeLocal()
    {
        String query;
        using (StreamReader reader = File.OpenText(Path.Combine("resources", "dbpedia-query-time.rq")))
        {
            query = reader.ReadToEnd();
            reader.Close();
        }

        SparqlRemoteEndpoint endpoint = RemoteEndpoints.GetQueryEndpoint();
        var timer = new Stopwatch();
        timer.Start();
        var syncGetResults = endpoint.QueryWithResultSet(query) as SparqlResultSet;
        timer.Stop();
        Console.WriteLine("Sync Query (GET): " + timer.Elapsed);
        TestTools.ShowResults(syncGetResults);
        Console.WriteLine();
        timer.Reset();

        timer.Start();
        endpoint.HttpMode = "POST";
        var syncPostResults = endpoint.QueryWithResultSet(query) as SparqlResultSet;
        timer.Stop();
        Console.WriteLine("Sync Query (POST): " + timer.Elapsed);
        TestTools.ShowResults(syncPostResults);
        Console.WriteLine();
        timer.Reset();

        var signal = new ManualResetEvent(false);
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
        Assert.True(signal.SafeWaitHandle.IsClosed, "Wait Handle should be closed");

        Console.WriteLine("Async Query: " + timer.Elapsed);//(end - start));
        TestTools.ShowResults(asyncResults);

        Assert.Equal(syncGetResults, asyncResults);
    }

    [Fact]
    public void SparqlRemoteEndpointSyncVsAsyncTimeLocalVirtuoso()
    {
        Assert.SkipUnless(TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseRemoteParsing),
            "Test Config marks Remote Parsing as unavailable, test cannot be run");

        String query;
        using (StreamReader reader = File.OpenText("dbpedia-query-time.rq"))
        {
            query = reader.ReadToEnd();
            reader.Close();
        }

        var endpoint = new SparqlRemoteEndpoint(new Uri(TestConfigManager.GetSetting(TestConfigManager.VirtuosoEndpoint)))
        {
            Timeout = AsyncTimeout
        };
        var timer = new Stopwatch();
        timer.Start();
        var syncGetResults = endpoint.QueryWithResultSet(query) as SparqlResultSet;
        timer.Stop();
        Console.WriteLine("Sync Query (GET): " + timer.Elapsed);
        TestTools.ShowResults(syncGetResults);
        Console.WriteLine();
        timer.Reset();

        timer.Start();
        endpoint.HttpMode = "POST";
        var syncPostResults = endpoint.QueryWithResultSet(query) as SparqlResultSet;
        timer.Stop();
        Console.WriteLine("Sync Query (POST): " + timer.Elapsed);
        TestTools.ShowResults(syncPostResults);
        Console.WriteLine();
        timer.Reset();

        var signal = new ManualResetEvent(false);
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
        Assert.True(signal.SafeWaitHandle.IsClosed, "Wait Handle should be closed");

        Console.WriteLine("Async Query: " + timer.Elapsed);//(end - start));
        TestTools.ShowResults(asyncResults);

        Assert.Equal(syncGetResults, asyncResults);
    }
}
