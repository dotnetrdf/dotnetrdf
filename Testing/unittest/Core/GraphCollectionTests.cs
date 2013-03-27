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

namespace VDS.RDF.Core
{
    [TestClass]
    public class GraphCollectionTests
    {
        [TestMethod]
        public void GraphCollectionBasic1()
        {
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            store.Add(g);

            Assert.IsTrue(store.HasGraph(g.BaseUri), "Graph Collection should contain the Graph");
            Assert.AreEqual(g, store[g.BaseUri], "Graphs should be equal");
        }

        [TestMethod]
        public void GraphCollectionBasic2()
        {
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            store.Add(g);

            Assert.IsTrue(store.HasGraph(g.BaseUri), "Graph Collection should contain the Graph");
            Assert.AreEqual(g, store[g.BaseUri], "Graphs should be equal");
        }

        [TestMethod]
        public void GraphCollectionBasic3()
        {
            GraphCollection collection = new GraphCollection();
            Graph g = new Graph();
            collection.Add(g, true);

            Assert.IsTrue(collection.Contains(g.BaseUri));
        }

        [TestMethod]
        public void GraphCollectionBasic4()
        {
            GraphCollection collection = new GraphCollection();
            Graph g = new Graph();
            collection.Add(g, true);

            Assert.IsTrue(collection.Contains(g.BaseUri));
            Assert.IsTrue(collection.GraphUris.Contains(null));
        }

#if !NO_FILE
        [TestMethod]
        public void GraphCollectionDiskDemand1()
        {
            TripleStore store = new TripleStore(new DiskDemandGraphCollection());
            Graph g = new Graph();
            g.LoadFromFile("InferenceTest.ttl");
            g.BaseUri = new Uri("file:///" + Path.GetFullPath("InferenceTest.ttl"));

            Assert.IsTrue(store.HasGraph(g.BaseUri), "Graph Collection should contain the Graph");
            Assert.AreEqual(g, store[g.BaseUri], "Graphs should be equal");
        }

        [TestMethod]
        public void GraphCollectionDiskDemand2()
        {
            //Test that on-demand loading does not kick in for pre-existing graphs
            TripleStore store = new TripleStore(new DiskDemandGraphCollection());

            Graph g = new Graph();
            g.LoadFromFile("InferenceTest.ttl");
            g.BaseUri = new Uri("file:///" + Path.GetFullPath("InferenceTest.ttl"));

            Graph empty = new Graph();
            empty.BaseUri = g.BaseUri;
            store.Add(empty);

            Assert.IsTrue(store.HasGraph(g.BaseUri), "Graph Collection should contain the Graph");
            Assert.AreNotEqual(g, store[g.BaseUri], "Graphs should not be equal");
        }
#endif

#if !SILVERLIGHT
        [TestMethod]
        public void GraphCollectionWebDemand1()
        {
            TripleStore store = new TripleStore(new WebDemandGraphCollection());
            Graph g = new Graph();
            Uri u = new Uri("http://www.dotnetrdf.org/configuration#");
            g.LoadFromUri(u);
            g.BaseUri = u; 

            Assert.IsTrue(store.HasGraph(g.BaseUri), "Graph Collection should contain the Graph");
            Assert.AreEqual(g, store[g.BaseUri], "Graphs should be equal");
        }

        [TestMethod]
        public void GraphCollectionWebDemand2()
        {
            //Test that on-demand loading does not kick in for pre-existing graphs
            TripleStore store = new TripleStore(new WebDemandGraphCollection());

            Graph g = new Graph();
            Uri u = new Uri("http://www.dotnetrdf.org/configuration#");
            g.LoadFromUri(u);
            g.BaseUri = u;

            Graph empty = new Graph();
            empty.BaseUri = g.BaseUri;
            store.Add(empty);

            Assert.IsTrue(store.HasGraph(g.BaseUri), "Graph Collection should contain the Graph");
            Assert.AreNotEqual(g, store[g.BaseUri], "Graphs should not be equal");
        }
#endif
    }
}
