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

namespace VDS.RDF.Test.Core
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
            Assert.AreEqual(g, store.Graph(g.BaseUri), "Graphs should be equal");
        }

        [TestMethod]
        public void GraphCollectionBasic2()
        {
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            store.Add(g);

            Assert.IsTrue(store.HasGraph(g.BaseUri), "Graph Collection should contain the Graph");
            Assert.AreEqual(g, store.Graph(g.BaseUri), "Graphs should be equal");
        }

        [TestMethod]
        public void GraphCollectionDiskDemand1()
        {
            TripleStore store = new TripleStore(new DiskDemandGraphCollection());
            Graph g = new Graph();
            g.LoadFromFile("InferenceTest.ttl");
            g.BaseUri = new Uri("file:///" + Path.GetFullPath("InferenceTest.ttl"));

            Assert.IsTrue(store.HasGraph(g.BaseUri), "Graph Collection should contain the Graph");
            Assert.AreEqual(g, store.Graph(g.BaseUri), "Graphs should be equal");
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
            Assert.AreNotEqual(g, store.Graph(g.BaseUri), "Graphs should not be equal");
        }

        [TestMethod]
        public void GraphCollectionWebDemand1()
        {
            TripleStore store = new TripleStore(new WebDemandGraphCollection());
            Graph g = new Graph();
            Uri u = new Uri("http://www.dotnetrdf.org/configuration#");
            g.LoadFromUri(u);
            g.BaseUri = u; 

            Assert.IsTrue(store.HasGraph(g.BaseUri), "Graph Collection should contain the Graph");
            Assert.AreEqual(g, store.Graph(g.BaseUri), "Graphs should be equal");
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
            Assert.AreNotEqual(g, store.Graph(g.BaseUri), "Graphs should not be equal");
        }
    }
}
