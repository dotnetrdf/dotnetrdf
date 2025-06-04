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
using VDS.RDF.Parsing;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query;


public class ResultAccessTests
{
    private readonly LeviathanQueryProcessor _processor;
    private readonly SparqlQueryParser _parser = new SparqlQueryParser();
    private readonly NTriplesFormatter _formatter = new NTriplesFormatter();

    public ResultAccessTests()
    {
        var g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        var store = new TripleStore();
        store.Add(g);
        ISparqlDataset dataset = new InMemoryDataset(store);
        _processor = new LeviathanQueryProcessor(dataset);
    }

    private SparqlQuery CreateQuery(String query)
    {
        var nsMapper = new NamespaceMapper();
        nsMapper.AddNamespace("ex", new Uri("http://example.org/"));
        var queryStr = new SparqlParameterizedString(query) {Namespaces = nsMapper};
        return _parser.ParseFromString(queryStr);
    }

    private SparqlResultSet GetResults(SparqlQuery query)
    {
        object results = _processor.ProcessQuery(query);
        Assert.IsType<SparqlResultSet>(results, exactMatch: false);
        return results as SparqlResultSet;
    }

    [Fact]
    public void SparqlResultAccessByName()
    {
        var query = "SELECT * WHERE { ?s a ?type ; rdfs:comment ?comment }";
        SparqlQuery q = CreateQuery(query);
        SparqlResultSet results = GetResults(q);

        foreach (SparqlResult r in results)
        {
            Assert.NotNull(r["s"]);
            Assert.NotNull(r["comment"]);
            Assert.NotNull(r["type"]);
        }
    }

    [Fact]
    public void SparqlResultAccessByNameError()
    {
        var query =
            "PREFIX ex: <http://example.org/> SELECT * WHERE { ?s a ?type . OPTIONAL { ?s ex:range ?range } }";
        SparqlQuery q = CreateQuery(query);
        SparqlResultSet results = GetResults(q);

        foreach (SparqlResult r in results)
        {
            Assert.Throws<RdfException>(() => r["range"]);
        }
    }

    [Fact]
    public void SparqlResultAccessByNameSafeHasValue()
    {
        var query = "SELECT * WHERE { ?s a ?type . OPTIONAL { ?s rdfs:range ?range } }";
        SparqlQuery q = CreateQuery(query);
        SparqlResultSet results = GetResults(q);

        var vars = results.Variables.ToList();
        foreach (SparqlResult r in results)
        {
            foreach (var var in vars)
            {
                if (r.HasValue(var) && r[var] != null)
                {
                    Assert.NotNull(r[var].ToString(_formatter));
                }
            }
        }
    }

    [Fact]
    public void SparqlResultAccessByNameSafeTryGetValue()
    {
        var query = "SELECT * WHERE { ?s a ?type . OPTIONAL { ?s rdfs:range ?range } }";
        SparqlQuery q = CreateQuery(query);
        SparqlResultSet results = GetResults(q);

        var vars = results.Variables.ToList();
        foreach (SparqlResult r in results)
        {
            foreach (var var in vars)
            {
                if (r.TryGetValue(var, out INode value) && value != null)
                {
                    Assert.NotNull(value.ToString(_formatter));
                }
            }
        }
    }

    [Fact]
    public void SparqlResultAccessByNameSafeTryGetBoundValue()
    {
        var query = "SELECT * WHERE { ?s a ?type . OPTIONAL { ?s rdfs:range ?range } }";
        SparqlQuery q = CreateQuery(query);
        SparqlResultSet results = GetResults(q);

        var vars = results.Variables.ToList();
        foreach (SparqlResult r in results)
        {
            foreach (var var in vars)
            {
                if (r.TryGetBoundValue(var, out INode value))
                {
                    Assert.NotNull(value);
                }
            }
        }
    }

    [Fact]
    public void SparqlResultAccessByIndex()
    {
        var query = "SELECT * WHERE { ?s a ?type ; rdfs:comment ?comment }";
        SparqlQuery q = CreateQuery(query);
        SparqlResultSet results = GetResults(q);

        var vars = results.Variables.ToList();
        foreach (SparqlResult r in results)
        {
            for (var i = 0; i < vars.Count; i++)
            {
                Assert.NotNull(r[i]);
            }
        }
    }

    [Fact]
    public void SparqlResultSetVariableOrder4()
    {
        var query = "SELECT ?comment ?type WHERE { ?s a ?type ; rdfs:comment ?comment }";
        SparqlQuery q = CreateQuery(query);
        SparqlResultSet results = GetResults(q);

        TestVariableOrder(results, new List<String>() { "comment", "type" });
    }

    [Fact]
    public void SparqlResultSetVariableOrder5()
    {
        var data = @"<?xml version=""1.0"" encoding=""utf-8""?>
<sparql xmlns=""" + SparqlSpecsHelper.SparqlNamespace + @""">
    <head>
        <variable name=""a"" />
        <variable name=""b"" />
    </head>
    <results />
</sparql>";

        var results = new SparqlResultSet();
        StringParser.ParseResultSet(results, data, new SparqlXmlParser());

        TestVariableOrder(results, new List<string>() { "a", "b" });
    }

    [Fact]
    public void SparqlResultSetVariableOrder6()
    {
        var data = @"<?xml version=""1.0"" encoding=""utf-8""?>
<sparql xmlns=""" + SparqlSpecsHelper.SparqlNamespace + @""">
    <head>
        <variable name=""b"" />
        <variable name=""a"" />
    </head>
    <results />
</sparql>";

        var results = new SparqlResultSet();
        StringParser.ParseResultSet(results, data, new SparqlXmlParser());

        TestVariableOrder(results, new List<string>() { "b", "a" });
    }

    [Fact]
    public void SparqlResultSetVariableOrder7()
    {
        var data = @"<?xml version=""1.0"" encoding=""utf-8""?>
<sparql xmlns=""" + SparqlSpecsHelper.SparqlNamespace + @""">
    <head>
        <variable name=""b"" />
        <variable name=""a"" />
    </head>
    <results>
        <result>
            <binding name=""a""><uri>http://example</uri></binding>
            <binding name=""b""><uri>http://example</uri></binding>
        </result>
        <result>
            <binding name=""b""><uri>http://example</uri></binding>
            <binding name=""a""><uri>http://example</uri></binding>
        </result>
        <result>
            <binding name=""a""><uri>http://example</uri></binding>
        </result>
        <result>
            <binding name=""b""><uri>http://example</uri></binding>
        </result>
    </results>
</sparql>";

        var results = new SparqlResultSet();
        StringParser.ParseResultSet(results, data, new SparqlXmlParser());

        TestVariableOrder(results, new List<string>() { "b", "a" });
    }

    private void TestVariableOrder(SparqlResultSet results, List<String> expected)
    {
        var vars = results.Variables.ToList();
        TestVariableOrder(expected, vars);

        foreach (SparqlResult r in results)
        {
            vars = r.Variables.ToList();
            TestVariableOrder(expected, vars);
        }
    }

    private void TestVariableOrder(List<String> expected, List<String> actual)
    {
        Assert.Equal(expected.Count, actual.Count);
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i], actual[i]);
        }
    }
}
