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
using VDS.RDF.Query;

namespace VDS.RDF.Parsing.Suites;


public abstract class BaseResultsParserSuite : BaseParserSuite<ISparqlResultsReader, SparqlResultSet>
{
    protected BaseResultsParserSuite(ISparqlResultsReader testParser, ISparqlResultsReader resultsParser, String baseDir)
        : base(testParser, resultsParser, baseDir)
    {
        Parser.Warning += TestTools.WarningPrinter;
        ResultsParser.Warning += TestTools.WarningPrinter;
    }

    protected override SparqlResultSet TryParseTestInput(string file)
    {
        var actual = new SparqlResultSet();
        Parser.Load(actual, file);
        return actual;
    }

    protected override void TryValidateResults(string testName, string resultFile, SparqlResultSet actual)
    {
        var expected = new SparqlResultSet();
        ResultsParser.Load(expected, resultFile);

        if (expected.Equals(actual))
        {
            Console.WriteLine("Parsed Results matches Expected Results (Test Passed)");
            PassedTest(testName);
        }
        else
        {
            Console.WriteLine("Parsed Results did not match Expected Graph (Test Failed)");
            FailedTest(testName, "Parsed Results did not match Expected Graph ");
            Console.WriteLine("Expected:");
            TestTools.ShowResults(expected);
            Console.WriteLine();
            Console.WriteLine("Actual:");
            TestTools.ShowResults(actual);
        }
    }

    protected override string FileExtension => ".srx";
}