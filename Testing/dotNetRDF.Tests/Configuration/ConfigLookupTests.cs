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
using Xunit;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;

namespace VDS.RDF.Configuration
{
    public class ConfigLookupTests : IDisposable
    {
        private TestSettingsProvider _testSettings;

        public const String Prefixes = @"@prefix rdf: <" + NamespaceMapper.RDF + @"> .
@prefix xsd: <"+ NamespaceMapper.XMLSCHEMA + @"> .
@prefix dnr: <http://www.dotnetrdf.org/configuration#> .";

        public ConfigLookupTests()
        {
            _testSettings = new TestSettingsProvider();
            ConfigurationLoader.SettingsProvider = _testSettings;
        }

        [Fact]
        public void ConfigurationLookupNode1()
        {
            String graph = Prefixes + @"
_:a dnr:type ""literal"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            INode value = ConfigurationLoader.GetConfigurationNode(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"));

            Assert.Equal(NodeType.Literal, value.NodeType);
            Assert.Equal("literal", ((ILiteralNode)value).Value);
        }

        [Fact]
        public void ConfigurationLookupNode2()
        {
            String graph = Prefixes + @"
_:a dnr:type <http://uri> .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            INode value = ConfigurationLoader.GetConfigurationNode(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"));

            Assert.Equal(NodeType.Uri, value.NodeType);
            Assert.True(EqualityHelper.AreUrisEqual(new Uri("http://uri"), ((IUriNode)value).Uri));
        }

        [Fact]
        public void ConfigurationLookupNode3()
        {
            String graph = Prefixes + @"
_:a dnr:type <appsetting:ConfigurationLookupNode3> .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            _testSettings.SettSetting("ConfigurationLookupNode3", "literal");

            INode value = ConfigurationLoader.GetConfigurationNode(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"));

            Assert.Equal(NodeType.Literal, value.NodeType);
            Assert.Equal("literal", ((ILiteralNode)value).Value);
        }

        [Fact]
        public void ConfigurationLookupNode4()
        {
            Graph g = new Graph();
            INode value = ConfigurationLoader.GetConfigurationNode(g, g.CreateBlankNode("a"), g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyType)));
            Assert.Null(value);
        }

        [Fact]
        public void ConfigurationLookupNode5()
        {
            String graph = Prefixes + @"
_:a dnr:other ""other"" ;
  dnr:type ""literal"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            INode value = ConfigurationLoader.GetConfigurationNode(g, g.GetBlankNode("a"), new INode[] { g.CreateUriNode("dnr:missing"), g.CreateUriNode("dnr:other"), g.CreateUriNode("dnr:type") });

            Assert.Equal(NodeType.Literal, value.NodeType);
            Assert.Equal("other", ((ILiteralNode)value).Value);
        }

        [Fact]
        public void ConfigurationLookupNode6()
        {
            String graph = Prefixes + @"
_:a dnr:other <http://other> ;
  dnr:type <http://uri> .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            INode value = ConfigurationLoader.GetConfigurationNode(g, g.GetBlankNode("a"), new INode[] { g.CreateUriNode("dnr:missing"), g.CreateUriNode("dnr:other"), g.CreateUriNode("dnr:type") });

            Assert.Equal(NodeType.Uri, value.NodeType);
            Assert.True(EqualityHelper.AreUrisEqual(new Uri("http://other"), ((IUriNode)value).Uri));
        }

        [Fact]
        public void ConfigurationLookupNode7()
        {
            String graph = Prefixes + @"
_:a dnr:other <http://other> ;
  dnr:type <http://uri> .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            INode value = ConfigurationLoader.GetConfigurationNode(g, g.GetBlankNode("a"), new INode[] { g.CreateUriNode("dnr:missing"), g.CreateUriNode("dnr:type"), g.CreateUriNode("dnr:other") });

            Assert.Equal(NodeType.Uri, value.NodeType);
            Assert.True(EqualityHelper.AreUrisEqual(new Uri("http://uri"), ((IUriNode)value).Uri));
        }

        [Fact]
        public void ConfigurationLookupNode8()
        {
            Graph g = new Graph();
            g.NamespaceMap.AddNamespace("dnr", UriFactory.Create(ConfigurationLoader.ConfigurationNamespace));
            INode value = ConfigurationLoader.GetConfigurationNode(g, g.CreateBlankNode("a"), new INode[] { g.CreateUriNode("dnr:missing"), g.CreateUriNode("dnr:type"), g.CreateUriNode("dnr:other") });
            Assert.Null(value);
        }

        [Fact]
        public void ConfigurationLookupNode9()
        {
            Graph g = new Graph();
            INode value = ConfigurationLoader.GetConfigurationNode(g, g.CreateBlankNode("a"), new INode[] { });
            Assert.Null(value);
        }

        [Fact]
        public void ConfigurationLookupBoolean1()
        {
            String graph = Prefixes + @"
_:a dnr:type true .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            bool value = ConfigurationLoader.GetConfigurationBoolean(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), false);
            Assert.True(value);
        }

        [Fact]
        public void ConfigurationLookupBoolean2()
        {
            String graph = Prefixes + @"
_:a dnr:type false .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            bool value = ConfigurationLoader.GetConfigurationBoolean(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), true);
            Assert.False(value);
        }

        [Fact]
        public void ConfigurationLookupBoolean3()
        {
            String graph = Prefixes + @"
_:a dnr:type ""not a boolean"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            bool value = ConfigurationLoader.GetConfigurationBoolean(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), true);
            Assert.True(value);
        }

        [Fact]
        public void ConfigurationLookupBoolean4()
        {
            String graph = Prefixes + @"
_:a dnr:type ""not a boolean"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            bool value = ConfigurationLoader.GetConfigurationBoolean(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), false);
            Assert.False(value);
        }

        [Fact]
        public void ConfigurationLookupBoolean5()
        {
            String graph = Prefixes + @"
_:a dnr:type <appsetting:ConfigurationLookupBoolean5> .";

            _testSettings.SettSetting("ConfigurationLookupBoolean5", "true");

            Graph g = new Graph();
            g.LoadFromString(graph);

            bool value = ConfigurationLoader.GetConfigurationBoolean(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), false);
            Assert.True(value);
        }

        [Fact]
        public void ConfigurationLookupBoolean6()
        {
            String graph = Prefixes + @"
_:a dnr:other false ; dnr:type true .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            bool value = ConfigurationLoader.GetConfigurationBoolean(g, g.GetBlankNode("a"), new INode[] { g.CreateUriNode("dnr:other"), g.CreateUriNode("dnr:type")}, true);
            Assert.False(value);
        }

        [Fact]
        public void ConfigurationLookupBoolean7()
        {
            String graph = Prefixes + @"
_:a dnr:other false ; dnr:type true .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            bool value = ConfigurationLoader.GetConfigurationBoolean(g, g.GetBlankNode("a"), new INode[] { g.CreateUriNode("dnr:type"), g.CreateUriNode("dnr:other") }, false);
            Assert.True(value);
        }

        [Fact]
        public void ConfigurationLookupInt1()
        {
            String graph = Prefixes + @"
_:a dnr:type 123 .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            int value = ConfigurationLoader.GetConfigurationInt32(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), 0);
            Assert.Equal(123, value);
        }

        [Fact]
        public void ConfigurationLookupInt2()
        {
            String graph = Prefixes + @"
_:a dnr:type ""not an integer"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            int value = ConfigurationLoader.GetConfigurationInt32(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), 0);
            Assert.Equal(0, value);
        }

        [Fact]
        public void ConfigurationLookupInt3()
        {
            String graph = Prefixes + @"
_:a dnr:type <appsetting:ConfigurationLookupInt3> .";

            _testSettings.SettSetting("ConfigurationLookupInt3", "123");

            Graph g = new Graph();
            g.LoadFromString(graph);

            int value = ConfigurationLoader.GetConfigurationInt32(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), 0);
            Assert.Equal(123, value);
        }

        [Fact]
        public void ConfigurationLookupLong1()
        {
            String graph = Prefixes + @"
_:a dnr:type 123 .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            long value = ConfigurationLoader.GetConfigurationInt64(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), 0);
            Assert.Equal(123, value);
        }

        [Fact]
        public void ConfigurationLookupLong2()
        {
            String graph = Prefixes + @"
_:a dnr:type ""not an integer"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            long value = ConfigurationLoader.GetConfigurationInt64(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), 0);
            Assert.Equal(0, value);
        }

        [Fact]
        public void ConfigurationLookupLong3()
        {
            String graph = Prefixes + @"
_:a dnr:type <appsetting:ConfigurationLookupLong3> .";

            _testSettings.SettSetting("ConfigurationLookupLong3", "123");

            Graph g = new Graph();
            g.LoadFromString(graph);

            long value = ConfigurationLoader.GetConfigurationInt32(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), 0);
            Assert.Equal(123, value);
        }

        [Fact]
        public void ConfigurationLookupString1()
        {
            String graph = Prefixes + @"
_:a dnr:type ""literal"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            String value = ConfigurationLoader.GetConfigurationString(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"));

            Assert.Equal("literal", value);
        }

        [Fact]
        public void ConfigurationLookupString2()
        {
            String graph = Prefixes + @"
_:a dnr:type <http://uri> .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            String value = ConfigurationLoader.GetConfigurationString(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"));

            Assert.Null(value);
        }

        [Fact]
        public void ConfigurationLookupString3()
        {
            String graph = Prefixes + @"
_:a dnr:type <appsetting:ConfigurationLookupString3> .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            _testSettings.SettSetting("ConfigurationLookupString3", "literal");

            String value = ConfigurationLoader.GetConfigurationString(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"));

            Assert.Equal("literal", value);
        }

        [Fact]
        public void ConfigurationLookupString4()
        {
            Graph g = new Graph();
            String value = ConfigurationLoader.GetConfigurationString(g, g.CreateBlankNode("a"), g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyType)));
            Assert.Null(value);
        }

        [Fact]
        public void ConfigurationLookupValue1()
        {
            String graph = Prefixes + @"
_:a dnr:type ""literal"" .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            String value = ConfigurationLoader.GetConfigurationValue(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"));

            Assert.Equal("literal", value);
        }

        [Fact]
        public void ConfigurationLookupValue2()
        {
            String graph = Prefixes + @"
_:a dnr:type <http://uri> .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            String value = ConfigurationLoader.GetConfigurationValue(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"));

            Assert.Equal("http://uri/", value);
        }

        [Fact]
        public void ConfigurationLookupValue3()
        {
            String graph = Prefixes + @"
_:a dnr:type <appsetting:ConfigurationLookupString3> .";

            Graph g = new Graph();
            g.LoadFromString(graph);

            _testSettings.SettSetting("ConfigurationLookupString3", "literal");

            String value = ConfigurationLoader.GetConfigurationValue(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"));

            Assert.Equal("literal", value);
        }

        [Fact]
        public void ConfigurationLookupValue4()
        {
            Graph g = new Graph();
            String value = ConfigurationLoader.GetConfigurationValue(g, g.CreateBlankNode("a"), g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyType)));
            Assert.Null(value);
        }

        private class TestSettingsProvider : ISettingsProvider
        {
            private readonly IDictionary<string, string> _dict = new Dictionary<string, string>();

            public string GetSetting(string key)
            {
                if (_dict.ContainsKey(key) == false)
                {
                    return null;
                }

                return _dict[key];
            }

            public void SettSetting(string key, string value)
            {
                _dict[key] = value;
            }
        }

        public void Dispose()
        {
            // revert the default value to not interfere with other tests
#if NET40
            ConfigurationLoader.SettingsProvider = new ConfigurationManagerSettingsProvider();
#else
            ConfigurationLoader.SettingsProvider = null;
#endif
        }
    }
}
