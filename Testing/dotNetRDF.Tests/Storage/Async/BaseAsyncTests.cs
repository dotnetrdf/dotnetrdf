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
using System.Text;
using System.Threading;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using VDS.RDF.XunitExtensions;

namespace VDS.RDF.Storage
{

    public abstract class BaseAsyncTests
    {
        private const String SaveGraphUri = "http://localhost/storage/async/SaveGraph";
        private const String AddTripleUri = "http://localhost/storage/async/AddTriples";
        private const String RemoveTriplesUri = "http://localhost/storage/async/RemoveTriples";
        private const String DeleteGraphUri = "http://localhost/storage/async/DeleteGraph";
        private const String QueryGraphUri = "http://localhost/storage/async/QueryGraph";

        protected int WaitDelay = 15000;

        /// <summary>
        /// Method to be implemented by derived classes to obtain the storage provider to test
        /// </summary>
        /// <returns></returns>
        /// <r
        protected abstract IAsyncStorageProvider GetAsyncProvider();

        protected void Fail(IAsyncStorageProvider provider, String msg)
        {
            Assert.True(false, "[" + provider.GetType().Name + "] " + msg);
        }

        protected void Fail(IAsyncStorageProvider provider, String msg, Exception e)
        {
            throw new Exception("[" + provider.GetType().Name + "] " + msg, e);
        }

        protected void TestAsyncSaveLoad(IGraph g)
        {
            IAsyncStorageProvider provider = this.GetAsyncProvider();
            try
            {
                ManualResetEvent signal = new ManualResetEvent(false);
                AsyncStorageCallbackArgs resArgs = null;
                g.BaseUri = UriFactory.Create(SaveGraphUri);
                provider.SaveGraph(g, (_, args, state) =>
                    {
                        resArgs = args;
                        signal.Set();
                    }, null);

                //Wait for response, max 15s
                signal.WaitOne(WaitDelay);

                if (resArgs == null) this.Fail(provider, "SaveGraph() did not return in " + (WaitDelay / 1000) + " seconds");
                if (resArgs.WasSuccessful)
                {
                    Console.WriteLine("Async SaveGraph() worked OK, trying async LoadGraph() to confirm operation worked as expected");

                    resArgs = null;
                    signal.Reset();
                    Graph h = new Graph();
                    provider.LoadGraph(h, SaveGraphUri, (_, args, state) =>
                        {
                            resArgs = args;
                            signal.Set();
                        }, null);

                    //Wait for response, max 15s
                    signal.WaitOne(WaitDelay);

                    if (resArgs == null) this.Fail(provider, "LoadGraph() did not return in " + (WaitDelay / 1000) + " seconds");
                    if (resArgs.WasSuccessful)
                    {
                        Console.WriteLine("Async LoadGraph() worked OK, checking for graph equality...");
                        GraphDiffReport diff = g.Difference(resArgs.Graph);
                        if (!diff.AreEqual) TestTools.ShowDifferences(diff);
                        Assert.True(diff.AreEqual, "[" + provider.GetType().Name + "] Graphs were not equal");
                    }
                    else
                    {
                        this.Fail(provider, "LoadGraph() returned error - " + resArgs.Error.Message, resArgs.Error);
                    }
                }
                else
                {
                    this.Fail(provider, "SaveGraph() returned error - " + resArgs.Error.Message, resArgs.Error);
                }
            }
            finally 
            {
                provider.Dispose();
            }
        }

