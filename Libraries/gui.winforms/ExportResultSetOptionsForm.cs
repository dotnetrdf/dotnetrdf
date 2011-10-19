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
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using VDS.RDF;
using VDS.RDF.Writing;

namespace VDS.RDF.GUI.WinForms
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
