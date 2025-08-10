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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using Xunit;
using Xunit.Sdk;

namespace VDS.RDF.Storage.Async;


public abstract class BaseAsyncTests
{
    private const string SaveGraphUri = "http://localhost/storage/async/SaveGraph";
    private const string AddTripleUri = "http://localhost/storage/async/AddTriples";
    private const string RemoveTriplesUri = "http://localhost/storage/async/RemoveTriples";
    private const string DeleteGraphUri = "http://localhost/storage/async/DeleteGraph";
    private const string QueryGraphUri = "http://localhost/storage/async/QueryGraph";
    private const string ListGraphsUri = "http://localhost/storage/async/ListGraphs";

    protected int WaitDelay = 15000;

    /// <summary>
    /// Method to be implemented by derived classes to obtain the storage provider to test
    /// </summary>
    /// <returns></returns>
    /// <r
    protected abstract IAsyncStorageProvider GetAsyncProvider();

    protected void Fail(IAsyncStorageProvider provider, string msg)
    {
        Assert.Fail("[" + provider.GetType().Name + "] " + msg);
    }

    protected void Fail(IAsyncStorageProvider provider, string msg, Exception e)
    {
        throw new Exception("[" + provider.GetType().Name + "] " + msg, e);
    }

#region Callback-based async API tests
#pragma warning disable CS0618 // Type or member is obsolete
    protected void TestAsyncSaveLoad(IGraph g)
    {
        IAsyncStorageProvider provider = GetAsyncProvider();
        Uri graphName = (g.Name as IUriNode)?.Uri;
        try
        {
            var signal = new ManualResetEvent(false);
            AsyncStorageCallbackArgs resArgs = null;
            provider.SaveGraph(g, (_, args, state) =>
                {
                    resArgs = args;
                    signal.Set();
                }, null);

            //Wait for response, max 15s
            signal.WaitOne(WaitDelay);

            if (resArgs == null) Fail(provider, "SaveGraph() did not return in " + (WaitDelay / 1000) + " seconds");
            if (resArgs.WasSuccessful)
            {
                Console.WriteLine("Async SaveGraph() worked OK, trying async LoadGraph() to confirm operation worked as expected");

                resArgs = null;
                signal.Reset();
                var h = new Graph();
                provider.LoadGraph(h, graphName, (_, args, state) =>
                    {
                        resArgs = args;
                        signal.Set();
                    }, null);

                //Wait for response, max 15s
                signal.WaitOne(WaitDelay);

                if (resArgs == null) Fail(provider, "LoadGraph() did not return in " + (WaitDelay / 1000) + " seconds");
                if (resArgs.WasSuccessful)
                {
                    Console.WriteLine("Async LoadGraph() worked OK, checking for graph equality...");
                    GraphDiffReport diff = g.Difference(resArgs.Graph);
                    if (!diff.AreEqual) TestTools.ShowDifferences(diff);
                    Assert.True(diff.AreEqual, "[" + provider.GetType().Name + "] Graphs were not equal");
                }
                else
                {
                    Fail(provider, "LoadGraph() returned error - " + resArgs.Error.Message, resArgs.Error);
                }
            }
            else
            {
                Fail(provider, "SaveGraph() returned error - " + resArgs.Error.Message, resArgs.Error);
            }
        }
        finally 
        {
            provider.Dispose();
        }
    }

