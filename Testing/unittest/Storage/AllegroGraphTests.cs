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
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage;


namespace VDS.RDF.Test.Storage
{
    [TestClass]
    public class AllegroGraphTests
    {
        public static AllegroGraphConnector GetConnection()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseAllegroGraph))
            {
                Assert.Inconclusive("Test Config marks AllegroGraph as unavailable, cannot run this test");
            }

            return new AllegroGraphConnector(TestConfigManager.GetSetting(TestConfigManager.AllegroGraphServer), TestConfigManager.GetSetting(TestConfigManager.AllegroGraphCatalog), TestConfigManager.GetSetting(TestConfigManager.AllegroGraphRepository));
        }

        [TestMethod]
        public void StorageAllegroGraphSaveLoad()
        {
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = new Uri("http://example.org/AllegroGraphTest");

            AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();
            agraph.SaveGraph(g);

            Graph h = new Graph();
            agraph.LoadGraph(h, "http://example.org/AllegroGraphTest");
            Assert.IsFalse(h.IsEmpty, "Graph should not be empty after loading");

            Assert.AreEqual(g, h, "Graphs should have been equal");
        }

        [TestMethod]
        public void StorageAllegroGraphDeleteTriples()
        {
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = new Uri("http://example.org/AllegroGraphTest");

            AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();
            agraph.SaveGraph(g);

            Console.WriteLine("Graph before deletion");
            TestTools.ShowGraph(g);

            //Delete all Triples about the Ford Fiesta
            agraph.UpdateGraph(g.BaseUri, null, g.GetTriplesWithSubject(new Uri("http://example.org/vehicles/FordFiesta")));

            Graph h = new Graph();
            agraph.LoadGraph(h, g.BaseUri);

            Console.WriteLine("Graph after deletion");
            TestTools.ShowGraph(h);

            Assert.IsFalse(h.IsEmpty, "Graph should not be completely empty");
            Assert.IsTrue(g.HasSubGraph(h), "Graph retrieved with missing Triples should be a sub-graph of the original Graph");
            Assert.IsFalse(g.Equals(h), "Graph retrieved should not be equal to original Graph");

            Object results = agraph.Query("ASK WHERE { GRAPH <http://example.org/AllegroGraphTest> { <http://example.org/vehicles/FordFiesta> ?p ?o } }");
            if (results is SparqlResultSet)
            {
                Assert.IsFalse(((SparqlResultSet)results).Result, "There should no longer be any triples about the Ford Fiesta present");
            }
        }

        [TestMethod]
        public void StorageAllegroGraphAsk()
        {
            AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();

            String ask = "ASK WHERE { ?s ?p ?o }";

            Object results = agraph.Query(ask);
            if (results is SparqlResultSet)
            {
                TestTools.ShowResults(results);
            }
            else
            {
                Assert.Fail("Failed to get a Result Set as expected");
            }
        }

        [TestMethod]
        public void StorageAllegroGraphDescribe()
        {
            AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();

            String describe = "DESCRIBE <http://example.org/Vehicles/FordFiesta>";

            Object results = agraph.Query(describe);
            if (results is IGraph)
            {
                TestTools.ShowGraph((IGraph)results);
            }
            else
            {
                Assert.Fail("Failed to get a Graph as expected");
            }
        }
    }
}
