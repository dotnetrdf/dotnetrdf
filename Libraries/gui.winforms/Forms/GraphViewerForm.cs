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

namespace VDS.RDF.GUI.WinForms.Forms
{
    /// <summary>
    /// A Form that displays a Graph using a DataGridView
    /// </summary>
    public partial class GraphViewerForm : Form
    {
        /// <summary>
        /// Displays the given Graph
        /// </summary>
        /// <param name="g">Graph to display</param>
        public GraphViewerForm(IGraph g)
        {
            InitializeComponent();
            if (Constants.WindowIcon != null)
            {
                this.Icon = Constants.WindowIcon;
            }
            this.Text = String.Format("Graph Viewer - {0} Triple(s)", g.Triples.Count);

            this.graphViewer.UriClicked += (sender, uri) => this.RaiseUriClicked(uri);
            this.Load += (sender, args) => this.graphViewer.DisplayGraph(g);
        }

        /// <summary>
        /// Displays the given Graph and prefixes the Form Title with the given Title
        /// </summary>
        /// <param name="g">Graph to display</param>
        /// <param name="title">Title</param>
        public GraphViewerForm(IGraph g, String title)
            : this(g)
        {
            this.Text = String.Format("{0} - Graph Viewer - {1} Triple(s)", title, g.Triples.Count);
        }

        /// <summary>
        /// Event which is raised when a User clicks a URI
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