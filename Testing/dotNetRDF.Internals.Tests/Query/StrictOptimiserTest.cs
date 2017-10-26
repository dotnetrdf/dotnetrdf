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
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query
{

    public class StrictOptimiserTest
    {
        private SparqlQueryParser _parser = new SparqlQueryParser();
        private SparqlFormatter _formatter = new SparqlFormatter();
        private StrictAlgebraOptimiser _optimiser = new StrictAlgebraOptimiser();

        private void TestStrictOptimiser(String query, String[] expectedOperators)
        {
            SparqlQuery q = this._parser.ParseFromString(query);
            Console.WriteLine("Query:");
            Console.WriteLine(this._formatter.Format(q));
            Console.WriteLine();

            q.AlgebraOptimisers = new IAlgebraOptimiser[] { this._optimiser };
            ISparqlAlgebra algebra = q.ToAlgebra();
            String output = algebra.ToString();
            Console.WriteLine("Algebra:");
            Console.WriteLine(output);
            Console.WriteLine();

            foreach (String op in expectedOperators)
            {
                Assert.True(output.Contains(op), "Should have contained " + op + " Operator");
            }
        }

        [Fact]
        public void SparqlAlgebraOptimiserStrict1()
        {
            this.TestStrictOptimiser("SELECT * WHERE { ?s ?p ?o }", new String[] { "BGP" });
        }

        [Fact]
        public void SparqlAlgebraOptimiserStrict2()
        {
            this.TestStrictOptimiser("SELECT * WHERE { ?s ?p ?o . FILTER(ISLITERAL(?o)) }", new String[] { "BGP", "Filter", "Filter(BGP(" });
        }

        [Fact]
        public void SparqlAlgebraOptimiserStrict3()
        {
            this.TestStrictOptimiser("SELECT * WHERE { ?s ?p ?o . FILTER(ISURI(?type)) . ?s a ?type }", new String[] { "BGP", "Filter", "Filter(BGP(", "Join" });
        }

        [Fact]
        public void SparqlAlgebraOptimiserStrict4()
        {
            this.TestStrictOptimiser("SELECT * WHERE { ?s ?p ?o . BIND(ISLITERAL(?o) AS ?hasLiteralObject) }", new String[] { "BGP", "Extend", "Extend(BGP(" });
        }

        [Fact]
        public void SparqlAlgebraOptimiserStrict5()
        {
            this.TestStrictOptimiser("SELECT * WHERE { ?s ?p ?o . BIND(ISURI(?type) AS ?hasNamedType) . ?s a ?type }", new String[] { "BGP", "Extend", "Extend(BGP(", "Join" });
        }

        [Fact]
        public void SparqlAlgebraOptimiserStrict6()
        {
            this.TestStrictOptimiser("SELECT * WHERE { ?s ?p ?o . FILTER(ISLITERAL(?o)) . BIND(ISURI(?s) AS ?Named) }", new String[] { "BGP", "Extend", "Filter", "Filter(BGP(", "Extend(Filter(" });
        }

        [Fact]
        public void SparqlAlgebraOptimiserStrict7()
        {
            this.TestStrictOptimiser("SELECT * WHERE { ?s ?p ?o . FILTER(ISURI(?type)) . ?s a ?type . BIND(ISURI(?s) AS ?Named) }", new String[] { "BGP", "Extend", "Filter", "Filter(BGP(", "Extend(Filter(" });
        }

        [Fact]
        public void SparqlAlgebraOptimiserStrict8()
        {
            this.TestStrictOptimiser("SELECT * WHERE { ?s ?p ?o . FILTER(?Named) . ?s a ?type . BIND(ISURI(?s) AS ?Named) }", new String[] { "BGP", "Extend", "Filter", "Extend(BGP(", "Filter(Extend(" });
        }
    }
}
