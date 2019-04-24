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

using System.IO;
using VDS.RDF.XunitExtensions;
using Xunit;
using Xunit.Abstractions;

namespace VDS.RDF.Parsing.Suites
{

    public class RdfXmlDomParser
        : BaseRdfParserSuite
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public RdfXmlDomParser(ITestOutputHelper testOutputHelper)
            : base(new RdfXmlParser(RdfXmlParserMode.DOM), new NTriplesParser(), "rdfxml\\")
        {
            _testOutputHelper = testOutputHelper;
            CheckResults = false;
        }

        [Fact]
        public void ParsingSuiteRdfXmlDOM()
        {
            //Run manifests
            RunAllDirectories(f => Path.GetExtension(f).Equals(".rdf") && !f.Contains("error"), true);
            RunAllDirectories(f => Path.GetExtension(f).Equals(".rdf") && f.Contains("error"), false);

            if (Count == 0) Assert.True(false, "No tests found");

            _testOutputHelper.WriteLine(Count + " Tests - " + Passed + " Passed - " + Failed + " Failed");
            _testOutputHelper.WriteLine(((Passed / (double)Count) * 100) + "% Passed");

            if (Failed > 0) Assert.True(false, Failed + " Tests failed");
            if (Indeterminate > 0) throw new SkipTestException(Indeterminate + " Tests are indeterminate");
        }

        [Fact]
        public void ParsingRdfXmlIDsDOM()
        {
            IGraph g = new Graph();
            g.BaseUri = BaseUri;
            Parser.Load(g, "resources\\rdfxml\\xmlbase\\test014.rdf");
        }
    }

    public class RdfXmlStreamingParser
        : BaseRdfParserSuite
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public RdfXmlStreamingParser(ITestOutputHelper testOutputHelper)
            : base(new RdfXmlParser(RdfXmlParserMode.Streaming), new NTriplesParser(), "rdfxml\\")
        {
            _testOutputHelper = testOutputHelper;
            CheckResults = false;
        }

        [Fact]
        public void ParsingSuiteRdfXmlStreaming()
        {
            //Run manifests
            RunAllDirectories(f => Path.GetExtension(f).Equals(".rdf") && !f.Contains("error"), true);
            RunAllDirectories(f => Path.GetExtension(f).Equals(".rdf") && f.Contains("error"), false);
            if (Count == 0) Assert.True(false, "No tests found");

            _testOutputHelper.WriteLine(Count + " Tests - " + Passed + " Passed - " + Failed + " Failed");
            _testOutputHelper.WriteLine(((Passed / (double)Count) * 100) + "% Passed");

            if (Failed > 0) Assert.True(false, Failed + " Tests failed");
            if (Indeterminate > 0) throw new SkipTestException(Indeterminate + " Tests are indeterminate");
        }

        [Fact]
        public void ParsingRdfXmlIDsStreaming()
        {
            IGraph g = new Graph();
            g.BaseUri = BaseUri;
            Parser.Load(g, "resources\\rdfxml\\xmlbase\\test014.rdf");
        }
    }
}
