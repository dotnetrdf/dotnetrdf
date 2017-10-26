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
using Xunit;

namespace VDS.RDF.Parsing.Suites
{

    public abstract class BaseDatasetParserSuite : BaseParserSuite<IStoreReader, TripleStore>
    {
        protected BaseDatasetParserSuite(IStoreReader testParser, IStoreReader resultsParser, String baseDir)
            : base(testParser, resultsParser, baseDir)
        {
        }

        protected override TripleStore TryParseTestInput(string file)
        {
            TripleStore actual = new TripleStore();
            this.Parser.Load(actual, file);
            return actual;
        }

        protected override void TryValidateResults(string testName, string resultFile, TripleStore actual)
        {
            TripleStore expected = new TripleStore();
            this.ResultsParser.Load(expected, resultFile);

            if (AreEqual(expected, actual))
            {
                Console.WriteLine("Parsed Dataset matches Expected Dataset (Test Passed)");
                this.Passed++;
            }
            else
            {
                Console.WriteLine("Parsed Dataset did not match Expected Dataset (Test Failed)");
                this.Failed++;
            }
        }

        protected override string FileExtension
        {
            get { return ".nt"; }
        }

        private static bool AreEqual(TripleStore expected, TripleStore actual)
        {
            if (expected.Graphs.Count != actual.Graphs.Count)
            {
                Console.WriteLine("Expected " + expected.Graphs.Count + " graphs but got " + actual.Graphs.Count);
                return false;
            }

            foreach (Uri u in expected.Graphs.GraphUris)
            {
                if (!actual.HasGraph(u))
                {
                    Console.WriteLine("Expected Graph {0} missing", u.ToSafeString());
                    return false;
                }
                GraphDiffReport diff = expected[u].Difference(actual[u]);
                if (!diff.AreEqual)
                {
                    Console.WriteLine("Expected Graph {0} not as expected", u.ToSafeString());
                    TestTools.ShowDifferences(diff);
                    return false;
                }
            }

            return true;
        }
    }
}