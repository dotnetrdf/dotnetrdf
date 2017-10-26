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
using Xunit;

namespace VDS.RDF.Parsing.Suites
{

    public abstract class BaseRdfParserSuite : BaseParserSuite<IRdfReader, Graph>
    {
        protected BaseRdfParserSuite(IRdfReader testParser, IRdfReader resultsParser, String baseDir)
            : base(testParser, resultsParser, baseDir)
        {
            this.Parser.Warning += TestTools.WarningPrinter;
            this.ResultsParser.Warning += TestTools.WarningPrinter;
        }

        protected override Graph TryParseTestInput(string file)
        {
            Graph actual = new Graph();
            actual.BaseUri = new Uri(BaseUri, Path.GetFileName(file));
            this.Parser.Load(actual, file);
            return actual;
        }

        protected override void TryValidateResults(string testName, string resultFile, Graph actual)
        {
            Graph expected = new Graph();
            this.ResultsParser.Load(expected, resultFile);

            GraphDiffReport diff = expected.Difference(actual);
            if (diff.AreEqual)
            {
                Console.WriteLine("Parsed Graph matches Expected Graph (Test Passed)");
                this.Passed++;
            }
            else
            {
                Console.WriteLine("Parsed Graph did not match Expected Graph (Test Failed)");
                Console.Error.WriteLine("Test " + testName + " - Parsed Graph did not match Expected Graph");
                this.Failed++;
                TestTools.ShowDifferences(diff, "Expected (" + this.ResultsParser.ToString() + ")", "Actual (" + this.Parser.ToString() + ")");
            }
        }

        protected override string FileExtension
        {
            get { return ".nt"; }
        }
    }
}