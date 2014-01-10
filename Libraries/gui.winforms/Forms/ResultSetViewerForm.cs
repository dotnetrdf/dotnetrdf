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
            : this(results, null, GetTitle(results))
        {
        }

        /// <summary>
        /// Displays the given SPARQL Result Set and prefixes the Form Title with the given Title
        /// </summary>
        /// <param name="results">SPARQL Result Set to display</param>
        /// <param name="title">Title</param>
        public ResultSetViewerForm(SparqlResultSet results, String title)
            : this(results, null, GetTitle(title, results))
        {
        }

        /// <summary>
        /// Creates a new Result Set viewer form
        /// </summary>
        /// <param name="results">Result Set</param>
        /// <param name="nsmap">Namespace Map to use for display</param>
        public ResultSetViewerForm(SparqlResultSet results, INamespaceMapper nsmap, String title)
        {
            InitializeComponent();
            if (Constants.WindowIcon != null)
            {
                this.Icon = Constants.WindowIcon;
            }
            this._results = results;
            this.Text = title;
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