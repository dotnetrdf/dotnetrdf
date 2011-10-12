using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VDS.RDF.Storage;
using VDS.RDF.Utilities.StoreManager.Tasks;

namespace VDS.RDF.Utilities.StoreManager
{
    partial class CopyMoveDialogue : Form
    {
        public CopyMoveDialogue(CopyMoveDragInfo info, IGenericIOManager target)
        {
            InitializeComponent();

            this.lblConfirm.Text = String.Format(this.lblConfirm.Text, info.SourceUri, info.Source.ToString(), target.ToString());
        }

        public bool IsCopy
        {
            get;
            private set;
        }

        public bool IsMove
        {
            get;
            private set;
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            this.IsCopy = true;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnMove_Click(object sender, EventArgs e)
        {
            this.IsMove = true;
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
