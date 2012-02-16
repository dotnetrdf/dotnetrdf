using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class VariableSubstitutionTests
    {
        private SparqlQueryParser _parser = new SparqlQueryParser();
        private SparqlFormatter _formatter = new SparqlFormatter(new NamespaceMapper());

        private void TestSubstitution(SparqlQuery q, String findVar, String replaceVar, IEnumerable<String> expected, IEnumerable<String> notExpected)
        {
            Console.WriteLine("Input Query:");
            Console.WriteLine(this._formatter.Format(q));
            Console.WriteLine();

            ISparqlAlgebra algebra = q.ToAlgebra();
            VariableSubstitutionTransformer transformer = new VariableSubstitutionTransformer(findVar, replaceVar);
            try
            {
                ISparqlAlgebra resAlgebra = transformer.Optimise(algebra);
                algebra = resAlgebra;
            }
            catch (Exception ex)
            {
                //Ignore errors
                Console.WriteLine("Error Transforming - " + ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine();
            }

            SparqlQuery resQuery = algebra.ToQuery();
            String resText = this._formatter.Format(resQuery);
            Console.WriteLine("Resulting Query:");
            Console.WriteLine(resText);
            Console.WriteLine();

            foreach (String x in expected)
            {
                Assert.IsTrue(resText.Contains(x), "Expected Transformed Query to contain string '" + x + "'");
            }
            foreach (String x in notExpected)
            {
                Assert.IsFalse(resText.Contains(x), "Transformed Query contained string '" + x + "' which was expected to have been transformed");
            }
        }

        [TestMethod]
        public void SparqlOptimiserAlgebraVarSub1()
        {
            String query = "SELECT * WHERE { ?s ?p ?o }";
            SparqlQuery q = this._parser.ParseFromString(query);
            this.TestSubstitution(q, "s", "x", new String[] { "?x" }, new String[] { "?s" });
        }

        [TestMethod]
        public void SparqlOptimiserAlgebraVarSub2()
        {
            String query = "SELECT * WHERE { ?s ?p ?o . ?s a ?type }";
            SparqlQuery q = this._parser.ParseFromString(query);
            this.TestSubstitution(q, "s", "x", new String[] { "?x" }, new String[] { "?s" });
        }

        [TestMethod]
        public void SparqlOptimiserAlgebraVarSub3()
        {
            String query = "SELECT * WHERE { ?s ?p ?o . FILTER(ISBLANK(?s)) }";
            SparqlQuery q = this._parser.ParseFromString(query);
            this.TestSubstitution(q, "s", "x", new String[] { "?x" }, new String[] { "?s" });
        }

        [TestMethod]
        public void SparqlOptimiserAlgebraVarSub4()
        {
            String query = "SELECT * WHERE { ?s ?p ?o . BIND(ISBLANK(?s) AS ?blank) }";
            SparqlQuery q = this._parser.ParseFromString(query);
            this.TestSubstitution(q, "s", "x", new String[] { "?x" }, new String[] { "?s" });
        }

        [TestMethod]
        public void SparqlOptimiserAlgebraVarSub5()
        {
            try
            {
                this._parser.SyntaxMode = SparqlQuerySyntax.Extended;

                String query = "SELECT * WHERE { ?s ?p ?o . LET (?blank := ISBLANK(?s)) }";
                SparqlQuery q = this._parser.ParseFromString(query);
                this.TestSubstitution(q, "s", "x", new String[] { "?x" }, new String[] { "?s" });
            }
            finally
            {
                this._parser.SyntaxMode = SparqlQuerySyntax.Sparql_1_1;
            }
        }

        [TestMethod]
        public void SparqlOptimiserAlgebraVarSub6()
        {
            String query = "SELECT * WHERE { ?s ?p ?o . FILTER(EXISTS { ?s a ?type }) }";
            SparqlQuery q = this._parser.ParseFromString(query);
            this.TestSubstitution(q, "s", "x", new String[] { "?x" }, new String[] { "?s" });
        }

        [TestMethod]
        public void SparqlOptimiserAlgebraVarSub7()
        {
            String query = "SELECT * WHERE { ?s ?p ?o . { ?s a ?type } }";
            SparqlQuery q = this._parser.ParseFromString(query);
            this.TestSubstitution(q, "s", "x", new String[] { "?x" }, new String[] { "?s" });
        }

        [TestMethod]
        public void SparqlOptimiserAlgebraVarSub8()
        {
            String query = "SELECT * WHERE { ?s ?p ?o . OPTIONAL { ?s a ?type } }";
            SparqlQuery q = this._parser.ParseFromString(query);
            this.TestSubstitution(q, "s", "x", new String[] { "?x", "OPTIONAL" }, new String[] { "?s" });
        }

        [TestMethod]
        public void SparqlOptimiserAlgebraVarSub9()
        {
            String query = "SELECT * WHERE { ?s ?p ?o . GRAPH <http://example.org/graph> { ?s a ?type } }";
            SparqlQuery q = this._parser.ParseFromString(query);
            this.TestSubstitution(q, "s", "x", new String[] { "?x", "GRAPH" }, new String[] { "?s" });
        }

        [TestMethod]
        public void SparqlOptimiserAlgebraVarSub10()
        {
            String query = "SELECT * WHERE { ?s ?p ?o . MINUS { ?s a ?type } }";
            SparqlQuery q = this._parser.ParseFromString(query);
            this.TestSubstitution(q, "s", "x", new String[] { "?x", "MINUS" }, new String[] { "?s" });
        }

        [TestMethod]
        public void SparqlOptimiserAlgebraVarSub11()
        {
            String query = "SELECT * WHERE { { ?s ?p ?o . } UNION { ?s a ?type } }";
            SparqlQuery q = this._parser.ParseFromString(query);
            this.TestSubstitution(q, "s", "x", new String[] { "?x", "UNION" }, new String[] { "?s" });
        }

        [TestMethod]
        public void SparqlOptimiserAlgebraVarSubBad1()
        {
            String query = "SELECT * WHERE { ?s <http://predicate>+ ?o }";
            SparqlQuery q = this._parser.ParseFromString(query);
            this.TestSubstitution(q, "s", "x", new String[] { "?s", "+" }, new String[] { "?x" });
        }

        [TestMethod]
        public void SparqlOptimiserAlgebraVarSubBad2()
        {
            String query = "SELECT * WHERE { { SELECT * WHERE { ?s ?p ?o } } }";
            SparqlQuery q = this._parser.ParseFromString(query);
            this.TestSubstitution(q, "s", "x", new String[] { "?s" }, new String[] { "?x" });
        }

        [TestMethod]
        public void SparqlOptimiserAlgebraVarSubBad3()
        {
            String query = "SELECT * WHERE { SERVICE <http://example.org/sparql> { ?s ?p ?o } }";
            SparqlQuery q = this._parser.ParseFromString(query);
            this.TestSubstitution(q, "s", "x", new String[] { "?s", "SERVICE" }, new String[] { "?x" });
        }
    }
}
