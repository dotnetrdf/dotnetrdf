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
using NUnit.Framework;
using VDS.RDF.Graphs;
using VDS.RDF.Namespaces;
using VDS.RDF.Nodes;
using VDS.RDF.Specifications;

namespace VDS.RDF.Parsing.Suites
{
    public abstract class BaseParserSuite<TParser, TResult> where TParser : class
    {
        private readonly String _baseDir;
        private bool _check = true;
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

        public int Count { get; private set; }

        public int Passed { get; set; }

        public int Failed { get; protected set; }

        public int Indeterminate { get; private set; }

        [SetUp]
        public void Setup()
        {
            this.Count = 0;
            this.Passed = 0;
            this.Failed = 0;
            this.Indeterminate = 0;
        }

        protected String GetFile(INode n)
        {
            switch (n.NodeType)
            {
                case NodeType.Uri:
                    Uri u = n.Uri;
                    if (u.IsAbsoluteUri && u.IsFile) return u.AbsolutePath;
                    if (u.IsAbsoluteUri)
                    {
                        String lastSegment = u.Segments[u.Segments.Length - 1];
                        return Path.Combine(this._baseDir, lastSegment);
                    }
                    return Path.Combine(this._baseDir, u.ToString());
                default:
                    Assert.Fail("Malformed manifest file, input file must be a  URI");
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
// ReSharper disable once AssignNullToNotNullAttribute
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
// ReSharper disable once AssignNullToNotNullAttribute
                this.RunTest(Path.GetFileName(file), null, file, Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file) + FileExtension), shouldParse);
            }
        }

        protected void RunManifest(String file, bool shouldParse)
        {
            if (!File.Exists(file))
            {
                Assert.Fail("Manifest file " + file + " not found");
            }

            IGraph manifest = new Graph();
            try
            {
                manifest.LoadFromFile(file);
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Bad Manifest", ex);
                Assert.Fail("Failed to load Manifest " + file);
            }

            manifest.Namespaces.AddNamespace("rdfs", new Uri("http://www.w3.org/2000/01/rdf-schema#"));
            manifest.Namespaces.AddNamespace("mf", new Uri("http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#"));
            manifest.Namespaces.AddNamespace("qt", new Uri("http://www.w3.org/2001/sw/DataAccess/tests/test-query#"));

            foreach (Triple testTriple in manifest.GetTriplesWithPredicate(manifest.CreateUriNode("mf:action")))
            {
                Triple nameTriple = manifest.GetTriplesWithSubjectPredicate(testTriple.Subject, manifest.CreateUriNode("mf:name")).FirstOrDefault();
                String name = nameTriple != null && nameTriple.Object.NodeType == NodeType.Literal ? nameTriple.Object.Value : null;

                Triple inputTriple = manifest.GetTriplesWithSubjectPredicate(testTriple.Object, manifest.CreateUriNode("qt:data")).FirstOrDefault();
                if (inputTriple == null) continue;
                String input = this.GetFile(inputTriple.Object);

                Triple commentTriple = manifest.GetTriplesWithSubjectPredicate(testTriple.Subject, manifest.CreateUriNode("rdfs:comment")).FirstOrDefault();
                String comment = commentTriple != null && commentTriple.Object.NodeType == NodeType.Literal ? commentTriple.Object.Value : null;

                Triple resultsTriple = manifest.GetTriplesWithSubjectPredicate(testTriple.Subject, manifest.CreateUriNode("mf:result")).FirstOrDefault();
                String results = resultsTriple != null && resultsTriple.Object.NodeType == NodeType.Literal ? this.GetFile(resultsTriple.Object) : null;

                this.RunTest(name, comment, input, results, shouldParse);
            }
        }

        /// <summary>
        /// Runs all tests found in the manifest, determines whether a test should pass/fail based on the test information
        /// </summary>
        /// <param name="file">Manifest file</param>
        /// <param name="positiveSyntaxTest">Type node that represents positive syntax tests</param>
        /// <param name="negativeSyntaxTest">Type node that represents negative syntax tests</param>
        protected void RunManifest(String file, INode positiveSyntaxTest, INode negativeSyntaxTest)
        {
            this.RunManifest(file, new INode[] { positiveSyntaxTest }, new INode[] { negativeSyntaxTest });
        }

        /// <summary>
        /// Runs all tests found in the manifest, determines whether a test should pass/fail based on the test information
        /// </summary>
        /// <param name="file">Manifest file</param>
        /// <param name="positiveSyntaxTests">Type nodes that represents positive syntax tests</param>
        /// <param name="negativeSyntaxTests">Type nodes that represents negative syntax tests</param>
        protected void RunManifest(String file, INode[] positiveSyntaxTests, INode[] negativeSyntaxTests)
        {
            if (!File.Exists(file))
            {
                Assert.Fail("Manifest file " + file + " not found");
            }

            Graph manifest = new Graph();
            try
            {
                manifest.LoadFromFile(file);
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Bad Manifest", ex);
                Assert.Fail("Failed to load Manifest " + file);
            }
            manifest.Namespaces.AddNamespace("rdf", UriFactory.Create(NamespaceMapper.RDF));
            manifest.Namespaces.AddNamespace("rdft", UriFactory.Create("http://www.w3.org/ns/rdftest#"));
            manifest.Namespaces.AddNamespace("rdfs", new Uri("http://www.w3.org/2000/01/rdf-schema#"));
            manifest.Namespaces.AddNamespace("mf", new Uri("http://www.w3.org/2001/sw/DataAccess/tests/test-manifest#"));
            manifest.Namespaces.AddNamespace("qt", new Uri("http://www.w3.org/2001/sw/DataAccess/tests/test-query#"));

            foreach (Triple testTriple in manifest.GetTriplesWithPredicate(manifest.CreateUriNode("mf:action")))
            {
                Triple nameTriple = manifest.GetTriplesWithSubjectPredicate(testTriple.Subject, manifest.CreateUriNode("mf:name")).FirstOrDefault();
                String name = nameTriple != null && nameTriple.Object.NodeType == NodeType.Literal ? nameTriple.Object.Value : null;

                String input;
                if (testTriple.Object.NodeType == NodeType.Blank)
                {
                    Triple inputTriple = manifest.GetTriplesWithSubjectPredicate(testTriple.Object, manifest.CreateUriNode("qt:data")).FirstOrDefault();
                    if (inputTriple == null) continue;
                    input = this.GetFile(inputTriple.Object);
                }
                else
                {
                    input = this.GetFile(testTriple.Object);
                }

                Triple commentTriple = manifest.GetTriplesWithSubjectPredicate(testTriple.Subject, manifest.CreateUriNode("rdfs:comment")).FirstOrDefault();
                String comment = commentTriple != null && commentTriple.Object.NodeType == NodeType.Literal ? commentTriple.Object.Value : null;

                Triple resultsTriple = manifest.GetTriplesWithSubjectPredicate(testTriple.Subject, manifest.CreateUriNode("mf:result")).FirstOrDefault();
                String results = resultsTriple != null && resultsTriple.Object.NodeType == NodeType.Literal ? this.GetFile(resultsTriple.Object) : null;

                bool? shouldParse = results != null;
                if (!shouldParse.Value)
                {
                    //No results declared so may be a positive/negative syntax test
                    //Inspect returned type to determine, if no type assume test should fail
                    Triple typeTriple = manifest.GetTriplesWithSubjectPredicate(testTriple.Subject, manifest.CreateUriNode("rdf:type")).FirstOrDefault();
                    if (typeTriple == null)
                    {
                        shouldParse = null;
                    }
                    else if (positiveSyntaxTests.Contains(typeTriple.Object))
                    {
                        shouldParse = true;
                    }
                    else if (negativeSyntaxTests.Contains(typeTriple.Object))
                    {}
                    else
                    {
                        //Unable to determine what the expected result is
                        shouldParse = null;
                    }
                }

                this.RunTest(name, comment, input, results, shouldParse);
            }
        }

        private void RunTest(String name, String comment, String file, String resultFile, bool? shouldParse)
        {
            Console.WriteLine("### Running Test #" + this.Count);
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
                this.Failed++;
                return;
            }

            try
            {
                var actual = TryParseTestInput(file);

                if (!shouldParse.HasValue)
                {
                    Console.WriteLine("Unable to determine whether the test should pass/fail based on manifest information (Test Indeterminate)");
                    this.Indeterminate++;
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
                            this.Failed++;
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
                                this.Indeterminate++;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("No Validation Required (Test Passed)");
                        this.Passed++;
                    }
                }
                else
                {
                    Console.WriteLine("Parsed when failure was expected (Test Failed)");
                    Console.Error.WriteLine("Test " + name + " - Parsed when failure was expected");
                    this.Failed++;
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
                    this.Failed++;
                }
                else
                {
                    Console.WriteLine("Parsing Failed as expected (Test Passed)");
                    this.Passed++;
                }
            }
            Console.WriteLine("### End Test #" + this.Count);
            Console.WriteLine();
            this.Count++;
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
