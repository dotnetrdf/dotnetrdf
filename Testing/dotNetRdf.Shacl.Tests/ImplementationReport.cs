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
using System.Linq;
using VDS.RDF.Shacl.Validation;
using VDS.RDF.Writing.Formatting;
using static VDS.RDF.Shacl.TestSuiteData;

namespace VDS.RDF.Shacl;

internal static class ImplementationReport
{
    internal static IGraph Generate()
    {
        var testSubject = GenerateTestSubjectGraph(DateTime.UtcNow);

        foreach (var test in CoreFullTests.Concat(SparqlTests).Select(testNames => testNames.Data).ToList())
        {
            ExtractTestData(test, out var testGraph, out var shouldFail, out var dataGraph, out var shapesGraph);

            bool conforms()
            {
                var report = new ShapesGraph(shapesGraph).Validate(dataGraph);

                var actual = report.Normalised;
                var expected = Report.Parse(testGraph).Normalised;

                RemoveUnnecessaryResultMessages(actual, expected);

                return expected.Equals(actual);
            }

            var success = false;
            if (shouldFail)
            {
                try
                {
                    conforms();
                }
                catch
                {
                    success = true;
                }
            }
            else
            {
                success = conforms();
            }

            if (success)
            {
                testSubject.Merge(GenerateAssertionGraph(test, "automatic"));
            }
            else
            {
                throw new Exception();
            }
        }

        // These test fail validation report graph equality chacking but otherwise pass.
        testSubject.Merge(GenerateAssertionGraph("core/path/path-complex-002", "semiAuto"));
        testSubject.Merge(GenerateAssertionGraph("core/property/nodeKind-001", "semiAuto"));

        // These tests contain the URI node <a:b> but otherwise pass.
        testSubject.Merge(GenerateAssertionGraph("core/node/minLength-001", "semiAuto"));
        testSubject.Merge(GenerateAssertionGraph("core/node/maxLength-001", "semiAuto"));

        return testSubject;
    }

    private static IGraph GenerateTestSubjectGraph(DateTime date)
    {
        var g = new Graph();
        g.LoadFromString($@"
@prefix doap: <http://usefulinc.com/ns/doap#> .
@prefix earl: <http://www.w3.org/ns/earl#> .
@prefix xsd: <http://www.w3.org/2001/XMLSchema#> .

<https://www.dotnetrdf.org/>
    a
        doap:Project,
        earl:Software,
        earl:TestSubject ;
    doap:developer <https://github.com/langsamu> ;
    doap:name ""dotNetRDF"" ;
    doap:date {new TurtleFormatter().Format(date.ToLiteral(g))} ;
.
");

        return g;
    }

    private static IGraph GenerateAssertionGraph(string test, string mode)
    {
        var g = new Graph();
        g.LoadFromString($@"
@prefix earl: <http://www.w3.org/ns/earl#> .

[
    a earl:Assertion ;
    earl:test <urn:x-shacl-test:/{test.Replace(".ttl", string.Empty)}> ;
    earl:subject <https://www.dotnetrdf.org/> ;
    earl:assertedBy <https://github.com/langsamu> ;
    earl:result [
        a earl:TestResult ;
        earl:mode earl:{mode} ;
        earl:outcome earl:passed ;
    ] ;
] .
");

        return g;
    }
}
