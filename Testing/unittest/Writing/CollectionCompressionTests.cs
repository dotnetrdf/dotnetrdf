using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Contexts;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Writing
{
    [TestClass]
    public class CollectionCompressionTests
    {
        List<KeyValuePair<IRdfWriter, IRdfReader>> _compressers = new List<KeyValuePair<IRdfWriter, IRdfReader>>()
        {
            new KeyValuePair<IRdfWriter, IRdfReader>(new CompressingTurtleWriter(), new TurtleParser()),
            new KeyValuePair<IRdfWriter, IRdfReader>(new FastRdfXmlWriter(), new RdfXmlParser()),
            new KeyValuePair<IRdfWriter, IRdfReader>(new Notation3Writer(), new Notation3Parser()),
            //new KeyValuePair<IRdfWriter, IRdfReader>(new RdfXmlTreeWriter(), new RdfXmlParser()),
            new KeyValuePair<IRdfWriter, IRdfReader>(new RdfXmlWriter(), new RdfXmlParser())
        };

        private void CheckCompressionRoundTrip(IGraph g)
        {
            foreach (KeyValuePair<IRdfWriter, IRdfReader> kvp in this._compressers)
            {

                IRdfWriter writer = kvp.Key;
                if (writer is ICompressingWriter)
                {
                    ((ICompressingWriter)writer).CompressionLevel = WriterCompressionLevel.High;
                }
                if (writer is IHighSpeedWriter)
                {
                    ((IHighSpeedWriter)writer).HighSpeedModePermitted = false;
                }
                System.IO.StringWriter strWriter = new System.IO.StringWriter();
                writer.Save(g, strWriter);

                Console.WriteLine("Compressed Output using " + kvp.Key.GetType().Name);
                Console.WriteLine(strWriter.ToString());
                Console.WriteLine();

                Graph h = new Graph();
                StringParser.Parse(h, strWriter.ToString(), kvp.Value);

                Assert.AreEqual(g, h, "Graphs should be equal after round trip to and from serialization using " + kvp.Key.GetType().Name);
            }
        }

        [TestMethod]
        public void WritingCollectionCompressionEmpty1()
        {
            Graph g = new Graph();
            g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
            INode n = g.CreateBlankNode();
            INode rdfType = g.CreateUriNode("rdf:type");

            g.Assert(g.CreateUriNode("ex:subj"), g.CreateUriNode("ex:pred"), n);

            CompressingTurtleWriterContext context = new CompressingTurtleWriterContext(g, Console.Out);
            WriterHelper.FindCollections(context);

            Assert.AreEqual(1, context.Collections.Count, "Expected 1 Collection to be found");
            Assert.AreEqual(0, context.Collections.First().Value.Triples.Count, "Expected no Triples to be in the collection");

            this.CheckCompressionRoundTrip(g);
        }

        [TestMethod]
        public void WritingCollectionCompressionEmpty2()
        {
            Graph g = new Graph();
            g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
            INode rdfType = g.CreateUriNode("rdf:type");

            g.Assert(g.CreateUriNode("ex:subj"), g.CreateUriNode("ex:pred"), g.CreateUriNode("rdf:nil"));

            CompressingTurtleWriterContext context = new CompressingTurtleWriterContext(g, Console.Out);
            WriterHelper.FindCollections(context);

            Assert.AreEqual(0, context.Collections.Count, "Expected 0 Collection to be found");

            this.CheckCompressionRoundTrip(g);
        }

        [TestMethod]
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

            Assert.AreEqual(1, context.Collections.Count, "Expected 1 Collection to be found");
            Assert.AreEqual(1, context.Collections.First().Value.Triples.Count, "Expected 1 Triple to be in the collection");

            this.CheckCompressionRoundTrip(g);
        }

        [TestMethod]
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

            Assert.AreEqual(0, context.Collections.Count, "Expected no Collection to be found");

            this.CheckCompressionRoundTrip(g);
        }

        [TestMethod]
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

            Assert.AreEqual(1, context.Collections.Count, "Expected 1 collection to be found");

            this.CheckCompressionRoundTrip(g);
        }

        [TestMethod]
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

            Assert.AreEqual(0, context.Collections.Count, "Expected no collections to be found");

            this.CheckCompressionRoundTrip(g);
        }

        [TestMethod]
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

            Assert.AreEqual(0, context.Collections.Count, "Expected no collections to be found");

            this.CheckCompressionRoundTrip(g);
        }

        [TestMethod]
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

            Assert.AreEqual(2, context.Collections.Count, "Expected 2 collections");

            this.CheckCompressionRoundTrip(g);
        }

        [TestMethod]
        public void WritingCollectionCompressionComplex2()
        {
            Graph g = new Graph();
            g.LoadFromFile("complex-collections.nt");

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

        [TestMethod]
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

            Assert.AreEqual(0, context.Collections.Count, "Expected no collections to be found");

            this.CheckCompressionRoundTrip(g);
        }

        [TestMethod]
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

            Assert.AreEqual(0, context.Collections.Count, "Expected no collections to be found");

            this.CheckCompressionRoundTrip(g);
        }

        [TestMethod]
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

            Assert.AreEqual(g, h, "Graphs should be equal");
        }

        [TestMethod]
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

            Assert.AreEqual(2, context.Collections.Count, "Expected 2 collections (one should be eliminated to break the cycle)");

            this.CheckCompressionRoundTrip(g);
        }

        [TestMethod]
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

            Assert.AreEqual(2, context.Collections.Count, "Expected 2 collections (one should be eliminated to break the cycle)");

            this.CheckCompressionRoundTrip(g);
        }

        [TestMethod]
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

            Assert.AreEqual(3, context.Collections.Count, "Expected 3 collections (one should be eliminated to break the cycle)");

            this.CheckCompressionRoundTrip(g);
        }
    }
}
