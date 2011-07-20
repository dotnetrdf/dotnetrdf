/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

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

If this license is not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using VDS.RDF;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.GUI.WinForms
{
    /// <summary>
    /// A Form that displays a Graph using a DataGridView
    /// </summary>
    public partial class GraphViewerForm : Form
    {
        private IGraph _g;
        private INodeFormatter _formatter = new NTriplesFormatter();

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

            //Load Formatters
            List<INodeFormatter> formatters = new List<INodeFormatter>();
            Type targetType = typeof(INodeFormatter);
            foreach (Type t in Assembly.GetAssembly(targetType).GetTypes())
            {
                if (t.Namespace == null) continue;

                if (t.Namespace.Equals("VDS.RDF.Writing.Formatting"))
                {
                    if (t.GetInterfaces().Contains(targetType))
                    {
                        try
                        {
                            INodeFormatter formatter = (INodeFormatter)Activator.CreateInstance(t);
                            formatters.Add(formatter);
                            if (formatter.GetType().Equals(this._formatter.GetType())) this._formatter = formatter;
                        }
                        catch
                        {
                            //Ignore this Formatter
                        }
                    }
                }
            }
            formatters.Sort(new ToStringComparer<INodeFormatter>());
            this.cboFormat.DataSource = formatters;
            this.cboFormat.SelectedItem = this._formatter;
            this.cboFormat.SelectedIndexChanged += new System.EventHandler(this.cboFormat_SelectedIndexChanged);

            this.dgvTriples.CellFormatting += new DataGridViewCellFormattingEventHandler(dgvTriples_CellFormatting);
            this.dgvTriples.CellContentClick += new DataGridViewCellEventHandler(dgvTriples_CellClick);

            if (g.BaseUri != null)
            {
                this.lnkBaseURI.Text = g.BaseUri.ToString();
            }

            this._g = g;

            this.Text = String.Format("Graph Viewer - {0} Triple(s)", g.Triples.Count);
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

        private void GraphViewerForm_Load(object sender, System.EventArgs e)
        {
            //Load Graph and Populate Form Fields
            this.LoadInternal();
        }

        private void LoadInternal()
        {
            //Show Graph Uri
            if (this._g.BaseUri != null)
            {
                this.lnkBaseURI.Text = this._g.BaseUri.ToString();
            }
            else
            {
                this.lnkBaseURI.Text = "null";
            }

            //Show Triples
            this.dgvTriples.DataSource = this._g.ToDataTable();
        }

        private void cboFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.cboFormat.SelectedItem != null)
            {
                INodeFormatter formatter = this.cboFormat.SelectedItem as INodeFormatter;
                if (formatter != null)
                {
                    INodeFormatter currFormatter = this._formatter;

                    Type t = formatter.GetType();
                    Type graphType = typeof(IGraph);
                    if (t.GetConstructors().Any(c => c.IsPublic && c.GetParameters().Any() && c.GetParameters().All(p => p.ParameterType.Equals(graphType))))
                    {
                        try
                        {
                            this._formatter = (INodeFormatter)Activator.CreateInstance(t, new object[] { this._g });
                        }
                        catch
                        {
                            this._formatter = formatter;
                        }
                    }
                    else
                    {
                        this._formatter = formatter;
                    }

                    if (!ReferenceEquals(currFormatter, this._formatter) || !currFormatter.GetType().Equals(this._formatter.GetType()))
                    {
                        DataTable tbl = (DataTable)this.dgvTriples.DataSource;
                        this.dgvTriples.DataSource = null;
                        this.dgvTriples.Refresh();
                        this.dgvTriples.DataSource = tbl;
                    }
                }
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            ExportGraphOptionsForm exporter = new ExportGraphOptionsForm();
            if (exporter.ShowDialog() == DialogResult.OK)
            {
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
