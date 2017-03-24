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
    /// A Form that can be used to select options for Exporting a Graph to a File
    /// </summary>
    public partial class ExportGraphOptionsForm : Form
    {
        /// <summary>
        /// Creates a new Export Graph Options Form
        /// </summary>
        public ExportGraphOptionsForm()
        {
            InitializeComponent();

            //Load Writers
            Type targetType = typeof(IRdfWriter);
            List<IRdfWriter> writers = new List<IRdfWriter>();
            foreach (Type t in Assembly.GetAssembly(targetType).GetTypes())
            {
                if (t.Namespace == null) continue;

                if (t.Namespace.Equals("VDS.RDF.Writing"))
                {
                    if (t.GetInterfaces().Contains(targetType))
                    {
                        try
                        {
                            IRdfWriter writer = (IRdfWriter)Activator.CreateInstance(t);
                            writers.Add(writer);
                        }
                        catch
                        {
                            //Ignore this Formatter
                        }
                    }
                }
            }
            writers.Sort(new ToStringComparer<IRdfWriter>());
            this.cboWriter.DataSource = writers;
            if (this.cboWriter.Items.Count > 0) this.cboWriter.SelectedIndex = 0;

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

        /// <summary>
        /// Gets the RDF Writer the user selected
        /// </summary>
        public IRdfWriter Writer
        {
            get
            {
                IRdfWriter writer = this.cboWriter.SelectedItem as IRdfWriter;
                if (writer == null) writer = new NTriplesWriter();

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
            this.sfdExport.Filter = MimeTypesHelper.GetFilenameFilter(true, false, false, false, false, false);
            if (this.sfdExport.ShowDialog() == DialogResult.OK)
            {
                this.txtFile.Text = this.sfdExport.FileName;
            }
        }

    }
}
