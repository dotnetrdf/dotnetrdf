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
using System.Windows.Forms;
using VDS.RDF.Query;

namespace VDS.RDF.GUI.WinForms.Forms
{
    /// <summary>
    /// A Form that displays a SPARQL Result Set using a DataGridView
    /// </summary>
    public partial class ResultSetViewerForm : Form
    {
        private readonly SparqlResultSet _results;
        private readonly INamespaceMapper _nsmap;

        /// <summary>
        /// Displays the given SPARQL Result Set
        /// </summary>
        /// <param name="results">SPARQL Result to display</param>
        public ResultSetViewerForm(SparqlResultSet results)
            : this(results, null, null) { }

        /// <summary>
        /// Displays the given SPARQL Result Set and prefixes the form title with the given title
        /// </summary>
        /// <param name="results">SPARQL Result Set to display</param>
        /// <param name="title">Title prefix</param>
        public ResultSetViewerForm(SparqlResultSet results, String title)
            : this(results, null, title) { }

        /// <summary>
        /// Creates a new Result Set viewer form
        /// </summary>
        /// <param name="results">Result Set</param>
        /// <param name="nsmap">Namespace Map to use for display</param>
        /// <param name="title">Title prefix</param>
        public ResultSetViewerForm(SparqlResultSet results, INamespaceMapper nsmap, String title)
        {
            InitializeComponent();
            if (Constants.WindowIcon != null)
            {
                this.Icon = Constants.WindowIcon;
            }
            this._results = results;
            this.Text = GetTitle(title, results);
            this._nsmap = nsmap;

            this.resultsViewer.UriClicked += (sender, uri) => this.RaiseUriClicked(uri);
            this.Load += (sender, args) => this.resultsViewer.DisplayResultSet(this._results, this._nsmap);
        }

        private static String GetTitle(SparqlResultSet results)
        {
            if (results.ResultsType == SparqlResultsType.Boolean)
                return "SPARQL Results Viewer - Boolean Result";
            return results.ResultsType == SparqlResultsType.VariableBindings ? String.Format("SPARQL Results Viewer - {0} Result(s)", results.Count) : "SPARQL Results Viewer";
        }

        private static String GetTitle(String title, SparqlResultSet results)
        {
            if (title == null) return GetTitle(results);
            return String.Format("{0} - " + GetTitle(results), title);
        }

        /// <summary>
        /// Event which is raised when the User clicks a URI
        /// </summary>
        public event UriClickedEventHandler UriClicked;

        private void RaiseUriClicked(Uri u)
        {
            UriClickedEventHandler d = this.UriClicked;
            if (d != null)
            {
                d(this, u);
            }
        }
    }
}