/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.Data;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Data.DataTables
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
                            DefaultObjectColumn = "Object";

        /// <summary>
        /// Data Table into which Triples will be converted to rows
        /// </summary>
        protected DataTable _table;
        private String _subjCol, _predCol, _objCol;

        /// <summary>
        /// Creates a new Handler for a given Data Table with custom column names
        /// </summary>
        /// <param name="table">Data Table</param>
        /// <param name="subjColName">Subject Column Name</param>
        /// <param name="predColName">Predicate Column Name</param>
        /// <param name="objColName">Object Column Name</param>
        public DataTableHandler(DataTable table, String subjColName, String predColName, String objColName)
        {
            this._table = table;
            this._subjCol = subjColName;
            this._predCol = predColName;
            this._objCol = objColName;            
        }

        /// <summary>
        /// Creates a new Handler for a given Data Table using the default column names
        /// </summary>
        /// <param name="table">Data Table</param>
        public DataTableHandler(DataTable table)
            : this(table, DefaultSubjectColumn, DefaultPredicateColumn, DefaultObjectColumn) { }

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
