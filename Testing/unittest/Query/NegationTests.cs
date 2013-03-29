/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query
{
    [TestClass]
    public class NegationTests
    {
        private SparqlQueryParser _parser = new SparqlQueryParser();

        private ISparqlDataset GetTestData()
        {
            InMemoryDataset dataset = new InMemoryDataset();
            Graph g = new Graph();
            g.LoadFromFile("InferenceTest.ttl");
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

        [TestMethod]
        public void SparqlNegationFullMinued()
        {
            SparqlQuery lhsQuery = this._parser.ParseFromFile("full-minuend-lhs.rq");
            SparqlQuery rhsQuery = this._parser.ParseFromFile("full-minuend-rhs.rq");
            SparqlQuery query = this._parser.ParseFromFile("full-minuend.rq");
            Graph g = new Graph();
            g.LoadFromFile("full-minuend.ttl");
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(new InMemoryQuadDataset(g));

            SparqlResultSet lhs = processor.ProcessQuery(lhsQuery) as SparqlResultSet;
            Console.WriteLine("LHS Intermediate Results");
            TestTools.ShowResults(lhs);
            Console.WriteLine();

            SparqlResultSet rhs = processor.ProcessQuery(rhsQuery) as SparqlResultSet;
            Console.WriteLine("RHS Intermediate Results");
            TestTools.ShowResults(rhs);
            Console.WriteLine();

            SparqlResultSet actual = processor.ProcessQuery(query) as SparqlResultSet;
            if (actual == null) Assert.Fail("Null results");
            SparqlResultSet expected = new SparqlResultSet();
            SparqlXmlParser parser = new SparqlXmlParser();
            parser.Load(expected, "full-minuend.srx");

            Console.WriteLine("Actual Results:");
            TestTools.ShowResults(actual);
            Console.WriteLine();
            Console.WriteLine("Expected Results:");
            TestTools.ShowResults(expected);

            Assert.AreEqual(expected, actual, "Result Sets should be equal");
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
