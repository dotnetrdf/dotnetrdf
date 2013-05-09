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
using System.Linq;
using System.Text;
using NUnit.Framework;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;

namespace VDS.RDF.Configuration
{
    [TestFixture]
    public class ConfigLookupTests
    {
        public const String Prefixes = @"@prefix rdf: <" + NamespaceMapper.RDF + @"> .
@prefix xsd: <"+ NamespaceMapper.XMLSCHEMA + @"> .
@prefix dnr: <http://www.dotnetrdf.org/configuration#> .";

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
        public void ConfigurationLookupNode4()
        {
            Graph g = new Graph();
            INode value = ConfigurationLoader.GetConfigurationNode(g, g.CreateBlankNode("a"), g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyType)));
            Assert.IsNull(value);
        }

        [Test]
        public void ConfigurationLookupNode5()
        {
            String graph = Prefixes + @"
_:a dnr:other ""other"" ;
  dnr:type ""literal"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            INode value = ConfigurationLoader.GetConfigurationNode(g, g.GetBlankNode("a"), new INode[] { g.CreateUriNode("dnr:missing"), g.CreateUriNode("dnr:other"), g.CreateUriNode("dnr:type") });

            Assert.AreEqual(NodeType.Literal, value.NodeType);
            Assert.AreEqual("other", ((ILiteralNode)value).Value);
        }

        [Test]
        public void ConfigurationLookupNode6()
        {
            String graph = Prefixes + @"
_:a dnr:other <http://other> ;
  dnr:type <http://uri> .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            INode value = ConfigurationLoader.GetConfigurationNode(g, g.GetBlankNode("a"), new INode[] { g.CreateUriNode("dnr:missing"), g.CreateUriNode("dnr:other"), g.CreateUriNode("dnr:type") });

            Assert.AreEqual(NodeType.Uri, value.NodeType);
            Assert.IsTrue(EqualityHelper.AreUrisEqual(new Uri("http://other"), ((IUriNode)value).Uri));
        }

        [Test]
        public void ConfigurationLookupNode7()
        {
            String graph = Prefixes + @"
_:a dnr:other <http://other> ;
  dnr:type <http://uri> .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            INode value = ConfigurationLoader.GetConfigurationNode(g, g.GetBlankNode("a"), new INode[] { g.CreateUriNode("dnr:missing"), g.CreateUriNode("dnr:type"), g.CreateUriNode("dnr:other") });

            Assert.AreEqual(NodeType.Uri, value.NodeType);
            Assert.IsTrue(EqualityHelper.AreUrisEqual(new Uri("http://uri"), ((IUriNode)value).Uri));
        }

        [Test]
        public void ConfigurationLookupNode8()
        {
            Graph g = new Graph();
            g.NamespaceMap.AddNamespace("dnr", UriFactory.Create(ConfigurationLoader.ConfigurationNamespace));
            INode value = ConfigurationLoader.GetConfigurationNode(g, g.CreateBlankNode("a"), new INode[] { g.CreateUriNode("dnr:missing"), g.CreateUriNode("dnr:type"), g.CreateUriNode("dnr:other") });
            Assert.IsNull(value);
        }

        [Test]
        public void ConfigurationLookupNode9()
        {
            Graph g = new Graph();
            INode value = ConfigurationLoader.GetConfigurationNode(g, g.CreateBlankNode("a"), new INode[] { });
            Assert.IsNull(value);
        }

        [Test]
        public void ConfigurationLookupBoolean1()
        {
            String graph = Prefixes + @"
_:a dnr:type true .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            bool value = ConfigurationLoader.GetConfigurationBoolean(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), false);
            Assert.IsTrue(value);
        }

        [Test]
        public void ConfigurationLookupBoolean2()
        {
            String graph = Prefixes + @"
_:a dnr:type false .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            bool value = ConfigurationLoader.GetConfigurationBoolean(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), true);
            Assert.IsFalse(value);
        }

        [Test]
        public void ConfigurationLookupBoolean3()
        {
            String graph = Prefixes + @"
_:a dnr:type ""not a boolean"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            bool value = ConfigurationLoader.GetConfigurationBoolean(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), true);
            Assert.IsTrue(value);
        }

        [Test]
        public void ConfigurationLookupBoolean4()
        {
            String graph = Prefixes + @"
_:a dnr:type ""not a boolean"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            bool value = ConfigurationLoader.GetConfigurationBoolean(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), false);
            Assert.IsFalse(value);
        }

        [Test]
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

        [Test]
        public void ConfigurationLookupBoolean6()
        {
            String graph = Prefixes + @"
_:a dnr:other false ; dnr:type true .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            bool value = ConfigurationLoader.GetConfigurationBoolean(g, g.GetBlankNode("a"), new INode[] { g.CreateUriNode("dnr:other"), g.CreateUriNode("dnr:type")}, true);
            Assert.IsFalse(value);
        }

        [Test]
        public void ConfigurationLookupBoolean7()
        {
            String graph = Prefixes + @"
_:a dnr:other false ; dnr:type true .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            bool value = ConfigurationLoader.GetConfigurationBoolean(g, g.GetBlankNode("a"), new INode[] { g.CreateUriNode("dnr:type"), g.CreateUriNode("dnr:other") }, false);
            Assert.IsTrue(value);
        }

        [Test]
        public void ConfigurationLookupInt1()
        {
            String graph = Prefixes + @"
_:a dnr:type 123 .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            int value = ConfigurationLoader.GetConfigurationInt32(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), 0);
            Assert.AreEqual(123, value);
        }

        [Test]
        public void ConfigurationLookupInt2()
        {
            String graph = Prefixes + @"
_:a dnr:type ""not an integer"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            int value = ConfigurationLoader.GetConfigurationInt32(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), 0);
            Assert.AreEqual(0, value);
        }

        [Test]
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

        [Test]
        public void ConfigurationLookupLong1()
        {
            String graph = Prefixes + @"
_:a dnr:type 123 .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            long value = ConfigurationLoader.GetConfigurationInt64(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), 0);
            Assert.AreEqual(123, value);
        }

        [Test]
        public void ConfigurationLookupLong2()
        {
            String graph = Prefixes + @"
_:a dnr:type ""not an integer"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            long value = ConfigurationLoader.GetConfigurationInt64(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), 0);
            Assert.AreEqual(0, value);
        }

        [Test]
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

        [Test]
        public void ConfigurationLookupString1()
        {
            String graph = Prefixes + @"
_:a dnr:type ""literal"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            String value = ConfigurationLoader.GetConfigurationString(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"));

            Assert.AreEqual("literal", value);
        }

        [Test]
        public void ConfigurationLookupString2()
        {
            String graph = Prefixes + @"
_:a dnr:type <http://uri> .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            String value = ConfigurationLoader.GetConfigurationString(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"));

            Assert.IsNull(value);
        }

        [Test]
        public void ConfigurationLookupString3()
        {
            String graph = Prefixes + @"
_:a dnr:type <appsetting:ConfigurationLookupString3> .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            System.Configuration.ConfigurationManager.AppSettings["ConfigurationLookupString3"] = "literal";

            String value = ConfigurationLoader.GetConfigurationString(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"));

            Assert.AreEqual("literal", value);
        }

        [Test]
        public void ConfigurationLookupString4()
        {
            Graph g = new Graph();
            String value = ConfigurationLoader.GetConfigurationString(g, g.CreateBlankNode("a"), g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyType)));
            Assert.IsNull(value);
        }

        [Test]
        public void ConfigurationLookupValue1()
        {
            String graph = Prefixes + @"
_:a dnr:type ""literal"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            String value = ConfigurationLoader.GetConfigurationValue(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"));

            Assert.AreEqual("literal", value);
        }

        [Test]
        public void ConfigurationLookupValue2()
        {
            String graph = Prefixes + @"
_:a dnr:type <http://uri> .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            String value = ConfigurationLoader.GetConfigurationValue(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"));

            Assert.AreEqual("http://uri/", value);
        }

        [Test]
        public void ConfigurationLookupValue3()
        {
            String graph = Prefixes + @"
_:a dnr:type <appsetting:ConfigurationLookupString3> .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            System.Configuration.ConfigurationManager.AppSettings["ConfigurationLookupString3"] = "literal";

            String value = ConfigurationLoader.GetConfigurationValue(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"));

            Assert.AreEqual("literal", value);
        }

        [Test]
        public void ConfigurationLookupValue4()
        {
            Graph g = new Graph();
            String value = ConfigurationLoader.GetConfigurationValue(g, g.CreateBlankNode("a"), g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyType)));
            Assert.IsNull(value);
        }
    }
}
