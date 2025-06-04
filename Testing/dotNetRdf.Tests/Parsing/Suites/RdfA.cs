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
using System.Diagnostics;
using System.IO;
using System.Linq;
using VDS.RDF.Query;
using VDS.RDF.Writing;
using Xunit;

namespace VDS.RDF.Parsing.Suites;

public class RdfA
{
    private readonly ITestOutputHelper _testOutputHelper;

    public RdfA(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    private void reportError(String header, Exception ex)
    {
        _testOutputHelper.WriteLine(header);
        _testOutputHelper.WriteLine(ex.Message);
        _testOutputHelper.WriteLine(ex.StackTrace);
    }

    [Fact]
    public void ParsingSuiteRdfA10()
    {
        String[] wantOutput = {  };
        var outputAll = false;
        String[] skipTests = { 
                               "0002.xhtml",
                               "0003.xhtml", 
                               "0004.xhtml",
                               "0005.xhtml",
                               "0016.xhtml",
                               "0022.xhtml",
                               "0024.xhtml",
                               "0028.xhtml",
                               "0043.xhtml",
                               "0044.xhtml",
                               "0045.xhtml",
                               "0095.xhtml",
                               "0096.xhtml",
                               "0097.xhtml",
                               "0098.xhtml",
                               "0122.xhtml",
                               "0123.xhtml",
                               "0124.xhtml",
                               "0125.xhtml",
                               "0126.xhtml"
                             };

        String[] skipCheck = {
                                 "0011.xhtml",
                                 "0092.xhtml",
                                 "0094.xhtml",
                                 "0100.xhtml",
                                 "0101.xhtml",
                                 "0102.xhtml",
                                 "0103.xhtml"
                             };

        String[] falseTests = {
                                "0042.xhtml",
                                "0086.xhtml",
                                "0095.xhtml",
                                "0096.xhtml",
                                "0097.xhtml",
                                "0107.xhtml",
                                "0116.xhtml",
                                "0122.xhtml",
                                "0125.xhtml"
                              };

        try
        {
            var testsPassed = 0;
            var testsFailed = 0;
            var files = Directory.GetFiles(Path.Combine("resources", "rdfa"));
            var parser = new RdfAParser(RdfASyntax.AutoDetectLegacy);
            //XHtmlPlusRdfAParser parser = new XHtmlPlusRdfAParser();
            parser.Warning += parser_Warning;
            var queryparser = new SparqlQueryParser();
            bool passed, passDesired;
            var g = new Graph();
            var timer = new Stopwatch();
            long totalTime = 0;
            long totalTriples = 0;

            foreach (var file in files)
            {
                timer.Reset();

                if (skipTests.Contains(Path.GetFileName(file)))
                {
                    _testOutputHelper.WriteLine("## Skipping Test of File " + Path.GetFileName(file));
                    _testOutputHelper.WriteLine("");
                    continue;
                }

                if (Path.GetExtension(file) != ".html" && Path.GetExtension(file) != ".xhtml")
                {
                    continue;
                }

                _testOutputHelper.WriteLine("## Testing File " + Path.GetFileName(file));
                _testOutputHelper.WriteLine("# Test Started at " + DateTime.Now);

                passed = false;
                passDesired = true;

                try
                {
                    g = new Graph
                    {
                        BaseUri = new Uri("http://www.w3.org/2006/07/SWD/RDFa/testsuite/xhtml1-testcases/" +
                                          Path.GetFileName(file))
                    };
                    if (Path.GetFileNameWithoutExtension(file).StartsWith("bad"))
                    {
                        passDesired = false;
                        _testOutputHelper.WriteLine("# Desired Result = Parsing Failed");
                    }
                    else
                    {
                        _testOutputHelper.WriteLine("# Desired Result = Parses OK");
                    }

                    timer.Start();
                    parser.Load(g, file);
                    timer.Stop();

                    _testOutputHelper.WriteLine("Parsing took " + timer.ElapsedMilliseconds + "ms");

                    passed = true;
                    _testOutputHelper.WriteLine("Parsed OK");

                    if (outputAll || wantOutput.Contains(Path.GetFileName(file)))
                    {
                        var writer = new NTriplesWriter();
                        writer.Save(g, "rdfa_tests\\" + Path.GetFileNameWithoutExtension(file) + ".out");
                    }

                }
                catch (IOException ioEx)
                {
                    reportError("IO Exception", ioEx);
                }
                catch (RdfParseException parseEx)
                {
                    reportError("Parsing Exception", parseEx);
                }
                catch (RdfException rdfEx)
                {
                    reportError("RDF Exception", rdfEx);
                }
                catch (Exception ex)
                {
                    reportError("Other Exception", ex);
                }
                finally
                {
                    timer.Stop();

                    //Write the Triples to the Output
                    foreach (Triple t in g.Triples)
                    {
                        _testOutputHelper.WriteLine(t.ToString());
                    }

                    //Now we run the Test SPARQL (if present)
                    if (File.Exists("rdfa_tests/" + Path.GetFileNameWithoutExtension(file) + ".sparql"))
                    {
                        if (skipCheck.Contains(Path.GetFileName(file)))
                        {
                            _testOutputHelper.WriteLine("## Skipping Check of File " + Path.GetFileName(file));
                            _testOutputHelper.WriteLine("");
                        }
                        else
                        {
                            try
                            {
                                SparqlQuery q = queryparser.ParseFromFile("rdfa_tests/" + Path.GetFileNameWithoutExtension(file) + ".sparql");
                                var results = g.ExecuteQuery(q);
                                if (results is SparqlResultSet)
                                {
                                    //The Result is the result of the ASK Query
                                    if (falseTests.Contains(Path.GetFileName(file)))
                                    {
                                        passed = !((SparqlResultSet)results).Result;
                                    }
                                    else
                                    {
                                        passed = ((SparqlResultSet)results).Result;
                                    }
                                }
                            }
                            catch
                            {
                                passed = false;
                            }
                        }
                    }

                    if (passed && passDesired)
                    {
                        //Passed and we wanted to Pass
                        testsPassed++;
                        _testOutputHelper.WriteLine("# Result = Test Passed");
                        totalTime += timer.ElapsedMilliseconds;
                        totalTriples += g.Triples.Count;
                    }
                    else if (!passed && passDesired)
                    {
                        //Failed when we should have Passed
                        testsFailed++;
                        _testOutputHelper.WriteLine("# Result = Test Failed");
                    }
                    else if (passed && !passDesired)
                    {
                        //Passed when we should have Failed
                        testsFailed++;
                        _testOutputHelper.WriteLine("# Result = Test Failed");
                    }
                    else
                    {
                        //Failed and we wanted to Fail
                        testsPassed++;
                        _testOutputHelper.WriteLine("# Result = Test Passed");
                    }

                    _testOutputHelper.WriteLine("# Triples Generated = " + g.Triples.Count());
                    _testOutputHelper.WriteLine("# Test Ended at " + DateTime.Now);
                }

                _testOutputHelper.WriteLine("");
            }

            _testOutputHelper.WriteLine(testsPassed + " Tests Passed");
            _testOutputHelper.WriteLine(testsFailed + " Tests Failed");
            _testOutputHelper.WriteLine("");
            _testOutputHelper.WriteLine($"Total Parsing Time was {totalTime} ms");
            if (totalTime > 1000)
            {
                _testOutputHelper.WriteLine(" (" + totalTime / 1000d + " seconds)");
            }
            _testOutputHelper.WriteLine("Average Parsing Speed was " + totalTriples / (totalTime / 1000d) + " triples/second");

            if (testsFailed > 0) Assert.Fail(testsFailed + " Tests Failed");
        }
        catch (Exception ex)
        {
            reportError("Other Exception", ex);
            throw;
        }
    }

    void parser_Warning(string warning)
    {
        _testOutputHelper.WriteLine("Warning: " + warning);
    }
}
