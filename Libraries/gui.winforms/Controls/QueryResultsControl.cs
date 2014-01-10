using System;
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

        /// <summary>
        /// Query Results Control
        /// </summary>
        public QueryResultsControl()
        {
            InitializeComponent();
            this.splPanel.Panel1Collapsed = true;
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
                if (value == null) return;
                if (value is SparqlResultSet)
                {
                    this._dataSource = value;
                    this.splPanel.SuspendLayout();
                    this.splPanel.Panel2.Controls.Clear();
                    ResultSetViewerControl control = new ResultSetViewerControl();
                    control.DisplayResultSet((SparqlResultSet)value, this.Namespaces);
                    control.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                    this.splPanel.Panel2.Controls.Add(control);
                    this.splPanel.ResumeLayout();
                } 
                else if (value is IGraph)
                {
                    this._dataSource = value;
                    this.splPanel.SuspendLayout();
                    this.splPanel.Panel2.Controls.Clear();
                    GraphViewerControl control = new GraphViewerControl();
                    IGraph g = (IGraph) value;
                    control.DisplayGraph(g, MergeNamespaceMaps(g.NamespaceMap, this.Namespaces));
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
            this.RaiseClosed();
        }

        protected void RaiseClosed()
        {
            ResultsClosed d = this.Closed;
            if (d == null) return;
            d(this);
        }

        public event ResultsClosed Closed;
    }
}