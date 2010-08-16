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
using System.Windows.Forms;
using VDS.RDF;

namespace dotNetRDFStore
{
    public partial class fclsGraphViewer : Form
    {
        private IGraph _g;

        public fclsGraphViewer(IGraph g)
        {
            InitializeComponent();

            this._g = g;
        }

        private void fclsGraphManager_Load(object sender, System.EventArgs e)
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
            this.lvwTriples.BeginUpdate();
            this.lvwTriples.Items.Clear();
            ListViewItem item;
            String[] triple;
            foreach (Triple t in this._g.Triples)
            {
                triple = new String[] { t.Subject.ToString(), t.Predicate.ToString(), t.Object.ToString() };
                item = new ListViewItem(triple);
                this.lvwTriples.Items.Add(item);
            }
            this.lvwTriples.EndUpdate();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            fclsExportGraph exporter = new fclsExportGraph();
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
            fclsVisualiseGraph visualiser = new fclsVisualiseGraph(this._g);
            visualiser.ShowDialog();
        }
    }
}
