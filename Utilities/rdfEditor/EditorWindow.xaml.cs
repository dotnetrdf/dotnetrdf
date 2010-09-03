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
        private const String FileFilterRdf = "All Supported RDF Files|*.rdf;*.ttl;*.n3;*.nt;*.json;*.owl;*.html;*.xhtml;*.htm;*.shtml|RDF/XML Files|*.rdf,*.owl|NTriples Files|*.nt|Turtle Files|*.ttl|Notation 3 Files|*.n3|RDF/JSON Files|*.json|(X)HTML+RDFa Files|*.html;*.xhtml;*.htm;*.shtml";
        private const String FileFilterSparql = "All Supported SPARQL Files|*.rq;*.sparql;*.srx|SPARQL Query Files|*.rq;*.sparql|SPARQL Results Files|*.srx;*.json";
        private const String FileFilterRdfDataset = "All Supported RDF Dataset Files|*.nq;*.trig;*.xml|NQuads Files|*.nq|TriG Files|*.trig|TriX Files|*.xml";
        private readonly String FileFilterAll = "All Supported RDF and SPARQL Files|*.rdf;*.ttl;*.n3;*.nt;*.json;*.owl;*.html;*.xhtml;*.htm;*.shtml;*.rq;*.sparql;*.srx;*.nq;*.trig;*.xml|" + FileFilterRdf + "|" + FileFilterSparql + "|" + FileFilterRdfDataset;

        private OpenFileDialog _ofd = new OpenFileDialog();
        private SaveFileDialog _sfd = new SaveFileDialog();
        private EditorManager _manager;

        public EditorWindow()
        {
            InitializeComponent();

            this._manager = new EditorManager(this.textEditor, this.mnuCurrentHighlighter, this.stsSyntaxValidation);
          
            //Set up the Editor Options
            TextEditorOptions options = new TextEditorOptions();
            options.EnableEmailHyperlinks = Properties.Settings.Default.EnableClickableUris;
            options.EnableHyperlinks = Properties.Settings.Default.EnableClickableUris;
            textEditor.Options = options;
            textEditor.ShowLineNumbers = true;

            //Setup Options based on the User Config file
            if (!Properties.Settings.Default.EnableAutoComplete) 
            {
                this._manager.IsAutoCompleteEnabled = false;
                this.mnuAutoComplete.IsChecked = false;
            }
            if (!Properties.Settings.Default.EnableHighlighting)
            {
                this._manager.IsHighlightingEnabled = false;
                this.mnuEnableHighlighting.IsChecked = false;
            }
            if (!Properties.Settings.Default.EnableValidateAsYouType)
            {
                this._manager.IsValidateAsYouType = false;
                this.mnuValidateAsYouType.IsChecked = false;
            }
            if (!Properties.Settings.Default.ShowLineNumbers)
            {
                textEditor.ShowLineNumbers = false;
                this.mnuShowLineNumbers.IsChecked = false;
            }
            if (Properties.Settings.Default.WordWrap)
            {
                textEditor.WordWrap = false;
                this.mnuWordWrap.IsChecked = true;
            }
            if (Properties.Settings.Default.EnableClickableUris)
            {
                this.mnuClickableUris.IsChecked = true;
            }
            this._manager.SetHighlighter(Properties.Settings.Default.DefaultHighlighter);

            //Enable/Disable state dependendet menu options
            this.mnuUndo.IsEnabled = textEditor.CanUndo;
            this.mnuRedo.IsEnabled = textEditor.CanRedo;

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
                        this._manager.AutoDetectSyntax(_ofd.FileName);
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
                if (diag.Parser != null)
                {
                    this._manager.SetHighlighter(diag.Parser);
                }
                else
                {
                    this._manager.AutoDetectSyntax();
                }
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
                this._manager.AutoDetectSyntax(this._manager.CurrentFile);
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
                            textEditor.Text = text;
                            this._manager.AutoDetectSyntax(this._manager.CurrentFile);
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
            mnuClose_Click(sender, e);
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            Application.Current.Shutdown();
        }

        #endregion

        #region Edit Menu

        private void mnuUndo_Click(object sender, RoutedEventArgs e)
        {
            if (textEditor.CanUndo)
            {
                textEditor.Undo();
            }
        }

        private void mnuRedo_Click(object sender, RoutedEventArgs e)
        {
            if (textEditor.CanRedo)
            {
                textEditor.Redo();
            }
        }


        private void mnuCut_Click(object sender, RoutedEventArgs e)
        {
            textEditor.Cut();
        }

        private void mnuCopy_Click(object sender, RoutedEventArgs e)
        {
            textEditor.Copy();
        }

        private void mnuPaste_Click(object sender, RoutedEventArgs e)
        {
            textEditor.Paste();
        }

        #endregion

        #region View Menu

        private void mnuShowLineNumbers_Click(object sender, RoutedEventArgs e)
        {
            textEditor.ShowLineNumbers = this.mnuShowLineNumbers.IsChecked;
            Properties.Settings.Default.ShowLineNumbers = this.mnuShowLineNumbers.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void mnuWordWrap_Click(object sender, RoutedEventArgs e)
        {
            textEditor.WordWrap = this.mnuWordWrap.IsChecked;
            Properties.Settings.Default.WordWrap = this.mnuWordWrap.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void mnuClickableUris_Click(object sender, RoutedEventArgs e)
        {
            textEditor.Options.EnableEmailHyperlinks = this.mnuClickableUris.IsChecked;
            textEditor.Options.EnableHyperlinks = this.mnuClickableUris.IsChecked;
            Properties.Settings.Default.EnableClickableUris = this.mnuClickableUris.IsChecked;
            Properties.Settings.Default.Save();
        }

        #endregion

        #region Options Menu

        private void mnuEnableHighlighting_Click(object sender, RoutedEventArgs e)
        {
            this._manager.IsHighlightingEnabled = mnuEnableHighlighting.IsChecked;
            Properties.Settings.Default.EnableHighlighting = this.mnuEnableHighlighting.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void mnuSetDefaultHighlighter_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.DefaultHighlighter = (this._manager.CurrentHighlighter != null) ? this._manager.CurrentHighlighter.Name : "None";
            Properties.Settings.Default.Save();
        }

        private void mnuValidateAsYouType_Click(object sender, RoutedEventArgs e)
        {
            this._manager.IsValidateAsYouType = this.mnuValidateAsYouType.IsChecked;
            Properties.Settings.Default.EnableValidateAsYouType = this.mnuValidateAsYouType.IsChecked;
            Properties.Settings.Default.Save();
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

        private void mnuHighlightErrors_Click(object sender, RoutedEventArgs e)
        {
            this._manager.IsHighlightErrorsEnabled = this.mnuHighlightErrors.IsChecked;
            Properties.Settings.Default.EnableErrorHighlighting = this.mnuHighlightErrors.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void mnuAutoComplete_Click(object sender, RoutedEventArgs e)
        {
            this._manager.IsAutoCompleteEnabled = this.mnuAutoComplete.IsChecked;
            Properties.Settings.Default.EnableAutoComplete = this.mnuAutoComplete.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void mnuCustomiseFileAssociations_Click(object sender, RoutedEventArgs e)
        {
            FileAssociations diag = new FileAssociations();
            diag.ShowDialog();
        }

        #endregion

        #region Tools Menu

        private void mnuValidateSyntax_Click(object sender, RoutedEventArgs e)
        {
            ISyntaxValidator validator = this._manager.CurrentValidator;
            if (validator != null)
            {
                ISyntaxValidationResults results = validator.Validate(textEditor.Text);
                String caption = results.IsValid ? "Valid Syntax" : "Invalid Syntax";
                MessageBox.Show(results.Message, caption);
                if (!this._manager.IsValidateAsYouType && this._manager.IsHighlightErrorsEnabled)
                {
                    this._manager.LastValidationError = results.Error;
                    textEditor.TextArea.InvalidateVisual();
                }
            }
            else
            {
                MessageBox.Show("Validation is not possible as there is no Syntax Validator registered for your currently selected Syntax Highlighting", "Validation Unavailable");
            }
        }

        #endregion

        #region Command Bindings for creating Keyboard Shortcuts

        private void NewCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            mnuNew_Click(sender, e);
        }

        private void OpenCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            mnuOpen_Click(sender, e);
        }

        private void SaveCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            mnuSave_Click(sender, e);
        }

        private void SaveAsCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            mnuSaveAs_Click(sender, e);
        }

        private void SaveWithNTriplesExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            mnuSaveWithNTriples_Click(sender, e);
        }

        private void SaveWithTurtleExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            mnuSaveWithTurtle_Click(sender, e);
        }

        private void SaveWithN3Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mnuSaveWithN3_Click(sender, e);
        }

        private void SaveWithRdfXmlExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            mnuSaveWithRdfXml_Click(sender, e);
        }

        private void SaveWithRdfJsonExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            mnuSaveWithRdfJson_Click(sender, e);
        }

        private void SaveWithXHtmlRdfAExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            mnuSaveWithRdfa_Click(sender, e);
        }

        private void CloseCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            mnuClose_Click(sender, e);
        }

        private void UndoCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            mnuUndo_Click(sender, e);
        }

        private void RedoCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            mnuRedo_Click(sender, e);
        }

        private void CutCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            mnuCut_Click(sender, e);
        }

        private void CopyCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            mnuCopy_Click(sender, e);
        }

        private void PasteCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            mnuPaste_Click(sender, e);
        }

        private void ToggleLineNumbersExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.mnuShowLineNumbers.IsChecked = !this.mnuShowLineNumbers.IsChecked;
            mnuShowLineNumbers_Click(sender, e);
        }

        private void ToggleWordWrapExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.mnuWordWrap.IsChecked = !this.mnuWordWrap.IsChecked;
            mnuWordWrap_Click(sender, e);
        }

        private void ToggleHighlightingExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.mnuEnableHighlighting.IsChecked = !this.mnuEnableHighlighting.IsChecked;
            mnuEnableHighlighting_Click(sender, e);
        }

        private void ToggleValidateAsYouTypeExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.mnuValidateAsYouType.IsChecked = !this.mnuValidateAsYouType.IsChecked;
            mnuValidateAsYouType_Click(sender, e);
        }

        private void ToggleAutoCompletionExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.mnuAutoComplete.IsChecked = !this.mnuAutoComplete.IsChecked;
            mnuAutoComplete_Click(sender, e);
        }

        #endregion

        #region Misc Event Handlers

        private void textEditor_TextChanged(object sender, EventArgs e)
        {
            this.mnuUndo.IsEnabled = textEditor.CanUndo;
            this.mnuRedo.IsEnabled = textEditor.CanRedo;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Open a File if we've been asked to do so
            String[] args = Environment.GetCommandLineArgs();
            if (args.Length >= 2)
            {
                if (File.Exists(args[1]))
                {
                    try
                    {
                        using (StreamReader reader = new StreamReader(args[1]))
                        {
                            String text = reader.ReadToEnd();
                            textEditor.Text = String.Empty;
                            textEditor.Text = text;
                            this._manager.AutoDetectSyntax(args[1]);
                        }
                        this._manager.CurrentFile = args[1];
                        this.Title = "rdfEditor - " + System.IO.Path.GetFileName(this._manager.CurrentFile);
                        this._manager.HasChanged = false;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred while opening the selected file: " + ex.Message, "Unable to Open File");
                    }
                }
            }

            //Check File Associations
            if (Properties.Settings.Default.AlwaysCheckFileAssociations)
            {
                if (Properties.Settings.Default.FirstRun)
                {
                    Properties.Settings.Default.AlwaysCheckFileAssociations = false;
                    Properties.Settings.Default.FirstRun = false;
                    Properties.Settings.Default.Save();
                }

                FileAssociations diag = new FileAssociations();
                if (!diag.AllAssociated) diag.ShowDialog(); //Don't show if all associations are already set
            }

            textEditor.Focus();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Application.Current.ShutdownMode != ShutdownMode.OnExplicitShutdown)
            {
                mnuClose_Click(sender, new RoutedEventArgs());
            }
        }

        #endregion
    }
}
