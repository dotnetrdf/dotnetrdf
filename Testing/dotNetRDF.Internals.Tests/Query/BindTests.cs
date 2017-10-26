/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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

namespace VDS.RDF.Query
{

    public class BindTests
    {
        [Fact]
        public void SparqlBindExistsAsChildExpression1()
        {
            String query = @"SELECT * WHERE
{
  ?s a ?type .
  BIND(IF(EXISTS { ?s rdfs:range ?range }, true, false) AS ?hasRange)
}";

            SparqlParameterizedString queryStr = new SparqlParameterizedString(query);
            queryStr.Namespaces.AddNamespace("rdfs", UriFactory.Create(NamespaceMapper.RDFS));

            SparqlQuery q = new SparqlQueryParser().ParseFromString(queryStr);

            IGraph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            SparqlResultSet results = g.ExecuteQuery(q) as SparqlResultSet;
            Assert.NotNull(results);

            TestTools.ShowResults(results);

            Assert.True(results.All(r => r.HasBoundValue("hasRange")));
        }

        [Fact]
        public void SparqlBindExistsAsChildExpression2()
        {
            String query = @"SELECT * WHERE
{
  ?s a ?type .
  BIND(IF(EXISTS { ?s rdfs:range ?range . FILTER EXISTS { ?s rdfs:domain ?domain } }, true, false) AS ?hasRangeAndDomain)
}";

            SparqlParameterizedString queryStr = new SparqlParameterizedString(query);
            queryStr.Namespaces.AddNamespace("rdfs", UriFactory.Create(NamespaceMapper.RDFS));

            SparqlQuery q = new SparqlQueryParser().ParseFromString(queryStr);

            IGraph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            SparqlResultSet results = g.ExecuteQuery(q) as SparqlResultSet;
            Assert.NotNull(results);

            TestTools.ShowResults(results);

            Assert.True(results.All(r => r.HasBoundValue("hasRangeAndDomain")));
        }

        [Fact]
        public void SparqlBindOverEmptyFilter1()
        {
            String query = "SELECT * WHERE { FILTER(false). BIND('test' AS ?test) }";
            SparqlQuery q = new SparqlQueryParser().ParseFromString(query);

            IGraph g = new Graph();
            SparqlResultSet results = g.ExecuteQuery(q) as SparqlResultSet;
            Assert.NotNull(results);

            TestTools.ShowResults(results);
            Assert.True(results.IsEmpty);
        }

        [Fact]
        public void SparqlBindOverEmptyFilter2()
        {
            String query = "SELECT * WHERE { FILTER(true). BIND('test' AS ?test) }";
            SparqlQuery q = new SparqlQueryParser().ParseFromString(query);

            IGraph g = new Graph();
            SparqlResultSet results = g.ExecuteQuery(q) as SparqlResultSet;
            Assert.NotNull(results);

            TestTools.ShowResults(results);
            Assert.False(results.IsEmpty);
            Assert.Equal(1, results.Count);
        }

        [Fact]
        public void SparqlBindEmpty1()
        {
            String query = "SELECT * WHERE { ?s ?p ?o . BIND('test' AS ?test) }";
            SparqlQuery q = new SparqlQueryParser().ParseFromString(query);

            IGraph g = new Graph();
            SparqlResultSet results = g.ExecuteQuery(q) as SparqlResultSet;
            Assert.NotNull(results);

            TestTools.ShowResults(results);
            Assert.True(results.IsEmpty);
        }

        [Fact]
        public void SparqlBindEmpty2()
        {
            String query = "SELECT * WHERE { BIND('test' AS ?test) }";
            SparqlQuery q = new SparqlQueryParser().ParseFromString(query);

            IGraph g = new Graph();
            SparqlResultSet results = g.ExecuteQuery(q) as SparqlResultSet;
            Assert.NotNull(results);

            TestTools.ShowResults(results);
            Assert.False(results.IsEmpty);
            Assert.Equal(1, results.Count);
        }
    }
}
