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
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Parsing.Handlers
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

        [TestMethod]
        public void ParsingPagingHandlerRdfXml()
        {
            this.ParsingUsingPagingHandler("temp.rdf", new RdfXmlParser());
        }

        [TestMethod]
        public void ParsingPagingHandlerRdfA()
        {
            this.ParsingUsingPagingHandler("temp.html", new RdfAParser());
        }

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

        [TestMethod]
        public void ParsingPagingHandlerRdfXml2()
        {
            this.ParsingUsingPagingHandler2("temp.rdf", new RdfXmlParser());
        }

        [TestMethod]
        public void ParsingPagingHandlerRdfA2()
        {
            this.ParsingUsingPagingHandler2("temp.html", new RdfAParser());
        }

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

        [TestMethod]
        public void ParsingPagingHandlerRdfXml3()
        {
            this.ParsingUsingPagingHandler3("temp.rdf", new RdfXmlParser());
        }

        [TestMethod]
        public void ParsingPagingHandlerRdfA3()
        {
            this.ParsingUsingPagingHandler3("temp.html", new RdfAParser());
        }

        [TestMethod]
        public void ParsingPagingHandlerRdfJson3()
        {
            this.ParsingUsingPagingHandler3("temp.json", new RdfJsonParser());
        }

        #endregion
    }
}
