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
using Xunit;

namespace VDS.RDF.Configuration;

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
        var graph = Prefixes + @"
_:a dnr:type ""literal"" .";

        var g = new Graph();
        g.LoadFromString(graph);

        INode value = ConfigurationLoader.GetConfigurationNode(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"));

        Assert.Equal(NodeType.Literal, value.NodeType);
        Assert.Equal("literal", ((ILiteralNode)value).Value);
    }

    [Fact]
    public void ConfigurationLookupNode2()
    {
        var graph = Prefixes + @"
_:a dnr:type <http://uri> .";

        var g = new Graph();
        g.LoadFromString(graph);

        INode value = ConfigurationLoader.GetConfigurationNode(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"));

        Assert.Equal(NodeType.Uri, value.NodeType);
        Assert.True(EqualityHelper.AreUrisEqual(new Uri("http://uri"), ((IUriNode)value).Uri));
    }

    [Fact]
    public void ConfigurationLookupNode3()
    {
        var graph = Prefixes + @"
_:a dnr:type <appsetting:ConfigurationLookupNode3> .";

        var g = new Graph();
        g.LoadFromString(graph);

        _testSettings.SettSetting("ConfigurationLookupNode3", "literal");

        INode value = ConfigurationLoader.GetConfigurationNode(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"));

        Assert.Equal(NodeType.Literal, value.NodeType);
        Assert.Equal("literal", ((ILiteralNode)value).Value);
    }

    [Fact]
    public void ConfigurationLookupNode4()
    {
        var g = new Graph();
        INode value = ConfigurationLoader.GetConfigurationNode(g, g.CreateBlankNode("a"), g.CreateUriNode(UriFactory.Root.Create(ConfigurationLoader.PropertyType)));
        Assert.Null(value);
    }

    [Fact]
    public void ConfigurationLookupNode5()
    {
        var graph = Prefixes + @"
_:a dnr:other ""other"" ;
  dnr:type ""literal"" .";

        var g = new Graph();
        g.LoadFromString(graph);

        INode value = ConfigurationLoader.GetConfigurationNode(g, g.GetBlankNode("a"), new INode[] { g.CreateUriNode("dnr:missing"), g.CreateUriNode("dnr:other"), g.CreateUriNode("dnr:type") });

        Assert.Equal(NodeType.Literal, value.NodeType);
        Assert.Equal("other", ((ILiteralNode)value).Value);
    }

    [Fact]
    public void ConfigurationLookupNode6()
    {
        var graph = Prefixes + @"
_:a dnr:other <http://other> ;
  dnr:type <http://uri> .";

        var g = new Graph();
        g.LoadFromString(graph);

        INode value = ConfigurationLoader.GetConfigurationNode(g, g.GetBlankNode("a"), new INode[] { g.CreateUriNode("dnr:missing"), g.CreateUriNode("dnr:other"), g.CreateUriNode("dnr:type") });

        Assert.Equal(NodeType.Uri, value.NodeType);
        Assert.True(EqualityHelper.AreUrisEqual(new Uri("http://other"), ((IUriNode)value).Uri));
    }

    [Fact]
    public void ConfigurationLookupNode7()
    {
        var graph = Prefixes + @"
_:a dnr:other <http://other> ;
  dnr:type <http://uri> .";

        var g = new Graph();
        g.LoadFromString(graph);

        INode value = ConfigurationLoader.GetConfigurationNode(g, g.GetBlankNode("a"), new INode[] { g.CreateUriNode("dnr:missing"), g.CreateUriNode("dnr:type"), g.CreateUriNode("dnr:other") });

        Assert.Equal(NodeType.Uri, value.NodeType);
        Assert.True(EqualityHelper.AreUrisEqual(new Uri("http://uri"), ((IUriNode)value).Uri));
    }

    [Fact]
    public void ConfigurationLookupNode8()
    {
        var g = new Graph();
        g.NamespaceMap.AddNamespace("dnr", UriFactory.Root.Create(ConfigurationLoader.ConfigurationNamespace));
        INode value = ConfigurationLoader.GetConfigurationNode(g, g.CreateBlankNode("a"), new INode[] { g.CreateUriNode("dnr:missing"), g.CreateUriNode("dnr:type"), g.CreateUriNode("dnr:other") });
        Assert.Null(value);
    }

    [Fact]
    public void ConfigurationLookupNode9()
    {
        var g = new Graph();
        INode value = ConfigurationLoader.GetConfigurationNode(g, g.CreateBlankNode("a"), new INode[] { });
        Assert.Null(value);
    }

    [Fact]
    public void ConfigurationLookupBoolean1()
    {
        var graph = Prefixes + @"
_:a dnr:type true .";

        var g = new Graph();
        g.LoadFromString(graph);

        var value = ConfigurationLoader.GetConfigurationBoolean(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), false);
        Assert.True(value);
    }

    [Fact]
    public void ConfigurationLookupBoolean2()
    {
        var graph = Prefixes + @"
_:a dnr:type false .";

        var g = new Graph();
        g.LoadFromString(graph);

        var value = ConfigurationLoader.GetConfigurationBoolean(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), true);
        Assert.False(value);
    }

    [Fact]
    public void ConfigurationLookupBoolean3()
    {
        var graph = Prefixes + @"
_:a dnr:type ""not a boolean"" .";

        var g = new Graph();
        g.LoadFromString(graph);

        var value = ConfigurationLoader.GetConfigurationBoolean(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), true);
        Assert.True(value);
    }

    [Fact]
    public void ConfigurationLookupBoolean4()
    {
        var graph = Prefixes + @"
_:a dnr:type ""not a boolean"" .";

        var g = new Graph();
        g.LoadFromString(graph);

        var value = ConfigurationLoader.GetConfigurationBoolean(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), false);
        Assert.False(value);
    }

    [Fact]
    public void ConfigurationLookupBoolean5()
    {
        var graph = Prefixes + @"
_:a dnr:type <appsetting:ConfigurationLookupBoolean5> .";

        _testSettings.SettSetting("ConfigurationLookupBoolean5", "true");

        var g = new Graph();
        g.LoadFromString(graph);

        var value = ConfigurationLoader.GetConfigurationBoolean(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), false);
        Assert.True(value);
    }

    [Fact]
    public void ConfigurationLookupBoolean6()
    {
        var graph = Prefixes + @"
_:a dnr:other false ; dnr:type true .";

        var g = new Graph();
        g.LoadFromString(graph);

        var value = ConfigurationLoader.GetConfigurationBoolean(g, g.GetBlankNode("a"), new INode[] { g.CreateUriNode("dnr:other"), g.CreateUriNode("dnr:type")}, true);
        Assert.False(value);
    }

    [Fact]
    public void ConfigurationLookupBoolean7()
    {
        var graph = Prefixes + @"
_:a dnr:other false ; dnr:type true .";

        var g = new Graph();
        g.LoadFromString(graph);

        var value = ConfigurationLoader.GetConfigurationBoolean(g, g.GetBlankNode("a"), new INode[] { g.CreateUriNode("dnr:type"), g.CreateUriNode("dnr:other") }, false);
        Assert.True(value);
    }

    [Fact]
    public void ConfigurationLookupInt1()
    {
        var graph = Prefixes + @"
_:a dnr:type 123 .";

        var g = new Graph();
        g.LoadFromString(graph);

        var value = ConfigurationLoader.GetConfigurationInt32(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), 0);
        Assert.Equal(123, value);
    }

    [Fact]
    public void ConfigurationLookupInt2()
    {
        var graph = Prefixes + @"
_:a dnr:type ""not an integer"" .";

        var g = new Graph();
        g.LoadFromString(graph);

        var value = ConfigurationLoader.GetConfigurationInt32(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), 0);
        Assert.Equal(0, value);
    }

    [Fact]
    public void ConfigurationLookupInt3()
    {
        var graph = Prefixes + @"
_:a dnr:type <appsetting:ConfigurationLookupInt3> .";

        _testSettings.SettSetting("ConfigurationLookupInt3", "123");

        var g = new Graph();
        g.LoadFromString(graph);

        var value = ConfigurationLoader.GetConfigurationInt32(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), 0);
        Assert.Equal(123, value);
    }

    [Fact]
    public void ConfigurationLookupLong1()
    {
        var graph = Prefixes + @"
_:a dnr:type 123 .";

        var g = new Graph();
        g.LoadFromString(graph);

        var value = ConfigurationLoader.GetConfigurationInt64(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), 0);
        Assert.Equal(123, value);
    }

    [Fact]
    public void ConfigurationLookupLong2()
    {
        var graph = Prefixes + @"
_:a dnr:type ""not an integer"" .";

        var g = new Graph();
        g.LoadFromString(graph);

        var value = ConfigurationLoader.GetConfigurationInt64(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), 0);
        Assert.Equal(0, value);
    }

    [Fact]
    public void ConfigurationLookupLong3()
    {
        var graph = Prefixes + @"
_:a dnr:type <appsetting:ConfigurationLookupLong3> .";

        _testSettings.SettSetting("ConfigurationLookupLong3", "123");

        var g = new Graph();
        g.LoadFromString(graph);

        long value = ConfigurationLoader.GetConfigurationInt32(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"), 0);
        Assert.Equal(123, value);
    }

    [Fact]
    public void ConfigurationLookupString1()
    {
        var graph = Prefixes + @"
_:a dnr:type ""literal"" .";

        var g = new Graph();
        g.LoadFromString(graph);

        var value = ConfigurationLoader.GetConfigurationString(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"));

        Assert.Equal("literal", value);
    }

    [Fact]
    public void ConfigurationLookupString2()
    {
        var graph = Prefixes + @"
_:a dnr:type <http://uri> .";

        var g = new Graph();
        g.LoadFromString(graph);

        var value = ConfigurationLoader.GetConfigurationString(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"));

        Assert.Null(value);
    }

    [Fact]
    public void ConfigurationLookupString3()
    {
        var graph = Prefixes + @"
_:a dnr:type <appsetting:ConfigurationLookupString3> .";

        var g = new Graph();
        g.LoadFromString(graph);

        _testSettings.SettSetting("ConfigurationLookupString3", "literal");

        var value = ConfigurationLoader.GetConfigurationString(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"));

        Assert.Equal("literal", value);
    }

    [Fact]
    public void ConfigurationLookupString4()
    {
        var g = new Graph();
        var value = ConfigurationLoader.GetConfigurationString(g, g.CreateBlankNode("a"), g.CreateUriNode(UriFactory.Root.Create(ConfigurationLoader.PropertyType)));
        Assert.Null(value);
    }

    [Fact]
    public void ConfigurationLookupValue1()
    {
        var graph = Prefixes + @"
_:a dnr:type ""literal"" .";

        var g = new Graph();
        g.LoadFromString(graph);

        var value = ConfigurationLoader.GetConfigurationValue(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"));

        Assert.Equal("literal", value);
    }

    [Fact]
    public void ConfigurationLookupValue2()
    {
        var graph = Prefixes + @"
_:a dnr:type <http://uri> .";

        var g = new Graph();
        g.LoadFromString(graph);

        var value = ConfigurationLoader.GetConfigurationValue(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"));

        Assert.Equal("http://uri/", value);
    }

    [Fact]
    public void ConfigurationLookupValue3()
    {
        var graph = Prefixes + @"
_:a dnr:type <appsetting:ConfigurationLookupString3> .";

        var g = new Graph();
        g.LoadFromString(graph);

        _testSettings.SettSetting("ConfigurationLookupString3", "literal");

        var value = ConfigurationLoader.GetConfigurationValue(g, g.GetBlankNode("a"), g.CreateUriNode("dnr:type"));

        Assert.Equal("literal", value);
    }

    [Fact]
    public void ConfigurationLookupValue4()
    {
        var g = new Graph();
        var value = ConfigurationLoader.GetConfigurationValue(g, g.CreateBlankNode("a"), g.CreateUriNode(UriFactory.Root.Create(ConfigurationLoader.PropertyType)));
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
