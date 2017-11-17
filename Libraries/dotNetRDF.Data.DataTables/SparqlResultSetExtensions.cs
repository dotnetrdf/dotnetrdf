using System;
using System.Data;
using VDS.RDF.Query;

namespace VDS.RDF.Data.DataTables
{
    /// <summary>
    /// Extends <see cref="SparqlResultSet"/> with a method to retrieve the results as a <see cref="DataTable"/>
    /// </summary>
    public static class SparqlResultSetExtensions
    {
        /// <summary>
        /// Casts a SPARQL Result Set to a DataTable with all Columns typed as <see cref="INode">INode</see> (Results with unbound variables will have nulls in the appropriate columns of their <see cref="System.Data.DataRow">DataRow</see>)
        /// </summary>
        /// <param name="results">SPARQL Result Set</param>
        /// <returns></returns>
        public static DataTable ToDataTable(this SparqlResultSet results)
        {
            DataTable table = new DataTable();
            DataRow row;

            switch (results.ResultsType)
            {
                case SparqlResultsType.VariableBindings:
                    foreach (String var in results.Variables)
                    {
                        table.Columns.Add(new DataColumn(var, typeof(INode)));
                    }

                    foreach (SparqlResult r in results)
                    {
                        row = table.NewRow();

                        foreach (String var in results.Variables)
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
                    break;
                case SparqlResultsType.Boolean:
                    table.Columns.Add(new DataColumn("ASK", typeof(bool)));
                    row = table.NewRow();
                    row["ASK"] = results.Result;
                    table.Rows.Add(row);
                    break;

                case SparqlResultsType.Unknown:
                default:
                    throw new InvalidCastException("Unable to cast a SparqlResultSet to a DataTable as the ResultSet has yet to be filled with data and so has no SparqlResultsType which determines how it is cast to a DataTable");
            }

            return table;
        }
    }
}
