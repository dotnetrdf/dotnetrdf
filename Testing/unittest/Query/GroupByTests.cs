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
using NUnit.Framework;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query
{
    [TestFixture]
    public class GroupByTests
    {
        [Test]
        public void SparqlGroupByInSubQuery()
        {
            String query = "SELECT ?s WHERE {{SELECT ?s WHERE {?s ?p ?o} GROUP BY ?s}} GROUP BY ?s";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);
        }

        [Test]
        public void SparqlGroupByAssignmentSimple()
        {
            String query = "SELECT ?x WHERE { ?s ?p ?o } GROUP BY ?s AS ?x";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            SparqlFormatter formatter = new SparqlFormatter();
            Console.WriteLine(formatter.Format(q));
            Console.WriteLine();

            QueryableGraph g = new QueryableGraph();
            FileLoader.Load(g, "resources\\InferenceTest.ttl");

            Object results = g.ExecuteQuery(q);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                TestTools.ShowResults(rset);

                Assert.IsTrue(rset.All(r => r.HasValue("x") && !r.HasValue("s")), "All Results should have a ?x variable and no ?s variable");
            }
            else
            {
                Assert.Fail("Didn't get a Result Set as expected");
            }
        }

        [Test]
        public void SparqlGroupByAssignmentSimple2()
        {
            String query = "SELECT ?x (COUNT(?p) AS ?predicates) WHERE { ?s ?p ?o } GROUP BY ?s AS ?x";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            SparqlFormatter formatter = new SparqlFormatter();
            Console.WriteLine(formatter.Format(q));
            Console.WriteLine();

            QueryableGraph g = new QueryableGraph();
            FileLoader.Load(g, "resources\\InferenceTest.ttl");

            Object results = g.ExecuteQuery(q);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                TestTools.ShowResults(rset);

                Assert.IsTrue(rset.All(r => r.HasValue("x") && !r.HasValue("s") && r.HasValue("predicates")), "All Results should have a ?x and ?predicates variables and no ?s variable");
            }
            else
            {
                Assert.Fail("Didn't get a Result Set as expected");
            }
        }

        [Test]
        public void SparqlGroupByAssignmentExpression()
        {
            String query = "SELECT ?s ?sum WHERE { ?s ?p ?o } GROUP BY ?s (1 + 2 AS ?sum)";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            SparqlFormatter formatter = new SparqlFormatter();
            Console.WriteLine(formatter.Format(q));
            Console.WriteLine();

            QueryableGraph g = new QueryableGraph();
            FileLoader.Load(g, "resources\\InferenceTest.ttl");

            Object results = g.ExecuteQuery(q);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                TestTools.ShowResults(rset);

                Assert.IsTrue(rset.All(r => r.HasValue("s") && r.HasValue("sum")), "All Results should have a ?s and a ?sum variable");
            }
            else
            {
                Assert.Fail("Didn't get a Result Set as expected");
            }
        }

        [Test]
        public void SparqlGroupByAssignmentExpression2()
        {
            String query = "SELECT ?lang (SAMPLE(?o) AS ?example) WHERE { ?s ?p ?o . FILTER(ISLITERAL(?o)) } GROUP BY (LANG(?o) AS ?lang)";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            SparqlFormatter formatter = new SparqlFormatter();
            Console.WriteLine(formatter.Format(q));
            Console.WriteLine();

            QueryableGraph g = new QueryableGraph();
            UriLoader.Load(g, new Uri("http://dbpedia.org/resource/Southampton"));

            Object results = g.ExecuteQuery(q);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                TestTools.ShowResults(rset);

                Assert.IsTrue(rset.All(r => r.HasValue("lang") && r.HasValue("example")), "All Results should have a ?lang and a ?example variable");
            }
            else
            {
                Assert.Fail("Didn't get a Result Set as expected");
            }
        }

        [Test]
        public void SparqlGroupByAssignmentExpression3()
        {
            String query = "SELECT ?lang (SAMPLE(?o) AS ?example) WHERE { ?s ?p ?o . FILTER(ISLITERAL(?o)) } GROUP BY (LANG(?o) AS ?lang) HAVING LANGMATCHES(?lang, \"*\")";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            SparqlFormatter formatter = new SparqlFormatter();
            Console.WriteLine(formatter.Format(q));
            Console.WriteLine();

            QueryableGraph g = new QueryableGraph();
            UriLoader.Load(g, new Uri("http://dbpedia.org/resource/Southampton"));

            Object results = g.ExecuteQuery(q);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                TestTools.ShowResults(rset);

                Assert.IsTrue(rset.All(r => r.HasValue("lang") && r.HasValue("example")), "All Results should have a ?lang and a ?example variable");
            }
            else
            {
                Assert.Fail("Didn't get a Result Set as expected");
            }
        }

        [Test]
        public void SparqlGroupBySample()
        {
            String query = "SELECT ?s (SAMPLE(?o) AS ?object) WHERE {?s ?p ?o} GROUP BY ?s";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);
        }

        [Test]
        public void SparqlGroupByAggregateEmptyGroup1()
        {
            String query = @"PREFIX ex: <http://example.com/>
SELECT (MAX(?value) AS ?max)
WHERE {
	?x ex:p ?value
}";

            SparqlQuery q = new SparqlQueryParser().ParseFromString(query);
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(new TripleStore());
            Console.WriteLine(q.ToAlgebra().ToString());

            SparqlResultSet results = processor.ProcessQuery(q) as SparqlResultSet;
            if (results == null) Assert.Fail("Null results");

            Assert.IsFalse(results.IsEmpty, "Results should not be empty");
            Assert.AreEqual(1, results.Count, "Should be a single result");
            Assert.IsTrue(results.First().All(kvp => kvp.Value == null), "Should be no bound values");
        }

        [Test]
        public void SparqlGroupByAggregateEmptyGroup2()
        {
            String query = @"PREFIX ex: <http://example.com/>
SELECT (MAX(?value) AS ?max)
WHERE {
	?x ex:p ?value
} GROUP BY ?x";

            SparqlQuery q = new SparqlQueryParser().ParseFromString(query);
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(new TripleStore());
            Console.WriteLine(q.ToAlgebra().ToString());

            SparqlResultSet results = processor.ProcessQuery(q) as SparqlResultSet;
            if (results == null) Assert.Fail("Null results");

            Assert.IsFalse(results.IsEmpty, "Results should not be empty");
        }

        [Test]
        public void SparqlGroupByRefactor1()
        {
            String query = @"BASE <http://www.w3.org/2001/sw/DataAccess/tests/data-r2/dataset/manifest#>
PREFIX : <http://example/>

SELECT * FROM <http://data-g3-dup.ttl>
FROM NAMED <http://data-g3.ttl>
WHERE 
{ 
  ?s ?p ?o . 
  GRAPH ?g { ?s ?q ?v . } 
}";

            String data = @"_:x <http://example/p> ""1""^^<http://www.w3.org/2001/XMLSchema#integer> <http://data-g3-dup.ttl> .
_:a <http://example/p> ""9""^^<http://www.w3.org/2001/XMLSchema#integer> <http://data-g3-dup.ttl> .
_:x <http://example/p> ""1""^^<http://www.w3.org/2001/XMLSchema#integer> <http://data-g3.ttl> .
_:a <http://example/p> ""9""^^<http://www.w3.org/2001/XMLSchema#integer> <http://data-g3.ttl> .";

            TripleStore store = new TripleStore();
            StringParser.ParseDataset(store, data, new NQuadsParser());

            SparqlResultSet results = store.ExecuteQuery(query) as SparqlResultSet;
            Assert.IsNotNull(results);
            TestTools.ShowResults(results);
            Assert.AreEqual(6, results.Variables.Count());
        }

        [Test]
        public void SparqlGroupByRefactor2()
        {
            String query = @"BASE <http://www.w3.org/2001/sw/DataAccess/tests/data-r2/algebra/manifest#>
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

            String data = @"<http://example/x1> <http://example/p> ""1""^^<http://www.w3.org/2001/XMLSchema#integer> <http://two-nested-opt.ttl> .
<http://example/x2> <http://example/p> ""2""^^<http://www.w3.org/2001/XMLSchema#integer> <http://two-nested-opt.ttl> .
<http://example/x3> <http://example/q> ""3""^^<http://www.w3.org/2001/XMLSchema#integer> <http://two-nested-opt.ttl> .
<http://example/x3> <http://example/q> ""4""^^<http://www.w3.org/2001/XMLSchema#integer> <http://two-nested-opt.ttl> .";

            TripleStore store = new TripleStore();
            StringParser.ParseDataset(store, data, new NQuadsParser());

            SparqlResultSet results = store.ExecuteQuery(query) as SparqlResultSet;
            Assert.IsNotNull(results);
            TestTools.ShowResults(results);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(2, results.Variables.Count());
        }

        [Test]
        public void SparqlGroupByRefactor3()
        {
            String query = @"BASE <http://www.w3.org/2001/sw/DataAccess/tests/data-r2/algebra/manifest#>
PREFIX : <http://example/>

SELECT ?x ?y ?z FROM <http://join-combo-graph-2.ttl>
FROM NAMED <http://join-combo-graph-1.ttl>
WHERE
{ 
  GRAPH ?g { ?x ?p 1  . } 
  { ?x :p ?y . } UNION { ?p a ?z . } 
}";

            String data = @"<http://example/x1> <http://example/p> ""1""^^<http://www.w3.org/2001/XMLSchema#integer> <http://join-combo-graph-2.ttl> .
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

            TripleStore store = new TripleStore();
            StringParser.ParseDataset(store, data, new NQuadsParser());

            SparqlResultSet results = store.ExecuteQuery(query) as SparqlResultSet;
            Assert.IsNotNull(results);
            TestTools.ShowResults(results);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(3, results.Variables.Count());
        }

        [Test]
        public void SparqlGroupByRefactor4()
        {
            String query = @"PREFIX : <http://example/>

SELECT ?s ?w
FROM <http://group-data-2.ttl>
WHERE
{
  ?s :p ?v .
  OPTIONAL { ?s :q ?w . }
}
GROUP BY ?s ?w";

            String data = @"<http://example/s1> <http://example/p> ""1""^^<http://www.w3.org/2001/XMLSchema#integer> <http://group-data-2.ttl> .
<http://example/s3> <http://example/p> ""1""^^<http://www.w3.org/2001/XMLSchema#integer> <http://group-data-2.ttl> .
<http://example/s1> <http://example/q> ""9""^^<http://www.w3.org/2001/XMLSchema#integer> <http://group-data-2.ttl> .
<http://example/s2> <http://example/p> ""2""^^<http://www.w3.org/2001/XMLSchema#integer> <http://group-data-2.ttl> .";

            TripleStore store = new TripleStore();
            StringParser.ParseDataset(store, data, new NQuadsParser());

            SparqlResultSet results = store.ExecuteQuery(query) as SparqlResultSet;
            Assert.IsNotNull(results);
            TestTools.ShowResults(results);
            Assert.AreEqual(3, results.Count);
            Assert.AreEqual(2, results.Variables.Count());
            Assert.IsTrue(results.All(r => r.Count > 0), "One or more rows were empty");
            Assert.IsTrue(results.All(r => r.HasBoundValue("s")), "One or more rows were empty or failed to have a value for ?s");
        }

        [Test]
        public void SparqlGroupByRefactor5()
        {
            String query = "SELECT ?s WHERE { ?s ?p ?o } GROUP BY ?s";

            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            String queryStr = q.ToString();
            Console.WriteLine("Raw ToString()");
            Console.WriteLine(queryStr);
            Console.WriteLine();
            Assert.IsTrue(queryStr.Contains("GROUP BY ?s"));

            String queryStrFmt = new SparqlFormatter().Format(q);
            Console.WriteLine("Formatted String");
            Console.WriteLine(queryStrFmt);
            Assert.IsTrue(queryStrFmt.Contains("GROUP BY ?s"));
        }

        [Test]
        public void SparqlGroupByRefactor6()
        {
            String query = "SELECT ?s ?p WHERE { ?s ?p ?o } GROUP BY ?s ?p";

            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            String queryStr = q.ToString();
            Console.WriteLine("Raw ToString()");
            Console.WriteLine(queryStr);
            Console.WriteLine();
            Assert.IsTrue(queryStr.Contains("GROUP BY ?s ?p"));

            String queryStrFmt = new SparqlFormatter().Format(q);
            Console.WriteLine("Formatted String");
            Console.WriteLine(queryStrFmt);
            Assert.IsTrue(queryStrFmt.Contains("GROUP BY ?s ?p"));
        }

        [Test]
        public void SparqlGroupByRefactor7()
        {
            String query = "SELECT ?s WHERE { ?s ?p ?o } GROUP BY (?s)";

            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            String queryStr = q.ToString();
            Console.WriteLine("Raw ToString()");
            Console.WriteLine(queryStr);
            Console.WriteLine();
            Assert.IsTrue(queryStr.Contains("GROUP BY ?s"));

            String queryStrFmt = new SparqlFormatter().Format(q);
            Console.WriteLine("Formatted String");
            Console.WriteLine(queryStrFmt);
            Assert.IsTrue(queryStrFmt.Contains("GROUP BY ?s"));
        }

        [Test]
        public void SparqlGroupByRefactor8()
        {
            String query = @"PREFIX : <http://example.org/>
PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>

SELECT ?o ?x (COALESCE(?o / ?x, -2 ) AS ?div)
FROM <http://data-coalesce.ttl>
WHERE
{
  ?s :p ?o .
  OPTIONAL { ?s :q ?x . }
}";

            String data = @"<http://example.org/n0> <http://example.org/p> ""1""^^<http://www.w3.org/2001/XMLSchema#integer> <http://data-coalesce.ttl> .
<http://example.org/n1> <http://example.org/p> ""0""^^<http://www.w3.org/2001/XMLSchema#integer> <http://data-coalesce.ttl> .
<http://example.org/n1> <http://example.org/q> ""0""^^<http://www.w3.org/2001/XMLSchema#integer> <http://data-coalesce.ttl> .
<http://example.org/n2> <http://example.org/p> ""0""^^<http://www.w3.org/2001/XMLSchema#integer> <http://data-coalesce.ttl> .
<http://example.org/n2> <http://example.org/q> ""2""^^<http://www.w3.org/2001/XMLSchema#integer> <http://data-coalesce.ttl> .
<http://example.org/n3> <http://example.org/p> ""4""^^<http://www.w3.org/2001/XMLSchema#integer> <http://data-coalesce.ttl> .
<http://example.org/n3> <http://example.org/q> ""2""^^<http://www.w3.org/2001/XMLSchema#integer> <http://data-coalesce.ttl> .";

            TripleStore store = new TripleStore();
            StringParser.ParseDataset(store, data, new NQuadsParser());

            SparqlResultSet results = store.ExecuteQuery(query) as SparqlResultSet;
            Assert.IsNotNull(results);
            TestTools.ShowResults(results);
            Assert.AreEqual(4, results.Count);
            Assert.AreEqual(3, results.Variables.Count());

            foreach (SparqlResult r in results)
            {
                if (r.HasBoundValue("x"))
                {
                    long xVal = r["x"].AsValuedNode().AsInteger();
                    if (xVal == 0)
                    {
                        Assert.AreEqual(-2, r["div"].AsValuedNode().AsInteger(), "Divide by zero did not get coalesced to -2 as expected");
                    }
                    else
                    {
                        Assert.AreEqual(r["o"].AsValuedNode().AsInteger() / xVal, r["div"].AsValuedNode().AsInteger(), "Division yielded incorrect result");
                    }
                }
                else
                {
                    Assert.AreEqual(-2, r["div"].AsValuedNode().AsInteger(), "Divide by null did not get coalesced to -2 as expected");
                }
            }            
        }

        [Test]
        public void SparqlGroupByRefactor9()
        {
            try
            {
                //Options.UsePLinqEvaluation = false;

                String query = @"PREFIX : <http://example.org/>
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

                String data = @"<http://example.org/n0> <http://example.org/p> ""1""^^<http://www.w3.org/2001/XMLSchema#integer> <http://data-coalesce.ttl> .
<http://example.org/n1> <http://example.org/p> ""0""^^<http://www.w3.org/2001/XMLSchema#integer> <http://data-coalesce.ttl> .
<http://example.org/n1> <http://example.org/q> ""0""^^<http://www.w3.org/2001/XMLSchema#integer> <http://data-coalesce.ttl> .
<http://example.org/n2> <http://example.org/p> ""0""^^<http://www.w3.org/2001/XMLSchema#integer> <http://data-coalesce.ttl> .
<http://example.org/n2> <http://example.org/q> ""2""^^<http://www.w3.org/2001/XMLSchema#integer> <http://data-coalesce.ttl> .
<http://example.org/n3> <http://example.org/p> ""4""^^<http://www.w3.org/2001/XMLSchema#integer> <http://data-coalesce.ttl> .
<http://example.org/n3> <http://example.org/q> ""2""^^<http://www.w3.org/2001/XMLSchema#integer> <http://data-coalesce.ttl> .";

                TripleStore store = new TripleStore();
                StringParser.ParseDataset(store, data, new NQuadsParser());

                SparqlResultSet results = store.ExecuteQuery(query) as SparqlResultSet;
                Assert.IsNotNull(results);
                TestTools.ShowResults(results);
                Assert.AreEqual(4, results.Count);
                Assert.AreEqual(4, results.Variables.Count());

                foreach (SparqlResult r in results)
                {
                    long cxVal = r["cx"].AsValuedNode().AsInteger();
                    if (cxVal == -1) Assert.AreEqual(-2, r["div"].AsValuedNode().AsInteger(), "?div value is incorrect");
                }
            }
            finally
            {
                //Options.UsePLinqEvaluation = true;
            }
        }

        [Test]
        public void SparqlGroupByComplex1()
        {
            String data = @"PREFIX : <http://test/> INSERT DATA { :x :p 1 , 2 . :y :p 5 }";
            String query = @"SELECT ?s (CONCAT('$', STR(SUM(?o))) AS ?Total) WHERE { ?s ?p ?o } GROUP BY ?s";

            TripleStore store = new TripleStore();
            store.ExecuteUpdate(data);
            Assert.AreEqual(1, store.Graphs.Count);
            Assert.AreEqual(3, store.Triples.Count());

            //Aggregates may occur in project expressions and should evaluate correctly
            SparqlResultSet results = store.ExecuteQuery(query) as SparqlResultSet;
            Assert.IsNotNull(results);
            Assert.IsTrue(results.All(r => r.HasBoundValue("Total")));

            SparqlResult x = results.Where(r => ((IUriNode)r["s"]).Uri.Equals(new Uri("http://test/x"))).FirstOrDefault();
            Assert.IsNotNull(x);
            Assert.AreEqual("$3", x["Total"].AsValuedNode().AsString());

            SparqlResult y = results.Where(r => ((IUriNode)r["s"]).Uri.Equals(new Uri("http://test/y"))).FirstOrDefault();
            Assert.IsNotNull(y);
            Assert.AreEqual("$5", y["Total"].AsValuedNode().AsString());
        }

        [Test,ExpectedException(typeof(RdfParseException))]
        public void SparqlGroupByComplex2()
        {
            //Nested aggregates are a parser error
            String query = @"SELECT ?s (SUM(MIN(?o)) AS ?Total) WHERE { ?s ?p ?o } GROUP BY ?s";
            SparqlQueryParser parser = new SparqlQueryParser();
            parser.ParseFromString(query);
        }

        [Test]
        public void SparqlGroupByWithValues1()
        {
            String query = @"SELECT ?a WHERE { VALUES ( ?a ) { ( 1 ) ( 2 ) } } GROUP BY ?a";

            QueryableGraph g = new QueryableGraph();
            SparqlResultSet results = g.ExecuteQuery(query) as SparqlResultSet;
            Assert.IsNotNull(results);
            Assert.IsFalse(results.IsEmpty);
            Assert.AreEqual(2, results.Count);
        }

        [Test]
        public void SparqlGroupByWithValues2()
        {
            String query = @"SELECT ?a ?b WHERE { VALUES ( ?a ?b ) { ( 1 2 ) ( 1 UNDEF ) ( UNDEF 2 ) } } GROUP BY ?a ?b";

            QueryableGraph g = new QueryableGraph();
            SparqlResultSet results = g.ExecuteQuery(query) as SparqlResultSet;
            Assert.IsNotNull(results);
            Assert.IsFalse(results.IsEmpty);
            Assert.AreEqual(3, results.Count);
        }

        [Test]
        public void SparqlGroupByWithGraph1()
        {
            String query = @"SELECT ?g WHERE { GRAPH ?g { } } GROUP BY ?g";

            TripleStore store = new TripleStore();
            IGraph def = new Graph();
            store.Add(def);
            IGraph named = new Graph();
            named.BaseUri = new Uri("http://name");
            store.Add(named);

            Assert.AreEqual(2, store.Graphs.Count);

            SparqlQuery q = new SparqlQueryParser().ParseFromString(query);
            SparqlResultSet results = store.ExecuteQuery(q) as SparqlResultSet;
            Assert.IsNotNull(results);
            Assert.IsFalse(results.IsEmpty);

            //Count only covers named graphs
            Assert.AreEqual(1, results.Count);
        }
    }
}
