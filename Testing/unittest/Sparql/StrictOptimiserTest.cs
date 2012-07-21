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
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
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
                Assert.IsTrue(output.Contains(op), "Should have contained " + op + " Operator");
            }
        }

        [TestMethod]
        public void SparqlAlgebraOptimiserStrict1()
        {
            this.TestStrictOptimiser("SELECT * WHERE { ?s ?p ?o }", new String[] { "BGP" });
        }

        [TestMethod]
        public void SparqlAlgebraOptimiserStrict2()
        {
            this.TestStrictOptimiser("SELECT * WHERE { ?s ?p ?o . FILTER(ISLITERAL(?o)) }", new String[] { "BGP", "Filter", "Filter(BGP(" });
        }

        [TestMethod]
        public void SparqlAlgebraOptimiserStrict3()
        {
            this.TestStrictOptimiser("SELECT * WHERE { ?s ?p ?o . FILTER(ISURI(?type)) . ?s a ?type }", new String[] { "BGP", "Filter", "Filter(BGP(", "Join" });
        }

        [TestMethod]
        public void SparqlAlgebraOptimiserStrict4()
        {
            this.TestStrictOptimiser("SELECT * WHERE { ?s ?p ?o . BIND(ISLITERAL(?o) AS ?hasLiteralObject) }", new String[] { "BGP", "Extend", "Extend(BGP(" });
        }

        [TestMethod]
        public void SparqlAlgebraOptimiserStrict5()
        {
            this.TestStrictOptimiser("SELECT * WHERE { ?s ?p ?o . BIND(ISURI(?type) AS ?hasNamedType) . ?s a ?type }", new String[] { "BGP", "Extend", "Extend(BGP(", "Join" });
        }

        [TestMethod]
        public void SparqlAlgebraOptimiserStrict6()
        {
            this.TestStrictOptimiser("SELECT * WHERE { ?s ?p ?o . FILTER(ISLITERAL(?o)) . BIND(ISURI(?s) AS ?Named) }", new String[] { "BGP", "Extend", "Filter", "Filter(BGP(", "Extend(Filter(" });
        }

        [TestMethod]
        public void SparqlAlgebraOptimiserStrict7()
        {
            this.TestStrictOptimiser("SELECT * WHERE { ?s ?p ?o . FILTER(ISURI(?type)) . ?s a ?type . BIND(ISURI(?s) AS ?Named) }", new String[] { "BGP", "Extend", "Filter", "Filter(BGP(", "Extend(Filter(" });
        }

        [TestMethod]
        public void SparqlAlgebraOptimiserStrict8()
        {
            this.TestStrictOptimiser("SELECT * WHERE { ?s ?p ?o . FILTER(?Named) . ?s a ?type . BIND(ISURI(?s) AS ?Named) }", new String[] { "BGP", "Extend", "Filter", "Extend(BGP(", "Filter(Extend(" });
        }
    }
}
