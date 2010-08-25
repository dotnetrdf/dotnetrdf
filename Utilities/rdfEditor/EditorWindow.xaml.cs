using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Microsoft.Win32;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing;

namespace rdfEditor
{
    /// <summary>
    /// Interaction logic for EditorWindow.xaml
    /// </summary>
    public partial class EditorWindow : Window
    {
        private const String FileFilterRdf = "All Supported RDF Files|*.rdf;*.ttl;*.n3;*.nt;*.json;*.owl|RDF/XML Files|*.rdf,*.owl|NTriples Files|*.nt|Turtle Files|*.ttl|Notation 3 Files|*.n3|RDF/JSON Files|*.json";
        private const String FileFilterSparql = "All Supported SPARQL Files|*.rq;*.srx|SPARQL Query Files|*.rq|SPARQL Results Files|*.srx;*.json";

        private OpenFileDialog _ofd = new OpenFileDialog();
        private SaveFileDialog _sfd = new SaveFileDialog();
        private String _currFile = null;
        private bool _changed = false;
        private bool _enableHighlighting = true;
        private bool _sparqlMode = false;

        public EditorWindow()
        {
            InitializeComponent();
            
            //Load in out Highlighters
            try
            {
                //RDF/XML
                IHighlightingDefinition rdfxml = HighlightingManager.Instance.GetDefinition("XML");
                HighlightingManager.Instance.RegisterHighlighting("RdfXml", new String[] { ".rdf", ".owl" }, rdfxml);

                //NTriples
                IHighlightingDefinition nt = this.LoadHighlighting("ntriples.xshd");
                HighlightingManager.Instance.RegisterHighlighting("NTriples", new String[] { ".nt" }, nt);

                //Turtle
                IHighlightingDefinition ttl = this.LoadHighlighting("turtle.xshd");
                HighlightingManager.Instance.RegisterHighlighting("Turtle", new String[] { ".ttl", ".n3" }, ttl);

                //Notation 3
                IHighlightingDefinition n3 = this.LoadHighlighting("n3.xshd");
                HighlightingManager.Instance.RegisterHighlighting("Notation3", new String[] { ".n3" }, n3);

                //RDF/JSON
                IHighlightingDefinition json = this.LoadHighlighting("rdfjson.xshd");
                HighlightingManager.Instance.RegisterHighlighting("RdfJson", new String[] { ".json" }, json);

                //SPARQL Query
                IHighlightingDefinition rq = this.LoadHighlighting("sparql-query.xshd");
                HighlightingManager.Instance.RegisterHighlighting("SparqlQuery10", new String[] { ".rq" }, rq);
                IHighlightingDefinition rq11 = this.LoadHighlighting("sparql-query-11.xshd");
                HighlightingManager.Instance.RegisterHighlighting("SparqlQuery11", new String[] { ".rq" }, rq11);

                //SPARQL Results Set
                IHighlightingDefinition srx = HighlightingManager.Instance.GetDefinition("XML");
                HighlightingManager.Instance.RegisterHighlighting("SparqlResultsXml", new String[] { ".srx" }, srx);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
            

            //Set up the Editor Options
            TextEditorOptions options = new TextEditorOptions();
            options.EnableEmailHyperlinks = false;
            options.EnableHyperlinks = false;
            textEditor.Options = options;

            //Create our Dialogs
            _ofd.Title = "Open RDF File";
            _ofd.DefaultExt = ".rdf";
            _ofd.Filter = FileFilterRdf;
            _sfd.Title = "Save RDF File";
            _sfd.DefaultExt = ".rdf";
            _sfd.Filter = _ofd.Filter;
        }

        #region Editor Features - Highlighting etc

        private IHighlightingDefinition LoadHighlighting(String filename)
        {
            return HighlightingLoader.Load(XmlReader.Create(Assembly.GetExecutingAssembly().GetManifestResourceStream("rdfEditor." + filename)), HighlightingManager.Instance);
        }

        private void SetHighlighting(String filename)
        {
            if (textEditor == null) return; //Not yet ready

            if (!this._enableHighlighting) return;
            if (filename == null)
            {
                textEditor.SyntaxHighlighting = null;
                mnuNoHighlighter.IsChecked = true;
                this.SetHighlighterSelection(mnuNoHighlighter);
                return;
            }

            try
            {
                IHighlightingDefinition def = HighlightingManager.Instance.GetDefinitionByExtension(System.IO.Path.GetExtension(filename));
                textEditor.SyntaxHighlighting = def;

                if (def != null)
                {
                    switch (def.Name)
                    {
                        case "NTriples":
                            this.SetHighlighterSelection(mnuNTriplesHighlighter);
                            break;
                        case "Turtle":
                            this.SetHighlighterSelection(mnuTurtleHighlighter);
                            break;
                        case "Notation3":
                            this.SetHighlighterSelection(mnuN3Highlighter);
                            break;
                        case "RdfXml":
                            this.SetHighlighterSelection(mnuRdfXmlHighlighter);
                            break;
                        case "RdfJson":
                            if (this._sparqlMode)
                            {
                                this.SetHighlighterSelection(mnuSparqlResultsJsonHighlighter);
                            }
                            else
                            {
                                this.SetHighlighterSelection(mnuRdfJsonHighlighter);
                            }
                            break;
                        case "SparqlQuery10":
                            this.SetHighlighterSelection(mnuSparql10Highlighter);
                            break;
                        case "SparqlQuery11":
                            this.SetHighlighterSelection(mnuSparql11Highlighter);
                            break;
                        case "SparqlResultsXml":
                            this.SetHighlighterSelection(mnuSparqlResultsXmlHighlighter);
                            break;
                        default:
                            this.SetHighlighterSelection(mnuNoHighlighter);
                            break;
                    }
                }
                else
                {
                    this.SetHighlighterSelection(mnuNoHighlighter);
                }
            }
            catch
            {
                textEditor.SyntaxHighlighting = null;
                this.SetHighlighterSelection(mnuNoHighlighter);
            }
        }

        private void SetHighlighting(IRdfReader parser)
        {
            if (parser is NTriplesParser)
            {
                this.SetHighlighter("NTriples", mnuNTriplesHighlighter, true);
            }
            else if (parser is TurtleParser)
            {
                this.SetHighlighter("Turtle", mnuTurtleHighlighter, true);
            }
            else if (parser is Notation3Parser)
            {
                this.SetHighlighter("Notation3", mnuN3Highlighter, true);
            }
            else if (parser is RdfXmlParser)
            {
                this.SetHighlighter("RdfXml", mnuRdfXmlHighlighter, true);
            }
            else if (parser is RdfJsonParser)
            {
                if (this._sparqlMode)
                {
                    this.SetHighlighter("SparqlResultsJson", mnuSparqlResultsJsonHighlighter, true);
                }
                else
                {
                    this.SetHighlighter("RdfJson", mnuRdfJsonHighlighter, true);
                }
            }
            else
            {
                textEditor.SyntaxHighlighting = null;
                this.SetHighlighterSelection(mnuNoHighlighter);
            }
        }

        private void SetHighlighterSelection(MenuItem selected)
        {
            foreach (MenuItem item in mnuCurrentHighlighter.Items.OfType<MenuItem>())
            {
                item.IsChecked = ReferenceEquals(item, selected);
            }
        }

        private void SetHighlighter(String name, MenuItem selected, bool forceChecked)
        {
            if (forceChecked) selected.IsChecked = true;
            this.SetHighlighter(name, selected);
        }

        private void SetHighlighter(String name, MenuItem selected)
        {
            if (selected.IsChecked)
            {
                textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition(name);
                this.SetHighlighterSelection(selected);
            }
            else
            {
                textEditor.SyntaxHighlighting = null;
                this.SetHighlighterSelection(mnuNoHighlighter);
            }
        }

        private void SetSparqlMode(MenuItem selected)
        {
            bool enabled = selected.IsChecked;
            this._sparqlMode = enabled;

            //Set non-SPARQL Highlighters to appropriate enabled state
            this.mnuRdfXmlHighlighter.IsEnabled = !enabled;
            this.mnuNTriplesHighlighter.IsEnabled = !enabled;
            this.mnuN3Highlighter.IsEnabled = !enabled;
            this.mnuTurtleHighlighter.IsEnabled = !enabled;
            this.mnuRdfJsonHighlighter.IsEnabled = !enabled;

            //Set Tools to appropriate enabled state
            this.mnuValidateRdf.IsEnabled = !enabled;
            this.mnuValidateSparqlQuery10.IsEnabled = enabled;
            this.mnuValidateSparqlQuery11.IsEnabled = enabled;

            if (enabled)
            {
                if (ReferenceEquals(this.mnuSparql10Highlighter, selected))
                {
                    this.SetHighlighter("SparqlQuery10", mnuSparql10Highlighter);
                }
                else if (ReferenceEquals(this.mnuSparql11Highlighter, selected))
                {
                    this.SetHighlighter("SparqlQuery11", mnuSparql11Highlighter);
                } 
                else if (ReferenceEquals(this.mnuSparqlResultsXmlHighlighter, selected))
                {
                    this.SetHighlighter("SparqlResultsXml", mnuSparqlResultsXmlHighlighter);
                }
                else if (ReferenceEquals(this.mnuSparqlResultsJsonHighlighter, selected))
                {
                    this.SetHighlighter("RdfJson", mnuSparqlResultsJsonHighlighter);
                }

                _sfd.Filter = FileFilterSparql;
                _ofd.Filter = FileFilterSparql;
                _sfd.DefaultExt = ".rq";
                _ofd.DefaultExt = ".rq";
            }
            else
            {
                textEditor.SyntaxHighlighting = null;
                this.SetHighlighterSelection(mnuNoHighlighter);

                _sfd.Filter = FileFilterRdf;
                _ofd.Filter = FileFilterRdf;
                _sfd.DefaultExt = ".rdf";
                _ofd.DefaultExt = ".rdf";
            }
        }

        #endregion

        #region File IO

        private void mnuOpen_Click(object sender, RoutedEventArgs e)
        {
            if (_ofd.ShowDialog() == true)
            {
                try
                {
                    using (StreamReader reader = new StreamReader(_ofd.FileName))
                    {
                        String text = reader.ReadToEnd();
                        textEditor.Text = String.Empty;
                        this.SetHighlighting(_ofd.FileName);
                        textEditor.Text = text;
                    }
                    this._currFile = _ofd.FileName;
                    this.Title = "rdfEditor - " + System.IO.Path.GetFileName(this._currFile);
                    this._changed = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while opening the selected file: " + ex.Message, "Unable to Open File");
                }
            }
        }

        private void mnuOpenUri_Click(object sender, RoutedEventArgs e)
        {
            if (this._currFile != null && this._changed)
            {
                MessageBoxResult res = MessageBox.Show("Would you like to save changes to the current file before opening a URI?", "Save Changes?", MessageBoxButton.YesNoCancel);
                if (res == MessageBoxResult.Cancel)
                {
                    return;
                }
                else if (res == MessageBoxResult.Yes)
                {
                    mnuSave_Click(sender, e);
                }
                mnuClose_Click(sender, e);
            }

            OpenUri diag = new OpenUri();
            if (diag.ShowDialog() == true)
            {
                textEditor.Text = diag.RetrievedData;
                this._changed = true;
                this.SetHighlighting(diag.Parser);
            }
        }

        private void mnuSave_Click(object sender, RoutedEventArgs e)
        {
            if (this._currFile != null)
            {
                StreamWriter fileWriter;
                try
                {
                    IRdfWriter writer = MimeTypesHelper.GetWriter(MimeTypesHelper.GetMimeType(System.IO.Path.GetExtension(this._currFile)));

                    //Use ASCII for NTriples and UTF-8 otherwise
                    if (writer is NTriplesWriter)
                    {
                        fileWriter = new StreamWriter(this._currFile, false, Encoding.ASCII);
                    }
                    else
                    {
                        fileWriter = new StreamWriter(this._currFile, false, Encoding.UTF8);
                    }
                }
                catch
                {
                    //Ignore and just create a UTF-8 Stream
                    fileWriter = new StreamWriter(this._currFile, false, Encoding.UTF8);
                }

                try
                {
                    fileWriter.Write(textEditor.Text);
                    this.Title = "rdfEditor - " + System.IO.Path.GetFileName(this._currFile);
                    fileWriter.Close();
                    this._changed = false;
                }
                catch (Exception ex)
                {
                    fileWriter.Close();
                    MessageBox.Show("An error occurred while saving: " + ex.Message, "Save Failed");
                }
            }
            else
            {
                mnuSaveAs_Click(sender, e);
            }
        }

        private void mnuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            if (this._sfd.ShowDialog() == true)
            {
                this._currFile = this._sfd.FileName;
                mnuSave_Click(sender, e);
                this.SetHighlighting(this._currFile);
            }
        }

