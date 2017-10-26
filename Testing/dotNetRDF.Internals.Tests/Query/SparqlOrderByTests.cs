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
using Xunit;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Ordering;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query
{

    public class SparqlOrderByTests
    {
        [Fact]
        public void SparqlOrderByWithRawVariableName()
        {
            // when
            var ordering = new OrderByVariable("name");

            // then
            Assert.Equal("name", ordering.Variables.Single());
        }

        [Fact]
        public void SparqlOrderByWithVariableNameWithDollarSign()
        {
            // when
            var ordering = new OrderByVariable("$name");

            // then
            Assert.Equal("name", ordering.Variables.Single());
        }

        [Fact]
        public void SparqlOrderByWithVariableNameWithQuestionMark()
        {
            // when
            var ordering = new OrderByVariable("?name");

            // then
            Assert.Equal("name", ordering.Variables.Single());
        }

        [Fact]
        public void SparqlOrderByDescendingScope1()
        {
            //Test Case for CORE-350
            //DESC() on a condition causes subsequent condition to act as DESC even if it was an ASC condition

            IGraph g = new Graph();
            g.NamespaceMap.AddNamespace(String.Empty, UriFactory.Create("http://example/"));
            INode a = g.CreateUriNode(":a");
            INode b = g.CreateUriNode(":b");
            INode c = g.CreateUriNode(":c");

            List<INode> nodes = new List<INode>() { a, b, c };
            List<Triple> ts = new List<Triple>();
            foreach (INode s in nodes)
            {
                foreach (INode p in nodes.OrderByDescending(x => x))
                {
                    foreach (INode o in nodes)
                    {
                        ts.Add(new Triple(s, p, o));
                    }
                }
            }
            g.Assert(ts);
            Assert.Equal(27, g.Triples.Count);

            String query = @"SELECT * WHERE { ?s ?p ?o } ORDER BY ?s DESC(?p) ?o";
            SparqlQuery q = new SparqlQueryParser().ParseFromString(query);

            SparqlResultSet results = g.ExecuteQuery(q) as SparqlResultSet;
            Assert.NotNull(results);
            Assert.Equal(27, results.Count);

            for (int i = 0; i < ts.Count; i++)
            {
                Triple t = new Triple(results[i]["s"], results[i]["p"], results[i]["o"]);
                Assert.Equal(ts[i], t);
            }
        }

        [Fact]
        public void SparqlOrderByDescendingScope2()
        {
            //Test Case for CORE-350
            //DESC() on a condition causes subsequent condition to act as DESC even if it was an ASC condition

            IGraph g = new Graph();
            g.NamespaceMap.AddNamespace(String.Empty, UriFactory.Create("http://example/"));
            INode a = g.CreateUriNode(":a");
            INode b = g.CreateUriNode(":b");
            INode c = g.CreateUriNode(":c");

            List<INode> nodes = new List<INode>() { a, b, c };
            List<Triple> ts = new List<Triple>();
            foreach (INode s in nodes)
            {
                foreach (INode p in nodes.OrderByDescending(x => x))
                {
                    foreach (INode o in nodes)
                    {
                        ts.Add(new Triple(s, p, o));
                    }
                }
            }
            g.Assert(ts);
            Assert.Equal(27, g.Triples.Count);

            String query = @"SELECT * WHERE { ?s ?p ?o } ORDER BY STR(?s) DESC(STR(?p)) STR(?o)";
            SparqlQuery q = new SparqlQueryParser().ParseFromString(query);

            SparqlResultSet results = g.ExecuteQuery(q) as SparqlResultSet;
            Assert.NotNull(results);
            Assert.Equal(27, results.Count);

            for (int i = 0; i < ts.Count; i++)
            {
                Triple t = new Triple(results[i]["s"], results[i]["p"], results[i]["o"]);
                Assert.Equal(ts[i], t);
            }
        }

        [Fact]
        public void SparqlOrderByNullInFirstCondition1()
        {
            //Test Case for CORE-350
            //If first condition has a null in it subsequent conditions are not applied

            String query = @"SELECT * WHERE
{
  VALUES ( ?a ?b )
  {
    (UNDEF 2)
    (UNDEF 1)
    (UNDEF 3)
    ('a' 3)
    ('a' 2)
    ('a' 1)
  }
} ORDER BY ?a ?b";

            Graph g = new Graph();
            SparqlQuery q = new SparqlQueryParser().ParseFromString(query);

            SparqlResultSet results = g.ExecuteQuery(q) as SparqlResultSet;
            Assert.NotNull(results);
            Assert.Equal(6, results.Count);

            for (int i = 0; i < results.Count; i++)
            {
                if (i < 3)
                {
                    Assert.False(results[i].HasBoundValue("a"));
                }
                else
                {
                    Assert.True(results[i].HasBoundValue("a"));
                }
                int expected = (i % 3) + 1;
                Assert.Equal(expected, results[i]["b"].AsValuedNode().AsInteger());
            }
        }

        [Fact]
        public void SparqlOrderByNullInFirstCondition2()
        {
            //Test Case for CORE-350
            //If first condition has a null in it subsequent conditions are not applied

            String query = @"SELECT * WHERE
{
  VALUES ( ?a ?b )
  {
    (UNDEF 2)
    (UNDEF 1)
    (UNDEF 3)
    ('a' 3)
    ('a' 2)
    ('a' 1)
  }
} ORDER BY STR(?a) STR(?b)";

            Graph g = new Graph();
            SparqlQuery q = new SparqlQueryParser().ParseFromString(query);

            SparqlResultSet results = g.ExecuteQuery(q) as SparqlResultSet;
            Assert.NotNull(results);
            Assert.Equal(6, results.Count);

            for (int i = 0; i < results.Count; i++)
            {
                if (i < 3)
                {
                    Assert.False(results[i].HasBoundValue("a"));
                }
                else
                {
                    Assert.True(results[i].HasBoundValue("a"));
                }
                int expected = (i % 3) + 1;
                Assert.Equal(expected, results[i]["b"].AsValuedNode().AsInteger());
            }
        }
    }
}