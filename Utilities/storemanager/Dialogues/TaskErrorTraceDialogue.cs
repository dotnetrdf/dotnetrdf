/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.IO;
using VDS.RDF.Utilities.StoreManager.Forms;
using VDS.RDF.Utilities.StoreManager.Tasks;

namespace VDS.RDF.Utilities.StoreManager.Dialogues
{
    public partial class TaskErrorTraceForm<T> 
        : CrossThreadForm where T : class
    {
        public TaskErrorTraceForm(ITask<T> task, String subtitle)
        {
            InitializeComponent();

            this.Text = String.Format(this.Text, task.Name, subtitle);
            this.ShowErrorTrace(task);

            task.StateChanged += () => this.ShowErrorTrace(task);

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
