using System;
using System.Windows.Forms;
using VDS.RDF.Utilities.StoreManager.Properties;

namespace VDS.RDF.Utilities.StoreManager.Dialogues
{
    public partial class RestoreConnectionsDialogue : Form
    {
        public RestoreConnectionsDialogue()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (this.chkRemember.Checked)
            {
                Settings.Default.PromptRestoreActiveConnections = false;
                Settings.Default.AlwaysRestoreActiveConnections = true;
            }
            else
            {
                Settings.Default.PromptRestoreActiveConnections = true;
                Settings.Default.AlwaysRestoreActiveConnections = false;
            }

            Settings.Default.Save();
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            if (this.chkRemember.Checked)
            {
                Settings.Default.PromptRestoreActiveConnections = false;
                Settings.Default.AlwaysRestoreActiveConnections = false;
            }
            else
            {
                Settings.Default.PromptRestoreActiveConnections = true;
                Settings.Default.AlwaysRestoreActiveConnections = false;
            }
            Settings.Default.Save();
            this.DialogResult = DialogResult.No;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
