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
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Configuration;
using VDS.RDF.Query.PropertyFunctions;

namespace VDS.RDF.Test.Configuration
{
    [TestClass]
    public class LoadObjectTests
    {
        [TestMethod]
        public void ConfigurationLoadObjectPropertyFunctionFactory()
        {
            String graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:SparqlPropertyFunctionFactory ;
  dnr:type """ + typeof(MockPropertyFunctionFactory).AssemblyQualifiedName + @""" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            IPropertyFunctionFactory factory = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as IPropertyFunctionFactory;
            Assert.IsNotNull(factory);
            Assert.AreEqual(typeof(MockPropertyFunctionFactory), factory.GetType());
        }

        [TestMethod]
        public void ConfigurationLoadObjectTripleCollection1()
        {
            String graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:TripleCollection ;
  dnr:type ""VDS.RDF.TreeIndexedTripleCollection"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            BaseTripleCollection collection = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as BaseTripleCollection;
            Assert.IsNotNull(collection);
            Assert.AreEqual(typeof(TreeIndexedTripleCollection), collection.GetType());
        }

        [TestMethod]
        public void ConfigurationLoadObjectTripleCollection2()
        {
            String graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:TripleCollection ;
  dnr:type ""VDS.RDF.ThreadSafeTripleCollection"" ;
  dnr:usingTripleCollection _:b .
_:b a dnr:TripleCollection ;
  dnr:type ""VDS.RDF.TreeIndexedTripleCollection"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            BaseTripleCollection collection = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as BaseTripleCollection;
            Assert.IsNotNull(collection);
            Assert.AreEqual(typeof(ThreadSafeTripleCollection), collection.GetType());
        }

        [TestMethod]
        public void ConfigurationLoadObjectGraphCollection1()
        {
            String graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:GraphCollection ;
  dnr:type ""VDS.RDF.GraphCollection"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            BaseGraphCollection collection = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as BaseGraphCollection;
            Assert.IsNotNull(collection);
            Assert.AreEqual(typeof(GraphCollection), collection.GetType());
        }

        [TestMethod]
        public void ConfigurationLoadObjectGraphCollection2()
        {
            String graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:GraphCollection ;
  dnr:type ""VDS.RDF.ThreadSafeGraphCollection"" ;
  dnr:usingGraphCollection _:b .
_:b a dnr:GraphCollection ;
  dnr:type ""VDS.RDF.GraphCollection"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            BaseGraphCollection collection = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as BaseGraphCollection;
            Assert.IsNotNull(collection);
            Assert.AreEqual(typeof(ThreadSafeGraphCollection), collection.GetType());
        }

        [TestMethod]
        public void ConfigurationLoadObjectGraphCollection3()
        {
            String graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:GraphCollection ;
  dnr:type ""VDS.RDF.DiskDemandGraphCollection"" ;
  dnr:usingGraphCollection _:b .
_:b a dnr:GraphCollection ;
  dnr:type ""VDS.RDF.GraphCollection"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            BaseGraphCollection collection = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as BaseGraphCollection;
            Assert.IsNotNull(collection);
            Assert.AreEqual(typeof(DiskDemandGraphCollection), collection.GetType());
        }

        [TestMethod]
        public void ConfigurationLoadObjectGraphCollection4()
        {
            String graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:GraphCollection ;
  dnr:type ""VDS.RDF.WebDemandGraphCollection"" ;
  dnr:usingGraphCollection _:b .
_:b a dnr:GraphCollection ;
  dnr:type ""VDS.RDF.GraphCollection"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            BaseGraphCollection collection = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as BaseGraphCollection;
            Assert.IsNotNull(collection);
            Assert.AreEqual(typeof(WebDemandGraphCollection), collection.GetType());
        }

        [TestMethod]
        public void ConfigurationLoadObjectGraphCollection5()
        {
            String graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:GraphCollection ;
  dnr:type ""VDS.RDF.WebDemandGraphCollection"" ;
  dnr:usingGraphCollection _:b .
_:b a dnr:GraphCollection ;
  dnr:type ""VDS.RDF.ThreadSafeGraphCollection"" ;
  dnr:usingGraphCollection _:c .
_:c a dnr:GraphCollection ;
  dnr:type ""VDS.RDF.GraphCollection"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            BaseGraphCollection collection = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as BaseGraphCollection;
            Assert.IsNotNull(collection);
            Assert.AreEqual(typeof(WebDemandGraphCollection), collection.GetType());
        }

        [TestMethod]
        public void ConfigurationLoadObjectGraphEmpty1()
        {
            String graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:Graph ;
  dnr:type ""VDS.RDF.Graph"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            IGraph result = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as IGraph;
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(Graph), result.GetType());
        }

        [TestMethod]
        public void ConfigurationLoadObjectGraphEmpty2()
        {
            String graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:Graph ;
  dnr:type ""VDS.RDF.ThreadSafeGraph"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            IGraph result = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as IGraph;
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(ThreadSafeGraph), result.GetType());
        }

        [TestMethod]
        public void ConfigurationLoadObjectGraphEmpty3()
        {
            String graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:Graph ;
  dnr:type ""VDS.RDF.Graph"" ;
  dnr:usingTripleCollection _:b .
_:b a dnr:TripleCollection ;
  dnr:type ""VDS.RDF.ThreadSafeTripleCollection"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            IGraph result = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as IGraph;
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(Graph), result.GetType());
            Assert.AreEqual(typeof(ThreadSafeTripleCollection), result.Triples.GetType());
        }

        [TestMethod]
        public void ConfigurationLoadObjectTripleStoreEmpty1()
        {
            String graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:TripleStore ;
  dnr:type ""VDS.RDF.TripleStore"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            ITripleStore result = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as ITripleStore;
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(TripleStore), result.GetType());
        }

        [TestMethod]
        public void ConfigurationLoadObjectTripleStoreEmpty2()
        {
            String graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:TripleStore ;
  dnr:type ""VDS.RDF.WebDemandTripleStore"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            ITripleStore result = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as ITripleStore;
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(WebDemandTripleStore), result.GetType());
        }

        [TestMethod]
        public void ConfigurationLoadObjectTripleStoreEmpty3()
        {
            String graph = ConfigLookupTests.Prefixes + @"
_:a a dnr:TripleStore ;
  dnr:type ""VDS.RDF.TripleStore"" ;
  dnr:usingGraphCollection _:b .
_:b a dnr:GraphCollection ;
  dnr:type ""VDS.RDF.ThreadSafeGraphCollection"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            ITripleStore result = ConfigurationLoader.LoadObject(g, g.GetBlankNode("a")) as ITripleStore;
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(TripleStore), result.GetType());
            Assert.AreEqual(typeof(ThreadSafeGraphCollection), result.Graphs.GetType());
        }
    }

    class MockPropertyFunctionFactory
        : IPropertyFunctionFactory
    {
        public bool IsPropertyFunction(Uri u)
        {
            throw new NotImplementedException();
        }

        public bool TryCreatePropertyFunction(PropertyFunctionInfo info, out RDF.Query.Patterns.IPropertyFunctionPattern function)
        {
            throw new NotImplementedException();
        }
    }
}
