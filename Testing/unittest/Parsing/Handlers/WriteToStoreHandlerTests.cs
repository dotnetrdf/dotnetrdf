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
using NUnit.Framework;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Storage;

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// Summary description for WriteToStoreHandlerTests
    /// </summary>
    [TestFixture]
    public class WriteToStoreHandlerTests
    {
        private readonly Uri TestGraphUri = new Uri("http://example.org/WriteToStoreHandlerTest");
        private readonly Uri TestBNodeUri = new Uri("http://example.org/WriteToStoreHandlerTest/BNodes");

        private void EnsureTestData()
        {
            if (!System.IO.File.Exists("temp.ttl"))
            {
                Graph g = new Graph();
                EmbeddedResourceLoader.Load(g, "dotNetRDF.Configuration.configuration.ttl");
                g.SaveToFile("temp.ttl");
            }
        }

        private void TestWriteToStoreHandler(IStorageProvider manager)
        {
            //First ensure that our test file exists
            EnsureTestData();

            //Try to ensure that the target Graph does not exist
            if (manager.DeleteSupported)
            {
                manager.DeleteGraph(TestGraphUri);
            }
            else
            {
                Graph g = new Graph();
                g.BaseUri = TestGraphUri;
                manager.SaveGraph(g);
            }

            Graph temp = new Graph();
            try
            {
                manager.LoadGraph(temp, TestGraphUri);
                Assert.IsTrue(temp.IsEmpty, "Unable to ensure that Target Graph in Store is empty prior to running Test");
            }
            catch
            {
                //An Error Loading the Graph is OK
            }

            WriteToStoreHandler handler = new WriteToStoreHandler(manager, TestGraphUri, 100);
            TurtleParser parser = new TurtleParser();
            parser.Load(handler, "temp.ttl");

            manager.LoadGraph(temp, TestGraphUri);
            Assert.IsFalse(temp.IsEmpty, "Graph should not be empty");

            Graph orig = new Graph();
            orig.LoadFromFile("temp.ttl");

            Assert.AreEqual(orig, temp, "Graphs should be equal");
        }

        private void TestWriteToStoreDatasetsHandler(IStorageProvider manager)
        {
            NodeFactory factory = new NodeFactory();
            INode a = factory.CreateUriNode(new Uri("http://example.org/a"));
            INode b = factory.CreateUriNode(new Uri("http://example.org/b"));
            INode c = factory.CreateUriNode(new Uri("http://example.org/c"));
            INode d = factory.CreateUriNode(new Uri("http://example.org/d"));

            Uri graphB = new Uri("http://example.org/graphs/b");
            Uri graphD = new Uri("http://example.org/graphs/d");

            //Try to ensure that the target Graphs do not exist
            if (manager.DeleteSupported)
            {
                manager.DeleteGraph(TestGraphUri);
                manager.DeleteGraph(graphB);
                manager.DeleteGraph(graphD);
            }
            else
            {
                Graph g = new Graph();
                g.BaseUri = TestGraphUri;
                manager.SaveGraph(g);
                g.BaseUri = graphB;
                manager.SaveGraph(g);
                g.BaseUri = graphD;
                manager.SaveGraph(g);
            }

            //Do the parsing and thus the loading
            WriteToStoreHandler handler = new WriteToStoreHandler(manager, TestGraphUri);
            NQuadsParser parser = new NQuadsParser();
            parser.Load(handler, new StreamReader("resources\\writetostore.nq"));

            //Load the expected Graphs
            Graph def = new Graph();
            manager.LoadGraph(def, TestGraphUri);
            Graph gB = new Graph();
            manager.LoadGraph(gB, graphB);
            Graph gD = new Graph();
            manager.LoadGraph(gD, graphD);

            Assert.AreEqual(2, def.Triples.Count, "Should be two triples in the default Graph");
            Assert.IsTrue(def.ContainsTriple(new Triple(a, a, a)), "Default Graph should have the a triple");
            Assert.AreEqual(1, gB.Triples.Count, "Should be one triple in the b Graph");
            Assert.IsTrue(gB.ContainsTriple(new Triple(b, b, b)), "b Graph should have the b triple");
            Assert.IsTrue(def.ContainsTriple(new Triple(c, c, c)), "Default Graph should have the c triple");
            Assert.AreEqual(1, gD.Triples.Count, "Should be one triple in the d Graph");
            Assert.IsTrue(gD.ContainsTriple(new Triple(d, d, d)), "d Graph should have the d triple");
        }

        private void TestWriteToStoreHandlerWithBNodes(IStorageProvider manager)
        {
            String fragment = "@prefix : <http://example.org/>. :subj :has [ a :BNode ; :with \"value\" ] .";

            //Try to ensure that the target Graph does not exist
            if (manager.DeleteSupported)
            {
                manager.DeleteGraph(TestBNodeUri);
            } 
            else 
            {
                Graph temp = new Graph();
                temp.BaseUri = TestBNodeUri;
                manager.SaveGraph(temp);
            }

            //Then write to the store
            TurtleParser parser = new TurtleParser();
            WriteToStoreHandler handler = new WriteToStoreHandler(manager, TestBNodeUri, 1);
            parser.Load(handler, new StringReader(fragment));

            //Then load back the data and check it
            Graph g = new Graph();
            manager.LoadGraph(g, TestBNodeUri);

            Assert.AreEqual(3, g.Triples.Count, "Should be 3 Triples");
            List<IBlankNode> nodes = g.Nodes.BlankNodes().ToList();
            for (int i = 0; i < nodes.Count; i++)
            {
                for (int j = 0; j < nodes.Count; j++)
                {
                    if (i == j) continue;
                    Assert.AreEqual(nodes[i], nodes[j], "All Blank Nodes should be the same");
                }
            }
        }

        [Test]
        public void ParsingWriteToStoreHandlerBadInstantiation()
        {
            Assert.Throws<ArgumentNullException>(() => new WriteToStoreHandler(null, null));
        }

        [Test]
        public void ParsingWriteToStoreHandlerBadInstantiation2()
        {
            Assert.Throws<ArgumentException>(() => new WriteToStoreHandler(new ReadOnlyConnector(new InMemoryManager()), null));
        }

        [Test]
        public void ParsingWriteToStoreHandlerBadInstantiation4()
        {
            Assert.Throws<ArgumentException>(() => new WriteToStoreHandler(new InMemoryManager(), null, 0));
        }

#if !PORTABLE // No VirtuosoManager in PCL
        [Test]
        public void ParsingWriteToStoreHandlerVirtuoso()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestWriteToStoreHandler(virtuoso);
        }
