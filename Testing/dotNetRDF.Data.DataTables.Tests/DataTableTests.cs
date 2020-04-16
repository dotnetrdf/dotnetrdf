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
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Data.DataTables;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using Xunit;
using Xunit.Abstractions;

namespace dotNetRDF.Data.DataTables.Tests
{
    public class DataTableTests
    {
        private readonly ITestOutputHelper _output;

        public DataTableTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void SparqlResultSetToDataTable()
        {
            String query = "SELECT * WHERE {?s ?p ?o}";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            Graph g = new Graph();
            FileLoader.Load(g, "resources\\InferenceTest.ttl");

            Object results = g.ExecuteQuery(q);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;

                DataTable table = new DataTable();
                foreach (String var in rset.Variables)
                {
                    table.Columns.Add(new DataColumn(var, typeof(INode)));
                }

                foreach (SparqlResult r in rset)
                {
                    DataRow row = table.NewRow();

                    foreach (String var in rset.Variables)
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

                Assert.Equal(rset.Variables.Count(), table.Columns.Count);
                Assert.Equal(rset.Count, table.Rows.Count);

                foreach (DataRow row in table.Rows)
                {
                    var s = new StringBuilder();
                    foreach (DataColumn col in table.Columns)
                    {
                        var temp = row[col];
                        s.Append(col.ColumnName + " = " + ((temp != null) ? temp.ToString() : string.Empty) + " , ");
                    }
                    _output.WriteLine(s.ToString());
                }
            }
            else
            {
                Assert.True(false, "Query should have returned a Result Set");
            }
        }

        [Fact]
        public void SparqlResultSetToDataTable2()
        {
            String query = "PREFIX ex: <http://example.org/vehicles/> SELECT * WHERE {?s a ex:Car . OPTIONAL { ?s ex:Speed ?speed }}";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            Graph g = new Graph();
            FileLoader.Load(g, "resources\\InferenceTest.ttl");

            Object results = g.ExecuteQuery(q);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;

                DataTable table = new DataTable();
                foreach (String var in rset.Variables)
                {
                    table.Columns.Add(new DataColumn(var, typeof(INode)));
                }

                foreach (SparqlResult r in rset)
                {
                    DataRow row = table.NewRow();

                    foreach (String var in rset.Variables)
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

                Assert.Equal(rset.Variables.Count(), table.Columns.Count);
                Assert.Equal(rset.Count, table.Rows.Count);

                foreach (DataRow row in table.Rows)
                {
                    var sb = new StringBuilder();
                    foreach (DataColumn col in table.Columns)
                    {
                        Object temp = row[col];
                        sb.Append(col.ColumnName + " = " + ((temp != null) ? temp.ToString() : String.Empty) + " , ");
                    }
                    _output.WriteLine(sb.ToString());
                }
            }
            else
            {
                Assert.True(false, "Query should have returned a Result Set");
            }
        }

        [Fact]
        public void SparqlResultSetToDataTable3()
        {
            String query = "SELECT * WHERE {?s ?p ?o}";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            Graph g = new Graph();
            FileLoader.Load(g, "resources\\InferenceTest.ttl");

            Object results = g.ExecuteQuery(q);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;

                DataTable table = rset.ToDataTable();

                Assert.Equal(rset.Variables.Count(), table.Columns.Count);
                Assert.Equal(rset.Count, table.Rows.Count);

                DumpTable(table);
            }
            else
            {
                Assert.True(false, "Query should have returned a Result Set");
            }
        }

        private void DumpTable(DataTable table)
        {
            foreach (DataRow row in table.Rows)
            {
                var sb = new StringBuilder();
                foreach (DataColumn col in table.Columns)
                {
                    var temp = row[col];
                    sb.Append(col.ColumnName + " = " + ((temp != null) ? temp.ToString() : string.Empty) + " , ");
                }

                _output.WriteLine(sb.ToString());
            }
        }

        [Fact]
        public void SparqlResultSetToDataTable4()
        {
            String query = "PREFIX ex: <http://example.org/vehicles/> SELECT * WHERE {?s a ex:Car . OPTIONAL { ?s ex:Speed ?speed }}";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            Graph g = new Graph();
            FileLoader.Load(g, "resources\\InferenceTest.ttl");

            Object results = g.ExecuteQuery(q);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;

                DataTable table = rset.ToDataTable();

                Assert.Equal(rset.Variables.Count(), table.Columns.Count);
                Assert.Equal(rset.Count, table.Rows.Count);
                DumpTable(table);
            }
            else
            {
                Assert.True(false, "Query should have returned a Result Set");
            }
        }

        [Fact]
        public void SparqlResultSetToDataTable5()
        {
            String query = "ASK WHERE {?s ?p ?o}";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            Graph g = new Graph();
            FileLoader.Load(g, "resources\\InferenceTest.ttl");

            Object results = g.ExecuteQuery(q);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;

                DataTable table = rset.ToDataTable();

                Assert.True(rset.ResultsType == SparqlResultsType.Boolean);
                Assert.Single(table.Columns);
                Assert.Single(table.Rows);
                Assert.True((bool)table.Rows[0]["ASK"], "Should be true");
                DumpTable(table);
            }
            else
            {
                Assert.True(false, "Query should have returned a Result Set");
            }
        }

        [Fact]
        public void SparqlResultSetToDataTable6()
        {
            String query = "ASK WHERE {?s <http://example.org/noSuchPredicate> ?o}";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            Graph g = new Graph();
            FileLoader.Load(g, "resources\\InferenceTest.ttl");

            Object results = g.ExecuteQuery(q);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;

                DataTable table = rset.ToDataTable();

                Assert.True(rset.ResultsType == SparqlResultsType.Boolean);
                Assert.Single(table.Columns);
                Assert.Single(table.Rows);
                Assert.False((bool)table.Rows[0]["ASK"], "Should be false");
                DumpTable(table);
            }
            else
            {
                Assert.True(false, "Query should have returned a Result Set");
            }
        }

        [Fact]
        public void SparqlResultSetToDataTable7()
        {
            SparqlResultSet rset = new SparqlResultSet(true);

            DataTable table = rset.ToDataTable();

            Assert.True(rset.ResultsType == SparqlResultsType.Boolean);
            Assert.Single(table.Columns);
            Assert.Single(table.Rows);
            Assert.True((bool)table.Rows[0]["ASK"], "Should be true");
            DumpTable(table);
        }

        [Fact]
        public void SparqlResultSetToDataTable8()
        {
            SparqlResultSet rset = new SparqlResultSet(false);

            DataTable table = rset.ToDataTable();

            Assert.True(rset.ResultsType == SparqlResultsType.Boolean);
            Assert.Single(table.Columns);
            Assert.Single(table.Rows);
            Assert.False((bool)table.Rows[0]["ASK"], "Should be false");
            DumpTable(table);
        }

        [Fact]
        public void SparqlResultSetToDataTable9()
        {
            var results = new SparqlResultSet();
            Assert.Throws<InvalidCastException>(() =>
            {
                var t = results.ToDataTable();
            });
        }
    }
}
