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
