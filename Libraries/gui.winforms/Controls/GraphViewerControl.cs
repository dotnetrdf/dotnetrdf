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
             this.dgvTriples.CellFormatting += dgvTriples_CellFormatting;
             this.dgvTriples.CellContentClick += dgvTriples_CellClick;
             this.fmtSelector.DefaultFormatter = typeof(TurtleFormatter);
             this.fmtSelector.FormatterChanged += fmtSelector_FormatterChanged;
        }

        private void fmtSelector_FormatterChanged(object sender, Formatter formatter)
        {
            if (ReferenceEquals(formatter, this._lastFormatter)) return;
            this._lastFormatter = formatter;
            this.Reformat();
        }

        private void Reformat()
        {
            if (ReferenceEquals(this._lastFormatter, null)) return;
            this._formatter = this._lastFormatter.CreateInstance(this._g.NamespaceMap);

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
            this.DisplayGraph(g, g.NamespaceMap);
        }

        /// <summary>
        /// Displays the given Graph
        /// </summary>
        /// <param name="g">Graph to display</param>
        /// <param name="namespaces">Namespaces to use which may be different than those attached to the actual graph</param>
        public void DisplayGraph(IGraph g, INamespaceMapper namespaces)
        {
            this.dgvTriples.DataSource = null;
            this.dgvTriples.Refresh();

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
            if (!(e.Value is INode)) return;
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

        void dgvTriples_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            Object value = this.dgvTriples[e.ColumnIndex, e.RowIndex].Value;
            if (value == null) return;
            if (!(value is INode)) return;
            INode n = (INode)value;
            if (n.NodeType == NodeType.Uri)
            {
                this.RaiseUriClicked(((IUriNode)n).Uri);
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
