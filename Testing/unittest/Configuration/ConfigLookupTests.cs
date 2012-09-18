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
    }
}
