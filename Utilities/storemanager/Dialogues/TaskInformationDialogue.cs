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
using System.Windows.Forms;
using VDS.RDF.Query;
using VDS.RDF.Writing.Formatting;
using VDS.RDF.Utilities.StoreManager.Tasks;
using VDS.RDF.GUI.WinForms;

namespace VDS.RDF.Utilities.StoreManager.Dialogues
{
    /// <summary>
    /// Form for viewing Task Information
    /// </summary>
    /// <typeparam name="T">Task Result Type</typeparam>
    public partial class TaskInformationForm<T> 
        : CrossThreadForm where T : class
    {
        public TaskInformationForm()
        {
            InitializeComponent();
        }

        public TaskInformationForm(ITask<T> task, String subtitle)
            : this()
        {
            this.Text = task.Name + " on " + subtitle;
            this.btnErrorTrace.Click += new EventHandler(delegate(Object sender, EventArgs e)
                {
                    TaskErrorTraceForm<T> errorTrace = new TaskErrorTraceForm<T>(task, subtitle);
                    errorTrace.MdiParent = this.MdiParent;
                    errorTrace.Show();
                });
            this.ShowInformation(task);

            task.StateChanged += new TaskStateChanged(delegate()
                {
                    this.ShowInformation(task);
                });

            if (task is CancellableTask<T>)
            {
                this.btnCancel.Click += new EventHandler(delegate(Object sender, EventArgs e)
                    {
                        ((CancellableTask<T>)task).Cancel();
                    });
            }
        }

        public TaskInformationForm(QueryTask task, String subtitle)
            : this((ITask<T>)task, subtitle)
        {
            this.ShowQueryInformation(task);
            if (task.Query == null)
            {
                task.StateChanged += new TaskStateChanged(delegate()
                    {
                        this.ShowQueryInformation(task);
                    });
            }
            this.btnViewResults.Click += new EventHandler(delegate(Object sender, EventArgs e)
                {
                    if (task.Result != null)
                    {
                        if (task.Result is IGraph)
                        {
                            GraphViewerForm graphViewer = new GraphViewerForm((IGraph)task.Result, subtitle);
                            graphViewer.MdiParent = this.MdiParent;
                            graphViewer.Show();
                        }
                        else if (task.Result is SparqlResultSet)
                        {
                            INamespaceMapper nsmap = (task is QueryTask ? ((QueryTask)task).Query.NamespaceMap : null);
                            ResultSetViewerForm resultsViewer = new ResultSetViewerForm((SparqlResultSet)task.Result, subtitle, nsmap);
                            resultsViewer.MdiParent = this.MdiParent;
                            resultsViewer.Show();
                        }
                        else
                        {
                            MessageBox.Show("Unable to show Query Results as did not get a Graph/Result Set as expected", "Unable to Show Results", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Query Results are not available", "Results Unavailable", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                });
        }

        public TaskInformationForm(UpdateTask task, String subtitle)
            : this((ITask<T>)task, subtitle)
        {
            this.ShowUpdateInformation(task);
            if (task.Updates == null)
            {
                task.StateChanged += new TaskStateChanged(delegate()
                    {
                        this.ShowUpdateInformation(task);
                    });
            }
        }

        public TaskInformationForm(ListGraphsTask task, String subtitle)
            : this((ITask<T>)task, subtitle)
        {

        }

        public TaskInformationForm(ListStoresTask task, String subtitle)
            : this((ITask<T>)task, subtitle)
        {

        }

        public TaskInformationForm(ITask<IGraph> task, String subtitle)
            : this((ITask<T>)task, subtitle)
        {
            CrossThreadSetEnabled(this.btnViewResults, task.State == TaskState.Completed && task.Result != null);
            this.btnViewResults.Click += new EventHandler(delegate(Object sender, EventArgs e)
                {
                    if (task.Result != null)
                    {
                        if (task.Result is IGraph)
                        {
                            GraphViewerForm graphViewer = new GraphViewerForm((IGraph)task.Result, subtitle);
                            graphViewer.MdiParent = this.MdiParent;
                            graphViewer.Show();
                        }
                        else
                        {
                            MessageBox.Show("Unable to show Results as did not get a Graph as expected", "Unable to Show Results", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Results are not available", "Results Unavailable", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                });
        }

        public TaskInformationForm(ITask<TaskValueResult<bool>> task, String subtitle)
            : this((ITask<T>)task, subtitle)
        {

        }

        public TaskInformationForm(GetStoreTask task, String subtitle)
            : this((ITask<T>)task, subtitle)
        {

        }

        private void ShowInformation(ITask<T> task)
        {
            CrossThreadSetText(this.lblTaskState, String.Format("Task State: {0}", task.State.GetStateDescription()));
            CrossThreadSetText(this.txtBasicInfo, task.Information);
            CrossThreadSetText(this.lblElapsed, (task.Elapsed != null) ? String.Format("Time Elapsed: {0}", task.Elapsed) : "N/A");
            CrossThreadSetEnabled(this.btnCancel,task.IsCancellable && task.State != TaskState.Completed && task.State != TaskState.CompletedWithErrors);
            CrossThreadSetEnabled(this.btnErrorTrace, task.Error != null);
            CrossThreadSetEnabled(this.btnViewResults, false);
        }

        private void ShowQueryInformation(QueryTask task)
        {
            CrossThreadSetEnabled(this.btnViewResults, task.State == TaskState.Completed && task.Result != null);
            if (task.Query != null)
            {
                StringWriter writer = new StringWriter();
                writer.WriteLine("Parsed Query:");
                if (task.Query != null)
                {
                    SparqlFormatter formatter = new SparqlFormatter(task.Query.NamespaceMap);
                    writer.WriteLine(formatter.Format(task.Query));
                    writer.WriteLine("SPARQL Algebra:");
                    writer.WriteLine(task.Query.ToAlgebra().ToString());
                }
                else
                {
                    writer.WriteLine("Unavailable - Not standard SPARQL 1.0/1.1");
                }
                CrossThreadSetText(this.txtAdvInfo, writer.ToString());
            }
        }

        private void ShowUpdateInformation(UpdateTask task)
        {
            if (task.Updates != null)
            {
                StringWriter writer = new StringWriter();
                writer.WriteLine("Parsed Updates:");
                writer.WriteLine(task.Updates.ToString());
                CrossThreadSetText(this.txtAdvInfo, writer.ToString());
            }
        }
    }
}
