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


namespace VDS.RDF.Shacl
{
    using System.Linq;
    using VDS.RDF.Nodes;
    using VDS.RDF.Parsing;
    using VDS.RDF.Writing;
    using Xunit;
    using Xunit.Abstractions;

    public class ShaclTestSuite
    {
        private readonly ITestOutputHelper output;

        public ShaclTestSuite(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [MemberData(nameof(ShaclTestSuiteData.CoreTestNames), MemberType = typeof(ShaclTestSuiteData))]
        public void CorePartialCompliance(string name)
        {
            Conforms(name);
        }

        [Theory]
        [MemberData(nameof(ShaclTestSuiteData.CoreTestNames), MemberType = typeof(ShaclTestSuiteData))]
        public void CoreFullCompliance(string name)
        {
            if (name == "core/path/path-complex-002.ttl")
            {
                Assert.False(true, "See https://github.com/dotnetrdf/dotnetrdf/issues/235");
            }

            Validates(name);
        }

        [Theory]
        [MemberData(nameof(ShaclTestSuiteData.SparqlTestNames), MemberType = typeof(ShaclTestSuiteData))]
        public void SparqlPartialCompliance(string name)
        {
            Conforms(name);
        }

        [Theory]
        [MemberData(nameof(ShaclTestSuiteData.SparqlTestNames), MemberType = typeof(ShaclTestSuiteData))]
        public void SparqlFullCompliance(string name)
        {
            Validates(name);
        }

        private static void Conforms(string name)
        {
            ShaclTestSuiteData.ExtractTestData(name, out var testGraph, out var failure, out var dataGraph, out var shapesGraph);

            try
            {
                var actual = new ShaclShapesGraph(shapesGraph).Validate(dataGraph);
                var expected = testGraph.GetTriplesWithPredicate(Shacl.Conforms).Single().Object.AsValuedNode().AsBoolean();

                Assert.Equal(expected, actual);
            }
            catch
            {
                Assert.True(failure);
            }
        }

        private static void RemoveUnnecessaryResultMessages(IGraph resultReport, IGraph testReport)
        {
            foreach (var t in resultReport.GetTriplesWithPredicate(Shacl.ResultMessage).ToList())
            {
                if (!testReport.GetTriplesWithPredicateObject(Shacl.ResultMessage, t.Object).Any())
                {
                    resultReport.Retract(t);
                }
            }
        }

        private static IGraph ExtractReportGraph(IGraph g)
        {
            var q = new SparqlQueryParser().ParseFromString(@"
PREFIX sh: <http://www.w3.org/ns/shacl#> 

DESCRIBE ?s
WHERE {
    ?s a sh:ValidationReport .
}
");
            q.Describer = new ShaclValidationReportDescribe();

            return (IGraph)g.ExecuteQuery(q);
        }

        private void Validates(string name)
        {
            ShaclTestSuiteData.ExtractTestData(name, out var testGraph, out var failure, out var dataGraph, out var shapesGraph);

            try
            {
                new ShaclShapesGraph(shapesGraph).Validate(dataGraph, out var report);

                var actual = ExtractReportGraph(report.Graph);
                var expected = ExtractReportGraph(testGraph);

                RemoveUnnecessaryResultMessages(actual, expected);

                var writer = new CompressingTurtleWriter();
                output.WriteLine(Writing.StringWriter.Write(expected, writer));
                output.WriteLine(Writing.StringWriter.Write(actual, writer));

                Assert.Equal(expected, actual);
            }
            catch
            {
                Assert.True(failure);
            }
        }
    }
}
