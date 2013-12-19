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
    public partial class EntityQueryGeneratorForm : Form
    {
        public EntityQueryGeneratorForm()
        {
            InitializeComponent();
        }

        private void btnClos_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            ((StoreManagerForm) this.Tag).GenerateQueryForEntities((int)this.numValuesPerPredicateLimit.Value, (int)numColumnWords.Value);
            this.Close();
        }
    }
}
