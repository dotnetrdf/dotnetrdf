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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using VDS.RDF;
using VDS.RDF.GUI;
using VDS.RDF.GUI.WinForms;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Params;
using VDS.RDF.Update;
using VDS.RDF.Writing;
using VDS.RDF.Utilities.StoreManager.Tasks;

namespace VDS.RDF.Utilities.StoreManager
{
    public partial class fclsGenericStoreManager : CrossThreadForm
    {
        private IGenericIOManager _manager;
        private int _taskID = 1;

        public fclsGenericStoreManager(IGenericIOManager manager)
        {
            InitializeComponent();

            this._manager = manager;
            this.Text = this._manager.ToString();
        }

        public IGenericIOManager Manager
        {
            get
            {
                return this._manager;
            }
        }

        private void fclsGenericStoreManager_Load(object sender, EventArgs e)
        {
            //Determine whether SPARQL Query is supported
            if (!(this._manager is IQueryableGenericIOManager))
            {
                this.tabFunctions.TabPages.Remove(this.tabSparqlQuery);
            }

            //Determine what SPARQL Update mode if any is supported
            if (this._manager is IUpdateableGenericIOManager)
            {
                this.lblUpdateMode.Text = "Update Mode: Native";
            }
            else if (!this._manager.IsReadOnly)
            {
                this.lblUpdateMode.Text = "Update Mode: Approximated";
            }
            else
            {
                this.tabFunctions.TabPages.Remove(this.tabSparqlUpdate);
            }

            //Disable Import for Read-Only stores
            if (this._manager.IsReadOnly)
            {
                this.grpImport.Enabled = false;
            }
        }

        #region Store Operations

        private void ListGraphs()
        {
            ListGraphsTasks task = new ListGraphsTasks(this._manager);
            this.AddTask<IEnumerable<Uri>>(task, this.ListGraphsCallback);
        }