    protected void TestAsyncDelete(IGraph g)
    {
        IAsyncStorageProvider provider = GetAsyncProvider();
        if (!provider.DeleteSupported)
        {
            Console.WriteLine("[" + provider.GetType().Name + "] IO Behaviour required for this test is not supported, skipping test for this provider");
            return;
        }
        try
        {
            var signal = new ManualResetEvent(false);
            AsyncStorageCallbackArgs resArgs = null;
            g.BaseUri = UriFactory.Root.Create(DeleteGraphUri);
            provider.SaveGraph(g, (_, args, state) =>
            {
                resArgs = args;
                signal.Set();
            }, null);

            //Wait for response, max 15s
            signal.WaitOne(WaitDelay);

            if (resArgs == null) Fail(provider, "SaveGraph() did not return in " + (WaitDelay / 1000) + " seconds");
            if (resArgs.WasSuccessful)
            {
                Console.WriteLine("Async SaveGraph() worked OK, trying async DeleteGraph() to remove newly added graph...");
                resArgs = null;
                signal.Reset();
                provider.DeleteGraph(DeleteGraphUri, (_, args, state) =>
                    {
                        resArgs = args;
                        signal.Set();
                    }, null);

                //Wait for response, max 15s
                signal.WaitOne(WaitDelay);

                if (resArgs == null) Fail(provider, "DeleteGraph() did not return in " + (WaitDelay / 1000) + " seconds");
                if (resArgs.WasSuccessful)
                {
                    Console.WriteLine("Async DeleteGraph() worked OK, trying async LoadGraph() to confirm operation worked as expected");

                    resArgs = null;
                    signal.Reset();
                    var h = new Graph();
                    provider.LoadGraph(h, DeleteGraphUri, (_, args, state) =>
                    {
                        resArgs = args;
                        signal.Set();
                    }, null);

                    //Wait for response, max 15s
                    signal.WaitOne(WaitDelay);

                    if (resArgs == null) Fail(provider, "LoadGraph() did not return in " + (WaitDelay / 1000) + " seconds");
                    if (resArgs.WasSuccessful)
                    {
                        Console.WriteLine("Async LoadGraph() worked OK, checking for empty graph");
                        Assert.True(resArgs.Graph.IsEmpty, "[" + provider.GetType().Name + "] Expected an empty Graph");
                    }
                    else
                    {
                        Fail(provider, "LoadGraph() returned error - " + resArgs.Error.Message, resArgs.Error);
                    }
                }
                else
                {
                    Fail(provider, "DeleteGraph() returned error - " + resArgs.Error.Message, resArgs.Error);
                }
            }
            else
            {
                Fail(provider, "SaveGraph() returned error - " + resArgs.Error.Message, resArgs.Error);
            }
        }
        finally
        {
            provider.Dispose();
        }
    }

    protected void TestAsyncDeleteTriples(IGraph g)
    {
        IAsyncStorageProvider provider = GetAsyncProvider();
        Uri graphName = (g.Name as IUriNode)?.Uri;
        if (!provider.UpdateSupported || (provider.IOBehaviour & IOBehaviour.CanUpdateDeleteTriples) == 0)
        {
            Console.WriteLine("[" + provider.GetType().Name + "] IO Behaviour required for this test is not supported, skipping test for this provider");
            return;
        }
        try
        {
            var signal = new ManualResetEvent(false);
            AsyncStorageCallbackArgs resArgs = null;
            provider.SaveGraph(g, (_, args, state) =>
            {
                resArgs = args;
                signal.Set();
            }, null);

            //Wait for response, max 15s
            signal.WaitOne(WaitDelay);

            if (resArgs == null) Fail(provider, "SaveGraph() did not return in " + (WaitDelay / 1000) + " seconds");
            if (resArgs.WasSuccessful)
            {
                Console.WriteLine("Async SaveGraph() worked OK, trying async UpdateGraph() to remove some triples...");
                var ts = g.GetTriplesWithPredicate(UriFactory.Root.Create(RdfSpecsHelper.RdfType)).ToList();
                resArgs = null;
                signal.Reset();
                provider.UpdateGraph(graphName, null, ts, (_, args, state) =>
                {
                    resArgs = args;
                    signal.Set();
                }, null);

                //Wait for response, max 15s
                signal.WaitOne(WaitDelay);

                if (resArgs == null) Fail(provider, "UpdateGraph() did not return in " + (WaitDelay / 1000) + " seconds");
                if (resArgs.WasSuccessful)
                {
                    Console.WriteLine("Async UpdateGraph() worked OK, trying async LoadGraph() to confirm operation worked as expected");

                    resArgs = null;
                    signal.Reset();
                    var h = new Graph();
                    provider.LoadGraph(h, graphName, (_, args, state) =>
                    {
                        resArgs = args;
                        signal.Set();
                    }, null);

                    //Wait for response, max 15s
                    signal.WaitOne(WaitDelay);

                    if (resArgs == null) Fail(provider, "LoadGraph() did not return in " + (WaitDelay / 1000) + " seconds");
                    if (resArgs.WasSuccessful)
                    {
                        Console.WriteLine("Async LoadGraph() worked OK, checking for triples removed...");
                        foreach (Triple t in ts)
                        {
                            Assert.False(resArgs.Graph.ContainsTriple(t), "[" + provider.GetType().Name + "] Removed Triple " + t.ToString() + " is still present");
                        }
                    }
                    else
                    {
                        Fail(provider, "LoadGraph() returned error - " + resArgs.Error.Message, resArgs.Error);
                    }
                }
                else
                {
                    Fail(provider, "UpdateGraph() returned error - " + resArgs.Error.Message, resArgs.Error);
                }
            }
            else
            {
                Fail(provider, "SaveGraph() returned error - " + resArgs.Error.Message, resArgs.Error);
            }
        }
        finally
        {
            provider.Dispose();
        }
    }

