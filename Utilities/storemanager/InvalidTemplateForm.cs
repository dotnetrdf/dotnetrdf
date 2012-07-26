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
    public partial class InvalidTemplateForm : Form
    {
        public InvalidTemplateForm(List<String> errors)
        {
            InitializeComponent();

            this.lstErrors.DataSource = errors;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
