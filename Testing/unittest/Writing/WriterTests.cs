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
using System.Text;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Formatting;
#pragma warning disable 618

namespace VDS.RDF.Writing
{
    public class WriterTests
        : CompressionTests
    {
        // private String prefix = "@prefix : <http://example.org>.\n";
        
        [Fact]
        public void WritingBlankNodeOutput()
        {
            //Create a Graph and add a couple of Triples which when serialized have
            //potentially colliding IDs

            Graph g = new Graph();
            g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org"));
            IUriNode subj = g.CreateUriNode("ex:subject");
            IUriNode pred = g.CreateUriNode("ex:predicate");
            IUriNode name = g.CreateUriNode("ex:name");
            IBlankNode b1 = g.CreateBlankNode("autos1");
            IBlankNode b2 = g.CreateBlankNode("1");

            g.Assert(subj, pred, b1);
            g.Assert(b1, name, g.CreateLiteralNode("First Triple"));
            g.Assert(subj, pred, b2);
            g.Assert(b2, name, g.CreateLiteralNode("Second Triple"));

            TurtleWriter ttlwriter = new TurtleWriter();
            ttlwriter.Save(g, "bnode-output-test.ttl");

            TestTools.ShowGraph(g);

            TurtleParser ttlparser = new TurtleParser();
            Graph h = new Graph();
            ttlparser.Load(h, "bnode-output-test.ttl");

            TestTools.ShowGraph(h);

            Assert.Equal(g.Triples.Count, h.Triples.Count);

        }

        [Fact]
        public void WritingOwlCharEscaping()
        {
            Graph g = new Graph();
            FileLoader.Load(g, "resources\\charescaping.owl");

            Console.WriteLine("Original Triples");
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString());
            }
            Console.WriteLine();               

            TurtleWriter ttlwriter = new TurtleWriter();
            String serialized = VDS.RDF.Writing.StringWriter.Write(g, ttlwriter);

            Graph h = new Graph();
            StringParser.Parse(h, serialized);