    protected void TestAsyncAddTriples(IGraph g)
    {
        IAsyncStorageProvider provider = GetAsyncProvider();
        var graphUri = (g.Name as IUriNode)?.Uri;
        if (!provider.UpdateSupported || (provider.IOBehaviour & IOBehaviour.CanUpdateAddTriples) == 0)
        {
            Console.WriteLine("[" + provider.GetType().Name + "] IO Behaviour required for this test is not supported, skipping test for this provider");
            return;
        }
        try
        {
            var signal = new ManualResetEvent(false);
            AsyncStorageCallbackArgs resArgs = null;
            provider.SaveGraph(g, (_, args, state) =>
            {
                resArgs = args;
                signal.Set();
            }, null);

            //Wait for response, max 15s
            signal.WaitOne(WaitDelay);

            if (resArgs == null) Fail(provider, "SaveGraph() did not return in " + (WaitDelay / 1000) + " seconds");
            if (resArgs.WasSuccessful)
            {
                Console.WriteLine("Async SaveGraph() worked OK, trying async UpdateGraph() to add some triples...");
                var ts = g.GetTriplesWithPredicate(UriFactory.Root.Create(RdfSpecsHelper.RdfType)).Select(t => new Triple(t.Subject, t.Predicate, g.CreateUriNode(UriFactory.Root.Create("http://example.org/Test")))).ToList();
                resArgs = null;
                signal.Reset();
                provider.UpdateGraph(graphUri, ts, null, (_, args, state) =>
                {
                    resArgs = args;
                    signal.Set();
                }, null);

                //Wait for response, max 15s
                signal.WaitOne(WaitDelay);

                if (resArgs == null) Fail(provider, "UpdateGraph() did not return in " + (WaitDelay / 1000) + " seconds");
                if (resArgs.WasSuccessful)
                {
                    Console.WriteLine("Async UpdateGraph() worked OK, trying async LoadGraph() to confirm operation worked as expected");

                    resArgs = null;
                    signal.Reset();
                    var h = new Graph();
                    provider.LoadGraph(h, graphUri, (_, args, state) =>
                    {
                        resArgs = args;
                        signal.Set();
                    }, null);

                    //Wait for response, max 15s
                    signal.WaitOne(WaitDelay);

                    if (resArgs == null) Fail(provider, "LoadGraph() did not return in " + (WaitDelay / 1000) + " seconds");
                    if (resArgs.WasSuccessful)
                    {
                        Console.WriteLine("Async LoadGraph() worked OK, checking for triples added...");
                        foreach (Triple t in ts)
                        {
                            Assert.True(resArgs.Graph.ContainsTriple(t), "[" + provider.GetType().Name + "] Added Triple " + t.ToString() + " is not present");
                        }
                    }
                    else
                    {
                        Fail(provider, "LoadGraph() returned error - " + resArgs.Error.Message, resArgs.Error);
                    }
                }
                else
                {
                    Fail(provider, "UpdateGraph() returned error - " + resArgs.Error.Message, resArgs.Error);
                }
            }
            else
            {
                Fail(provider, "SaveGraph() returned error - " + resArgs.Error.Message, resArgs.Error);
            }
        }
        finally
        {
            provider.Dispose();
        }
    }

