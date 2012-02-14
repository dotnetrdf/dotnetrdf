/*

Copyright Robert Vesse 2009-12
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

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

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
