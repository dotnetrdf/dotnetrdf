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
using System.Data;
using System.IO;
using System.Linq;
using Xunit;
using VDS.RDF.Data.DataTables;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace VDS.RDF;

public class SparqlResultsToDataTableTests
{
    [Fact]
    public void SparqlResultSetToDataTable()
    {
        var query = "SELECT * WHERE {?s ?p ?o}";
        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);

        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

        var results = g.ExecuteQuery(q);
        if (results is SparqlResultSet resultSet)
        {
            var table = new DataTable();
            foreach (var var in resultSet.Variables)
            {
                table.Columns.Add(new DataColumn(var, typeof(INode)));
            }

            foreach (ISparqlResult r in resultSet)
            {
                DataRow row = table.NewRow();

                foreach (var var in resultSet.Variables)
                {
                    if (r.HasValue(var))
                    {
                        row[var] = r[var];
                    }
                    else
                    {
                        row[var] = null;
                    }
                }
                table.Rows.Add(row);
            }

            Assert.Equal(resultSet.Variables.Count(), table.Columns.Count);
            Assert.Equal(resultSet.Count, table.Rows.Count);
        }
        else
        {
            Assert.Fail("Query should have returned a Result Set");
        }
    }

    [Fact]
    public void SparqlResultSetToDataTable2()
    {
        var query = "PREFIX ex: <http://example.org/vehicles/> SELECT * WHERE {?s a ex:Car . OPTIONAL { ?s ex:Speed ?speed }}";
        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);

        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

        var results = g.ExecuteQuery(q);
        if (results is SparqlResultSet resultSet)
        {
            var table = new DataTable();
            foreach (var var in resultSet.Variables)
            {
                table.Columns.Add(new DataColumn(var, typeof(INode)));
            }

            foreach (ISparqlResult r in resultSet)
            {
                DataRow row = table.NewRow();

                foreach (var var in resultSet.Variables)
                {
                    if (r.HasValue(var))
                    {
                        row[var] = r[var];
                    }
                    else
                    {
                        row[var] = null;
                    }
                }
                table.Rows.Add(row);
            }

            Assert.Equal(resultSet.Variables.Count(), table.Columns.Count);
            Assert.Equal(resultSet.Count, table.Rows.Count);
        }
        else
        {
            Assert.Fail("Query should have returned a Result Set");
        }
    }

    [Fact]
    public void SparqlResultSetToDataTable3()
    {
        var query = "SELECT * WHERE {?s ?p ?o}";
        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);

        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

        var results = g.ExecuteQuery(q);
        if (results is SparqlResultSet resultSet)
        {
            var table = resultSet.ToDataTable();
            Assert.Equal(resultSet.Variables.Count(), table.Columns.Count);
            Assert.Equal(resultSet.Count, table.Rows.Count);
        }
        else
        {
            Assert.Fail("Query should have returned a Result Set");
        }
    }

    [Fact]
    public void SparqlResultSetToDataTable4()
    {
        var query = "PREFIX ex: <http://example.org/vehicles/> SELECT * WHERE {?s a ex:Car . OPTIONAL { ?s ex:Speed ?speed }}";
        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);

        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

        var results = g.ExecuteQuery(q);
        if (results is SparqlResultSet resultSet)
        {
            var table = resultSet.ToDataTable();
            Assert.Equal(resultSet.Variables.Count(), table.Columns.Count);
            Assert.Equal(resultSet.Count, table.Rows.Count);
        }
        else
        {
            Assert.Fail("Query should have returned a Result Set");
        }
    }

    [Fact]
    public void SparqlResultSetToDataTable5()
    {
        var query = "ASK WHERE {?s ?p ?o}";
        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);

        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

        var results = g.ExecuteQuery(q);
        if (results is SparqlResultSet resultSet)
        {
            var table = resultSet.ToDataTable();
            Assert.Equal(SparqlResultsType.Boolean, resultSet.ResultsType);
            Assert.Single(table.Columns);
            Assert.Single(table.Rows);
            Assert.True((bool)table.Rows[0]["ASK"], "Should be true");
        }
        else
        {
            Assert.Fail("Query should have returned a Result Set");
        }
    }

    [Fact]
    public void SparqlResultSetToDataTable6()
    {
        var query = "ASK WHERE {?s <http://example.org/noSuchPredicate> ?o}";
        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);

        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

        var results = g.ExecuteQuery(q);
        if (results is SparqlResultSet resultSet)
        {
            var table = resultSet.ToDataTable();

            Assert.Equal(SparqlResultsType.Boolean, resultSet.ResultsType);
            Assert.Single(table.Columns);
            Assert.Single(table.Rows);
            Assert.False((bool)table.Rows[0]["ASK"], "Should be false");
        }
        else
        {
            Assert.Fail("Query should have returned a Result Set");
        }
    }

    [Fact]
    public void SparqlResultSetToDataTable7()
    {
        var resultSet = new SparqlResultSet(true);

        var table = resultSet.ToDataTable();

        Assert.Equal(SparqlResultsType.Boolean, resultSet.ResultsType);
        Assert.Single(table.Columns);
        Assert.Single(table.Rows);
        Assert.True((bool)table.Rows[0]["ASK"], "Should be true");
    }

    [Fact]
    public void SparqlResultSetToDataTable8()
    {
        var resultSet = new SparqlResultSet(false);

        var table = resultSet.ToDataTable();

        Assert.Equal(SparqlResultsType.Boolean, resultSet.ResultsType);
        Assert.Single(table.Columns);
        Assert.Single(table.Rows);
        Assert.False((bool)table.Rows[0]["ASK"], "Should be false");
    }

    [Fact]
    public void SparqlResultSetToDataTable9()
    {
        var results = new SparqlResultSet();
        Assert.Throws<InvalidCastException>(() => results.ToDataTable());
    }
}
