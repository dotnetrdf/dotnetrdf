/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

#if !NO_DATA

using System;
using System.Data;

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// RDF Handler which turns triples into rows in a Data Table
    /// </summary>
    public class DataTableHandler
        : BaseRdfHandler
    {
        /// <summary>
        /// Constants for Default Column Names
        /// </summary>
        public const String DefaultSubjectColumn = "Subject",
                            DefaultPredicateColumn = "Predicate",
                            DefaultObjectColumn = "Object",
                            DefaultGraphColumn = "Graph";

        /// <summary>
        /// Data Table into which Triples will be converted to rows
        /// </summary>
        protected DataTable _table;
        private String _subjCol, _predCol, _objCol, _graphCol;

        /// <summary>
        /// Creates a new Handler for a given Data Table with custom column names
        /// </summary>
        /// <param name="table">Data Table</param>
        /// <param name="subjColName">Subject Column Name</param>
        /// <param name="predColName">Predicate Column Name</param>
        /// <param name="objColName">Object Column Name</param>
        /// <param name="graphColName">Graph Column Name</param>
        public DataTableHandler(DataTable table, String subjColName, String predColName, String objColName, String graphColName)
        {
            this._table = table;
            this._subjCol = subjColName;
            this._predCol = predColName;
            this._objCol = objColName;
            this._graphCol = graphColName;
        }

        /// <summary>
        /// Creates a new Handler for a given Data Table using the default column names
        /// </summary>
        /// <param name="table">Data Table</param>
        public DataTableHandler(DataTable table)
            : this(table, DefaultSubjectColumn, DefaultPredicateColumn, DefaultObjectColumn, DefaultGraphColumn) { }

        /// <summary>
        /// Handles a Triple by turning it into a row in the Data Table
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        /// <remarks>
        /// To customize how a Triple is converted into a row in the table derive from this class and override this method
        /// </remarks>
        protected override bool HandleTripleInternal(Triple t)
        {
            DataRow row = this._table.NewRow();
            row[this._subjCol] = t.Subject;
            row[this._predCol] = t.Predicate;
            row[this._objCol] = t.Object;
            row[this._graphCol] = null;
            this._table.Rows.Add(row);
            return true;
        }

        protected override bool HandleQuadInternal(Quad q)
        {
            DataRow row = this._table.NewRow();
            row[this._subjCol] = q.Subject;
            row[this._predCol] = q.Predicate;
            row[this._objCol] = q.Object;
            row[this._graphCol] = q.Graph;
            this._table.Rows.Add(row);
            return true;
        }

        /// <summary>
        /// Indicates that the Handler accepts all triples
        /// </summary>
        public override bool AcceptsAll
        {
            get 
            {
                return true;
            }
        }
    }
}

#endif
