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
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Update;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query;


public class ParsingTests
{
    private readonly SparqlQueryParser _parser = new SparqlQueryParser();
    private readonly SparqlUpdateParser _updateParser = new SparqlUpdateParser();
    private readonly ITestOutputHelper _output;

    public ParsingTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private SparqlQuery TestQuery(string query)
    {
        var q = _parser.ParseFromString(query);

        _output.WriteLine(q.ToString());
        _output.WriteLine("");
        _output.WriteLine(q.ToAlgebra().ToString());

        return q;
    }

    private SparqlUpdateCommandSet TestUpdate(string update)
    {
        var cmds = _updateParser.ParseFromString(update);

        _output.WriteLine(cmds.ToString());

        return cmds;
    }

    [Fact]
    public void SparqlParsingComplexGraphAfterUnion()
    {
        var query = "SELECT * WHERE {{?x ?y ?z} UNION {?z ?y ?x} GRAPH ?g {?x ?y ?z}}";
        TestQuery(query);
    }

    [Fact]
    public void SparqlParsingComplexFilterAfterUnion()
    {
        var query = "SELECT * WHERE {{?x ?y ?z} UNION {?z ?y ?x} FILTER(true)}";
        TestQuery(query);
    }

    [Fact]
    public void SparqlParsingComplexOptionalAfterUnion()
    {
        var query = "SELECT * WHERE {{?x ?y ?z} UNION {?z ?y ?x} OPTIONAL {?x a ?u}}";
        TestQuery(query);
    }

    [Fact]
    public void SparqlParsingComplexMinusAfterUnion()
    {
        var query = "SELECT * WHERE {{?x ?y ?z} UNION {?z ?y ?x} MINUS {?s ?p ?o}}";
        TestQuery(query);
    }

    [Fact]
    public void SparqlParsingComplexOptionalServiceUnion()
    {
        var query = "SELECT * WHERE {{?x ?y ?z} UNION {?z ?y ?x} SERVICE ?g {?x ?y ?z}}";
        TestQuery(query);
    }

    [Fact]
    public void SparqlParsingSingleSubQuery()
    {
        var query = "SELECT * WHERE {{SELECT * WHERE {?s ?p ?o}}}";
        TestQuery(query);
    }

    [Fact]
    public void SparqlParsingSetExpression()
    {
        var query = "SELECT * WHERE { ?s ?p ?o . FILTER(?o IN (1, 2, 3)) }";
        TestQuery(query);
    }

    [Fact]
    public void SparqlParsingSetExpression2()
    {
        var query = "PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA +
                    "> SELECT * WHERE { ?s ?p ?o . FILTER(xsd:integer(?o) IN (1, 2, 3)) }";
        TestQuery(query);
    }

    [Fact]
    public void SparqlParsingSetExpression3()
    {
        var query = "PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA +
                    "> SELECT * WHERE { ?s ?p ?o . FILTER(xsd:integer(?o) NOT IN (1, 2, 3)) }";
        TestQuery(query);
    }

    [Fact]
    public void SparqlParsingSetExpression4()
    {
        var query = "PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA +
                    "> SELECT * WHERE { ?s ?p ?o . FILTER(1 + 3 IN (1, 2, 3)) }";
        TestQuery(query);
    }

    [Fact]
    public void SparqlParsingNumericExpression1()
    {
        var query = "PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA +
                    "> SELECT * WHERE { ?s ?p ?o . FILTER(1 + '3'^^xsd:long) }";
        TestQuery(query);
    }

    [Fact]
    public void SparqlParsingNumericExpression2()
    {
        var query = "PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA +
                    "> SELECT * WHERE { ?s ?p ?o . FILTER(1 + '3'^^xsd:short) }";
        TestQuery(query);
    }

    [Fact]
    public void SparqlParsingNumericExpression3()
    {
        var query = "PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA +
                    "> SELECT * WHERE { ?s ?p ?o . FILTER(1 + '3'^^xsd:byte) }";
        TestQuery(query);
    }

    [Fact]
    public void SparqlParsingNumericExpression4()
    {
        var query = "PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA +
                    "> SELECT * WHERE { ?s ?p ?o . FILTER(1 + '3'^^xsd:nonPositiveInteger) }";
        TestQuery(query);
    }

