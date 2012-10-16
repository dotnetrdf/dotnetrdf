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
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class ParsingTests
    {
        private SparqlQueryParser _parser = new SparqlQueryParser();

        private void TestQuery(String query)
        {
            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(q.ToString());
            Console.WriteLine();
            Console.WriteLine(q.ToAlgebra().ToString());
        }

        [TestMethod]
        public void SparqlParsingComplexGraphAfterUnion()
        {
            String query = "SELECT * WHERE {{?x ?y ?z} UNION {?z ?y ?x} GRAPH ?g {?x ?y ?z}}";
            this.TestQuery(query);
        }

        [TestMethod]
        public void SparqlParsingComplexFilterAfterUnion()
        {
            String query = "SELECT * WHERE {{?x ?y ?z} UNION {?z ?y ?x} FILTER(true)}";
            this.TestQuery(query);
        }

        [TestMethod]
        public void SparqlParsingComplexOptionalAfterUnion()
        {
            String query = "SELECT * WHERE {{?x ?y ?z} UNION {?z ?y ?x} OPTIONAL {?x a ?u}}";
            this.TestQuery(query);
        }

        [TestMethod]
        public void SparqlParsingComplexMinusAfterUnion()
        {
            String query = "SELECT * WHERE {{?x ?y ?z} UNION {?z ?y ?x} MINUS {?s ?p ?o}}";
            this.TestQuery(query);
        }

        [TestMethod]
        public void SparqlParsingComplexOptionalServiceUnion()
        {
            String query = "SELECT * WHERE {{?x ?y ?z} UNION {?z ?y ?x} SERVICE ?g {?x ?y ?z}}";
            this.TestQuery(query);
        }

        [TestMethod]
        public void SparqlParsingSingleSubQuery()
        {
            String query = "SELECT * WHERE {{SELECT * WHERE {?s ?p ?o}}}";
            this.TestQuery(query);
        }

        [TestMethod]
        public void SparqlParsingSetExpression()
        {
            String query = "SELECT * WHERE { ?s ?p ?o . FILTER(?o IN (1, 2, 3)) }";
            TestQuery(query);
        }

        [TestMethod]
        public void SparqlParsingSetExpression2()
        {
            String query = "PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA + "> SELECT * WHERE { ?s ?p ?o . FILTER(xsd:integer(?o) IN (1, 2, 3)) }";
            TestQuery(query);
        }

        [TestMethod]
        public void SparqlParsingSetExpression3()
        {
            String query = "PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA + "> SELECT * WHERE { ?s ?p ?o . FILTER(xsd:integer(?o) NOT IN (1, 2, 3)) }";
            TestQuery(query);
        }

        [TestMethod]
        public void SparqlParsingSetExpression4()
        {
            String query = "PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA + "> SELECT * WHERE { ?s ?p ?o . FILTER(1 + 3 IN (1, 2, 3)) }";
            TestQuery(query);
        }

        [TestMethod]
        public void SparqlParsingPropertyPathsNegatedSetSimple()
        {
            String query = "SELECT * WHERE { ?s !a ?o }";
            TestQuery(query);
        }

        [TestMethod]
        public void SparqlParsingPropertyPathsNegatedSetSimpleInSequence1()
        {
            String query = "SELECT * WHERE { ?s a / !a ?o }";
            TestQuery(query);
        }

        [TestMethod]
        public void SparqlParsingPropertyPathsNegatedSetSimpleInSequence2()
        {
            String query = "SELECT * WHERE { ?s !a / a ?o }";
            TestQuery(query);
        }

        [TestMethod]
        public void SparqlParsingPropertyPathsNegatedSet()
        {
            String query = "PREFIX rdfs: <" + NamespaceMapper.RDFS + "> SELECT * WHERE { ?s !(rdfs:label|rdfs:comment) ?o }";
            TestQuery(query);
        }

        [TestMethod]
        public void SparqlParsingPropertyPathsNegatedSetModified()
        {
            String query = "PREFIX rdfs: <" + NamespaceMapper.RDFS + "> SELECT * WHERE { ?s !(rdfs:label|rdfs:comment)+ ?o }";
            TestQuery(query);
        }

        [TestMethod]
        public void SparqlParsingPropertyPathsNegatedSetSimpleInAlternative()
        {
            String query = "PREFIX rdfs: <" + NamespaceMapper.RDFS + "> SELECT * WHERE { ?s (rdfs:label|!rdfs:comment) ?o }";
            TestQuery(query);
        }

        [TestMethod]
        public void SparqlParsingNoGapPrefixes()
        {
            String query;
            using (StreamReader reader = new StreamReader("no-gap-prefixes.rq"))
            {
                query = reader.ReadToEnd();
                reader.Close();
            }
            TestQuery(query);
        }

        [TestMethod]
        public void SparqlParsingPropertyPathsUnbracketedAlternatives()
        {
            String query = @"PREFIX : <http://www.example.org/>

SELECT ?X WHERE
{ 
  [ :p|:q|:r ?X ]
}";
            TestQuery(query);
        }

        [TestMethod]
        public void SparqlVarNames()
        {
            List<String> names = new List<String>
            {
                "?var",
                "$var",
                "var"
            };

            foreach (String name in names)
            {
                SparqlVariable var = new SparqlVariable(name);
                Assert.AreEqual("var", var.Name);

                VariablePattern varPat = new VariablePattern(name);
                Assert.AreEqual("var", varPat.VariableName);

                VariableTerm varTerm = new VariableTerm(name);
                Assert.AreEqual("var", varTerm.Variables.First());
            }
        }
    }
}
