/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

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

namespace VDS.RDF.Test.Storage
{
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
}

