using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VDS.RDF.Query;
using VDS.RDF.Writing.Formatting;
using VDS.RDF.Utilities.StoreManager.Tasks;
using VDS.RDF.GUI.WinForms;

namespace VDS.RDF.Utilities.StoreManager
{
    public partial class fclsTaskInformation<T> : CrossThreadForm where T : class
    {
        public fclsTaskInformation()
        {
            InitializeComponent();
        }

        public fclsTaskInformation(ITask<T> task, String subtitle)
            : this()
        {
            this.Text = task.Name + " on " + subtitle;
            this.btnErrorTrace.Click += new EventHandler(delegate(Object sender, EventArgs e)
                {
                    fclsTaskErrorTrace<T> errorTrace = new fclsTaskErrorTrace<T>(task, subtitle);
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

        public fclsTaskInformation(QueryTask task, String subtitle)
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

        public fclsTaskInformation(UpdateTask task, String subtitle)
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

        public fclsTaskInformation(ListGraphsTask task, String subtitle)
            : this((ITask<T>)task, subtitle)
        {

        }

        public fclsTaskInformation(ITask<IGraph> task, String subtitle)
            : this((ITask<T>)task, subtitle)
        {
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

        private void ShowInformation(ITask<T> task)
        {
            CrossThreadSetText(this.lblTaskState, String.Format("Task State: {0}", task.State.GetStateDescription()));
            CrossThreadSetText(this.txtBasicInfo, task.Information);
            CrossThreadSetText(this.lblElapsed, (task.Elapsed != null) ? String.Format("Time Elapsed: {0}", task.Elapsed) : "N/A");
            CrossThreadSetEnabled(this.btnCancel,task.IsCancellable && task.State != TaskState.Completed && task.State != TaskState.CompletedWithErrors);
            CrossThreadSetEnabled(this.btnErrorTrace, task.Error != null);
            CrossThreadSetEnabled(this.btnViewResults, task.State == TaskState.Completed && task.Result != null);
        }

        private void ShowQueryInformation(QueryTask task)
        {
            if (task.Query != null)
            {
                SparqlFormatter formatter = new SparqlFormatter(task.Query.NamespaceMap);
                StringWriter writer = new StringWriter();
                writer.WriteLine("Parsed Query:");
                writer.WriteLine(formatter.Format(task.Query));
                writer.WriteLine("SPARQL Algebra:");
                writer.WriteLine(task.Query.ToAlgebra().ToString());
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
