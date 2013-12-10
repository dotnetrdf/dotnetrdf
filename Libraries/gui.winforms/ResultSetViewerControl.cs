using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using VDS.RDF.Query;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.GUI.WinForms
{
    /// <summary>
    /// A Form that displays a SPARQL Result Set using a DataGridView
    /// </summary>
    public partial class ResultSetViewerControl : UserControl
    {
        private SparqlResultSet _results;
        private INodeFormatter _formatter = new SparqlFormatter();
        private INamespaceMapper _nsmap;

        public ResultSetViewerControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Displays the given SPARQL Result Set
        /// </summary>
        /// <param name="results">SPARQL Result to display</param>
        public void DisplayResultSet(SparqlResultSet results)
        {
            

            //Load Formatters
            List<INodeFormatter> formatters = new List<INodeFormatter>();
            Type targetType = typeof(INodeFormatter);
            foreach (Type t in Assembly.GetAssembly(targetType).GetTypes())
            {
                if (t.Namespace == null) continue;

                if (t.Namespace.Equals("VDS.RDF.Writing.Formatting"))
                {
                    if (t.GetInterfaces().Contains(targetType))
                    {
                        try
                        {
                            INodeFormatter formatter = (INodeFormatter)Activator.CreateInstance(t);
                            formatters.Add(formatter);
                            if (formatter.GetType().Equals(this._formatter.GetType())) this._formatter = formatter;
                        }
                        catch
                        {
                            //Ignore this Formatter
                        }
                    }
                }
            }
            formatters.Sort(new ToStringComparer<INodeFormatter>());
            this.cboFormat.DataSource = formatters;
            this.cboFormat.SelectedItem = this._formatter;
            this.cboFormat.SelectedIndexChanged += new System.EventHandler(this.cboFormat_SelectedIndexChanged);

            this.dgvResults.CellFormatting += new DataGridViewCellFormattingEventHandler(dgvTriples_CellFormatting);
            this.dgvResults.CellContentClick += new DataGridViewCellEventHandler(dgvTriples_CellClick);

            this._results = results;

            this.Text = this.GetTitle();

            //Load Graph and Populate Form Fields
            this.LoadInternal();
        }

       /// <summary>
        /// Creates a new Result Set viewer form
        /// </summary>
        /// <param name="results">Result Set</param>
        /// <param name="nsmap">Namespace Map to use for display</param>
        public void DisplayResultSet(SparqlResultSet results, INamespaceMapper nsmap)
        {
            this._nsmap = nsmap;
            if (nsmap != null) this._formatter = new SparqlFormatter(nsmap);

            DisplayResultSet(results);
        }

        private String GetTitle()
        {
            if (this._results.ResultsType == SparqlResultsType.Boolean)
            {
                return "SPARQL Results Viewer - Boolean Result";
            }
            else if (this._results.ResultsType == SparqlResultsType.VariableBindings)
            {
                return String.Format("SPARQL Results Viewer - {0} Result(s)", this._results.Count);
            }
            else
            {
                return "SPARQL Results Viewer";
            }
        }

        private String GetTitle(String title)
        {
            return String.Format("{0} - " + this.GetTitle(), title);
        }

        /// <summary>
        /// Event which is raised when the User clicks a URI
        /// </summary>
        public event UriClickedEventHandler UriClicked;

        #region Internal Implementation

        void dgvTriples_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value is INode)
            {
                INode n = (INode)e.Value;
                e.Value = this._formatter.Format(n);
                e.FormattingApplied = true;
                switch (n.NodeType)
                {
                    case NodeType.Uri:
                        e.CellStyle.Font = new System.Drawing.Font(e.CellStyle.Font, System.Drawing.FontStyle.Underline);
                        e.CellStyle.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0, 255);
                        break;
                }
            }
        }

        void dgvTriples_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            Object value = this.dgvResults[e.ColumnIndex, e.RowIndex].Value;
            if (value != null)
            {
                if (value is INode)
                {
                    INode n = (INode)value;
                    if (n.NodeType == NodeType.Uri)
                    {
                        this.RaiseUriClicked(((IUriNode)n).Uri);
                    }
                }
            }
        }

      

        private void LoadInternal()
        {
            //Show Results
            if (this._results.ResultsType == SparqlResultsType.Boolean)
            {
                DataTable table = new DataTable();
                table.Columns.Add(new DataColumn("ASK", typeof(String)));
                DataRow row = table.NewRow();
                row["ASK"] = this._results.Result.ToString();
                table.Rows.Add(row);
                this.dgvResults.DataSource = table;
            }
            else
            {
                this.dgvResults.DataSource = (DataTable)this._results;
            }
        }

        private void cboFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.cboFormat.SelectedItem != null)
            {
                INodeFormatter formatter = this.cboFormat.SelectedItem as INodeFormatter;
                if (formatter != null)
                {
                    INodeFormatter currFormatter = this._formatter;

                    Type t = formatter.GetType();
                    if (this._nsmap != null)
                    {
                        Type nsmapType = typeof(INamespaceMapper);
                        if (t.GetConstructors().Any(c => c.IsPublic && c.GetParameters().Any() && c.GetParameters().All(p => p.ParameterType.Equals(nsmapType))))
                        {
                            try
                            {
                                this._formatter = (INodeFormatter)Activator.CreateInstance(t, new object[] { this._nsmap });
                            }
                            catch
                            {
                                this._formatter = formatter;
                            }
                        }
                        else
                        {
                            this._formatter = formatter;
                        }
                    }
                    else
                    {
                        this._formatter = formatter;
                    }

                    if (!ReferenceEquals(currFormatter, this._formatter) || !currFormatter.GetType().Equals(this._formatter.GetType()))
                    {
                        DataTable tbl = (DataTable)this.dgvResults.DataSource;
                        this.dgvResults.DataSource = null;
                        this.dgvResults.Refresh();
                        this.dgvResults.DataSource = tbl;
                    }
                }
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            ExportResultSetOptionsForm exporter = new ExportResultSetOptionsForm();
            if (exporter.ShowDialog() == DialogResult.OK)
            {
                ISparqlResultsWriter writer = exporter.Writer;
                String file = exporter.File;

                try
                {
                    writer.Save(this._results, file);

                    MessageBox.Show("Successfully exported the SPARQL Results to the file '" + file + "'", "SPARQL Results Export Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred attempting to export the SPARQL Results:\n" + ex.Message, "SPARQL Results Export Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void RaiseUriClicked(Uri u)
        {
            UriClickedEventHandler d = this.UriClicked;
            if (d != null)
            {
                d(this, u);
            }
        }

        #endregion
    }
}
