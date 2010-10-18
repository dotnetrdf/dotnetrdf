using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions;

namespace VDS.RDF.Test
{
    [TestClass]
    public class SparqlTests
    {
        [TestMethod]
        public void SparqlBind()
        {
            String query = "PREFIX fn: <" + XPathFunctionFactory.XPathFunctionsNamespace + "> SELECT ?triple WHERE { ?s ?p ?o . BIND(fn:concat(STR(?s), ' ', STR(?p), ' ', STR(?o)) AS ?triple) }";

            TripleStore store = new TripleStore();
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            store.Add(g);

            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            Object results = q.Evaluate(store);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.WriteLine(r.ToString());
                }
                Assert.IsTrue(rset.Count > 0, "Expected 1 or more results");
            }
            else
            {
                Assert.Fail("Expected a SPARQL Result Set");
            }
        }

        [TestMethod]
        public void SparqlBindLazy()
        {
            String query = "PREFIX fn: <" + XPathFunctionFactory.XPathFunctionsNamespace + "> SELECT ?triple WHERE { ?s ?p ?o . BIND(fn:concat(STR(?s), ' ', STR(?p), ' ', STR(?o)) AS ?triple) } LIMIT 1";

            TripleStore store = new TripleStore();
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            store.Add(g);

            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            Console.WriteLine(q.ToAlgebra().ToString());
            Assert.IsTrue(q.ToAlgebra().ToString().Contains("LazyBgp"), "Should have been optimised to use a Lazy BGP");
            Console.WriteLine();

            Object results = q.Evaluate(store);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.WriteLine(r.ToString());
                }
                Assert.IsTrue(rset.Count == 1, "Expected exactly 1 results");
            }
            else
            {
                Assert.Fail("Expected a SPARQL Result Set");
            }
        }

        [TestMethod]
        public void SparqlBindLazy2()
        {
            String query = "PREFIX fn: <" + XPathFunctionFactory.XPathFunctionsNamespace + "> SELECT * WHERE { ?s ?p ?o . BIND(fn:concat(STR(?s), ' ', STR(?p), ' ', STR(?o)) AS ?triple) } LIMIT 10";

            TripleStore store = new TripleStore();
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            store.Add(g);

            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            Console.WriteLine(q.ToAlgebra().ToString());
            Assert.IsTrue(q.ToAlgebra().ToString().Contains("LazyBgp"), "Should have been optimised to use a Lazy BGP");
            Console.WriteLine();

            Object results = q.Evaluate(store);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.WriteLine(r.ToString());
                }
                Assert.IsTrue(rset.Count == 10, "Expected exactly 10 results");
            }
            else
            {
                Assert.Fail("Expected a SPARQL Result Set");
            }
        }

        [TestMethod]
        public void SparqlBindLazy3()
        {
            String query = "PREFIX fn: <" + XPathFunctionFactory.XPathFunctionsNamespace + "> SELECT * WHERE { ?s ?p ?o . BIND(fn:concat(STR(?s), ' ', STR(?p), ' ', STR(?o)) AS ?triple) } LIMIT 10 OFFSET 10";

            TripleStore store = new TripleStore();
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            store.Add(g);

            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            Console.WriteLine(q.ToAlgebra().ToString());
            Assert.IsTrue(q.ToAlgebra().ToString().Contains("LazyBgp"), "Should have been optimised to use a Lazy BGP");
            Console.WriteLine();

            Object results = q.Evaluate(store);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.WriteLine(r.ToString());
                }
                Assert.IsTrue(rset.Count == 10, "Expected exactly 10 results");
            }
            else
            {
                Assert.Fail("Expected a SPARQL Result Set");
            }
        }

        [TestMethod]
        public void SparqlBindNested()
        {
            String query = "PREFIX fn: <" + XPathFunctionFactory.XPathFunctionsNamespace + "> SELECT ?triple WHERE { ?s ?p ?o .{ BIND(fn:concat(STR(?s), ' ', STR(?p), ' ', STR(?o)) AS ?triple) }}";

            TripleStore store = new TripleStore();
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            store.Add(g);

            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            Object results = q.Evaluate(store);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.WriteLine(r.ToString());
                }
                Assert.IsTrue(rset.Count == 0, "Expected no results");
            }
            else
            {
                Assert.Fail("Expected a SPARQL Result Set");
            }
        }

        [TestMethod]
        public void SparqlBindIn10Standard()
        {
            String query = "PREFIX fn: <" + XPathFunctionFactory.XPathFunctionsNamespace + "> SELECT ?triple WHERE { ?s ?p ?o . BIND(fn:concat(STR(?s), ' ', STR(?p), ' ', STR(?o)) AS ?triple) }";

            TripleStore store = new TripleStore();
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            store.Add(g);

            SparqlQueryParser parser = new SparqlQueryParser(SparqlQuerySyntax.Sparql_1_0);
            try
            {
                SparqlQuery q = parser.ParseFromString(query);
                Assert.Fail("Expected a RdfParseException to be thrown");
            }
            catch (RdfParseException)
            {
                Console.WriteLine("Error thrown as expected");
            }
            catch
            {
                Assert.Fail("Expected a RdfParseException");
            }
        }

        [TestMethod]
        public void SparqlBindToExistingVariable()
        {
            String query = "PREFIX fn: <" + XPathFunctionFactory.XPathFunctionsNamespace + "> SELECT * WHERE { ?s ?p ?o . BIND(?s AS ?p) }";

            TripleStore store = new TripleStore();
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            store.Add(g);

            SparqlQueryParser parser = new SparqlQueryParser();
            try
            {
                SparqlQuery q = parser.ParseFromString(query);
                store.ExecuteQuery(q);
                Assert.Fail("Expected a RdfQueryException to be thrown");
            }
            catch (RdfQueryException)
            {
                Console.WriteLine("Error thrown as expected");
            }
            catch
            {
                Assert.Fail("Expected a RdfQueryException");
            }
        }

        [TestMethod]
        public void SparqlBindToExistingVariableLazy()
        {
            String query = "PREFIX fn: <" + XPathFunctionFactory.XPathFunctionsNamespace + "> SELECT * WHERE { ?s ?p ?o . BIND(?s AS ?p) } LIMIT 1";

            TripleStore store = new TripleStore();
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            store.Add(g);

            SparqlQueryParser parser = new SparqlQueryParser();
            try
            {
                SparqlQuery q = parser.ParseFromString(query);

                Console.WriteLine(q.ToAlgebra().ToString());
                Assert.IsTrue(q.ToAlgebra().ToString().Contains("LazyBgp"), "Should have been optimised to use a Lazy BGP");
                Console.WriteLine();

                store.ExecuteQuery(q);
                Assert.Fail("Expected a RdfQueryException to be thrown");
            }
            catch (RdfQueryException)
            {
                Console.WriteLine("Error thrown as expected");
            }
            catch
            {
                Assert.Fail("Expected a RdfQueryException");
            }
        }

        [TestMethod]
        public void SparqlBindToExistingVariableNested()
        {
            String query = "PREFIX fn: <" + XPathFunctionFactory.XPathFunctionsNamespace + "> SELECT * WHERE { ?s ?p ?o .{ BIND(?s AS ?p)} }";

            TripleStore store = new TripleStore();
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            store.Add(g);

            SparqlQueryParser parser = new SparqlQueryParser();
            try
            {
                SparqlQuery q = parser.ParseFromString(query);
                store.ExecuteQuery(q);
                Assert.Fail("Expected a RdfQueryException to be thrown");
            }
            catch (RdfQueryException)
            {
                Console.WriteLine("Error thrown as expected");
            }
            catch
            {
                Assert.Fail("Expected a RdfQueryException");
            }
        }

        [TestMethod]
        public void SparqlLet()
        {
            String query = "PREFIX fn: <" + XPathFunctionFactory.XPathFunctionsNamespace + "> SELECT ?triple WHERE { ?s ?p ?o . LET (?triple := fn:concat(STR(?s), ' ', STR(?p), ' ', STR(?o))) }";

            TripleStore store = new TripleStore();
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            store.Add(g);

            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            Object results = q.Evaluate(store);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.WriteLine(r.ToString());
                }
                Assert.IsTrue(rset.Count > 0, "Expected 1 or more results");
            }
            else
            {
                Assert.Fail("Expected a SPARQL Result Set");
            }
        }

        [TestMethod]
        public void SparqlLetIn11Standard()
        {
            String query = "PREFIX fn: <" + XPathFunctionFactory.XPathFunctionsNamespace + "> SELECT ?triple WHERE { ?s ?p ?o . LET (?triple := fn:concat(STR(?s), ' ', STR(?p), ' ', STR(?o))) }";

            TripleStore store = new TripleStore();
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            store.Add(g);

            SparqlQueryParser parser = new SparqlQueryParser(SparqlQuerySyntax.Sparql_1_1);
            try
            {
                SparqlQuery q = parser.ParseFromString(query);
                Assert.Fail("Expected a RdfParseException to be thrown");
            }
            catch (RdfParseException)
            {
                Console.WriteLine("Error thrown as expected");
            } 
            catch 
            {
                Assert.Fail("Expected a RdfParseException");
            }
        }

        [TestMethod]
        public void SparqlSubQueryLazy()
        {
            String query = "SELECT * WHERE { {SELECT * WHERE { ?s ?p ?o}}} LIMIT 1";

            TripleStore store = new TripleStore();
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            store.Add(g);

            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            Console.WriteLine(q.ToAlgebra().ToString());
            Assert.IsTrue(q.ToAlgebra().ToString().Contains("LazyBgp"), "Should have been optimised to use a Lazy BGP");
            Console.WriteLine();

            Object results = q.Evaluate(store);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.WriteLine(r.ToString());
                }
                Assert.IsTrue(rset.Count == 1, "Expected exactly 1 results");
            }
            else
            {
                Assert.Fail("Expected a SPARQL Result Set");
            }
        }

        [TestMethod]
        public void SparqlSubQueryLazy2()
        {
            String query = "SELECT * WHERE { {SELECT * WHERE { ?s ?p ?o}}} LIMIT 10";

            TripleStore store = new TripleStore();
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            store.Add(g);

            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            Console.WriteLine(q.ToAlgebra().ToString());
            Assert.IsTrue(q.ToAlgebra().ToString().Contains("LazyBgp"), "Should have been optimised to use a Lazy BGP");
            Console.WriteLine();

            Object results = q.Evaluate(store);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.WriteLine(r.ToString());
                }
                Assert.IsTrue(rset.Count == 10, "Expected exactly 10 results");
            }
            else
            {
                Assert.Fail("Expected a SPARQL Result Set");
            }
        }

        [TestMethod]
        public void SparqlSubQueryLazy3()
        {
            String query = "SELECT * WHERE { {SELECT * WHERE { ?s ?p ?o}}} LIMIT 10 OFFSET 10";

            TripleStore store = new TripleStore();
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            store.Add(g);

            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            Console.WriteLine(q.ToAlgebra().ToString());
            Assert.IsTrue(q.ToAlgebra().ToString().Contains("LazyBgp"), "Should have been optimised to use a Lazy BGP");
            Console.WriteLine();

            Object results = q.Evaluate(store);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.WriteLine(r.ToString());
                }
                Assert.IsTrue(rset.Count == 10, "Expected exactly 10 results");
            }
            else
            {
                Assert.Fail("Expected a SPARQL Result Set");
            }
        }

        [TestMethod]
        public void SparqlSubQueryLazyComplex()
        {
            String query = "SELECT * WHERE { ?s a <http://example.org/vehicles/Car> . {SELECT * WHERE { ?s <http://example.org/vehicles/Speed> ?speed}}} LIMIT 1";

            TripleStore store = new TripleStore();
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            store.Add(g);

            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            Console.WriteLine(q.ToAlgebra().ToString());
            Assert.IsTrue(q.ToAlgebra().ToString().Contains("LazyBgp"), "Should have been optimised to use a Lazy BGP");
            Console.WriteLine();

            Object results = q.Evaluate(store);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.WriteLine(r.ToString());
                }
                Assert.IsTrue(rset.Count == 1, "Expected exactly 1 results");
            }
            else
            {
                Assert.Fail("Expected a SPARQL Result Set");
            }
        }

        [TestMethod]
        public void SparqlSubQueryLazyComplex2()
        {
            String query = "SELECT * WHERE { ?s a <http://example.org/vehicles/Car> . {SELECT * WHERE { ?s <http://example.org/vehicles/Speed> ?speed}}} LIMIT 5";

            TripleStore store = new TripleStore();
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            store.Add(g);

            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            Console.WriteLine(q.ToAlgebra().ToString());
            Assert.IsTrue(q.ToAlgebra().ToString().Contains("LazyBgp"), "Should have been optimised to use a Lazy BGP");
            Console.WriteLine();

            Object results = q.Evaluate(store);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.WriteLine(r.ToString());
                }
                Assert.IsTrue(rset.Count <= 5, "Expected at most 5 results");
            }
            else
            {
                Assert.Fail("Expected a SPARQL Result Set");
            }
        }
    }
}
