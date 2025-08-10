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
using VDS.RDF.Query.Operators;
using VDS.RDF.Query.Operators.DateTime;
using Xunit;

namespace VDS.RDF.Configuration;


public class AutoConfigTests
{
    [Fact]
    public void ConfigurationStaticOptionUri1()
    {
        var optionUri = new Uri("dotnetrdf-configure:VDS.RDF.Options#UsePLinqEvaluation");

        Assert.Equal("dotnetrdf-configure", optionUri.Scheme);
        Assert.Single(optionUri.Segments);
        Assert.Equal("VDS.RDF.Options", optionUri.Segments[0]);
        Assert.Equal("VDS.RDF.Options", optionUri.AbsolutePath);
        Assert.Equal("UsePLinqEvaluation", optionUri.Fragment.Substring(1));
    }

    [Fact]
    public void ConfigurationStaticOptionUri2()
    {
        var optionUri = new Uri("dotnetrdf-configure:VDS.RDF.Options,SomeAssembly#UsePLinqEvaluation");

        Assert.Equal("dotnetrdf-configure", optionUri.Scheme);
        Assert.Single(optionUri.Segments);
        Assert.Equal("VDS.RDF.Options,SomeAssembly", optionUri.Segments[0]);
        Assert.Equal("VDS.RDF.Options,SomeAssembly", optionUri.AbsolutePath);
        Assert.Equal("UsePLinqEvaluation", optionUri.Fragment.Substring(1));
    }

    private void ApplyStaticOptionsConfigure(Uri option, String value)
    {
        var g = new Graph();
        INode configure = g.CreateUriNode(UriFactory.Root.Create(ConfigurationLoader.PropertyConfigure));
        g.Assert(g.CreateUriNode(option), configure, g.CreateLiteralNode(value));
        ApplyStaticOptionsConfigure(g);
    }

    private void ApplyStaticOptionsConfigure(IGraph g, Uri option, INode value)
    {
        INode configure = g.CreateUriNode(UriFactory.Root.Create(ConfigurationLoader.PropertyConfigure));
        g.Assert(g.CreateUriNode(option), configure, value);
        ApplyStaticOptionsConfigure(g);
    }

    private void ApplyStaticOptionsConfigure(IGraph g)
    {
        ConfigurationLoader.AutoConfigureStaticOptions(g);
    }

    [Fact]
    public void ConfigurationStaticOptionsNoFragment()
    {
        var optionUri = new Uri("dotnetrdf-configure:VDS.RDF.Graph");

        Assert.Throws<DotNetRdfConfigurationException>(() => ApplyStaticOptionsConfigure(optionUri, ""));
    }

    [Fact]
    public void ConfigurationStaticOptionsBadClass()
    {
        var optionUri = new Uri("dotnetrdf-configure:VDS.RDF.NoSuchClass#Property");

        Assert.Throws<DotNetRdfConfigurationException>(() => ApplyStaticOptionsConfigure(optionUri, ""));
    }

    [Fact]
    public void ConfigurationStaticOptionsBadProperty()
    {
        var optionUri = new Uri("dotnetrdf-configure:VDS.RDF.Graph#NoSuchProperty");

        Assert.Throws<DotNetRdfConfigurationException>(() => ApplyStaticOptionsConfigure(optionUri, ""));
    }

    [Fact]
    public void ConfigurationStaticOptionsNonStaticProperty()
    {
        var optionUri = new Uri("dotnetrdf-configure:VDS.RDF.Graph#BaseUri");

        Assert.Throws<DotNetRdfConfigurationException>(() => ApplyStaticOptionsConfigure(optionUri, "http://example.org"));
    }

    [Fact]
    public void ConfigurationStaticOptionsEnumProperty()
    {
        var current = EqualityHelper.LiteralEqualityMode;
        try
        {
            Assert.Equal(current, EqualityHelper.LiteralEqualityMode);

            var optionUri = new Uri("dotnetrdf-configure:VDS.RDF.EqualityHelper#LiteralEqualityMode");
            ApplyStaticOptionsConfigure(optionUri, "Loose");

            Assert.Equal(LiteralEqualityMode.Loose, EqualityHelper.LiteralEqualityMode);
        }
        finally
        {
            EqualityHelper.LiteralEqualityMode = current;
        }
    }

    [Fact]
    public void ConfigurationStaticOptionsInt32Property()
    {
        MockConfigurationTarget.StaticIntOption = 0;
        var optionUri = new Uri($"dotnetrdf-configure:VDS.RDF.Configuration.MockConfigurationTarget,dotNetRDF.Test#StaticIntOption");
        var g = new Graph();
        ApplyStaticOptionsConfigure(g, optionUri, (99).ToLiteral(g));
        Assert.Equal(99, MockConfigurationTarget.StaticIntOption);
    }
    [Fact]
    public void ConfigurationAutoOperators1()
    {
        try
        {
            var data = @"@prefix dnr: <http://www.dotnetrdf.org/configuration#> .
_:a a dnr:SparqlOperator ;
dnr:type """ + typeof(MockSparqlOperator).AssemblyQualifiedName + @""" .";

            var g = new Graph();
            g.LoadFromString(data);
            ConfigurationLoader.AutoConfigure(g);

            SparqlOperators.TryGetOperator(SparqlOperatorType.Add, false, out var op, null);

            Assert.Equal(typeof(MockSparqlOperator), op.GetType());
            SparqlOperators.RemoveOperator(op);
        }
        finally
        {
            SparqlOperators.Reset();
        }
    }

    [Fact]
    public void ConfigurationAutoOperators2()
    {
        try
        {
            var data = @"@prefix dnr: <http://www.dotnetrdf.org/configuration#> .
_:a a dnr:SparqlOperator ;
dnr:type ""VDS.RDF.Query.Operators.DateTime.DateTimeAddition"" ;
dnr:enabled false .";

            var g = new Graph();
            g.LoadFromString(data);
            ConfigurationLoader.AutoConfigure(g);

            Assert.False(SparqlOperators.IsRegistered(new DateTimeAddition()));
        }
        finally
        {
            SparqlOperators.Reset();
        }
    }
}
