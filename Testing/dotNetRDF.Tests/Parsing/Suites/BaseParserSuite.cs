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
using System.Linq;
using Xunit;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing.Suites
{
    public abstract class BaseParserSuite<TParser, TResult> where TParser : class
    {
        private readonly String _baseDir;
        private bool _check = true;
        private int _count;
        private int _pass;
        private int _fail;
        private int _indeterminate;
        public static Uri BaseUri = new Uri("http://www.w3.org/2001/sw/DataAccess/df1/tests/");
        private readonly TParser _parser;
        private readonly TParser _resultsParser;

        protected BaseParserSuite(TParser testParser, TParser resultsParser, string baseDir)
        {
            if (baseDir == null) throw new ArgumentNullException("baseDir");
            if (testParser == null) throw new ArgumentNullException("testParser");
            if (resultsParser == null) throw new ArgumentNullException("resultsParser");

            this._parser = testParser;
            this._resultsParser = resultsParser;
            this._baseDir = string.Format("resources\\{0}", baseDir);
        }

        /// <summary>
        /// Gets/Sets whether the suite needs to check result files for tests that should parse
        /// </summary>
        public bool CheckResults
        {
            get
            {
                return this._check;
            }
            set
            {
                this._check = value;
            }
        }

        public int Count
        {
            get
            {
                return this._count;
            }
        }

        public int Passed
        {
            get
            {
                return this._pass;
            }
            set { _pass = value; }
        }

        public int Failed
        {
            get
            {
                return this._fail;
            }
            protected set { this._fail = value; }
        }

        public int Indeterminate
        {
            get
            {
                return this._indeterminate;
            }
        }

        public BaseParserSuite()
        {
            this._count = 0;
            this._pass = 0;
            this._fail = 0;
            this._indeterminate = 0;
        }

        protected String GetFile(INode n)
        {
            switch (n.NodeType)
            {
                case NodeType.Uri:
                    Uri u = ((IUriNode)n).Uri;
                    if (u.IsFile)
                    {
                        return u.AbsolutePath;
                    }
                    else
                    {
                        String lastSegment = u.Segments[u.Segments.Length - 1];
                        return Path.Combine(this._baseDir, lastSegment);
                    }
                default:
                    Assert.True(false, "Malformed manifest file, input file must be a  URI");
                    break;
            }
            //Keep compiler happy
            return null;
        }

        protected void RunDirectory(String pattern, bool shouldParse)
        {
            this.RunDirectory(this._baseDir, pattern, shouldParse);
        }

        protected void RunDirectory(Func<String, bool> isTest, bool shouldParse)
        {
            this.RunDirectory(this._baseDir, isTest, shouldParse);
        }

        protected void RunDirectory(String dir, String pattern, bool shouldParse)
        {
            foreach (String file in Directory.GetFiles(dir, pattern))
            {
                this.RunTest(Path.GetFileName(file), null, file, Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file) + ".nt"), shouldParse);
            }
        }

        protected void RunAllDirectories(Func<String, bool> isTest, bool shouldParse)
        {
            this.RunAllDirectories(this._baseDir, isTest, shouldParse);
        }

        protected void RunAllDirectories(String dir, Func<String, bool> isTest, bool shouldParse)
        {
            foreach (String subdir in Directory.GetDirectories(dir))
            {
                this.RunDirectory(subdir, isTest, shouldParse);
                this.RunAllDirectories(subdir, isTest, shouldParse);
            }
        }

        protected void RunDirectory(String dir, Func<String, bool> isTest, bool shouldParse)
        {
            foreach (String file in Directory.GetFiles(dir).Where(isTest))
            {
                this.RunTest(Path.GetFileName(file), null, file, Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file) + FileExtension), shouldParse);
            }
        }

        protected void RunManifest(String file, bool shouldParse)
        {
            Assert.True(File.Exists(file), "Manifest file " + file + " not found");

            Graph manifest = new Graph();
            manifest.BaseUri = BaseUri;
            try
            {
                manifest.LoadFromFile(file);
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Bad Manifest", ex);
                Assert.True(false, "Failed to load Manifest " + file);
            }

            const string findTests = @"prefix rdf:    <http://www.w3.org/1999/02/22-rdf-syntax-ns#> 
prefix rdfs:	<http://www.w3.org/2000/01/rdf-schema#> 
prefix mf:     <http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#> 
prefix qt:     <http://www.w3.org/2001/sw/DataAccess/tests/test-query#> 
SELECT ?name ?input ?comment ?result
WHERE
{
  ?test mf:action [ qt:data ?input ] .
  OPTIONAL { ?test mf:name ?name }
  OPTIONAL { ?test rdfs:comment ?comment }
  OPTIONAL { ?test mf:result ?result }
}";

            SparqlResultSet tests = manifest.ExecuteQuery(findTests) as SparqlResultSet;
            Assert.NotNull(tests);

            foreach (SparqlResult test in tests)
            {
                INode nameNode, commentNode, resultNode;
                String name = test.TryGetBoundValue("name", out nameNode) ? nameNode.ToString() : null;
                INode inputNode = test["input"];
                String input = this.GetFile(inputNode);
                String comment = test.TryGetBoundValue("comment", out commentNode) ? commentNode.ToString() : null;
                String results = test.TryGetBoundValue("result", out resultNode) ? this.GetFile(resultNode) : null;

                this.RunTest(name, comment, input, results, shouldParse);
            }
        }/// <summary>
        /// Runs all tests found in the manifest, determines whether a test should pass/fail based on the test information
        /// </summary>
        /// <param name="file">Manifest file</param>
        protected void RunManifest(String file, INode positiveSyntaxTest, INode negativeSyntaxTest)
        {
            this.RunManifest(file, new INode[] { positiveSyntaxTest }, new INode[] { negativeSyntaxTest });
        }

        protected void RunManifest(String file, INode[] positiveSyntaxTests, INode[] negativeSyntaxTests)
        {
            Assert.True(File.Exists(file), "Manifest file " + file + " not found");
            

            Graph manifest = new Graph();
            manifest.BaseUri = BaseUri;
            try
            {
                manifest.LoadFromFile(file);
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Bad Manifest", ex);
                Assert.True(false, "Failed to load Manifest " + file);
            }
            manifest.NamespaceMap.AddNamespace("rdf", UriFactory.Create("http://www.w3.org/ns/rdftest#"));

            String findTests = @"prefix rdf:    <http://www.w3.org/1999/02/22-rdf-syntax-ns#> 
prefix rdfs:    <http://www.w3.org/2000/01/rdf-schema#> 
prefix mf:     <http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#> 
prefix qt:     <http://www.w3.org/2001/sw/DataAccess/tests/test-query#> 
prefix rdft:   <http://www.w3.org/ns/rdftest#>
SELECT ?name ?input ?comment ?result ?type
WHERE
{
  { ?test mf:action [ qt:data ?input ] . }
  UNION
  { ?test mf:action ?input . FILTER(!ISBLANK(?input)) }
  OPTIONAL { ?test a ?type }
  OPTIONAL { ?test mf:name ?name }
  OPTIONAL { ?test rdfs:comment ?comment }
  OPTIONAL { ?test mf:result ?result }
}";

            SparqlResultSet tests = manifest.ExecuteQuery(findTests) as SparqlResultSet;
            Assert.NotNull(tests);

            foreach (SparqlResult test in tests)
            {
                INode nameNode, inputNode, commentNode, resultNode;
                String name = test.TryGetBoundValue("name", out nameNode) ? nameNode.ToString() : null;
                inputNode = test["input"];
                String input = this.GetFile(inputNode);
                String comment = test.TryGetBoundValue("comment", out commentNode) ? commentNode.ToString() : null;
                String results = test.TryGetBoundValue("result", out resultNode) ? this.GetFile(resultNode) : null;

                //Determine expected outcome
                //Evaluation tests will have results and should always parse succesfully
                bool? shouldParse = results != null ? true : false;
                if (!shouldParse.Value)
                {
                    //No results declared so may be a positive/negative syntax test
                    //Inspect returned type to determine, if no type assume test should fail
                    INode type;
                    if (test.TryGetBoundValue("type", out type))
                    {
                        if (positiveSyntaxTests.Contains(type))
                        {
                            shouldParse = true;
                        }
                        else if (negativeSyntaxTests.Contains(type))
                        {
                            shouldParse = false;
                        }
                        else
                        {
                            //Unable to determine what the expected result is
                            shouldParse = null;
                        }
                    }
                }

                this.RunTest(name, comment, input, results, shouldParse);
            }
        }

        private void RunTest(String name, String comment, String file, String resultFile, bool? shouldParse)
        {
            Console.WriteLine("### Running Test #" + this._count);
            if (name != null) Console.WriteLine("Test Name " + name);
            if (comment != null) Console.WriteLine(comment);
            Console.WriteLine();
            Console.WriteLine("Input File is " + file);
            if (resultFile != null) Console.WriteLine("Expected Output File is " + resultFile);

            //Check File Exists
            if (!File.Exists(file))
            {
                Console.WriteLine("Input File not found");
                Console.Error.WriteLine("Test " + name + " - Input File not found: " + file);
                this._fail++;
                return;
            }

            try
            {
                var actual = TryParseTestInput(file);

                if (!shouldParse.HasValue)
                {
                    Console.WriteLine("Unable to determine whether the test should pass/fail based on manifest information (Test Indeterminate)");
                    this._indeterminate++;
                }
                else if (shouldParse.Value)
                {
                    Console.WriteLine("Parsed input in OK");

                    //Validate if necessary
                    if (this.CheckResults && resultFile != null)
                    {
                        if (!File.Exists(resultFile))
                        {
                            Console.WriteLine("Expected Output File not found");
                            Console.Error.WriteLine("Test " + name + " - Expected Output File not found: " + resultFile);
                            this._fail++;
                        }
                        else
                        {
                            try
                            {
                                TryValidateResults(name, resultFile, actual);
                            }
                            catch (RdfParseException)
                            {
                                Console.WriteLine("Expected Output File could not be parsed (Test Indeterminate)");
                                this._indeterminate++;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("No Validation Required (Test Passed)");
                        this._pass++;
                    }
                }
                else
                {
                    Console.WriteLine("Parsed when failure was expected (Test Failed)");
                    Console.Error.WriteLine("Test " + name + " - Parsed when failure was expected");
                    this._fail++;
                }
            }
            catch (RdfParseException parseEx)
            {
                if (shouldParse.HasValue && shouldParse.Value)
                {
                    Console.WriteLine("Parsing failed when success was expected (Test Failed)");
                    Console.Error.WriteLine("Test " + name + " - Failed to parse when success was expected");

                    //Repeat parsing with tracing enabled if appropriate
                    //This gives us more useful debugging output for failed tests
                    if (this._parser is ITraceableTokeniser)
                    {
                        try
                        {
                            ((ITraceableTokeniser)this._parser).TraceTokeniser = true;
                            ((IRdfReader)this._parser).Load(new Graph(), Path.GetFileName(file));
                        }
                        catch
                        {
                            //Ignore errors the 2nd time around, we've already got a copy of the error to report
                        }
                        finally
                        {
                            ((ITraceableTokeniser)this._parser).TraceTokeniser = false;
                        }
                    }

                    TestTools.ReportError("Parse Error", parseEx);
                    this._fail++;
                }
                else
                {
                    Console.WriteLine("Parsing Failed as expected (Test Passed)");
                    this._pass++;
                }
            }
            Console.WriteLine("### End Test #" + this._count);
            Console.WriteLine();
            this._count++;
        }

        protected abstract TResult TryParseTestInput(string file);

        protected abstract void TryValidateResults(string testName, string resultFile, TResult actual);

        protected abstract string FileExtension { get; }

        protected TParser Parser
        {
            get { return _parser; }
        }

        protected TParser ResultsParser
        {
            get { return _resultsParser; }
        }
    }
}
