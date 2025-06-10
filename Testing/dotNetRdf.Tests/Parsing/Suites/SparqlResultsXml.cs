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
using System.IO;
using VDS.RDF.Query;
using Xunit;

namespace VDS.RDF.Parsing.Suites;



public class SparqlResultsXml
    : BaseResultsParserSuite
{
    private readonly ITestOutputHelper _testOutputHelper;

    public SparqlResultsXml(ITestOutputHelper testOutputHelper)
        : base(new SparqlXmlParser(), new SparqlXmlParser(), "srx")
    {
        _testOutputHelper = testOutputHelper;
        CheckResults = false;
    }

    [Fact]
    public void ParsingSuiteSparqlResultsXml()
    {
        //Run manifests
        RunDirectory(f => Path.GetExtension(f).Equals(".srx") && !f.Contains("bad"), true);
        RunDirectory(f => Path.GetExtension(f).Equals(".srx") && f.Contains("bad"), false);

        if (Count == 0) Assert.Fail("No tests found");

        _testOutputHelper.WriteLine(Count + " Tests - " + Passed + " Passed - " + Failed + " Failed");
        _testOutputHelper.WriteLine(((Passed / (double)Count) * 100) + "% Passed");

        if (Failed > 0) Assert.Fail(Failed + " Tests failed");
        Assert.SkipWhen(Indeterminate > 0, Indeterminate + " Tests are indeterminate");
    }

    [Fact]
    public void ParsingSparqlResultsXmlCustomAttributes()
    {
        // Test case based off of CORE-410
        var results = new SparqlResultSet();
        ResultsParser.Load(results, Path.Combine("resources", "sparql", "core-410.srx"));

        TestTools.ShowResults(results);

        INode first = results[0]["test"];
        INode second = results[1]["test"];
        INode third = results[2]["test"];

        Assert.Equal(NodeType.Literal, first.NodeType);
        var firstLit = (ILiteralNode) first;
        Assert.NotNull(firstLit.DataType);
        Assert.Equal(XmlSpecsHelper.XmlSchemaDataTypeInteger, firstLit.DataType.AbsoluteUri);
        Assert.Equal("1993", firstLit.Value);

        Assert.Equal(NodeType.Literal, second.NodeType);
        var secondLit = (ILiteralNode) second;
        Assert.NotEqual(String.Empty, secondLit.Language);
        Assert.NotNull(secondLit.DataType);
        Assert.Equal("en", secondLit.Language);
        Assert.Equal("test", secondLit.Value);
        Assert.Equal(RdfSpecsHelper.RdfLangString, secondLit.DataType.AbsoluteUri);

        Assert.Equal(NodeType.Literal, third.NodeType);
        var thirdLit = (ILiteralNode) third;
        Assert.Equal(String.Empty, thirdLit.Language);
        Assert.NotNull(thirdLit.DataType);
        Assert.Equal(XmlSpecsHelper.XmlSchemaDataTypeString, thirdLit.DataType.AbsoluteUri);
        Assert.Equal("test plain literal", thirdLit.Value);
    }

    [Fact]
    public void ParsingSparqlResultsXmlConflictingAttributes()
    {
        // Test case based off of CORE-410
        var results = new SparqlResultSet();

        Assert.Throws<RdfParseException>(() => ResultsParser.Load(results, Path.Combine("resources", "sparql", "bad-core-410.srx")));
    }
}
