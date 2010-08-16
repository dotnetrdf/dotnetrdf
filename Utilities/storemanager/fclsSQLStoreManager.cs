/*

Copyright Robert Vesse 2009-10
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

If this license is not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Data;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Params;
using VDS.RDF.Writing;

namespace dotNetRDFStore
{
    public partial class fclsSQLStoreManager : CrossThreadForm
    {
        private BaseDBConnection _connection = null;
        private StoreUpgrader _upgrader = null;
        private StoreCompacter _compacter = null;

        public fclsSQLStoreManager()
        {
            InitializeComponent();
        }

        public ISqlIOManager Manager
        {
            get
            {
                if (this._connection != null)
                {
                    return this._connection.Manager;
                }
                else
                {
                    return null;
                }
            }
        }

        private void fclsStoreManager_Load(object sender, EventArgs e)
        {
            this.cboDBFormat.SelectedIndex = 0;
            this.cboExportFormat.SelectedIndex = 0;

            this.ShowDatabaseInformation();
        }

        private void fclsStoreManager_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            if (this._connection != null)
            {
                if (this._connection.IsConnected)
                {
                    if (this._upgrader != null || this._compacter != null)
                    {
                        this.Text = "Waiting to Disconnect";

                        while (this._upgrader != null || this._compacter != null)
                        {
                            Thread.Sleep(100);
                        }
                    }
                    this._connection.Disconnect();
                }
            }
        }

        private void ShowDatabaseInformation()
        {
            if (this._connection != null)
            {
                this.lblConnectionRequired.Visible = false;
                this.lvwDBInfo.Visible = true;

                this.lvwDBInfo.BeginUpdate();
                this.lvwDBInfo.Items.Clear();

                if (this._connection.IsConnected)
                {
                    this.Text = "Store - " + this._connection.ToString();
                    this.btnInfoRefresh.Enabled = true;
                    try
                    {
                        //Create the Items
                        ListViewItem item;
                        String[] data;

                        //Is dotNetRDFStore
                        data = new String[] { "Is a dotNetRDF Store?", this.BooleanToYesNo(this._connection.IsDotNetRDFStore()) };
                        item = new ListViewItem(data);
                        this.lvwDBInfo.Items.Add(item);

                        //Store Version
                        data = new String[] { "dotNetRDF Store Version", this._connection.Version() };
                        item = new ListViewItem(data);
                        this.lvwDBInfo.Items.Add(item);

                        if (this._connection.IsDotNetRDFStore())
                        {
                            char quoteStart = '[';
                            char quoteEnd = ']';
                            if (this._connection is MySQLServerConnection)
                            {
                                quoteStart = quoteEnd = '\'';
                            }
                            String[] stats = new String[] {
                                "SELECT COUNT(graphID) AS " + quoteStart + "Total Graphs" + quoteEnd + " FROM GRAPHS",
                                "SELECT COUNT(graphID) AS " + quoteStart + "Total Graph Triples" + quoteEnd + " FROM GRAPH_TRIPLES",
                                "SELECT COUNT(tripleID) AS " + quoteStart + "Unique Triples" + quoteEnd + " FROM TRIPLES",
                                "SELECT COUNT(nodeID) AS " + quoteStart + "Unique Nodes" + quoteEnd + " FROM NODES"
                            };

                            foreach (String stat in stats)
                            {
                                DataTable results = this._connection.ExecuteQuery(stat);
                                data = new String[] { results.Columns[0].ColumnName, results.Rows[0][0].ToString() };
                                item = new ListViewItem(data);
                                this.lvwDBInfo.Items.Add(item);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error retrieving Database Information:\n" + ex.Message);
                    }
                }
                else
                {
                    this.Text = "Store - Disconnected";
                    this.lblConnectionRequired.Visible = true;
                    this.lvwDBInfo.Visible = false;
                    this.btnInfoRefresh.Enabled = false;
                }
                this.lvwDBInfo.EndUpdate();
            }
            else
            {
                this.Text = "Store - Disconnected";
                this.lblConnectionRequired.Visible = true;
                this.lvwDBInfo.Visible = false;
                this.btnInfoRefresh.Enabled = false;
            }

            this.tabInfo.Refresh();
            this.ShowDatabaseTools();
            this.ShowDatabaseGraphs();
            this.ShowDatabaseIO();
        }

        private void ShowDatabaseTools()
        {
            if (this._connection != null)
            {
                if (this._connection.IsConnected)
                {
                    this.panTools.Visible = true;
                    this.lblConnectionRequired2.Visible = false;

                    if (this._connection.IsDotNetRDFStore())
                    {
                        this.btnCreateStore.Enabled = false;
                        this.btnDeleteStore.Enabled = true;

                        if (this._connection.Version().Equals("0.1.0"))
                        {
                            if (this._upgrader == null)
                            {
                                this.prgUpgrade.Value = 0;
                                this.btnUpgradeStore.Enabled = true;
                            }
                            else
                            {
                                this.btnUpgradeStore.Enabled = false;
                            }
                        }
                        else
                        {
                            this.btnUpgradeStore.Enabled = false;
                        }

                        if (this._compacter == null)
                        {
                            this.prgCompact.Value = 0;
                            this.btnCompactStore.Enabled = true;
                        }
                        else
                        {
                            this.btnCompactStore.Enabled = false;
                        }
                    }
                    else
                    {
                        this.btnCreateStore.Enabled = true;
                        this.btnDeleteStore.Enabled = false;
                        this.btnUpgradeStore.Enabled = false;
                        this.btnCompactStore.Enabled = false;
                    }
                }
                else
                {
                    this.panTools.Visible = false;
                    this.lblConnectionRequired2.Visible = true;
                }
            }
            else
            {
                this.panTools.Visible = false;
                this.lblConnectionRequired2.Visible = true;
            }
            this.tabTools.Refresh();
        }

        private void ShowDatabaseGraphs()
        {
            if (this._connection != null)
            {
                if (this._connection.IsConnected)
                {
                    this.lblConnectionRequired3.Visible = false;

                    this.lvwGraphs.Visible = true;
                    this.lvwGraphs.BeginUpdate();
                    this.lvwGraphs.Items.Clear();

                    if (this._connection.IsDotNetRDFStore())
                    {
                        String getGraphs = "SELECT graphUri, COUNT(tripleID) AS GraphTriples FROM GRAPHS G INNER JOIN GRAPH_TRIPLES T ON G.graphID=T.graphID GROUP BY graphUri ORDER BY graphUri";
                        DataTable graphs = this._connection.ExecuteQuery(getGraphs);
                        foreach (DataRow r in graphs.Rows)
                        {
                            String[] g = new String[] { r["graphUri"].ToString(), r["GraphTriples"].ToString() };
                            ListViewItem item = new ListViewItem(g);
                            this.lvwGraphs.Items.Add(item);
                        }

                        this.btnGraphRefresh.Enabled = true;
                    }
                    else
                    {
                        this.btnGraphRefresh.Enabled = false;
                    }

                    this.lvwGraphs.EndUpdate();
                }
                else
                {
                    this.btnGraphRefresh.Enabled = false;
                    this.lblConnectionRequired3.Visible = true;
                    this.lvwGraphs.Visible = false;
                }
            }
            else
            {
                this.btnGraphRefresh.Enabled = false;
                this.lblConnectionRequired3.Visible = true;
                this.lvwGraphs.Visible = false;
            }
            this.tabGraphs.Refresh();
        }

        private void ShowDatabaseIO()
        {
            bool show = false;
            if (this._connection != null)
            {
                show = this._connection.IsConnected;
            }
            this.lblConnectionRequired4.Visible = !show;
            this.grpImport.Visible = show;
            this.grpExport.Visible = show;
            this.btnExport.Enabled = show;
            if (show)
            {
                if (!this._connection.IsDotNetRDFStore())
                {
                    this.btnExport.Enabled = false;
                }
            }
            this.tabIO.Refresh();
        }

        private String BooleanToYesNo(bool b)
        {
            return (b) ? "Yes" : "No";
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (this._connection != null) 
            {
                this._connection.Disconnect();
                this.btnDisconnect.Enabled = false;
                this.ShowDatabaseInformation();
            }

            try {
                switch (this.cboDBFormat.SelectedIndex)
                {
                    case 0:
                        //Microsoft SQL Server
                        this._connection = new MicrosoftSQLServerConnection(this.txtDBServer.Text, this.txtDBName.Text, this.txtUsername.Text, this.txtPassword.Text);
                        break;

                    case 1:
                        //MySQL Server
                        if (this.txtPort.Text.Equals(String.Empty))
                        {
                            //No Port specified
                            this._connection = new MySQLServerConnection(this.txtDBServer.Text, this.txtDBName.Text, this.txtUsername.Text, this.txtPassword.Text);
                        }
                        else
                        {
                            int port;
                            if (Int32.TryParse(this.txtPort.Text, out port))
                            {
                                //Port Specified
                                this._connection = new MySQLServerConnection(this.txtDBServer.Text, port, this.txtDBName.Text, this.txtUsername.Text, this.txtPassword.Text);
                            }
                            else
                            {
                                //Invalid Port Specified
                                this._connection = new MySQLServerConnection(this.txtDBServer.Text, this.txtDBName.Text, this.txtUsername.Text, this.txtPassword.Text);
                            }
                        } 
                        break;

                    case 2:
                        //Virtuoso Universal Server (Virtual Database)
                        MessageBox.Show("Virtuoso Universal Server (Virtual Database) not yet supported in this Tool");
                        this._connection = null;
                        break;
                    case 3:
                        //Microsoft Access
                        MessageBox.Show("Microsoft Access not yet supported in this Tool");
                        this._connection = null;
                        break;
                    default:
                        MessageBox.Show("Unknown Database Format selected");
                        this._connection = null;
                        break;
                }

                if (this._connection != null)
                {
                    this._connection.Connect();
                    this.btnDisconnect.Enabled = true;
                    this.btnConnect.Enabled = false;
                }
                else
                {
                    this.btnDisconnect.Enabled = false;
                    this.btnConnect.Enabled = true;
                }
                this.ShowDatabaseInformation();
            } 
            catch (Exception ex) 
            {
                MessageBox.Show("Error connecting to the specified Database:\n" + ex.Message, "Database Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            if (this._connection != null)
            {
                if (this._upgrader != null || this._compacter != null)
                {
                    MessageBox.Show("Unable to Disconnect while a Store Upgrade/Compaction is in progress!");
                }
                else
                {
                    this._connection.Disconnect();
                    this.btnDisconnect.Enabled = false;
                    this.btnConnect.Enabled = true;
                    this.ShowDatabaseInformation();
                }
            }
        }

        private void btnCreateStore_Click(object sender, EventArgs e)
        {
            if (this._connection != null)
            {
                if (this._connection.IsConnected)
                {
                    if (!this._connection.IsDotNetRDFStore())
                    {
                        try
                        {
                            this._connection.CreateStore();

                            MessageBox.Show("Store Created successfully!", "Store Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Store Creation failed:\n" + ex.Message, "Store Creation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }

                        this.ShowDatabaseInformation();
                    }
                    else
                    {
                        MessageBox.Show("This Database is already a dotNetRDF Store so Store Creation is not required", "Create Store Not Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void btnDeleteStore_Click(object sender, EventArgs e)
        {
            if (this._connection != null)
            {
                if (this._connection.IsConnected)
                {
                    if (this._connection.IsDotNetRDFStore())
                    {
                        if (MessageBox.Show("Are you sure you wish to delete your dotNetRDF Store?  This action is permanent and non-reversible, do you still wish to proceed?", "Confirm Store Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            try
                            {
                                this._connection.DropStore();

                                MessageBox.Show("Store Deleted successfully!", "Store Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Store Deletion failed:\n" + ex.Message, "Store Deletion Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }

                            this.ShowDatabaseInformation();
                        }
                    }
                    else
                    {
                        MessageBox.Show("This Database is not a dotNetRDF Store so Store Deletion is not required", "Delete Store Not Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void btnGraphRefresh_Click(object sender, EventArgs e)
        {
            this.ShowDatabaseGraphs();
        }

        private void lvwGraphs_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            switch (this.lvwGraphs.Sorting)
            {
                case SortOrder.Ascending:
                    this.lvwGraphs.Sorting = SortOrder.Descending;
                    break;
                case SortOrder.Descending:
                case SortOrder.None:
                default:
                    this.lvwGraphs.Sorting = SortOrder.Ascending;
                    break;
            }

            this.lvwGraphs.ListViewItemSorter = new ListViewColumnSorter(e.Column, this.lvwGraphs.Sorting);
        }

        private void lvwGraphs_DoubleClick(object sender, System.EventArgs e)
        {
            if (this.lvwGraphs.SelectedIndices.Count > 0)
            {
                int id = this.lvwGraphs.SelectedIndices[0];

                //Get the Graph Uri
                Uri u = new Uri(this.lvwGraphs.Items[id].SubItems[0].Text);

                //Show the Graph Manager
                fclsSQLGraphManager graphManager = new fclsSQLGraphManager(u, this._connection.Manager);
                graphManager.MdiParent = this.MdiParent;
                graphManager.Show();
            }
        }

        private void btnUpgradeStore_Click(object sender, EventArgs e)
        {
            if (this._upgrader == null)
            {
                if (MessageBox.Show("Are you sure you wish to Upgrade the current Store to the latest dotNetRDF Store Format?  This format will has better performance but this action cannot be reversed - do you still wish to proceed?", "Confirm Store Upgrade", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    this.prgUpgrade.Value = 0;

                    this._upgrader = new StoreUpgrader(this._connection);

                    if (this._upgrader.UpgradeRequired)
                    {
                        this.prgUpgrade.Maximum = this._upgrader.OperationsRequired();
                        this._upgrader.Progress += this.UpgradeProgress;
                        this._upgrader.Error += this.UpgradeError;

                        if (MessageBox.Show("Ready to begin Upgrade, please click OK to begin", "Confirm Upgrade", MessageBoxButtons.OKCancel) == DialogResult.OK)
                        {
                            this.btnUpgradeStore.Enabled = false;
                            this._upgrader.Upgrade();
                        }
                        else
                        {
                            this._upgrader = null;
                        }
                    }
                    else
                    {
                        MessageBox.Show("No Upgrade of this Store is required", "Upgrade Store Not Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.btnUpgradeStore.Enabled = false;
                        this._upgrader = null;
                    }
                }
            }
        }

        private void btnCompactStore_Click(object sender, EventArgs e)
        {
            if (this._compacter == null)
            {
                if (this._connection.IsDotNetRDFStore())
                {
                    this._compacter = new StoreCompacter(this._connection, this.chkCompactFull.Checked);

                    if (MessageBox.Show("Are you sure you wish to Compact this dotNetRDF Store?", "Confirm Store Compaction", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        this.prgCompact.Value = 0;
                        this.prgCompact.Maximum = this._compacter.OperationsRequired();
                        this._compacter.Error += this.CompactionError;
                        this._compacter.Progress += this.CompactionProgress;

                        this.btnCompactStore.Enabled = false;
                        this._compacter.Compact();
                    }
                }
            }
        }

        #region Cross Thread UI Stuff

        private delegate void OperationProgressDelegate(int value);

        private delegate void OperationErrorDelegate(Exception ex);

        private void UpgradeProgress(int value)
        {
            if (this.prgUpgrade.InvokeRequired)
            {
                Delegate d = new OperationProgressDelegate(UpgradeProgress);
                this.Invoke(d, new Object[] { value });
            }
            else
            {
                this.prgUpgrade.Value = value;
                if (this.prgUpgrade.Value == this.prgUpgrade.Maximum)
                {
                    this._upgrader = null;
                    this.ShowDatabaseInformation();
                    MessageBox.Show("Upgrade Completed successfully!");
                }
            }
        }

        private void UpgradeError(Exception ex)
        {
            if (this.InvokeRequired)
            {
                Delegate d = new OperationErrorDelegate(UpgradeError);
                this.Invoke(d, new Object[] { ex });
            }
            else
            {
                MessageBox.Show("An error occurred while trying to perform the Upgrade:\n" + ex.Message);
                this.ShowDatabaseInformation();
                this._upgrader = null;
            }
        }

        private void CompactionProgress(int value)
        {
            if (this.prgCompact.InvokeRequired)
            {
                Delegate d = new OperationProgressDelegate(CompactionProgress);
                this.Invoke(d, new Object[] { value });
            }
            else
            {
                this.prgCompact.Value = value;
                if (this.prgCompact.Value == this.prgCompact.Maximum)
                {
                    this._compacter = null;
                    this.ShowDatabaseInformation();
                    MessageBox.Show("Compaction Completed successfully!");
                }
            }
        }

        private void CompactionError(Exception ex)
        {
            if (this.InvokeRequired)
            {
                Delegate d = new OperationErrorDelegate(CompactionError);
                this.Invoke(d, new Object[] { ex });
            }
            else
            {
                MessageBox.Show("An error occurred while trying to compact the Store:\n" + ex.Message);
                this.ShowDatabaseInformation();
                this._compacter = null;
            }
        }

        #endregion

        private void btnInfoRefresh_Click(object sender, EventArgs e)
        {
            this.ShowDatabaseInformation();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            this.sfdExport.FilterIndex = this.cboExportFormat.SelectedIndex+1;
            if (this.sfdExport.ShowDialog() == DialogResult.OK)
            {
                String destFile = this.sfdExport.FileName;

                //Select the Writer
                IStoreWriter writer;
                switch (this.cboExportFormat.SelectedIndex)
                {
                    case 0:
                        writer = new TriGWriter();
                        break;
                    case 1:
                        writer = new TriXWriter();
                        break;
                    case 2:
                        writer = new NQuadsWriter();
                        break;
                    default:
                        writer = new TriGWriter();
                        break;
                }

                //Check the Extension matches the output format
                String ext = MimeTypesHelper.GetFileExtension(writer);
                if (!Path.GetExtension(destFile).Equals("." + ext))
                {
                    destFile = Path.Combine(Path.GetDirectoryName(destFile), Path.GetFileNameWithoutExtension(destFile) + "." + ext);
                }

                //Estimate Load Time
                int graphTriples = Int32.Parse(this.lvwDBInfo.Items[3].SubItems[1].Text);
                int loadTime = graphTriples / 25000;

                if (MessageBox.Show("Are you sure you wish to export this Store?  To do this your entire Store must first be loaded into memory - we estimate that this will take approximately " + loadTime + " seconds - and then it will be written to disk.  Are you sure you wish to proceed?", "Confirm Export", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    //Export
                    try
                    {
                        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
                        timer.Start();
                        ThreadedSqlTripleStore store = new ThreadedSqlTripleStore(this._connection.ThreadedManager, 8, false);
                        timer.Stop();
                        double actualLoadTime = (double)timer.ElapsedMilliseconds / 1000d;
                        timer.Reset();
                        timer.Start();
                        writer.Save(store, new StreamParams(destFile));
                        timer.Stop();
                        double actualWriteTime = (double)timer.ElapsedMilliseconds / 1000d;

                        MessageBox.Show("Export completed successfully to file '" + destFile + "'\n\nLoad took " + actualLoadTime + " seconds - Writing took " + actualWriteTime + " seconds - Export took " + (actualLoadTime+actualWriteTime) + " seconds total", "Export Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Export failed due to the following error:\n" + ex.Message, "Export Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }                        
                }
                
            }
        }

        private void btnImportFile_Click(object sender, EventArgs e)
        {
            if (!this.txtImportFile.Text.Equals(String.Empty))
            {
                try
                {
                    IRdfReader parser = MimeTypesHelper.GetParser(MimeTypesHelper.GetMimeType(Path.GetExtension(this.txtImportFile.Text)));
                    Graph g = new Graph();
                    parser.Load(g, this.txtImportFile.Text);

                    SqlWriter writer = new SqlWriter(this._connection.Manager);
                    writer.Save(g, true);

                    MessageBox.Show("Successfully imported a Graph into the Store", "Import Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    this.ShowDatabaseInformation();
                }
                catch (RdfParserSelectionException)
                {
                    //Should be an RDF dataset instead?
                    try
                    {
                        //Push into a Background Thread here and show a modal dialog while it's processing
                        Thread importer = new Thread(new ThreadStart(delegate { this.ImportStore(this.txtImportFile.Text); }));
                        fclsPleaseWait wait = new fclsPleaseWait("Import", importer);
                        wait.ShowDialog();

                        this.ShowDatabaseInformation();
                    }
                    catch (RdfParserSelectionException)
                    {
                        MessageBox.Show("Unable to perform an input as the format of the file could not be determined", "Import Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (RdfParseException)
                    {
                        MessageBox.Show("Unable to parse RDF Graphs from the selected file as a Parsing Error occurred", "Import Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (RdfParseException)
                {
                    MessageBox.Show("Unable to parse an RDF Graph from the selected file as a Parsing Error occurred", "Import Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (RdfStorageException storeEx)
                {
                    MessageBox.Show("Import failed due to the following error:\n" + storeEx.Message, "Import Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ImportStore(String filename)
        {
            try
            {
                IStoreReader storeparser = MimeTypesHelper.GetStoreParser(MimeTypesHelper.GetMimeType(Path.GetExtension(filename)));
                TripleStore store = new TripleStore();
                storeparser.Load(store, new StreamParams(this.txtImportFile.Text));

                SqlStoreWriter writer = new SqlStoreWriter();
                writer.Save(store, new SqlIOParams(this._connection.Manager, true));

                this.CrossThreadMessage("Successfully imported " + store.Graphs.Count + " Graphs into the Store", "Import Completed", MessageBoxIcon.Information);
            }
            catch (RdfStorageException storeEx)
            {
                this.CrossThreadMessage("An error occurred during import:\n" + storeEx.Message, "Import Failed", MessageBoxIcon.Error);
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (this.ofdImport.ShowDialog() == DialogResult.OK)
            {
                this.txtImportFile.Text = this.ofdImport.FileName;
            }
        }
 

    }
}
