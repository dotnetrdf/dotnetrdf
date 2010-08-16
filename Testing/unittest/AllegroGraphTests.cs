using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;

namespace VDS.RDF.Test
{
    [TestClass()]
    public class AllegroGraphTests
    {
        [TestMethod()]
        public void AllegroGraphSaveLoadTest()
        {
            try
            {
                Graph g = new Graph();
                FileLoader.Load(g, "InferenceTest.ttl");
                g.BaseUri = new Uri("http://example.org/AllegroGraphTest");

                AllegroGraphConnector agraph = new AllegroGraphConnector("http://localhost:9875", "test", "unit-test");
                agraph.SaveGraph(g);

                Graph h = new Graph();
                agraph.LoadGraph(h, "http://example.org/AllegroGraphTest");
                Assert.IsFalse(h.IsEmpty, "Graph should not be empty after loading");

                Assert.AreEqual(g, h, "Graphs should have been equal");
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Error", ex, true);
            }
        }
    }
}
