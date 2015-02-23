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
using NUnit.Framework;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Update;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query
{
    [TestFixture]
    public class ParsingTests
    {
        private SparqlQueryParser _parser = new SparqlQueryParser();
        private SparqlUpdateParser _updateParser = new SparqlUpdateParser();

        private SparqlQuery TestQuery(String query)
        {
            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine(q.ToString());
            Console.WriteLine();
            Console.WriteLine(q.ToAlgebra().ToString());

            return q;
        }

        private SparqlUpdateCommandSet TestUpdate(String update)
        {
            SparqlUpdateCommandSet cmds = this._updateParser.ParseFromString(update);

            Console.WriteLine(cmds.ToString());

            return cmds;
        }

        [Test]
        public void SparqlParsingComplexGraphAfterUnion()
        {
            String query = "SELECT * WHERE {{?x ?y ?z} UNION {?z ?y ?x} GRAPH ?g {?x ?y ?z}}";
            this.TestQuery(query);
        }

        [Test]
        public void SparqlParsingComplexFilterAfterUnion()
        {
            String query = "SELECT * WHERE {{?x ?y ?z} UNION {?z ?y ?x} FILTER(true)}";
            this.TestQuery(query);
        }

        [Test]
        public void SparqlParsingComplexOptionalAfterUnion()
        {
            String query = "SELECT * WHERE {{?x ?y ?z} UNION {?z ?y ?x} OPTIONAL {?x a ?u}}";
            this.TestQuery(query);
        }

        [Test]
        public void SparqlParsingComplexMinusAfterUnion()
        {
            String query = "SELECT * WHERE {{?x ?y ?z} UNION {?z ?y ?x} MINUS {?s ?p ?o}}";
            this.TestQuery(query);
        }

        [Test]
        public void SparqlParsingComplexOptionalServiceUnion()
        {
            String query = "SELECT * WHERE {{?x ?y ?z} UNION {?z ?y ?x} SERVICE ?g {?x ?y ?z}}";
            this.TestQuery(query);
        }

        [Test]
        public void SparqlParsingSingleSubQuery()
        {
            String query = "SELECT * WHERE {{SELECT * WHERE {?s ?p ?o}}}";
            this.TestQuery(query);
        }

        [Test]
        public void SparqlParsingSetExpression()
        {
            String query = "SELECT * WHERE { ?s ?p ?o . FILTER(?o IN (1, 2, 3)) }";
            TestQuery(query);
        }

        [Test]
        public void SparqlParsingSetExpression2()
        {
            String query = "PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA +
                           "> SELECT * WHERE { ?s ?p ?o . FILTER(xsd:integer(?o) IN (1, 2, 3)) }";
            TestQuery(query);
        }

        [Test]
        public void SparqlParsingSetExpression3()
        {
            String query = "PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA +
                           "> SELECT * WHERE { ?s ?p ?o . FILTER(xsd:integer(?o) NOT IN (1, 2, 3)) }";
            TestQuery(query);
        }

        [Test]
        public void SparqlParsingSetExpression4()
        {
            String query = "PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA +
                           "> SELECT * WHERE { ?s ?p ?o . FILTER(1 + 3 IN (1, 2, 3)) }";
            TestQuery(query);
        }

        [Test]
        public void SparqlParsingNumericExpression1()
        {
            String query = "PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA +
                           "> SELECT * WHERE { ?s ?p ?o . FILTER(1 + '3'^^xsd:long) }";
            TestQuery(query);
        }

        [Test]
        public void SparqlParsingNumericExpression2()
        {
            String query = "PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA +
                           "> SELECT * WHERE { ?s ?p ?o . FILTER(1 + '3'^^xsd:short) }";
            TestQuery(query);
        }

        [Test]
        public void SparqlParsingNumericExpression3()
        {
            String query = "PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA +
                           "> SELECT * WHERE { ?s ?p ?o . FILTER(1 + '3'^^xsd:byte) }";
            TestQuery(query);
        }

        [Test]
        public void SparqlParsingNumericExpression4()
        {
            String query = "PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA +
                           "> SELECT * WHERE { ?s ?p ?o . FILTER(1 + '3'^^xsd:nonPositiveInteger) }";
            TestQuery(query);
        }

        [Test]
        public void SparqlParsingPropertyPathsNegatedSetSimple()
        {
            String query = "SELECT * WHERE { ?s !a ?o }";
            TestQuery(query);
        }

        [Test]
        public void SparqlParsingPropertyPathsNegatedSetSimpleInSequence1()
        {
            String query = "SELECT * WHERE { ?s a / !a ?o }";
            TestQuery(query);
        }

        [Test]
        public void SparqlParsingPropertyPathsNegatedSetSimpleInSequence2()
        {
            String query = "SELECT * WHERE { ?s !a / a ?o }";
            TestQuery(query);
        }

        [Test]
        public void SparqlParsingPropertyPathsNegatedSet()
        {
            String query = "PREFIX rdfs: <" + NamespaceMapper.RDFS +
                           "> SELECT * WHERE { ?s !(rdfs:label|rdfs:comment) ?o }";
            TestQuery(query);
        }

        [Test]
        public void SparqlParsingPropertyPathsNegatedSetModified()
        {
            String query = "PREFIX rdfs: <" + NamespaceMapper.RDFS +
                           "> SELECT * WHERE { ?s !(rdfs:label|rdfs:comment)+ ?o }";
            TestQuery(query);
        }

        [Test]
        public void SparqlParsingPropertyPathsNegatedSetSimpleInAlternative()
        {
            String query = "PREFIX rdfs: <" + NamespaceMapper.RDFS +
                           "> SELECT * WHERE { ?s (rdfs:label|!rdfs:comment) ?o }";
            TestQuery(query);
        }

        [Test]
        public void SparqlParsingNoGapPrefixes()
        {
            String query;
            using (StreamReader reader = new StreamReader("resources\\no-gap-prefixes.rq"))
            {
                query = reader.ReadToEnd();
                reader.Close();
            }
            TestQuery(query);
        }

        [Test]
        public void SparqlParsingPropertyPathsUnbracketedAlternatives()
        {
            String query = @"PREFIX : <http://www.example.org/>

SELECT ?X WHERE
{ 
  [ :p|:q|:r ?X ]
}";
            TestQuery(query);
        }

        [Test]
        public void SparqlParsingExistsWithinSubQuery1()
        {
            String query = "SELECT * WHERE { { SELECT ?s WHERE { ?s a ?type FILTER NOT EXISTS { ?s a <http://restricted> } } } }";
            TestQuery(query);
        }

        [Test]
        public void SparqlParsingExistsWithinSubQuery2()
        {
            String query = "SELECT * WHERE { { SELECT ?s WHERE { ?s a ?type FILTER NOT EXISTS { { ?s a <http://restricted> } UNION { ?s a <http://other> } } } } }";
            TestQuery(query);
        }

        [Test]
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

        [Test, ExpectedException(typeof (RdfParseException))]
        public void SparqlParsingInsertDataWithGraphVar()
        {
            String update = "INSERT DATA { GRAPH ?g { } }";
            TestUpdate(update);
        }

        [Test, ExpectedException(typeof (RdfParseException))]
        public void SparqlParsingDeleteDataWithGraphVar()
        {
            String update = "DELETE DATA { GRAPH ?g { } }";
            TestUpdate(update);
        }

        [Test]
        public void SparqlParsingExistsWithinUpdateWhereClause1()
        {
            String update = @"WITH <http://source>
DELETE { GRAPH <http://source> { ?s ?p ?o } }
INSERT { GRAPH <http://target> { ?s ?p ?o } } 
WHERE { ?s ?p ?o . FILTER NOT EXISTS { ?s a <http://restricted> } }";
            TestUpdate(update);
        }

        [Test]
        public void SparqlParsingExistsWithinUpdateWhereClause2()
        {
            String update = @"WITH <http://source>
INSERT { GRAPH <http://target> { ?s ?p ?o } } 
WHERE { ?s ?p ?o . FILTER NOT EXISTS { ?s a <http://restricted> } }";
            TestUpdate(update);
        }

        [Test]
        public void SparqlParsingExistsWithinUpdateWhereClause3()
        {
            String update = @"WITH <http://source>
DELETE { GRAPH <http://source> { ?s ?p ?o } }
WHERE { ?s ?p ?o . FILTER NOT EXISTS { ?s a <http://restricted> } }";
            TestUpdate(update);
        }

        [Test]
        public void SparqlParsingExistsWithinUpdateWhereClause4()
        {
            String update = @"DELETE { GRAPH <http://source> { ?s ?p ?o } }
WHERE { GRAPH <htp://source> { ?s ?p ?o  } . FILTER NOT EXISTS { ?s a <http://restricted> } }";
            TestUpdate(update);
        }

        [Test]
        public void SparqlParsingExistsWithinUpdateWhereClause5()
        {
            String update = @"INSERT { GRAPH <http://target> { ?s ?p ?o } }
WHERE { GRAPH <htp://source> { ?s ?p ?o } . FILTER NOT EXISTS { ?s a <http://restricted> } }";
            TestUpdate(update);
        }

        [Test]
        public void SparqlParsingExistsWithinUpdateWhereClause6()
        {
            String update = @"PREFIX myschema: <http://www.example.com/schema#>
    INSERT {
        GRAPH <data:public> {
            ?s ?p ?o
        }
    } WHERE {
        GRAPH <input:source> {
            ?s ?p ?o .
        } .
        FILTER( NOT EXISTS {
            {?p a myschema:PrivateProperty}
            UNION { ?s a myschema:PrivateResource }
            UNION { ?o a myschema:PrivateResource }
        })
    }";
            TestUpdate(update);
        }

        [Test]
        public void SparqlParsingExistsWithinUpdateWhereClause7()
        {
            String update = @"DELETE { GRAPH <http://source> { ?s ?p ?o } }
WHERE { GRAPH <htp://source> { ?s ?p ?o  } . FILTER (NOT EXISTS { ?s a <http://restricted> }) }";
            TestUpdate(update);
        }

        [Test]
        public void SparqlParsingExistsWithinUpdateWhereClause8()
        {
            String update = @"INSERT { GRAPH <http://target> { ?s ?p ?o } }
WHERE { GRAPH <htp://source> { ?s ?p ?o } . FILTER (NOT EXISTS { ?s a <http://restricted> }) }";
            TestUpdate(update);
        }

        [Test]
        public void SparqlParsingLiteralsInExpressions()
        {
            Queue<IToken> tokens = new Queue<IToken>();
            tokens.Enqueue(new LiteralToken("value", 0, 0, 0));
            tokens.Enqueue(new HatHatToken(0, 0));
            tokens.Enqueue(new DataTypeToken("<http://example/type>", 0, 0, 0));

            SparqlExpressionParser parser = new SparqlExpressionParser();
            ISparqlExpression expr = parser.Parse(tokens);

            Assert.IsInstanceOf(typeof (ConstantTerm), expr);
            ConstantTerm constant = expr as ConstantTerm;
            Assert.IsNotNull(constant);

            IValuedNode n = constant.Node;
            Assert.IsInstanceOf(typeof(ILiteralNode), n);
            ILiteralNode lit = (ILiteralNode) n;
            Assert.AreEqual(String.Empty, lit.Language);
            Assert.IsTrue(EqualityHelper.AreUrisEqual(lit.DataType, new Uri("http://example/type")));
        }
        
        [Test]
        public void SparqlParsingHandlesDollarSignInUriParameter1()
        {
            const string queryString = @"SELECT ?p ?o WHERE { @subject ?p ?o . }";
            const string expectedCondition = @"<http://dbpedia.org/resource/$_(film)> ?p ?o";
            var uri = new Uri("http://dbpedia.org/resource/$_(film)");

            var parametrizedQueryString = new SparqlParameterizedString(queryString);
            parametrizedQueryString.SetUri("subject", uri);
            var sparqlQuery = new SparqlQueryParser().ParseFromString(parametrizedQueryString);
            Console.WriteLine(sparqlQuery.ToString());

            Assert.That(sparqlQuery.ToString(), Contains.Substring(expectedCondition));
        }

        [Test]
        public void SparqlParsingHandlesDollarSignInUriParameter2()
        {
            const string queryString = @"SELECT ?p ?o WHERE { $subject ?p ?o . }";
            const string expectedCondition = @"<http://dbpedia.org/resource/$_(film)> ?p ?o";
            var uri = new Uri("http://dbpedia.org/resource/$_(film)");

            var parametrizedQueryString = new SparqlParameterizedString(queryString);
            parametrizedQueryString.SetVariable("subject", new UriNode(null, uri));
            var sparqlQuery = new SparqlQueryParser().ParseFromString(parametrizedQueryString);
            Console.WriteLine(sparqlQuery.ToString());

            Assert.That(sparqlQuery.ToString(), Contains.Substring(expectedCondition));
        }

        [Test]
        public void SparqlParsingExcessTokens1()
        {
            const String query = @"PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
SELECT * WHERE
{
  ?s rdf:type ?type ?p ?o
}
";
            try
            {
                this._parser.ParseFromString(query);
                Assert.Fail("Did not error as expected");
            }
            catch (RdfParseException parseEx)
            {
                Console.WriteLine(parseEx.Message);
                Assert.IsFalse(parseEx.Message.Contains("?s"));
                Assert.IsFalse(parseEx.Message.Contains("rdf:type"));
                Assert.IsFalse(parseEx.Message.Contains("?type"));
                Assert.IsTrue(parseEx.Message.Contains("?p"));
                Assert.IsTrue(parseEx.Message.Contains("?o"));
            }
        }

        [Test]
        public void SparqlParsingExcessTokens2()
        {
            const String query = @"PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
SELECT * WHERE
{
  ?s rdf:type ?type ?p ?o .
}
";
            try
            {
                this._parser.ParseFromString(query);
                Assert.Fail("Did not error as expected");
            }
            catch (RdfParseException parseEx)
            {
                Console.WriteLine(parseEx.Message);
                Assert.IsFalse(parseEx.Message.Contains("?s"));
                Assert.IsFalse(parseEx.Message.Contains("rdf:type"));
                Assert.IsFalse(parseEx.Message.Contains("?type"));
                Assert.IsTrue(parseEx.Message.Contains("?p"));
                Assert.IsTrue(parseEx.Message.Contains("?o"));
            }
        }

        [Test]
        public void SparqlParsingExcessTokens3()
        {
            const String query = @"PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
SELECT * WHERE
{
  ?s rdf:type ?type ?p ?o ;
}
";
            try
            {
                this._parser.ParseFromString(query);
                Assert.Fail("Did not error as expected");
            }
            catch (RdfParseException parseEx)
            {
                Console.WriteLine(parseEx.Message);
                Assert.IsFalse(parseEx.Message.Contains("?s"));
                Assert.IsFalse(parseEx.Message.Contains("rdf:type"));
                Assert.IsFalse(parseEx.Message.Contains("?type"));
                Assert.IsTrue(parseEx.Message.Contains("?p"));
                Assert.IsTrue(parseEx.Message.Contains("?o"));
            }
        }

        [Test]
        public void SparqlParsingExcessTokens4()
        {
            const String query = @"PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
SELECT * WHERE
{
  ?s rdf:type ?type ?p ?o ,
}
";
            try
            {
                this._parser.ParseFromString(query);
                Assert.Fail("Did not error as expected");
            }
            catch (RdfParseException parseEx)
            {
                Console.WriteLine(parseEx.Message);
                Assert.IsFalse(parseEx.Message.Contains("?s"));
                Assert.IsFalse(parseEx.Message.Contains("rdf:type"));
                Assert.IsFalse(parseEx.Message.Contains("?type"));
                Assert.IsTrue(parseEx.Message.Contains("?p"));
                Assert.IsTrue(parseEx.Message.Contains("?o"));
            }
        }

        [Test]
        public void SparqlParsingComplexCore428_1()
        {
            // Distilled from CORE-428 report
            const String query = @"SELECT *
WHERE
{
  OPTIONAL 
  {
    { SELECT * WHERE { } }
    BIND('test' AS ?test)
  }
}";

            // Should be a valid query
            this._parser.ParseFromString(query);
        }

        [Test]
        public void SparqlParsingComplexCore428_2()
        {
            // Distilled from CORE-428 report
            const String query = @"SELECT *
WHERE
{
  OPTIONAL 
  {
    OPTIONAL { }
    BIND('test' AS ?test)
  }
}";

            // Should be a valid query
            this._parser.ParseFromString(query);
        }

        [Test]
        public void SparqlParsingComplexCore428_3()
        {
            // Distilled from CORE-428 report
            const String query = @"SELECT *
WHERE
{
  OPTIONAL 
  {
    { } UNION { }
    BIND('test' AS ?test)
  }
}";

            // Should be a valid query
            this._parser.ParseFromString(query);
        }

        [Test]
        public void SparqlParsingComplexCore428_4()
        {
            // Distilled from CORE-428 report
            const String query = @"SELECT *
WHERE
{
  OPTIONAL 
  {
    MINUS { }
    BIND('test' AS ?test)
  }
}";

            // Should be a valid query
            this._parser.ParseFromString(query);
        }

        [Test]
        public void SparqlParsingComplexCore428_5()
        {
            // Distilled from CORE-428 report
            const String query = @"SELECT *
WHERE
{
  OPTIONAL 
  {
    { }
    BIND('test' AS ?test)
  }
}";

            // Should be a valid query
            this._parser.ParseFromString(query);
        }

        [Test]
        public void SparqlParsingCore427_1()
        {
            const String query = "SELECT (UUID() AS ?test) { }";

            SparqlQuery q = this._parser.ParseFromString(query);

            String toString = q.ToString();
            Assert.IsTrue(toString.Contains("(UUID"));

            String formattedString = new SparqlFormatter().Format(q);
            Assert.IsTrue(formattedString.Contains("(UUID"));
        }

        [Test]
        public void SparqlParsingCore427_2()
        {
            const String query = "SELECT (StrUUID() AS ?test) { }";

            SparqlQuery q = this._parser.ParseFromString(query);

            String toString = q.ToString();
            Assert.IsTrue(toString.Contains("(STRUUID"));

            String formattedString = new SparqlFormatter().Format(q);
            Assert.IsTrue(formattedString.Contains("(STRUUID"));
        }

        [Test, ExpectedException(typeof(RdfParseException))]
        public void SparqlParsingIllegalWhitespaceInUris()
        {
            const string query = "SELECT * WHERE { <http://example.com/foo bar> a <http://example.com/foo%20type> }";
            SparqlQuery q = this._parser.ParseFromString(query);
            var pattern = q.RootGraphPattern.TriplePatterns[0] as IMatchTriplePattern;
            var subjectMatch = pattern.Subject as NodeMatchPattern;
            Assert.AreEqual(new Uri("http://example.com/foo bar"), ((IUriNode)subjectMatch.Node).Uri);
        }
        
        [Test]
        public void SparqlParsingEscapedWhitespaceInUris()
        {
            const string query = "SELECT * WHERE { <http://example.com/foo%20bar> a <http://example.com/foo%20type> }";
            SparqlQuery q = this._parser.ParseFromString(query);
            var pattern = q.RootGraphPattern.TriplePatterns[0] as IMatchTriplePattern;
            var subjectMatch = pattern.Subject as NodeMatchPattern;
            Assert.AreEqual(new Uri("http://example.com/foo bar"), ((IUriNode)subjectMatch.Node).Uri);
        }
    }
}