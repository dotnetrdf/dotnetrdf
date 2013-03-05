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
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Update;

namespace VDS.RDF.Query
{
    [TestClass]
    public class ParsingTests
    {
        private SparqlQueryParser _parser = new SparqlQueryParser();
        private SparqlUpdateParser _updateParser = new SparqlUpdateParser();

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
        public void SparqlParsingNumericExpression1()
        {
            String query = "PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA + "> SELECT * WHERE { ?s ?p ?o . FILTER(1 + '3'^^xsd:long) }";
            TestQuery(query);
        }

        [TestMethod]
        public void SparqlParsingNumericExpression2()
        {
            String query = "PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA + "> SELECT * WHERE { ?s ?p ?o . FILTER(1 + '3'^^xsd:short) }";
            TestQuery(query);
        }

        [TestMethod]
        public void SparqlParsingNumericExpression3()
        {
            String query = "PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA + "> SELECT * WHERE { ?s ?p ?o . FILTER(1 + '3'^^xsd:byte) }";
            TestQuery(query);
        }

        [TestMethod]
        public void SparqlParsingNumericExpression4()
        {
            String query = "PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA + "> SELECT * WHERE { ?s ?p ?o . FILTER(1 + '3'^^xsd:nonPositiveInteger) }";
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

        [TestMethod,ExpectedException(typeof(RdfParseException))]
        public void SparqlParsingInsertDataWithGraphVar()
        {
            String update = "INSERT DATA { GRAPH ?g { } }";
            SparqlUpdateCommandSet commands = this._updateParser.ParseFromString(update);
        }

        [TestMethod, ExpectedException(typeof(RdfParseException))]
        public void SparqlParsingDeleteDataWithGraphVar()
        {
            String update = "DELETE DATA { GRAPH ?g { } }";
            SparqlUpdateCommandSet commands = this._updateParser.ParseFromString(update);
        }
    }
}