    protected void TestAsyncListGraphs()
    {
        IAsyncStorageProvider provider = GetAsyncProvider();
        if (!provider.ListGraphsSupported)
        {
            Console.WriteLine("[" + provider.GetType().Name + "] IO Behaviour required for this test is not supported, skipping test for this provider");
            return;
        }
        try
        {
            var signal = new ManualResetEvent(false);
            AsyncStorageCallbackArgs resArgs = null;
            provider.ListGraphs((_, args, state) =>
                {
                    resArgs = args;
                    signal.Set();
                }, null);

            //Wait, max 15s
            signal.WaitOne(WaitDelay);

            if (resArgs == null) Fail(provider, "ListGraphs() failed to return in 15s");
            if (resArgs.WasSuccessful)
            {
                foreach (Uri u in resArgs.GraphUris)
                {
                    Console.WriteLine((u != null ? u.ToString() : "Default Graph"));
                }
            }
            else
            {
                Fail(provider, "ListGraphs() returned an error - " + resArgs.Error.Message, resArgs.Error);
            }
        }
        finally
        {
            provider.Dispose();
        }            
    }

    protected void TestAsyncQuery(IGraph g)
    {
        IAsyncStorageProvider provider = GetAsyncProvider();
        if (!(provider is IAsyncQueryableStorage))
        {
            Console.WriteLine("[" + provider.GetType().Name + "] IO Behaviour required for this test is not supported, skipping test for this provider");
            return;
        }
        try
        {
            var signal = new ManualResetEvent(false);
            AsyncStorageCallbackArgs resArgs = null;
            g.BaseUri = UriFactory.Root.Create(SaveGraphUri);
            provider.SaveGraph(g, (_, args, state) =>
            {
                resArgs = args;
                signal.Set();
            }, null);

            //Wait for response, max 15s
            signal.WaitOne(WaitDelay);

            if (resArgs == null) Fail(provider, "SaveGraph() did not return in " + (WaitDelay / 1000) + " seconds");
            if (resArgs.WasSuccessful)
            {
                Console.WriteLine("Async SaveGraph() worked OK, trying async Query() to confirm operation worked as expected");

                resArgs = null;
                signal.Reset();
                ((IAsyncQueryableStorage)provider).Query("SELECT * WHERE { GRAPh <" + QueryGraphUri + "> { ?s a ?type } }", (_, args, state) =>
                {
                    resArgs = args;
                    signal.Set();
                }, null);

                //Wait for response, max 15s
                signal.WaitOne(WaitDelay);

                if (resArgs == null) Fail(provider, "Query() did not return in " + (WaitDelay / 1000) + " seconds");
                if (resArgs.WasSuccessful)
                {
                    Console.WriteLine("Async Query() worked OK, checking results...");
                    var results = resArgs.QueryResults as SparqlResultSet;
                    if (results == null) Fail(provider, "Result Set was empty");
                    foreach (SparqlResult r in results)
                    {
                        Assert.True(g.GetTriplesWithSubjectObject(r["s"], r["type"]).Any(), "Unexpected Type triple " + r["s"].ToString() + " a " + r["type"].ToString() + " was returned");
                    }
                }
                else
                {
                    Fail(provider, "LoadGraph() returned error - " + resArgs.Error.Message, resArgs.Error);
                }
            }
            else
            {
                Fail(provider, "SaveGraph() returned error - " + resArgs.Error.Message, resArgs.Error);
            }
        }
        finally
        {
            provider.Dispose();
        }
    }

    [Fact]
    public void StorageAsyncSaveLoad()
    {
        var g = new Graph(new UriNode(new Uri(SaveGraphUri)));
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        TestAsyncSaveLoad(g);
    }