        protected void TestAsyncDelete(IGraph g)
        {
            IAsyncStorageProvider provider = this.GetAsyncProvider();
            if (!provider.DeleteSupported)
            {
                Console.WriteLine("[" + provider.GetType().Name + "] IO Behaviour required for this test is not supported, skipping test for this provider");
                return;
            }
            try
            {
                ManualResetEvent signal = new ManualResetEvent(false);
                AsyncStorageCallbackArgs resArgs = null;
                g.BaseUri = UriFactory.Create(DeleteGraphUri);
                provider.SaveGraph(g, (_, args, state) =>
                {
                    resArgs = args;
                    signal.Set();
                }, null);

                //Wait for response, max 15s
                signal.WaitOne(WaitDelay);

                if (resArgs == null) this.Fail(provider, "SaveGraph() did not return in " + (WaitDelay / 1000) + " seconds");
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

                    if (resArgs == null) this.Fail(provider, "DeleteGraph() did not return in " + (WaitDelay / 1000) + " seconds");
                    if (resArgs.WasSuccessful)
                    {
                        Console.WriteLine("Async DeleteGraph() worked OK, trying async LoadGraph() to confirm operation worked as expected");

                        resArgs = null;
                        signal.Reset();
                        Graph h = new Graph();
                        provider.LoadGraph(h, DeleteGraphUri, (_, args, state) =>
                        {
                            resArgs = args;
                            signal.Set();
                        }, null);

                        //Wait for response, max 15s
                        signal.WaitOne(WaitDelay);

                        if (resArgs == null) this.Fail(provider, "LoadGraph() did not return in " + (WaitDelay / 1000) + " seconds");
                        if (resArgs.WasSuccessful)
                        {
                            Console.WriteLine("Async LoadGraph() worked OK, checking for empty graph");
                            Assert.True(resArgs.Graph.IsEmpty, "[" + provider.GetType().Name + "] Expected an empty Graph");
                        }
                        else
                        {
                            this.Fail(provider, "LoadGraph() returned error - " + resArgs.Error.Message, resArgs.Error);
                        }
                    }
                    else
                    {
                        this.Fail(provider, "DeleteGraph() returned error - " + resArgs.Error.Message, resArgs.Error);
                    }
                }
                else
                {
                    this.Fail(provider, "SaveGraph() returned error - " + resArgs.Error.Message, resArgs.Error);
                }
            }
            finally
            {
                provider.Dispose();
            }
        }

