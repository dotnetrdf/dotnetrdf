using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace VDS.RDF.Utilities.StoreManager
{
    public partial class fclsAbout : Form
    {
        public fclsAbout()
        {
            InitializeComponent();
            this.lblAppVersionActual.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.lblApiVersionActual.Text = Assembly.GetAssembly(typeof(IGraph)).GetName().Version.ToString();
        }
    }
}