    [Fact]
    public void StorageAsyncDeleteGraph()
    {
        var g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        TestAsyncDelete(g);
    }
    
    [Fact]
    public void StorageAsyncRemoveTriples()
    {
        var g = new Graph(new UriNode(new Uri(RemoveTriplesUri)));
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        TestAsyncDeleteTriples(g);
    }

    [Fact]
    public void StorageAsyncAddTriples()
    {
        var g = new Graph(new UriNode(new Uri(AddTripleUri)));
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        TestAsyncAddTriples(g);
    }

    [Fact]
    public void StorageAsyncListGraphs()
    {
        TestAsyncListGraphs();
    }

    [Fact]
    public void StorageAsyncQuery()
    {
        var g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        TestAsyncQuery(g);
    }
#pragma warning restore CS0618 // Type or member is obsolete
    #endregion

    #region Task-based async API tests

    protected async Task TestSaveLoadAsync(IGraph g)
    {
        IAsyncStorageProvider provider = GetAsyncProvider();
        try
        {
            await provider.SaveGraphAsync(g, CancellationToken.None);
            var h = new Graph();
            await provider.LoadGraphAsync(h, g.Name?.ToString(), CancellationToken.None);
            GraphDiffReport diff = g.Difference(h);
            if (!diff.AreEqual)
            {
                TestTools.ShowDifferences(diff);
            }
            Assert.True(diff.AreEqual, "[" + provider.GetType().Name + "] Graphs were not equal");
        }
        finally
        {
            provider.Dispose();
        }
    }

    protected async Task TestDeleteGraphAsync(IGraph g)
    {
        IAsyncStorageProvider provider = GetAsyncProvider();
        if (!provider.DeleteSupported)
        {
            throw SkipException.ForSkip("[" + provider.GetType().Name +
                                    "] IO Behaviour required for this test is not supported, skipping test for this provider");
        }

        try
        {
            g.BaseUri = UriFactory.Root.Create(DeleteGraphUri);
            await provider.SaveGraphAsync(g, CancellationToken.None);

            await provider.DeleteGraphAsync(DeleteGraphUri, CancellationToken.None);

            var h = new Graph();
            await provider.LoadGraphAsync(h, DeleteGraphUri, CancellationToken.None);
            Assert.True(h.IsEmpty, "[" + provider.GetType().Name + "] Expected an empty Graph");
        }
        finally
        {
            provider.Dispose();
        }
    }

    protected async Task TestDeleteTriplesAsync(IGraph g)
    {
        IAsyncStorageProvider provider = GetAsyncProvider();
        if (!provider.UpdateSupported || (provider.IOBehaviour & IOBehaviour.CanUpdateDeleteTriples) == 0)
        {
            throw SkipException.ForSkip("[" + provider.GetType().Name +
                                    "] IO Behaviour required for this test is not supported, skipping test for this provider");
        }

        try
        {
            g.BaseUri = UriFactory.Root.Create(RemoveTriplesUri);
            await provider.SaveGraphAsync(g, CancellationToken.None);
            var ts = g.GetTriplesWithPredicate(UriFactory.Root.Create(RdfSpecsHelper.RdfType)).ToList();
            await provider.UpdateGraphAsync(g.Name?.ToString(), null, ts, CancellationToken.None);
            var h = new Graph();
            await provider.LoadGraphAsync(h, g.Name?.ToString(), CancellationToken.None);

            foreach (Triple t in ts)
            {
                Assert.False(h.ContainsTriple(t),
                    "[" + provider.GetType().Name + "] Removed Triple " + t + " is still present");
            }
        }
        finally
        {
            provider.Dispose();
        }
    }

