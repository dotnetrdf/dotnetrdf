using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Writing;

namespace VDS.RDF.Test.Writing
{
    [TestClass]
    public class GraphVizTests
    {
        [TestMethod]
        public void WritingGraphViz1()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseGraphViz))
            {
                Assert.Inconclusive("Test Config marks GraphViz as unavailable, test cannot be run");
            }

            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            GraphVizWriter writer = new GraphVizWriter();
            writer.Save(g, "WritingGraphViz1.dot");
        }

        [TestMethod]
        public void WritingGraphViz2()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseGraphViz))
            {
                Assert.Inconclusive("Test Config marks GraphViz as unavailable, test cannot be run");
            }

            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            GraphVizGenerator generator = new GraphVizGenerator("svg");
            generator.Generate(g, "WritingGraphViz2.svg", false);
        }

        [TestMethod]
        public void WritingGraphViz3()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseGraphViz))
            {
                Assert.Inconclusive("Test Config marks GraphViz as unavailable, test cannot be run");
            }

            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            GraphVizGenerator generator = new GraphVizGenerator("png");
            generator.Generate(g, "WritingGraphViz3.png", false);
        }
    }
}
