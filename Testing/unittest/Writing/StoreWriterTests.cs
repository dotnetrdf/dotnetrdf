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
using System.Threading;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace VDS.RDF.Writing
{

    public class StoreWriterTests
    {
        private void TestWriter(IStoreWriter writer, IStoreReader reader, bool useMultiThreaded, int compressionLevel)
        {
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            g.BaseUri = null;
            store.Add(g);
            g = new Graph();
            g.LoadFromFile("resources\\InferenceTest.ttl");
            g.BaseUri = new Uri("http://example.org/graph");
            store.Add(g);
            g = new Graph();
            g.LoadFromFile(@"resources\cyrillic.rdf");
            g.BaseUri = new Uri("http://example.org/cyrillic");
            store.Add(g);

            if (writer is ICompressingWriter)
            {
                ((ICompressingWriter)writer).CompressionLevel = compressionLevel;
            }
            if (writer is IMultiThreadedWriter)
            {
                ((IMultiThreadedWriter)writer).UseMultiThreadedWriting = useMultiThreaded;
            }
            System.IO.StringWriter strWriter = new System.IO.StringWriter();
            writer.Save(store, strWriter);

            Console.WriteLine(strWriter.ToString());

            Assert.False(strWriter.ToString().Equals(String.Empty));

            TripleStore store2 = new TripleStore();
            reader.Load(store2, new System.IO.StringReader(strWriter.ToString()));

            foreach (IGraph graph in store.Graphs)
            {
                Assert.True(store2.HasGraph(graph.BaseUri), "Parsed Stored should have contained serialized graph");
                Assert.Equal(graph, store2[graph.BaseUri]);
            }
        }

        private void TestWriter(IStoreWriter writer, IStoreReader reader, bool useMultiThreaded)
        {
            this.TestWriter(writer, reader, useMultiThreaded, Options.DefaultCompressionLevel);
        }

        [Fact]
        public void WritingNQuads()
        {
            TestTools.TestInMTAThread(this.WritingNQuadsActual);
        }

        [Fact]
        public void WritingNQuadsSingleThreaded()
        {
            this.WritingNQuadsActual();
        }

        private void WritingNQuadsActual()
        {
            this.TestWriter(new NQuadsWriter(NQuadsSyntax.Original), new NQuadsParser(NQuadsSyntax.Original), true);
        }

        [Fact]
        public void WritingNQuadsMixed()
        {
            TestTools.TestInMTAThread(this.WritingNQuadsMixedActual);
        }

        [Fact]
        public void WritingNQuadsMixedSingleThreaded()
        {
            this.WritingNQuadsMixedActual();
        }

        private void WritingNQuadsMixedActual()
        {
            this.TestWriter(new NQuadsWriter(NQuadsSyntax.Original), new NQuadsParser(NQuadsSyntax.Rdf11), true);
        }

        [Fact]
        public void WritingNQuadsMixedBad()
        {
            Assert.Throws<RdfParseException>(() => TestTools.TestInMTAThread(this.WritingNQuadsMixedBadActual));
        }

        [Fact]
        public void WritingNQuadsMixedBadSingleThreaded()
        {
            Assert.Throws<RdfParseException>(() => this.WritingNQuadsMixedBadActual());
        }

        private void WritingNQuadsMixedBadActual()
        {
            this.TestWriter(new NQuadsWriter(NQuadsSyntax.Rdf11), new NQuadsParser(NQuadsSyntax.Original), true);
        }

        [Fact]
        public void WritingNQuads11()
        {
            TestTools.TestInMTAThread(this.WritingNQuads11Actual);
        }

        [Fact]
        public void WritingNQuads11SingleThreaded()
        {
            this.WritingNQuads11Actual();
        }

        private void WritingNQuads11Actual()
        {
            this.TestWriter(new NQuadsWriter(NQuadsSyntax.Rdf11), new NQuadsParser(NQuadsSyntax.Rdf11), true);
        }

        [Fact]
        public void WritingTriG()
        {
            TestTools.TestInMTAThread(this.WritingTriGActual);
        }

        [Fact]
        public void WritingTriGSingleThreaded()
        {
            this.WritingTriGActual();
        }

        private void WritingTriGActual()
        {
            this.TestWriter(new TriGWriter(), new TriGParser(), true);
        }

        [Fact]
        public void WritingTriGUncompressed()
        {
            TestTools.TestInMTAThread(this.WritingTriGUncompressedActual);
        }

        [Fact]
        public void WritingTriGUncompressedSingleThreaded()
        {
            this.WritingTriGUncompressedActual();
        }

        private void WritingTriGUncompressedActual()
        {
            this.TestWriter(new TriGWriter(), new TriGParser(), true, WriterCompressionLevel.None);
        }

        [Fact]
        public void WritingTriX()
        {
            this.TestWriter(new TriXWriter(), new TriXParser(), false);
        }
    }
}
