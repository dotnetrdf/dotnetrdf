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
using VDS.RDF.Writing;

namespace dotNetRDFStore
{
    public partial class fclsExportGraph : Form
    {
        public fclsExportGraph()
        {
            InitializeComponent();

            this.cboWriter.SelectedIndex = 0;
            this.cboCompression.SelectedIndex = 1;
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (this.txtFile.Text.Equals(String.Empty))
            {
                MessageBox.Show("You must enter a filename you wish to export the Graph to", "Filename Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        public IRdfWriter Writer
        {
            get
            {
                IRdfWriter writer;
                switch (this.cboWriter.SelectedIndex)
                {
                    case 0:
                        writer = new NTriplesWriter();
                        break;
                    case 1:
                        writer = new TurtleWriter();
                        break;
                    case 2:
                        writer = new CompressingTurtleWriter();
                        break;
                    case 3:
                        writer = new Notation3Writer();
                        break;
                    case 4:
                        writer = new RdfXmlTreeWriter();
                        break;
                    case 5:
                        writer = new FastRdfXmlWriter();
                        break;
                    case 6:
                        writer = new RdfJsonWriter();
                        break;
                    case 7:
                        writer = new HtmlWriter();
                        break;
                    case 8:
                        writer = new CsvWriter();
                        break;
                    case 9:
                        writer = new TsvWriter();
                        break;
                    default:
                        writer = new NTriplesWriter();
                        break;
                }

                //Configure Options on the Writer
                if (writer is IPrettyPrintingWriter)
                {
                    ((IPrettyPrintingWriter)writer).PrettyPrintMode = this.chkPrettyPrinting.Checked;
                }
                if (writer is IHighSpeedWriter)
                {
                    ((IHighSpeedWriter)writer).HighSpeedModePermitted = this.chkHighSpeed.Checked;
                }
                if (writer is ICompressingWriter)
                {
                    int c = WriterCompressionLevel.Default;
                    switch (this.cboCompression.SelectedIndex)
                    {
                        case 0:
                            c = WriterCompressionLevel.None;
                            break;
                        case 1:
                            c = WriterCompressionLevel.Default;
                            break;
                        case 2:
                            c = WriterCompressionLevel.Minimal;
                            break;
                        case 3:
                            c = WriterCompressionLevel.Medium;
                            break;
                        case 4:
                            c = WriterCompressionLevel.More;
                            break;
                        case 5:
                            c = WriterCompressionLevel.High;
                            break;
                        default:
                            c = WriterCompressionLevel.Default;
                            break;
                    }
                    ((ICompressingWriter)writer).CompressionLevel = c;
                }

                return writer;
            }
        }

        public String File
        {
            get
            {
                return this.txtFile.Text;
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            this.sfdExport.Filter = Constants.RdfFilter + "|" + Constants.NonStandardFilter;
            if (this.sfdExport.ShowDialog() == DialogResult.OK)
            {
                this.txtFile.Text = this.sfdExport.FileName;
            }
        }

    }
}