        private void mnuClose_Click(object sender, RoutedEventArgs e)
        {
            if (this._changed)
            {
                //Prompt user to save
                MessageBoxResult result = MessageBox.Show("Do you wish to save changes to the current file before closing it?", "Save Changes", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
                else if (result == MessageBoxResult.Yes)
                {
                    mnuSave_Click(sender, e);
                }
                else if (result == MessageBoxResult.No)
                {
                    return;
                }
            }

            textEditor.Text = String.Empty;
            this._changed = false;
            this._currFile = null;
            this.Title = "rdfEditor";
            textEditor.SyntaxHighlighting = null;
            this.SetHighlighterSelection(mnuNoHighlighter);
        }

        private void SaveWith(IRdfWriter writer)
        {
            IRdfReader parser = this.SelectParser();
            Graph g = new Graph();
            try
            {
                StringParser.Parse(g, textEditor.Text, parser);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to Save With an RDF Writer as the current file is not a valid RDF document when parsed with the " + parser.GetType().Name + ".  If you believe this is a valid RDF document please select the correct Syntax Highlighting from the Options Menu and retry", "Save With Failed");
                return;
            }

            bool filenameRequired = (this._currFile == null);
            if (!filenameRequired)
            {
                MessageBoxResult res = MessageBox.Show("Are you sure you wish to overwrite your existing file with the output of the " + writer.GetType().Name + "?  Click Yes to proceed, No to select a different Filename or Cancel to abort this operation", "Overwrite File",MessageBoxButton.YesNoCancel);
                if (res == MessageBoxResult.Cancel)
                {
                    return;
                }
                else if (res == MessageBoxResult.No)
                {
                    filenameRequired = true;
                }
                else if (res == MessageBoxResult.None)
                {
                    return;
                }
            }

            //Get a Filename to Save to
            String origFilename = this._currFile;
            if (filenameRequired)
            {
                if (_sfd.ShowDialog() == true)
                {
                    this._currFile = _sfd.FileName;
                }
                else
                {
                    return;
                }
            }


            try
            {
                writer.Save(g, this._currFile);

                MessageBoxResult res = MessageBox.Show("Would you like to switch editing to the newly created file?", "Switch Editing", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (StreamReader reader = new StreamReader(this._currFile))
                        {
                            String text = reader.ReadToEnd();
                            textEditor.Text = String.Empty;
                            this.SetHighlighting(this._currFile);
                            textEditor.Text = text;
                        }
                        this.Title = "rdfEditor - " + System.IO.Path.GetFileName(this._currFile);
                        this._changed = false;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred while opening the selected file: " + ex.Message, "Unable to Open File");
                    }
                }
                else
                {
                    if (origFilename != null)
                    {
                        this._currFile = origFilename;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while saving: " + ex.Message, "Save With Failed");
            }
        }

        private void mnuSaveWithNTriples_Click(object sender, RoutedEventArgs e)
        {
            this.SaveWith(new NTriplesWriter());
        }

        private void mnuSaveWithTurtle_Click(object sender, RoutedEventArgs e)
        {
            this.SaveWith(new CompressingTurtleWriter(WriterCompressionLevel.High));
        }

        private void mnuSaveWithN3_Click(object sender, RoutedEventArgs e)
        {
            this.SaveWith(new Notation3Writer());
        }

        private void mnuSaveWithRdfXml_Click(object sender, RoutedEventArgs e)
        {
            this.SaveWith(new FastRdfXmlWriter());
        }

        private void mnuSaveWithRdfJson_Click(object sender, RoutedEventArgs e)
        {
            this.SaveWith(new RdfJsonWriter());
        }

        private void mnuSaveWithRdfa_Click(object sender, RoutedEventArgs e)
        {
            this.SaveWith(new HtmlWriter());
        }

        #endregion

        private void textEditor_TextChanged(object sender, EventArgs e)
        {
            this._changed = true;
        }

        #region Options Menu

        private void mnuEnableHighlighting_Click(object sender, RoutedEventArgs e)
        {
            this._enableHighlighting = mnuEnableHighlighting.IsChecked;
            if (mnuCurrentHighlighter != null) mnuCurrentHighlighter.IsEnabled = this._enableHighlighting;
            if (this._enableHighlighting)
            {
                this.SetHighlighting(this._currFile);
            }
            else
            {
                textEditor.SyntaxHighlighting = null;
            }
        }

        private void mnuRdfXmlHighlighter_Click(object sender, RoutedEventArgs e)
        {
            this.SetHighlighter("RdfXml", mnuRdfXmlHighlighter);
        }

        private void mnuRdfJsonHighlighter_Click(object sender, RoutedEventArgs e)
        {
            this.SetHighlighter("RdfJson", mnuRdfJsonHighlighter);
        }

        private void mnuNTriplesHighlighter_Click(object sender, RoutedEventArgs e)
        {
            this.SetHighlighter("NTriples", mnuNTriplesHighlighter);
        }

        private void mnuTurtleHighlighter_Click(object sender, RoutedEventArgs e)
        {
            this.SetHighlighter("Turtle", mnuTurtleHighlighter);
        }

        private void mnuN3Highlighter_Click(object sender, RoutedEventArgs e)
        {
            this.SetHighlighter("Notation3", mnuN3Highlighter);
        }

        private void mnuNoHighlighter_Click(object sender, RoutedEventArgs e)
        {
            textEditor.SyntaxHighlighting = null;
            this.SetHighlighterSelection(mnuNoHighlighter);
        }

        private void mnuSparql10Highlighter_Click(object sender, RoutedEventArgs e)
        {
            this.SetSparqlMode(mnuSparql10Highlighter);
        }

        private void mnuSparql11Highlighter_Click(object sender, RoutedEventArgs e)
        {
            this.SetSparqlMode(mnuSparql11Highlighter);
        }

        private void mnuSparqlResultsXmlHighlighter_Click(object sender, RoutedEventArgs e)
        {
            this.SetSparqlMode(mnuSparqlResultsXmlHighlighter);
        }

        private void mnuSparqlResultsJsonHighlighter_Click(object sender, RoutedEventArgs e)
        {
            this.SetSparqlMode(mnuSparqlResultsJsonHighlighter);
        }

        #endregion

        #region Tools Menu

        private IRdfReader SelectParser()
        {
            IRdfReader parser = null;
            if (this._enableHighlighting)
            {
                if (textEditor.SyntaxHighlighting != null)
                {
                    switch (textEditor.SyntaxHighlighting.Name)
                    {
                        case "NTriples":
                            parser = new NTriplesParser();
                            break;
                        case "Turtle":
                            parser = new TurtleParser();
                            break;
                        case "Notation3":
                            parser = new Notation3Parser();
                            break;
                        case "RdfXml":
                        case "XML":
                            parser = new RdfXmlParser();
                            break;
                        default:
                            parser = new NTriplesParser();
                            break;
                    }
                }
                else
                {
                    parser = StringParser.GetParser(textEditor.Text);
                }
            }
            else if (this._currFile != null)
            {
                try
                {
                    parser = MimeTypesHelper.GetParser(MimeTypesHelper.GetMimeType(System.IO.Path.GetExtension(this._currFile)));
                }
                catch (RdfParserSelectionException)
                {
                    parser = StringParser.GetParser(textEditor.Text);
                }
            }
            else
            {
                parser = StringParser.GetParser(textEditor.Text);
            }
            return parser;
        }

        private void mnuValidateRdf_Click(object sender, RoutedEventArgs e)
        {
            IRdfReader parser = this.SelectParser();

            try
            {
                Graph g = new Graph();
                StringParser.Parse(g, textEditor.Text, parser);

                MessageBox.Show("RDF appears to be valid - " + g.Triples.Count + " Triples parsed using parser " + parser.GetType().Name, "Valid RDF");
            }
            catch (RdfParseException parseEx)
            {
                MessageBox.Show("Invalid RDF due to parsing error using parser " + parser.GetType().Name + "\n\n" + parseEx.Message, "Invalid RDF - Parser Error");
            }
            catch (RdfException rdfEx)
            {
                MessageBox.Show("Invalid RDF due to RDF error using parser " + parser.GetType().Name + "\n\n" + rdfEx.Message, "Invalid RDF - RDF Error");
            } 
            catch (Exception ex)
            {
                MessageBox.Show("Invalid RDF due to error using parser " + parser.GetType().Name + "\n\n" + ex.Message, "Invalid RDF - Error");
            }
        }

        private void mnuValidateSparqlQuery10_Click(object sender, RoutedEventArgs e)
        {
            if (!this._sparqlMode) return;

            //Compute the actual syntax compatability
            SparqlQueryParser parser = new SparqlQueryParser();
            parser.SyntaxMode = SparqlQuerySyntax.Sparql_1_0;
            try
            {
                SparqlQuery q = parser.ParseFromString(textEditor.Text);
                MessageBox.Show("This query is valid according to the SPARQL 1.0 specification", "Valid SPARQL");
            }
            catch (RdfParseException parseEx)
            {
                MessageBox.Show("Invalid SPARQL 1.0 Query due to parsing error\n\n" + parseEx.Message, "Invalid SPARQL 1.0 Query - Parser Error");
            }
            catch (RdfException rdfEx)
            {
                MessageBox.Show("Invalid SPARQL 1.0 Query due to RDF error\n\n" + rdfEx.Message, "Invalid SPARQL 1.0 Query - RDF Error");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Invalid SPARQL 1.0 Query due to error\n\n" + ex.Message, "Invalid SPARQL 1.0 Query - Error");
            }
        }

        private void mnuValidateSparqlQuery11_Click(object sender, RoutedEventArgs e)
        {
            if (!this._sparqlMode) return;

            //Compute the actual syntax compatability
            SparqlQueryParser parser = new SparqlQueryParser();
            parser.SyntaxMode = SparqlQuerySyntax.Sparql_1_1;
            try
            {
                SparqlQuery q = parser.ParseFromString(textEditor.Text);
                MessageBox.Show("This query is valid according to the SPARQL 1.1 specification", "Valid SPARQL");
            }
            catch (RdfParseException parseEx)
            {
                MessageBox.Show("Invalid SPARQL 1.1 Query due to parsing error\n\n" + parseEx.Message, "Invalid SPARQL 1.1 Query - Parser Error");
            }
            catch (RdfException rdfEx)
            {
                MessageBox.Show("Invalid SPARQL 1.1 Query due to RDF error\n\n" + rdfEx.Message, "Invalid SPARQL 1.1 Query - RDF Error");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Invalid SPARQL 1.1 Query due to error\n\n" + ex.Message, "Invalid SPARQL 1.1 Query - Error");
            }
        }

        #endregion
    }
}
