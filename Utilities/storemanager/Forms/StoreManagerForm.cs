/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using VDS.RDF.GUI.WinForms.Controls;
using VDS.RDF.GUI.WinForms.Forms;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Management;
using VDS.RDF.Storage.Management.Provisioning;
using VDS.RDF.Utilities.StoreManager.Connections;
using VDS.RDF.Utilities.StoreManager.Dialogues;
using VDS.RDF.Utilities.StoreManager.Properties;
using VDS.RDF.Utilities.StoreManager.Tasks;
using Timer = System.Windows.Forms.Timer;

namespace VDS.RDF.Utilities.StoreManager.Forms
{
    /// <summary>
    /// Form for managing stores
    /// </summary>
    public partial class StoreManagerForm
        : CrossThreadForm
    {
        private readonly EventHandler _copyGraphHandler, _moveGraphHandler;
        private bool _codeFormatInProgress = false;
        private readonly List<HighLight> _highLights = new List<HighLight>();
        private readonly Timer _highlightsUpdateTimer = new Timer();
        private bool _codeHighLightingInProgress = false;
        private int _taskId;
        private readonly System.Timers.Timer _timStartup;
        private bool _closing = true;
        private int _queryId = 0;

        /// <summary>
        /// Creates a new Store Manager form
        /// </summary>
        /// <param name="connection">Connection</param>
        public StoreManagerForm(Connection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (!connection.IsOpen) throw new ArgumentException(Resources.ConnectionMustBeOpen, "connection");

            InitializeComponent();
            this.Closing += OnClosing;

            splitQueryResults.Panel2Collapsed = true;
            ActivateHighlighting();

            // Configure Connection
            this.Connection = connection;
            this.StorageProvider = connection.StorageProvider;
            this.Text = connection.Name;

            // Subscribe to events on the connection
            this.Connection.PropertyChanged += ConnectionOnPropertyChanged;

            // Configure Tasks List
            this.lvwTasks.ListViewItemSorter = new SortTasksById();

            // Configure Graphs List
            this.lvwGraphs.ItemDrag += lvwGraphs_ItemDrag;
            this.lvwGraphs.DragEnter += lvwGraphs_DragEnter;
            this.lvwGraphs.DragDrop += lvwGraphs_DragDrop;
            this._copyGraphHandler = this.CopyGraphClick;
            this._moveGraphHandler = this.MoveGraphClick;

            // Startup Timer
            this._timStartup = new System.Timers.Timer(250);
            this._timStartup.Elapsed += timStartup_Tick;

            // Apply Editor Options
            this.ApplyEditorOptions();

            // Set highlight delay for 2 secs
            _highlightsUpdateTimer.Interval = (int) TimeSpan.FromSeconds(2).TotalMilliseconds;
            _highlightsUpdateTimer.Tick += HighlightsUpdateTimerOnTick;
        }

        private void fclsGenericStoreManager_Load(object sender, EventArgs e)
        {
            //Determine whether SPARQL Query is supported
            if (!(this.StorageProvider is IQueryableStorage))
            {
                this.tabFunctions.TabPages.Remove(this.tabSparqlQuery);
            }

            //Determine what SPARQL Update mode if any is supported
            if (this.StorageProvider is IUpdateableStorage)
            {
                this.lblUpdateMode.Text = Resources.UpdateModeNative;
            }
            else if (!this.StorageProvider.IsReadOnly)
            {
                this.lblUpdateMode.Text = Resources.UpdarteModeApproximated;
            }
            else
            {
                this.tabFunctions.TabPages.Remove(this.tabSparqlUpdate);
            }

            //Disable Import for Read-Only stores
            if (this.StorageProvider.IsReadOnly)
            {
                this.tabFunctions.TabPages.Remove(this.tabImport);
            }

            //Disable Server Management for non Storage Servers
            if (this.StorageProvider.ParentServer == null)
            {
                this.tabFunctions.TabPages.Remove(this.tabServer);
            }

            //Show Connection Information
            this.propInfo.SelectedObject = this.Connection.Information;

            //Run Startup Timer
            this._timStartup.Start();
        }

        #region Connection Management

        private void ConnectionOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            String property = propertyChangedEventArgs.PropertyName;
            if (property.Equals("IsOpen"))
            {
                // If the connection gets closed then close the form
                if (!this.Connection.IsOpen && !this._closing)
                    this.Close();
            }
            else if (property.Equals("Name"))
            {
                // If the connection is renamed update the form title
                this.Text = this.Connection.Name;
            }
        }

        /// <summary>
        /// Gets/Sets the storage provider
        /// </summary>
        private IStorageProvider StorageProvider { get; set; }

        /// <summary>
        /// Gets the connection
        /// </summary>
        public Connection Connection { get; private set; }

        /// <summary>
        /// Gets/Sets whether to force close the connection when this form closes
        /// </summary>
        private bool ForceClose { get; set; }

        #endregion

        #region Editor Options

        /// <summary>
        /// Method that should be called whenever editor options are changed so that all editors are updated
        /// </summary>
        public void ApplyEditorOptions()
        {
            this.rtbSparqlQuery.WordWrap = Settings.Default.EditorWordWrap;
            this.rtbSparqlQuery.DetectUrls = Settings.Default.EditorDetectUrls;
            if (Settings.Default.EditorHighlighting)
            {
                this.ActivateHighlighting();
            }
            else
            {
                this.ClearHighlighting();
            }
        }

        #endregion

        #region Store Operations

        /// <summary>
        /// Requests that the graphs be listed
        /// </summary>
        public void ListGraphs()
        {
            ListGraphsTask task = new ListGraphsTask(this.StorageProvider);
            this.AddTask(task, this.ListGraphsCallback);
        }

        /// <summary>
        /// Requests that the stores be listed
        /// </summary>
        public void ListStores()
        {
            ListStoresTask task = new ListStoresTask(this.StorageProvider.ParentServer);
            this.AddTask(task, this.ListStoresCallback);
        }

        /// <summary>
        /// Requests that the view of a graph be returned
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        private void ViewGraph(String graphUri)
        {
            ViewGraphTask task = new ViewGraphTask(this.StorageProvider, graphUri);
            this.AddTask(task, this.ViewGraphCallback);
        }

        /// <summary>
        /// Requests the preview of a graph
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        private void PreviewGraph(String graphUri)
        {
            PreviewGraphTask task = new PreviewGraphTask(this.StorageProvider, graphUri, Settings.Default.PreviewSize);
            this.AddTask(task, this.PreviewGraphCallback);
        }

        /// <summary>
        /// Requests the count of triples for a graph
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        private void CountTriples(String graphUri)
        {
            CountTriplesTask task = new CountTriplesTask(this.StorageProvider, graphUri);
            this.AddTask(task, this.CountTriplesCallback);
        }

        /// <summary>
        /// Requests the deletion of a graph
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        private void DeleteGraph(String graphUri)
        {
            DeleteGraphTask task = new DeleteGraphTask(this.StorageProvider, graphUri);
            this.AddTask(task, this.DeleteGraphCallback);
        }

