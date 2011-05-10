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

namespace VDS.RDF.Test.Writing
{
    [TestClass]
    public class RdfXmlWriterTests
    {
        private List<IRdfWriter> _writers = new List<IRdfWriter>()
        {
            new RdfXmlWriter(WriterCompressionLevel.High),
            new FastRdfXmlWriter(WriterCompressionLevel.High),
            new PrettyRdfXmlWriter(WriterCompressionLevel.High)
        };

        private IRdfReader _parser = new RdfXmlParser();
        private NTriplesFormatter _formatter = new NTriplesFormatter();

        private void CheckRoundTrip(IGraph g, IEnumerable<Type> exceptions)
        {
            Console.WriteLine("Original Triples:");
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString(this._formatter));
            }
            Console.WriteLine();

            foreach (IRdfWriter writer in this._writers)
            {
                Console.WriteLine("Checking round trip with " + writer.GetType().Name);
                System.IO.StringWriter strWriter = new System.IO.StringWriter();
                writer.Save(g, strWriter);
                Console.WriteLine(strWriter.ToString());
                Console.WriteLine();

                Graph h = new Graph();
                this._parser.Load(h, new StringReader(strWriter.ToString()));
                Console.WriteLine("Parsed Triples:");
                foreach (Triple t in h.Triples)
                {
                    Console.WriteLine(t.ToString(this._formatter));
                }
                Console.WriteLine();

                if (exceptions.Contains(writer.GetType()))
                {
                    Console.WriteLine("Graph Equality test was skipped");
                }
                else
                {
                    Assert.AreEqual(g, h, "Graphs should be equal [" + writer.GetType().Name + "]");
                    Console.WriteLine("Graphs are equal");
                }
            }
        }

        private void CheckRoundTrip(IGraph g)
        {
            this.CheckRoundTrip(g, Enumerable.Empty<Type>());
        }

        [TestMethod]
        public void WritingRdfXmlLiteralsWithLanguageTags()
        {
            Graph g = new Graph();
            INode s = g.CreateUriNode(new Uri("http://example.org/subject"));
            INode p = g.CreateUriNode(new Uri("http://example.org/predicate"));
            INode o = g.CreateLiteralNode("string", "en");
            g.Assert(s, p, o);

            this.CheckRoundTrip(g);
        }

        [TestMethod]
        public void WritingRdfXmlLiteralsWithReservedCharacters()
        {
            Graph g = new Graph();
            INode s = g.CreateUriNode(new Uri("http://example.org/subject"));
            INode p = g.CreateUriNode(new Uri("http://example.org/predicate"));
            INode o = g.CreateLiteralNode("<tag>");
            g.Assert(s, p, o);

            this.CheckRoundTrip(g);
        }

        [TestMethod]
        public void WritingRdfXmlLiteralsWithReservedCharacters2()
        {
            Graph g = new Graph();
            INode s = g.CreateUriNode(new Uri("http://example.org/subject"));
            INode p = g.CreateUriNode(new Uri("http://example.org/predicate"));
            INode o = g.CreateLiteralNode("&lt;tag>");
            g.Assert(s, p, o);

            this.CheckRoundTrip(g, new Type[] { typeof(FastRdfXmlWriter) });
        }

        [TestMethod]
        public void WritingRdfXmlLiteralsWithReservedCharacters3()
        {
            Graph g = new Graph();
            INode s = g.CreateUriNode(new Uri("http://example.org/subject"));
            INode p = g.CreateUriNode(new Uri("http://example.org/predicate"));
            INode o = g.CreateLiteralNode("string", new Uri("http://example.org/object?a=b&c=d"));
            g.Assert(s, p, o);

            this.CheckRoundTrip(g);
        }

        [TestMethod]
        public void WritingRdfXmlLiterals()
        {
            Graph g = new Graph();
            INode s = g.CreateUriNode(new Uri("http://example.org/subject"));
            INode p = g.CreateUriNode(new Uri("http://example.org/predicate"));
            INode o = g.CreateLiteralNode("<tag />", new Uri(RdfSpecsHelper.RdfXmlLiteral));
            g.Assert(s, p, o);

            this.CheckRoundTrip(g);
        }

        [TestMethod]
        public void WritingRdfXmlLiterals2()
        {
            Graph g = new Graph();
            INode s = g.CreateUriNode(new Uri("http://example.org/subject"));
            INode p = g.CreateUriNode(new Uri("http://example.org/predicate"));
            INode o = g.CreateLiteralNode("<tag>this &amp; that</tag>", new Uri(RdfSpecsHelper.RdfXmlLiteral));
            g.Assert(s, p, o);

            this.CheckRoundTrip(g);
        }

        [TestMethod]
        public void WritingRdfXmlUrisWithReservedCharacters()
        {
            Graph g = new Graph();
            INode s = g.CreateUriNode(new Uri("http://example.org/subject"));
            INode p = g.CreateUriNode(new Uri("http://example.org/predicate"));
            INode o = g.CreateUriNode(new Uri("http://example.org/object?a=b&c=d"));
            g.Assert(s, p, o);

            this.CheckRoundTrip(g);
        }

        [TestMethod]
        public void WritingRdfXmlBNodes()
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

        [TestMethod]
        public void WritingRdfXmlSimpleBNodeCollection()
        {
            String fragment = "@prefix : <http://example.org/>. :subj :pred [ :something :else ].";

            Graph g = new Graph();
            g.LoadFromString(fragment);

            this.CheckRoundTrip(g);
        }

        [TestMethod]
        public void WritingRdfXmlSimpleBNodeCollection2()
        {
            String fragment = "@prefix : <http://example.org/>. :subj :pred [ :something :else ; :another :thing ].";

            Graph g = new Graph();
            g.LoadFromString(fragment);

            this.CheckRoundTrip(g);
        }

        [TestMethod]
        public void WritingRdfXmlSimpleBNodeCollection3()
        {
            String fragment = "@prefix : <http://example.org/>. :subj :pred [ a :BNode ; :another :thing ].";

            Graph g = new Graph();
            g.LoadFromString(fragment);

            this.CheckRoundTrip(g);
        }

        [TestMethod]
        public void WritingRdfXmlSimpleCollection()
        {
            String fragment = "@prefix : <http://example.org/>. :subj :pred ( 1 2 3 ).";

            Graph g = new Graph();
            g.LoadFromString(fragment);

            this.CheckRoundTrip(g);
        }

        [TestMethod]
        public void WritingRdfXmlComplex()
        {
            Graph g = new Graph();
            g.LoadFromFile("chado-in-owl.ttl");

            this.CheckRoundTrip(g);
        }
    }
}
