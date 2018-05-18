/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2018 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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
