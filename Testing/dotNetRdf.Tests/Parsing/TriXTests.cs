/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using VDS.RDF.Writing;
using System.Xml;

namespace VDS.RDF.Parsing;


public class TriXTests
{
    private TriXWriter _writer;
    private TriXParser _parser;

    public TriXTests()
    {
        _writer = new TriXWriter();
        _parser = new TriXParser();
    }

    [Fact]
    [Trait("Coverage", "Skip")]
    public void ParsingTriXPerformanceCore351()
    {
        //Test case from CORE-351
        var store = new TripleStore();
        var timer = new Stopwatch();
        timer.Start();
        _parser.Load(store, Path.Combine("resources", "lib_p11_ontology.trix"));
        timer.Stop();
        Console.WriteLine("Took " + timer.Elapsed + " to read from disk");
    }

    [Theory]
    [Trait("Coverage", "Skip")]
    [InlineData(1000, 100)]
    public void ParsingTriXPerformance_LargeDataset(int numGraphs, int triplesPerGraph)
    {
        ParsingTriXPerformance(numGraphs, triplesPerGraph);
    }

    [Theory]
    [InlineData(1000, 10)]
    [InlineData(10, 1000)]
    [InlineData(1, 100)]
    public void ParsingTriXPerformance(int numGraphs, int triplesPerGraph)
    {
        //Generate data
        var store = new TripleStore();
        for (var i = 1; i <= numGraphs; i++)
        {
            var g = new Graph(new UriNode(new Uri("http://example.org/graph/" + i)))
            {
                BaseUri = new Uri("http://example.org/graph/" + i)
            };

            for (var j = 1; j <= triplesPerGraph; j++)
            {
                g.Assert(new Triple(g.CreateUriNode(UriFactory.Root.Create("http://example.org/subject/" + j)), g.CreateUriNode(UriFactory.Root.Create("http://example.org/predicate/" + j)), (j).ToLiteral(g)));
            }
            store.Add(g);
        }

        Console.WriteLine("Generated dataset with " + numGraphs + " named graphs (" + triplesPerGraph + " triples/graph) with a total of " + (numGraphs * triplesPerGraph) + " triples");

        var timer = new Stopwatch();

        //Write out to disk
        timer.Start();
        _writer.Save(store, "temp.trix");
        timer.Stop();
        Console.WriteLine("Took " + timer.Elapsed + " to write to disk");
        timer.Reset();

        //Read back from disk
        var store2 = new TripleStore();
        timer.Start();
        _parser.Load(store2, "temp.trix");
        timer.Stop();
        Console.WriteLine("Took " + timer.Elapsed + " to read from disk");

        Assert.Equal(numGraphs * triplesPerGraph, store2.Graphs.Sum(g => g.Triples.Count));

    }

    [Fact]
    public void ParseTriXWithEmptyGraph()
    {
        var store = new TripleStore();
        _parser.Load(store, Path.Combine("resources", "trix", "emptygraph.trix"));
        Assert.Equal(0, store.Graphs.Count);
    }

    [Fact]
    public void EntityParsingCanBeDisabled()
    {
        var store = new TripleStore();
        var parser = new TriXParser();
        parser.XmlReaderSettings.DtdProcessing = DtdProcessing.Prohibit;
        var thrown = Assert.Throws<RdfParseException>(() =>
        parser.Load(store, System.IO.Path.Combine("resources", "trix", "entities.trix")));
        Assert.IsType<XmlException>(thrown.InnerException);
    }
}
