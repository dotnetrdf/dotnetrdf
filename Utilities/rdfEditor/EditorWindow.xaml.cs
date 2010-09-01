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
using rdfEditor.Syntax;

namespace rdfEditor
{
    /// <summary>
    /// Interaction logic for EditorWindow.xaml
    /// </summary>
    public partial class EditorWindow : Window
    {
        private const String FileFilterRdf = "All Supported RDF Files|*.rdf;*.ttl;*.n3;*.nt;*.json;*.owl;*.html;*.xhtml;*.htm;*.shtml|RDF/XML Files|*.rdf,*.owl|NTriples Files|*.nt|Turtle Files|*.ttl|Notation 3 Files|*.n3|RDF/JSON Files|*.json|(X)HTML+RDFa Files|*.html;*.xhtml;*.htm;*.shtml|All Files|*.*";
        private const String FileFilterSparql = "All Supported SPARQL Files|*.rq;*.sparql;*.srx|SPARQL Query Files|*.rq;*.sparql|SPARQL Results Files|*.srx;*.json";
        private const String FileFilterAll = "All Supported RDF and SPARQL Files|*.rdf;*.ttl;*.n3;*.nt;*.json;*.owl;*.html;*.xhtml;*.htm;*.shtml;*.rq;*.sparql;*.srx|All Supported RDF Files|*.rdf;*.ttl;*.n3;*.nt;*.json;*.owl|All Supported SPARQL Files|*.rq;*.sparql;*.srx|RDF/XML Files|*.rdf,*.owl|NTriples Files|*.nt|Turtle Files|*.ttl|Notation 3 Files|*.n3|RDF/JSON Files|*.json|(X)HTML+RDFa Files|*.html;*.xhtml;*.htm;*.shtml|SPARQL Query Files|*.rq;*.sparql|SPARQL Results Files|*.srx;*.json|All Files|*.*";

        private OpenFileDialog _ofd = new OpenFileDialog();
        private SaveFileDialog _sfd = new SaveFileDialog();
        private EditorManager _manager;

        public EditorWindow()
        {
            InitializeComponent();

            this._manager = new EditorManager(this.textEditor, this.mnuCurrentHighlighter, this.stsSyntaxValidation);

            //Set up the Editor Options
            TextEditorOptions options = new TextEditorOptions();
            options.EnableEmailHyperlinks = false;
            options.EnableHyperlinks = false;
            textEditor.Options = options;

            //Create our Dialogs
            _ofd.Title = "Open RDF/SPARQL File";
            _ofd.DefaultExt = ".rdf";
            _ofd.Filter = FileFilterAll;
            _sfd.Title = "Save RDF/SPARQL File";
            _sfd.DefaultExt = ".rdf";
            _sfd.Filter = _ofd.Filter;
        }

        #region File Menu

