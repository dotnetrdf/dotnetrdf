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
using Xunit;
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

        [Fact]
        public void SparqlParsingComplexGraphAfterUnion()
        {
            String query = "SELECT * WHERE {{?x ?y ?z} UNION {?z ?y ?x} GRAPH ?g {?x ?y ?z}}";
            this.TestQuery(query);
        }

        [Fact]
        public void SparqlParsingComplexFilterAfterUnion()
        {
            String query = "SELECT * WHERE {{?x ?y ?z} UNION {?z ?y ?x} FILTER(true)}";
            this.TestQuery(query);
        }

        [Fact]
        public void SparqlParsingComplexOptionalAfterUnion()
        {
            String query = "SELECT * WHERE {{?x ?y ?z} UNION {?z ?y ?x} OPTIONAL {?x a ?u}}";
            this.TestQuery(query);
        }

        [Fact]
        public void SparqlParsingComplexMinusAfterUnion()
        {
            String query = "SELECT * WHERE {{?x ?y ?z} UNION {?z ?y ?x} MINUS {?s ?p ?o}}";
            this.TestQuery(query);
        }

        [Fact]
        public void SparqlParsingComplexOptionalServiceUnion()
        {
            String query = "SELECT * WHERE {{?x ?y ?z} UNION {?z ?y ?x} SERVICE ?g {?x ?y ?z}}";
            this.TestQuery(query);
        }

        [Fact]
        public void SparqlParsingSingleSubQuery()
        {
            String query = "SELECT * WHERE {{SELECT * WHERE {?s ?p ?o}}}";
            this.TestQuery(query);
        }

        [Fact]
        public void SparqlParsingSetExpression()
        {
            String query = "SELECT * WHERE { ?s ?p ?o . FILTER(?o IN (1, 2, 3)) }";
            TestQuery(query);
        }

        [Fact]
        public void SparqlParsingSetExpression2()
        {
            String query = "PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA +
                           "> SELECT * WHERE { ?s ?p ?o . FILTER(xsd:integer(?o) IN (1, 2, 3)) }";
            TestQuery(query);
        }

        [Fact]
        public void SparqlParsingSetExpression3()
        {
            String query = "PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA +
                           "> SELECT * WHERE { ?s ?p ?o . FILTER(xsd:integer(?o) NOT IN (1, 2, 3)) }";
            TestQuery(query);
        }

        [Fact]
        public void SparqlParsingSetExpression4()
        {
            String query = "PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA +
                           "> SELECT * WHERE { ?s ?p ?o . FILTER(1 + 3 IN (1, 2, 3)) }";
            TestQuery(query);
        }

        [Fact]
        public void SparqlParsingNumericExpression1()
        {
            String query = "PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA +
                           "> SELECT * WHERE { ?s ?p ?o . FILTER(1 + '3'^^xsd:long) }";
            TestQuery(query);
        }

        [Fact]
        public void SparqlParsingNumericExpression2()
        {
            String query = "PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA +
                           "> SELECT * WHERE { ?s ?p ?o . FILTER(1 + '3'^^xsd:short) }";
            TestQuery(query);
        }

        [Fact]
        public void SparqlParsingNumericExpression3()
        {
            String query = "PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA +
                           "> SELECT * WHERE { ?s ?p ?o . FILTER(1 + '3'^^xsd:byte) }";
            TestQuery(query);
        }

        [Fact]
        public void SparqlParsingNumericExpression4()
        {
            String query = "PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA +
                           "> SELECT * WHERE { ?s ?p ?o . FILTER(1 + '3'^^xsd:nonPositiveInteger) }";
            TestQuery(query);
        }

        [Fact]
        public void SparqlParsingPropertyPathsNegatedSetSimple()
        {
            String query = "SELECT * WHERE { ?s !a ?o }";
            TestQuery(query);
        }

        [Fact]
        public void SparqlParsingPropertyPathsNegatedSetSimpleInSequence1()
        {
            String query = "SELECT * WHERE { ?s a / !a ?o }";
            TestQuery(query);
        }

        [Fact]
        public void SparqlParsingPropertyPathsNegatedSetSimpleInSequence2()
        {
            String query = "SELECT * WHERE { ?s !a / a ?o }";
            TestQuery(query);
        }

        [Fact]
        public void SparqlParsingPropertyPathsNegatedSet()
        {
            String query = "PREFIX rdfs: <" + NamespaceMapper.RDFS +
                           "> SELECT * WHERE { ?s !(rdfs:label|rdfs:comment) ?o }";
            TestQuery(query);
        }

        [Fact]
        public void SparqlParsingPropertyPathsNegatedSetModified()
        {
            String query = "PREFIX rdfs: <" + NamespaceMapper.RDFS +
                           "> SELECT * WHERE { ?s !(rdfs:label|rdfs:comment)+ ?o }";
            TestQuery(query);
        }

        [Fact]
        public void SparqlParsingPropertyPathsNegatedSetSimpleInAlternative()
        {
            String query = "PREFIX rdfs: <" + NamespaceMapper.RDFS +
                           "> SELECT * WHERE { ?s (rdfs:label|!rdfs:comment) ?o }";
            TestQuery(query);
        }

        [Fact]
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

        [Fact]
        public void SparqlParsingPropertyPathsUnbracketedAlternatives()
        {
            String query = @"PREFIX : <http://www.example.org/>

SELECT ?X WHERE
{ 
  [ :p|:q|:r ?X ]
}";
            TestQuery(query);
        }

        [Fact]
        public void SparqlParsingExistsWithinSubQuery1()
        {
            String query = "SELECT * WHERE { { SELECT ?s WHERE { ?s a ?type FILTER NOT EXISTS { ?s a <http://restricted> } } } }";
            TestQuery(query);
        }

        [Fact]
        public void SparqlParsingExistsWithinSubQuery2()
        {
            String query = "SELECT * WHERE { { SELECT ?s WHERE { ?s a ?type FILTER NOT EXISTS { { ?s a <http://restricted> } UNION { ?s a <http://other> } } } } }";
            TestQuery(query);
        }

        [Fact]
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
                Assert.Equal("var", var.Name);

                VariablePattern varPat = new VariablePattern(name);
                Assert.Equal("var", varPat.VariableName);

                VariableTerm varTerm = new VariableTerm(name);
                Assert.Equal("var", varTerm.Variables.First());
            }
        }

        [Fact]
        public void SparqlParsingInsertDataWithGraphVar()
        {
            String update = "INSERT DATA { GRAPH ?g { } }";

            Assert.Throws<RdfParseException>(() => TestUpdate(update));
        }

        [Fact]
        public void SparqlParsingDeleteDataWithGraphVar()
        {
            String update = "DELETE DATA { GRAPH ?g { } }";

            Assert.Throws<RdfParseException>(() => TestUpdate(update));
        }

        [Fact]
        public void SparqlParsingExistsWithinUpdateWhereClause1()
        {
            String update = @"WITH <http://source>
DELETE { GRAPH <http://source> { ?s ?p ?o } }
INSERT { GRAPH <http://target> { ?s ?p ?o } } 
WHERE { ?s ?p ?o . FILTER NOT EXISTS { ?s a <http://restricted> } }";
            TestUpdate(update);
        }

        [Fact]
        public void SparqlParsingExistsWithinUpdateWhereClause2()
        {
            String update = @"WITH <http://source>
INSERT { GRAPH <http://target> { ?s ?p ?o } } 
WHERE { ?s ?p ?o . FILTER NOT EXISTS { ?s a <http://restricted> } }";
            TestUpdate(update);
        }

        [Fact]
        public void SparqlParsingExistsWithinUpdateWhereClause3()
        {
            String update = @"WITH <http://source>
DELETE { GRAPH <http://source> { ?s ?p ?o } }
WHERE { ?s ?p ?o . FILTER NOT EXISTS { ?s a <http://restricted> } }";
            TestUpdate(update);
        }

        [Fact]
        public void SparqlParsingExistsWithinUpdateWhereClause4()
        {
            String update = @"DELETE { GRAPH <http://source> { ?s ?p ?o } }
WHERE { GRAPH <htp://source> { ?s ?p ?o  } . FILTER NOT EXISTS { ?s a <http://restricted> } }";
            TestUpdate(update);
        }

        [Fact]
        public void SparqlParsingExistsWithinUpdateWhereClause5()
        {
            String update = @"INSERT { GRAPH <http://target> { ?s ?p ?o } }
WHERE { GRAPH <htp://source> { ?s ?p ?o } . FILTER NOT EXISTS { ?s a <http://restricted> } }";
            TestUpdate(update);
        }

        [Fact]
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

        [Fact]
        public void SparqlParsingExistsWithinUpdateWhereClause7()
        {
            String update = @"DELETE { GRAPH <http://source> { ?s ?p ?o } }
WHERE { GRAPH <htp://source> { ?s ?p ?o  } . FILTER (NOT EXISTS { ?s a <http://restricted> }) }";
            TestUpdate(update);
        }

        [Fact]
        public void SparqlParsingExistsWithinUpdateWhereClause8()
        {
            String update = @"INSERT { GRAPH <http://target> { ?s ?p ?o } }
WHERE { GRAPH <htp://source> { ?s ?p ?o } . FILTER (NOT EXISTS { ?s a <http://restricted> }) }";
            TestUpdate(update);
        }

        [Fact]
        public void SparqlParsingLiteralsInExpressions()
        {
            Queue<IToken> tokens = new Queue<IToken>();
            tokens.Enqueue(new LiteralToken("value", 0, 0, 0));
            tokens.Enqueue(new HatHatToken(0, 0));
            tokens.Enqueue(new DataTypeToken("<http://example/type>", 0, 0, 0));

            SparqlExpressionParser parser = new SparqlExpressionParser();
            ISparqlExpression expr = parser.Parse(tokens);

            Assert.IsType(typeof (ConstantTerm), expr);
            ConstantTerm constant = expr as ConstantTerm;
            Assert.NotNull(constant);

            IValuedNode n = constant.Node;
            Assert.IsType(typeof(ILiteralNode), n);
            ILiteralNode lit = (ILiteralNode) n;
            Assert.Equal(String.Empty, lit.Language);
            Assert.True(EqualityHelper.AreUrisEqual(lit.DataType, new Uri("http://example/type")));
        }
        
        [Fact]
        public void SparqlParsingHandlesDollarSignInUriParameter1()
        {
            const string queryString = @"SELECT ?p ?o WHERE { @subject ?p ?o . }";
            const string expectedCondition = @"<http://dbpedia.org/resource/$_(film)> ?p ?o";
            var uri = new Uri("http://dbpedia.org/resource/$_(film)");

            var parametrizedQueryString = new SparqlParameterizedString(queryString);
            parametrizedQueryString.SetUri("subject", uri);
            var sparqlQuery = new SparqlQueryParser().ParseFromString(parametrizedQueryString);
            Console.WriteLine(sparqlQuery.ToString());

            Assert.Contains(expectedCondition, sparqlQuery.ToString());
        }

        [Fact]
        public void SparqlParsingHandlesDollarSignInUriParameter2()
        {
            const string queryString = @"SELECT ?p ?o WHERE { $subject ?p ?o . }";
            const string expectedCondition = @"<http://dbpedia.org/resource/$_(film)> ?p ?o";
            var uri = new Uri("http://dbpedia.org/resource/$_(film)");

            var parametrizedQueryString = new SparqlParameterizedString(queryString);
            parametrizedQueryString.SetVariable("subject", new UriNode(null, uri));
            var sparqlQuery = new SparqlQueryParser().ParseFromString(parametrizedQueryString);
            Console.WriteLine(sparqlQuery.ToString());

            Assert.Contains(expectedCondition, sparqlQuery.ToString());
        }

        [Fact]
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
                Assert.True(false, "Did not error as expected");
            }
            catch (RdfParseException parseEx)
            {
                Console.WriteLine(parseEx.Message);
                Assert.False(parseEx.Message.Contains("?s"));
                Assert.False(parseEx.Message.Contains("rdf:type"));
                Assert.False(parseEx.Message.Contains("?type"));
                Assert.True(parseEx.Message.Contains("?p"));
                Assert.True(parseEx.Message.Contains("?o"));
            }
        }

        [Fact]
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
                Assert.True(false, "Did not error as expected");
            }
            catch (RdfParseException parseEx)
            {
                Console.WriteLine(parseEx.Message);
                Assert.False(parseEx.Message.Contains("?s"));
                Assert.False(parseEx.Message.Contains("rdf:type"));
                Assert.False(parseEx.Message.Contains("?type"));
                Assert.True(parseEx.Message.Contains("?p"));
                Assert.True(parseEx.Message.Contains("?o"));
            }
        }

        [Fact]
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
                Assert.True(false, "Did not error as expected");
            }
            catch (RdfParseException parseEx)
            {
                Console.WriteLine(parseEx.Message);
                Assert.False(parseEx.Message.Contains("?s"));
                Assert.False(parseEx.Message.Contains("rdf:type"));
                Assert.False(parseEx.Message.Contains("?type"));
                Assert.True(parseEx.Message.Contains("?p"));
                Assert.True(parseEx.Message.Contains("?o"));
            }
        }

        [Fact]
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
                Assert.True(false, "Did not error as expected");
            }
            catch (RdfParseException parseEx)
            {
                Console.WriteLine(parseEx.Message);
                Assert.False(parseEx.Message.Contains("?s"));
                Assert.False(parseEx.Message.Contains("rdf:type"));
                Assert.False(parseEx.Message.Contains("?type"));
                Assert.True(parseEx.Message.Contains("?p"));
                Assert.True(parseEx.Message.Contains("?o"));
            }
        }

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
        public void SparqlParsingCore427_1()
        {
            const String query = "SELECT (UUID() AS ?test) { }";

            SparqlQuery q = this._parser.ParseFromString(query);

            String toString = q.ToString();
            Assert.True(toString.Contains("(UUID"));

            String formattedString = new SparqlFormatter().Format(q);
            Assert.True(formattedString.Contains("(UUID"));
        }

        [Fact]
        public void SparqlParsingCore427_2()
        {
            const String query = "SELECT (StrUUID() AS ?test) { }";

            SparqlQuery q = this._parser.ParseFromString(query);

            String toString = q.ToString();
            Assert.True(toString.Contains("(STRUUID"));

            String formattedString = new SparqlFormatter().Format(q);
            Assert.True(formattedString.Contains("(STRUUID"));
        }

        [Fact]
        public void SparqlParsingIllegalWhitespaceInUris()
        {
            const string query = "SELECT * WHERE { <http://example.com/foo bar> a <http://example.com/foo%20type> }";

            Assert.Throws<RdfParseException>(() => this._parser.ParseFromString(query));
        }
        
        [Fact]
        public void SparqlParsingEscapedWhitespaceInUris()
        {
            const string query = "SELECT * WHERE { <http://example.com/foo%20bar> a <http://example.com/foo%20type> }";
            SparqlQuery q = this._parser.ParseFromString(query);
            var pattern = q.RootGraphPattern.TriplePatterns[0] as IMatchTriplePattern;
            var subjectMatch = pattern.Subject as NodeMatchPattern;
            Assert.Equal(new Uri("http://example.com/foo bar"), ((IUriNode)subjectMatch.Node).Uri);
        }

        [Fact]
        public void SparqlParsingAggregatesCore446_1()
        {
            const String query = @" PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
PREFIX sem: <http://ns.kodak.com/sem/1.0/>
PREFIX afn: <http://jena.hpl.hp.com/ARQ/function#>

SELECT ( afn:sqrt( sum( (?fVal - ?ave) * (?fVal - ?ave) ) /  (COUNT(?fv) - 1) ) as ?stddev )  (avg(?fVal) as ?a)
FROM <urn:guid:mdw>
WHERE 
{
  ?pic sem:MyPredicate ?fv .
  BIND (xsd:float(?fv ) AS ?fVal)
  {
    SELECT (AVG(?fVal2) AS ?ave) (COUNT(?fVal2) as ?cnt) 
    WHERE
    {
      ?pic sem:MyPredicate ?fvi .
      BIND (xsd:float(?fvi ) AS ?fVal2)
    }
  }
}";

            this._parser.ParseFromString(query);
        }

        [Fact]
        public void SparqlParsingAggregatesCore446_2()
        {
            // Valid because only aggregates used in the projection
            const String query = @"SELECT (<http://func>() AS ?test) (COUNT(*) AS ?count) WHERE { ?s ?p ?o }";
            this._parser.ParseFromString(query);
        }

        [Fact]
        public void SparqlParsingAggregatesCore446_3()
        {
            // Invalid because non-aggregate and non-group key used in projection
            const String query = @"SELECT (<http://func>(?s) AS ?test) (COUNT(*) AS ?count) WHERE { ?s ?p ?o }";

            Assert.Throws<RdfParseException>(() => this._parser.ParseFromString(query));
        }

        [Fact]
        public void SparqlParsingAggregatesCore446_4()
        {
            // Valid because only aggregates and group keys used in projection
            const String query = @"SELECT (<http://func>(?s) AS ?test) (COUNT(*) AS ?count) WHERE { ?s ?p ?o } GROUP BY ?s";
            this._parser.ParseFromString(query);
        }

        [Fact]
        public void SparqlParsingAggregatesCore446_5()
        {
            // Invalid because non-aggregate and non-group key used in projection
            const String query = @"SELECT (<http://func>(?count) AS ?test) (COUNT(*) AS ?count) WHERE { ?s ?p ?o }";
            this._parser.ParseFromString(query);
        }
    }
}