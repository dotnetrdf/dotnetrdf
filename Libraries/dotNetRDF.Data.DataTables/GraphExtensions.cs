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

using System.Data;

namespace VDS.RDF.Data.DataTables
{
    /// <summary>
    /// Extends <see cref="IGraph"/> with a method to retrieve the graph triples as a <see cref="DataTable"/>
    /// </summary>
    public static class GraphExtensions
    {
        /// <summary>
        /// Casts a Graph to a DataTable with all Columns typed as <see cref="INode">INode</see> (Column Names are Subject, Predicate and Object
        /// </summary>
        /// <param name="g">Graph to convert</param>
        /// <returns>
        /// A DataTable containing three Columns (Subject, Predicate and Object) all typed as <see cref="INode">INode</see> with a Row per Triple
        /// </returns>
        public static DataTable ToDataTable(this IGraph g)
        {
            DataTable table = new DataTable();
            table.Columns.Add(new DataColumn("Subject", typeof(INode)));
            table.Columns.Add(new DataColumn("Predicate", typeof(INode)));
            table.Columns.Add(new DataColumn("Object", typeof(INode)));

            foreach (Triple t in g.Triples)
            {
                DataRow row = table.NewRow();
                row["Subject"] = t.Subject;
                row["Predicate"] = t.Predicate;
                row["Object"] = t.Object;
                table.Rows.Add(row);
            }

            return table;
        }
    }
}
