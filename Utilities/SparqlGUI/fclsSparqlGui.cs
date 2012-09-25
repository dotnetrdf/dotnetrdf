/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Store;
using VDS.RDF.GUI;
using VDS.RDF.GUI.WinForms;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.FullText.Indexing;
using VDS.RDF.Query.FullText.Indexing.Lucene;
using VDS.RDF.Query.FullText.Schema;
using VDS.RDF.Query.FullText.Search;
using VDS.RDF.Query.FullText.Search.Lucene;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Utilities.Sparql
{
    public partial class fclsSparqlGui : Form
    {
        private ISparqlDataset _dataset = new InMemoryQuadDataset();
        private LeviathanQueryProcessor _processor;
        private IRdfWriter _rdfwriter = new CompressingTurtleWriter(WriterCompressionLevel.High);
        private ISparqlResultsWriter _resultswriter = new SparqlHtmlWriter();
        private String _rdfext = ".ttl";
        private String _resultsext = ".html";
        private bool _noDataWarning = true, _logExplain = false;
        private String _logfile;
        private long _tripleCount = 0;

        //Full Text Indexing stuff
        private Lucene.Net.Store.Directory _ftIndex;
        private IFullTextIndexer _ftIndexer;
        private IFullTextSearchProvider _ftSearcher;
        private FullTextOptimiser _ftOptimiser;

        public fclsSparqlGui()
        {
            InitializeComponent();
            Constants.WindowIcon = this.Icon;
            this._processor = new LeviathanQueryProcessor(this._dataset);

            //Enable UTF-8 BOM setting if user set
            Options.UseBomForUtf8 = false;
            if (Properties.Settings.Default.UseUtf8Bom)
            {
                Options.UseBomForUtf8 = true;
                this.chkUseUtf8Bom.Checked = true;
            }

            String temp = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            String sep = new String(new char[] { Path.DirectorySeparatorChar });
            if (!temp.EndsWith(sep)) temp += sep;
            temp = Path.Combine(temp, @"dotNetRDF\");
            if (!System.IO.Directory.Exists(temp)) System.IO.Directory.CreateDirectory(temp);
            temp = Path.Combine(temp, @"SparqlGUI\");
            if (!System.IO.Directory.Exists(temp)) System.IO.Directory.CreateDirectory(temp);
            this._logfile = Path.Combine(temp, "SparqlGui-" + DateTime.Now.ToString("MMM-yyyy") + ".log");

            this.ofdBrowse.Filter = MimeTypesHelper.GetFilenameFilter(true, true, false, false, false, true);
            this.ofdQuery.Filter = MimeTypesHelper.GetFilenameFilter(false, false, false, true, false, true);
        }

        private void fclsSparqlGui_Load(object sender, EventArgs e)
        {
            if (File.Exists("default.rq"))
            {
                StreamReader reader = new StreamReader("default.rq");
                String defaultQuery = reader.ReadToEnd();
                this.txtQuery.Text = defaultQuery;
                reader.Close();
            }
            this.cboGraphFormat.SelectedIndex = 5;
            this.cboResultsFormat.SelectedIndex = 2;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (this.ofdBrowse.ShowDialog() == DialogResult.OK)
            {
                this.txtSourceFile.Text = this.ofdBrowse.FileName;
            }
        }

        private void btnImportFile_Click(object sender, EventArgs e)
        {
            if (this.txtSourceFile.Text.Equals(string.Empty))
            {
                MessageBox.Show("Please enter a File you wish to import RDF from...", "No File Specified");
            }
            else
            {
                try
                {
                    //Try and get a Graph Parser and load
                    IRdfReader parser = MimeTypesHelper.GetParser(MimeTypesHelper.GetMimeType(MimeTypesHelper.GetTrueFileExtension(this.txtSourceFile.Text)));
                    Graph g = new Graph();
                    FileLoader.Load(g, this.txtSourceFile.Text);
                    this.LogImportSuccess(this.txtSourceFile.Text, 1, g.Triples.Count);

                    //Add to Store
                    try
                    {
                        this._tripleCount += g.Triples.Count;
                        this._dataset.AddGraph(g);
                    }
                    catch (Exception ex)
                    {
                        this.LogImportFailure(this.txtSourceFile.Text, ex);
                        MessageBox.Show("An error occurred trying to add the RDF Graph to the Dataset:\n" + ex.Message, "File Import Error");
                        return;
                    }
                }
                catch (RdfParserSelectionException)
                {
                    try
                    {
                        //Try and get a Store Parser and load
                        IStoreReader storeparser = MimeTypesHelper.GetStoreParser(MimeTypesHelper.GetMimeType(MimeTypesHelper.GetTrueFileExtension(this.txtSourceFile.Text)));
                        TripleStore store = new TripleStore();
                        storeparser.Load(store, this.txtSourceFile.Text);

                        foreach (IGraph g in store.Graphs)
                        {
                            if (this._dataset.HasGraph(g.BaseUri))
                            {
                                int triplesBefore = this._dataset[g.BaseUri].Triples.Count;
                                this._dataset[g.BaseUri].Merge(g);
                                this._tripleCount += this._dataset[g.BaseUri].Triples.Count - triplesBefore;
                            }
                            else
                            {
                                this._dataset.AddGraph(g);
                                this._tripleCount += g.Triples.Count;
                            }
                        }

                        this.LogImportSuccess(this.txtSourceFile.Text, store.Graphs.Count, store.Graphs.Sum(g => g.Triples.Count));
                    }
                    catch (RdfParserSelectionException selEx)
                    {
                        this.LogImportFailure(this.txtSourceFile.Text, selEx);
                        MessageBox.Show("The given file does not appear to be an RDF Graph/Dataset File Format the tool understands", "File Import Error");
                        return;
                    }
                    catch (Exception ex)
                    {
                        this.LogImportFailure(this.txtSourceFile.Text, ex);
                        MessageBox.Show("An error occurred trying to read an RDF Dataset from the file:\n" + ex.Message, "File Import Error");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    this.LogImportFailure(this.txtSourceFile.Text, ex);
                    MessageBox.Show("An error occurred trying to read an RDF Graph from the file:\n" + ex.Message, "File Import Error");
                    return;
                }

                this.stsGraphs.Text = this._dataset.GraphUris.Count() + " Graphs";
                this.stsTriples.Text = this._tripleCount + " Triples";
                MessageBox.Show("RDF added to the Dataset OK", "File Import Done");
            }
        }

        private void btnImportUri_Click(object sender, EventArgs e)
        {
            if (this.txtSourceUri.Text.Equals(string.Empty))
            {
                MessageBox.Show("Please enter a URI you wish to import RDF from...", "No URI Specified");
            }
            else
            {
                Graph g = new Graph();
                try
                {
                    UriLoader.Load(g, new Uri(this.txtSourceUri.Text));

                    try
                    {
                        if (this._dataset.HasGraph(g.BaseUri))
                        {
                            int triplesBefore = this._dataset[g.BaseUri].Triples.Count;
                            this._dataset[g.BaseUri].Merge(g);
                            this._tripleCount += this._dataset[g.BaseUri].Triples.Count - triplesBefore;
                        }
                        else
                        {
                            this._dataset.AddGraph(g);
                            this._tripleCount += g.Triples.Count;
                        }

                        this.LogImportSuccess(new Uri(this.txtSourceUri.Text), 1, g.Triples.Count);
                    }
                    catch (Exception ex)
                    {
                        this.LogImportFailure(new Uri(this.txtSourceUri.Text), ex);
                        MessageBox.Show("An error occurred trying to add the RDF Graph to the Dataset:\n" + ex.Message, "URI Import Error");
                        return;
                    }
                }
                catch (UriFormatException uriEx)
                {
                    MessageBox.Show("The URI you have entered is malformed:\n" + uriEx.Message, "Malformed URI");
                }
                catch (Exception ex)
                {
                    this.LogImportFailure(new Uri(this.txtSourceUri.Text), ex);
                    MessageBox.Show("An error occurred while loading RDF from the given URI:\n" + ex.Message, "URI Import Error");
                    return;
                }
                this.stsGraphs.Text = this._dataset.GraphUris.Count() + " Graphs";
                this.stsTriples.Text = this._tripleCount + " Triples";
                MessageBox.Show("RDF added to the Dataset OK", "URI Import Done");
            }
        }

        private void btnClearDataset_Click(object sender, EventArgs e)
        {
            this._dataset = new InMemoryQuadDataset();
            this._processor = new LeviathanQueryProcessor(this._dataset);
            if (this.chkFullTextIndexing.Checked) this.EnableFullTextIndex();
            this._tripleCount = 0;
            this.stsGraphs.Text = this._dataset.GraphUris.Count() + " Graphs";
            this.stsTriples.Text = this._tripleCount + " Triples";
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            try
            {
                SparqlQueryParser parser = new SparqlQueryParser();
                SparqlQuery query = parser.ParseFromString(this.txtQuery.Text);
                query.Timeout = (long)this.numTimeout.Value;
                query.PartialResultsOnTimeout = this.chkPartialResults.Checked;

                if (this._tripleCount == 0 && this._noDataWarning)
                {
                    switch (MessageBox.Show("You have no data loaded to query over - do you wish to run this query anyway?  Press Abort if you'd like to load data first, Retry to continue anyway or Ignore to continue anyway and suppress this message during this session", "Continue Query without Data", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Question))
                    {
                        case DialogResult.Abort:
                            return;
                        case DialogResult.Ignore:
                            //Set the Ignore flag then continue anyway
                            this._noDataWarning = false;
                            break;
                        default:
                            //Continue anyway
                            break;
                    }
                }

                this.LogStartQuery(query);

                //Evaluate the Query
                Object results;
                if (this._logExplain)
                {
                    using (StreamWriter writer = new StreamWriter(this._logfile, true, Encoding.UTF8))
                    {
                        ExplainQueryProcessor explainer = new ExplainQueryProcessor(this._dataset, (ExplanationLevel.OutputToTrace | ExplanationLevel.ShowAll | ExplanationLevel.AnalyseAll ) ^ ExplanationLevel.ShowThreadID);
                        TextWriterTraceListener listener = new TextWriterTraceListener(writer, "SparqlGUI");
                        Trace.Listeners.Add(listener);
                        try
                        {
                            results = explainer.ProcessQuery(query);
                        }
                        finally
                        {
                            Trace.Listeners.Remove(listener);
                        }

                        writer.Close();
                    }
                }
                else
                {
                    results = this._processor.ProcessQuery(query);
                }

                //Process the Results
                if (results is IGraph)
                {
                    this.LogEndQuery(query, (IGraph)results);

                    if (this.chkViewResultsInApp.Checked)
                    {
                        GraphViewerForm graphViewer = new GraphViewerForm((IGraph)results, "SPARQL GUI");
                        graphViewer.Show();
                    }
                    else
                    {
                        this._rdfwriter.Save((IGraph)results, "temp" + this._rdfext);
                        System.Diagnostics.Process.Start("temp" + this._rdfext);
                    }
                }
                else if (results is SparqlResultSet)
                {
                    this.LogEndQuery(query, (SparqlResultSet)results);

                    if (this.chkViewResultsInApp.Checked)
                    {
                        ResultSetViewerForm resultSetViewer = new ResultSetViewerForm((SparqlResultSet)results, "SPARQL GUI", query.NamespaceMap);
                        resultSetViewer.Show();
                    }
                    else
                    {
                        this._resultswriter.Save((SparqlResultSet)results, "temp" + this._resultsext);
                        System.Diagnostics.Process.Start("temp" + this._resultsext);
                    }
                }
                else
                {
                    throw new RdfException("Unexpected Result Type");
                }
                this.stsLastQuery.Text = "Last Query took " + query.QueryExecutionTime;
            }
            catch (RdfParseException parseEx)
            {
                this.LogMalformedQuery(parseEx);
                MessageBox.Show("Query failed to parse:\n" + parseEx.Message, "Query Failed");
            }
            catch (RdfQueryException queryEx)
            {
                this.LogFailedQuery(queryEx);
                MessageBox.Show("Query failed during Execution:\n" + queryEx.Message, "Query Failed");
            }
            catch (Exception ex)
            {
                this.LogFailedQuery(ex);
                MessageBox.Show("Query failed:\n" + ex.Message + "\n" + ex.StackTrace, "Query Failed");
            }
        }

        private void btnInspect_Click(object sender, EventArgs e)
        {
            try
            {
                SparqlQueryParser parser = new SparqlQueryParser();
                Stopwatch timer = new Stopwatch();
                timer.Start();
                SparqlQuery query = parser.ParseFromString(this.txtQuery.Text);
                timer.Stop();

                fclsInspect inspect = new fclsInspect(query, timer.ElapsedMilliseconds, this.txtQuery.Text);
                inspect.Show();
            }
            catch (RdfParseException parseEx)
            {
                MessageBox.Show("Query failed to parse:\n" + parseEx.Message, "Query Failed");
            }
            catch (RdfQueryException queryEx)
            {
                MessageBox.Show("Query failed during Execution:\n" + queryEx.Message, "Query Failed");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Query failed:\n" + ex.Message + "\n" + ex.StackTrace, "Query Failed");
            }
        }

        private void radSparql10_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radSparql10.Checked) Options.QueryDefaultSyntax = SparqlQuerySyntax.Sparql_1_0;
        }

        private void radSparql11_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radSparql11.Checked) Options.QueryDefaultSyntax = SparqlQuerySyntax.Sparql_1_1;
        }

        private void radSparqlExtended_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radSparqlExtended.Checked) Options.QueryDefaultSyntax = SparqlQuerySyntax.Extended;
        }

        private void chkWebDemand_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chkWebDemand.Checked)
            {
                this.EnableWebDemand();
            }
            else
            {
                this.DisableWebDemand();
            }
        }

        private void cboGraphFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (this.cboGraphFormat.SelectedIndex)
            {
                case 0:
                    this._rdfwriter = new CsvWriter();
                    this._rdfext = ".csv";
                    break;
                case 1:
                    fclsStylesheetPicker stylesheetPicker = new fclsStylesheetPicker("CSS (Optional)");
                    if (stylesheetPicker.ShowDialog() == DialogResult.OK)
                    {
                        HtmlWriter temp = new HtmlWriter();
                        temp.Stylesheet = stylesheetPicker.StylesheetUri;
                        this._rdfwriter = temp;
                    }
                    else
                    {
                        this._rdfwriter = new HtmlWriter();
                    }
                    this._rdfext = ".html";
                    break;
                case 2:
                    this._rdfwriter = new Notation3Writer();
                    this._rdfext = ".n3";
                    break;
                case 3:
                    this._rdfwriter = new NTriplesWriter();
                    this._rdfext = ".nt";
                    break;
                case 4:
                    this._rdfwriter = new RdfJsonWriter();
                    this._rdfext = ".json";
                    break;
                case 5:
                    this._rdfwriter = new RdfXmlWriter();
                    this._rdfext = ".rdf";
                    break;
                case 6:
                    this._rdfwriter = new CompressingTurtleWriter();
                    this._rdfext = ".ttl";
                    break;
                case 7:
                    this._rdfwriter = new TsvWriter();
                    this._rdfext = ".tsv";
                    break;
            }

            if (this._rdfwriter is ICompressingWriter)
            {
                ((ICompressingWriter)this._rdfwriter).CompressionLevel = WriterCompressionLevel.High;
            }

            if (this.cboResultsFormat.SelectedIndex == 1)
            {
                this._resultswriter = new SparqlRdfWriter(this._rdfwriter);
                this._resultsext = this._rdfext;
            }
        }

        private void cboResultsFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            fclsStylesheetPicker stylesheetPicker;

            switch (this.cboResultsFormat.SelectedIndex)
            {
                case 0:
                    this._resultswriter = new SparqlCsvWriter();
                    this._resultsext = ".csv";
                    break;
                case 1:
                    this._resultswriter = new SparqlRdfWriter(this._rdfwriter);
                    this._resultsext = this._rdfext;
                    break;
                case 2:
                    this._resultswriter = new SparqlHtmlWriter();
                    this._resultsext = ".html";
                    break;
                case 3:
                    stylesheetPicker = new fclsStylesheetPicker("CSS");
                    if (stylesheetPicker.ShowDialog() == DialogResult.OK) 
                    {
                        SparqlHtmlWriter temp = new SparqlHtmlWriter();
                        temp.Stylesheet = stylesheetPicker.StylesheetUri;
                        this._resultswriter = temp;
                    } 
                    else 
                    {
                        this._resultswriter = new SparqlHtmlWriter();
                    }
                    this._resultsext = ".html";
                    break;
                    
                case 4:
                    this._resultswriter = new SparqlJsonWriter();
                    this._resultsext = ".json";
                    break;
                case 5:
                    this._resultswriter = new SparqlTsvWriter();
                    this._resultsext = ".tsv";
                    break;
                case 6:
                    this._resultswriter = new SparqlXmlWriter();
                    this._resultsext = ".srx";
                    break;
                case 7:
                    stylesheetPicker = new fclsStylesheetPicker("XSLT");
                    if (stylesheetPicker.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            this._resultswriter = new SparqlXsltWriter(stylesheetPicker.StylesheetUri);
                            this._resultsext = ".xml";
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Unable to use the selected XSLT Stylesheet due to the following error:\n" + ex.Message, "Invalid Stylesheet", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            this.cboResultsFormat.SelectedIndex = 2;
                            this._resultswriter = new SparqlHtmlWriter();
                            this._resultsext = ".html";
                        }
                    }
                    else
                    {
                        this.cboResultsFormat.SelectedIndex = 2;
                        this._resultswriter = new SparqlHtmlWriter();
                        this._resultsext = ".html";
                    }
                    break;
            }
        }

        private void btnSaveQuery_Click(object sender, EventArgs e)
        {
            if (this.sfdQuery.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter writer = new StreamWriter(this.sfdQuery.FileName))
                {
                    writer.Write(this.txtQuery.Text);
                }
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (this.ofdQuery.ShowDialog() == DialogResult.OK)
            {
                using (StreamReader reader = new StreamReader(this.ofdQuery.FileName))
                {
                    this.txtQuery.Text = reader.ReadToEnd();
                }
            }
        }

        private void chkAllowUnknownFunctions_CheckedChanged(object sender, EventArgs e)
        {
            Options.QueryAllowUnknownFunctions = this.chkAllowUnknownFunctions.Checked;
        }

        private void chkQueryOptimisation_CheckedChanged(object sender, EventArgs e)
        {
            Options.QueryOptimisation = this.chkQueryOptimisation.Checked;
        }

        private void chkAlgebraOptimisation_CheckedChanged(object sender, EventArgs e)
        {
            Options.AlgebraOptimisation = this.chkAlgebraOptimisation.Checked;
        }

        private void chkUseUtf8Bom_CheckedChanged(object sender, EventArgs e)
        {
            Options.UseBomForUtf8 = this.chkUseUtf8Bom.Checked;
            Properties.Settings.Default.UseUtf8Bom = this.chkUseUtf8Bom.Checked;
            Properties.Settings.Default.Save();
        }

        private void Log(String action, String information)
        {
            using (StreamWriter writer = new StreamWriter(this._logfile, true, System.Text.Encoding.UTF8))
            {
                writer.Write("[" + DateTime.Now + "] " + action);
                if (information.Contains('\n') || information.Contains('\r'))
                {
                    writer.WriteLine();
                    writer.Write(information);
                }
                else
                {
                    writer.WriteLine(' ');
                    writer.WriteLine(information);
                }
                writer.Close();
            }
        }

        private void LogImportSuccess(String file, int graphs, int triples)
        {
            this.Log("IMPORT", "Import from File '" + file + "' - " + graphs + " Graphs with " + triples + " Triples");
        }

        private void LogImportSuccess(Uri u, int graphs, int triples)
        {
            this.Log("IMPORT", "Import from URI '" + u.AbsoluteUri + "' - " + graphs + " Graphs with " + triples + " Triples");
        }

        private void LogImportFailure(String file, Exception ex)
        {
            this.Log("IMPORT FAILURE", "Import from File '" + file + "' failed\n" + this.GetFullErrorTrace(ex));
        }

        private void LogImportFailure(Uri u, Exception ex)
        {
            this.Log("IMPORT FAILURE", "Import from URI '" + u.AbsoluteUri + "' failed\n" + this.GetFullErrorTrace(ex));
        }

        private void LogMalformedQuery(Exception ex)
        {
            this.Log("QUERY PARSING FAILURE", "Failed to Parse Query\n" + this.GetFullErrorTrace(ex));
        }

        private void LogStartQuery(SparqlQuery q)
        {
            SparqlFormatter formatter = new SparqlFormatter(q.NamespaceMap);
            this.Log("QUERY START", formatter.Format(q));
        }

        private void LogFailedQuery(Exception ex)
        {
            this.Log("QUERY FAILED", "Query Failed during Execution\n" + this.GetFullErrorTrace(ex));
        }

        private void LogEndQuery(SparqlQuery q, SparqlResultSet results)
        {
            if (results.ResultsType == SparqlResultsType.Boolean)
            {
                this.Log("QUERY END", "Query Finished in " + q.QueryExecutionTime + " producing a Boolean Result of " + results.Result);
            }
            else
            {
                this.Log("QUERY END", "Query Finished in " + q.QueryExecutionTime + " producing a Result Set containing " + results.Count + " Results");
            }
        }

        private void LogEndQuery(SparqlQuery q, IGraph g)
        {
            this.Log("QUERY END", "Query Finished in " + q.QueryExecutionTime + " producing a Graph contaning " + g.Triples.Count + " Triples");
        }

        private String GetFullErrorTrace(Exception ex)
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine(ex.Message);
            output.AppendLine(ex.StackTrace);

            while (ex.InnerException != null)
            {
                output.AppendLine();
                output.AppendLine(ex.InnerException.Message);
                output.AppendLine(ex.InnerException.StackTrace);
                ex = ex.InnerException;
            }

            return output.ToString();
        }

        private void btnViewLog_Click(object sender, EventArgs e)
        {
            if (File.Exists(this._logfile))
            {
                try
                {
                    Process.Start(this._logfile);
                }
                catch
                {
                    MessageBox.Show("Error opening log file");
                }
            }
            else
            {
                MessageBox.Show("Log File not found!");
            }
        }

        private void chkLogExplanation_CheckedChanged(object sender, EventArgs e)
        {
            this._logExplain = this.chkLogExplanation.Checked;
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            if (File.Exists(this._logfile))
            {
                try
                {
                    File.Delete(this._logfile);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error clearing Log File - " + ex.Message, "Clear Log Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnExplain_Click(object sender, EventArgs e)
        {
            try
            {
                SparqlQueryParser parser = new SparqlQueryParser();
                Stopwatch timer = new Stopwatch();
                timer.Start();
                SparqlQuery query = parser.ParseFromString(this.txtQuery.Text);
                timer.Stop();

                fclsExplanation explain = new fclsExplanation(query, timer.ElapsedMilliseconds);
                explain.Show();
            }
            catch (RdfParseException parseEx)
            {
                MessageBox.Show("Query failed to parse:\n" + parseEx.Message, "Query Failed");
            }
            catch (RdfQueryException queryEx)
            {
                MessageBox.Show("Query failed during Execution:\n" + queryEx.Message, "Query Failed");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Query failed:\n" + ex.Message + "\n" + ex.StackTrace, "Query Failed");
            }
        }

        private void chkFullTextIndexing_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chkFullTextIndexing.Checked)
            {
                this.EnableFullTextIndex();
            }
            else
            {
                this.DisableFullTextIndex();
            }
        }

        #region Feature Configuration

        private void EnableFullTextIndex()
        {
            if (this._dataset is FullTextIndexedDataset)
            {
                //Nothing to do
            }
            else if (this._dataset is WebDemandDataset)
            {
                WebDemandDataset ds = (WebDemandDataset)this._dataset;
                this._dataset = ds.UnderlyingDataset;
                this.EnableFullTextIndex();
                this._dataset = new WebDemandDataset(this._dataset);
            }
            else
            {
                //Create and ensure index ready for use
                this._ftIndex = new RAMDirectory();
                IndexWriter writer = new IndexWriter(this._ftIndex, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29), IndexWriter.MaxFieldLength.UNLIMITED);
                writer.Close();

                //Create Indexer and wrap dataset
                this._ftIndexer = new LuceneObjectsIndexer(this._ftIndex, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29), new DefaultIndexSchema());
                if (this._dataset is WebDemandDataset)
                {
                    //Web Demand needs to go around Full Text as we want to index on demand loaded content
                    this._dataset = new WebDemandDataset(new FullTextIndexedDataset(((WebDemandDataset)this._dataset).UnderlyingDataset, this._ftIndexer, true));
                }
                else
                {
                    this._dataset = new FullTextIndexedDataset(this._dataset, this._ftIndexer, true);
                }

                //Create and Register Optimizer
                this._ftSearcher = new LuceneSearchProvider(Lucene.Net.Util.Version.LUCENE_29, this._ftIndex);
                this._ftOptimiser = new FullTextOptimiser(this._ftSearcher);
                SparqlOptimiser.AddOptimiser(this._ftOptimiser);
            }
            this._processor = new LeviathanQueryProcessor(this._dataset);
        }

        private void DisableFullTextIndex()
        {
            if (this._dataset is WebDemandDataset)
            {
                WebDemandDataset ds = (WebDemandDataset)this._dataset;
                if (ds.UnderlyingDataset is FullTextIndexedDataset)
                {
                    this._dataset = ds.UnderlyingDataset;
                    this.DisableFullTextIndex();
                    this._dataset = new WebDemandDataset(this._dataset);
                }
            }
            else if (this._dataset is FullTextIndexedDataset)
            {
                SparqlOptimiser.RemoveOptimiser(this._ftOptimiser);
                this._ftOptimiser = null;
                this._ftSearcher.Dispose();
                this._ftSearcher = null;
                this._dataset = ((FullTextIndexedDataset)this._dataset).UnderlyingDataset;
                this._ftIndexer.Dispose();
                this._ftIndexer = null;
                this._ftIndex.Close();
                this._ftIndex = null;
            }
            this._processor = new LeviathanQueryProcessor(this._dataset);
        }

        private void EnableWebDemand()
        {
            if (this._dataset is WebDemandDataset)
            {
                //Nothing to do
            }
            else
            {
                //Wrap dataset in a WebDemandDataset
                this._dataset = new WebDemandDataset(this._dataset);
            }
            this._processor = new LeviathanQueryProcessor(this._dataset);
        }

        private void DisableWebDemand()
        {
            if (this._dataset is WebDemandDataset)
            {
                this._dataset = ((WebDemandDataset)this._dataset).UnderlyingDataset;
            }
            this._processor = new LeviathanQueryProcessor(this._dataset);
        }

        #endregion

        private void chkUnsafeOptimisation_CheckedChanged(object sender, EventArgs e)
        {
            Options.UnsafeOptimisation = this.chkUnsafeOptimisation.Checked;
        }

        private void chkParallelEval_CheckedChanged(object sender, EventArgs e)
        {
            Options.UsePLinqEvaluation = this.chkParallelEval.Checked;
        }
    }
}
