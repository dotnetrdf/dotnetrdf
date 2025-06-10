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
using VDS.RDF.Parsing;

namespace VDS.RDF.Writing;

public class StoreWriterTests
{
    private void TestWriter(IStoreWriter writer, IStoreReader reader, bool useMultiThreaded, int compressionLevel = WriterCompressionLevel.More)
    {
        var store = new TripleStore();
        var g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        store.Add(g);

        g = new Graph(new UriNode(new Uri("http://example.org/graph")));
        g.LoadFromFile(Path.Combine("resources", "InferenceTest.ttl"));
        store.Add(g);

        g = new Graph(new UriNode(new Uri("http://example.org/cyrillic")));
        g.LoadFromFile(Path.Combine("resources", "cyrillic.rdf"));
        store.Add(g);

        if (writer is ICompressingWriter)
        {
            ((ICompressingWriter)writer).CompressionLevel = compressionLevel;
        }
        if (writer is IMultiThreadedWriter)
        {
            ((IMultiThreadedWriter)writer).UseMultiThreadedWriting = useMultiThreaded;
        }
        var strWriter = new System.IO.StringWriter();
        writer.Save(store, strWriter);

        Console.WriteLine(strWriter.ToString());

        Assert.NotEqual(strWriter.ToString(), String.Empty);

        var store2 = new TripleStore();
        reader.Load(store2, new System.IO.StringReader(strWriter.ToString()));

        foreach (IGraph graph in store.Graphs)
        {
            Assert.True(store2.HasGraph(graph.Name), "Parsed Stored should have contained serialized graph");
            Assert.Equal(graph, store2[graph.Name]);
        }
    }

    [Fact(SkipExceptions = [typeof(PlatformNotSupportedException)])]
    public void WritingTriX()
    {
        TestWriter(new TriXWriter(), new TriXParser(), false);
    }

    [Fact(SkipExceptions = [typeof(PlatformNotSupportedException)])]
    public void WritingNQuads()
    {
        TestTools.TestInMTAThread(WritingNQuadsActual);
    }

    [Fact]
    public void WritingNQuadsSingleThreaded()
    {
        WritingNQuadsActual();
    }

    private void WritingNQuadsActual()
    {
        TestWriter(new NQuadsWriter(NQuadsSyntax.Original), new NQuadsParser(NQuadsSyntax.Original), true);
    }

    [Fact(SkipExceptions = [typeof(PlatformNotSupportedException)])]
    public void WritingNQuadsMixed()
    {
        TestTools.TestInMTAThread(WritingNQuadsMixedActual);
    }

    [Fact]
    public void WritingNQuadsMixedSingleThreaded()
    {
        WritingNQuadsMixedActual();
    }

    private void WritingNQuadsMixedActual()
    {
        TestWriter(new NQuadsWriter(NQuadsSyntax.Original), new NQuadsParser(NQuadsSyntax.Rdf11), true);
    }

    [Fact]
    public void WritingNQuadsMixedBad()
    {
        Assert.SkipUnless(System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows), "Only supported on Windows");

        Assert.Throws<RdfParseException>(() => TestTools.TestInMTAThread(WritingNQuadsMixedBadActual));
    }

    [Fact]
    public void WritingNQuadsMixedBadSingleThreaded()
    {
        Assert.Throws<RdfParseException>(() => WritingNQuadsMixedBadActual());
    }

    private void WritingNQuadsMixedBadActual()
    {
        TestWriter(new NQuadsWriter(NQuadsSyntax.Rdf11), new NQuadsParser(NQuadsSyntax.Original), true);
    }

    [Fact(SkipExceptions = [typeof(PlatformNotSupportedException)])]
    public void WritingNQuads11()
    {
        TestTools.TestInMTAThread(WritingNQuads11Actual);
    }

    [Fact]
    public void WritingNQuads11SingleThreaded()
    {
        WritingNQuads11Actual();
    }

    private void WritingNQuads11Actual()
    {
        TestWriter(new NQuadsWriter(NQuadsSyntax.Rdf11), new NQuadsParser(NQuadsSyntax.Rdf11), true);
    }

    [Fact(SkipExceptions = [typeof(PlatformNotSupportedException)])]
    public void WritingTriG()
    {
        TestTools.TestInMTAThread(WritingTriGActual);
    }

    [Fact]
    public void WritingTriGSingleThreaded()
    {
        WritingTriGActual();
    }

    private void WritingTriGActual()
    {
        TestWriter(new TriGWriter(), new TriGParser(), true);
    }

    [Fact(SkipExceptions = [typeof(PlatformNotSupportedException)])]
    public void WritingTriGUncompressed()
    {
        TestTools.TestInMTAThread(WritingTriGUncompressedActual);
    }

    [Fact]
    public void WritingTriGUncompressedSingleThreaded()
    {
        WritingTriGUncompressedActual();
    }

    private void WritingTriGUncompressedActual()
    {
        TestWriter(new TriGWriter(), new TriGParser(), true, WriterCompressionLevel.None);
    }
}
