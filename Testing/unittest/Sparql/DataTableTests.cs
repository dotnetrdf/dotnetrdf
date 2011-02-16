using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class DataTableTests
    {
        [TestMethod]
        public void SparqlResultSetToDataTable()
        {
            String query = "SELECT * WHERE {?s ?p ?o}";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");

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

                Assert.AreEqual(rset.Variables.Count(), table.Columns.Count, "Number of Columns should be equal");
                Assert.AreEqual(rset.Count, table.Rows.Count, "Number of Rows should be equal");

                foreach (DataRow row in table.Rows)
                {
                    foreach (DataColumn col in table.Columns)
                    {
                        Object temp = row[col];
                        Console.Write(col.ColumnName + " = " + ((temp != null) ? temp.ToString() : String.Empty) + " , ");
                    }
                    Console.WriteLine();
                }
            }
            else
            {
                Assert.Fail("Query should have returned a Result Set");
            }
        }

        [TestMethod]
        public void SparqlResultSetToDataTable2()
        {
            String query = "PREFIX ex: <http://example.org/vehicles/> SELECT * WHERE {?s a ex:Car . OPTIONAL { ?s ex:Speed ?speed }}";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");

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

                Assert.AreEqual(rset.Variables.Count(), table.Columns.Count, "Number of Columns should be equal");
                Assert.AreEqual(rset.Count, table.Rows.Count, "Number of Rows should be equal");

                foreach (DataRow row in table.Rows)
                {
                    foreach (DataColumn col in table.Columns)
                    {
                        Object temp = row[col];
                        Console.Write(col.ColumnName + " = " + ((temp != null) ? temp.ToString() : String.Empty) + " , ");
                    }
                    Console.WriteLine();
                }
            }
            else
            {
                Assert.Fail("Query should have returned a Result Set");
            }
        }

        [TestMethod]
        public void SparqlResultSetToDataTable3()
        {
            String query = "SELECT * WHERE {?s ?p ?o}";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");

            Object results = g.ExecuteQuery(q);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;

                DataTable table = (DataTable)rset;

                Assert.AreEqual(rset.Variables.Count(), table.Columns.Count, "Number of Columns should be equal");
                Assert.AreEqual(rset.Count, table.Rows.Count, "Number of Rows should be equal");

                foreach (DataRow row in table.Rows)
                {
                    foreach (DataColumn col in table.Columns)
                    {
                        Object temp = row[col];
                        Console.Write(col.ColumnName + " = " + ((temp != null) ? temp.ToString() : String.Empty) + " , ");
                    }
                    Console.WriteLine();
                }
            }
            else
            {
                Assert.Fail("Query should have returned a Result Set");
            }
        }

        [TestMethod]
        public void SparqlResultSetToDataTable4()
        {
            String query = "PREFIX ex: <http://example.org/vehicles/> SELECT * WHERE {?s a ex:Car . OPTIONAL { ?s ex:Speed ?speed }}";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");

            Object results = g.ExecuteQuery(q);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;

                DataTable table = (DataTable)rset;

                Assert.AreEqual(rset.Variables.Count(), table.Columns.Count, "Number of Columns should be equal");
                Assert.AreEqual(rset.Count, table.Rows.Count, "Number of Rows should be equal");

                foreach (DataRow row in table.Rows)
                {
                    foreach (DataColumn col in table.Columns)
                    {
                        Object temp = row[col];
                        Console.Write(col.ColumnName + " = " + ((temp != null) ? temp.ToString() : String.Empty) + " , ");
                    }
                    Console.WriteLine();
                }
            }
            else
            {
                Assert.Fail("Query should have returned a Result Set");
            }
        }
    }
}
