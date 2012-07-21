/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
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
            new RdfXmlWriter(WriterCompressionLevel.High, false),
            new FastRdfXmlWriter(WriterCompressionLevel.High),
            new FastRdfXmlWriter(WriterCompressionLevel.High, false),
            new PrettyRdfXmlWriter(WriterCompressionLevel.High),
            new PrettyRdfXmlWriter(WriterCompressionLevel.High, false),
            new PrettyRdfXmlWriter(WriterCompressionLevel.High, true, false)
        };

        private IRdfReader _parser = new RdfXmlParser();
        private NTriplesFormatter _formatter = new NTriplesFormatter();

        private void CheckRoundTrip(IGraph g, IEnumerable<Type> exceptions)
        {
            //Console.WriteLine("Original Triples:");
            //foreach (Triple t in g.Triples)
            //{
            //    Console.WriteLine(t.ToString(this._formatter));
            //}
            //Console.WriteLine();

            foreach (IRdfWriter writer in this._writers)
            {
                Console.WriteLine("Checking round trip with " + writer.GetType().Name);
                System.IO.StringWriter strWriter = new System.IO.StringWriter();
                writer.Save(g, strWriter);
                //Console.WriteLine(strWriter.ToString());
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
                        Assert.AreEqual(g, h, "Graphs should be equal [" + writer.GetType().Name + "]");
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
                if (failed) Assert.Fail(writer.GetType().Name + " produced output when failure was expected");
                Console.WriteLine();
            }
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
            TurtleParser parser = new TurtleParser();
            parser.Load(new PagingHandler(new GraphHandler(g), 1000), "chado-in-owl.ttl");

            this.CheckRoundTrip(g);
        }

        [TestMethod]
        public void WritingRdfXmlWithDtds()
        {
            String fragment = "@prefix xsd: <" + NamespaceMapper.XMLSCHEMA + ">. @prefix : <http://example.org/>. :subj a :obj ; :has \"string\"^^xsd:string ; :has 23 .";
            Graph g = new Graph();
            g.LoadFromString(fragment);

            this.CheckRoundTrip(g);
        }

        [TestMethod]
        public void WritingRdfXmlInvalidPredicates1()
        {
            String fragment = "@prefix ex: <http://example.org/>. ex:subj ex:123 ex:object .";
            Graph g = new Graph();
            g.LoadFromString(fragment);

            this.CheckFailure(g);
        }       
    }
}
