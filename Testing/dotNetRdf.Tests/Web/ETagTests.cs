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
using Xunit;
using VDS.RDF.Parsing;

namespace VDS.RDF.Web;

public class ETagTests
{
    private readonly ITestOutputHelper _output;
    public ETagTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void WebETagComputation()
    {
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        var timer = new Stopwatch();
        timer.Start();
        var etag = g.GetETag();
        timer.Stop();
        _output.WriteLine("ETag 1 took " + timer.Elapsed + " to calculate");

        Assert.False(String.IsNullOrEmpty(etag));
        Assert.Equal(40, etag.Length);

        timer.Reset();
        timer.Start();
        var etag2 = g.GetETag();
        timer.Stop();
        _output.WriteLine("ETag 2 took " + timer.Elapsed + " to calculate after no graph modifications");

        Assert.Equal(etag, etag2);
        Assert.Equal(40, etag2.Length);

        g.Retract(g.Triples.First());
        timer.Reset();
        timer.Start();
        var etag3 = g.GetETag();
        timer.Stop();
        _output.WriteLine("ETag 3 took " + timer.Elapsed + " to calculate after triple retraction");

        Assert.NotEqual(etag, etag3);
        Assert.NotEqual(etag2, etag3);
        Assert.Equal(40, etag3.Length);
    }
}
