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
    public partial class CloseConnectionDialogue : Form
    {
        public CloseConnectionDialogue()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Whether all windows should be closed
        /// </summary>
        public bool ForceClose { get; private set; }

        private void btnCloseWindow_Click(object sender, EventArgs e)
        {
            this.ForceClose = false;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCloseAll_Click(object sender, EventArgs e)
        {
            this.ForceClose = true;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
