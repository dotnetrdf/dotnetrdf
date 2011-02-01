using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Grouping;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Sparql
{
    /// <summary>
    /// Summary description for GroupByAssignment
    /// </summary>
    [TestClass]
    public class GroupByAssignment
    {
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
            TestTools.ShowResults(results);
        }

        [TestMethod]
        public void SparqlGroupByAssignmentAggregate()
        {
            String query = "SELECT ?s ?predicates WHERE { ?s ?p ?o } GROUP BY ?s (COUNT(?p) AS ?predicates)";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            SparqlFormatter formatter = new SparqlFormatter();
            Console.WriteLine(formatter.Format(q));
            Console.WriteLine();

            QueryableGraph g = new QueryableGraph();
            FileLoader.Load(g, "InferenceTest.ttl");

            Object results = g.ExecuteQuery(q);
            TestTools.ShowResults(results);
        }
    }
}
