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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Parsing.Handlers
{
    [TestClass]
    public class PagingHandlerTests
    {
        public void ParsingUsingPagingHandler(String tempFile, IRdfReader parser)
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");
            g.SaveToFile(tempFile);

            Graph h = new Graph();
            PagingHandler handler = new PagingHandler(new GraphHandler(h), 25);
            parser.Load(handler, tempFile);
            h.Retract(h.Triples.Where(t => !t.IsGroundTriple).ToList());

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (Triple t in h.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }
            Console.WriteLine();

            Assert.IsFalse(h.IsEmpty, "Graph should not be empty");
            Assert.IsTrue(h.Triples.Count <= 25, "Graphs should have <= 25 Triples");

            Graph i = new Graph();
            handler = new PagingHandler(new GraphHandler(i), 25, 25);
            parser.Load(handler, tempFile);
            i.Retract(i.Triples.Where(t => !t.IsGroundTriple));

            foreach (Triple t in i.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }
            Console.WriteLine();

            Assert.IsFalse(i.IsEmpty, "Graph should not be empty");
            Assert.IsTrue(i.Triples.Count <= 25, "Graphs should have <= 25 Triples");

            GraphDiffReport report = h.Difference(i);
            Assert.IsFalse(report.AreEqual, "Graphs should not be equal");
            Assert.AreEqual(i.Triples.Count, report.AddedTriples.Count(), "Should be " + i.Triples.Count + " added Triples");
            Assert.AreEqual(h.Triples.Count, report.RemovedTriples.Count(), "Should be " + h.Triples.Count + " removed Triples");
        }

        public void ParsingUsingPagingHandler2(String tempFile, IRdfReader parser)
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");
            g.SaveToFile(tempFile);

            Graph h = new Graph();
            PagingHandler handler = new PagingHandler(new GraphHandler(h), 0);

            parser.Load(handler, tempFile);

            Assert.IsTrue(h.IsEmpty, "Graph should be empty");
        }

        public void ParsingUsingPagingHandler3(String tempFile, IRdfReader parser)
        {
            Graph g = new Graph();
            EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");
            g.SaveToFile(tempFile);

            Graph h = new Graph();
            PagingHandler handler = new PagingHandler(new GraphHandler(h), -1, 100);

            parser.Load(handler, tempFile);

            Assert.IsFalse(h.IsEmpty, "Graph should not be empty");
            Assert.AreEqual(g.Triples.Count - 100, h.Triples.Count, "Should have 100 less triples than original graph as first 100 triples are skipped");
        }

        #region These tests take two slices from the graph (0-25) and (26-50) and ensure they are different

        [TestMethod]
        public void ParsingPagingHandlerNTriples()
        {
            this.ParsingUsingPagingHandler("temp.nt", new NTriplesParser());
        }

        [TestMethod]
        public void ParsingPagingHandlerTurtle()
        {
            this.ParsingUsingPagingHandler("temp.ttl", new TurtleParser());
        }

        [TestMethod]
        public void ParsingPagingHandlerNotation3()
        {
            this.ParsingUsingPagingHandler("temp.n3", new Notation3Parser());
        }

#if !NO_XMLENTITIES
        [TestMethod]
        public void ParsingPagingHandlerRdfXml()
        {
            this.ParsingUsingPagingHandler("temp.rdf", new RdfXmlParser());
        }
#endif

#if !NO_HTMLAGILITYPACK
        [TestMethod]
        public void ParsingPagingHandlerRdfA()
        {
            this.ParsingUsingPagingHandler("temp.html", new RdfAParser());
        }
#endif

        [TestMethod]
        public void ParsingPagingHandlerRdfJson()
        {
            this.ParsingUsingPagingHandler("temp.json", new RdfJsonParser());
        }

        #endregion

        #region These tests take 0 triples from the graph and ensure it is empty

        [TestMethod]
        public void ParsingPagingHandlerNTriples2()
        {
            this.ParsingUsingPagingHandler2("temp.nt", new NTriplesParser());
        }

        [TestMethod]
        public void ParsingPagingHandlerTurtle2()
        {
            this.ParsingUsingPagingHandler2("temp.ttl", new TurtleParser());
        }

        [TestMethod]
        public void ParsingPagingHandlerNotation3_2()
        {
            this.ParsingUsingPagingHandler2("temp.n3", new Notation3Parser());
        }

#if !NO_XMLENTITIES
        [TestMethod]
        public void ParsingPagingHandlerRdfXml2()
        {
            this.ParsingUsingPagingHandler2("temp.rdf", new RdfXmlParser());
        }
#endif

#if !NO_HTMLAGILITYPACK
        [TestMethod]
        public void ParsingPagingHandlerRdfA2()
        {
            this.ParsingUsingPagingHandler2("temp.html", new RdfAParser());
        }
#endif

        [TestMethod]
        public void ParsingPagingHandlerRdfJson2()
        {
            this.ParsingUsingPagingHandler2("temp.json", new RdfJsonParser());
        }

        #endregion

        #region These tests discard the first 100 triples and take the rest

        [TestMethod]
        public void ParsingPagingHandlerNTriples3()
        {
            this.ParsingUsingPagingHandler3("temp.nt", new NTriplesParser());
        }

        [TestMethod]
        public void ParsingPagingHandlerTurtle3()
        {
            this.ParsingUsingPagingHandler3("temp.ttl", new TurtleParser());
        }

        [TestMethod]
        public void ParsingPagingHandlerNotation3_3()
        {
            this.ParsingUsingPagingHandler3("temp.n3", new Notation3Parser());
        }

#if !NO_XMLENTITIES
        [TestMethod]
        public void ParsingPagingHandlerRdfXml3()
        {
            this.ParsingUsingPagingHandler3("temp.rdf", new RdfXmlParser());
        }
#endif

#if !NO_HTMLAGILITYPACK
        [TestMethod]
        public void ParsingPagingHandlerRdfA3()
        {
            this.ParsingUsingPagingHandler3("temp.html", new RdfAParser());
        }
#endif

        [TestMethod]
        public void ParsingPagingHandlerRdfJson3()
        {
            this.ParsingUsingPagingHandler3("temp.json", new RdfJsonParser());
        }

        #endregion
    }
}
