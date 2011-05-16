using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VDS.RDF.Utilities.StoreManager.Tasks;

namespace VDS.RDF.Utilities.StoreManager
{
    public partial class fclsTaskInformation<T> : Form where T : class
    {
        public fclsTaskInformation(ITask<T> task)
        {
            InitializeComponent();
        }
    }
}
