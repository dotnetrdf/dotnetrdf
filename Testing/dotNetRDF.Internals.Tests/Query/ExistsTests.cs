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
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query
{

    public class ExistsTests
    {
        [Fact]
        public void SparqlExistsSimple1()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            String query = @"SELECT *
WHERE
{
  ?s ?p ?o .
  FILTER EXISTS { ?s a ?type }
}";

            SparqlQuery q = new SparqlQueryParser().ParseFromString(query);

            SparqlResultSet results = g.ExecuteQuery(q) as SparqlResultSet;
            Assert.NotNull(results);
            Assert.False(results.IsEmpty);
        }

        [Fact]
        public void SparqlExistsSimple2()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            int expected = g.GetTriplesWithObject(g.CreateUriNode(UriFactory.Create(ConfigurationLoader.ClassHttpHandler))).Count();

            String query = @"SELECT *
WHERE
{
  ?s ?p ?o .
  FILTER EXISTS { ?s ?property <" + ConfigurationLoader.ClassHttpHandler + @"> }
}";

            SparqlQuery q = new SparqlQueryParser().ParseFromString(query);

            SparqlResultSet results = g.ExecuteQuery(q) as SparqlResultSet;
            Assert.NotNull(results);
            Assert.False(results.IsEmpty);
            Assert.True(expected < results.Count, "Should be more triples found that just those matched by the EXISTS clause");
        }

        [Fact]
        public void SparqlNotExistsEmptyBgp()
        {
            var g = new Graph();
            g.Assert(g.CreateUriNode(new Uri("http://example.org/s")),
                g.CreateUriNode(new Uri("http://example.org/p")),
                g.CreateUriNode(new Uri("http://example.org/o")));

            // If FILTER NOT EXISTS returns false, no results expected
            var query =
                @"SELECT (<http://example.org/foo> as ?subject) WHERE { FILTER NOT EXISTS { <http://example.org/s> <http://example.org/p> <http://example.org/o> } }";
            var q = new SparqlQueryParser().ParseFromString(query);
            var results = g.ExecuteQuery(q) as SparqlResultSet;
            Assert.NotNull(results);
            Assert.True(results.IsEmpty);
            
            // If FILTER NOT EXISTS returns true, one result expected
            query =
                @"SELECT (<http://example.org/foo> as ?subject) WHERE { FILTER NOT EXISTS { <http://example.org/s> <http://example.org/p> <http://example.org/o2> } }";
            q = new SparqlQueryParser().ParseFromString(query);
            results = g.ExecuteQuery(q) as SparqlResultSet;
            Assert.NotNull(results);
            Assert.False(results.IsEmpty);
            Assert.Equal(1, results.Count);

            // If FILTER EXISTS returns true, one result expected
            query =
                @"SELECT (<http://example.org/foo> as ?subject) WHERE { FILTER EXISTS { <http://example.org/s> <http://example.org/p> <http://example.org/o> } }";
            q = new SparqlQueryParser().ParseFromString(query);
            results = g.ExecuteQuery(q) as SparqlResultSet;
            Assert.NotNull(results);
            Assert.False(results.IsEmpty);
            Assert.Equal(1, results.Count);

            // If FILTER EXISTS returns false, no results expected
            query =
                @"SELECT (<http://example.org/foo> as ?subject) WHERE { FILTER EXISTS { <http://example.org/s> <http://example.org/p> <http://example.org/o2> } }";
            q = new SparqlQueryParser().ParseFromString(query);
            results = g.ExecuteQuery(q) as SparqlResultSet;
            Assert.NotNull(results);
            Assert.True(results.IsEmpty);
        }
    }
}
