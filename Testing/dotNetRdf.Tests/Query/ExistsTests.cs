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
using System.Linq;
using Xunit;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query;


public class ExistsTests
{
    [Fact]
    public void SparqlExistsSimple1()
    {
        var g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

        var query = @"SELECT *
WHERE
{
  ?s ?p ?o .
  FILTER EXISTS { ?s a ?type }
}";

        SparqlQuery q = new SparqlQueryParser().ParseFromString(query);

        var results = g.ExecuteQuery(q) as SparqlResultSet;
        Assert.NotNull(results);
        Assert.False(results.IsEmpty);
    }

    [Fact]
    public void SparqlExistsSimple2()
    {
        var g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        var expected = g.GetTriplesWithObject(g.CreateUriNode(UriFactory.Root.Create(ConfigurationLoader.ClassHttpHandler))).Count();

        var query = @"SELECT *
WHERE
{
  ?s ?p ?o .
  FILTER EXISTS { ?s ?property <" + ConfigurationLoader.ClassHttpHandler + @"> }
}";

        SparqlQuery q = new SparqlQueryParser().ParseFromString(query);

        var results = g.ExecuteQuery(q) as SparqlResultSet;
        Assert.NotNull(results);
        Assert.False(results.IsEmpty);
        Assert.True(expected < results.Count, "Should be more triples found that just those matched by the EXISTS clause");
    }

    [Theory]
    [InlineData("FILTER NOT EXISTS { <http://example.org/s> <http://example.org/p> <http://example.org/o> }", 0)]
    [InlineData("FILTER NOT EXISTS { <http://example.org/s> <http://example.org/p> <http://example.org/o2> }", 1)]
    [InlineData("FILTER EXISTS { <http://example.org/s> <http://example.org/p> <http://example.org/o> }", 1)]
    [InlineData("FILTER EXISTS { <http://example.org/s> <http://example.org/p> <http://example.org/o2> }", 0)]
    [InlineData("FILTER NOT EXISTS { <http://example.org/s> <http://example.org/p> ?o }", 0)]
    public void SparqlNotExistsEmptyBgp(string filter, int expectCount)
    {
        var g = new Graph();
        g.Assert(g.CreateUriNode(new Uri("http://example.org/s")),
            g.CreateUriNode(new Uri("http://example.org/p")),
            g.CreateUriNode(new Uri("http://example.org/o")));

        // If FILTER NOT EXISTS returns false, no results expected
        var query =
            $"SELECT (<http://example.org/foo> as ?subject) WHERE {{ {filter} }}";
        SparqlQuery q = new SparqlQueryParser().ParseFromString(query);
        var results = g.ExecuteQuery(q) as SparqlResultSet;
        Assert.NotNull(results);
        Assert.Equal(expectCount, results.Count);
    }

    [Theory]
    [InlineData("FILTER NOT EXISTS { <http://example.org/s> <http://example.org/p> <http://example.org/o> }", 1)]
    [InlineData("FILTER NOT EXISTS { <http://example.org/s> <http://example.org/p> ?o }", 1)]
    [InlineData("FILTER EXISTS { <http://example.org/s> <http://example.org/p> <http://example.org/o> }", 0)]
    [InlineData("FILTER EXISTS { <http://example.org/s> <http://example.org/p> ?o }", 0)]
    public void SparqlNotExistsEmptyBgpEmptyGraph(string filter, int expectCount)
    {
        var g = new Graph();
        // If FILTER NOT EXISTS returns false, no results expected
        var query =
            $"SELECT (<http://example.org/foo> as ?subject) WHERE {{ {filter} }}";
        SparqlQuery q = new SparqlQueryParser().ParseFromString(query);
        var results = g.ExecuteQuery(q) as SparqlResultSet;
        Assert.NotNull(results);
        Assert.Equal(expectCount, results.Count);
    }
}