        /// <summary>
        /// Runs a Query
        /// </summary>
        private void Query()
        {
            if (!this.StorageProvider.IsReady)
            {
                MessageBox.Show(Resources.StoreNotReady_Query_Text, Resources.StoreNotReady_Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (this.StorageProvider is IQueryableStorage)
            {
                if (this.chkPageQuery.Checked)
                {
                    QueryTask task = new QueryTask((IQueryableStorage) this.StorageProvider, this.rtbSparqlQuery.Text, (int) this.numPageSize.Value);
                    this.AddTask(task, this.QueryCallback);
                }
                else
                {
                    QueryTask task = new QueryTask((IQueryableStorage) this.StorageProvider, this.rtbSparqlQuery.Text);
                    this.AddTask(task, this.QueryCallback);
                }
            }
            else
            {
                MessageBox.Show(Resources.Query_Unsupported, Resources.Query_Error_Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Runs a GenerateEntitiesQueryTask
        /// </summary>
        public void GenerateEntitiesQuery(String query, int predicateLimitCount)
        {
            if (!this.StorageProvider.IsReady)
            {
                MessageBox.Show(Resources.StoreNotReady_Query_Text, Resources.StoreNotReady_Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (this.StorageProvider is IQueryableStorage)
            {
                GenerateEntitiesQueryTask task = new GenerateEntitiesQueryTask((IQueryableStorage) this.StorageProvider, query, predicateLimitCount);
                this.AddTask(task, this.GenerateEntitiesQueryCallback);
            }
            else
            {
                MessageBox.Show(Resources.Query_Unsupported, Resources.Query_Error_Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Runs an Update
        /// </summary>
        private void SparqlUpdate()
        {
            if (!this.StorageProvider.IsReady)
            {
                MessageBox.Show(Resources.StoreNotReady_Update_Text, Resources.StoreNotReady_Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            UpdateTask task = new UpdateTask(this.StorageProvider, this.txtSparqlUpdate.Text);
            this.AddTask(task, this.UpdateCallback);
        }

        /// <summary>
        /// Imports a File
        /// </summary>
        private void ImportFile()
        {
            if (!this.StorageProvider.IsReady)
            {
                MessageBox.Show(Resources.StoreNoteReady_Import_Text, Resources.StoreNotReady_Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (this.txtImportFile.Text.Equals(String.Empty))
            {
                MessageBox.Show(Resources.ImportData_NoFile_Text, Resources.ImportData_NoFile_Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Uri targetUri = null;
            try
            {
                if (this.chkImportDefaultUri.Checked)
                {
                    targetUri = new Uri(this.txtImportDefaultGraph.Text);
                }
            }
            catch (UriFormatException uriEx)
            {
                MessageBox.Show(string.Format(Resources.ImportData_InvalidTarget_Text, uriEx.Message), Resources.ImportData_InvalidTarget_Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ImportFileTask task = new ImportFileTask(this.StorageProvider, this.txtImportFile.Text, targetUri, (int) this.numBatchSize.Value);
            this.AddTask(task, this.ImportCallback);
        }

        /// <summary>
        /// Imports  URI
        /// </summary>
        private void ImportUri()
        {
            if (!this.StorageProvider.IsReady)
            {
                MessageBox.Show(Resources.StoreNoteReady_Import_Text, Resources.StoreNotReady_Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (this.txtImportUri.Text.Equals(String.Empty))
            {
                MessageBox.Show(Resources.ImportData_NoUri_Text, Resources.ImportData_NoUri_Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Uri targetUri = null;
            try
            {
                if (this.chkImportDefaultUri.Checked)
                {
                    targetUri = new Uri(this.txtImportDefaultGraph.Text);
                }
            }
            catch (UriFormatException uriEx)
            {
                MessageBox.Show(string.Format(Resources.ImportData_InvalidTarget_Text, uriEx.Message), Resources.ImportData_InvalidTarget_Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                ImportUriTask task = new ImportUriTask(this.StorageProvider, new Uri(this.txtImportUri.Text), targetUri, (int) this.numBatchSize.Value);
                this.AddTask(task, this.ImportCallback);
            }
            catch (UriFormatException uriEx)
            {
                MessageBox.Show(string.Format(Resources.ImportData_InvalidSource_Text, uriEx.Message), Resources.ImportData_InvalidSource_Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Exports data
        /// </summary>
        private void Export()
        {
            if (!this.StorageProvider.IsReady)
            {
                MessageBox.Show(Resources.StoreNoteReady_Import_Text, Resources.StoreNotReady_Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (this.txtExportFile.Text.Equals(String.Empty))
            {
                MessageBox.Show(Resources.ExportData_NoFile_Text, Resources.ExportData_NoFile_Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ExportTask task = new ExportTask(this.StorageProvider, this.txtExportFile.Text);
            this.AddTask(task, this.ExportCallback);
        }

        /// <summary>
        /// Copies a Graph
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="target">Target</param>
        public void CopyGraph(String graphUri, Connection target)
        {
            if (target == null) return;

            Uri source = graphUri.Equals("Default Graph") ? null : new Uri(graphUri);
            if (ReferenceEquals(this.Connection, target))
            {
                CopyMoveRenameGraphForm rename = new CopyMoveRenameGraphForm("Copy");

                if (rename.ShowDialog() == DialogResult.OK)
                {
                    CopyMoveTask task = new CopyMoveTask(this.Connection, target, source, rename.Uri, ReferenceEquals(this.Connection, target));
                    this.AddTask(task, this.CopyMoveRenameCallback);
                }
            }
            else
            {
                CopyMoveTask task = new CopyMoveTask(this.Connection, target, source, source, true);
                this.AddTask(task, this.CopyMoveRenameCallback);
            }
        }

        /// <summary>
        /// Renames a Graph
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        private void RenameGraph(String graphUri)
        {
            CopyMoveRenameGraphForm rename = new CopyMoveRenameGraphForm("Rename");
            Uri source = graphUri.Equals("Default Graph") ? null : new Uri(graphUri);
            if (rename.ShowDialog() != DialogResult.OK) return;
            CopyMoveTask task = new CopyMoveTask(this.Connection, this.Connection, source, rename.Uri, false);
            this.AddTask(task, this.CopyMoveRenameCallback);
        }

        /// <summary>
        /// Moves a Graph
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="target">Target</param>
        public void MoveGraph(String graphUri, Connection target)
        {
            if (target == null) return;

            if (ReferenceEquals(this.Connection, target))
            {
                this.RenameGraph(graphUri);
            }
            else
            {
                Uri source = graphUri.Equals("Default Graph") ? null : new Uri(graphUri);
                CopyMoveTask task = new CopyMoveTask(this.Connection, target, source, source, false);
                this.AddTask(task, this.CopyMoveRenameCallback);
            }
        }

        #region Server Operations

        /// <summary>
        /// Requests a Store be retrieved
        /// </summary>
        /// <param name="id">Store ID</param>
        public void GetStore(String id)
        {
            GetStoreTask task = new GetStoreTask(this.StorageProvider.ParentServer, id);
            this.AddTask(task, this.GetStoreCallback);
        }

        /// <summary>
        /// Requests a Store be deleted
        /// </summary>
        /// <param name="id">Store ID</param>
        public void DeleteStore(String id)
        {
            DeleteStoreTask task = new DeleteStoreTask(this.StorageProvider.ParentServer, id);
            this.AddTask(task, this.DeleteStoreCallback);
        }

        /// <summary>
        /// Requests a store be created
        /// </summary>
        /// <param name="template">Template</param>
        public void CreateStore(IStoreTemplate template)
        {
            CreateStoreTask task = new CreateStoreTask(this.StorageProvider.ParentServer, template);
            this.AddTask(task, this.CreateStoreCallback);
        }

        #endregion

        #endregion

        #region Control Event Handlers

        private void btnSparqlQuery_Click(object sender, EventArgs e)
        {
            this.Query();
        }

        private void btnGraphRefresh_Click(object sender, EventArgs e)
        {
            this.ListGraphs();
        }

        private void btnRefreshStores_Click(object sender, EventArgs e)
        {
            this.ListStores();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            this.ofdImport.Filter = MimeTypesHelper.GetFilenameFilter(true, true, false, false, false, false);
            if (this.ofdImport.ShowDialog() == DialogResult.OK)
            {
                this.txtImportFile.Text = this.ofdImport.FileName;
            }
        }

        private void btnImportFile_Click(object sender, EventArgs e)
        {
            this.ImportFile();
        }

        private void btnImportUri_Click(object sender, EventArgs e)
        {
            this.ImportUri();
        }

        private void lvwGraphs_DoubleClick(object sender, EventArgs e)
        {
            if (this.lvwGraphs.SelectedItems.Count > 0)
            {
                String graphUri = this.lvwGraphs.SelectedItems[0].Text;
                if (graphUri.Equals("Default Graph")) graphUri = null;

                this.ViewGraph(graphUri);
            }
        }

        private void lvwGraphs_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (this.lvwGraphs.SelectedItems.Count > 0)
            {
                String graphUri = this.lvwGraphs.SelectedItems[0].Text;
                CopyMoveDragInfo info = new CopyMoveDragInfo(this, graphUri);
                DragDropEffects effects = DragDropEffects.Copy;
                if (this.StorageProvider.DeleteSupported) effects = effects | DragDropEffects.Move; //Move only possible if this storage supports DeleteGraph()

                this.lvwGraphs.DoDragDrop(info, effects);
            }
        }

        private void lvwGraphs_DragEnter(object sender, DragEventArgs e)
        {
            if ((e.AllowedEffect & DragDropEffects.Copy) == 0 && (e.AllowedEffect & DragDropEffects.Move) == 0) return;
            if (!e.Data.GetDataPresent(typeof (CopyMoveDragInfo))) return;

            //Cannot Copy/Move if a read-only storage is the target
            if (this.StorageProvider.IsReadOnly) return;

            CopyMoveDragInfo info = e.Data.GetData(typeof (CopyMoveDragInfo)) as CopyMoveDragInfo;
            if (info == null) return;

            DragDropEffects effects = DragDropEffects.Copy;
            if (info.Source.StorageProvider.DeleteSupported) effects = effects | DragDropEffects.Move; //Move only possible if the source storage supports DeleteGraph()
            e.Effect = effects;
        }

        private void lvwGraphs_DragDrop(object sender, DragEventArgs e)
        {
            if ((e.AllowedEffect & DragDropEffects.Copy) != 0 || (e.AllowedEffect & DragDropEffects.Move) != 0)
            {
                if (e.Data.GetDataPresent(typeof (CopyMoveDragInfo)))
                {
                    CopyMoveDragInfo info = e.Data.GetData(typeof (CopyMoveDragInfo)) as CopyMoveDragInfo;
                    if (info == null) return;

                    //Check whether Move is permitted?
                    if ((e.Effect & DragDropEffects.Move) != 0)
                    {
                        CopyMoveDialogue copyMoveConfirm = new CopyMoveDialogue(info, this.Connection);
                        if (copyMoveConfirm.ShowDialog() == DialogResult.OK)
                        {
                            if (copyMoveConfirm.IsMove)
                            {
                                info.Form.MoveGraph(info.SourceUri, this.Connection);
                            }
                            else if (copyMoveConfirm.IsCopy)
                            {
                                info.Form.CopyGraph(info.SourceUri, this.Connection);
                            }
                        }
                    }
                    else
                    {
                        //Just do a Copy
                        info.Form.CopyGraph(info.SourceUri, this.Connection);
                    }
                }
            }
        }

        private void timStartup_Tick(object sender, EventArgs e)
        {
            if (!this.StorageProvider.IsReady) return;
            this.CrossThreadSetText(this.stsCurrent, Resources.Status_Ready);
            this.ListGraphs();
            if (this.StorageProvider.ParentServer != null)
            {
                this.ListStores();
            }
            this._timStartup.Stop();
        }

        private void btnSaveQuery_Click(object sender, EventArgs e)
        {
            this.sfdQuery.Filter = MimeTypesHelper.GetFilenameFilter(false, false, false, true, false, true);
            if (this.sfdQuery.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter writer = new StreamWriter(this.sfdQuery.FileName))
                {
                    writer.Write(this.rtbSparqlQuery.Text);
                }
            }
        }

        private void btnLoadQuery_Click(object sender, EventArgs e)
        {
            this.ofdQuery.Filter = MimeTypesHelper.GetFilenameFilter(false, false, false, true, false, true);
            if (this.ofdQuery.ShowDialog() == DialogResult.OK)
            {
                using (StreamReader reader = new StreamReader(this.ofdQuery.FileName))
                {
                    this.rtbSparqlQuery.Text = reader.ReadToEnd();
                }
            }
        }

        private void btnSparqlUpdate_Click(object sender, EventArgs e)
        {
            this.SparqlUpdate();
        }

        private void chkImportDefaultUri_CheckedChanged(object sender, EventArgs e)
        {
            this.txtImportDefaultGraph.Enabled = this.chkImportDefaultUri.Checked;
        }

        private void btnBrowseExport_Click(object sender, EventArgs e)
        {
            this.sfdExport.Filter = MimeTypesHelper.GetFilenameFilter(false, true, false, false, false, false);
            if (this.sfdExport.ShowDialog() == DialogResult.OK)
            {
                this.txtExportFile.Text = this.sfdExport.FileName;
            }
        }

        private void btnExportStore_Click(object sender, EventArgs e)
        {
            this.Export();
        }

        #region Graphs Context Menu

        private void mnuGraphs_Opening(object sender, CancelEventArgs e)
        {
            if (this.lvwGraphs.SelectedItems.Count > 0)
            {
                this.mnuDeleteGraph.Enabled = this.StorageProvider.DeleteSupported;
                this.mnuPreviewGraph.Text = String.Format("Preview first {0} Triples", Settings.Default.PreviewSize);
                this.mnuMoveGraphTo.Enabled = this.StorageProvider.DeleteSupported;
                this.mnuCopyGraph.Enabled = !this.StorageProvider.IsReadOnly;
                this.mnuRenameGraph.Enabled = !this.StorageProvider.IsReadOnly && this.StorageProvider.DeleteSupported;

                //Fill Copy To and Move To menus
                while (this.mnuCopyGraphTo.DropDownItems.Count > 2)
                {
                    this.mnuCopyGraphTo.DropDownItems.RemoveAt(2);
                }
                while (this.mnuMoveGraphTo.DropDownItems.Count > 2)
                {
                    this.mnuMoveGraphTo.DropDownItems.RemoveAt(2);
                }
                foreach (Connection connection in Program.ActiveConnections)
                {
                    if (!ReferenceEquals(connection.StorageProvider, this.StorageProvider) && !connection.StorageProvider.IsReadOnly)
                    {
                        //Copy To entry
                        ToolStripMenuItem item = new ToolStripMenuItem(connection.Name);
                        item.Tag = connection;
                        item.Click += this._copyGraphHandler;
                        this.mnuCopyGraphTo.DropDownItems.Add(item);

                        //Move To entry
                        item = new ToolStripMenuItem(connection.Name);
                        item.Tag = connection;
                        item.Click += this._moveGraphHandler;
                        this.mnuMoveGraphTo.DropDownItems.Add(item);
                    }
                }
                if (mnuCopyGraphTo.DropDownItems.Count == 2 && this.StorageProvider.IsReadOnly)
                {
                    this.mnuCopyGraphTo.Enabled = false;
                }
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void mnuViewGraph_Click(object sender, EventArgs e)
        {
            if (this.lvwGraphs.SelectedItems.Count > 0)
            {
                String graphUri = this.lvwGraphs.SelectedItems[0].Text;
                if (graphUri.Equals("Default Graph")) graphUri = null;

                this.ViewGraph(graphUri);
            }
        }

        private void mnuDeleteGraph_Click(object sender, EventArgs e)
        {
            if (this.lvwGraphs.SelectedItems.Count <= 0) return;
            String graphUri = this.lvwGraphs.SelectedItems[0].Text;
            if (MessageBox.Show(string.Format(Resources.DeleteGraph_Confirm_Text, graphUri), Resources.DeleteGraph_Confirm_Title, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            if (graphUri.Equals("Default Graph")) graphUri = null;
            this.DeleteGraph(graphUri);
        }

        private void mnuPreviewGraph_Click(object sender, EventArgs e)
        {
            if (this.lvwGraphs.Items.Count > 0)
            {
                String graphUri = this.lvwGraphs.SelectedItems[0].Text;
                if (graphUri.Equals("Default Graph")) graphUri = null;

                this.PreviewGraph(graphUri);
            }
        }

        private void mnuCountTriples_Click(object sender, EventArgs e)
        {
            if (this.lvwGraphs.Items.Count > 0)
            {
                String graphUri = this.lvwGraphs.SelectedItems[0].Text;
                if (graphUri.Equals("Default Graph")) graphUri = null;

                this.CountTriples(graphUri);
            }
        }

        private void mnuCopyGraph_Click(object sender, EventArgs e)
        {
            if (this.lvwGraphs.SelectedItems.Count > 0)
            {
                String graphUri = this.lvwGraphs.SelectedItems[0].Text;
                this.CopyGraph(graphUri, this.Connection);
            }
        }

        private void mnuRenameGraph_Click(object sender, EventArgs e)
        {
            if (this.lvwGraphs.SelectedItems.Count > 0)
            {
                String graphUri = this.lvwGraphs.SelectedItems[0].Text;
                this.RenameGraph(graphUri);
            }
        }

        private void CopyGraphClick(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item == null) return;
            if (!(item.Tag is Connection)) return;
            if (this.lvwGraphs.SelectedItems.Count > 0)
            {
                String graphUri = this.lvwGraphs.SelectedItems[0].Text;
                this.CopyGraph(graphUri, item.Tag as Connection);
            }
        }

        private void MoveGraphClick(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item == null) return;
            if (!(item.Tag is Connection)) return;
            if (this.lvwGraphs.SelectedItems.Count > 0)
            {
                String graphUri = this.lvwGraphs.SelectedItems[0].Text;
                this.MoveGraph(graphUri, item.Tag as Connection);
            }
        }

        #endregion

        #region Tasks Context Menu

        private void mnuTasks_Opening(object sender, CancelEventArgs e)
        {
            if (this.lvwTasks.SelectedItems.Count > 0)
            {
                ListViewItem item = this.lvwTasks.SelectedItems[0];
                Object tag = item.Tag;
                if (tag != null)
                {
                    if (tag is QueryTask)
                    {
                        QueryTask qTask = (QueryTask) tag;
                        this.mnuViewErrors.Enabled = qTask.Error != null;
                        this.mnuViewResults.Enabled = (qTask.State == TaskState.Completed && qTask.Result != null);
                        this.mnuCancel.Enabled = qTask.IsCancellable;
                    }
                    else if (tag is BaseImportTask)
                    {
                        BaseImportTask importTask = (BaseImportTask) tag;
                        this.mnuViewErrors.Enabled = importTask.Error != null;
                        this.mnuViewResults.Enabled = false;
                        this.mnuCancel.Enabled = importTask.IsCancellable;
                    }
                    else if (tag is ListGraphsTask)
                    {
                        ListGraphsTask graphsTask = (ListGraphsTask) tag;
                        this.mnuViewErrors.Enabled = graphsTask.Error != null;
                        this.mnuViewResults.Enabled = false;
                        this.mnuCancel.Enabled = graphsTask.IsCancellable;
                    }
                    else if (tag is ListStoresTask)
                    {
                        ListStoresTask storesTask = (ListStoresTask) tag;
                        this.mnuViewErrors.Enabled = storesTask.Error != null;
                        this.mnuViewResults.Enabled = false;
                        this.mnuCancel.Enabled = storesTask.IsCancellable;
                    }
                    else if (tag is GetStoreTask)
                    {
                        GetStoreTask getStoreTask = (GetStoreTask) tag;
                        this.mnuViewErrors.Enabled = getStoreTask.Error != null;
                        this.mnuViewResults.Enabled = false;
                        this.mnuCancel.Enabled = getStoreTask.IsCancellable;
                    }
                    else if (tag is CountTriplesTask)
                    {
                        CountTriplesTask countTask = (CountTriplesTask) tag;
                        this.mnuViewErrors.Enabled = countTask.Error != null;
                        this.mnuViewResults.Enabled = false;
                        this.mnuCancel.Enabled = countTask.IsCancellable;
                    }
                    else if (tag is GenerateEntitiesQueryTask)
                    {
                        GenerateEntitiesQueryTask genTask = (GenerateEntitiesQueryTask) tag;
                        this.mnuViewErrors.Enabled = genTask.Error != null;
                        this.mnuViewResults.Enabled = genTask.State == TaskState.Completed;
                        this.mnuCancel.Enabled = genTask.IsCancellable;
                    }
                    else if (tag is ITask<IGraph>)
                    {
                        ITask<IGraph> graphTask = (ITask<IGraph>) tag;
                        this.mnuViewErrors.Enabled = graphTask.Error != null;
                        this.mnuViewResults.Enabled = (graphTask.State == TaskState.Completed && graphTask.Result != null);
                        this.mnuCancel.Enabled = graphTask.IsCancellable;
                    }
                    else if (tag is ITask<TaskResult>)
                    {
                        ITask<TaskResult> basicTask = (ITask<TaskResult>) tag;
                        this.mnuViewErrors.Enabled = basicTask.Error != null;
                        this.mnuViewResults.Enabled = false;
                        this.mnuCancel.Enabled = basicTask.IsCancellable;
                    }
                    else if (tag is ITask<TaskValueResult<bool>>)
                    {
                        ITask<TaskValueResult<bool>> boolTask = (ITask<TaskValueResult<bool>>) tag;
                        this.mnuViewErrors.Enabled = boolTask.Error != null;
                        this.mnuViewResults.Enabled = false;
                        this.mnuCancel.Enabled = boolTask.IsCancellable;
                    }
                    else
                    {
                        e.Cancel = true;
                    }
                }
                else
                {
                    e.Cancel = true;
                }
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void mnuCancel_Click(object sender, EventArgs e)
        {
            if (this.lvwTasks.SelectedItems.Count > 0)
            {
                ListViewItem item = this.lvwTasks.SelectedItems[0];
                Object tag = item.Tag;

                if (tag is CancellableTask<TaskResult>)
                {
                    ((CancellableTask<TaskResult>) tag).Cancel();
                }
            }
        }

        private void mnuViewDetail_Click(object sender, EventArgs e)
        {
            if (this.lvwTasks.SelectedItems.Count > 0)
            {
                ListViewItem item = this.lvwTasks.SelectedItems[0];
                Object tag = item.Tag;

                if (tag is QueryTask)
                {
                    TaskInformationForm<Object> queryInfo = new TaskInformationForm<object>((QueryTask) tag, this.Connection.Name);
                    queryInfo.MdiParent = this.MdiParent;
                    queryInfo.Show();
                }
                else if (tag is UpdateTask)
                {
                    TaskInformationForm<TaskResult> updateInfo = new TaskInformationForm<TaskResult>((UpdateTask) tag, this.Connection.Name);
                    updateInfo.MdiParent = this.MdiParent;
                    updateInfo.Show();
                }
                else if (tag is ListGraphsTask)
                {
                    TaskInformationForm<IEnumerable<Uri>> listInfo = new TaskInformationForm<IEnumerable<Uri>>((ListGraphsTask) tag, this.Connection.Name);
                    listInfo.MdiParent = this.MdiParent;
                    listInfo.Show();
                }
                else if (tag is ListStoresTask)
                {
                    TaskInformationForm<IEnumerable<String>> storeInfo = new TaskInformationForm<IEnumerable<string>>((ListStoresTask) tag, this.Connection.Name);
                    storeInfo.MdiParent = this.MdiParent;
                    storeInfo.Show();
                }
                else if (tag is GetStoreTask)
                {
                    TaskInformationForm<IStorageProvider> getStoreInfo = new TaskInformationForm<IStorageProvider>((GetStoreTask) tag, this.Connection.Name);
                    getStoreInfo.MdiParent = this.MdiParent;
                    getStoreInfo.Show();
                }
                else if (tag is CountTriplesTask)
                {
                    TaskInformationForm<TaskValueResult<int>> countInfo = new TaskInformationForm<TaskValueResult<int>>((CountTriplesTask) tag, this.Connection.Name);
                    countInfo.MdiParent = this.MdiParent;
                    countInfo.Show();
                }
                else if (tag is GenerateEntitiesQueryTask)
                {
                    TaskInformationForm<String> genEntitiesQueryInfo = new TaskInformationForm<string>((GenerateEntitiesQueryTask) tag, this.Connection.Name);
                    genEntitiesQueryInfo.MdiParent = this.MdiParent;
                    genEntitiesQueryInfo.Show();
                }
                else if (tag is ITask<IGraph>)
                {
                    TaskInformationForm<IGraph> graphInfo = new TaskInformationForm<IGraph>((ITask<IGraph>) tag, this.Connection.Name);
                    graphInfo.MdiParent = this.MdiParent;
                    graphInfo.Show();
                }
                else if (tag is ITask<TaskResult>)
                {
                    TaskInformationForm<TaskResult> simpleInfo = new TaskInformationForm<TaskResult>((ITask<TaskResult>) tag, this.Connection.Name);
                    simpleInfo.MdiParent = this.MdiParent;
                    simpleInfo.Show();
                }
                else if (tag is ITask<TaskValueResult<bool>>)
                {
                    TaskInformationForm<TaskValueResult<bool>> boolInfo = new TaskInformationForm<TaskValueResult<bool>>((ITask<TaskValueResult<bool>>) tag, this.Connection.Name);
                    boolInfo.MdiParent = this.MdiParent;
                    boolInfo.Show();
                }
                else
                {
                    MessageBox.Show(Resources.TaskInfo_Unavailable_Text, Resources.TaskInfo_Unavailable_Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void mnuViewErrors_Click(object sender, EventArgs e)
        {
            if (this.lvwTasks.SelectedItems.Count > 0)
            {
                ListViewItem item = this.lvwTasks.SelectedItems[0];
                Object tag = item.Tag;

                if (tag is QueryTask)
                {
                    TaskErrorTraceForm<Object> queryInfo = new TaskErrorTraceForm<object>((ITask<Object>) tag, this.Connection.Name);
                    queryInfo.MdiParent = this.MdiParent;
                    queryInfo.Show();
                }
                else if (tag is ListGraphsTask)
                {
                    TaskErrorTraceForm<IEnumerable<Uri>> listInfo = new TaskErrorTraceForm<IEnumerable<Uri>>((ITask<IEnumerable<Uri>>) tag, this.Connection.Name);
                    listInfo.MdiParent = this.MdiParent;
                    listInfo.Show();
                }
                else if (tag is ListStoresTask)
                {
                    TaskErrorTraceForm<IEnumerable<String>> storeInfo = new TaskErrorTraceForm<IEnumerable<string>>((ListStoresTask) tag, this.Connection.Name);
                    storeInfo.MdiParent = this.MdiParent;
                    storeInfo.Show();
                }
                else if (tag is GetStoreTask)
                {
                    TaskErrorTraceForm<IStorageProvider> getStoreInfo = new TaskErrorTraceForm<IStorageProvider>((GetStoreTask) tag, this.Connection.Name);
                    getStoreInfo.MdiParent = this.MdiParent;
                    getStoreInfo.Show();
                }
                else if (tag is ITask<IGraph>)
                {
                    TaskErrorTraceForm<IGraph> graphInfo = new TaskErrorTraceForm<IGraph>((ITask<IGraph>) tag, this.Connection.Name);
                    graphInfo.MdiParent = this.MdiParent;
                    graphInfo.Show();
                }
                else if (tag is ITask<TaskResult>)
                {
                    TaskErrorTraceForm<TaskResult> simpleInfo = new TaskErrorTraceForm<TaskResult>((ITask<TaskResult>) tag, this.Connection.Name);
                    simpleInfo.MdiParent = this.MdiParent;
                    simpleInfo.Show();
                }
                else if (tag is ITask<TaskValueResult<bool>>)
                {
                    TaskErrorTraceForm<TaskValueResult<bool>> boolInfo = new TaskErrorTraceForm<TaskValueResult<bool>>((ITask<TaskValueResult<bool>>) tag, this.Connection.Name);
                    boolInfo.MdiParent = this.MdiParent;
                    boolInfo.Show();
                }
                else if (tag is ITask<String>)
                {
                    TaskErrorTraceForm<String> stringInfo = new TaskErrorTraceForm<string>((ITask<String>) tag, this.Connection.Name);
                    stringInfo.MdiParent = this.MdiParent;
                    stringInfo.Show();
                }
                else
                {
                    MessageBox.Show(Resources.TaskInfo_Unavailable_Text, Resources.TaskInfo_Unavailable_Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void mnuViewResults_Click(object sender, EventArgs e)
        {
            if (this.lvwTasks.SelectedItems.Count <= 0) return;
            ListViewItem item = this.lvwTasks.SelectedItems[0];
            Object tag = item.Tag;

            if (tag is QueryTask)
            {
                QueryTask qTask = (QueryTask) tag;
                if (qTask.State == TaskState.Completed && qTask.Result != null)
                {
                    Object result = qTask.Result;

                    if (result is IGraph)
                    {
                        GraphViewerForm graphViewer = new GraphViewerForm((IGraph) result, this.Connection.Name);
                        CrossThreadSetMdiParent(graphViewer);
                        CrossThreadShow(graphViewer);
                    }
                    else if (result is SparqlResultSet)
                    {
                        ResultSetViewerForm resultsViewer = new ResultSetViewerForm((SparqlResultSet) result, qTask.Query != null ? qTask.Query.NamespaceMap : null, this.Connection.Name);
                        CrossThreadSetMdiParent(resultsViewer);
                        CrossThreadShow(resultsViewer);
                    }
                    else
                    {
                        CrossThreadMessage(Resources.QueryResults_NotViewable_Text, Resources.QueryResults_NotViewable_Title, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    CrossThreadMessage(Resources.QueryResults_Unavailable_Text, Resources.QueryResults_Unavailable_Title, MessageBoxIcon.Error);
                }
            }
            else if (tag is GenerateEntitiesQueryTask)
            {
                GenerateEntitiesQueryTask genTask = (GenerateEntitiesQueryTask) tag;
                StringResultDialogue dialogue = new StringResultDialogue(string.Format("Generated Entity Query on {0}", this.Connection.Name), genTask.Result, this.rtbSparqlQuery, "Query Editor");
                CrossThreadSetMdiParent(dialogue);
                CrossThreadShow(dialogue);
            }
            else if (tag is ITask<IGraph>)
            {
                ITask<IGraph> graphTask = (ITask<IGraph>) tag;
                if (graphTask.Result != null)
                {
                    GraphViewerForm graphViewer = new GraphViewerForm(graphTask.Result, this.Connection.Name);
                    CrossThreadSetMdiParent(graphViewer);
                    CrossThreadShow(graphViewer);
                }
                else
                {
                    CrossThreadMessage(Resources.Graph_Unavailable_Text, Resources.Graph_Unavailable_Title, MessageBoxIcon.Error);
                }
            }
            else if (tag is ITask<String>)
            {
            }
        }

        #endregion

        #region Stores Context Menu

        private void mnuStores_Opening(object sender, CancelEventArgs e)
        {
            IStorageServer server = this.StorageProvider.ParentServer;
            if (server == null)
            {
                e.Cancel = true;
                return;
            }

            this.mnuNewStore.Enabled = (server.IOBehaviour & IOBehaviour.CanCreateStores) != 0;
            if (this.lvwStores.SelectedItems.Count > 0)
            {
                this.mnuOpenStore.Enabled = true;
                this.mnuDeleteStore.Enabled = (server.IOBehaviour & IOBehaviour.CanDeleteStores) != 0;
            }
            else
            {
                this.mnuOpenStore.Enabled = false;
                this.mnuDeleteStore.Enabled = false;
            }
        }

        private void mnuNewStore_Click(object sender, EventArgs e)
        {
            NewStoreForm newStore = new NewStoreForm(this.StorageProvider.ParentServer);
            if (newStore.ShowDialog() == DialogResult.OK)
            {
                this.CreateStore(newStore.Template);
            }
        }

        private void mnuOpenStore_Click(object sender, EventArgs e)
        {
            if (this.lvwStores.SelectedItems.Count > 0)
            {
                String id = this.lvwStores.SelectedItems[0].Text;
                this.GetStore(id);
            }
            else
            {
                MessageBox.Show(Resources.OpenStore_Error_Text, Resources.OpenStore_Error_Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void mnuDeleteStore_Click(object sender, EventArgs e)
        {
            if (this.lvwStores.SelectedItems.Count > 0)
            {
                String id = this.lvwStores.SelectedItems[0].Text;
                if (MessageBox.Show(string.Format(Resources.DeleteStore_Confirm_Text, id), Resources.DeleteStore_Confirm_Title, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    this.DeleteStore(id);
                }
            }
        }

        #endregion

        #endregion

        #region Task Management

        private void AddTask<T>(ITask<T> task, TaskCallback<T> callback) where T : class
        {
            String[] items = new String[]
                {
                    (++this._taskId).ToString(System.Globalization.CultureInfo.CurrentUICulture),
                    task.Name,
                    task.State.GetStateDescription(),
                    task.Information
                };
            ListViewItem item = new ListViewItem(items);
            item.Tag = task;
            CrossThreadAddItem(this.lvwTasks, item);

            //Ensure that the Task Information gets updated automatically when the Task State changes
            TaskStateChanged d = delegate
                {
                    CrossThreadAlterSubItem(item, 2, task.State.GetStateDescription());
                    CrossThreadAlterSubItem(item, 3, task.Information);
                    CrossThreadRefresh(this.lvwTasks);
                };
            task.StateChanged += d;

            //Clear old Tasks if necessary and enabled
            if (this.chkRemoveOldTasks.Checked)
            {
                if (this.lvwTasks.Items.Count > 10)
                {
                    int i = this.lvwTasks.Items.Count - 1;
                    do
                    {
                        ListViewItem oldItem = this.lvwTasks.Items[i];
                        if (oldItem.Tag is ITaskBase)
                        {
                            ITaskBase t = (ITaskBase) oldItem.Tag;
                            if (t.State == TaskState.Completed || t.State == TaskState.CompletedWithErrors)
                            {
                                this.lvwTasks.Items.RemoveAt(i);
                                i--;
                            }
                        }

                        i--;
                    } while (this.lvwTasks.Items.Count > 10 && i >= 0);
                }
            }

            //Start the Task
            task.RunTask(callback);
        }

        private void ListGraphsCallback(ITask<IEnumerable<Uri>> task)
        {
            if (task.State == TaskState.Completed && task.Result != null)
            {
                this.CrossThreadSetText(this.stsCurrent, Resources.Status_RenderingGraphList);
                this.CrossThreadSetVisibility(this.lvwGraphs, true);
                this.CrossThreadBeginUpdate(this.lvwGraphs);
                this.CrossThreadClear(this.lvwGraphs);

                foreach (Uri u in task.Result)
                {
                    this.CrossThreadAdd(this.lvwGraphs, u != null ? u.AbsoluteUri : "Default Graph");
                }

                this.CrossThreadEndUpdate(this.lvwGraphs);

                this.CrossThreadSetText(this.stsCurrent, Resources.Status_Ready);
                this.CrossThreadSetEnabled(this.btnGraphRefresh, true);

                task.Information = string.Format(Resources.ListGraphs_Information, this.lvwGraphs.Items.Count);
            }
            else
            {
                this.CrossThreadSetText(this.stsCurrent, Resources.Status_GraphListingUnavailable);
                if (task.Error != null)
                {
                    CrossThreadMessage(string.Format(Resources.ListGraphs_Error_Text, task.Error.Message), Resources.ListGraphs_Error_Title, MessageBoxIcon.Warning);
                }
                this.CrossThreadSetVisibility(this.lvwGraphs, false);
                this.CrossThreadSetVisibility(this.lblGraphListUnavailable, true);
                this.CrossThreadRefresh(this.tabGraphs);
            }
        }

        private void ListStoresCallback(ITask<IEnumerable<String>> task)
        {
            if (task.State == TaskState.Completed && task.Result != null)
            {
                this.CrossThreadSetText(this.stsCurrent, Resources.Status_RenderingStoreList);
                this.CrossThreadSetVisibility(this.lvwStores, true);
                this.CrossThreadBeginUpdate(this.lvwStores);
                this.CrossThreadClear(this.lvwStores);

                foreach (String id in task.Result)
                {
                    if (id != null)
                    {
                        this.CrossThreadAdd(this.lvwStores, id);
                    }
                }

                this.CrossThreadEndUpdate(this.lvwStores);

                this.CrossThreadSetText(this.stsCurrent, Resources.Status_Ready);
                this.CrossThreadSetEnabled(this.btnRefreshStores, true);

                task.Information = string.Format(Resources.ListStores_Information, this.lvwStores.Items.Count);
            }
            else
            {
                this.CrossThreadSetText(this.stsCurrent, Resources.Status_StoreListUnavailable);
                if (task.Error != null)
                {
                    CrossThreadMessage(string.Format(Resources.ListStores_Error_Text, task.Error.Message), Resources.ListStores_Error_Title, MessageBoxIcon.Warning);
                }
                this.CrossThreadRefresh(this.tabServer);
            }
        }

        private void ViewGraphCallback(ITask<IGraph> task)
        {
            if (task.State == TaskState.Completed && task.Result != null)
            {
                GraphViewerForm graphViewer = new GraphViewerForm(task.Result, this.Connection.Name);
                CrossThreadSetMdiParent(graphViewer);
                CrossThreadShow(graphViewer);
            }
            else
            {
                if (task.Error != null)
                {
                    CrossThreadMessage(string.Format(Resources.ViewGraph_Error_Text, task.Error.Message), Resources.ViewGraph_Error_Title, MessageBoxIcon.Error);
                }
                else
                {
                    CrossThreadMessage(Resources.ViewGraph_UnknownError_Text, Resources.ViewGraph_Error_Title, MessageBoxIcon.Error);
                }
            }
        }

        private void PreviewGraphCallback(ITask<IGraph> task)
        {
            if (task.State == TaskState.Completed && task.Result != null)
            {
                GraphViewerForm graphViewer = new GraphViewerForm(task.Result, this.Connection.Name);
                CrossThreadSetMdiParent(graphViewer);
                CrossThreadShow(graphViewer);
            }
            else
            {
                if (task.Error != null)
                {
                    CrossThreadMessage("Preview Graph Failed due to the following error: " + task.Error.Message, "Preview Graph Failed", MessageBoxIcon.Error);
                }
                else
                {
                    CrossThreadMessage("Preview Graph Failed due to an unknown error", "Preview Graph Failed", MessageBoxIcon.Error);
                }
            }
        }

        private void CountTriplesCallback(ITask<TaskValueResult<int>> task)
        {
            if (task.State == TaskState.Completed && task.Result != null)
            {
                CrossThreadMessage(task.Information, Resources.TriplesCounted, MessageBoxIcon.Information);
            }
            else
            {
                if (task.Error != null)
                {
                    CrossThreadMessage("Count Triples Failed due to the following error: " + task.Error.Message, "Count Triples Failed", MessageBoxIcon.Error);
                }
                else
                {
                    CrossThreadMessage("Count Triples Failed due to an unknown error", "Count Triples Failed", MessageBoxIcon.Error);
                }
            }
        }

        private void DeleteGraphCallback(ITask<TaskResult> task)
        {
            if (task.State == TaskState.Completed)
            {
                CrossThreadMessage(task.Information, "Deleted Graph OK", MessageBoxIcon.Information);
                ListGraphs();
            }
            else
            {
                if (task.Error != null)
                {
                    CrossThreadMessage("Delete Graph Failed due to the following error: " + task.Error.Message, "Delete Graph Failed", MessageBoxIcon.Error);
                }
                else
                {
                    CrossThreadMessage("Delete Graph Failed due to an unknown error", "Delete Graph Failed", MessageBoxIcon.Error);
                }
            }
        }

        private void QueryCallback(ITask<Object> task)
        {
            QueryTask qTask;
            if (task is QueryTask)
            {
                qTask = (QueryTask) task;
                if (qTask.Query != null)
                {
                    try
                    {
                        if (task.State == TaskState.Completed)
                        {
                            this.CrossThreadSetText(this.stsCurrent, "Query Completed OK (Took " + qTask.Query.QueryExecutionTime.Value.ToString() + ")");
                        }
                        else
                        {
                            this.CrossThreadSetText(this.stsCurrent, "Query Failed (Took " + qTask.Query.QueryExecutionTime.Value.ToString() + ")");
                        }
                    }
                    catch
                    {
                        this.CrossThreadSetText(this.stsCurrent, qTask.State == TaskState.Completed ? "Query Completed OK" : "Query Failed");
                    }
                }
            }
            else
            {
                CrossThreadMessage("Unexpected Error - QueryCallback was invoked but the given task was not a QueryTask", "Unexpected Error", MessageBoxIcon.Exclamation);
                return;
            }

            if (task.State == TaskState.Completed)
            {
                Object result = task.Result;

                if (result is IGraph)
                {
                    CrossThreadShowQueryPanel(splitQueryResults);
                    this.DisplayQueryResults(qTask);
                }
                else if (result is SparqlResultSet)
                {
                    CrossThreadShowQueryPanel(splitQueryResults);
                    this.DisplayQueryResults(qTask);
                }
                else
                {
                    CrossThreadMessage("Unable to show Query Results as did not get a Graph/Result Set as expected", "Unable to Show Results", MessageBoxIcon.Error);
                }
            }
            else
            {
                if (task.Error != null)
                {
                    CrossThreadMessage("Query Failed due to the following error: " + task.Error.Message, "Query Failed", MessageBoxIcon.Error);
                }
                else
                {
                    CrossThreadMessage("Query Failed due to an unknown error", "Query Failed", MessageBoxIcon.Error);
                }
            }
        }

        private delegate void DisplayQueryResultsDelegate(QueryTask task);

        private void DisplayQueryResults(QueryTask task)
        {
            if (this.InvokeRequired)
            {
                DisplayQueryResultsDelegate d = this.DisplayQueryResults;
                this.Invoke(d, new object[] {task});
            }
            else
            {
                TabPage tabPage = new TabPage();
                tabPage.Text = "Query " + ++this._queryId;
                QueryResultsControl control = new QueryResultsControl();
                control.Namespaces = task.Query != null ? task.Query.NamespaceMap : null;
                control.QueryString = task.QueryString;
                control.DataSource = task.Result;
                control.CloseRequested += delegate
                    {
                        this.tabResults.TabPages.Remove(tabPage);
                        if (this.tabResults.TabPages.Count == 0) this.splitQueryResults.Panel2Collapsed = true;
                    };
                control.DetachRequested += delegate
                    {
                        if (control.DataSource is SparqlResultSet)
                        {
                            ResultSetViewerForm resultsViewer = new ResultSetViewerForm((SparqlResultSet) control.DataSource, control.Namespaces, this.Connection.Name);
                            CrossThreadSetMdiParent(resultsViewer);
                            CrossThreadShow(resultsViewer);
                        }
                        else if (control.DataSource is IGraph)
                        {
                            GraphViewerForm graphViewer = new GraphViewerForm((IGraph)control.DataSource, this.Connection.Name);
                            CrossThreadSetMdiParent(graphViewer);
                            CrossThreadShow(graphViewer);
                        }
                        this.tabResults.TabPages.Remove(tabPage);
                        if (this.tabResults.TabPages.Count == 0) this.splitQueryResults.Panel2Collapsed = true;
                    };
                tabPage.SuspendLayout();
                tabPage.Controls.Add(control);
                this.tabResults.TabPages.Add(tabPage);
                this.tabResults.SelectTab(tabPage);
                tabPage.ResumeLayout();

                // Try and get control to take up all available space
                control.SuspendLayout();
                control.Anchor = AnchorStyles.Bottom |
                                 AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                control.Height = tabPage.Height;
                control.Width = tabPage.Width;
                control.ResumeLayout();
                
            }
        }

        private void GenerateEntitiesQueryCallback(ITask<String> task)
        {
            if (task.State == TaskState.Completed)
            {
                StringResultDialogue dialogue = new StringResultDialogue(string.Format("Generated Entity Query on {0}", this.Connection.Name), task.Result, this.rtbSparqlQuery, "Query Editor");
                CrossThreadSetMdiParent(dialogue);
                CrossThreadShow(dialogue);
            }
            else
            {
                if (task.Error != null)
                {
                    CrossThreadMessage("Generating an entities query failed due to the following error: " + task.Error.Message, "Generate Entities Query Failed", MessageBoxIcon.Error);
                }
                else
                {
                    CrossThreadMessage("Generating an entities query failed due to an unknown error", "Generate Entities Query Failed", MessageBoxIcon.Error);
                }
            }
        }

        private void UpdateCallback(ITask<TaskResult> task)
        {
            if (task is UpdateTask)
            {
                UpdateTask uTask = (UpdateTask) task;
                if (uTask.Updates != null)
                {
                    try
                    {
                        if (task.State == TaskState.Completed)
                        {
                            this.CrossThreadSetText(this.stsCurrent, "Updates Completed OK (Took " + uTask.Updates.UpdateExecutionTime.Value.ToString() + ")");
                        }
                        else
                        {
                            this.CrossThreadSetText(this.stsCurrent, "Updates Failed (Took " + uTask.Updates.UpdateExecutionTime.Value.ToString() + ")");
                        }
                    }
                    catch
                    {
                        this.CrossThreadSetText(this.stsCurrent, uTask.State == TaskState.Completed ? "Updates Completed OK" : "Updates Failed");
                    }
                }
            }

            if (task.State == TaskState.Completed)
            {
                CrossThreadMessage("Updates Completed successfully", "Updates Completed", MessageBoxIcon.Information);
            }
            else
            {
                if (task.Error != null)
                {
                    CrossThreadMessage("Updates Failed due to the following error: " + task.Error.Message, "Updates Failed", MessageBoxIcon.Error);
                }
                else
                {
                    CrossThreadMessage("Updates Failed due to an unknown error", "Updates Failed", MessageBoxIcon.Error);
                }
            }
        }

        private void ImportCallback(ITask<TaskResult> task)
        {
            if (task.State == TaskState.Completed)
            {
                CrossThreadMessage("Import Completed OK\n" + task.Information, "Import Completed", MessageBoxIcon.Information);
            }
            else
            {
                if (task.Error != null)
                {
                    CrossThreadMessage("Import Failed due to the following error: " + task.Error.Message, "Import Failed", MessageBoxIcon.Error);
                }
                else
                {
                    CrossThreadMessage("Import Failed due to an unknown error", "Import Failed", MessageBoxIcon.Error);
                }
            }
            this.ListGraphs();
        }

        private void ExportCallback(ITask<TaskResult> task)
        {
            if (task.State == TaskState.Completed)
            {
                CrossThreadMessage("Export Completed OK - " + task.Information, "Export Completed", MessageBoxIcon.Information);
            }
            else
            {
                if (task.Error != null)
                {
                    CrossThreadMessage("Export Failed due to the following error: " + task.Error.Message, "Export Failed", MessageBoxIcon.Error);
                }
                else
                {
                    CrossThreadMessage("Export Failed due to an unknown error", "Export Failed", MessageBoxIcon.Error);
                }
            }
        }

        private void CopyMoveRenameCallback(ITask<TaskResult> task)
        {
            if (task.State == TaskState.Completed)
            {
                CrossThreadMessage(task.Name + " Completed OK - " + task.Information, task.Name + " Completed", MessageBoxIcon.Information);
                this.ListGraphs();

                if (task is CopyMoveTask)
                {
                    CopyMoveTask cmTask = (CopyMoveTask) task;
                    if (!ReferenceEquals(this.Connection, cmTask.Target))
                    {
                        foreach (StoreManagerForm managerForm in Program.MainForm.MdiChildren.OfType<StoreManagerForm>())
                        {
                            if (ReferenceEquals(this, managerForm) || !ReferenceEquals(cmTask.Target, managerForm.Connection)) continue;
                            managerForm.ListGraphs();
                            break;
                        }
                    }
                }
            }
            else
            {
                if (task.Error != null)
                {
                    CrossThreadMessage(task.Name + " Failed due to the following error: " + task.Error.Message, task.Name + " Failed", MessageBoxIcon.Error);
                }
                else
                {
                    CrossThreadMessage(task.Name + " Failed due to an unknown error", task.Name + " Failed", MessageBoxIcon.Error);
                }
            }
        }

        private void GetStoreCallback(ITask<IStorageProvider> task)
        {
            if (task.State == TaskState.Completed)
            {
                // TODO Need a better way to modify the connection definition appropriately to set the relevant Store ID - currently this is done via a hack in the Connection constructor
                IConnectionDefinition definition = this.Connection.Definition.Copy();
                Connection connection = new Connection(definition, task.Result);
                Program.MainForm.ShowStoreManagerForm(connection);
            }
            else
            {
                if (task.Error != null)
                {
                    CrossThreadMessage(task.Name + " Failed due to the following error: " + task.Error.Message, task.Name + " Failed", MessageBoxIcon.Error);
                }
                else
                {
                    CrossThreadMessage(task.Name + " Failed due to an unknown error", task.Name + " Failed", MessageBoxIcon.Error);
                }
            }
        }

        private void DeleteStoreCallback(ITask<TaskResult> task)
        {
            if (task.State == TaskState.Completed)
            {
                CrossThreadMessage(task.Name + " Completed OK - " + task.Information, task.Name + " Completed", MessageBoxIcon.Information);
                this.ListStores();
            }
            else
            {
                if (task.Error != null)
                {
                    CrossThreadMessage(task.Name + " Failed due to the following error: " + task.Error.Message, task.Name + " Failed", MessageBoxIcon.Error);
                }
                else
                {
                    CrossThreadMessage(task.Name + " Failed due to an unknown error", task.Name + " Failed", MessageBoxIcon.Error);
                }
            }
        }

        private void CreateStoreCallback(ITask<TaskValueResult<bool>> task)
        {
            if (task.State == TaskState.Completed)
            {
                if (task.Result.Value == true)
                {
                    CrossThreadMessage(task.Name + " Completed OK - " + task.Information, task.Name + " Completed", MessageBoxIcon.Information);
                    this.ListStores();
                }
                else
                {
                    CrossThreadMessage(task.Name + " Failed - Underlying Server returned that a Store was not created", task.Name + " Failed", MessageBoxIcon.Warning);
                }
            }
            else
            {
                if (task.Error != null)
                {
                    CrossThreadMessage(task.Name + " Failed due to the following error: " + task.Error.Message, task.Name + " Failed", MessageBoxIcon.Error);
                }
                else
                {
                    CrossThreadMessage(task.Name + " Failed due to an unknown error", task.Name + " Failed", MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            if (this.Connection.ActiveUsers > 1)
            {
                CloseConnectionDialogue closeConnection = new CloseConnectionDialogue();
                if (closeConnection.ShowDialog() == DialogResult.Cancel)
                {
                    cancelEventArgs.Cancel = true;
                    return;
                }
                this.ForceClose = closeConnection.ForceClose;
            }
            else
            {
                this.ForceClose = false;
            }
            this._closing = true;
        }

        protected override void OnClosed(EventArgs e)
        {
            this.Connection.Close(this.ForceClose);
            base.OnClosed(e);
        }

        private void btnChangeOrientation_Click(object sender, EventArgs e)
        {
            if (this.splitQueryResults.Orientation == Orientation.Horizontal)
            {
                this.splitQueryResults.Orientation = Orientation.Vertical;
            }
            else
            {
                this.splitQueryResults.Orientation = Orientation.Horizontal;
            }
        }

        private void btnOpenEntityGeneratorForm_Click(object sender, EventArgs e)
        {
            EntityQueryGeneratorDialogue queryGeneratorDialogue = new EntityQueryGeneratorDialogue(this.rtbSparqlQuery.Text);
            if (queryGeneratorDialogue.ShowDialog() == DialogResult.OK)
            {
                this.GenerateEntitiesQuery(queryGeneratorDialogue.QueryString, queryGeneratorDialogue.MinPredicateUsageLimit);
            }
        }

        private void btnFormatQuery_Click(object sender, EventArgs e)
        {
            var rtb = this.rtbSparqlQuery;

            if (_codeFormatInProgress) return;
            try
            {
                rtb.Text = ReformatText(rtb.Text);
            }
            finally
            {
                _codeFormatInProgress = false;
            }
        }

        private string ReformatText(string text)
        {
            _codeFormatInProgress = true;
            string[] currentText = Regex.Split(text, "\r?\n");

            int lvl = 0;
            string newString = "";
            bool lineAdded = false;
            foreach (string line in currentText)
            {
                if (line.Contains("{"))
                {
                    newString += ApplyIndentation(lvl) + line.TrimStart(' ') + "\r\n";
                    lineAdded = true;
                    lvl += line.Count(f => f == '{');
                }
                if (line.Contains("}"))
                {
                    lvl -= line.Count(f => f == '}');
                    if (!lineAdded)
                    {
                        newString += ApplyIndentation(lvl) + line.TrimStart(' ') + "\r\n";
                        lineAdded = true;
                    }
                }

                if (!lineAdded)
                {
                    newString += ApplyIndentation(lvl) + line.TrimStart(' ') + "\r\n";
                }

                lineAdded = false;
            }

            return newString.TrimEnd('\n').TrimEnd('\r');
        }

        private static string ApplyIndentation(int indentLevel)
        {
            string space = "";
            if (indentLevel > 0)
            {
                for (int lvl = 0; lvl < indentLevel; lvl++)
                {
                    space += " ".PadLeft(2);
                }
            }

            return space;
        }

        private void rtbSparqlQuery_TextChanged(object sender, EventArgs e)
        {
            // Reset timer
            _highlightsUpdateTimer.Stop();
            if (!_codeHighLightingInProgress)
            {
                _highlightsUpdateTimer.Start();
            }
        }

        private void HighlightsUpdateTimerOnTick(object sender, EventArgs eventArgs)
        {
            _highlightsUpdateTimer.Stop();
            ActivateHighlighting();
        }

        private void ActivateHighlighting()
        {
            if (_codeHighLightingInProgress) return;
            _codeHighLightingInProgress = true;

            rtbSparqlQuery.BeginUpdate();
            int initialSelectionStart = rtbSparqlQuery.SelectionStart;
            ClearHighlighting();
            HighlightText("prefix", Color.DarkBlue);

            HighlightText("select", Color.Blue);
            HighlightText("FROM", Color.Blue);
            HighlightText("FROM NAMED", Color.Blue);
            HighlightText("GRAPH", Color.Blue);

            HighlightText("describe", Color.Blue);
            HighlightText("ask", Color.Blue);
            HighlightText("construct", Color.Blue);

            HighlightText("where", Color.Blue);
            HighlightText("filter", Color.Blue);
            HighlightText("distinct", Color.Blue);
            HighlightText("optional", Color.Blue);

            HighlightText("order by", Color.Blue);
            HighlightText("limit", Color.Blue);
            HighlightText("offset", Color.Blue);
            HighlightText("REDUCED", Color.Blue);


            HighlightText("GROUP BY", Color.Blue);
            HighlightText("HAVING", Color.Blue);

            rtbSparqlQuery.SelectionStart = initialSelectionStart;
            rtbSparqlQuery.SelectionLength = 0;
            rtbSparqlQuery.EndUpdate();
            rtbSparqlQuery.Invalidate();
            _codeHighLightingInProgress = false;
        }

        private void HighlightText(string text, Color color)
        {
            if (text.Length <= 0) return;
            int startPosition = 0;
            int foundPosition = 0;
            while (foundPosition > -1)
            {
                foundPosition = rtbSparqlQuery.Find(text, startPosition, RichTextBoxFinds.WholeWord);
                if (foundPosition < 0) continue;
                rtbSparqlQuery.Select(foundPosition, text.Length);
                rtbSparqlQuery.SelectionColor = color;
                startPosition = foundPosition + text.Length;
                _highLights.Add(new HighLight() {Start = foundPosition, End = text.Length});
            }
        }

        private void ClearHighlighting()
        {
            rtbSparqlQuery.Select(0, rtbSparqlQuery.TextLength - 1);
            rtbSparqlQuery.SelectionColor = rtbSparqlQuery.ForeColor;
            _highLights.Clear();
        }
    }

    internal class HighLight
    {
        public int Start { get; set; }
        public int End { get; set; }
    }

    /// <summary>
    /// Comparer for sorting tasks in ascending order by their IDs
    /// </summary>
    internal class SortTasksById
        : IComparer, IComparer<ListViewItem>
    {
        /// <summary>
        /// Compares two tasks
        /// </summary>
        /// <param name="x">Task</param>
        /// <param name="y">Task</param>
        /// <returns></returns>
        public int Compare(ListViewItem x, ListViewItem y)
        {
            int a;
            if (!Int32.TryParse(x.SubItems[0].Text, out a)) return -1;
            int b;
            if (Int32.TryParse(y.SubItems[0].Text, out b)) return -1*a.CompareTo(b);
            return 1;
        }

        #region IComparer Members

        /// <summary>
        /// Compares two tasks
        /// </summary>
        /// <param name="x">Task</param>
        /// <param name="y">Task</param>
        /// <returns></returns>
        public int Compare(object x, object y)
        {
            if (x is ListViewItem && y is ListViewItem)
            {
                return this.Compare((ListViewItem) x, (ListViewItem) y);
            }
            return 0;
        }

        #endregion
    }
}