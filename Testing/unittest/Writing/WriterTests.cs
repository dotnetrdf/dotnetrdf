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
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing
{
    [TestClass]
    public class WriterTests
        : CompressionTests
    {

        private List<String> samples = new List<string>()
            {
                @":backslashes :string ""This example has some backslashes: C:\Program Files\somewhere"".",
                ":no-escapes :string \"a piece of text with no escapes\".",
                ":quote-escapes :string \"a piece of text containing \"quotes\"\".",
                ":long-literal :string \"\"\"a piece of text containing \"quotes\"\"\"\".",
                ":newline-longliteral-escapes :string \"\"\"this is a long literal\nwhich contains\nmultiple new lines\"\"\".",
                ":newline-longliteral-escapes-alt :string \"\"\"this is another long literal which mixes both\n\r and \n\rnew line escapes\"\"\".",
                ":tabs :string \"this string contains   unescaped tabs\".",
                ":tabs-escaped :string \"this string contains \t escaped tabs\"."
            };

        private String prefix = "@prefix : <http://example.org>.\n";
        
        [TestMethod]
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

            Assert.AreEqual(g.Triples.Count, h.Triples.Count, "Expected same number of Triples after serialization and reparsing");

        }

        [TestMethod]
        public void WritingCharEscaping()
        {
            List<IRdfReader> readers = new List<IRdfReader>()
            {
                new TurtleParser(),
                new Notation3Parser()
            };
            List<IRdfWriter> writers = new List<IRdfWriter>() 
            {
                new CompressingTurtleWriter(WriterCompressionLevel.High),
                new Notation3Writer()
            };

            for (int i = 0; i < readers.Count; i++)
            {
                IRdfReader parser = readers[i];
                IRdfWriter writer = writers[i];

                foreach (String sample in samples)
                {
                    Graph g = new Graph();
                    Console.WriteLine("Original RDF Fragment");
                    Console.WriteLine(prefix + sample);
                    StringParser.Parse(g, prefix + sample, parser);
                    Console.WriteLine();
                    Console.WriteLine("Triples in Original");
                    foreach (Triple t in g.Triples)
                    {
                        Console.WriteLine(t.ToString());
                    }
                    Console.WriteLine();

                    String serialized = VDS.RDF.Writing.StringWriter.Write(g, writer);
                    Console.WriteLine("Serialized RDF Fragment");
                    Console.WriteLine(serialized);

                    Graph h = new Graph();
                    StringParser.Parse(h, serialized, parser);

                    Console.WriteLine();
                    Console.WriteLine("Triples in Serialized");
                    foreach (Triple t in g.Triples)
                    {
                        Console.WriteLine(t.ToString());
                    }
                    Console.WriteLine();

                    Assert.AreEqual(g, h, "Graphs should have been equal");

                    Console.WriteLine("Graphs were equal as expected");
                    Console.WriteLine();
                }
            }
        }

        [TestMethod]
        public void WritingNTriplesCharEscaping()
        {
            TurtleParser parser = new TurtleParser();
            NTriplesParser ntparser = new NTriplesParser();
            NTriplesWriter ntwriter = new NTriplesWriter();

            foreach (String sample in samples)
            {
                Graph g = new Graph();
                Console.WriteLine("Original RDF Fragment");
                Console.WriteLine(prefix + sample);
                StringParser.Parse(g, prefix + sample, parser);
                Console.WriteLine();
                Console.WriteLine("Triples in Original");
                foreach (Triple t in g.Triples)
                {
                    Console.WriteLine(t.ToString());
                }
                Console.WriteLine();

                String serialized = VDS.RDF.Writing.StringWriter.Write(g, ntwriter);
                Console.WriteLine("Serialized RDF Fragment");
                Console.WriteLine(serialized);

                Graph h = new Graph();
                StringParser.Parse(h, serialized, ntparser);

                Console.WriteLine();
                Console.WriteLine("Triples in Serialized");
                foreach (Triple t in g.Triples)
                {
                    Console.WriteLine(t.ToString());
                }
                Console.WriteLine();

                Assert.AreEqual(g, h, "Graphs should have been equal");

                Console.WriteLine("Graphs were equal as expected");
                Console.WriteLine();
            }
        }

        [TestMethod]
        public void WritingOwlCharEscaping()
        {
            Graph g = new Graph();
            FileLoader.Load(g, "charescaping.owl");

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

            Assert.AreEqual(g, h, "Graphs should have been equal");
        }

        [TestMethod]
        public void WritingHtmlWriter()
        {
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");

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

            Assert.AreEqual(g, h, "Graphs should have been the same");
        }

        [TestMethod]
        public void WritingCollections()
        {
            Graph g = new Graph();
            Options.UriLoaderCaching = false;
            UriLoader.Load(g, new Uri("http://www.wurvoc.org/vocabularies/om-1.6/Kelvin_scale"));

            CompressingTurtleWriter ttlwriter = new CompressingTurtleWriter(WriterCompressionLevel.High);
            ttlwriter.Save(g, Console.Out);
        }

        [TestMethod]
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
                Assert.AreEqual(outputs[i], WriterHelper.EncodeForXml(inputs[i]), "Ampersands should have been encoded correctly");
            }
        }

        [TestMethod]
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
                new CompressingTurtleWriter(),
                new PrettyRdfXmlWriter(),
                new HtmlWriter(),
                new Notation3Writer(),
                new NTriplesWriter(),
                new PrettyRdfXmlWriter(),
                new RdfJsonWriter(),
                new RdfXmlWriter(),
                new TurtleWriter()
            };
            List<IRdfReader> parsers = new List<IRdfReader>()
            {
                new TurtleParser(),
                new RdfXmlParser(),
                new RdfAParser(),
                new Notation3Parser(),
                new NTriplesParser(),
                new RdfXmlParser(),
                new RdfJsonParser(),
                new RdfXmlParser(),
                new TurtleParser()
            };

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

                Assert.AreEqual(g, h, "Graphs should be equal");
                if (h.NamespaceMap.HasNamespace("ex"))
                {
                    Assert.AreEqual(g.NamespaceMap.GetNamespaceUri("ex"), h.NamespaceMap.GetNamespaceUri("ex"), "Namespaces ex should have equivalent URIs");
                }
                if (h.BaseUri != null)
                {
                    Assert.AreEqual(g.BaseUri, h.BaseUri, "Base URIs should be equal");
                }
            }
        }

        [TestMethod]
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
    }
}
