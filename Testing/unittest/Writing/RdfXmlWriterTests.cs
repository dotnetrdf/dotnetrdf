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
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing
{
    public partial class RdfXmlWriterTests
    {
        private readonly List<IRdfWriter> _writers = new List<IRdfWriter>()
        {
            new RdfXmlWriter(WriterCompressionLevel.High),
            new RdfXmlWriter(WriterCompressionLevel.High, false),
            new PrettyRdfXmlWriter(WriterCompressionLevel.High),
            new PrettyRdfXmlWriter(WriterCompressionLevel.High, false),
            new PrettyRdfXmlWriter(WriterCompressionLevel.High, true, false)
        };

        private readonly IRdfReader _parser = new RdfXmlParser();
        private readonly NTriplesFormatter _formatter = new NTriplesFormatter();

        private void CheckRoundTrip(IGraph g, IEnumerable<Type> exceptions)
        {
            foreach (IRdfWriter writer in this._writers)
            {
                Console.WriteLine("Checking round trip with " + writer.GetType().Name);
                System.IO.StringWriter strWriter = new System.IO.StringWriter();
                writer.Save(g, strWriter);
                Console.WriteLine();

                Graph h = new Graph();
                try
                {
                    this._parser.Load(h, new StringReader(strWriter.ToString()));
                    Console.WriteLine("Output was parsed OK");
                }
                catch
                {
                    Console.WriteLine("Invalid Output:");
                    Console.WriteLine(strWriter.ToString());
                    throw;
                }
                Console.WriteLine();

                if (exceptions.Contains(writer.GetType()))
                {
                    Console.WriteLine("Graph Equality test was skipped");
                }
                else
                {
                    try
                    {
                        Assert.Equal(g, h);
                    }
                    catch
                    {
                        Console.WriteLine("Output did not round trip:");
                        Console.WriteLine(strWriter.ToString());
                        throw;
                    }
                    Console.WriteLine("Graphs are equal");
                }
            }
        }

        private void CheckRoundTrip(IGraph g)
        {
            this.CheckRoundTrip(g, Enumerable.Empty<Type>());
        }

        private void CheckFailure(IGraph g)
        {
            Console.WriteLine("Original Triples:");
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString(this._formatter));
            }
            Console.WriteLine();

            foreach (IRdfWriter writer in this._writers)
            {
                Console.WriteLine("Checking for Failure with " + writer.GetType().Name);
                bool failed = false;
                try
                {
                    System.IO.StringWriter sw = new System.IO.StringWriter();
                    writer.Save(g, sw);

                    Console.WriteLine("Produced Output when failure was expected:");
                    Console.WriteLine(sw.ToString());

                    failed = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed as expected - " + ex.Message);
                }
                if (failed) Assert.True(false, writer.GetType().Name + " produced output when failure was expected");
                Console.WriteLine();
            }
        }

        [Fact]
        public void WritingRdfXmlLiteralsWithLanguageTags()
        {
            Graph g = new Graph();
            INode s = g.CreateUriNode(new Uri("http://example.org/subject"));
            INode p = g.CreateUriNode(new Uri("http://example.org/predicate"));
            INode o = g.CreateLiteralNode("string", "en");
            g.Assert(s, p, o);

            this.CheckRoundTrip(g);
        }

        [Fact]
        public void WritingRdfXmlLiteralsWithReservedCharacters()
        {
            Graph g = new Graph();
            INode s = g.CreateUriNode(new Uri("http://example.org/subject"));
            INode p = g.CreateUriNode(new Uri("http://example.org/predicate"));
            INode o = g.CreateLiteralNode("<tag>");
            g.Assert(s, p, o);

            this.CheckRoundTrip(g);
        }

        [Fact]
        public void WritingRdfXmlLiteralsWithReservedCharacters2()
        {
            Graph g = new Graph();
            INode s = g.CreateUriNode(new Uri("http://example.org/subject"));
            INode p = g.CreateUriNode(new Uri("http://example.org/predicate"));
            INode o = g.CreateLiteralNode("&lt;tag>");
            g.Assert(s, p, o);

            this.CheckRoundTrip(g, new Type[] { typeof(PrettyRdfXmlWriter) });
        }

        [Fact]
        public void WritingRdfXmlLiteralsWithReservedCharacters3()
        {
            Graph g = new Graph();
            INode s = g.CreateUriNode(new Uri("http://example.org/subject"));
            INode p = g.CreateUriNode(new Uri("http://example.org/predicate"));
            INode o = g.CreateLiteralNode("string", new Uri("http://example.org/object?a=b&c=d"));
            g.Assert(s, p, o);

            this.CheckRoundTrip(g);
        }

        [Fact]
        public void WritingRdfXmlLiterals()
        {
            Graph g = new Graph();
            INode s = g.CreateUriNode(new Uri("http://example.org/subject"));
            INode p = g.CreateUriNode(new Uri("http://example.org/predicate"));
            INode o = g.CreateLiteralNode("<tag />", new Uri(RdfSpecsHelper.RdfXmlLiteral));
            g.Assert(s, p, o);

            this.CheckRoundTrip(g);
        }

        [Fact]
        public void WritingRdfXmlLiterals2()
        {
            Graph g = new Graph();
            INode s = g.CreateUriNode(new Uri("http://example.org/subject"));
            INode p = g.CreateUriNode(new Uri("http://example.org/predicate"));
            INode o = g.CreateLiteralNode("<tag>this &amp; that</tag>", new Uri(RdfSpecsHelper.RdfXmlLiteral));
            g.Assert(s, p, o);

            this.CheckRoundTrip(g);
        }

        [Fact]
        public void WritingRdfXmlUrisWithReservedCharacters()
        {
            Graph g = new Graph();
            INode s = g.CreateUriNode(new Uri("http://example.org/subject"));
            INode p = g.CreateUriNode(new Uri("http://example.org/predicate"));
            INode o = g.CreateUriNode(new Uri("http://example.org/object?a=b&c=d"));
            g.Assert(s, p, o);

            this.CheckRoundTrip(g);
        }

        [Fact]
        public void WritingRdfXmlBNodes1()
        {
            Graph g = new Graph();
            INode s = g.CreateUriNode(new Uri("http://example.org/subject"));
            INode p = g.CreateUriNode(new Uri("http://example.org/predicate"));
            INode o = g.CreateBlankNode();
            g.Assert(s, p, o);

            s = o;
            p = g.CreateUriNode(new Uri("http://example.org/nextPredicate"));
            o = g.CreateLiteralNode("string");

            g.Assert(s, p, o);

            this.CheckRoundTrip(g);
        }

        [Fact]
        public void WritingRdfXmlBNodes2()
        {
            String data = "@prefix : <http://example.org/>. [a :bNode ; :connectsTo [a :bNode ; :connectsTo []]] a [] .";
            Graph g = new Graph();
            g.LoadFromString(data, new TurtleParser());

            this.CheckRoundTrip(g);
        }

        [Fact]
        public void WritingRdfXmlSimpleBNodeCollection()
        {
            String fragment = "@prefix : <http://example.org/>. :subj :pred [ :something :else ].";

            Graph g = new Graph();
            g.LoadFromString(fragment);

            this.CheckRoundTrip(g);
        }

        [Fact]
        public void WritingRdfXmlSimpleBNodeCollection2()
        {
            String fragment = "@prefix : <http://example.org/>. :subj :pred [ :something :else ; :another :thing ].";

            Graph g = new Graph();
            g.LoadFromString(fragment);

            this.CheckRoundTrip(g);
        }

        [Fact]
        public void WritingRdfXmlSimpleBNodeCollection3()
        {
            String fragment = "@prefix : <http://example.org/>. :subj :pred [ a :BNode ; :another :thing ].";

            Graph g = new Graph();
            g.LoadFromString(fragment);

            this.CheckRoundTrip(g);
        }

        [Fact]
        public void WritingRdfXmlInvalidPredicates1()
        {
            String fragment = "@prefix ex: <http://example.org/>. ex:subj ex:123 ex:object .";
            Graph g = new Graph();
            g.LoadFromString(fragment);

            this.CheckFailure(g);
        }

        [Fact]
        public void WritingRdfXmlPrettySubjectCollection1()
        {
            String graph = @"@prefix ex: <http://example.com/>. (1) ex:someProp ""Value"".";
            Graph g = new Graph();
            g.LoadFromString(graph, new TurtleParser());

            PrettyRdfXmlWriter writer = new PrettyRdfXmlWriter();
            System.IO.StringWriter strWriter = new System.IO.StringWriter();
            writer.Save(g, strWriter);

            Console.WriteLine(strWriter.ToString());
            Console.WriteLine();

            Graph h = new Graph();
            h.LoadFromString(strWriter.ToString(), new RdfXmlParser());

            Assert.Equal(g, h);
        }

        [Fact]
        public void WritingRdfXmlEntityCompactionLeadingDigits()
        {
            const String data = "@prefix ex: <http://example.org/> . ex:1s ex:p ex:2o .";
            Graph g = new Graph();
            g.LoadFromString(data, new TurtleParser());

            RdfXmlWriter writer = new RdfXmlWriter();
            writer.CompressionLevel = WriterCompressionLevel.High;
            String outData = StringWriter.Write(g, writer);

            Assert.Contains("rdf:about=\"&ex;1s\"", outData);
            Assert.Contains("rdf:resource=\"&ex;2o\"", outData);
        }

        /// <summary>
        /// Reproduces issue #243
        /// </summary>
        [Fact]
        public void RdfXmlEntityEscapesApostrophes()
        {
            const string data =
                "@prefix ex: <http://dbpedia.org/resource/Buyer's_Remorse:> . ex:s <http://example.org/p> <http://example.org/o> .";
            var g = new Graph();
            g.LoadFromString(data, new TurtleParser());

            var writer = new RdfXmlWriter {CompressionLevel = WriterCompressionLevel.High};
            var outData = StringWriter.Write(g, writer);
            Assert.Contains("<!ENTITY ex 'http://dbpedia.org/resource/Buyer&apos;s_Remorse:'>", outData);
            Assert.Contains("xmlns:ex=\"http://dbpedia.org/resource/Buyer&apos;s_Remorse:\"", outData);
            Assert.Contains("rdf:about=\"&ex;s", outData);
        }
    }
}