        protected void TestAsyncDeleteTriples(IGraph g)
        {
            IAsyncStorageProvider provider = this.GetAsyncProvider();
            if (!provider.UpdateSupported || (provider.IOBehaviour & IOBehaviour.CanUpdateDeleteTriples) == 0)
            {
                Console.WriteLine("[" + provider.GetType().Name + "] IO Behaviour required for this test is not supported, skipping test for this provider");
                return;
            }
            try
            {
                ManualResetEvent signal = new ManualResetEvent(false);
                AsyncStorageCallbackArgs resArgs = null;
                g.BaseUri = UriFactory.Create(RemoveTriplesUri);
                provider.SaveGraph(g, (_, args, state) =>
                {
                    resArgs = args;
                    signal.Set();
                }, null);

                //Wait for response, max 15s
                signal.WaitOne(WaitDelay);

                if (resArgs == null) this.Fail(provider, "SaveGraph() did not return in " + (WaitDelay / 1000) + " seconds");
                if (resArgs.WasSuccessful)
                {
                    Console.WriteLine("Async SaveGraph() worked OK, trying async UpdateGraph() to remove some triples...");
                    List<Triple> ts = g.GetTriplesWithPredicate(UriFactory.Create(RdfSpecsHelper.RdfType)).ToList();
                    resArgs = null;
                    signal.Reset();
                    provider.UpdateGraph(RemoveTriplesUri, null, ts, (_, args, state) =>
                    {
                        resArgs = args;
                        signal.Set();
                    }, null);

                    //Wait for response, max 15s
                    signal.WaitOne(WaitDelay);

                    if (resArgs == null) this.Fail(provider, "UpdateGraph() did not return in " + (WaitDelay / 1000) + " seconds");
                    if (resArgs.WasSuccessful)
                    {
                        Console.WriteLine("Async UpdateGraph() worked OK, trying async LoadGraph() to confirm operation worked as expected");

                        resArgs = null;
                        signal.Reset();
                        Graph h = new Graph();
                        provider.LoadGraph(h, RemoveTriplesUri, (_, args, state) =>
                        {
                            resArgs = args;
                            signal.Set();
                        }, null);

                        //Wait for response, max 15s
                        signal.WaitOne(WaitDelay);

                        if (resArgs == null) this.Fail(provider, "LoadGraph() did not return in " + (WaitDelay / 1000) + " seconds");
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
                            this.Fail(provider, "LoadGraph() returned error - " + resArgs.Error.Message, resArgs.Error);
                        }
                    }
                    else
                    {
                        this.Fail(provider, "UpdateGraph() returned error - " + resArgs.Error.Message, resArgs.Error);
                    }
                }
                else
                {
                    this.Fail(provider, "SaveGraph() returned error - " + resArgs.Error.Message, resArgs.Error);
                }
            }
            finally
            {
                provider.Dispose();
            }
        }

        protected void TestAsyncAddTriples(IGraph g)
        {
            IAsyncStorageProvider provider = this.GetAsyncProvider();
            if (!provider.UpdateSupported || (provider.IOBehaviour & IOBehaviour.CanUpdateAddTriples) == 0)
            {
                Console.WriteLine("[" + provider.GetType().Name + "] IO Behaviour required for this test is not supported, skipping test for this provider");
                return;
            }
            try
            {
                ManualResetEvent signal = new ManualResetEvent(false);
                AsyncStorageCallbackArgs resArgs = null;
                g.BaseUri = UriFactory.Create(AddTripleUri);
                provider.SaveGraph(g, (_, args, state) =>
                {
                    resArgs = args;
                    signal.Set();
                }, null);

                //Wait for response, max 15s
                signal.WaitOne(WaitDelay);

                if (resArgs == null) this.Fail(provider, "SaveGraph() did not return in " + (WaitDelay / 1000) + " seconds");
                if (resArgs.WasSuccessful)
                {
                    Console.WriteLine("Async SaveGraph() worked OK, trying async UpdateGraph() to add some triples...");
                    List<Triple> ts = g.GetTriplesWithPredicate(UriFactory.Create(RdfSpecsHelper.RdfType)).Select(t => new Triple(t.Subject, t.Predicate, g.CreateUriNode(UriFactory.Create("http://example.org/Test")))).ToList();
                    resArgs = null;
                    signal.Reset();
                    provider.UpdateGraph(AddTripleUri, ts, null, (_, args, state) =>
                    {
                        resArgs = args;
                        signal.Set();
                    }, null);

                    //Wait for response, max 15s
                    signal.WaitOne(WaitDelay);

                    if (resArgs == null) this.Fail(provider, "UpdateGraph() did not return in " + (WaitDelay / 1000) + " seconds");
                    if (resArgs.WasSuccessful)
                    {
                        Console.WriteLine("Async UpdateGraph() worked OK, trying async LoadGraph() to confirm operation worked as expected");

                        resArgs = null;
                        signal.Reset();
                        Graph h = new Graph();
                        provider.LoadGraph(h, AddTripleUri, (_, args, state) =>
                        {
                            resArgs = args;
                            signal.Set();
                        }, null);

                        //Wait for response, max 15s
                        signal.WaitOne(WaitDelay);

                        if (resArgs == null) this.Fail(provider, "LoadGraph() did not return in " + (WaitDelay / 1000) + " seconds");
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
                            this.Fail(provider, "LoadGraph() returned error - " + resArgs.Error.Message, resArgs.Error);
                        }
                    }
                    else
                    {
                        this.Fail(provider, "UpdateGraph() returned error - " + resArgs.Error.Message, resArgs.Error);
                    }
                }
                else
                {
                    this.Fail(provider, "SaveGraph() returned error - " + resArgs.Error.Message, resArgs.Error);
                }
            }
            finally
            {
                provider.Dispose();
            }
        }

        protected void TestAsyncListGraphs()
        {
            IAsyncStorageProvider provider = this.GetAsyncProvider();
            if (!provider.ListGraphsSupported)
            {
                Console.WriteLine("[" + provider.GetType().Name + "] IO Behaviour required for this test is not supported, skipping test for this provider");
                return;
            }
            try
            {
                ManualResetEvent signal = new ManualResetEvent(false);
                AsyncStorageCallbackArgs resArgs = null;
                provider.ListGraphs((_, args, state) =>
                    {
                        resArgs = args;
                        signal.Set();
                    }, null);

                //Wait, max 15s
                signal.WaitOne(WaitDelay);

                if (resArgs == null) this.Fail(provider, "ListGraphs() failed to return in 15s");
                if (resArgs.WasSuccessful)
                {
                    foreach (Uri u in resArgs.GraphUris)
                    {
                        Console.WriteLine((u != null ? u.ToString() : "Default Graph"));
                    }
                }
                else
                {
                    this.Fail(provider, "ListGraphs() returned an error - " + resArgs.Error.Message, resArgs.Error);
                }
            }
            finally
            {
                provider.Dispose();
            }            
        }

        protected void TestAsyncQuery(IGraph g)
        {
            IAsyncStorageProvider provider = this.GetAsyncProvider();
            if (!(provider is IAsyncQueryableStorage))
            {
                Console.WriteLine("[" + provider.GetType().Name + "] IO Behaviour required for this test is not supported, skipping test for this provider");
                return;
            }
            try
            {
                ManualResetEvent signal = new ManualResetEvent(false);
                AsyncStorageCallbackArgs resArgs = null;
                g.BaseUri = UriFactory.Create(SaveGraphUri);
                provider.SaveGraph(g, (_, args, state) =>
                {
                    resArgs = args;
                    signal.Set();
                }, null);

                //Wait for response, max 15s
                signal.WaitOne(WaitDelay);

                if (resArgs == null) this.Fail(provider, "SaveGraph() did not return in " + (WaitDelay / 1000) + " seconds");
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

                    if (resArgs == null) this.Fail(provider, "Query() did not return in " + (WaitDelay / 1000) + " seconds");
                    if (resArgs.WasSuccessful)
                    {
                        Console.WriteLine("Async Query() worked OK, checking results...");
                        SparqlResultSet results = resArgs.QueryResults as SparqlResultSet;
                        if (results == null) this.Fail(provider, "Result Set was empty");
                        foreach (SparqlResult r in results)
                        {
                            Assert.True(g.GetTriplesWithSubjectObject(r["s"], r["type"]).Any(), "Unexpected Type triple " + r["s"].ToString() + " a " + r["type"].ToString() + " was returned");
                        }
                    }
                    else
                    {
                        this.Fail(provider, "LoadGraph() returned error - " + resArgs.Error.Message, resArgs.Error);
                    }
                }
                else
                {
                    this.Fail(provider, "SaveGraph() returned error - " + resArgs.Error.Message, resArgs.Error);
                }
            }
            finally
            {
                provider.Dispose();
            }
        }

        [SkippableFact]
        public void StorageAsyncSaveLoad()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            this.TestAsyncSaveLoad(g);
        }

        [SkippableFact]
        public void StorageAsyncDeleteGraph()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            this.TestAsyncDelete(g);
        }
        
        [SkippableFact]
        public void StorageAsyncRemoveTriples()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            this.TestAsyncDeleteTriples(g);
        }

        [SkippableFact]
        public void StorageAsyncAddTriples()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            this.TestAsyncAddTriples(g);
        }

        [SkippableFact]
        public void StorageAsyncListGraphs()
        {
            this.TestAsyncListGraphs();
        }

        [SkippableFact]
        public void StorageAsyncQuery()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            this.TestAsyncQuery(g);
        }
    }
}
