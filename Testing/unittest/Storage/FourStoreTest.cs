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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Storage
{
    /// <summary>
    /// Summary description for FourStoreTest
    /// </summary>
    [TestClass]
    public class FourStoreTest
    {
        private NTriplesFormatter _formatter = new NTriplesFormatter();

        public static FourStoreConnector GetConnection()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseFourStore))
            {
                Assert.Inconclusive("Test Config marks 4store as unavailable, test cannot be run");
            }
            return new FourStoreConnector(TestConfigManager.GetSetting(TestConfigManager.FourStoreServer));
        }

        [TestMethod]
        public void StorageFourStoreSaveGraph()
        {
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = new Uri("http://example.org/4storeTest");

            FourStoreConnector fourstore = FourStoreTest.GetConnection();
            fourstore.SaveGraph(g);

            Graph h = new Graph();
            fourstore.LoadGraph(h, "http://example.org/4storeTest");

            Assert.AreEqual(g, h, "Graphs should be equal");
        }

        [TestMethod]
        public void StorageFourStoreLoadGraph()
        {
            StorageFourStoreSaveGraph();

            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = new Uri("http://example.org/4storeTest");

            FourStoreConnector fourstore = FourStoreTest.GetConnection();

            Graph h = new Graph();
            fourstore.LoadGraph(h, "http://example.org/4storeTest");

            Assert.AreEqual(g, h, "Graphs should be equal");
        }

        [TestMethod]
        public void StorageFourStoreDeleteGraph()
        {
            StorageFourStoreSaveGraph();

            FourStoreConnector fourstore = FourStoreTest.GetConnection();
            fourstore.DeleteGraph("http://example.org/4storeTest");

            Graph g = new Graph();
            fourstore.LoadGraph(g, "http://example.org/4storeTest");

            Assert.IsTrue(g.IsEmpty, "Graph should be empty as it was deleted from 4store");
        }

        [TestMethod]
        public void StorageFourStoreAddTriples()
        {
            StorageFourStoreDeleteGraph();
            StorageFourStoreSaveGraph();

            Graph g = new Graph();
            List<Triple> ts = new List<Triple>();
            ts.Add(new Triple(g.CreateUriNode(new Uri("http://example.org/subject")), g.CreateUriNode(new Uri("http://example.org/predicate")), g.CreateUriNode(new Uri("http://example.org/object"))));

            FourStoreConnector fourstore = FourStoreTest.GetConnection();
            fourstore.UpdateGraph("http://example.org/4storeTest", ts, null);

            fourstore.LoadGraph(g, "http://example.org/4storeTest");

            Assert.IsTrue(ts.All(t => g.ContainsTriple(t)), "Added Triple should be in the Graph");
        }

        [TestMethod]
        public void StorageFourStoreRemoveTriples()
        {
            StorageFourStoreAddTriples();

            Graph g = new Graph();
            List<Triple> ts = new List<Triple>();
            ts.Add(new Triple(g.CreateUriNode(new Uri("http://example.org/subject")), g.CreateUriNode(new Uri("http://example.org/predicate")), g.CreateUriNode(new Uri("http://example.org/object"))));

            FourStoreConnector fourstore = FourStoreTest.GetConnection();
            fourstore.UpdateGraph("http://example.org/4storeTest", null, ts);

            Thread.Sleep(2500);

            fourstore.LoadGraph(g, "http://example.org/4storeTest");

            Assert.IsTrue(ts.All(t => !g.ContainsTriple(t)), "Removed Triple should not have been in the Graph");
        }

        [TestMethod]
        public void StorageFourStoreUpdate()
        {
            FourStoreConnector fourstore = FourStoreTest.GetConnection();
            fourstore.Update("CREATE SILENT GRAPH <http://example.org/update>; INSERT DATA { GRAPH <http://example.org/update> { <http://example.org/subject> <http://example.org/predicate> <http://example.org/object> } }");

            Graph g = new Graph();
            fourstore.LoadGraph(g, "http://example.org/update");

            Assert.AreEqual(1, g.Triples.Count, "The CREATE GRAPH and INSERT DATA commands should result in 1 Triple in the Graph");

            fourstore.Update("DROP SILENT GRAPH <http://example.org/update>");
            Graph h = new Graph();
            fourstore.LoadGraph(h, "http://example.org/update");

            Assert.IsTrue(h.IsEmpty, "Graph should be empty after the DROP GRAPH update was issued");
            Assert.AreNotEqual(g, h, "Graphs should not be equal");
        }
    }
}