    protected async Task TestAddTriplesAsync(IGraph g)
    {
        IAsyncStorageProvider provider = GetAsyncProvider();
        if (!provider.UpdateSupported || (provider.IOBehaviour & IOBehaviour.CanUpdateAddTriples) == 0)
        {
            throw SkipException.ForSkip(
                $"[{provider.GetType().Name}] IO Behaviour required for this test is not supported, skipping test for this provider");
        }

        try
        {
            var emptyGraph = new Graph(new UriNode(UriFactory.Root.Create(AddTripleUri)));
            await provider.SaveGraphAsync(emptyGraph, CancellationToken.None);
            var ts = g.GetTriplesWithPredicate(UriFactory.Root.Create(RdfSpecsHelper.RdfType)).Select(t =>
                    new Triple(t.Subject, t.Predicate,
                        g.CreateUriNode(UriFactory.Root.Create("http://example.org/Test"))))
                .ToList();
            await provider.UpdateGraphAsync(AddTripleUri, ts, null, CancellationToken.None);

            var h = new Graph();
            await provider.LoadGraphAsync(h, AddTripleUri, CancellationToken.None);
            foreach (Triple t in ts)
            {
                Assert.True(h.ContainsTriple(t),
                    $"[{provider.GetType().Name}] Added Triple {t} is not present");
            }
        }
        finally
        {
            provider.Dispose();
        }
    }

    protected async Task TestListGraphsAsync()
    {
        IAsyncStorageProvider provider = GetAsyncProvider();
        if (!provider.ListGraphsSupported)
        {
            throw SkipException.ForSkip("[" + provider.GetType().Name +
                                    "] IO Behaviour required for this test is not supported, skipping test for this provider");
        }

        try
        {
            var emptyGraph = new Graph(new UriNode(UriFactory.Root.Create(ListGraphsUri)));
            await provider.SaveGraphAsync(emptyGraph, CancellationToken.None);
            IEnumerable<string> graphs = await provider.ListGraphsAsync(CancellationToken.None);
            Assert.Contains(ListGraphsUri, graphs);
        }
        finally
        {
            provider.Dispose();
        }
    }

    protected async Task TestQueryAsync(IGraph g)
    {
        IAsyncStorageProvider provider = GetAsyncProvider();
        if (!(provider is IAsyncQueryableStorage))
        {
            throw SkipException.ForSkip("[" + provider.GetType().Name +
                                    "] IO Behaviour required for this test is not supported, skipping test for this provider");
        }

        try
        {
            g.BaseUri = UriFactory.Root.Create(SaveGraphUri);
            await provider.SaveGraphAsync(g, CancellationToken.None);
            var results = await ((IAsyncQueryableStorage)provider).QueryAsync(
                "SELECT * WHERE { GRAPh <" + QueryGraphUri + "> { ?s a ?type } }", CancellationToken.None);
            Assert.NotNull(results);
            SparqlResultSet resultSet = Assert.IsType<SparqlResultSet>(results, exactMatch: false);
            foreach (SparqlResult r in resultSet)
            {
                Assert.True(g.GetTriplesWithSubjectObject(r["s"], r["type"]).Any(),
                    "Unexpected Type triple " + r["s"].ToString() + " a " + r["type"].ToString() +
                    " was returned");
            }
        }
        finally
        {
            provider.Dispose();
        }
    }

    [Fact]
    public async Task StorageSaveLoadAsync()
    {
        var g = new Graph(new UriNode(UriFactory.Root.Create(SaveGraphUri)));
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        await TestSaveLoadAsync(g);
    }

    [Fact]
    public async Task StorageDeleteGraphAsync()
    {
        var g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        await TestDeleteGraphAsync(g);
    }

    [Fact]
    public async Task StorageDeleteTriplesAsync()
    {
        var g = new Graph(new UriNode(new Uri(RemoveTriplesUri)));
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        await TestDeleteTriplesAsync(g);
    }

    [Fact]
    public async Task StorageAddTriplesAsync()
    {
        var g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        await TestAddTriplesAsync(g);
    }

    [Fact]
    public async Task StorageListGraphsAsync()
    {
        await TestListGraphsAsync();
    }

    [Fact]
    public async Task StorageQueryAsync()
    {
        var g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        await TestQueryAsync(g);
    }
    #endregion 

}
