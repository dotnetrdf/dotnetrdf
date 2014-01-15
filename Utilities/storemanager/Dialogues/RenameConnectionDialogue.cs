using System;
using System.Windows.Forms;
using VDS.RDF.Utilities.StoreManager.Connections;

namespace VDS.RDF.Utilities.StoreManager.Dialogues
{
    public partial class RenameConnectionDialogue : Form
    {
        public RenameConnectionDialogue(Connection connection)
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private Connection Connection { get; set; }

        private void btnRename_Click(object sender, EventArgs e)
        {
            this.Connection.Name = String.IsNullOrEmpty(this.txtName.Text) ? null : this.txtName.Text;
            this.Close();
        }
    }
}
