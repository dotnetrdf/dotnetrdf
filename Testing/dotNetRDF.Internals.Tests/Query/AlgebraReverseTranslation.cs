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
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF
{

    public class AlgebraReverseTranslation
    {
        [Fact]
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
