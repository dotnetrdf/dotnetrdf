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
    public class ParsingTests
    {
        private SparqlQueryParser _parser = new SparqlQueryParser();

        private void TestQuery(String query)
        {
            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(q.ToString());
            Console.WriteLine();
            Console.WriteLine(q.ToAlgebra().ToString());
        }

        [TestMethod]
        public void SparqlParsingComplexGraphAfterUnion()
        {
            String query = "SELECT * WHERE {{?x ?y ?z} UNION {?z ?y ?x} GRAPH ?g {?x ?y ?z}}";
            this.TestQuery(query);
        }

        [TestMethod]
        public void SparqlParsingComplexFilterAfterUnion()
        {
            String query = "SELECT * WHERE {{?x ?y ?z} UNION {?z ?y ?x} FILTER(true)}";
            this.TestQuery(query);
        }

        [TestMethod]
        public void SparqlParsingComplexOptionalAfterUnion()
        {
            String query = "SELECT * WHERE {{?x ?y ?z} UNION {?z ?y ?x} OPTIONAL {?x a ?u}}";
            this.TestQuery(query);
        }

        [TestMethod]
        public void SparqlParsingComplexMinusAfterUnion()
        {
            String query = "SELECT * WHERE {{?x ?y ?z} UNION {?z ?y ?x} MINUS {?s ?p ?o}}";
            this.TestQuery(query);
        }

        [TestMethod]
        public void SparqlParsingComplexOptionalServiceUnion()
        {
            String query = "SELECT * WHERE {{?x ?y ?z} UNION {?z ?y ?x} SERVICE ?g {?x ?y ?z}}";
            this.TestQuery(query);
        }

        [TestMethod]
        public void SparqlParsingSingleSubQuery()
        {
            String query = "SELECT * WHERE {{SELECT * WHERE {?s ?p ?o}}}";
            this.TestQuery(query);
        }

        [TestMethod]
        public void SparqlParsingSetExpression()
        {
            String query = "SELECT * WHERE { ?s ?p ?o . FILTER(?o IN (1, 2, 3)) }";
            TestQuery(query);
        }

        [TestMethod]
        public void SparqlParsingSetExpression2()
        {
            String query = "PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA + "> SELECT * WHERE { ?s ?p ?o . FILTER(xsd:integer(?o) IN (1, 2, 3)) }";
            TestQuery(query);
        }

        [TestMethod]
        public void SparqlParsingSetExpression3()
        {
            String query = "PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA + "> SELECT * WHERE { ?s ?p ?o . FILTER(xsd:integer(?o) NOT IN (1, 2, 3)) }";
            TestQuery(query);
        }

        [TestMethod]
        public void SparqlParsingSetExpression4()
        {
            String query = "PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA + "> SELECT * WHERE { ?s ?p ?o . FILTER(1 + 3 IN (1, 2, 3)) }";
            TestQuery(query);
        }
    }
}
