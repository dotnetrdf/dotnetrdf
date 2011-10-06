using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Writing;

namespace VDS.RDF.Test.Writing
{
    [TestClass]
    public class SparqlTsvTests
    {
        private InMemoryDataset _dataset;
        private LeviathanQueryProcessor _processor;
        private SparqlQueryParser _parser = new SparqlQueryParser();
        private SparqlTsvParser _tsvParser = new SparqlTsvParser();
        private SparqlTsvWriter _tsvWriter = new SparqlTsvWriter();

        private void EnsureTestData()
        {
            if (this._dataset == null)
            {
                TripleStore store = new TripleStore();
                Graph g = new Graph();
                g.LoadFromFile("InferenceTest.ttl");
                g.BaseUri = new Uri("http://example.org/graph");
                store.Add(g);

                this._dataset = new InMemoryDataset(store);
                this._processor = new LeviathanQueryProcessor(this._dataset);
            }
        }

        private void TestTsvRoundTrip(String query)
        {
            this.EnsureTestData();

            SparqlParameterizedString queryString = new SparqlParameterizedString(query);
            queryString.Namespaces.AddNamespace("ex", new Uri("http://example.org/vehicles/"));
            SparqlQuery q = this._parser.ParseFromString(queryString);

            SparqlResultSet original = this._processor.ProcessQuery(q) as SparqlResultSet;
            if (original == null) Assert.Fail("Did not get a SPARQL Result Set as expected");

            Console.WriteLine("Original Results:");
            TestTools.ShowResults(original);

            System.IO.StringWriter writer = new System.IO.StringWriter();
            this._tsvWriter.Save(original, writer);
            Console.WriteLine("Serialized TSV Results:");
            Console.WriteLine(writer.ToString());
            Console.WriteLine();

            SparqlResultSet results = new SparqlResultSet();
            this._tsvParser.Load(results, new StringReader(writer.ToString()));
            Console.WriteLine("Parsed Results:");
            TestTools.ShowResults(results);

            Assert.AreEqual(original, results, "Result Sets should be equal");
            
        }

        [TestMethod]
        public void WritingSparqlTsv1()
        {
            this.TestTsvRoundTrip("SELECT * WHERE { ?s a ?type }");
        }

        [TestMethod]
        public void WritingSparqlTsv2()
        {
            this.TestTsvRoundTrip("SELECT * WHERE { ?s a ?type . ?s ex:Speed ?speed }");
        }

        [TestMethod]
        public void WritingSparqlTsv3()
        {
            this.TestTsvRoundTrip("SELECT * WHERE { ?s a ?type . OPTIONAL { ?s ex:Speed ?speed } }");
        }

        [TestMethod]
        public void WritingSparqlTsv4()
        {
            this.TestTsvRoundTrip("SELECT * WHERE { ?s <http://example.org/noSuchThing> ?o }");
        }

        [TestMethod]
        public void WritingSparqlTsv5()
        {
            this.TestTsvRoundTrip("SELECT * WHERE { ?s a ?type . OPTIONAL { ?s ex:Speed ?speed } ?s ?p ?o }");
        }

        [TestMethod]
        public void WritingSparqlTsv6()
        {
            this.TestTsvRoundTrip("SELECT ?s (ISLITERAL(?o) AS ?LiteralObject) WHERE { ?s ?p ?o }");
        }
    }
}
