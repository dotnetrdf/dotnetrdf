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
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using VDS.RDF.Writing;

namespace VDS.RDF.GUI.WinForms.Forms
{
    /// <summary>
    /// A Form that can be used to select options for Exporting a SPARQL Result Set to a File
    /// </summary>
    public partial class ExportResultSetOptionsForm : Form
    {
        /// <summary>
        /// Creates a new Export SPARQL Results Options Form
        /// </summary>
        public ExportResultSetOptionsForm()
        {
            InitializeComponent();

            //Load Writers
            Type targetType = typeof(ISparqlResultsWriter);
            List<ISparqlResultsWriter> writers = new List<ISparqlResultsWriter>();
            foreach (Type t in Assembly.GetAssembly(targetType).GetTypes())
            {
                if (t.Namespace == null) continue;

                if (t.Namespace.Equals("VDS.RDF.Writing"))
                {
                    if (t.GetInterfaces().Contains(targetType))
                    {
                        try
                        {
                            ISparqlResultsWriter writer = (ISparqlResultsWriter)Activator.CreateInstance(t);
                            writers.Add(writer);
                        }
                        catch
                        {
                            //Ignore this Writer
                        }
                    }
                }
            }
            writers.Sort(new ToStringComparer<ISparqlResultsWriter>());
            this.cboWriter.DataSource = writers;
            if (this.cboWriter.Items.Count > 0) this.cboWriter.SelectedIndex = 0;

            this.cboWriter.SelectedIndex = 0;
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (this.txtFile.Text.Equals(String.Empty))
            {
                MessageBox.Show("You must enter a filename you wish to export the SPARQL Result Set to", "Filename Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// Gets the SPARQL Results Writer the user selected
        /// </summary>
        public ISparqlResultsWriter Writer
        {
            get
            {
                ISparqlResultsWriter writer = this.cboWriter.SelectedItem as ISparqlResultsWriter;
                if (writer == null) writer = new SparqlXmlWriter();
                return writer;
            }
        }

        /// <summary>
        /// Gets the Target Filename the user selected
        /// </summary>
        public String File
        {
            get
            {
                return this.txtFile.Text;
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            this.sfdExport.Filter = MimeTypesHelper.GetFilenameFilter(false, false, true, false, false, false);
            if (this.sfdExport.ShowDialog() == DialogResult.OK)
            {
                this.txtFile.Text = this.sfdExport.FileName;
            }
        }
    }
}
