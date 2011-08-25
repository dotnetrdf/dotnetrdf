using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class BlankNodeVariableTests
    {
        private LeviathanQueryProcessor _processor;
        private TripleStore _data;
        private SparqlQueryParser _parser = new SparqlQueryParser();

        private void EnsureTestData()
        {
            if (this._data == null)
            {
                this._data = new TripleStore();
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                this._data.Add(g);
            }
            if (this._processor == null)
            {
                this._processor = new LeviathanQueryProcessor(this._data);
            }
        }

        [TestMethod]
        public void SparqlBlankNodeVariables1()
        {
            this.EnsureTestData();

            SparqlQuery q = this._parser.ParseFromString("SELECT ?o WHERE { _:s ?p1 ?o1 FILTER(ISURI(?o1)) ?o1 ?p2 ?o . FILTER(ISLITERAL(?o)) }");
            q.AlgebraOptimisers = new IAlgebraOptimiser[] { new StrictAlgebraOptimiser() };
            SparqlFormatter formatter = new SparqlFormatter();
            Console.WriteLine(formatter.Format(q));
            Console.WriteLine(q.ToAlgebra().ToString());

            SparqlResultSet results = this._processor.ProcessQuery(q) as SparqlResultSet;
            if (results == null) Assert.Fail("Did not get a SPARQL Result Set as expected");
            Assert.IsFalse(results.Count == 0, "Result Set should not be empty");

            Assert.IsTrue(results.All(r => r.HasValue("o") && r["o"] != null && r["o"].NodeType == NodeType.Literal), "All results should be literals");
        }
    }
}
