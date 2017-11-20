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
    {
        [Fact]
        public void WritingCollectionCompressionEmpty1()
        {
            Graph g = new Graph();
            g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
            INode n = g.CreateBlankNode();

            g.Assert(g.CreateUriNode("ex:subj"), g.CreateUriNode("ex:pred"), n);

            CompressingTurtleWriterContext context = new CompressingTurtleWriterContext(g, Console.Out);
            WriterHelper.FindCollections(context);

            Assert.Single(context.Collections);
            Assert.Empty(context.Collections.First().Value.Triples);

            this.CheckCompressionRoundTrip(g);
        }

        [Fact]
        public void WritingCollectionCompressionEmpty2()
        {
            Graph g = new Graph();
            g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
            INode rdfType = g.CreateUriNode("rdf:type");

            g.Assert(g.CreateUriNode("ex:subj"), g.CreateUriNode("ex:pred"), g.CreateUriNode("rdf:nil"));

            CompressingTurtleWriterContext context = new CompressingTurtleWriterContext(g, Console.Out);
            WriterHelper.FindCollections(context);

            Assert.Empty(context.Collections);

            this.CheckCompressionRoundTrip(g);
        }

        [Fact]
        public void WritingCollectionCompressionSimple1()
        {
            Graph g = new Graph();
            g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
            INode n = g.CreateBlankNode();
            INode rdfType = g.CreateUriNode("rdf:type");

            g.Assert(g.CreateUriNode("ex:subj"), g.CreateUriNode("ex:pred"), n);
            g.Assert(n, rdfType, g.CreateUriNode("ex:BlankNode"));

            CompressingTurtleWriterContext context = new CompressingTurtleWriterContext(g, Console.Out);
            WriterHelper.FindCollections(context);

            Assert.Single(context.Collections);
            Assert.Single(context.Collections.First().Value.Triples);

            this.CheckCompressionRoundTrip(g);
        }

        [Fact]
        public void WritingCollectionCompressionSimple2()
        {
            Graph g = new Graph();
            g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
            INode n = g.CreateBlankNode();
            INode rdfType = g.CreateUriNode("rdf:type");

            g.Assert(g.CreateUriNode("ex:subj"), g.CreateUriNode("ex:pred"), n);
            g.Assert(g.CreateUriNode("ex:subj"), g.CreateUriNode("ex:pred2"), n);
            g.Assert(n, rdfType, g.CreateUriNode("ex:BlankNode"));

            CompressingTurtleWriterContext context = new CompressingTurtleWriterContext(g, Console.Out);
            WriterHelper.FindCollections(context);

            Assert.Empty(context.Collections);

            this.CheckCompressionRoundTrip(g);
        }

        [Fact]
        public void WritingCollectionCompressionSimple3()
        {
            Graph g = new Graph();
            g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
            INode n = g.CreateBlankNode();
            INode rdfType = g.CreateUriNode("rdf:type");
            INode rdfFirst = g.CreateUriNode("rdf:first");
            INode rdfRest = g.CreateUriNode("rdf:rest");
            INode rdfNil = g.CreateUriNode("rdf:nil");

            g.Assert(g.CreateUriNode("ex:subj"), g.CreateUriNode("ex:pred"), n);
            g.Assert(n, rdfFirst, g.CreateLiteralNode("first"));
            g.Assert(n, rdfRest, rdfNil);

            CompressingTurtleWriterContext context = new CompressingTurtleWriterContext(g, Console.Out);
            WriterHelper.FindCollections(context);

            Assert.Single(context.Collections);

            this.CheckCompressionRoundTrip(g);
        }

        [Fact]
        public void WritingCollectionCompressionSimple4()
        {
            Graph g = new Graph();
            g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
            INode n = g.CreateBlankNode();
            INode rdfType = g.CreateUriNode("rdf:type");
            INode rdfFirst = g.CreateUriNode("rdf:first");
            INode rdfRest = g.CreateUriNode("rdf:rest");
            INode rdfNil = g.CreateUriNode("rdf:nil");

            g.Assert(n, rdfFirst, g.CreateLiteralNode("first"));
            g.Assert(n, rdfRest, rdfNil);

            CompressingTurtleWriterContext context = new CompressingTurtleWriterContext(g, Console.Out);
            WriterHelper.FindCollections(context);

            Assert.Empty(context.Collections);

            this.CheckCompressionRoundTrip(g);
        }

        [Fact]
        public void WritingCollectionCompressionSimple5()
        {
            Graph g = new Graph();
            g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
            INode n = g.CreateBlankNode();
            INode rdfType = g.CreateUriNode("rdf:type");
            INode rdfFirst = g.CreateUriNode("rdf:first");
            INode rdfRest = g.CreateUriNode("rdf:rest");
            INode rdfNil = g.CreateUriNode("rdf:nil");

            g.Assert(g.CreateUriNode("ex:subj"), g.CreateUriNode("ex:pred"), n);
            g.Assert(g.CreateUriNode("ex:subj"), g.CreateUriNode("ex:pred2"), n);
            g.Assert(n, rdfFirst, g.CreateLiteralNode("first"));
            g.Assert(n, rdfRest, rdfNil);

            CompressingTurtleWriterContext context = new CompressingTurtleWriterContext(g, Console.Out);
            WriterHelper.FindCollections(context);

            Assert.Empty(context.Collections);

            this.CheckCompressionRoundTrip(g);
        }

        [Fact]
        public void WritingCollectionCompressionSimple6()
        {
            Graph g = new Graph();
            g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
            INode n = g.CreateBlankNode();
            INode rdfType = g.CreateUriNode("rdf:type");

            g.Assert(n, rdfType, g.CreateUriNode("ex:Obj"));

            CompressingTurtleWriterContext context = new CompressingTurtleWriterContext(g, Console.Out);
            WriterHelper.FindCollections(context);

            Assert.Single(context.Collections);
            Assert.Empty(context.Collections.First().Value.Triples);

            this.CheckCompressionRoundTrip(g);
        }

        [Fact]
        public void WritingCollectionCompressionComplex1()
        {
            SparqlConnector connector = new SparqlConnector(new VDS.RDF.Query.SparqlRemoteEndpoint(new Uri("http://dbpedia.org/sparql")));
            Graph g = new Graph();
            g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
            g.NamespaceMap.AddNamespace("dnr", new Uri(ConfigurationLoader.ConfigurationNamespace));
            INode n = g.CreateBlankNode();

            g.Assert(g.CreateUriNode("ex:subj"), g.CreateUriNode("dnr:genericManager"), n);
            ConfigurationSerializationContext sContext = new ConfigurationSerializationContext(g);
            sContext.NextSubject = n;
            connector.SerializeConfiguration(sContext);

            CompressingTurtleWriterContext context = new CompressingTurtleWriterContext(g, Console.Out);
            WriterHelper.FindCollections(context);

            Assert.Equal(2, context.Collections.Count);

            this.CheckCompressionRoundTrip(g);
        }

        [Fact]
        public void WritingCollectionCompressionComplex2()
        {
            Graph g = new Graph();
            g.LoadFromFile("resources\\complex-collections.nt");

            CompressingTurtleWriterContext context = new CompressingTurtleWriterContext(g, Console.Out);
            WriterHelper.FindCollections(context);

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (KeyValuePair<INode, OutputRdfCollection> kvp in context.Collections)
            {
                Console.WriteLine("Collection Root - " + kvp.Key.ToString(formatter));
                Console.WriteLine("Collection Triples (" + kvp.Value.Triples.Count + ")");
                foreach (Triple t in kvp.Value.Triples)
                {
                    Console.WriteLine(t.ToString(formatter));
                }
                Console.WriteLine();
            }

            this.CheckCompressionRoundTrip(g);
        }

        [Fact]
        public void WritingCollectionCompressionNamedListNodes()
        {
            Graph g = new Graph();
            g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
            INode n = g.CreateUriNode("ex:list");
            INode rdfType = g.CreateUriNode("rdf:type");
            INode rdfFirst = g.CreateUriNode("rdf:first");
            INode rdfRest = g.CreateUriNode("rdf:rest");
            INode rdfNil = g.CreateUriNode("rdf:nil");

            g.Assert(g.CreateUriNode("ex:subj"), g.CreateUriNode("ex:pred"), n);
            g.Assert(n, rdfFirst, g.CreateLiteralNode("first"));
            g.Assert(n, rdfRest, rdfNil);

            CompressingTurtleWriterContext context = new CompressingTurtleWriterContext(g, Console.Out);
            WriterHelper.FindCollections(context);

            Assert.Empty(context.Collections);

            this.CheckCompressionRoundTrip(g);
        }

        [Fact]
        public void WritingCollectionCompressionNamedListNodes2()
        {
            Graph g = new Graph();
            g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
            INode n = g.CreateUriNode("ex:listRoot");
            INode m = g.CreateUriNode("ex:listItem");
            INode rdfType = g.CreateUriNode("rdf:type");
            INode rdfFirst = g.CreateUriNode("rdf:first");
            INode rdfRest = g.CreateUriNode("rdf:rest");
            INode rdfNil = g.CreateUriNode("rdf:nil");

            g.Assert(g.CreateUriNode("ex:subj"), g.CreateUriNode("ex:pred"), n);
            g.Assert(n, rdfFirst, g.CreateLiteralNode("first"));
            g.Assert(n, rdfRest, m);
            g.Assert(m, rdfFirst, g.CreateLiteralNode("second"));
            g.Assert(m, rdfRest, rdfNil);

            CompressingTurtleWriterContext context = new CompressingTurtleWriterContext(g, Console.Out);
            WriterHelper.FindCollections(context);

            Assert.Empty(context.Collections);

            this.CheckCompressionRoundTrip(g);
        }
    }
}
