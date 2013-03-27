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
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Storage;

namespace VDS.RDF.Storage
{
#if !NO_SYNC_HTTP // The tests here all use the synchronous API
    [TestClass]
    public class DydraTests
    {
       
        public static DydraConnector GetConnection()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseDydra))
            {
                Assert.Inconclusive("Test Config marks Dydra as unavailable, cannot run this test");
            }

            return new DydraConnector(TestConfigManager.GetSetting(TestConfigManager.DydraAccount), TestConfigManager.GetSetting(TestConfigManager.DydraRepository), TestConfigManager.GetSetting(TestConfigManager.DydraApiKey));
        }

        [TestMethod]
        public void StorageDydraSaveToDefaultGraph()
        {
            try
            {
                Options.HttpDebugging = true;

                Graph orig = new Graph();
                orig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                orig.BaseUri = null;

                DydraConnector dydra = DydraTests.GetConnection();
                dydra.SaveGraph(orig);

                Graph g = new Graph();
                dydra.LoadGraph(g, (Uri)null);

                if (orig.Triples.Count == g.Triples.Count)
                {
                    Assert.AreEqual(orig, g, "Graphs should be equal");
                }
                else
                {
                    Assert.IsTrue(g.HasSubGraph(orig), "Original Graph should be a sub-graph of retrieved Graph");
                }
            }
            finally
            {
                Options.HttpFullDebugging = false;
                Options.HttpDebugging = false;
            }
        }

        [TestMethod]
        public void StorageDydraSaveToNamedGraph()
        {
            try
            {
                Options.HttpDebugging = true;

                Graph orig = new Graph();
                orig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                orig.BaseUri = new Uri("http://example.org/storage/dydra/save/named/");

                DydraConnector dydra = DydraTests.GetConnection();
                dydra.SaveGraph(orig);

                Graph g = new Graph();
                dydra.LoadGraph(g, orig.BaseUri);

                if (orig.Triples.Count == g.Triples.Count)
                {
                    GraphDiffReport report = orig.Difference(g);
                    if (!report.AreEqual)
                    {
                        TestTools.ShowDifferences(report);
                    }
                    Assert.AreEqual(orig, g, "Graphs should be equal");
                }
                else
                {
                    Assert.IsTrue(g.HasSubGraph(orig), "Original Graph should be a sub-graph of retrieved Graph");
                }
            }
            finally
            {
                Options.HttpFullDebugging = false;
                Options.HttpDebugging = false;
            }
        }

        [TestMethod]
        public void StorageDydraLoadGraphWithHandler()
        {
            try
            {
                Options.HttpDebugging = true;

                Graph orig = new Graph();
                orig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                orig.BaseUri = new Uri("http://example.org/storage/dydra/tests/counting");

                DydraConnector dydra = DydraTests.GetConnection();
                dydra.DeleteGraph(orig.BaseUri);
                dydra.SaveGraph(orig);

                CountHandler handler = new CountHandler();
                dydra.LoadGraph(handler, orig.BaseUri);

                Assert.AreEqual(orig.Triples.Count, handler.Count, "Triple Counts should be equal");
            }
            finally
            {
                Options.HttpDebugging = false;
            }
        }

        [TestMethod]
        public void StorageDydraDeleteGraph()
        {
            try
            {
                Options.HttpDebugging = true;

                Graph orig = new Graph();
                orig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                orig.BaseUri = new Uri("http://example.org/storage/dydra/delete");

                DydraConnector dydra = DydraTests.GetConnection();
                dydra.SaveGraph(orig);

                Graph g = new Graph();
                dydra.LoadGraph(g, orig.BaseUri);

                if (orig.Triples.Count == g.Triples.Count)
                {
                    GraphDiffReport report = orig.Difference(g);
                    if (!report.AreEqual)
                    {
                        TestTools.ShowDifferences(report);
                    }
                    Assert.AreEqual(orig, g, "Graphs should be equal");
                }
                else
                {
                    Assert.IsTrue(g.HasSubGraph(orig), "Original Graph should be a sub-graph of retrieved Graph");
                }

                //Now delete the Graph
                dydra.DeleteGraph(orig.BaseUri);

                //And retrieve it again
                g = new Graph();
                dydra.LoadGraph(g, orig.BaseUri);

                Assert.IsTrue(g.IsEmpty, "Graph should be empty as was deleted from repository");
                Assert.AreNotEqual(orig, g, "Graphs should not be equal");
            }
            finally
            {
                Options.HttpDebugging = false;
            }
        }

        [TestMethod]
        public void StorageDydraListGraphs()
        {
            DydraConnector dydra = DydraTests.GetConnection();
            List<Uri> graphUris = dydra.ListGraphs().ToList();

            Console.WriteLine("Dydra returned " + graphUris.Count + " Graph URIs");
            foreach (Uri u in graphUris)
            {
                Console.WriteLine(u.ToString());
            }
        }

        [TestMethod]
        public void StorageDydraQuery()
        {
            try
            {
                Options.HttpDebugging = true;

                DydraConnector dydra = DydraTests.GetConnection();
                Object results = dydra.Query("SELECT * WHERE { { ?s a ?type } UNION { GRAPH ?g { ?s a ?type } } }");
                if (results is SparqlResultSet)
                {
                    SparqlResultSet rset = (SparqlResultSet)results;
                    TestTools.ShowResults(rset);
                }
                else
                {
                    Assert.Fail("Did not get a SPARQL Result Set as expected");
                }
            }
            finally
            {
                Options.HttpFullDebugging = false;
                Options.HttpDebugging = false;
            }
        }

        [TestMethod]
        public void StorageDydraConstructQuery()
        {
            try
            {
                Options.HttpDebugging = true;
                //Options.HttpFullDebugging = true;

                DydraConnector dydra = DydraTests.GetConnection();
                Object results = dydra.Query("CONSTRUCT { ?s a ?type } WHERE { { ?s a ?type } UNION { GRAPH ?g { ?s a ?type } } }");
                if (results is IGraph)
                {
                    IGraph g = (IGraph)results;
                    TestTools.ShowResults(g);
                }
                else
                {
                    Assert.Fail("Did not get a Graph as expected");
                }
            }
            finally
            {
                Options.HttpFullDebugging = false;
                Options.HttpDebugging = false;
            }
        }

        [TestMethod]
        public void StorageDydraAddTriples()
        {
            try
            {
                Options.HttpDebugging = true;

                DydraConnector dydra = DydraTests.GetConnection();

                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                g.BaseUri = new Uri("http://example.org/storage/dydra/update/add");
                dydra.SaveGraph(g);

                List<Triple> ts = new List<Triple>();
                ts.Add(new Triple(g.CreateUriNode(new Uri("http://example.org/subject")), g.CreateUriNode(new Uri("http://example.org/predicate")), g.CreateUriNode(new Uri("http://example.org/object"))));

                dydra.UpdateGraph(g.BaseUri, ts, null);

                g = new Graph();
                dydra.LoadGraph(g, "http://example.org/storage/dydra/update/add");

                Assert.IsTrue(ts.All(t => g.ContainsTriple(t)), "Added Triple should be in the Graph");
            }
            finally
            {
                Options.HttpDebugging = false;
            }
        }

        [TestMethod]
        public void StorageDydraRemoveTriples()
        {
            try
            {
                Options.HttpDebugging = true;

                DydraConnector dydra = DydraTests.GetConnection();

                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                g.BaseUri = new Uri("http://example.org/storage/dydra/update/remove");
                dydra.SaveGraph(g);

                List<Triple> ts = new List<Triple>();
                ts.Add(new Triple(g.CreateUriNode(new Uri("http://example.org/subject")), g.CreateUriNode(new Uri("http://example.org/predicate")), g.CreateUriNode(new Uri("http://example.org/object"))));

                dydra.UpdateGraph(g.BaseUri, null, ts);

                g = new Graph();
                dydra.LoadGraph(g, "http://example.org/storage/dydra/update/remove");

                Assert.IsTrue(ts.All(t => !g.ContainsTriple(t)), "Removed Triple should be in the Graph");
            }
            finally
            {
                Options.HttpDebugging = false;
            }
        }
    }
#endif
}

