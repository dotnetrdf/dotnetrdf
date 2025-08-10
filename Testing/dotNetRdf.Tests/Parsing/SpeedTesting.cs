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
using Xunit;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Parsing;

[Trait("Coverage", "Skip")]
[Trait("Category", "explicit")]
[Trait("Category", "performance")]
// TODO: Speed testing should test with URI interning enabled vs disabled
public class SpeedTesting
{
    private readonly ITestOutputHelper _testOutputHelper;

    public SpeedTesting(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    private void EnsureTestData(int triples, string file, ITripleFormatter formatter)
    {
        if (!File.Exists(file))
        {
            var g = new Graph();
            g.NamespaceMap.AddNamespace(string.Empty, new Uri("http://example.org/node#"));

            using (var writer = new StreamWriter(File.OpenWrite(file)))
            {
                for (var i = 1; i <= triples; i++)
                {
                    var temp = g.CreateUriNode(":" + i);
                    writer.WriteLine(formatter.Format(new Triple(temp, temp, temp)));
                }

                writer.Close();
            }
        }

        //Force a GC prior to each of these tests
        GC.GetTotalMemory(true);
    }

    private void CalculateSpeed(int triples, Stopwatch watch)
    {
        var tps = triples * (1000.0d / watch.ElapsedMilliseconds);
        _testOutputHelper.WriteLine("Triples/Second = " + tps);
    }

    [Fact]
    public void ParsingSpeedTurtle10Thousand()
    {
        EnsureTestData(10000, "10thou.ttl", new TurtleFormatter());

        var g = new Graph();
        var watch = new Stopwatch();
        var parser = new TurtleParser();

        watch.Start();
        parser.Load(g, "10thou.ttl");
        watch.Stop();

        _testOutputHelper.WriteLine(watch.Elapsed.ToString());
        CalculateSpeed(10000, watch);
    }

    [Fact]
    public void ParsingSpeedTurtle100Thousand()
    {
        EnsureTestData(100000, "100thou.ttl", new TurtleFormatter());

        var g = new Graph();
        var watch = new Stopwatch();
        var parser = new TurtleParser();

        watch.Start();
        parser.Load(g, "100thou.ttl");
        watch.Stop();

        _testOutputHelper.WriteLine(watch.Elapsed.ToString());
        CalculateSpeed(100000, watch);
    }

    [Fact]
    public void ParsingSpeedTurtle500Thousand()
    {
        EnsureTestData(500000, "500thou.ttl", new TurtleFormatter());

        var g = new Graph();
        var watch = new Stopwatch();
        var parser = new TurtleParser();

        watch.Start();
        parser.Load(g, "500thou.ttl");
        watch.Stop();

        _testOutputHelper.WriteLine(watch.Elapsed.ToString());
        CalculateSpeed(500000, watch);
    }

    [Fact]
    public void ParsingSpeedTurtle10ThousandCountOnly()
    {
        EnsureTestData(10000, "10thou.ttl", new TurtleFormatter());

        var handler = new CountHandler();
        var watch = new Stopwatch();
        var parser = new TurtleParser();

        watch.Start();
        parser.Load(handler, "10thou.ttl");
        watch.Stop();

        _testOutputHelper.WriteLine(watch.Elapsed.ToString());
        CalculateSpeed(10000, watch);

        Assert.Equal(10000, handler.Count);
    }

    [Fact]
    public void ParsingSpeedTurtle100ThousandCountOnly()
    {
        EnsureTestData(100000, "100thou.ttl", new TurtleFormatter());

        var handler = new CountHandler();
        var watch = new Stopwatch();
        var parser = new TurtleParser();

        watch.Start();
        parser.Load(handler, "100thou.ttl");
        watch.Stop();

        _testOutputHelper.WriteLine(watch.Elapsed.ToString());
        CalculateSpeed(100000, watch);

        Assert.Equal(100000, handler.Count);
    }

    [Fact]
    public void ParsingSpeedNTriples10Thousand()
    {
        EnsureTestData(10000, "10thou.nt", new NTriplesFormatter());

        var g = new Graph();
        var watch = new Stopwatch();
        var parser = new NTriplesParser();

        watch.Start();
        parser.Load(g, "10thou.nt");
        watch.Stop();

        _testOutputHelper.WriteLine(watch.Elapsed.ToString());
        CalculateSpeed(10000, watch);
    }

    [Fact]
    public void ParsingSpeedNTriples100Thousand()
    {
        EnsureTestData(100000, "100thou.nt", new NTriplesFormatter());

        var g = new Graph();
        var watch = new Stopwatch();
        var parser = new NTriplesParser();

        watch.Start();
        parser.Load(g, "100thou.nt");
        watch.Stop();

        _testOutputHelper.WriteLine(watch.Elapsed.ToString());
        CalculateSpeed(100000, watch);
    }

    [Fact]
    public void ParsingSpeedNTriples500Thousand()
    {
        EnsureTestData(500000, "500thou.nt", new NTriplesFormatter());

        var g = new Graph();
        var watch = new Stopwatch();
        var parser = new NTriplesParser();

        watch.Start();
        parser.Load(g, "500thou.nt");
        watch.Stop();

        _testOutputHelper.WriteLine(watch.Elapsed.ToString());
        CalculateSpeed(500000, watch);
    }

    [Fact(SkipExceptions = [typeof(OutOfMemoryException)])]
    [Trait("Category", "explicit")]
    public void ParsingSpeedNTriples1Million()
    {
        EnsureTestData(1000000, "million.nt", new NTriplesFormatter());

        var g = new Graph();
        var watch = new Stopwatch();
        var parser = new NTriplesParser();

        watch.Start();
        parser.Load(g, "million.nt");
        watch.Stop();

        _testOutputHelper.WriteLine(watch.Elapsed.ToString());
        CalculateSpeed(1000000, watch);
    }
}
