using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class ExplainProcessorTests
    {
        private ExplainQueryProcessor _processor;
        private SparqlQueryParser _parser = new SparqlQueryParser();
        private SparqlFormatter _formatter = new SparqlFormatter();

        private void TestExplainProcessor(String query)
        {
            if (this._processor == null)
            {
                TripleStore store = new TripleStore();
                Graph g = new Graph();
                g.LoadFromFile("InferenceTest.ttl");
                g.BaseUri = null;
                store.Add(g);

                this._processor = new ExplainQueryProcessor(store);
            }

            SparqlQuery q = this._parser.ParseFromString(query);
            Console.WriteLine("Input Query:");
            Console.WriteLine(this._formatter.Format(q));
            Console.WriteLine();

            Console.WriteLine("Explanation with Default Options:");
            this._processor.ExplanationLevel = ExplanationLevel.Default;
            Object results = this._processor.ProcessQuery(q);

            Console.WriteLine();
            Console.WriteLine("Explanation with Full Options:");
            this._processor.ExplanationLevel = ExplanationLevel.All;
            results = this._processor.ProcessQuery(q);
        }

        [TestMethod]
        public void SparqlExplainProcessor1()
        {
            this.TestExplainProcessor("SELECT * WHERE { ?s ?p ?o }");
        }

        [TestMethod]
        public void SparqlExplainProcessor2()
        {
            this.TestExplainProcessor("SELECT * WHERE { ?s ?p ?o OPTIONAL { ?s a ?type } }");
        }

        [TestMethod]
        public void SparqlExplainProcessor3()
        {
            this.TestExplainProcessor("SELECT * WHERE { ?s ?p ?o MINUS { ?s a ?type } }");
        }

        [TestMethod]
        public void SparqlExplainProcessor4()
        {
            this.TestExplainProcessor("SELECT * WHERE { ?s ?p ?o . OPTIONAL { ?s a ?type } FILTER(!BOUND(?type)) }");
        }

        [TestMethod]
        public void SparqlExplainProcessor5()
        {
            this.TestExplainProcessor("SELECT * WHERE { ?s ?p ?o . ?x ?y ?z }");
        }

        [TestMethod]
        public void SparqlExplainProcessor6()
        {
            this.TestExplainProcessor("SELECT * WHERE { ?s ?p ?o . ?s a ?type }");
        }

        [TestMethod]
        public void SparqlExplainProcessor7()
        {
            this.TestExplainProcessor("SELECT * WHERE { ?s ?p ?o . ?s ?p ?o2 }");
        }

        [TestMethod]
        public void SparqlExplainProcessor8()
        {
            this.TestExplainProcessor("SELECT * WHERE { ?s ?p ?o MINUS { ?x ?y ?z } }");
        }

        [TestMethod]
        public void SparqlExplainProcessor9()
        {
            this.TestExplainProcessor("SELECT * WHERE { ?s ?p ?o . FILTER (!SAMETERM(?s, <ex:nothing>)) . BIND(IsLiteral(?o) AS ?hasLiteral) . ?s a ?type }");
        }

        [TestMethod]
        public void SparqlExplainProcessor10()
        {
            this.TestExplainProcessor("SELECT * WHERE { ?s ?p ?o . FILTER (!SAMETERM(?s, <ex:nothing>)) . ?s a ?type . BIND(IsLiteral(?o) AS ?hasLiteral)}");
        }
    }
}
