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
using System.Linq;
using System.Windows.Forms;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage.Params;
using VDS.RDF.Writing;

namespace SparqlGUI
{
    public partial class fclsSparqlGui : Form
    {
        private IInMemoryQueryableStore _store = new TripleStore();
        private IRdfWriter _rdfwriter = new CompressingTurtleWriter(WriterCompressionLevel.High);
        private ISparqlResultsWriter _resultswriter = new SparqlHtmlWriter();
        private String _rdfext = ".ttl";
        private String _resultsext = ".html";
        private bool _noDataWarning = true;

        public fclsSparqlGui()
        {
            InitializeComponent();
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
                    IRdfReader parser = MimeTypesHelper.GetParser(MimeTypesHelper.GetMimeType(Path.GetExtension(this.txtSourceFile.Text)));
                    Graph g = new Graph();
                    FileLoader.Load(g, this.txtSourceFile.Text);

                    //Add to Store
                    try
                    {
                        this._store.Add(g);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred trying to add the RDF Graph to the Dataset:\n" + ex.Message, "File Import Error");
                        return;
                    }
                }
                catch (RdfParserSelectionException)
                {
                    try
                    {
                        //Try and get a Store Parser and load
                        IStoreReader storeparser = MimeTypesHelper.GetStoreParser(MimeTypesHelper.GetMimeType(Path.GetExtension(this.txtSourceFile.Text)));
                        storeparser.Load(this._store, new StreamParams(this.txtSourceFile.Text));
                    }
                    catch (RdfParserSelectionException)
                    {
                        MessageBox.Show("The given file does not appear to be an RDF Graph/Dataset File Format the tool understands", "File Import Error");
                        return;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred trying to read an RDF Dataset from the file:\n" + ex.Message, "File Import Error");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred trying to read an RDF Graph from the file:\n" + ex.Message, "File Import Error");
                    return;
                }

                this.stsGraphs.Text = this._store.Graphs.Count + " Graphs";
                this.stsTriples.Text = this._store.Triples.Count() + " Triples";
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
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while loading RDF from the given URI:\n" + ex.Message, "URI Import Error");
                    return;
                }
                try
                {
                    this._store.Add(g);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred trying to add the RDF Graph to the Dataset:\n" + ex.Message, "URI Import Error");
                    return;
                }
                this.stsGraphs.Text = this._store.Graphs.Count + " Graphs";
                this.stsTriples.Text = this._store.Triples.Count() + " Triples";
                MessageBox.Show("RDF added to the Dataset OK", "URI Import Done");
            }
        }

        private void btnClearDataset_Click(object sender, EventArgs e)
        {
            this._store.Dispose();
            this._store = new TripleStore();
            this.stsGraphs.Text = this._store.Graphs.Count + " Graphs";
            this.stsTriples.Text = this._store.Triples.Count() + " Triples";
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            try
            {
                SparqlQueryParser parser = new SparqlQueryParser();
                SparqlQuery query = parser.ParseFromString(this.txtQuery.Text);
                query.Timeout = (long)this.numTimeout.Value;
                query.PartialResultsOnTimeout = this.chkPartialResults.Checked;

                if (this._store.Graphs.Count == 0 && this._noDataWarning)
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

                Object results = this._store.ExecuteQuery(query);
                if (results is Graph)
                {
                    this._rdfwriter.Save((Graph)results, new StreamWriter("temp" + this._rdfext));
                    System.Diagnostics.Process.Start("temp" + this._rdfext);
                }
                else if (results is SparqlResultSet)
                {
                    this._resultswriter.Save((SparqlResultSet)results, new StreamWriter("temp" + this._resultsext));
                    System.Diagnostics.Process.Start("temp" + this._resultsext);
                }
                else
                {
                    MessageBox.Show("Unknown Result returned from the Query", "Query Failed");
                    return;
                }
                this.stsLastQuery.Text = "Last Query took " + query.QueryTime + " ms";
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
                if (this._store is WebDemandTripleStore)
                {
                    //Nothing to do
                }
                else
                {
                    WebDemandTripleStore store = new WebDemandTripleStore();
                    foreach (IGraph g in this._store.Graphs)
                    {
                        store.Add(g);
                    }
                    this._store = store;
                }
            }
            else
            {
                if (this._store is TripleStore)
                {
                    //Nothing to do
                }
                else
                {
                    TripleStore store = new TripleStore();
                    foreach (IGraph g in this._store.Graphs)
                    {
                        store.Add(g);
                    }
                    this._store = store;
                }
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
    }
}
