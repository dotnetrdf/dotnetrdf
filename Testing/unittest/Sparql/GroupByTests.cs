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
    public class GroupByTests
    {
        [TestMethod]
        public void SparqlGroupByInSubQuery()
        {
            String query = "SELECT ?s WHERE {{SELECT * WHERE {?s ?p ?o} GROUP BY ?s}} GROUP BY ?s";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);
        }

        [TestMethod]
        public void SparqlGroupByAssignmentSimple()
        {
            String query = "SELECT ?x WHERE { ?s ?p ?o } GROUP BY ?s AS ?x";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            SparqlFormatter formatter = new SparqlFormatter();
            Console.WriteLine(formatter.Format(q));
            Console.WriteLine();

            QueryableGraph g = new QueryableGraph();
            FileLoader.Load(g, "InferenceTest.ttl");

            Object results = g.ExecuteQuery(q);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                TestTools.ShowResults(rset);

                Assert.IsTrue(rset.All(r => r.HasValue("x") && !r.HasValue("s")), "All Results should have a ?x variable and no ?s variable");
            }
            else
            {
                Assert.Fail("Didn't get a Result Set as expected");
            }
        }

        [TestMethod]
        public void SparqlGroupByAssignmentSimple2()
        {
            String query = "SELECT ?x (COUNT(?p) AS ?predicates) WHERE { ?s ?p ?o } GROUP BY ?s AS ?x";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            SparqlFormatter formatter = new SparqlFormatter();
            Console.WriteLine(formatter.Format(q));
            Console.WriteLine();

            QueryableGraph g = new QueryableGraph();
            FileLoader.Load(g, "InferenceTest.ttl");

            Object results = g.ExecuteQuery(q);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                TestTools.ShowResults(rset);

                Assert.IsTrue(rset.All(r => r.HasValue("x") && !r.HasValue("s") && r.HasValue("predicates")), "All Results should have a ?x and ?predicates variables and no ?s variable");
            }
            else
            {
                Assert.Fail("Didn't get a Result Set as expected");
            }
        }

        [TestMethod]
        public void SparqlGroupByAssignmentExpression()
        {
            String query = "SELECT ?s ?sum WHERE { ?s ?p ?o } GROUP BY ?s (1 + 2 AS ?sum)";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            SparqlFormatter formatter = new SparqlFormatter();
            Console.WriteLine(formatter.Format(q));
            Console.WriteLine();

            QueryableGraph g = new QueryableGraph();
            FileLoader.Load(g, "InferenceTest.ttl");

            Object results = g.ExecuteQuery(q);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                TestTools.ShowResults(rset);

                Assert.IsTrue(rset.All(r => r.HasValue("s") && r.HasValue("sum")), "All Results should have a ?s and a ?sum variable");
            }
            else
            {
                Assert.Fail("Didn't get a Result Set as expected");
            }
        }

        [TestMethod]
        public void SparqlGroupByAssignmentExpression2()
        {
            String query = "SELECT ?lang (SAMPLE(?o) AS ?example) WHERE { ?s ?p ?o . FILTER(ISLITERAL(?o)) } GROUP BY (LANG(?o) AS ?lang)";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            SparqlFormatter formatter = new SparqlFormatter();
            Console.WriteLine(formatter.Format(q));
            Console.WriteLine();

            QueryableGraph g = new QueryableGraph();
            UriLoader.Load(g, new Uri("http://dbpedia.org/resource/Southampton"));

            Object results = g.ExecuteQuery(q);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                TestTools.ShowResults(rset);

                Assert.IsTrue(rset.All(r => r.HasValue("lang") && r.HasValue("example")), "All Results should have a ?lang and a ?example variable");
            }
            else
            {
                Assert.Fail("Didn't get a Result Set as expected");
            }
        }

        [TestMethod]
        public void SparqlGroupByAssignmentExpression3()
        {
            String query = "SELECT ?lang (SAMPLE(?o) AS ?example) WHERE { ?s ?p ?o . FILTER(ISLITERAL(?o)) } GROUP BY (LANG(?o) AS ?lang) HAVING LANGMATCHES(?lang, \"*\")";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            SparqlFormatter formatter = new SparqlFormatter();
            Console.WriteLine(formatter.Format(q));
            Console.WriteLine();

            QueryableGraph g = new QueryableGraph();
            UriLoader.Load(g, new Uri("http://dbpedia.org/resource/Southampton"));

            Object results = g.ExecuteQuery(q);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                TestTools.ShowResults(rset);

                Assert.IsTrue(rset.All(r => r.HasValue("lang") && r.HasValue("example")), "All Results should have a ?lang and a ?example variable");
            }
            else
            {
                Assert.Fail("Didn't get a Result Set as expected");
            }
        }
    }
}
