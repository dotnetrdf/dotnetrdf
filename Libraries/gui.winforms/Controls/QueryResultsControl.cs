using System;
using System.ComponentModel;
using System.Windows.Forms;
using VDS.RDF.Query;

namespace VDS.RDF.GUI.WinForms.Controls
{
    /// <summary>
    /// A control for displaying query results
    /// </summary>
    public partial class QueryResultsControl : UserControl
    {
        private Object _dataSource;
        private bool _allowDetach = true;

        /// <summary>
        /// Query Results Control
        /// </summary>
        public QueryResultsControl()
        {
            InitializeComponent();
            this.splPanel.Panel1Collapsed = true;
            this.splResults.Visible = false;

            // Only detachable if allowed and not already top level control on the form
            this.btnDetach.Visible = this.AllowDetach && !(this.Parent is Form);
        }

        private void btnToggleQuery_Click(object sender, EventArgs e)
        {
            this.splPanel.Panel1Collapsed = !this.splPanel.Panel1Collapsed;
            this.btnToggleQuery.Text = this.splPanel.Panel1Collapsed ? "Show &Query" : "Hide &Query";
            this.btnToggleResults.Text = this.splPanel.Panel2Collapsed ? "Show &Results" : "Hide &Results";
        }

        private void btnToggleResults_Click(object sender, EventArgs e)
        {
            this.splPanel.Panel2Collapsed = !this.splPanel.Panel2Collapsed;
            this.btnToggleQuery.Text = this.splPanel.Panel1Collapsed ? "Show &Query" : "Hide &Query";
            this.btnToggleResults.Text = this.splPanel.Panel2Collapsed ? "Show &Results" : "Hide &Results";
        }

        /// <summary>
        /// Gets/Sets whether detaching results is allowing
        /// </summary>
        [DefaultValue(true)]
        public bool AllowDetach
        {
            get { return this._allowDetach; }
            set
            {
                this._allowDetach = value;
                this.btnDetach.Visible = this._allowDetach;
            }
        }

        /// <summary>
        /// Get/Sets the namespaces to use
        /// </summary>
        public INamespaceMapper Namespaces { get; set; }

        /// <summary>
        /// Gets/Sets the query string
        /// </summary>
        public String QueryString
        {
            get { return this.txtQuery.Text; }
            set { this.txtQuery.Text = value.Replace("\n", "\r\n"); }
        }

        /// <summary>
        /// Gets/Sets the data source
        /// </summary>
        public Object DataSource
        {
            get { return this._dataSource; }
            set
            {
                if (value == null)
                {
                    this.splResults.Visible = false;
                    return;
                }
                if (value is SparqlResultSet)
                {
                    this._dataSource = value;
                    this.splResults.SuspendLayout();
                    this.splResults.Panel1Collapsed = false;
                    this.splResults.Panel2Collapsed = true;
                    this.resultsViewer.DisplayResultSet((SparqlResultSet) value, this.Namespaces);
                    this.splResults.ResumeLayout();
                    this.splResults.Visible = true;
                }
                else if (value is IGraph)
                {
                    this._dataSource = value;
                    this.splResults.SuspendLayout();
                    this.splResults.Panel1Collapsed = true;
                    this.splResults.Panel2Collapsed = false;
                    IGraph g = (IGraph) value;
                    this.graphViewer.DisplayGraph(g, MergeNamespaceMaps(g.NamespaceMap, this.Namespaces));
                    this.splResults.ResumeLayout();
                    this.splResults.Visible = true;
                }
                else
                {
                    throw new ArgumentException("Only SparqlResultSet and IGraph may be used as the DataSource for this control");
                }
            }
        }

        private static INamespaceMapper MergeNamespaceMaps(INamespaceMapper main, INamespaceMapper secondary)
        {
            NamespaceMapper nsmap = new NamespaceMapper(true);
            if (main != null) nsmap.Import(main);
            if (secondary != null) nsmap.Import(secondary);
            return nsmap;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.RaiseCloseRequested();
        }


        private void btnDetach_Click(object sender, EventArgs e)
        {
            this.RaiseDetachRequested();
        }

        protected void RaiseCloseRequested()
        {
            ResultCloseRequested d = this.CloseRequested;
            if (d == null) return;
            d(this);
        }

        /// <summary>
        /// Event which is raised when the user has clicked the close button
        /// </summary>
        public event ResultCloseRequested CloseRequested;

        /// <summary>
        /// Event which is raised when the user has clicked the detach button
        /// </summary>
        public event ResultDetachRequested DetachRequested;

        protected void RaiseDetachRequested()
        {
            ResultDetachRequested d = this.DetachRequested;
            if (d == null) return;
            d(this);
        }
    }
}