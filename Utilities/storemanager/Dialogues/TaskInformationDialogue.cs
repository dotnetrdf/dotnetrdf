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
using VDS.RDF.GUI.WinForms.Forms;
using VDS.RDF.Query;
using VDS.RDF.Utilities.StoreManager.Forms;
using VDS.RDF.Utilities.StoreManager.Properties;
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
            this.Text = string.Format(Resources.TaskForm_Title, task.Name, subtitle);
            this.btnErrorTrace.Click += delegate
                {
                    TaskErrorTraceForm<T> errorTrace = new TaskErrorTraceForm<T>(task, subtitle);
                    errorTrace.MdiParent = this.MdiParent;
                    errorTrace.Show();
                };
            this.ShowInformation(task);

            task.StateChanged += () => this.ShowInformation(task);

            if (task is CancellableTask<T>)
            {
                this.btnCancel.Click += (sender, e) => ((CancellableTask<T>) task).Cancel();
            }
        }

        public TaskInformationForm(QueryTask task, String subtitle)
            : this((ITask<T>)task, subtitle)
        {
            this.ShowQueryInformation(task);
            if (task.Query == null)
            {
                task.StateChanged += () => this.ShowQueryInformation(task);
            }
            this.btnViewResults.Click += delegate
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
                            INamespaceMapper nsmap = task.Query.NamespaceMap;
                            ResultSetViewerForm resultsViewer = new ResultSetViewerForm((SparqlResultSet)task.Result, nsmap, subtitle);
                            resultsViewer.MdiParent = this.MdiParent;
                            resultsViewer.Show();
                        }
                        else
                        {
                            MessageBox.Show(Resources.QueryResults_NotViewable_Text, Resources.QueryResults_NotViewable_Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show(Resources.QueryResults_Unavailable_Text, Resources.QueryResults_Unavailable_Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };
        }

        public TaskInformationForm(UpdateTask task, String subtitle)
            : this((ITask<T>)task, subtitle)
        {
            this.ShowUpdateInformation(task);
            if (task.Updates == null)
            {
                task.StateChanged += () => this.ShowUpdateInformation(task);
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
            this.btnViewResults.Click += delegate
                {
                    if (task.Result != null)
                    {
                        GraphViewerForm graphViewer = new GraphViewerForm(task.Result, subtitle);
                        graphViewer.MdiParent = this.MdiParent;
                        graphViewer.Show();
                    }
                    else
                    {
                        MessageBox.Show(Resources.Graph_Unavailable_Text, Resources.Graph_Unavailable_Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };
        }

        public TaskInformationForm(ITask<TaskValueResult<bool>> task, String subtitle)
            : this((ITask<T>)task, subtitle)
        {

        }

        public TaskInformationForm(GetStoreTask task, String subtitle)
            : this((ITask<T>)task, subtitle)
        {

        }

        public TaskInformationForm(GenerateEntitiesQueryTask task, String subtitle)
            : this((ITask<T>) task, subtitle)
        {
            this.txtAdvInfo.Text = "Original Query String:" + Environment.NewLine + task.OriginalQueryString;
            this.btnViewResults.Click += delegate
                {
                    StringResultDialogue dialogue = new StringResultDialogue(string.Format("Generated Entity Query on {0}", subtitle), task.Result);
                    CrossThreadSetMdiParent(dialogue);
                    CrossThreadShow(dialogue);
                };
            this.btnViewResults.Enabled = task.State == TaskState.Completed;
        }

        private void ShowInformation(ITaskBase task)
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
            if (task.Query == null) return;
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

        private void ShowUpdateInformation(UpdateTask task)
        {
            if (task.Updates == null) return;
            StringWriter writer = new StringWriter();
            writer.WriteLine("Parsed Updates:");
            writer.WriteLine(task.Updates.ToString());
            CrossThreadSetText(this.txtAdvInfo, writer.ToString());
        }
    }
}
