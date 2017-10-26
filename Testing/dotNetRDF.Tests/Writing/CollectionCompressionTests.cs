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
using System.Linq;
using System.Text;
using Xunit;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Contexts;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing
{
    public partial class CollectionCompressionTests
        : CompressionTests
    {
        [Fact(Skip = "Commented out before, now fails (?)")]
        public void WritingCollectionCompressionSimple7()
        {
            Graph g = new Graph();
            g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
            INode n = g.CreateBlankNode();
            INode rdfType = g.CreateUriNode("rdf:type");

            g.Assert(n, rdfType, g.CreateUriNode("ex:Obj"));
            g.Assert(n, rdfType, g.CreateUriNode("ex:Test"));

            CompressingTurtleWriterContext context = new CompressingTurtleWriterContext(g, Console.Out);
            WriterHelper.FindCollections(context);

            Assert.Equal(1, context.Collections.Count);
            Assert.Equal(1, context.Collections.First().Value.Triples.Count);

            this.CheckCompressionRoundTrip(g);
        }

        [Fact]
        public void WritingCollectionCompressionNamedListNodes3()
        {
            Graph g = new Graph();
            INode data1 = g.CreateBlankNode();
            g.Assert(data1, g.CreateUriNode(new Uri("http://property")), g.CreateLiteralNode("test1"));
            INode data2 = g.CreateBlankNode();
            g.Assert(data2, g.CreateUriNode(new Uri("http://property")), g.CreateLiteralNode("test2"));

            INode listEntry1 = g.CreateUriNode(new Uri("http://test/1"));
            INode rdfFirst = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfListFirst));
            INode rdfRest = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfListRest));
            INode rdfNil = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfListNil));
            g.Assert(listEntry1, rdfFirst, data1);
            g.Assert(listEntry1, rdfRest, rdfNil);

            INode listEntry2 = g.CreateUriNode(new Uri("http://test/2"));
            g.Assert(listEntry2, rdfFirst, data2);
            g.Assert(listEntry2, rdfRest, listEntry1);

            INode root = g.CreateUriNode(new Uri("http://root"));
            g.Assert(root, g.CreateUriNode(new Uri("http://list")), listEntry2);

            NTriplesFormatter formatter = new NTriplesFormatter();
            Console.WriteLine("Original Graph");
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }
            Console.WriteLine();

            CompressingTurtleWriterContext context = new CompressingTurtleWriterContext(g, Console.Out);
            WriterHelper.FindCollections(context);
            Console.WriteLine(context.Collections.Count + " Collections Found");
            Console.WriteLine();

            System.IO.StringWriter strWriter = new System.IO.StringWriter();
            CompressingTurtleWriter writer = new CompressingTurtleWriter();
            writer.CompressionLevel = WriterCompressionLevel.High;
            writer.Save(g, strWriter);

            Console.WriteLine("Compressed Turtle");
            Console.WriteLine(strWriter.ToString());
            Console.WriteLine();

            Graph h = new Graph();
            TurtleParser parser = new TurtleParser();
            StringParser.Parse(h, strWriter.ToString());
            Console.WriteLine("Graph after Round Trip to Compressed Turtle");
            foreach (Triple t in h.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }

            Assert.Equal(g, h);
        }

        [Fact]
        public void WritingCollectionCompressionCyclic()
        {
            Graph g = new Graph();
            g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
            g.NamespaceMap.AddNamespace("dnr", new Uri(ConfigurationLoader.ConfigurationNamespace));
            INode a = g.CreateBlankNode();
            INode b = g.CreateBlankNode();
            INode c = g.CreateBlankNode();

            INode pred = g.CreateUriNode("ex:pred");

            g.Assert(a, pred, b);
            g.Assert(a, pred, g.CreateLiteralNode("Value for A"));
            g.Assert(b, pred, c);
            g.Assert(b, pred, g.CreateLiteralNode("Value for B"));
            g.Assert(c, pred, a);
            g.Assert(c, pred, g.CreateLiteralNode("Value for C"));

            CompressingTurtleWriterContext context = new CompressingTurtleWriterContext(g, Console.Out);
            WriterHelper.FindCollections(context);

            Assert.Equal(2, context.Collections.Count);

            this.CheckCompressionRoundTrip(g);
        }

        [Fact]
        public void WritingCollectionCompressionCyclic2()
        {
            Graph g = new Graph();
            g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
            g.NamespaceMap.AddNamespace("dnr", new Uri(ConfigurationLoader.ConfigurationNamespace));
            INode a = g.CreateBlankNode();
            INode b = g.CreateBlankNode();
            INode c = g.CreateBlankNode();
            INode d = g.CreateBlankNode();

            INode pred = g.CreateUriNode("ex:pred");

            g.Assert(d, pred, a);
            g.Assert(d, pred, g.CreateLiteralNode("D"));
            g.Assert(a, pred, b);
            g.Assert(a, pred, g.CreateLiteralNode("A"));
            g.Assert(b, pred, c);
            g.Assert(b, pred, g.CreateLiteralNode("B"));
            g.Assert(c, pred, a);
            g.Assert(c, pred, g.CreateLiteralNode("C"));

            CompressingTurtleWriterContext context = new CompressingTurtleWriterContext(g, Console.Out);
            WriterHelper.FindCollections(context);

            Assert.Equal(2, context.Collections.Count);

            this.CheckCompressionRoundTrip(g);
        }

        [Fact]
        public void WritingCollectionCompressionCyclic3()
        {
            Graph g = new Graph();
            g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
            g.NamespaceMap.AddNamespace("dnr", new Uri(ConfigurationLoader.ConfigurationNamespace));
            INode a = g.CreateBlankNode();
            INode b = g.CreateBlankNode();
            INode c = g.CreateBlankNode();
            INode d = g.CreateBlankNode();
            INode e = g.CreateBlankNode();

            INode pred = g.CreateUriNode("ex:pred");

            g.Assert(d, pred, a);
            g.Assert(d, pred, g.CreateLiteralNode("D"));
            g.Assert(a, pred, b);
            g.Assert(a, pred, g.CreateLiteralNode("A"));
            g.Assert(b, pred, c);
            g.Assert(b, pred, g.CreateLiteralNode("B"));
            g.Assert(c, pred, a);
            g.Assert(c, pred, g.CreateLiteralNode("C"));
            g.Assert(e, pred, g.CreateLiteralNode("E"));

            CompressingTurtleWriterContext context = new CompressingTurtleWriterContext(g, Console.Out);
            WriterHelper.FindCollections(context);

            Assert.Equal(3, context.Collections.Count);

            this.CheckCompressionRoundTrip(g);
        }
    }
}
