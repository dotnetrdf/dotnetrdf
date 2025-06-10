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
using Xunit;

namespace VDS.RDF.Parsing.Suites;

public class TriX
    : BaseDatasetParserSuite
{
    private readonly ITestOutputHelper _testOutputHelper;

    public TriX(ITestOutputHelper testOutputHelper)
        : base(new TriXParser(), new NQuadsParser(), "trix")
    {
        _testOutputHelper = testOutputHelper;
        CheckResults = false;
    }

    [Fact]
    public void ParsingSuiteTriX()
    {
        RunManifests();

        if (Count == 0) Assert.Fail("No tests found");

        _testOutputHelper.WriteLine(Count + " Tests - " + Passed + " Passed - " + Failed + " Failed");
        _testOutputHelper.WriteLine(((Passed / (double)Count) * 100) + "% Passed");

        if (Failed > 0)
        {
            foreach(var failure in FailedTests) { _testOutputHelper.WriteLine(failure.ToString());}
            Assert.Fail(Failed + " Tests failed");
        }
        Assert.SkipWhen(Indeterminate > 0, Indeterminate + " Tests are indeterminate");
    }

    private void RunManifests()
    {
        RunDirectory(f => Path.GetExtension(f).Equals(".xml") && !f.Contains("bad"), true);
        RunDirectory(f => Path.GetExtension(f).Equals(".xml") && f.Contains("bad"), false);
    }
}
