using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class ExplainProcessorTests
    {
        private ExplainQueryProcessor _processor;
        private SparqlQueryParser _parser = new SparqlQueryParser();

        private void TestExplainProcessor(String query)
        {
            if (this._processor == null)
            {
                TripleStore store = new TripleStore();
                Graph g = new Graph();
                g.LoadFromFile("InferenceTest.ttl");
                g.BaseUri = null;
                store.Add(g);

                this._processor = new ExplainQueryProcessor(store, Console.Out);
            }

            SparqlQuery q = this._parser.ParseFromString(query);
            Object results = this._processor.ProcessQuery(q);
            TestTools.ShowResults(results);
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
    }
}
