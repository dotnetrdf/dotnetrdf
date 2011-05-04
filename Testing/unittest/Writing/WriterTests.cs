using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace VDS.RDF.Test.Writing
{
    [TestClass]
    public class WriterTests
    {
        private List<String> samples = new List<string>()
            {
                ":no-escapes :string \"a piece of text with no escapes\".",
                ":quote-escapes :string \"a piece of text containing \\\"quotes\\\"\".",
                ":long-literal :string \"\"\"a piece of text containing \"quotes\\\"\"\"\".",
                ":newline-escapes :string \"this contains a \\nnew line\".",
                ":newline-longliteral-escapes :string \"\"\"this is a long literal\nwhich contains\nmultiple new lines\"\"\".",
                ":newline-escapes-alt :string \"this contains another \\rnew line\".",
                ":newline-longliteral-escapes-alt :string \"\"\"this is another long literal which mixes both\n \\r and \\n\rnew line escapes\"\"\".",
                ":tabs :string \"this string contains   unescaped tabs\".",
                ":tabs-escaped :string \"this string contains \\t escaped tabs\"."
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

                    String serialized = StringWriter.Write(g, writer);
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

                String serialized = StringWriter.Write(g, ntwriter);
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
            String serialized = StringWriter.Write(g, ttlwriter);

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
            String data = StringWriter.Write(g, writer);

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
            Console.WriteLine(StringWriter.Write(g, ttlwriter));
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
                new String('&', 1000)
            };

            List<String> outputs = new List<string>()
            {
                "&amp;value",
                "&amp;",
                "&amp;",
                "&amp;value&amp;next"
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
                Assert.AreEqual(WriterHelper.EncodeForXml(inputs[i]), outputs[i], "Ampersands should have been encoded correctly");
            }
        }
    }
}
