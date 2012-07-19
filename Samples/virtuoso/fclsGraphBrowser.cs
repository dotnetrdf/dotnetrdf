using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VDS.RDF;
using VDS.RDF.Storage;

namespace dotNetRDFVirtuosoSample
{
    public partial class fclsGraphBrowser : Form
    {
        private Uri _graphUri;
        private VirtuosoManager _manager;
        private DataTable _data;

        private const String VirtuosoQuadStoreDB = "DB";

        public fclsGraphBrowser()
        {
            InitializeComponent();

            //Connect to Virtuoso
            this._manager = new VirtuosoManager(Properties.Settings.Default.Server, Properties.Settings.Default.Port, VirtuosoQuadStoreDB, Properties.Settings.Default.Username, Properties.Settings.Default.Password);

            //Add some fake test data
            DataTable data = new DataTable();
            data.Columns.Add("Subject");
            data.Columns.Add("Predicate");
            data.Columns.Add("Object");
            data.Columns["Subject"].DataType = typeof(INode);
            data.Columns["Predicate"].DataType = typeof(INode);
            data.Columns["Object"].DataType = typeof(INode);

            Graph g = new Graph();
            DataRow row = data.NewRow();
            row["Subject"] = g.CreateUriNode(new Uri("http://example.org/subject"));
            row["Predicate"] = g.CreateUriNode(new Uri("http://example.org/predicate"));
            row["Object"] = g.CreateUriNode(new Uri("http://example.org/object"));
            data.Rows.Add(row);

            this.BindGraph(data);
        }

        public fclsGraphBrowser(Uri u)
            : this()
        {
            this._graphUri = u;
            this.txtURI.Text = u.AbsoluteUri;
            this.ViewGraph(u);
        }

        private void BindGraph(DataTable data)
        {
            this._data = data;

            //Show in the DataGridView
            this.dvwBrowser.SuspendLayout();
            this.dvwBrowser.Rows.Clear();

            foreach (DataRow r in data.Rows)
            {
                this.dvwBrowser.Rows.Add(new String[] { r["Subject"].ToString(), r["Predicate"].ToString(), r["Object"].ToString() });          
            }

            this.dvwBrowser.AutoResizeColumns();
            this.dvwBrowser.ResumeLayout(true);
        }

        private void ViewGraph(Uri u)
        {
            this._graphUri = u;
            this.Text = "Browsing Graph '" + u.AbsoluteUri + "'";

            Graph g = new Graph();
            try
            {
                //Load the Graph from Virtuoso
                this._manager.LoadGraph(g, u);

                if (g.IsEmpty)
                {
                    //If we get an Empty Graph then we want to try DESCRIBing the resource instead
                    Object result = this._manager.Query("DESCRIBE <" + u.AbsoluteUri.Replace(">","\\>") + ">");
                    if (result is Graph)
                    {
                        g = (Graph)result;
                        this.Text = "Browsing DESCRIBE of '" + u.AbsoluteUri + "'";
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error trying to load the Graph '" + u.AbsoluteUri + "'\n\n" + ex.Message);
            }

            //Create a DataTable
            DataTable data = new DataTable();
            data.Columns.Add("Subject");
            data.Columns.Add("Predicate");
            data.Columns.Add("Object");
            data.Columns["Subject"].DataType = typeof(INode);
            data.Columns["Predicate"].DataType = typeof(INode);
            data.Columns["Object"].DataType = typeof(INode);

            //Populate with Data
            foreach (Triple t in g.Triples)
            {
                //Create a new ListViewItem
                DataRow row = data.NewRow();

                //Fill it's subitems with the Subject, Predicate and Object
                row["Subject"] = t.Subject;
                row["Predicate"] = t.Predicate;
                row["Object"] = t.Object;

                data.Rows.Add(row);
            }

            this.BindGraph(data);
        }

        private void btnViewGraph_Click(object sender, EventArgs e)
        {
            try
            {
                Uri u = new Uri(this.txtURI.Text);
                this.ViewGraph(u);
            }
            catch (UriFormatException)
            {
                MessageBox.Show("Not a valid URI");
            }
        }

        private void dvwBrowser_CellDoubleClick(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                Object temp = this._data.Rows[e.RowIndex][e.ColumnIndex];
                if (temp is INode)
                {
                    INode n = (INode)temp;
                    if (n.NodeType == NodeType.Uri)
                    {
                        Uri u = ((IUriNode)n).Uri;
                        fclsGraphBrowser browser = new fclsGraphBrowser(u);
                        browser.Show();
                    }
                }
            }
        }
    }
}
