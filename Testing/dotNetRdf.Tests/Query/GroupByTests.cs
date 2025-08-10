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
using System.IO;
using System.Linq;
using Xunit;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query;


public class GroupByTests
{
    private object ExecuteQuery(IInMemoryQueryableStore store, string query)
    {
        var parser = new SparqlQueryParser();
        SparqlQuery parsedQuery = parser.ParseFromString(query);
        var processor =new LeviathanQueryProcessor(store);
        return processor.ProcessQuery(parsedQuery);
    }

    [Fact]
    public void SparqlGroupByInSubQuery()
    {
        const string query = "SELECT ?s WHERE {{SELECT ?s WHERE {?s ?p ?o} GROUP BY ?s}} GROUP BY ?s";
        var parser = new SparqlQueryParser();
        SparqlQuery _ = parser.ParseFromString(query);
    }

    [Fact]
    public void SparqlGroupByAssignmentSimple()
    {
        const string query = "SELECT ?x WHERE { ?s ?p ?o } GROUP BY ?s AS ?x";
        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);

        var g = new QueryableGraph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

        var results = g.ExecuteQuery(q);
        if (results is SparqlResultSet rset)
        {
            Assert.True(rset.All(r => r.HasValue("x") && !r.HasValue("s")), "All Results should have a ?x variable and no ?s variable");
        }
        else
        {
            Assert.Fail("Didn't get a Result Set as expected");
        }
    }

    [Fact]
    public void SparqlGroupByAssignmentSimple2()
    {
        const string query = "SELECT ?x (COUNT(?p) AS ?predicates) WHERE { ?s ?p ?o } GROUP BY ?s AS ?x";
        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);

        var g = new QueryableGraph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

        var results = g.ExecuteQuery(q);
        if (results is SparqlResultSet resultSet)
        {
            Assert.True(resultSet.All(r => r.HasValue("x") && !r.HasValue("s") && r.HasValue("predicates")), "All Results should have a ?x and ?predicates variables and no ?s variable");
        }
        else
        {
            Assert.Fail("Didn't get a Result Set as expected");
        }
    }

    [Fact]
    public void SparqlGroupByAssignmentExpression()
    {
        const string query = "SELECT ?s ?sum WHERE { ?s ?p ?o } GROUP BY ?s (1 + 2 AS ?sum)";
        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);

        var g = new QueryableGraph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

        var results = g.ExecuteQuery(q);
        if (results is SparqlResultSet resultSet)
        {
            Assert.True(resultSet.All(r => r.HasValue("s") && r.HasValue("sum")), "All Results should have a ?s and a ?sum variable");
        }
        else
        {
            Assert.Fail("Didn't get a Result Set as expected");
        }
    }

    [Fact]
    public void SparqlGroupByAssignmentExpression2()
    {
        const string query = "SELECT ?lang (SAMPLE(?o) AS ?example) WHERE { ?s ?p ?o . FILTER(ISLITERAL(?o)) } GROUP BY (LANG(?o) AS ?lang)";
        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);

        var g = new QueryableGraph();
        FileLoader.Load(g, Path.Combine("resources", "rdfserver", "southampton.rdf"));
        var results = g.ExecuteQuery(q);
        if (results is SparqlResultSet resultSet)
        {
            Assert.True(resultSet.All(r => r.HasValue("lang") && r.HasValue("example")), "All Results should have a ?lang and a ?example variable");
        }
        else
        {
            Assert.Fail("Didn't get a Result Set as expected");
        }
    }

    [Fact]
    public void SparqlGroupByAssignmentExpression3()
    {
        const string query = "SELECT ?lang (SAMPLE(?o) AS ?example) WHERE { ?s ?p ?o . FILTER(ISLITERAL(?o)) } GROUP BY (LANG(?o) AS ?lang) HAVING LANGMATCHES(?lang, \"*\")";
        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);

        var g = new QueryableGraph();
        FileLoader.Load(g, Path.Combine("resources", "rdfserver", "southampton.rdf"));
        var results = g.ExecuteQuery(q);
        if (results is SparqlResultSet resultSet)
        {
            Assert.True(resultSet.All(r => r.HasValue("lang") && r.HasValue("example")), "All Results should have a ?lang and a ?example variable");
        }
        else
        {
            Assert.Fail("Didn't get a Result Set as expected");
        }
    }

    [Fact]
    public void SparqlGroupBySample()
    {
        const string query = "SELECT ?s (SAMPLE(?o) AS ?object) WHERE {?s ?p ?o} GROUP BY ?s";
        var parser = new SparqlQueryParser();
        SparqlQuery _ = parser.ParseFromString(query);
    }

    [Fact]
    public void SparqlAggEmptyGroupMax2()
    {
        const string query = @"PREFIX ex: <http://example.com/>
SELECT (MAX(?value) AS ?max)
WHERE {
	?x ex:p ?value
}";

        SparqlQuery q = new SparqlQueryParser().ParseFromString(query);
        var processor = new LeviathanQueryProcessor(new TripleStore());

        var results = processor.ProcessQuery(q) as SparqlResultSet;
        if (results == null) Assert.Fail("Null results");

        Assert.False(results.IsEmpty, "Results should not be empty");
        Assert.Equal(1, results.Count);
        Assert.True(results.First().All(kvp => kvp.Value == null), "Should be no bound values");
    }

    [Fact]
    public void SparqlAggEmptyGroupMax1()
    {
        const string query = @"PREFIX ex: <http://example.com/>
SELECT (MAX(?value) AS ?max)
WHERE {
	?x ex:p ?value
} GROUP BY ?x";

        SparqlQuery q = new SparqlQueryParser().ParseFromString(query);
        var processor = new LeviathanQueryProcessor(new TripleStore());
        var results = processor.ProcessQuery(q) as SparqlResultSet;
        Assert.NotNull(results);
        Assert.True(results.IsEmpty, "Results should be empty");
    }

    [Fact]
    public void SparqlAggEmptyGroupCount1()
    {
        // counting no results with grouping returns no results
        var query = @"PREFIX : <http://example/>

SELECT (count(*) AS ?C)
WHERE {
   ?s :p ?x
}
GROUP BY ?s";
        SparqlQuery q = new SparqlQueryParser().ParseFromString(query);
        var processor = new LeviathanQueryProcessor(new TripleStore());
        var results = processor.ProcessQuery(q) as SparqlResultSet;
        Assert.NotNull(results);
        Assert.True(results.IsEmpty);
    }

    [Fact]
    public void SparqlAggEmptyGroupCount2()
    {
        // counting no results without grouping always returns a single results
        var query = @"PREFIX : <http://example/>

SELECT (count(*) AS ?C)
WHERE {
   ?s :p ?x
}";
        SparqlQuery q = new SparqlQueryParser().ParseFromString(query);
        var processor = new LeviathanQueryProcessor(new TripleStore());
        var results = processor.ProcessQuery(q) as SparqlResultSet;
        Assert.NotNull(results);
        Assert.False(results.IsEmpty);
    }

    [Fact]
    public void SparqlGroupByRefactor1()
    {
        const string query = @"BASE <http://www.w3.org/2001/sw/DataAccess/tests/data-r2/dataset/manifest#>
PREFIX : <http://example/>

SELECT * FROM <http://data-g3-dup.ttl>
FROM NAMED <http://data-g3.ttl>
WHERE 
{ 
  ?s ?p ?o . 
  GRAPH ?g { ?s ?q ?v . } 
}";

        const string data = @"_:x <http://example/p> ""1""^^<http://www.w3.org/2001/XMLSchema#integer> <http://data-g3-dup.ttl> .
_:a <http://example/p> ""9""^^<http://www.w3.org/2001/XMLSchema#integer> <http://data-g3-dup.ttl> .
_:x <http://example/p> ""1""^^<http://www.w3.org/2001/XMLSchema#integer> <http://data-g3.ttl> .
_:a <http://example/p> ""9""^^<http://www.w3.org/2001/XMLSchema#integer> <http://data-g3.ttl> .";

        var store = new TripleStore();
        StringParser.ParseDataset(store, data, new NQuadsParser());

        var results = ExecuteQuery(store, query) as SparqlResultSet;
        Assert.NotNull(results);
        Assert.Equal(6, results.Variables.Count());
    }

    [Fact]
    public void SparqlGroupByRefactor2()
    {
        const string query = @"BASE <http://www.w3.org/2001/sw/DataAccess/tests/data-r2/algebra/manifest#>
PREFIX : <http://example/>

SELECT * FROM <http://two-nested-opt.ttl>
WHERE 
{ 
  :x1 :p ?v . 
  OPTIONAL { 
    :x3 :q ?w . 
    OPTIONAL { :x2 :p ?v . } 
  }
}";

        const string data = @"<http://example/x1> <http://example/p> ""1""^^<http://www.w3.org/2001/XMLSchema#integer> <http://two-nested-opt.ttl> .
<http://example/x2> <http://example/p> ""2""^^<http://www.w3.org/2001/XMLSchema#integer> <http://two-nested-opt.ttl> .
<http://example/x3> <http://example/q> ""3""^^<http://www.w3.org/2001/XMLSchema#integer> <http://two-nested-opt.ttl> .
<http://example/x3> <http://example/q> ""4""^^<http://www.w3.org/2001/XMLSchema#integer> <http://two-nested-opt.ttl> .";

        var store = new TripleStore();
        StringParser.ParseDataset(store, data, new NQuadsParser());

        var results = ExecuteQuery(store, query) as SparqlResultSet;
        Assert.NotNull(results);
        Assert.Equal(1, results.Count);
        Assert.Equal(2, results.Variables.Count());
    }

    [Fact]
    public void SparqlGroupByRefactor3()
    {
        const string query = @"BASE <http://www.w3.org/2001/sw/DataAccess/tests/data-r2/algebra/manifest#>
PREFIX : <http://example/>

SELECT ?x ?y ?z FROM <http://join-combo-graph-2.ttl>
FROM NAMED <http://join-combo-graph-1.ttl>
WHERE
{ 
  GRAPH ?g { ?x ?p 1  . } 
  { ?x :p ?y . } UNION { ?p a ?z . } 
}";

        const string data = @"<http://example/x1> <http://example/p> ""1""^^<http://www.w3.org/2001/XMLSchema#integer> <http://join-combo-graph-2.ttl> .
<http://example/x1> <http://example/r> ""4""^^<http://www.w3.org/2001/XMLSchema#integer> <http://join-combo-graph-2.ttl> .
<http://example/x2> <http://example/p> ""2""^^<http://www.w3.org/2001/XMLSchema#integer> <http://join-combo-graph-2.ttl> .
<http://example/x2> <http://example/r> ""10""^^<http://www.w3.org/2001/XMLSchema#integer> <http://join-combo-graph-2.ttl> .
<http://example/x2> <http://example/x> ""1""^^<http://www.w3.org/2001/XMLSchema#integer> <http://join-combo-graph-2.ttl> .
<http://example/x3> <http://example/q> ""3""^^<http://www.w3.org/2001/XMLSchema#integer> <http://join-combo-graph-2.ttl> .
<http://example/x3> <http://example/q> ""4""^^<http://www.w3.org/2001/XMLSchema#integer> <http://join-combo-graph-2.ttl> .
<http://example/x3> <http://example/s> ""1""^^<http://www.w3.org/2001/XMLSchema#integer> <http://join-combo-graph-2.ttl> .
<http://example/x3> <http://example/t> <http://example/s> <http://join-combo-graph-2.ttl> .
<http://example/p> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/1999/02/22-rdf-syntax-ns#Property> <http://join-combo-graph-2.ttl> .
<http://example/x1> <http://example/z> <http://example/p> <http://join-combo-graph-2.ttl> .
<http://example/b> <http://example/p> ""1""^^<http://www.w3.org/2001/XMLSchema#integer> <http://join-combo-graph-1.ttl> .
_:a <http://example/p> ""9""^^<http://www.w3.org/2001/XMLSchema#integer> <http://join-combo-graph-1.ttl> .";

        var store = new TripleStore();
        StringParser.ParseDataset(store, data, new NQuadsParser());

        var results = ExecuteQuery(store, query) as SparqlResultSet;
        Assert.NotNull(results);
        Assert.Equal(1, results.Count);
        Assert.Equal(3, results.Variables.Count());
    }

    [Fact]
    public void SparqlGroupByRefactor4()
    {
        const string query = @"PREFIX : <http://example/>

SELECT ?s ?w
FROM <http://group-data-2.ttl>
WHERE
{
  ?s :p ?v .
  OPTIONAL { ?s :q ?w . }
}
GROUP BY ?s ?w";

        const string data = @"<http://example/s1> <http://example/p> ""1""^^<http://www.w3.org/2001/XMLSchema#integer> <http://group-data-2.ttl> .
<http://example/s3> <http://example/p> ""1""^^<http://www.w3.org/2001/XMLSchema#integer> <http://group-data-2.ttl> .
<http://example/s1> <http://example/q> ""9""^^<http://www.w3.org/2001/XMLSchema#integer> <http://group-data-2.ttl> .
<http://example/s2> <http://example/p> ""2""^^<http://www.w3.org/2001/XMLSchema#integer> <http://group-data-2.ttl> .";

        var store = new TripleStore();
        StringParser.ParseDataset(store, data, new NQuadsParser());

        var results = ExecuteQuery(store, query) as SparqlResultSet;
        Assert.NotNull(results);
        Assert.Equal(3, results.Count);
        Assert.Equal(2, results.Variables.Count());
        Assert.True(results.All(r => r.Count > 0), "One or more rows were empty");
        Assert.True(results.All(r => r.HasBoundValue("s")), "One or more rows were empty or failed to have a value for ?s");
    }

    [Fact]
    public void SparqlGroupByRefactor5()
    {
        const string query = "SELECT ?s WHERE { ?s ?p ?o } GROUP BY ?s";

        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);

        var queryStr = q.ToString();
        Assert.Contains("GROUP BY ?s", queryStr);

        var queryStrFmt = new SparqlFormatter().Format(q);
        Assert.Contains("GROUP BY ?s", queryStrFmt);
    }

    [Fact]
    public void SparqlGroupByRefactor6()
    {
        const string query = "SELECT ?s ?p WHERE { ?s ?p ?o } GROUP BY ?s ?p";

        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);

        var queryStr = q.ToString();
        Assert.Contains("GROUP BY ?s ?p", queryStr);

        var queryStrFmt = new SparqlFormatter().Format(q);
        Assert.Contains("GROUP BY ?s ?p", queryStrFmt);
    }

    [Fact]
    public void SparqlGroupByRefactor7()
    {
        const string query = "SELECT ?s WHERE { ?s ?p ?o } GROUP BY (?s)";

        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);

        var queryStr = q.ToString();
        Assert.Contains("GROUP BY ?s", queryStr);

        var queryStrFmt = new SparqlFormatter().Format(q);
        Assert.Contains("GROUP BY ?s", queryStrFmt);
    }

    [Fact]
    public void SparqlGroupByRefactor8()
    {
        const string query = @"PREFIX : <http://example.org/>
PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>

SELECT ?o ?x (COALESCE(?o / ?x, -2 ) AS ?div)
FROM <http://data-coalesce.ttl>
WHERE
{
  ?s :p ?o .
  OPTIONAL { ?s :q ?x . }
}";

        const string data = @"<http://example.org/n0> <http://example.org/p> ""1""^^<http://www.w3.org/2001/XMLSchema#integer> <http://data-coalesce.ttl> .
<http://example.org/n1> <http://example.org/p> ""0""^^<http://www.w3.org/2001/XMLSchema#integer> <http://data-coalesce.ttl> .
<http://example.org/n1> <http://example.org/q> ""0""^^<http://www.w3.org/2001/XMLSchema#integer> <http://data-coalesce.ttl> .
<http://example.org/n2> <http://example.org/p> ""0""^^<http://www.w3.org/2001/XMLSchema#integer> <http://data-coalesce.ttl> .
<http://example.org/n2> <http://example.org/q> ""2""^^<http://www.w3.org/2001/XMLSchema#integer> <http://data-coalesce.ttl> .
<http://example.org/n3> <http://example.org/p> ""4""^^<http://www.w3.org/2001/XMLSchema#integer> <http://data-coalesce.ttl> .
<http://example.org/n3> <http://example.org/q> ""2""^^<http://www.w3.org/2001/XMLSchema#integer> <http://data-coalesce.ttl> .";

        var store = new TripleStore();
        StringParser.ParseDataset(store, data, new NQuadsParser());

        var results = ExecuteQuery(store, query) as SparqlResultSet;
        Assert.NotNull(results);
        Assert.Equal(4, results.Count);
        Assert.Equal(3, results.Variables.Count());

        foreach (ISparqlResult r in results)
        {
            if (r.HasBoundValue("x"))
            {
                var xVal = r["x"].AsValuedNode().AsInteger();
                if (xVal == 0)
                {
                    Assert.Equal(-2, r["div"].AsValuedNode().AsInteger());
                }
                else
                {
                    Assert.Equal(r["o"].AsValuedNode().AsInteger() / xVal, r["div"].AsValuedNode().AsInteger());
                }
            }
            else
            {
                Assert.Equal(-2, r["div"].AsValuedNode().AsInteger());
            }
        }            
    }

    [Fact]
    public void SparqlGroupByRefactor9()
    {
        const string query = @"PREFIX : <http://example.org/>
PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>

SELECT (COALESCE(?x, -1 ) AS ?cx) (COALESCE(?o / ?x, -2 ) AS ?div)
(COALESCE(?z, -3 ) AS ?def)
(COALESCE(?z) AS ?err)
FROM <http://data-coalesce.ttl>
WHERE
{
  ?s :p ?o .
  OPTIONAL { ?s :q ?x . }
}";

        const string data = @"<http://example.org/n0> <http://example.org/p> ""1""^^<http://www.w3.org/2001/XMLSchema#integer> <http://data-coalesce.ttl> .
<http://example.org/n1> <http://example.org/p> ""0""^^<http://www.w3.org/2001/XMLSchema#integer> <http://data-coalesce.ttl> .
<http://example.org/n1> <http://example.org/q> ""0""^^<http://www.w3.org/2001/XMLSchema#integer> <http://data-coalesce.ttl> .
<http://example.org/n2> <http://example.org/p> ""0""^^<http://www.w3.org/2001/XMLSchema#integer> <http://data-coalesce.ttl> .
<http://example.org/n2> <http://example.org/q> ""2""^^<http://www.w3.org/2001/XMLSchema#integer> <http://data-coalesce.ttl> .
<http://example.org/n3> <http://example.org/p> ""4""^^<http://www.w3.org/2001/XMLSchema#integer> <http://data-coalesce.ttl> .
<http://example.org/n3> <http://example.org/q> ""2""^^<http://www.w3.org/2001/XMLSchema#integer> <http://data-coalesce.ttl> .";

        var store = new TripleStore();
        StringParser.ParseDataset(store, data, new NQuadsParser());

        var results = ExecuteQuery(store, query) as SparqlResultSet;
        Assert.NotNull(results);
        Assert.Equal(4, results.Count);
        Assert.Equal(4, results.Variables.Count());

        foreach (ISparqlResult r in results)
        {
            var cxVal = r["cx"].AsValuedNode().AsInteger();
            if (cxVal == -1) Assert.Equal(-2, r["div"].AsValuedNode().AsInteger());
        }
    }

    [Fact]
    public void SparqlGroupByComplex1()
    {
        const string data = @"PREFIX : <http://test/> INSERT DATA { :x :p 1 , 2 . :y :p 5 }";
        const string query = @"SELECT ?s (CONCAT('$', STR(SUM(?o))) AS ?Total) WHERE { ?s ?p ?o } GROUP BY ?s";

        var store = new TripleStore();
        store.ExecuteUpdate(data);
        Assert.Equal(1, store.Graphs.Count);
        Assert.Equal(3, store.Triples.Count());

        //Aggregates may occur in project expressions and should evaluate correctly
        var results = ExecuteQuery(store, query) as SparqlResultSet;
        Assert.NotNull(results);
        Assert.True(results.All(r => r.HasBoundValue("Total")));

        ISparqlResult x = results.FirstOrDefault(r => ((IUriNode)r["s"]).Uri.Equals(new Uri("http://test/x")));
        Assert.NotNull(x);
        Assert.Equal("$3", x["Total"].AsValuedNode().AsString());

        ISparqlResult y = results.FirstOrDefault(r => ((IUriNode)r["s"]).Uri.Equals(new Uri("http://test/y")));
        Assert.NotNull(y);
        Assert.Equal("$5", y["Total"].AsValuedNode().AsString());
    }

    [Fact]
    public void SparqlGroupByComplex2()
    {
        //Nested aggregates are a parser error
        const string query = @"SELECT ?s (SUM(MIN(?o)) AS ?Total) WHERE { ?s ?p ?o } GROUP BY ?s";
        var parser = new SparqlQueryParser();

        Assert.Throws<RdfParseException>(() => parser.ParseFromString(query));
    }

    [Fact]
    public void SparqlGroupByWithValues1()
    {
        const string query = @"SELECT ?a WHERE { VALUES ( ?a ) { ( 1 ) ( 2 ) } } GROUP BY ?a";

        var g = new QueryableGraph();
        var results = g.ExecuteQuery(query) as SparqlResultSet;
        Assert.NotNull(results);
        Assert.False(results.IsEmpty);
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void SparqlGroupByWithValues2()
    {
        const string query = @"SELECT ?a ?b WHERE { VALUES ( ?a ?b ) { ( 1 2 ) ( 1 UNDEF ) ( UNDEF 2 ) } } GROUP BY ?a ?b";

        var g = new QueryableGraph();
        var results = g.ExecuteQuery(query) as SparqlResultSet;
        Assert.NotNull(results);
        Assert.False(results.IsEmpty);
        Assert.Equal(3, results.Count);
    }

    [Fact]
    public void SparqlGroupByWithGraph1()
    {
        const string query = @"SELECT ?g WHERE { GRAPH ?g { } } GROUP BY ?g";

        var store = new TripleStore();
        IGraph def = new Graph();
        store.Add(def);
        IGraph named = new Graph(new UriNode(new Uri("http://name")));
        store.Add(named);

        Assert.Equal(2, store.Graphs.Count);

        var results = ExecuteQuery(store, query) as SparqlResultSet;
        Assert.NotNull(results);
        Assert.False(results.IsEmpty);

        //Count only covers named graphs
        Assert.Equal(1, results.Count);
    }

    [Fact]
    public void SparqlGroupByWithEmptyResultSet()
    {
        //var query = "SELECT ?s  (COUNT(?o) AS ?c) WHERE {?s a ?o} GROUP BY ?s";
        var query = "SELECT ?s  (COUNT(?o) AS ?c) WHERE {?s a ?o} GROUP BY ?s";
        var m = new InMemoryManager();
        var result = (SparqlResultSet)m.Query(query);
        Assert.Empty(result.Results);
    }
}
