/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using VDS.RDF.Shacl.Validation;
using VDS.RDF.Writing;
using Xunit;
using static VDS.RDF.Shacl.TestSuiteData;

namespace VDS.RDF.Shacl;

public class TestSuite
{
    private readonly ITestOutputHelper _output;

    public TestSuite(ITestOutputHelper output)
    {
        this._output = output;
    }

    [Theory]
    [MemberData(nameof(CoreTests), MemberType = typeof(TestSuiteData))]
    public void CorePartialCompliance(string name)
    {
        Conforms(name);
    }

    [Theory]
    [MemberData(nameof(CoreFullTests), MemberType = typeof(TestSuiteData))]
    public void CoreFullCompliance(string name)
    {
        Validates(name);
    }

    [Theory]
    [MemberData(nameof(SparqlTests), MemberType = typeof(TestSuiteData))]
    public void SparqlPartialCompliance(string name)
    {
        Conforms(name);
    }

    [Theory]
    [MemberData(nameof(SparqlTests), MemberType = typeof(TestSuiteData))]
    public void SparqlFullCompliance(string name)
    {
        Validates(name);
    }

    private static void Conforms(string name)
    {
        ExtractTestData(name, out IGraph testGraph, out var failure, out IGraph dataGraph, out IGraph shapesGraph);

        void conforms()
        {
            var actual = new ShapesGraph(shapesGraph).Conforms(dataGraph);
            var expected = Report.Parse(testGraph).Conforms;

            Assert.Equal(expected, actual);
        }

        if (failure)
        {
            Assert.ThrowsAny<Exception>((Action)conforms);
        }
        else
        {
            conforms();
        }
    }

    private void Validates(string name)
    {
        ExtractTestData(name, out IGraph testGraph, out var failure, out IGraph dataGraph, out IGraph shapesGraph);

        void validates()
        {
            IGraph actual = new ShapesGraph(shapesGraph).Validate(dataGraph).Normalised;
            IGraph expected = Report.Parse(testGraph).Normalised;

            var writer = new CompressingTurtleWriter();
            _output.WriteLine(StringWriter.Write(expected, writer));
            _output.WriteLine(StringWriter.Write(actual, writer));

            RemoveUnnecessaryResultMessages(actual, expected);

            Assert.Equal(expected, actual);
        }

        if (failure)
        {
            Assert.ThrowsAny<Exception>((Action)validates);
        }
        else
        {
            validates();
        }
    }
}
