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

namespace dotNetRDFStore
{
    public partial class fclsGenericStoreManager : CrossThreadForm
    {
        private IGenericIOManager _manager;

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
            this.cboSPARQLResultFormat.SelectedIndex = 0;

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

        private void ListGraphs()
        {
            if (!this._manager.IsReady) return;

            if (this._manager is IQueryableGenericIOManager)
            {
                this.CrossThreadSetText(this.stsCurrent, "Retrieving Graph List from the Store...");

                this.CrossThreadSetVisibility(this.lvwGraphs, true);
                this.CrossThreadBeginUpdate(this.lvwGraphs);
                this.CrossThreadClear(this.lvwGraphs);

                if (this._manager.ListGraphsSupported)
                {
                    try
                    {
                        //Use ListGraphs() if it is supported
                        foreach (Uri u in this._manager.ListGraphs())
                        {
                            this.CrossThreadAdd(this.lvwGraphs, u.ToString());
                        }

                        this.CrossThreadSetText(this.stsCurrent, "Store is ready");
                    }
                    catch (RdfStorageException storeEx)
                    {
                        this.CrossThreadSetText(this.stsCurrent, "Graph Listing unavailable - Store is ready");
                        this.CrossThreadMessage("Unable to list Graphs due to the following error:\n" + storeEx.Message, "Graph List Unavailable", MessageBoxIcon.Warning);
                    }
                    catch (RdfException rdfEx)
                    {
                        this.CrossThreadSetText(this.stsCurrent, "Graph Listing unavailable - Store is ready");
                        CrossThreadMessage("Unable to list Graphs due to the following error:\n" + rdfEx.Message, "Graph List Unavailable", MessageBoxIcon.Warning);
                    }
                    catch (Exception ex)
                    {
                        this.CrossThreadSetText(this.stsCurrent, "Graph Listing unavailable - Store is ready");
                        CrossThreadMessage("Unable to list Graphs due to the following error:\n" + ex.Message, "Graph List Unavailable", MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    //Otherwise List Graphs by issuing SELECT DISTINCT ?g WHERE {GRAPH ?g {?s ?p ?o}}
                    IQueryableGenericIOManager queryManager = (IQueryableGenericIOManager)this._manager;
                    try
                    {
                        Object results = queryManager.Query("SELECT DISTINCT ?g WHERE {GRAPH ?g {?s ?p ?o}}");
                        if (results is SparqlResultSet)
                        {
                            SparqlResultSet rset = (SparqlResultSet)results;
                            foreach (SparqlResult res in rset)
                            {
                                this.CrossThreadAdd(this.lvwGraphs, res["g"].ToString());
                            }
                        }
                        this.CrossThreadSetText(this.stsCurrent, "Store is ready");
                    }
                    catch (RdfStorageException storeEx)
                    {
                        this.CrossThreadSetText(this.stsCurrent, "Graph Listing unavailable - Store is ready");
                        this.CrossThreadMessage("Unable to list Graphs due to the following error:\n" + storeEx.Message, "Graph List Unavailable", MessageBoxIcon.Warning);
                    }
                    catch (RdfException rdfEx)
                    {
                        this.CrossThreadSetText(this.stsCurrent, "Graph Listing unavailable - Store is ready");
                        CrossThreadMessage("Unable to list Graphs due to the following error:\n" + rdfEx.Message, "Graph List Unavailable", MessageBoxIcon.Warning);
                    }
                    catch (Exception ex)
                    {
                        this.CrossThreadSetText(this.stsCurrent, "Graph Listing unavailable - Store is ready");
                        CrossThreadMessage("Unable to list Graphs due to the following error:\n" + ex.Message, "Graph List Unavailable", MessageBoxIcon.Warning);
                    }
                }

                this.CrossThreadEndUpdate(this.lvwGraphs);
                this.CrossThreadSetEnabled(this.btnGraphRefresh, true);
            } 
            else
            {
                this.CrossThreadSetVisibility(this.lvwGraphs, false);
                this.CrossThreadSetVisibility(this.lblGraphListUnavailable, true);
                this.CrossThreadRefresh(this.tabGraphs);
            }
        }

        private void btnSparqlQuery_Click(object sender, EventArgs e)
        {
            if (!this._manager.IsReady)
            {
                MessageBox.Show("Please wait for Store to be ready before attempting to make a SPARQL Query", "Store Not Ready", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (this._manager is IQueryableGenericIOManager)
            {
                this.stsCurrent.Text = "Processing SPARQL Query...";
                IQueryableGenericIOManager queryManager = (IQueryableGenericIOManager)this._manager;

                Stopwatch timer = new Stopwatch();

                try
                {
                    timer.Start();
                    Object results = queryManager.Query(this.txtSparqlQuery.Text);
                    timer.Stop();

                    if (results is SparqlResultSet)
                    {
                        if (!Directory.Exists("results")) Directory.CreateDirectory("results");
                        if (!File.Exists("results\\sparql.css")) 
                        {
                            StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("dotNetRDFStore.sparql.css"));
                            String css = reader.ReadToEnd();
                            reader.Close();
                            StreamWriter csswriter = new StreamWriter("results\\sparql.css");
                            csswriter.Write(css);
                            csswriter.Close();
                        }

                        ISparqlResultsWriter writer;
                        String destFile = "results\\" + DateTime.Now.ToString("yyyyMMddHHmmssffff");
                        switch (this.cboSPARQLResultFormat.SelectedIndex)
                        {
                            case 1:
                                writer = new SparqlXmlWriter();
                                destFile += ".srx";
                                break;
                            case 2:
                                writer = new SparqlJsonWriter();
                                destFile += ".json";
                                break;
                            case 0:
                            default:
                                writer = new SparqlHtmlWriter();
                                ((IHtmlWriter)writer).Stylesheet = "sparql.css";
                                destFile += ".html";
                                break;
                        }

                        try
                        {
                            writer.Save((SparqlResultSet)results, destFile);
                            this.stsCurrent.Text = "SPARQL Query Complete (Took " + timer.Elapsed + ") - Store is ready";
                            System.Diagnostics.Process.Start(destFile);
                        }
                        catch (Exception ex)
                        {
                            this.stsCurrent.Text = "Unable to display SPARQL Query results (Took " + timer.Elapsed + ") - Store is ready";
                            MessageBox.Show("Unable to display results due to the following error during output:\n" + ex.Message, "Output Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else if (results is Graph)
                    {
                        this.stsCurrent.Text = "SPARQL Query Complete (Took " + timer.Elapsed + ") - Store is ready";
                        GraphViewerForm graphViewer = new GraphViewerForm((Graph)results, "dotNetRDF Store Manager");
                        graphViewer.MdiParent = this.MdiParent;
                        graphViewer.Show();
                    }
                    else
                    {
                        this.stsCurrent.Text = "SPARQL Query returned unknown result (Took " + timer.Elapsed + ") - Store is ready";
                        MessageBox.Show("Received an unknown result from the SPARQL Query", "SPARQL Query Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (RdfStorageException storeEx)
                {
                    if (timer.IsRunning) timer.Stop();
                    this.stsCurrent.Text = "SPARQL Query Failed (Took " + timer.Elapsed + ") - Store is ready";
                    MessageBox.Show("SPARQL Query failed due to the following error:\n" + storeEx.Message, "SPARQL Query Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (RdfParseException parseEx)
                {
                    if (timer.IsRunning) timer.Stop();
                    this.stsCurrent.Text = "Unable to parse SPARQL Query Results (Took " + timer.Elapsed + ") - Store is ready";
                    MessageBox.Show("Parsing the results of the SPARQL Query failed due to the following error:\n" + parseEx.Message, "SPARQL Query Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    if (timer.IsRunning) timer.Stop();
                    this.stsCurrent.Text = "SPARQL Query Failed (Took " + timer.Elapsed + ") - Store is ready";
                    MessageBox.Show("SPARQL Query failed due to the following error:\n" + ex.Message, "SPARQL Query Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Unable to execute a SPARQL Query since your Store does not support SPARQL", "SPARQL Query Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                Thread t = new Thread(new ThreadStart(this.ListGraphs));
                t.IsBackground = true;
                t.Start();
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
    }
}
