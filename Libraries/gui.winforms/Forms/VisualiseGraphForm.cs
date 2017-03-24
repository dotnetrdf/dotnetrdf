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
using System.IO;
using System.Windows.Forms;
using VDS.RDF.Writing;

namespace VDS.RDF.GUI.WinForms.Forms
{
    /// <summary>
    /// A Form that can be used to Visualise a Graph using GraphViz (or produce DOT output for use with GraphViz)
    /// </summary>
    public partial class VisualiseGraphForm : Form
    {
        private IGraph _g;

        /// <summary>
        /// Creates a new Form for Visualising Graphs
        /// </summary>
        /// <param name="g">Graph to visualise</param>
        public VisualiseGraphForm(IGraph g)
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
