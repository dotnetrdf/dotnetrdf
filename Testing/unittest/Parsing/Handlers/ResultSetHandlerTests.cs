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
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Parsing.Handlers
{
    [TestClass]
    public class ResultSetHandlerTests
    {
        private void EnsureTestData(String file)
        {
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriter(MimeTypesHelper.GetMimeTypes(Path.GetExtension(file)));
            this.EnsureTestData(file, writer);
        }

        private void EnsureTestData(String file, ISparqlResultsWriter writer)
        {
            if (!File.Exists(file))
            {
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                g.Retract(g.Triples.Where(t => !t.IsGroundTriple).ToList());
                SparqlResultSet results = g.ExecuteQuery("SELECT * WHERE { ?s ?p ?o }") as SparqlResultSet;
                if (results == null) Assert.Fail("Failed to generate sample SPARQL Results");

                writer.Save(results, file);
            }
        }

        [TestMethod]
        public void ParsingResultSetHandlerImplicitSparqlXml()
        {
            this.EnsureTestData("test.srx");

            SparqlXmlParser parser = new SparqlXmlParser();
            SparqlResultSet results = new SparqlResultSet();
            parser.Load(results, "test.srx");

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (SparqlResult r in results)
            {
                Console.WriteLine(r.ToString(formatter));
            }

            Assert.IsFalse(results.IsEmpty, "Result Set should not be empty");
            Assert.AreEqual(SparqlResultsType.VariableBindings, results.ResultsType, "Results Type should be VariableBindings");
        }

        [TestMethod]
        public void ParsingResultSetHandlerImplicitSparqlJson()
        {
            this.EnsureTestData("test.srj");

            SparqlJsonParser parser = new SparqlJsonParser();
            SparqlResultSet results = new SparqlResultSet();
            parser.Load(results, "test.srj");

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (SparqlResult r in results)
            {
                Console.WriteLine(r.ToString(formatter));
            }

            Assert.IsFalse(results.IsEmpty, "Result Set should not be empty");
            Assert.AreEqual(SparqlResultsType.VariableBindings, results.ResultsType, "Results Type should be VariableBindings");
        }

        [TestMethod]
        public void ParsingResultSetHandlerImplicitSparqlRdfNTriples()
        {
            this.EnsureTestData("test.sparql.nt", new SparqlRdfWriter(new NTriplesWriter()));

            SparqlRdfParser parser = new SparqlRdfParser(new NTriplesParser());
            SparqlResultSet results = new SparqlResultSet();
            parser.Load(results, "test.sparql.nt");

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (SparqlResult r in results)
            {
                Console.WriteLine(r.ToString(formatter));
            }

            Assert.IsFalse(results.IsEmpty, "Result Set should not be empty");
            Assert.AreEqual(SparqlResultsType.VariableBindings, results.ResultsType, "Results Type should be VariableBindings");
        }

        [TestMethod]
        public void ParsingResultSetHandlerImplicitSparqlRdfTurtle()
        {
            this.EnsureTestData("test.sparql.ttl", new SparqlRdfWriter(new CompressingTurtleWriter()));

            SparqlRdfParser parser = new SparqlRdfParser(new TurtleParser());
            SparqlResultSet results = new SparqlResultSet();
            parser.Load(results, "test.sparql.ttl");

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (SparqlResult r in results)
            {
                Console.WriteLine(r.ToString(formatter));
            }

            Assert.IsFalse(results.IsEmpty, "Result Set should not be empty");
            Assert.AreEqual(SparqlResultsType.VariableBindings, results.ResultsType, "Results Type should be VariableBindings");
        }

        [TestMethod]
        public void ParsingResultSetHandlerImplicitSparqlRdfNotation3()
        {
            this.EnsureTestData("test.sparql.n3", new SparqlRdfWriter(new Notation3Writer()));

            SparqlRdfParser parser = new SparqlRdfParser(new Notation3Parser());
            SparqlResultSet results = new SparqlResultSet();
            parser.Load(results, "test.sparql.n3");

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (SparqlResult r in results)
            {
                Console.WriteLine(r.ToString(formatter));
            }

            Assert.IsFalse(results.IsEmpty, "Result Set should not be empty");
            Assert.AreEqual(SparqlResultsType.VariableBindings, results.ResultsType, "Results Type should be VariableBindings");
        }

        [TestMethod]
        public void ParsingResultSetHandlerImplicitSparqlRdfXml()
        {
            this.EnsureTestData("test.sparql.rdf", new SparqlRdfWriter(new RdfXmlWriter()));

            SparqlRdfParser parser = new SparqlRdfParser(new RdfXmlParser());
            SparqlResultSet results = new SparqlResultSet();
            parser.Load(results, "test.sparql.rdf");

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (SparqlResult r in results)
            {
                Console.WriteLine(r.ToString(formatter));
            }

            Assert.IsFalse(results.IsEmpty, "Result Set should not be empty");
            Assert.AreEqual(SparqlResultsType.VariableBindings, results.ResultsType, "Results Type should be VariableBindings");
        }

        [TestMethod]
        public void ParsingResultSetHandlerImplicitSparqlRdfJson()
        {
            this.EnsureTestData("test.sparql.json", new SparqlRdfWriter(new RdfJsonWriter()));

            SparqlRdfParser parser = new SparqlRdfParser(new RdfJsonParser());
            SparqlResultSet results = new SparqlResultSet();
            parser.Load(results, "test.sparql.json");

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (SparqlResult r in results)
            {
                Console.WriteLine(r.ToString(formatter));
            }

            Assert.IsFalse(results.IsEmpty, "Result Set should not be empty");
            Assert.AreEqual(SparqlResultsType.VariableBindings, results.ResultsType, "Results Type should be VariableBindings");
        }

        [TestMethod]
        public void ParsingResultSetHandlerExplicitSparqlXml()
        {
            this.EnsureTestData("test.srx");

            SparqlXmlParser parser = new SparqlXmlParser();
            SparqlResultSet results = new SparqlResultSet();
            parser.Load(new ResultSetHandler(results), "test.srx");

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (SparqlResult r in results)
            {
                Console.WriteLine(r.ToString(formatter));
            }

            Assert.IsFalse(results.IsEmpty, "Result Set should not be empty");
            Assert.AreEqual(SparqlResultsType.VariableBindings, results.ResultsType, "Results Type should be VariableBindings");
        }

        [TestMethod]
        public void ParsingResultSetHandlerExplicitSparqlJson()
        {
            this.EnsureTestData("test.srj");

            SparqlJsonParser parser = new SparqlJsonParser();
            SparqlResultSet results = new SparqlResultSet();
            parser.Load(new ResultSetHandler(results), "test.srj");

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (SparqlResult r in results)
            {
                Console.WriteLine(r.ToString(formatter));
            }

            Assert.IsFalse(results.IsEmpty, "Result Set should not be empty");
            Assert.AreEqual(SparqlResultsType.VariableBindings, results.ResultsType, "Results Type should be VariableBindings");
        }

        [TestMethod]
        public void ParsingResultSetHandlerExplicitSparqlRdfNTriples()
        {
            this.EnsureTestData("test.sparql.nt", new SparqlRdfWriter(new NTriplesWriter()));

            SparqlRdfParser parser = new SparqlRdfParser(new NTriplesParser());
            SparqlResultSet results = new SparqlResultSet();
            parser.Load(new ResultSetHandler(results), "test.sparql.nt");

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (SparqlResult r in results)
            {
                Console.WriteLine(r.ToString(formatter));
            }

            Assert.IsFalse(results.IsEmpty, "Result Set should not be empty");
            Assert.AreEqual(SparqlResultsType.VariableBindings, results.ResultsType, "Results Type should be VariableBindings");
        }

        [TestMethod]
        public void ParsingResultSetHandlerExplicitSparqlRdfTurtle()
        {
            this.EnsureTestData("test.sparql.ttl", new SparqlRdfWriter(new CompressingTurtleWriter()));

            SparqlRdfParser parser = new SparqlRdfParser(new TurtleParser());
            SparqlResultSet results = new SparqlResultSet();
            parser.Load(new ResultSetHandler(results), "test.sparql.ttl");

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (SparqlResult r in results)
            {
                Console.WriteLine(r.ToString(formatter));
            }

            Assert.IsFalse(results.IsEmpty, "Result Set should not be empty");
            Assert.AreEqual(SparqlResultsType.VariableBindings, results.ResultsType, "Results Type should be VariableBindings");
        }

        [TestMethod]
        public void ParsingResultSetHandlerExplicitSparqlRdfNotation3()
        {
            this.EnsureTestData("test.sparql.n3", new SparqlRdfWriter(new Notation3Writer()));

            SparqlRdfParser parser = new SparqlRdfParser(new Notation3Parser());
            SparqlResultSet results = new SparqlResultSet();
            parser.Load(new ResultSetHandler(results), "test.sparql.n3");

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (SparqlResult r in results)
            {
                Console.WriteLine(r.ToString(formatter));
            }

            Assert.IsFalse(results.IsEmpty, "Result Set should not be empty");
            Assert.AreEqual(SparqlResultsType.VariableBindings, results.ResultsType, "Results Type should be VariableBindings");
        }

        [TestMethod]
        public void ParsingResultSetHandlerExplicitSparqlRdfXml()
        {
            this.EnsureTestData("test.sparql.rdf", new SparqlRdfWriter(new RdfXmlWriter()));

            SparqlRdfParser parser = new SparqlRdfParser(new RdfXmlParser());
            SparqlResultSet results = new SparqlResultSet();
            parser.Load(new ResultSetHandler(results), "test.sparql.rdf");

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (SparqlResult r in results)
            {
                Console.WriteLine(r.ToString(formatter));
            }

            Assert.IsFalse(results.IsEmpty, "Result Set should not be empty");
            Assert.AreEqual(SparqlResultsType.VariableBindings, results.ResultsType, "Results Type should be VariableBindings");
        }

        [TestMethod]
        public void ParsingResultSetHandlerExplicitSparqlRdfJson()
        {
            this.EnsureTestData("test.sparql.json", new SparqlRdfWriter(new RdfJsonWriter()));

            SparqlRdfParser parser = new SparqlRdfParser(new RdfJsonParser());
            SparqlResultSet results = new SparqlResultSet();
            parser.Load(new ResultSetHandler(results), "test.sparql.json");

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (SparqlResult r in results)
            {
                Console.WriteLine(r.ToString(formatter));
            }

            Assert.IsFalse(results.IsEmpty, "Result Set should not be empty");
            Assert.AreEqual(SparqlResultsType.VariableBindings, results.ResultsType, "Results Type should be VariableBindings");
        }

        [TestMethod]
        public void ParsingMergingResultSetHandler()
        {
            this.EnsureTestData("test.srx", new SparqlXmlWriter());

            SparqlXmlParser parser = new SparqlXmlParser();
            SparqlResultSet results = new SparqlResultSet();
            MergingResultSetHandler handler = new MergingResultSetHandler(results);
            parser.Load(handler, "test.srx");

            Assert.IsFalse(results.IsEmpty, "Result Set should not be empty");
            Assert.AreEqual(SparqlResultsType.VariableBindings, results.ResultsType, "Results Type should be VariableBindings");

            int count = results.Count;

            //Load again
            parser.Load(handler, "test.srx");

            Assert.AreEqual(count * 2, results.Count, "Expected result count to have doubled");
        }

        [TestMethod]
        public void ParsingResultsWriteThroughHandlerSparqlXml()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Query.Optimisation.OptimiserStats.ttl");
            SparqlResultSet original = g.ExecuteQuery("SELECT * WHERE { ?s ?p ?o }") as SparqlResultSet;
            SparqlXmlWriter sparqlWriter = new SparqlXmlWriter();
            sparqlWriter.Save(original, "test.custom.srx");

            SparqlXmlParser parser = new SparqlXmlParser();
            System.IO.StringWriter writer = new System.IO.StringWriter();
            ResultWriteThroughHandler handler = new ResultWriteThroughHandler(new SparqlXmlFormatter(), writer, false);
            parser.Load(handler, "test.custom.srx");

            Console.WriteLine(writer.ToString());

            SparqlResultSet results = new SparqlResultSet();
            parser.Load(results, new StringReader(writer.ToString()));

            Console.WriteLine("Original Result Count: " + original.Count);
            Console.WriteLine("Round Trip Result Count: " + results.Count);

            Assert.AreEqual(original, results, "Result Sets should be equal");
        }
    }
}