        private void mnuNew_Click(object sender, RoutedEventArgs e)
        {
            mnuClose_Click(sender, e);
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
                        textEditor.Text = text;
                        this._manager.AutoDetectSyntaxHighlighter(_ofd.FileName);
                    }
                    this._manager.CurrentFile = _ofd.FileName;
                    this.Title = "rdfEditor - " + System.IO.Path.GetFileName(this._manager.CurrentFile);
                    this._manager.HasChanged = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while opening the selected file: " + ex.Message, "Unable to Open File");
                }
            }
        }

        private void mnuOpenUri_Click(object sender, RoutedEventArgs e)
        {
            if (this._manager.HasChanged)
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
                this._manager.HasChanged = false;
                mnuClose_Click(sender, e);
            }

            OpenUri diag = new OpenUri();
            if (diag.ShowDialog() == true)
            {
                textEditor.Text = diag.RetrievedData;
                this._manager.HasChanged = true;
                this._manager.SetHighlighter(diag.Parser);
            }
        }

        private void mnuOpenQueryResults_Click(object sender, RoutedEventArgs e)
        {
            if (this._manager.HasChanged)
            {
                MessageBoxResult res = MessageBox.Show("Would you like to save changes to the current file before opening Query Results?", "Save Changes?", MessageBoxButton.YesNoCancel);
                if (res == MessageBoxResult.Cancel)
                {
                    return;
                }
                else if (res == MessageBoxResult.Yes)
                {
                    mnuSave_Click(sender, e);
                }
                this._manager.HasChanged = false;
                mnuClose_Click(sender, e);
            }

            OpenQueryResults diag = new OpenQueryResults();
            if (diag.ShowDialog() == true)
            {
                textEditor.Text = diag.RetrievedData;
                this._manager.HasChanged = true;
                this._manager.SetHighlighter(diag.Parser);
            }
        }

        private void mnuSave_Click(object sender, RoutedEventArgs e)
        {
            if (this._manager.CurrentFile != null)
            {
                StreamWriter fileWriter;
                try
                {
                    IRdfWriter writer = MimeTypesHelper.GetWriter(MimeTypesHelper.GetMimeType(System.IO.Path.GetExtension(this._manager.CurrentFile)));

                    //Use ASCII for NTriples and UTF-8 otherwise
                    if (writer is NTriplesWriter)
                    {
                        fileWriter = new StreamWriter(this._manager.CurrentFile, false, Encoding.ASCII);
                    }
                    else
                    {
                        fileWriter = new StreamWriter(this._manager.CurrentFile, false, Encoding.UTF8);
                    }
                }
                catch
                {
                    //Ignore and just create a UTF-8 Stream
                    fileWriter = new StreamWriter(this._manager.CurrentFile, false, Encoding.UTF8);
                }

                try
                {
                    fileWriter.Write(textEditor.Text);
                    this.Title = "rdfEditor - " + System.IO.Path.GetFileName(this._manager.CurrentFile);
                    fileWriter.Close();
                    this._manager.HasChanged = false;
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
                this._manager.CurrentFile = this._sfd.FileName;
                mnuSave_Click(sender, e);
                this._manager.AutoDetectSyntaxHighlighter(this._manager.CurrentFile);
            }
        }

        private void SaveWith(IRdfWriter writer)
        {
            IRdfReader parser = this._manager.GetParser();
            Graph g = new Graph();
            try
            {
                StringParser.Parse(g, textEditor.Text, parser);
            }
            catch
            {
                MessageBox.Show("Unable to Save With an RDF Writer as the current file is not a valid RDF document when parsed with the " + parser.GetType().Name + ".  If you believe this is a valid RDF document please select the correct Syntax Highlighting from the Options Menu and retry", "Save With Failed");
                return;
            }

            bool filenameRequired = (this._manager.CurrentFile == null);
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
            String origFilename = this._manager.CurrentFile;
            if (filenameRequired)
            {
                if (_sfd.ShowDialog() == true)
                {
                    this._manager.CurrentFile = _sfd.FileName;
                }
                else
                {
                    return;
                }
            }


            try
            {
                writer.Save(g, this._manager.CurrentFile);

                MessageBoxResult res = MessageBox.Show("Would you like to switch editing to the newly created file?", "Switch Editing", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (StreamReader reader = new StreamReader(this._manager.CurrentFile))
                        {
                            String text = reader.ReadToEnd();
                            textEditor.Text = String.Empty;
                            this._manager.AutoDetectSyntaxHighlighter(this._manager.CurrentFile);
                            textEditor.Text = text;
                        }
                        this.Title = "rdfEditor - " + System.IO.Path.GetFileName(this._manager.CurrentFile);
                        this._manager.HasChanged = false;
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
                        this._manager.CurrentFile = origFilename;
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

        private void mnuClose_Click(object sender, RoutedEventArgs e)
        {
            if (this._manager.HasChanged)
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
            this._manager.HasChanged = false;
            this._manager.CurrentFile = null;
            this.Title = "rdfEditor";
            this._manager.SetNoHighlighting();
        }

        private void mnuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        #endregion

        #region Options Menu

        private void mnuEnableHighlighting_Click(object sender, RoutedEventArgs e)
        {
            this._manager.IsHighlightingEnabled = mnuEnableHighlighting.IsChecked;
        }

        private void mnuValidateAsYouType_Click(object sender, RoutedEventArgs e)
        {
            this._manager.IsValidateAsYouType = this.mnuValidateAsYouType.IsChecked;
            if (this._manager.IsValidateAsYouType)
            {
                this.stsSyntaxValidation.Content = "Validate Syntax as you Type Enabled";
                this._manager.DoValidation();
            }
            else
            {
                this.stsSyntaxValidation.Content = "Validate Syntax as you Type Disabled";
            }
        }

        private void mnuAutoComplete_Click(object sender, RoutedEventArgs e)
        {
            this._manager.IsAutoCompleteEnabled = this.mnuAutoComplete.IsChecked;
        }

        #endregion

        #region Tools Menu

        private void mnuValidateSyntax_Click(object sender, RoutedEventArgs e)
        {
            String syntax = (this._manager.CurrentHighlighter == null) ? "None" : this._manager.CurrentHighlighter.Name;
            ISyntaxValidator validator = SyntaxManager.GetValidator(syntax);
            if (validator != null)
            {
                String message;
                String caption = (validator.Validate(textEditor.Text, out message)) ? "Valid Syntax" : "Invalid Syntax";
                MessageBox.Show(message, caption);
            }
            else
            {
                MessageBox.Show("Validation is not possible as there is no Syntax Validator registered for your currently selected Syntax Highlighting", "Validation Unavailable");
            }
        }

        #endregion
    }
}
