using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VDS.RDF.Utilities.StoreManager.Tasks;

namespace VDS.RDF.Utilities.StoreManager
{
    public partial class fclsTaskErrorTrace<T> 
        : CrossThreadForm where T : class
    {
        public fclsTaskErrorTrace(ITask<T> task, String subtitle)
        {
            InitializeComponent();

            this.Text = String.Format(this.Text, task.Name, subtitle);
            this.ShowErrorTrace(task);

            task.StateChanged += new TaskStateChanged(delegate()
                {
                    this.ShowErrorTrace(task);
                });

            this.btnClose.Focus();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ShowErrorTrace(ITask<T> task)
        {
            StringWriter writer = new StringWriter();
            if (task.Error == null)
            {
                writer.Write("No Error(s)");
            }
            else
            {
                Exception ex = task.Error;
                while (ex != null)
                {
                    writer.WriteLine(ex.Message);
                    writer.WriteLine(ex.StackTrace);

                    if (ex.InnerException != null)
                    {
                        writer.WriteLine();
                        writer.WriteLine("Inner Exception:");
                    }
                    ex = ex.InnerException;
                }
            }
            CrossThreadSetText(this.txtErrorTrace, writer.ToString());
        }
    }
}
