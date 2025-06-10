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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VDS.RDF.Query;
using Xunit;

namespace VDS.RDF.Parsing.Suites;

public class TestFailure
{
    public string TestFile;
    public string FailureMessage;
    public Exception FailureException;

    public TestFailure(string fileName, string message, Exception failureException)
    {
        TestFile = fileName;
        FailureMessage = message;
        FailureException = failureException;
    }

    public override string ToString()
    {
        return FailureException != null ? $"{TestFile}: {FailureMessage}\n\tCause: {FailureException}" : $"{TestFile}: {FailureMessage}";
    }
}

public abstract class BaseParserSuite<TParser, TResult> where TParser : class
{
    private readonly string _baseDir;
    public static Uri BaseUri = new Uri("http://www.w3.org/2001/sw/DataAccess/df1/tests/");
    private readonly List<string> _passedTests;
    private readonly List<TestFailure> _failedTests;
    private readonly List<string> _indeterminateTests;

    protected BaseParserSuite(TParser testParser, TParser resultsParser, string baseDir)
    {
        if (baseDir == null) throw new ArgumentNullException(nameof(baseDir));

        Parser = testParser ?? throw new ArgumentNullException(nameof(testParser));
        ResultsParser = resultsParser ?? throw new ArgumentNullException(nameof(resultsParser));
        _baseDir = Path.Combine("resources", baseDir);
        _passedTests = new List<string>();
        _failedTests = new List<TestFailure>();
        _indeterminateTests = new List<string>();
    }

    /// <summary>
    /// Gets/Sets whether the suite needs to check result files for tests that should parse
    /// </summary>
    public bool CheckResults { get; set; } = true;

    public int Count { get; private set; }

    public int Passed => _passedTests.Count;

    public int Failed => _failedTests.Count;

    public int Indeterminate => _indeterminateTests.Count;

    public IReadOnlyList<string> PassedTests => _passedTests.AsReadOnly();
    public IReadOnlyList<TestFailure> FailedTests => _failedTests.AsReadOnly();
    public IReadOnlyList<string> IndeterminateTests => _indeterminateTests.AsReadOnly();

    protected void PassedTest(string testName) { _passedTests.Add(testName);}

    protected void FailedTest(string testName, string failureReason, Exception failureException = null)
    {
        _failedTests.Add(new TestFailure(testName, failureReason, failureException));
    }

    protected void IndeterminateTest(string testName) { _indeterminateTests.Add(testName);}