#endif

#if !NO_SYNC_HTTP // Require Sync interface for test
        [Test]
        public void ParsingWriteToStoreHandlerAllegroGraph()
        {
            AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();
            this.TestWriteToStoreHandler(agraph);
        }
#endif

#if !NO_SYNC_HTTP // Test requires synchronous APIs
        [Test]
        public void ParsingWriteToStoreHandlerFuseki()
        {
            try
            {
#if !NO_URICACHE
                Options.UriLoaderCaching = false;
#endif
                FusekiConnector fuseki = FusekiTest.GetConnection();
                this.TestWriteToStoreHandler(fuseki);
            }
            finally
            {
#if !NO_URICACHE
                Options.UriLoaderCaching = true;
#endif
            }
        }
#endif

        [Test]
        public void ParsingWriteToStoreHandlerInMemory()
        {
            InMemoryManager mem = new InMemoryManager();
            this.TestWriteToStoreHandler(mem);
        }

        [Test]
        public void ParsingWriteToStoreHandlerDatasetsInMemory()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestWriteToStoreDatasetsHandler(manager);
        }

#if !PORTABLE // No VirtuousoManager in PCL
        [Test]
        public void ParsingWriteToStoreHandlerDatasetsVirtuoso()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestWriteToStoreDatasetsHandler(virtuoso);
        }
#endif

#if !NO_SYNC_HTTP // Test requires synchronous APIs
        [Test]
        public void ParsingWriteToStoreHandlerBNodesAcrossBatchesAllegroGraph()
        {
            AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();
            this.TestWriteToStoreHandlerWithBNodes(agraph);
        }
#endif

#if !NO_SYNC_HTTP
        [Test]
        public void ParsingWriteToStoreHandlerBNodesAcrossBatchesFuseki()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestWriteToStoreHandlerWithBNodes(fuseki);
        }
#endif

        [Test]
        public void ParsingWriteToStoreHandlerBNodesAcrossBatchesInMemory()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestWriteToStoreHandlerWithBNodes(manager);
        }

#if !PORTABLE // No VirtuosoManager in PCL
        [Test]
        public void ParsingWriteToStoreHandlerBNodesAcrossBatchesVirtuoso()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestWriteToStoreHandlerWithBNodes(virtuoso);
        }
#endif
    }
}
