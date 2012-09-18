using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;

namespace VDS.RDF.Test.Configuration
{
    [TestClass]
    public class ConfigLookupTests
    {
        private const String Prefixes = @"@prefix rdf: <" + NamespaceMapper.RDF + @"> .
@prefix xsd: <"+ NamespaceMapper.XMLSCHEMA + @"> .
@prefix dnr: <http://www.dotnetrdf.org/configuration#> .";

        [TestMethod]
        public void ConfigurationLookupNode1()
        {
            String graph = Prefixes + @"
_:a dnr:type ""literal"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            INode value = ConfigurationLoader.GetConfigurationNode(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"));

            Assert.AreEqual(NodeType.Literal, value.NodeType);
            Assert.AreEqual("literal", ((ILiteralNode)value).Value);
        }

        [TestMethod]
        public void ConfigurationLookupNode2()
        {
            String graph = Prefixes + @"
_:a dnr:type <http://uri> .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            INode value = ConfigurationLoader.GetConfigurationNode(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"));

            Assert.AreEqual(NodeType.Uri, value.NodeType);
            Assert.IsTrue(EqualityHelper.AreUrisEqual(new Uri("http://uri"), ((IUriNode)value).Uri));
        }

        [TestMethod]
        public void ConfigurationLookupNode3()
        {
            String graph = Prefixes + @"
_:a dnr:type <appsetting:ConfigurationLookupNode3> .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            System.Configuration.ConfigurationManager.AppSettings["ConfigurationLookupNode3"] = "literal";

            INode value = ConfigurationLoader.GetConfigurationNode(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"));

            Assert.AreEqual(NodeType.Literal, value.NodeType);
            Assert.AreEqual("literal", ((ILiteralNode)value).Value);
        }

        [TestMethod]
        public void ConfigurationLookupNode4()
        {
            Graph g = new Graph();
            INode value = ConfigurationLoader.GetConfigurationNode(g, g.CreateBlankNode("a"), g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyType)));
            Assert.IsNull(value);
        }

        [TestMethod]
        public void ConfigurationLookupBoolean1()
        {
            String graph = Prefixes + @"
_:a dnr:type true .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            bool value = ConfigurationLoader.GetConfigurationBoolean(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), false);
            Assert.IsTrue(value);
        }

        [TestMethod]
        public void ConfigurationLookupBoolean2()
        {
            String graph = Prefixes + @"
_:a dnr:type false .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            bool value = ConfigurationLoader.GetConfigurationBoolean(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), true);
            Assert.IsFalse(value);
        }

        [TestMethod]
        public void ConfigurationLookupBoolean3()
        {
            String graph = Prefixes + @"
_:a dnr:type ""not a boolean"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            bool value = ConfigurationLoader.GetConfigurationBoolean(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), true);
            Assert.IsTrue(value);
        }

        [TestMethod]
        public void ConfigurationLookupBoolean4()
        {
            String graph = Prefixes + @"
_:a dnr:type ""not a boolean"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            bool value = ConfigurationLoader.GetConfigurationBoolean(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), false);
            Assert.IsFalse(value);
        }

        [TestMethod]
        public void ConfigurationLookupBoolean5()
        {
            String graph = Prefixes + @"
_:a dnr:type <appsetting:ConfigurationLookupBoolean5> .";

            System.Configuration.ConfigurationManager.AppSettings["ConfigurationLookupBoolean5"] = "true";

            Graph g = new Graph();
            g.LoadFromString(graph);

            bool value = ConfigurationLoader.GetConfigurationBoolean(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), false);
            Assert.IsTrue(value);
        }

        [TestMethod]
        public void ConfigurationLookupInt1()
        {
            String graph = Prefixes + @"
_:a dnr:type 123 .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            int value = ConfigurationLoader.GetConfigurationInt32(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), 0);
            Assert.AreEqual(123, value);
        }

        [TestMethod]
        public void ConfigurationLookupInt2()
        {
            String graph = Prefixes + @"
_:a dnr:type ""not an integer"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            int value = ConfigurationLoader.GetConfigurationInt32(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), 0);
            Assert.AreEqual(0, value);
        }

        [TestMethod]
        public void ConfigurationLookupInt3()
        {
            String graph = Prefixes + @"
_:a dnr:type <appsetting:ConfigurationLookupInt3> .";

            System.Configuration.ConfigurationManager.AppSettings["ConfigurationLookupInt3"] = "123";

            Graph g = new Graph();
            g.LoadFromString(graph);

            int value = ConfigurationLoader.GetConfigurationInt32(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), 0);
            Assert.AreEqual(123, value);
        }

        [TestMethod]
        public void ConfigurationLookupLong1()
        {
            String graph = Prefixes + @"
_:a dnr:type 123 .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            long value = ConfigurationLoader.GetConfigurationInt64(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), 0);
            Assert.AreEqual(123, value);
        }

        [TestMethod]
        public void ConfigurationLookupLong2()
        {
            String graph = Prefixes + @"
_:a dnr:type ""not an integer"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            long value = ConfigurationLoader.GetConfigurationInt64(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), 0);
            Assert.AreEqual(0, value);
        }

        [TestMethod]
        public void ConfigurationLookupLong3()
        {
            String graph = Prefixes + @"
_:a dnr:type <appsetting:ConfigurationLookupLong3> .";

            System.Configuration.ConfigurationManager.AppSettings["ConfigurationLookupLong3"] = "123";

            Graph g = new Graph();
            g.LoadFromString(graph);

            long value = ConfigurationLoader.GetConfigurationInt32(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), 0);
            Assert.AreEqual(123, value);
        }
    }
}
