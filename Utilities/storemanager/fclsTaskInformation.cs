using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VDS.RDF.Writing.Formatting;
using VDS.RDF.Utilities.StoreManager.Tasks;

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
            this.ShowInformation(task);

            task.StateChanged += new TaskStateChanged(delegate()
                {
                    this.ShowInformation(task);
                });
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
                CrossThreadSetText(this.txtAdvInfo, "Parsed Query:\n" + formatter.Format(task.Query) + "\nSPARQL Algebra:\n" + task.Query.ToAlgebra().ToString());
            }
        }

        private void ShowUpdateInformation(UpdateTask task)
        {
            if (task.Updates != null)
            {
                CrossThreadSetText(this.txtAdvInfo, "Parsed Updates:\n" + task.Updates.ToString());
            }
        }

    }
}
