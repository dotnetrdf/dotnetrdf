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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using VDS.RDF.GUI.WinForms.Forms;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.GUI.WinForms.Controls
{
    /// <summary>
    /// A Form that displays a Graph using a DataGridView
    /// </summary>
    public partial class GraphViewerControl : UserControl
    {
        private IGraph _g;
        private Formatter _lastFormatter;
        private INodeFormatter _formatter = new TurtleFormatter();

        /// <summary>
        /// Creates a new control
        /// </summary>
        public GraphViewerControl()
        {
             InitializeComponent();
             this.fmtSelector.DefaultFormatter = typeof(TurtleFormatter);
             this.fmtSelector.FormatterChanged += fmtSelector_FormatterChanged;
        }

        private void fmtSelector_FormatterChanged(object sender, Formatter formatter)
        {
            if (ReferenceEquals(formatter, this._lastFormatter)) return;
            this._lastFormatter = formatter;
            this._formatter = formatter.CreateInstance(this._g.NamespaceMap);

            if (this.dgvTriples.DataSource == null) return;
            DataTable tbl = (DataTable)this.dgvTriples.DataSource;
            this.dgvTriples.DataSource = null;
            this.dgvTriples.Refresh();
            this.dgvTriples.DataSource = tbl;
        }

        /// <summary>
        /// Displays the given Graph
        /// </summary>
        /// <param name="g">Graph to display</param>
        public void DisplayGraph(IGraph g)
        {
            this.dgvTriples.CellFormatting += dgvTriples_CellFormatting;
            this.dgvTriples.CellContentClick += dgvTriples_CellClick;

            if (g.BaseUri != null)
            {
                this.lnkBaseURI.Text = g.BaseUri.ToString();
            }

            this._g = g;

            this.Text = String.Format("Graph Viewer - {0} Triple(s)", g.Triples.Count);

            this.LoadInternal();
        }

        /// <summary>
        /// Event which is raised when a User clicks a URI
        /// </summary>
        public event UriClickedEventHandler UriClicked;

        #region Internal Implementation

        void dgvTriples_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value is INode)
            {
                INode n = (INode)e.Value;

                TripleSegment? segment = null;
                switch (e.ColumnIndex)
                {
                    case 0:
                        segment = TripleSegment.Subject;
                        break;
                    case 1:
                        segment = TripleSegment.Predicate;
                        break;
                    case 2:
                        segment = TripleSegment.Object;
                        break;
                }

                e.Value = this._formatter.Format(n, segment);
                e.FormattingApplied = true;
                switch (n.NodeType)
                {
                    case NodeType.Uri:
                        e.CellStyle.Font = new System.Drawing.Font(e.CellStyle.Font, System.Drawing.FontStyle.Underline);
                        e.CellStyle.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0, 255);
                        break;
                }
            }
        }

        void dgvTriples_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            Object value = this.dgvTriples[e.ColumnIndex, e.RowIndex].Value;
            if (value != null)
            {
                if (value is INode)
                {
                    INode n = (INode)value;
                    if (n.NodeType == NodeType.Uri)
                    {
                        this.RaiseUriClicked(((IUriNode)n).Uri);
                    }
                }
            }
        }

        private void LoadInternal()
        {
            //Show Graph Uri
            this.lnkBaseURI.Text = this._g.BaseUri != null ? this._g.BaseUri.ToString() : "null";

            //Show Triples
            this.dgvTriples.DataSource = this._g.ToDataTable();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            ExportGraphOptionsForm exporter = new ExportGraphOptionsForm();
            if (exporter.ShowDialog() != DialogResult.OK) return;
            IRdfWriter writer = exporter.Writer;
            String file = exporter.File;

            try
            {
                writer.Save(this._g, file);

                MessageBox.Show("Successfully exported the Graph to the file '" + file + "'", "Graph Export Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred attempting to export the Graph:\n" + ex.Message, "Graph Export Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnVisualise_Click(object sender, EventArgs e)
        {
            VisualiseGraphForm visualiser = new VisualiseGraphForm(this._g);
            visualiser.ShowDialog();
        }

        private void RaiseUriClicked(Uri u)
        {
            UriClickedEventHandler d = this.UriClicked;
            if (d != null)
            {
                d(this, u);
            }
        }

        #endregion
    }
}
