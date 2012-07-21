/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

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
    public partial class TaskErrorTraceForm<T> 
        : CrossThreadForm where T : class
    {
        public TaskErrorTraceForm(ITask<T> task, String subtitle)
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
