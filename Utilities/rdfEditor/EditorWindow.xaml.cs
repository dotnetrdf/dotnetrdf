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
using VDS.RDF.Writing;

namespace rdfEditor
{
    /// <summary>
    /// Interaction logic for EditorWindow.xaml
    /// </summary>
    public partial class EditorWindow : Window
    {
        private OpenFileDialog _ofd = new OpenFileDialog();
        private SaveFileDialog _sfd = new SaveFileDialog();
        private String _currFile = null;
        private bool _changed = false;
        private bool _enableHighlighting = true;

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
            _ofd.Filter = "All Supported RDF Files|*.rdf;*.ttl;*.n3;*.nt;*.json;*.owl|RDF/XML Files|*.rdf,*.owl|NTriples Files|*.nt|Turtle Files|*.ttl|Notation 3 Files|*.n3|RDF/JSON Files|*.json";
            _sfd.Title = "Save RDF File";
            _sfd.DefaultExt = ".rdf";
            _sfd.FileName = _ofd.Filter;
        }

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

        private void SetHighlighterSelection(MenuItem selected)
        {
            foreach (MenuItem item in mnuCurrentHighlighter.Items.OfType<MenuItem>())
            {
                item.IsChecked = ReferenceEquals(item, selected);
            }
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
            }

            textEditor.Text = String.Empty;
            this._changed = false;
            this._currFile = null;
            this.Title = "rdfEditor";
            textEditor.SyntaxHighlighting = null;
            this.SetHighlighterSelection(mnuNoHighlighter);
        }

        private void textEditor_TextChanged(object sender, EventArgs e)
        {
            this._changed = true;
        }

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

        private void mnuValidateRdf_Click(object sender, RoutedEventArgs e)
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
    }
}
