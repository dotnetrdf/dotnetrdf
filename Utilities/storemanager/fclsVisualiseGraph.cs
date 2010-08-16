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
using System.IO;
using System.Windows.Forms;
using VDS.RDF;
using VDS.RDF.Writing;

namespace dotNetRDFStore
{
    public partial class fclsVisualiseGraph : Form
    {
        private IGraph _g;

        public fclsVisualiseGraph(IGraph g)
        {
            InitializeComponent();

            this._g = g;
        }

        private void btnVisualise_Click(object sender, EventArgs e)
        {
            if (this.txtFile.Text.Equals(String.Empty))
            {
                MessageBox.Show("You must enter a filename you wish to visualise the Graph to", "Filename Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                try
                {
                    switch (Path.GetExtension(this.txtFile.Text))
                    {
                        case ".dot":
                        case "dot":
                            GraphVizWriter dotwriter = new GraphVizWriter();
                            dotwriter.Save(this._g, this.txtFile.Text);
                            break;
                        case ".png":
                        case "png":
                            GraphVizGenerator pnggenerator = new GraphVizGenerator("png");
                            pnggenerator.Generate(this._g, this.txtFile.Text, true);
                            break;
                        case ".svg":
                        case "svg":
                        default:
                            GraphVizGenerator svggenerator = new GraphVizGenerator("svg");
                            svggenerator.Generate(this._g, this.txtFile.Text, true);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An Error occurred while trying to visualise the selected Graph:\n" + ex.Message, "Visualisation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (this.sfdVisualise.ShowDialog() == DialogResult.OK)
            {
                this.txtFile.Text = this.sfdVisualise.FileName;
            }
        }

    }
}