    [Fact]
    public void SparqlParsingIfInNumericExpression()
    {
        var query = @"SELECT * WHERE { BIND (1 + IF(1, 2, 3) + 1 AS ?x) }";
        TestQuery(query);
    }

    [Fact]
    public void SparqlParsingPropertyPathsNegatedSetSimple()
    {
        var query = "SELECT * WHERE { ?s !a ?o }";
        TestQuery(query);
    }

    [Fact]
    public void SparqlParsingPropertyPathsNegatedSetSimpleInSequence1()
    {
        var query = "SELECT * WHERE { ?s a / !a ?o }";
        TestQuery(query);
    }

    [Fact]
    public void SparqlParsingPropertyPathsNegatedSetSimpleInSequence2()
    {
        var query = "SELECT * WHERE { ?s !a / a ?o }";
        TestQuery(query);
    }

    [Fact]
    public void SparqlParsingPropertyPathsNegatedSet()
    {
        var query = "PREFIX rdfs: <" + NamespaceMapper.RDFS +
                    "> SELECT * WHERE { ?s !(rdfs:label|rdfs:comment) ?o }";
        TestQuery(query);
    }

    [Fact]
    public void SparqlParsingPropertyPathsNegatedSetModified()
    {
        var query = "PREFIX rdfs: <" + NamespaceMapper.RDFS +
                    "> SELECT * WHERE { ?s !(rdfs:label|rdfs:comment)+ ?o }";
        TestQuery(query);
    }

    [Fact]
    public void SparqlParsingPropertyPathsNegatedSetSimpleInAlternative()
    {
        var query = "PREFIX rdfs: <" + NamespaceMapper.RDFS +
                    "> SELECT * WHERE { ?s (rdfs:label|!rdfs:comment) ?o }";
        TestQuery(query);
    }

    [Fact]
    public void SparqlParsingNoGapPrefixes()
    {
        string query;
        using (var reader = File.OpenText(Path.Combine("resources", "no-gap-prefixes.rq")))
        {
            query = reader.ReadToEnd();
            reader.Close();
        }
        TestQuery(query);
    }

    [Fact]
    public void SparqlParsingPropertyPathsUnbracketedAlternatives()
    {
        var query = @"PREFIX : <http://www.example.org/>

SELECT ?X WHERE
{ 
  [ :p|:q|:r ?X ]
}";
        TestQuery(query);
    }

    [Fact]
    public void SparqlParsingExistsWithinSubQuery1()
    {
        var query = "SELECT * WHERE { { SELECT ?s WHERE { ?s a ?type FILTER NOT EXISTS { ?s a <http://restricted> } } } }";
        TestQuery(query);
    }

    [Fact]
    public void SparqlParsingExistsWithinSubQuery2()
    {
        var query = "SELECT * WHERE { { SELECT ?s WHERE { ?s a ?type FILTER NOT EXISTS { { ?s a <http://restricted> } UNION { ?s a <http://other> } } } } }";
        TestQuery(query);
    }

    [Fact]
    public void SparqlVarNames()
    {
        var names = new List<string>
            {
                "?var",
                "$var",
                "var"
            };

        foreach (var name in names)
        {
            var var = new SparqlVariable(name);
            Assert.Equal("var", var.Name);

            var varPat = new VariablePattern(name);
            Assert.Equal("var", varPat.VariableName);

            var varTerm = new VariableTerm(name);
            Assert.Equal("var", varTerm.Variables.First());
        }
    }

    [Fact]
    public void SparqlParsingInsertDataWithGraphVar()
    {
        var update = "INSERT DATA { GRAPH ?g { } }";

        Assert.Throws<RdfParseException>(() => TestUpdate(update));
    }

    [Fact]
    public void SparqlParsingDeleteDataWithGraphVar()
    {
        var update = "DELETE DATA { GRAPH ?g { } }";

        Assert.Throws<RdfParseException>(() => TestUpdate(update));
    }