        private void Query()
        {
            if (!this._manager.IsReady)
            {
                MessageBox.Show("Please wait for Store to be ready before attempting to make a SPARQL Query", "Store Not Ready", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (this._manager is IQueryableGenericIOManager)
            {
                QueryTask task = new QueryTask((IQueryableGenericIOManager)this._manager, this.txtSparqlQuery.Text);
                this.AddTask<Object>(task, this.QueryCallback);
            }
            else
            {
                MessageBox.Show("Unable to execute a SPARQL Query since your Store does not support SPARQL", "SPARQL Query Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        private void btnSparqlQuery_Click(object sender, EventArgs e)
        {
            this.Query();
        }

        private void btnGraphRefresh_Click(object sender, EventArgs e)
        {
            this.ListGraphs();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            this.ofdImport.Filter = Constants.RdfOrDatasetFilter;
            if (this.ofdImport.ShowDialog() == DialogResult.OK)
            {
                this.txtImportFile.Text = this.ofdImport.FileName;
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

                    this._manager.SaveGraph(g);

                    MessageBox.Show("Successfully imported a Graph into the Store", "Import Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    this.ListGraphs();
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

                        this.ListGraphs();
                    }
                    catch (RdfParserSelectionException)
                    {
                        MessageBox.Show("Unable to perform an import as the format of the file could not be determined", "Import Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void btnImportUri_Click(object sender, EventArgs e)
        {
            if (!this.txtImportUri.Text.Equals(String.Empty))
            {
                try
                {
                    Graph g = new Graph();
                    UriLoader.Load(g, new Uri(this.txtImportUri.Text));

                    this._manager.SaveGraph(g);

                    MessageBox.Show("Successfully imported a Graph into the Store", "Import Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    this.ListGraphs();
                }
                catch (UriFormatException)
                {
                    MessageBox.Show("Unable to perform an import as the URI specified was not a valid URI", "Import Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (RdfParserSelectionException)
                {
                    //Should be an RDF dataset instead?
                    try
                    {
                        //Push into a Background Thread here and show a modal dialog while it's processing
                        Thread importer = new Thread(new ThreadStart(delegate { this.ImportStore(new Uri(this.txtImportUri.Text)); }));
                        fclsPleaseWait wait = new fclsPleaseWait("Import", importer);
                        wait.ShowDialog();

                        this.ListGraphs();
                    }
                    catch (UriFormatException)
                    {
                        MessageBox.Show("Unable to perform an import as the URI specified was not a valid URI", "Import Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (RdfParserSelectionException)
                    {
                        MessageBox.Show("Unable to perform an import as the format of the RDF from the URI could not be determined", "Import Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (RdfParseException)
                    {
                        MessageBox.Show("Unable to parse RDF Graphs from the selected URI as a Parsing Error occurred", "Import Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (RdfParseException)
                {
                    MessageBox.Show("Unable to parse an RDF Graph from the selected URI as a Parsing Error occurred", "Import Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                storeparser.Load(store, new StreamParams(filename));

                GenericStoreWriter writer = new GenericStoreWriter();
                writer.Save(store, new GenericIOParams(this._manager, 4));

                this.CrossThreadMessage("Successfully imported " + store.Graphs.Count + " Graphs into the Store", "Import Completed", MessageBoxIcon.Information);
            }
            catch (RdfStorageException storeEx)
            {
                this.CrossThreadMessage("An error occurred during import:\n" + storeEx.Message, "Import Failed", MessageBoxIcon.Error);
            }
        }

        private void ImportStore(Uri u)
        {
            try
            {
                TripleStore store = new TripleStore();
                UriLoader.Load(store, u);

                GenericStoreWriter writer = new GenericStoreWriter();
                writer.Save(store, new GenericIOParams(this._manager, 4));

                this.CrossThreadMessage("Successfully imported " + store.Graphs.Count + " Graphs into the Store", "Import Completed", MessageBoxIcon.Information);
            }
            catch (RdfStorageException storeEx)
            {
                this.CrossThreadMessage("An error occurred during import:\n" + storeEx.Message, "Import Failed", MessageBoxIcon.Error);
            }
        }

        private void lvwGraphs_DoubleClick(object sender, System.EventArgs e)
        {
            if (this.lvwGraphs.SelectedItems.Count > 0)
            {
                String graphUri = this.lvwGraphs.SelectedItems[0].Text;
                Graph g = new Graph();
                this._manager.LoadGraph(g, graphUri);
                GraphViewerForm graphViewer = new GraphViewerForm(g, "dotNetRDF Store Manager");
                graphViewer.MdiParent = this.MdiParent;
                graphViewer.Show();
            }
        }

        private void timStartup_Tick(object sender, EventArgs e)
        {
            if (this._manager.IsReady)
            {
                this.stsCurrent.Text = "Store is ready";
                this.ListGraphs();
                this.timStartup.Stop();
            }
        }

        private void btnSaveQuery_Click(object sender, EventArgs e)
        {
            this.sfdQuery.Filter = Constants.SparqlQueryFilter;
            if (this.sfdQuery.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter writer = new StreamWriter(this.sfdQuery.FileName))
                {
                    writer.Write(this.txtSparqlQuery.Text);
                }
            }
        }

        private void btnLoadQuery_Click(object sender, EventArgs e)
        {
            this.ofdQuery.Filter = Constants.SparqlQueryFilter;
            if (this.ofdQuery.ShowDialog() == DialogResult.OK)
            {
                using (StreamReader reader = new StreamReader(this.ofdQuery.FileName))
                {
                    this.txtSparqlQuery.Text = reader.ReadToEnd();
                }
            }
        }

        private void btnSparqlUpdate_Click(object sender, EventArgs e)
        {
            if (!this._manager.IsReady)
            {
                MessageBox.Show("Please wait for Store to be ready before attempting to make a SPARQL Update", "Store Not Ready", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            this.stsCurrent.Text = "Processing SPARQL Update...";

            try
            {
                GenericUpdateProcessor processor = new GenericUpdateProcessor(this._manager);      
      
                //Parse the commands
                SparqlUpdateParser parser = new SparqlUpdateParser();
                SparqlUpdateCommandSet commands = parser.ParseFromString(this.txtSparqlUpdate.Text);

                //Process the Update
                processor.ProcessCommandSet(commands);

                this.stsCurrent.Text = "SPARQL Update completed - Store is ready";
            }
            catch (RdfStorageException storeEx)
            {
                this.stsCurrent.Text = "SPARQL Update failed - Store is ready";
                MessageBox.Show("SPARQL Update failed due to the following error:\n" + storeEx.Message, "SPARQL Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (RdfParseException parseEx)
            {
                this.stsCurrent.Text = "Unable to parse SPARQL Update  - Store is ready";
                MessageBox.Show("Parsing the SPARQL Update failed due to the following error:\n" + parseEx.Message, "SPARQL Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                this.stsCurrent.Text = "SPARQL Update failed - Store is ready";
                MessageBox.Show("SPARQL Update failed due to the following error:\n" + ex.Message, "SPARQL Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Task Management

        private void AddTask<T>(ITask<T> task, TaskCallback<T> callback) where T : class
        {
            String[] items = new String[]
            {
                this._taskID.ToString(),
                task.Name,
                task.State.GetStateDescription(),
                task.Information
            };
            ListViewItem item = new ListViewItem(items);
            this.lvwTasks.Items.Add(item);

            //Ensure that the Task Information gets updated automatically when the Task State changes
            TaskStateChanged d = delegate()
            {
                CrossThreadAlterSubItem(item, 2, task.State.GetStateDescription());
                CrossThreadAlterSubItem(item, 3, task.Information);
                CrossThreadRefresh(this.lvwTasks);
            };
            task.StateChanged += d;

            //Start the Task
            task.RunTask(callback);
        }

        private void ListGraphsCallback(ITask<IEnumerable<Uri>> task)
        {
            if (task.State == TaskState.Completed && task.Result != null)
            {
                this.CrossThreadSetText(this.stsCurrent, "Rendering Graph List...");
                this.CrossThreadSetVisibility(this.lvwGraphs, true);
                this.CrossThreadBeginUpdate(this.lvwGraphs);
                this.CrossThreadClear(this.lvwGraphs);

                foreach (Uri u in task.Result)
                {
                    this.CrossThreadAdd(this.lvwGraphs, u.ToString());
                }

                this.CrossThreadEndUpdate(this.lvwGraphs);

                this.CrossThreadSetText(this.stsCurrent, "Store is ready");
                this.CrossThreadSetEnabled(this.btnGraphRefresh, true);
            }
            else
            {
                this.CrossThreadSetText(this.stsCurrent, "Graph Listing unavailable - Store is ready");
                if (task.Error != null)
                {
                    CrossThreadMessage("Unable to list Graphs due to the following error:\n" + task.Error.Message, "Graph List Unavailable", MessageBoxIcon.Warning);
                }
                this.CrossThreadSetVisibility(this.lvwGraphs, false);
                this.CrossThreadSetVisibility(this.lblGraphListUnavailable, true);
                this.CrossThreadRefresh(this.tabGraphs);
            }
        }

        private void QueryCallback(ITask<Object> task)
        {
            if (task is QueryTask)
            {
                QueryTask qTask = (QueryTask)task;
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
                        //Ignore Exceptions in reporting Execution Time
                    }
                }
            }

            if (task.State == TaskState.Completed)
            {
                Object result = task.Result;

                if (result is IGraph)
                {
                    GraphViewerForm graphViewer = new GraphViewerForm((IGraph)result);
                    CrossThreadSetMdiParent(graphViewer);
                    CrossThreadShow(graphViewer);
                }
                else if (result is SparqlResultSet)
                {
                    ResultSetViewerForm resultsViewer = new ResultSetViewerForm((SparqlResultSet)result);
                    CrossThreadSetMdiParent(resultsViewer);
                    CrossThreadShow(resultsViewer);
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

        #endregion
    }
}