    protected string GetFile(INode n)
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
                    var lastSegment = u.Segments[u.Segments.Length - 1];
                    return Path.Combine(_baseDir, lastSegment);
                }
            default:
                Assert.Fail("Malformed manifest file, input file must be a  URI");
                break;
        }
        //Keep compiler happy
        return null;
    }

    protected void RunDirectory(string pattern, bool shouldParse)
    {
        RunDirectory(_baseDir, pattern, shouldParse);
    }

    protected void RunDirectory(Func<string, bool> isTest, bool shouldParse)
    {
        RunDirectory(_baseDir, isTest, shouldParse);
    }

    protected void RunDirectory(string dir, string pattern, bool shouldParse)
    {
        foreach (var file in Directory.GetFiles(dir, pattern))
        {
            RunTest(Path.GetFileName(file), null, file, Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file) + ".nt"), shouldParse);
        }
    }

    protected void RunAllDirectories(Func<string, bool> isTest, bool shouldParse)
    {
        RunAllDirectories(_baseDir, isTest, shouldParse);
    }

    protected void RunAllDirectories(string dir, Func<string, bool> isTest, bool shouldParse)
    {
        foreach (var subdir in Directory.GetDirectories(dir))
        {
            RunDirectory(subdir, isTest, shouldParse);
            RunAllDirectories(subdir, isTest, shouldParse);
        }
    }

    protected void RunDirectory(string dir, Func<string, bool> isTest, bool shouldParse)
    {
        foreach (var file in Directory.GetFiles(dir).Where(isTest))
        {
            RunTest(Path.GetFileName(file), null, file, Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file) + FileExtension), shouldParse);
        }
    }

    protected void RunManifest(string file, bool shouldParse)
    {
        Assert.True(File.Exists(file), "Manifest file " + file + " not found");

        var manifest = new Graph
        {
            BaseUri = BaseUri
        };
        try
        {
            manifest.LoadFromFile(file);
        }
        catch (Exception ex)
        {
            TestTools.ReportError("Bad Manifest", ex);
            Assert.Fail("Failed to load Manifest " + file);
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

        var tests = manifest.ExecuteQuery(findTests) as SparqlResultSet;
        Assert.NotNull(tests);

        foreach (SparqlResult test in tests)
        {
            INode nameNode, commentNode, resultNode;
            var name = test.TryGetBoundValue("name", out nameNode) ? nameNode.ToString() : null;
            INode inputNode = test["input"];
            var input = GetFile(inputNode);
            var comment = test.TryGetBoundValue("comment", out commentNode) ? commentNode.ToString() : null;
            var results = test.TryGetBoundValue("result", out resultNode) ? GetFile(resultNode) : null;

            RunTest(name, comment, input, results, shouldParse);
        }
    }/// <summary>
    /// Runs all tests found in the manifest, determines whether a test should pass/fail based on the test information
    /// </summary>
    /// <param name="file">Manifest file</param>
    protected void RunManifest(string file, INode positiveSyntaxTest, INode negativeSyntaxTest)
    {
        RunManifest(file, new[] { positiveSyntaxTest }, new[] { negativeSyntaxTest });
    }

    protected void RunManifest(string file, INode[] positiveSyntaxTests, INode[] negativeSyntaxTests)
    {
        Assert.True(File.Exists(file), "Manifest file " + file + " not found");


        var manifest = new Graph
        {
            BaseUri = BaseUri
        };
        try
        {
            manifest.LoadFromFile(file);
        }
        catch (Exception ex)
        {
            TestTools.ReportError("Bad Manifest", ex);
            Assert.Fail("Failed to load Manifest " + file);
        }
        manifest.NamespaceMap.AddNamespace("rdf", UriFactory.Root.Create("http://www.w3.org/ns/rdftest#"));

        var findTests = @"prefix rdf:    <http://www.w3.org/1999/02/22-rdf-syntax-ns#> 
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

        var tests = manifest.ExecuteQuery(findTests) as SparqlResultSet;
        Assert.NotNull(tests);

        foreach (SparqlResult test in tests)
        {
            INode nameNode, inputNode, commentNode, resultNode;
            var name = test.TryGetBoundValue("name", out nameNode) ? nameNode.ToString() : null;
            inputNode = test["input"];
            var input = GetFile(inputNode);
            var comment = test.TryGetBoundValue("comment", out commentNode) ? commentNode.ToString() : null;
            var results = test.TryGetBoundValue("result", out resultNode) ? GetFile(resultNode) : null;

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

            RunTest(name, comment, input, results, shouldParse);
        }
    }

    private void RunTest(string name, string comment, string file, string resultFile, bool? shouldParse)
    {
        Console.WriteLine("### Running Test #" + Count);
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
            FailedTest(file, "Input file not found");
            return;
        }

        try
        {
            var actual = TryParseTestInput(file);

            if (!shouldParse.HasValue)
            {
                Console.WriteLine("Unable to determine whether the test should pass/fail based on manifest information (Test Indeterminate)");
                IndeterminateTest(name);
            }
            else if (shouldParse.Value)
            {
                Console.WriteLine("Parsed input in OK");

                //Validate if necessary
                if (CheckResults && resultFile != null)
                {
                    if (!File.Exists(resultFile))
                    {
                        Console.WriteLine("Expected Output File not found");
                        Console.Error.WriteLine("Test " + name + " - Expected Output File not found: " + resultFile);
                        FailedTest(name, "Expected output file not found:" + resultFile );
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
                            IndeterminateTest(name);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No Validation Required (Test Passed)");
                    PassedTest(name);
                }
            }
            else
            {
                Console.WriteLine("Parsed when failure was expected (Test Failed)");
                Console.Error.WriteLine("Test " + name + " - Parsed when failure was expected");
                FailedTest(name, "File parsed successfully when a failure was expected");
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
                if (Parser is ITraceableTokeniser)
                {
                    try
                    {
                        ((ITraceableTokeniser)Parser).TraceTokeniser = true;
                        ((IRdfReader)Parser).Load(new Graph(), Path.GetFileName(file));
                    }
                    catch
                    {
                        //Ignore errors the 2nd time around, we've already got a copy of the error to report
                    }
                    finally
                    {
                        ((ITraceableTokeniser)Parser).TraceTokeniser = false;
                    }
                }

                TestTools.ReportError("Parse Error", parseEx);
                FailedTest(file, "Parsing failed when a success was expected." + parseEx );
            }
            else
            {
                Console.WriteLine("Parsing Failed as expected (Test Passed)");
                PassedTest(name);
            }
        }
        Console.WriteLine("### End Test #" + Count);
        Console.WriteLine();
        Count++;
    }

    protected abstract TResult TryParseTestInput(string file);

    protected abstract void TryValidateResults(string testName, string resultFile, TResult actual);

    protected abstract string FileExtension { get; }

    protected TParser Parser { get; }

    protected TParser ResultsParser { get; }
}
