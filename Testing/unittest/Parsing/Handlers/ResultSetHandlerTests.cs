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
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriter(MimeTypesHelper.GetMimeTypes(file));
            this.EnsureTestData(file, writer);
        }

        private void EnsureTestData(String file, ISparqlResultsWriter writer)
        {
            if (!File.Exists(file))
            {
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
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
        public void ParsingResultSetHandlerMerging()
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
    }
}
