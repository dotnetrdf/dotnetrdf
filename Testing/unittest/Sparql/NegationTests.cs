using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class NegationTests
    {
        private SparqlQueryParser _parser = new SparqlQueryParser();

        private ISparqlDataset GetTestData()
        {
            InMemoryDataset dataset = new InMemoryDataset();
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            dataset.AddGraph(g);

            return dataset;
        }

        [TestMethod]
        public void SparqlNegationMinusSimple()
        {
            SparqlParameterizedString negQuery = new SparqlParameterizedString();
            negQuery.Namespaces.AddNamespace("ex", new Uri("http://example.org/vehicles/"));
            negQuery.CommandText = "SELECT ?s WHERE { ?s a ex:Car MINUS { ?s ex:Speed ?speed } }";

            SparqlParameterizedString query = new SparqlParameterizedString();
            query.Namespaces = negQuery.Namespaces;
            query.CommandText = "SELECT ?s WHERE { ?s a ex:Car OPTIONAL { ?s ex:Speed ?speed } FILTER(!BOUND(?speed)) }";

            this.TestNegation(this.GetTestData(), negQuery, query);
        }

        [TestMethod]
        public void SparqlNegationMinusComplex()
        {
            SparqlParameterizedString negQuery = new SparqlParameterizedString();
            negQuery.Namespaces.AddNamespace("ex", new Uri("http://example.org/vehicles/"));
            negQuery.CommandText = "SELECT ?s WHERE { ?s a ex:Car MINUS { ?s ex:Speed ?speed FILTER(EXISTS { ?s ex:PassengerCapacity ?passengers }) } }";

            SparqlParameterizedString query = new SparqlParameterizedString();
            query.Namespaces = negQuery.Namespaces;
            query.CommandText = "SELECT ?s WHERE { ?s a ex:Car OPTIONAL { ?s ex:Speed ?speed ; ex:PassengerCapacity ?passengers} FILTER(!BOUND(?speed)) }";

            this.TestNegation(this.GetTestData(), negQuery, query);
        }

        [TestMethod]
        public void SparqlNegationMinusComplex2()
        {
            SparqlParameterizedString negQuery = new SparqlParameterizedString();
            negQuery.Namespaces.AddNamespace("ex", new Uri("http://example.org/vehicles/"));
            negQuery.CommandText = "SELECT ?s WHERE { ?s a ex:Car MINUS { ?s ex:Speed ?speed FILTER(NOT EXISTS { ?s ex:PassengerCapacity ?passengers }) } }";

            SparqlParameterizedString query = new SparqlParameterizedString();
            query.Namespaces = negQuery.Namespaces;
            query.CommandText = "SELECT ?s WHERE { ?s a ex:Car OPTIONAL { ?s ex:Speed ?speed OPTIONAL { ?s ex:PassengerCapacity ?passengers} FILTER(!BOUND(?passengers)) } FILTER(!BOUND(?speed)) }";

            this.TestNegation(this.GetTestData(), negQuery, query);
        }

        [TestMethod]
        public void SparqlNegationMinusDisjoint()
        {
            String query = "SELECT ?s ?p ?o WHERE { ?s ?p ?o }";
            String negQuery = "SELECT ?s ?p ?o WHERE { ?s ?p ?o MINUS { ?x ?y ?z }}";

            this.TestNegation(this.GetTestData(), negQuery, query);
        }

        [TestMethod]
        public void SparqlNegationNotExistsDisjoint()
        {
            String negQuery = "SELECT ?s ?p ?o WHERE { ?s ?p ?o FILTER(NOT EXISTS { ?x ?y ?z }) }";

            this.TestNegation(this.GetTestData(), negQuery);
        }

        [TestMethod]
        public void SparqlNegationNotExistsSimple()
        {
            SparqlParameterizedString negQuery = new SparqlParameterizedString();
            negQuery.Namespaces.AddNamespace("ex", new Uri("http://example.org/vehicles/"));
            negQuery.CommandText = "SELECT ?s WHERE { ?s a ex:Car FILTER(NOT EXISTS { ?s ex:Speed ?speed }) }";

            SparqlParameterizedString query = new SparqlParameterizedString();
            query.Namespaces = negQuery.Namespaces;
            query.CommandText = "SELECT ?s WHERE { ?s a ex:Car OPTIONAL { ?s ex:Speed ?speed } FILTER(!BOUND(?speed)) }";

            this.TestNegation(this.GetTestData(), negQuery, query);
        }

        [TestMethod]
        public void SparqlNegationExistsSimple()
        {
            SparqlParameterizedString negQuery = new SparqlParameterizedString();
            negQuery.Namespaces.AddNamespace("ex", new Uri("http://example.org/vehicles/"));
            negQuery.CommandText = "SELECT ?s WHERE { ?s a ex:Car FILTER(EXISTS { ?s ex:Speed ?speed }) }";

            SparqlParameterizedString query = new SparqlParameterizedString();
            query.Namespaces = negQuery.Namespaces;
            query.CommandText = "SELECT ?s WHERE { ?s a ex:Car OPTIONAL { ?s ex:Speed ?speed } FILTER(BOUND(?speed)) }";

            this.TestNegation(this.GetTestData(), negQuery, query);
        }

        private void TestNegation(ISparqlDataset data, SparqlParameterizedString queryWithNegation, SparqlParameterizedString queryWithoutNegation)
        {
            this.TestNegation(data, queryWithNegation.ToString(), queryWithoutNegation.ToString());
        }

        private void TestNegation(ISparqlDataset data, String queryWithNegation, String queryWithoutNegation)
        {
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(data);
            SparqlQuery negQuery = this._parser.ParseFromString(queryWithNegation);
            SparqlQuery noNegQuery = this._parser.ParseFromString(queryWithoutNegation);

            SparqlResultSet negResults = processor.ProcessQuery(negQuery) as SparqlResultSet;
            SparqlResultSet noNegResults = processor.ProcessQuery(noNegQuery) as SparqlResultSet;

            if (negResults == null) Assert.Fail("Did not get a SPARQL Result Set for the Negation Query");
            if (noNegResults == null) Assert.Fail("Did not get a SPARQL Result Set for the Non-Negation Query");

            Console.WriteLine("Negation Results");
            TestTools.ShowResults(negResults);
            Console.WriteLine();
            Console.WriteLine("Non-Negation Results");
            TestTools.ShowResults(noNegResults);
            Console.WriteLine();

            Assert.AreEqual(noNegResults, negResults, "Result Sets should have been equal");
        }

        private void TestNegation(ISparqlDataset data, String queryWithNegation)
        {
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(data);
            SparqlQuery negQuery = this._parser.ParseFromString(queryWithNegation);

            SparqlResultSet negResults = processor.ProcessQuery(negQuery) as SparqlResultSet;

            if (negResults == null) Assert.Fail("Did not get a SPARQL Result Set for the Negation Query");

            Console.WriteLine("Negation Results");
            TestTools.ShowResults(negResults);
            Console.WriteLine();

            Assert.IsTrue(negResults.IsEmpty, "Result Set should be empty");
        }
    }
}