            Console.WriteLine("Serialized Triples");
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString());
            }
            Console.WriteLine();

            Assert.Equal(g, h);
        }

        [Fact]
        public void WritingHtmlWriter()
        {
            Graph g = new Graph();
            FileLoader.Load(g, "resources\\InferenceTest.ttl");

            HtmlWriter writer = new HtmlWriter();
            String data = VDS.RDF.Writing.StringWriter.Write(g, writer);

            Console.WriteLine("Serialized as XHTML+RDFa");
            Console.WriteLine(data);

            Console.WriteLine();

            Graph h = new Graph();
            h.BaseUri = new Uri("http://example.org");
            StringParser.Parse(h, data, new RdfAParser());

            Console.WriteLine("Extracted Triples");
            foreach (Triple t in h.Triples)
            {
                Console.WriteLine(t.ToString());
            }

            Assert.Equal(g, h);
        }

        [Fact]
        public void WritingRdfCollections()
        {
            Graph g = new Graph();
            FileLoader.Load(g, "resources\\swrc.owl");
            CompressingTurtleWriter ttlwriter = new CompressingTurtleWriter(WriterCompressionLevel.High);
            ttlwriter.Save(g, Console.Out);
        }

        [Fact]
        public void WritingXmlAmpersandEscaping()
        {
            List<String> inputs = new List<string>()
            {
                "&value",
                "&amp;",
                "&",
                "&value&next",
                "& &squot; < > ' \"",
                new String('&', 1000)
            };

            List<String> outputs = new List<string>()
            {
                "&amp;value",
                "&amp;",
                "&amp;",
                "&amp;value&amp;next",
                "&amp; &squot; &lt; &gt; &apos; &quot;"
            };
            StringBuilder temp = new StringBuilder();
            for (int i = 0; i < 1000; i++)
            {
                temp.Append("&amp;");
            }
            outputs.Add(temp.ToString());

            for (int i = 0; i < inputs.Count; i++)
            {
                Console.WriteLine("Input: " + inputs[i] + " - Expected Output: " + outputs[i] + " - Actual Output: " + WriterHelper.EncodeForXml(inputs[i]));
                Assert.Equal(outputs[i], WriterHelper.EncodeForXml(inputs[i]));
            }
        }

        [Fact]
        public void WritingUriEscaping()
        {
            Graph g = new Graph();
            g.BaseUri = new Uri("http://example.org/space in/base");
            g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/space in/namespace"));
            IUriNode subj = g.CreateUriNode(new Uri("http://example.org/subject"));
            IUriNode pred = g.CreateUriNode(new Uri("http://example.org/predicate"));
            IUriNode obj = g.CreateUriNode(new Uri("http://example.org/with%20uri%20escapes"));
            IUriNode obj2 = g.CreateUriNode(new Uri("http://example.org/needs escapes"));

            g.Assert(subj, pred, obj);
            g.Assert(subj, pred, obj2);

            NTriplesFormatter formatter = new NTriplesFormatter();
            List<IRdfWriter> writers = new List<IRdfWriter>()
            {
                new CompressingTurtleWriter(TurtleSyntax.Original),
                new CompressingTurtleWriter(TurtleSyntax.W3C),
                new PrettyRdfXmlWriter(),
                new HtmlWriter(),
                new Notation3Writer(),
                new NTriplesWriter(NTriplesSyntax.Original),
                new NTriplesWriter(NTriplesSyntax.Rdf11),
                new PrettyRdfXmlWriter(),
                new RdfJsonWriter(),
                new RdfXmlWriter(),
                new TurtleWriter()
            };
            List<IRdfReader> parsers = new List<IRdfReader>()
            {
                new TurtleParser(TurtleSyntax.Original),
                new TurtleParser(TurtleSyntax.W3C),
                new RdfXmlParser(),
                new RdfAParser(),
                new Notation3Parser(),
                new NTriplesParser(NTriplesSyntax.Original),
                new NTriplesParser(NTriplesSyntax.Rdf11),
                new RdfXmlParser(),
                new RdfJsonParser(),
                new RdfXmlParser(),
                new TurtleParser()
            };

            Assert.Equal(parsers.Count, writers.Count);

            Console.WriteLine("Input Data:");
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }
            Console.WriteLine();

            for (int i = 0; i < writers.Count; i++)
            {
                IRdfWriter writer = writers[i];
                Console.WriteLine("Using " + writer.ToString());
                System.IO.StringWriter strWriter = new System.IO.StringWriter();
                writer.Save(g, strWriter);
                Console.WriteLine(strWriter.ToString());
                Console.WriteLine();
                Console.Out.Flush();

                IRdfReader parser = parsers[i];
                Graph h = new Graph(true);
                parser.Load(h, new StringReader(strWriter.ToString()));
                Console.WriteLine("Parsed Data:");
                foreach (Triple t in h.Triples)
                {
                    Console.WriteLine(t.ToString(formatter));
                }
                Console.WriteLine();

                Assert.Equal(g, h);
                if (h.NamespaceMap.HasNamespace("ex"))
                {
                    Assert.Equal(g.NamespaceMap.GetNamespaceUri("ex"), h.NamespaceMap.GetNamespaceUri("ex"));
                }
                if (h.BaseUri != null)
                {
                    Assert.Equal(g.BaseUri, h.BaseUri);
                }
            }
        }

        [Fact]
        public void WritingQNameValidation()
        {
            Graph g = new Graph();
            g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
            INode subj = g.CreateUriNode("ex:subject");
            INode pred = g.CreateUriNode("ex:predicate");
            List<INode> objects = new List<INode>()
            {
                g.CreateUriNode("ex:123"),
                g.CreateBlankNode("a_blank_node"),
                g.CreateBlankNode("_blank"),
                g.CreateBlankNode("-blank"),
                g.CreateBlankNode("123blank"),
                g.CreateUriNode("ex:_object"),
                g.CreateUriNode("ex:-object")
            };
            foreach (INode obj in objects)
            {
                g.Assert(subj, pred, obj);
            }

            this.CheckCompressionRoundTrip(g);
        }

        [Theory]
        [InlineData(typeof(NTriplesWriter))]
        [InlineData(typeof(RdfXmlWriter))]
        [InlineData(typeof(PrettyRdfXmlWriter))]
        [InlineData(typeof(RdfJsonWriter))]
        [InlineData(typeof(CompressingTurtleWriter))]
        [InlineData(typeof(HtmlWriter))]
        [InlineData(typeof(HtmlSchemaWriter))]
        [InlineData(typeof(GraphVizWriter))]
        public void TextWriterCanBeLeftOpen(Type writerType)
        {
            var g = new Graph();
            var writer = new System.IO.StringWriter();
            var rdfWriter = writerType.GetConstructor(new Type[0]).Invoke(new object[0]) as IRdfWriter;
            rdfWriter.Save(g, writer, true);
            writer.Write("\n"); // This should not throw because the writer is still open
            rdfWriter.Save(g, writer);
            Assert.Throws<ObjectDisposedException>(() => writer.Write("\n"));
        }
    }
}
