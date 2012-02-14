/*

Copyright Robert Vesse 2009-12
rvesse@vdesign-studios.com

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
using VDS.RDF.Query;
using VDS.RDF.Writing.Formatting;
using VDS.RDF.Utilities.StoreManager.Tasks;
using VDS.RDF.GUI.WinForms;

namespace VDS.RDF.Utilities.StoreManager
{
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

        public TaskInformationForm(ITask<IGraph> task, String subtitle)
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
