using System;
using System.Data;
using VDS.RDF;
using VDS.RDF.Data.DataTables;
using Xunit;

namespace dotNetRDF.Data.DataTables.Tests
{
    public class GraphConversionTests
    {
        [Fact]
        public void GraphToDataTable()
        {
            Graph g = new Graph();
            g.LoadFromFile("resources\\InferenceTest.ttl");

            DataTable table = g.ToDataTable();

            Assert.Equal(g.Triples.Count, table.Rows.Count);
            Assert.Equal(3, table.Columns.Count);

            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn col in table.Columns)
                {
                    Console.Write(col.ColumnName + " = " + row[col].ToString() + " , ");
                }
                Console.WriteLine();
            }
        }

        [Fact]
        public void GraphToDataTable2()
        {
            Graph g = new Graph();

            DataTable table = g.ToDataTable();

            Assert.Equal(g.Triples.Count, table.Rows.Count);
            Assert.Equal(3, table.Columns.Count);

            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn col in table.Columns)
                {
                    Console.Write(col.ColumnName + " = " + row[col].ToString() + " , ");
                }
                Console.WriteLine();
            }
        }
    }
}
