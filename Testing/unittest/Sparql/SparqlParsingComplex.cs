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
    public class SparqlParsingComplex
    {
        [TestMethod]
        public void SparqlNestedGraphPatternFirstItem()
        {
            try
            {
                SparqlQueryParser parser = new SparqlQueryParser();
                SparqlQuery q = parser.ParseFromFile("childgraphpattern.rq");

                Console.WriteLine(q.ToString());
                Console.WriteLine();
                Console.WriteLine(q.ToAlgebra().ToString());
            }
            catch (RdfParseException parseEx)
            {
                TestTools.ReportError("Parsing Error", parseEx, true);
            }
        }

        [TestMethod]
        public void SparqlNestedGraphPatternFirstItem2()
        {
            try
            {
                SparqlQueryParser parser = new SparqlQueryParser();
                SparqlQuery q = parser.ParseFromFile("childgraphpattern2.rq");

                Console.WriteLine(q.ToString());
                Console.WriteLine();
                Console.WriteLine(q.ToAlgebra().ToString());
            }
            catch (RdfParseException parseEx)
            {
                TestTools.ReportError("Parsing Error", parseEx, true);
            }
        }

        [TestMethod]
        public void SparqlSubQueryWithLimitAndOrderBy()
        {
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");

            String query = "SELECT * WHERE { { SELECT * WHERE {?s ?p ?o} ORDER BY ?p ?o LIMIT 2 } }";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            Object results = g.ExecuteQuery(q);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                TestTools.ShowResults(rset);

                Assert.IsTrue(rset.All(r => r.HasValue("s") && r.HasValue("p") && r.HasValue("o")), "All Results should have had ?s, ?p and ?o variables");
            }
            else
            {
                Assert.Fail("Expected a SPARQL Result Set");
            }
        }
    }
}
