/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

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
        public void SparqlAlgebraReverseSimple()
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
