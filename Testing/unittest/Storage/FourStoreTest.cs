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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Storage
{

    /// <summary>
    /// Summary description for FourStoreTest
    /// </summary>
    [TestFixture]
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

#if !NO_SYNC_HTTP // The tests here all use the synchronous API
        [Test]

        public void StorageFourStoreSaveGraph()
        {
            Graph g = new Graph();
            FileLoader.Load(g, "resources\\InferenceTest.ttl");
            g.BaseUri = new Uri("http://example.org/4storeTest");

            FourStoreConnector fourstore = FourStoreTest.GetConnection();
            fourstore.SaveGraph(g);

            Graph h = new Graph();
            fourstore.LoadGraph(h, "http://example.org/4storeTest");

            Assert.AreEqual(g, h, "Graphs should be equal");
        }

        [Test]
        public void StorageFourStoreLoadGraph()
        {
            StorageFourStoreSaveGraph();

            Graph g = new Graph();
            FileLoader.Load(g, "resources\\InferenceTest.ttl");
            g.BaseUri = new Uri("http://example.org/4storeTest");

            FourStoreConnector fourstore = FourStoreTest.GetConnection();

            Graph h = new Graph();
            fourstore.LoadGraph(h, "http://example.org/4storeTest");

            Assert.AreEqual(g, h, "Graphs should be equal");
        }

        [Test]
        public void StorageFourStoreDeleteGraph()
        {
            StorageFourStoreSaveGraph();

            FourStoreConnector fourstore = FourStoreTest.GetConnection();
            fourstore.DeleteGraph("http://example.org/4storeTest");

            Graph g = new Graph();
            fourstore.LoadGraph(g, "http://example.org/4storeTest");

            Assert.IsTrue(g.IsEmpty, "Graph should be empty as it was deleted from 4store");
        }

        [Test]
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

        [Test]
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

        [Test]
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
#endif
    }
}
