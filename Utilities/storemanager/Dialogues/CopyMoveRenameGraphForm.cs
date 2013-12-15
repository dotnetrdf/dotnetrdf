/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VDS.RDF.Utilities.StoreManager
{
    public partial class CopyMoveRenameGraphForm : Form
    {
        public CopyMoveRenameGraphForm(String task)
        {
            InitializeComponent();
            this.Text = String.Format(this.Text, task);
        }

        public Uri Uri
        {
            get;
            private set;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                this.Uri = new Uri(this.txtUri.Text);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (UriFormatException uriEx)
            {
                MessageBox.Show("Not a valid URI: " + uriEx.Message, "Invalid URI", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void CopyMoveRenameGraphForm_Load(object sender, EventArgs e)
        {
            this.txtUri.SelectAll();
            this.txtUri.Focus();
        }
    }
}
