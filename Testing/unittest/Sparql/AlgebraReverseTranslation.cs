using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Test
{
    [TestClass]
    public class AlgebraReverseTranslation
    {
        [TestMethod]
        public void AlgebraReverseSimple()
        {
            List<String> tests = new List<string>()
            {
                "SELECT * WHERE { ?s ?p ?o }",
                "SELECT ?s WHERE { ?s ?p ?o }",
                "SELECT ?p ?o WHERE { ?s ?p ?o }",
                "PREFIX fn: <" + XPathFunctionFactory.XPathFunctionsNamespace + "> SELECT (fn:concat(?s, ?p) AS ?string) WHERE {?s ?p ?o}",
                "SELECT * WHERE {?s ?p ?o . ?o ?x ?y }",
                "SELECT * WHERE {?s ?p ?o . OPTIONAL {?o ?x ?y} }",
                "SELECT * WHERE {?s ?p ?o . OPTIONAL {?o ?x ?y . FILTER(BOUND(?s)) } }"
            };

            SparqlQueryParser parser = new SparqlQueryParser();
            foreach (String test in tests)
            {
                Console.WriteLine("Test Input:");
                Console.WriteLine(test);
                Console.WriteLine();
                Console.WriteLine("Parsed Query:");
                SparqlQuery q = parser.ParseFromString(test);
                Console.WriteLine(q.ToString());
                Console.WriteLine();
                Console.WriteLine("Algebra:");
                Console.WriteLine(q.ToAlgebra().ToString());
                Console.WriteLine();
                Console.WriteLine("Algebra Reverse Transformed to Query:");
                Console.WriteLine(q.ToAlgebra().ToQuery().ToString());
                Console.WriteLine();
                Console.WriteLine();
            }
        }
    }
}