    [Fact]
    public void SparqlParsingExistsWithinUpdateWhereClause1()
    {
        var update = @"WITH <http://source>
DELETE { GRAPH <http://source> { ?s ?p ?o } }
INSERT { GRAPH <http://target> { ?s ?p ?o } } 
WHERE { ?s ?p ?o . FILTER NOT EXISTS { ?s a <http://restricted> } }";
        TestUpdate(update);
    }

    [Fact]
    public void SparqlParsingExistsWithinUpdateWhereClause2()
    {
        var update = @"WITH <http://source>
INSERT { GRAPH <http://target> { ?s ?p ?o } } 
WHERE { ?s ?p ?o . FILTER NOT EXISTS { ?s a <http://restricted> } }";
        TestUpdate(update);
    }

    [Fact]
    public void SparqlParsingExistsWithinUpdateWhereClause3()
    {
        var update = @"WITH <http://source>
DELETE { GRAPH <http://source> { ?s ?p ?o } }
WHERE { ?s ?p ?o . FILTER NOT EXISTS { ?s a <http://restricted> } }";
        TestUpdate(update);
    }

    [Fact]
    public void SparqlParsingExistsWithinUpdateWhereClause4()
    {
        var update = @"DELETE { GRAPH <http://source> { ?s ?p ?o } }
WHERE { GRAPH <htp://source> { ?s ?p ?o  } . FILTER NOT EXISTS { ?s a <http://restricted> } }";
        TestUpdate(update);
    }

    [Fact]
    public void SparqlParsingExistsWithinUpdateWhereClause5()
    {
        var update = @"INSERT { GRAPH <http://target> { ?s ?p ?o } }
WHERE { GRAPH <htp://source> { ?s ?p ?o } . FILTER NOT EXISTS { ?s a <http://restricted> } }";
        TestUpdate(update);
    }

    [Fact]
    public void SparqlParsingExistsWithinUpdateWhereClause6()
    {
        var update = @"PREFIX myschema: <http://www.example.com/schema#>
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
        var update = @"DELETE { GRAPH <http://source> { ?s ?p ?o } }
WHERE { GRAPH <htp://source> { ?s ?p ?o  } . FILTER (NOT EXISTS { ?s a <http://restricted> }) }";
        TestUpdate(update);
    }

    [Fact]
    public void SparqlParsingExistsWithinUpdateWhereClause8()
    {
        var update = @"INSERT { GRAPH <http://target> { ?s ?p ?o } }
WHERE { GRAPH <htp://source> { ?s ?p ?o } . FILTER (NOT EXISTS { ?s a <http://restricted> }) }";
        TestUpdate(update);
    }

    [Fact]
    public void SparqlParsingLiteralsInExpressions()
    {
        var tokens = new Queue<IToken>();
        tokens.Enqueue(new LiteralToken("value", 0, 0, 0));
        tokens.Enqueue(new HatHatToken(0, 0));
        tokens.Enqueue(new DataTypeToken("<http://example/type>", 0, 0, 0));

        var parser = new SparqlExpressionParser(UriFactory.Root);
        var expr = parser.Parse(tokens);

        Assert.IsType<ConstantTerm>(expr);
        var constant = expr as ConstantTerm;
        Assert.NotNull(constant);

        var n = constant.Node;
        Assert.IsType<ILiteralNode>(n, exactMatch: false);
        var lit = (ILiteralNode) n;
        Assert.Equal(string.Empty, lit.Language);
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
        _output.WriteLine(sparqlQuery.ToString());

        Assert.Contains(expectedCondition, sparqlQuery.ToString());
    }

    [Fact]
    public void SparqlParsingHandlesDollarSignInUriParameter2()
    {
        const string queryString = @"SELECT ?p ?o WHERE { $subject ?p ?o . }";
        const string expectedCondition = @"<http://dbpedia.org/resource/$_(film)> ?p ?o";
        var uri = new Uri("http://dbpedia.org/resource/$_(film)");

        var parametrizedQueryString = new SparqlParameterizedString(queryString);
        parametrizedQueryString.SetVariable("subject", new UriNode(uri));
        var sparqlQuery = new SparqlQueryParser().ParseFromString(parametrizedQueryString);
        _output.WriteLine(sparqlQuery.ToString());

        Assert.Contains(expectedCondition, sparqlQuery.ToString());
    }

    [Fact]
    public void SparqlParsingExcessTokens1()
    {
        const string query = @"PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
SELECT * WHERE
{
  ?s rdf:type ?type ?p ?o
}
";
        try
        {
            _parser.ParseFromString(query);
            Assert.Fail("Did not error as expected");
        }
        catch (RdfParseException parseEx)
        {
            _output.WriteLine(parseEx.Message);
            Assert.DoesNotContain("?s", parseEx.Message);
            Assert.DoesNotContain("rdf:type", parseEx.Message);
            Assert.DoesNotContain("?type", parseEx.Message);
            Assert.Contains("?p", parseEx.Message);
            Assert.Contains("?o", parseEx.Message);
        }
    }

    [Fact]
    public void SparqlParsingExcessTokens2()
    {
        const string query = @"PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
SELECT * WHERE
{
  ?s rdf:type ?type ?p ?o .
}
";
        try
        {
            _parser.ParseFromString(query);
            Assert.Fail("Did not error as expected");
        }
        catch (RdfParseException parseEx)
        {
            _output.WriteLine(parseEx.Message);
            Assert.DoesNotContain("?s", parseEx.Message);
            Assert.DoesNotContain("rdf:type", parseEx.Message);
            Assert.DoesNotContain("?type", parseEx.Message);
            Assert.Contains("?p", parseEx.Message);
            Assert.Contains("?o", parseEx.Message);
        }
    }

    [Fact]
    public void SparqlParsingExcessTokens3()
    {
        const string query = @"PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
SELECT * WHERE
{
  ?s rdf:type ?type ?p ?o ;
}
";
        try
        {
            _parser.ParseFromString(query);
            Assert.Fail("Did not error as expected");
        }
        catch (RdfParseException parseEx)
        {
            _output.WriteLine(parseEx.Message);
            Assert.DoesNotContain("?s", parseEx.Message);
            Assert.DoesNotContain("rdf:type", parseEx.Message);
            Assert.DoesNotContain("?type", parseEx.Message);
            Assert.Contains("?p", parseEx.Message);
            Assert.Contains("?o", parseEx.Message);
        }
    }

    [Fact]
    public void SparqlParsingExcessTokens4()
    {
        const string query = @"PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
SELECT * WHERE
{
  ?s rdf:type ?type ?p ?o ,
}
";
        try
        {
            _parser.ParseFromString(query);
            Assert.Fail("Did not error as expected");
        }
        catch (RdfParseException parseEx)
        {
            _output.WriteLine(parseEx.Message);
            Assert.DoesNotContain("?s", parseEx.Message);
            Assert.DoesNotContain("rdf:type", parseEx.Message);
            Assert.DoesNotContain("?type", parseEx.Message);
            Assert.Contains("?p", parseEx.Message);
            Assert.Contains("?o", parseEx.Message);
        }
    }

    [Fact]
    public void SparqlParsingComplexCore428_1()
    {
        // Distilled from CORE-428 report
        const string query = @"SELECT *
WHERE
{
  OPTIONAL 
  {
    { SELECT * WHERE { } }
    BIND('test' AS ?test)
  }
}";

        // Should be a valid query
        _parser.ParseFromString(query);
    }

    [Fact]
    public void SparqlParsingComplexCore428_2()
    {
        // Distilled from CORE-428 report
        const string query = @"SELECT *
WHERE
{
  OPTIONAL 
  {
    OPTIONAL { }
    BIND('test' AS ?test)
  }
}";

        // Should be a valid query
        _parser.ParseFromString(query);
    }

    [Fact]
    public void SparqlParsingComplexCore428_3()
    {
        // Distilled from CORE-428 report
        const string query = @"SELECT *
WHERE
{
  OPTIONAL 
  {
    { } UNION { }
    BIND('test' AS ?test)
  }
}";

        // Should be a valid query
        _parser.ParseFromString(query);
    }

    [Fact]
    public void SparqlParsingComplexCore428_4()
    {
        // Distilled from CORE-428 report
        const string query = @"SELECT *
WHERE
{
  OPTIONAL 
  {
    MINUS { }
    BIND('test' AS ?test)
  }
}";

        // Should be a valid query
        _parser.ParseFromString(query);
    }

    [Fact]
    public void SparqlParsingComplexCore428_5()
    {
        // Distilled from CORE-428 report
        const string query = @"SELECT *
WHERE
{
  OPTIONAL 
  {
    { }
    BIND('test' AS ?test)
  }
}";

        // Should be a valid query
        _parser.ParseFromString(query);
    }

    [Fact]
    public void SparqlParsingCore427_1()
    {
        const string query = "SELECT (UUID() AS ?test) { }";

        var q = _parser.ParseFromString(query);

        var toString = q.ToString();
        Assert.Contains("(UUID", toString);

        var formattedString = new SparqlFormatter().Format(q);
        Assert.Contains("(UUID", formattedString);
    }

    [Fact]
    public void SparqlParsingCore427_2()
    {
        const string query = "SELECT (StrUUID() AS ?test) { }";

        var q = _parser.ParseFromString(query);

        var toString = q.ToString();
        Assert.Contains("(STRUUID", toString);

        var formattedString = new SparqlFormatter().Format(q);
        Assert.Contains("(STRUUID", formattedString);
    }

    [Fact]
    public void SparqlParsingIllegalWhitespaceInUris()
    {
        const string query = "SELECT * WHERE { <http://example.com/foo bar> a <http://example.com/foo%20type> }";

        Assert.Throws<RdfParseException>(() => _parser.ParseFromString(query));
    }
    
    [Fact]
    public void SparqlParsingEscapedWhitespaceInUris()
    {
        const string query = "SELECT * WHERE { <http://example.com/foo%20bar> a <http://example.com/foo%20type> }";
        var q = _parser.ParseFromString(query);
        var pattern = q.RootGraphPattern.TriplePatterns[0] as IMatchTriplePattern;
        Assert.NotNull(pattern);
        var subjectMatch = pattern.Subject as NodeMatchPattern;
        Assert.Equal(new Uri("http://example.com/foo bar"), ((IUriNode)subjectMatch.Node).Uri);
    }

    [Fact]
    public void SparqlParsingAggregatesCore446_1()
    {
        const string query = @" PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
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

        _parser.ParseFromString(query);
    }

    [Fact]
    public void SparqlParsingAggregatesCore446_2()
    {
        // Valid because only aggregates used in the projection
        const string query = @"SELECT (<http://func>() AS ?test) (COUNT(*) AS ?count) WHERE { ?s ?p ?o }";
        _parser.ParseFromString(query);
    }

    [Fact]
    public void SparqlParsingAggregatesCore446_3()
    {
        // Invalid because non-aggregate and non-group key used in projection
        const string query = @"SELECT (<http://func>(?s) AS ?test) (COUNT(*) AS ?count) WHERE { ?s ?p ?o }";

        Assert.Throws<RdfParseException>(() => _parser.ParseFromString(query));
    }

    [Fact]
    public void SparqlParsingAggregatesCore446_4()
    {
        // Valid because only aggregates and group keys used in projection
        const string query = @"SELECT (<http://func>(?s) AS ?test) (COUNT(*) AS ?count) WHERE { ?s ?p ?o } GROUP BY ?s";
        _parser.ParseFromString(query);
    }

    [Fact]
    public void SparqlParsingAggregatesCore446_5()
    {
        // Invalid because non-aggregate and non-group key used in projection
        const string query = @"SELECT (<http://func>(?count) AS ?test) (COUNT(*) AS ?count) WHERE { ?s ?p ?o }";
        _parser.ParseFromString(query);
    }

    [Fact]
    public void ParsingUnbracketedMultiplicationExpression()
    {
        TestQuery("SELECT ?f WHERE { BIND (1*2*3 as ?f)}");
    }

    [Fact]
    public void ParsingUnbracketedDivisionExpression()
    {
        TestQuery("SELECT ?f WHERE { BIND (10/5/2 as ?f)}");
    }

    [Fact]
    public void ParsingUnbracketedAdditionWithDivisionTerms()
    {
        TestQuery("SELECT ?f WHERE {BIND(6+10 / 2 as ?f)}");
    }
}