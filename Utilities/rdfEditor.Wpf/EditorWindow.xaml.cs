using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Microsoft.Win32;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Validation;
using VDS.RDF.Query;
using VDS.RDF.Writing;
using VDS.RDF.Utilities.Editor.Syntax;

namespace VDS.RDF.Utilities.Editor
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
        private bool _saveWindowSize = false;
        private FindReplace _findReplace;

        public EditorWindow()
        {
            InitializeComponent();

            //Create the Editor Manager
            this._manager = new EditorManager(this.textEditor, this.mnuCurrentHighlighter, this.stsCurrSyntax, this.stsSyntaxValidation, this.mnuSymbolBoundaries);
          
            //Set up the Editor Options
            TextEditorOptions options = new TextEditorOptions();
            options.EnableEmailHyperlinks = Properties.Settings.Default.EnableClickableUris;
            options.EnableHyperlinks = Properties.Settings.Default.EnableClickableUris;
            options.ShowEndOfLine = Properties.Settings.Default.ShowEndOfLine;
            options.ShowSpaces = Properties.Settings.Default.ShowSpaces;
            options.ShowTabs = Properties.Settings.Default.ShowTabs;
            textEditor.Options = options;
            textEditor.ShowLineNumbers = true;
            if (Properties.Settings.Default.EditorFontFace != null)
            {
                textEditor.FontFamily = Properties.Settings.Default.EditorFontFace;
            }
            textEditor.FontSize = Math.Round(Properties.Settings.Default.EditorFontSize, 0);
            textEditor.Foreground = new SolidColorBrush(Properties.Settings.Default.EditorForeground);
            textEditor.Background = new SolidColorBrush(Properties.Settings.Default.EditorBackground);
            
            //Setup Options based on the User Config file
            if (!Properties.Settings.Default.UseUtf8Bom)
            {
                this.mnuUseBomForUtf8.IsChecked = false;
                Options.UseBomForUtf8 = false;
            }
            if (Properties.Settings.Default.SaveWithOptionsPrompt)
            {
                this.mnuSaveWithPromptOptions.IsChecked = true;
            }
            if (!Properties.Settings.Default.EnableSymbolSelection)
            {
                this._manager.IsSymbolSelectionEnabled = false;
                this.mnuSymbolSelectEnabled.IsChecked = false;
            }
            if (!Properties.Settings.Default.IncludeSymbolBoundaries)
            {
                this._manager.IncludeBoundaryInSymbolSelection = false;
                this.mnuSymbolSelectIncludeBoundary.IsChecked = false;
            }
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
                textEditor.WordWrap = true;
                this.mnuWordWrap.IsChecked = true;
            }
            if (Properties.Settings.Default.EnableClickableUris)
            {
                this.mnuClickableUris.IsChecked = true;
            }
            if (Properties.Settings.Default.ShowEndOfLine)
            {
                this.mnuShowSpecialEOL.IsChecked = true;
            }
            if (Properties.Settings.Default.ShowSpaces)
            {
                this.mnuShowSpecialSpaces.IsChecked = true;
            }
            if (Properties.Settings.Default.ShowTabs)
            {
                this.mnuShowSpecialTabs.IsChecked = true;
            }
            this._manager.SetHighlighter(Properties.Settings.Default.DefaultHighlighter);

            //Enable/Disable state dependent menu options
            this.mnuUndo.IsEnabled = textEditor.CanUndo;
            this.mnuRedo.IsEnabled = textEditor.CanRedo;

            //Create our Dialogs
            _ofd.Title = "Open RDF/SPARQL File";
            _ofd.DefaultExt = ".rdf";
            _ofd.Filter = FileFilterAll;
            _sfd.Title = "Save RDF/SPARQL File";
            _sfd.DefaultExt = ".rdf";
            _sfd.Filter = _ofd.Filter;

            //Setup dropping of files
            textEditor.AllowDrop = true;
            textEditor.Drop += new DragEventHandler(textEditor_Drop);
        }

        void textEditor_Drop(object sender, DragEventArgs e)
        {
            //Is the data FileDrop data?
            String[] droppedFilePaths = e.Data.GetData(DataFormats.FileDrop, false) as string[];
            if (droppedFilePaths == null) return;

            e.Handled = true;

            if (droppedFilePaths.Length > 0)
            {
                //Open the 1st File in the Current Window
                String file = droppedFilePaths[0];
                mnuClose_Click(sender, e);

                try
                {
                    if (!PreOpenCheck(file)) return;
                    using (StreamReader reader = new StreamReader(file))
                    {
                        String text = reader.ReadToEnd();
                        textEditor.Text = String.Empty;
                        textEditor.Text = text;
                        this._manager.AutoDetectSyntax(file);
                    }
                    this._manager.CurrentFile = file;
                    this.Title = "rdfEditor - " + System.IO.Path.GetFileName(this._manager.CurrentFile);
                    this._manager.HasChanged = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while opening the selected file: " + ex.Message, "Unable to Open File");
                }

                for (int i = 1; i < droppedFilePaths.Length; i++)
                {
                    try
                    {
                        ProcessStartInfo info = new ProcessStartInfo();
                        info.FileName = Assembly.GetExecutingAssembly().Location;
                        info.Arguments = "\"" + droppedFilePaths[i] + "\"";
                        Process.Start(info);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Unable to open " + droppedFilePaths[i] + " due to the following error: " + ex.Message, "Unable to Open File");
                    }
                }
            } 
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
                    if (!PreOpenCheck(_ofd.FileName)) return;
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
                    this.UpdateMruList(_ofd.FileName);
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
            }
            mnuClose_Click(sender, e);

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
            String queryText = String.Empty;
            if (this._manager.CurrentSyntax.StartsWith("SparqlQuery"))
            {
                queryText = this.textEditor.Text;
            }

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
            }
            mnuClose_Click(sender, e);

            OpenQueryResults diag = new OpenQueryResults();
            if (!queryText.Equals(String.Empty)) diag.Query = queryText;
            if (diag.ShowDialog() == true)
            {
                textEditor.Text = diag.RetrievedData;
                this._manager.HasChanged = true;
                this._manager.SetHighlighter(diag.Parser);
            }
        }

        private bool PreOpenCheck(String file)
        {
            if (this._manager.HasChanged)
            {
                //Prompt user to save
                MessageBoxResult result = MessageBox.Show("Do you wish to save changes to the current file before opening another file?", "Save Changes", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Cancel)
                {
                    return false;
                }
                else if (result == MessageBoxResult.Yes)
                {
                    mnuSave_Click(null, new RoutedEventArgs());
                }
            }

            FileInfo info = new FileInfo(file);
            long sizeInMB = info.Length / 1024 / 1024;

            if (sizeInMB >= 10)
            {
                if (MessageBox.Show("The file that you are opening is considered large (>= 10MB) by this editor and so Syntax Highlighting, Validate as you Type, Highlight Validation Errors and Auto-Completion have been temporarily disabled.  You may re-enable these features if you wish but they may significantly degrade performance on a file of this size.  You can cancel opening this file if you wish by clicking Cancel", "Large File Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                {
                    //Disable features if user proceeds
                    if (this.mnuEnableHighlighting.IsChecked)
                    {
                        mnuEnableHighlighting.IsChecked = false;
                        mnuEnableHighlighting_Click(null, new RoutedEventArgs());
                    }
                    if (this.mnuValidateAsYouType.IsChecked)
                    {
                        mnuValidateAsYouType.IsChecked = false;
                        mnuValidateAsYouType_Click(null, new RoutedEventArgs());
                    }
                    if (this.mnuHighlightErrors.IsChecked)
                    {
                        mnuHighlightErrors.IsChecked = false;
                        mnuHighlightErrors_Click(null, new RoutedEventArgs());
                    }
                    if (this.mnuAutoComplete.IsChecked)
                    {
                        mnuAutoComplete.IsChecked = false;
                        mnuAutoComplete_Click(null, new RoutedEventArgs());
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        private void mnuSave_Click(object sender, RoutedEventArgs e)
        {
            if (this._manager.CurrentFile != null)
            {
                this.UpdateMruList(this._manager.CurrentFile);

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
                        fileWriter = new StreamWriter(this._manager.CurrentFile, false, new UTF8Encoding(Options.UseBomForUtf8));
                    }
                }
                catch
                {
                    //Ignore and just create a UTF-8 Stream
                    fileWriter = new StreamWriter(this._manager.CurrentFile, false, new UTF8Encoding(Options.UseBomForUtf8));
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
                if (!this._sfd.FileName.Equals(this._manager.CurrentFile))
                {
                    this._manager.CurrentFile = this._sfd.FileName;
                    mnuSave_Click(sender, e);
                    this._manager.AutoDetectSyntax(this._manager.CurrentFile);
                }
                else
                {
                    mnuSave_Click(sender, e);
                }
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
                    try
                    {
                        this.UpdateMruList(this._sfd.FileName);
                    }
                    catch
                    {
                        //Ignore Errors here
                    }
                    this._manager.CurrentFile = _sfd.FileName;
                }
                else
                {
                    return;
                }
            }


            try
            {
                //Check whether the User wants to set advanced options?
                if (Properties.Settings.Default.SaveWithOptionsPrompt)
                {
                    RdfWriterOptionsWindow optPrompt = new RdfWriterOptionsWindow(writer);
                    optPrompt.Owner = this;
                    if (optPrompt.ShowDialog() != true) return;
                }

                //Do the actual save
                writer.Save(g, this._manager.CurrentFile);

                //Give the user the option of switching to this new file
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
            this.SaveWith(new PrettyRdfXmlWriter());
        }

        private void mnuSaveWithRdfJson_Click(object sender, RoutedEventArgs e)
        {
            this.SaveWith(new RdfJsonWriter());
        }

        private void mnuSaveWithRdfa_Click(object sender, RoutedEventArgs e)
        {
            this.SaveWith(new HtmlWriter());
        }

        private void mnuUseBomForUtf8_Click(object sender, RoutedEventArgs e)
        {
            Options.UseBomForUtf8 = this.mnuUseBomForUtf8.IsChecked;
        }

        private void mnuPageSetup_Click(object sender, RoutedEventArgs e)
        {
            this.textEditor.PageSetupDialog();
        }

        private void mnuPrintPreview_Click(object sender, RoutedEventArgs e)
        {
            this.textEditor.PrintPreviewDialog(this._manager.CurrentFile);
        }

        private void mnuPrintPreviewNoHighlighting_Click(object sender, RoutedEventArgs e)
        {
            this.textEditor.PrintPreviewDialog(this._manager.CurrentFile, false);
        }

        private void mnuPrint_Click(object sender, RoutedEventArgs e)
        {
            this.textEditor.PrintDialog(this._manager.CurrentFile, true);
        }

        private void mnuPrintNoHighlighting_Click(object sender, RoutedEventArgs e)
        {
            this.textEditor.PrintDialog(this._manager.CurrentFile, false);
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

        private void mnuFind_Click(object sender, RoutedEventArgs e)
        {
            if (this._findReplace == null)
            {
                this._findReplace = new FindReplace();
            }
            if (this._findReplace.Visibility != Visibility.Visible)
            {
                this._findReplace.Mode = FindReplaceMode.Find;
                this._findReplace.Editor = this.textEditor;
                this._findReplace.Show();
            }
            this._findReplace.BringIntoView();
            this._findReplace.Focus();
        }

        private void mnuFindNext_Click(object sender, RoutedEventArgs e)
        {
            if (this._findReplace == null)
            {
                this.mnuFind_Click(sender, e);
            }
            else
            {
                this._findReplace.Find(this.textEditor);
            }
        }

        private void mnuReplace_Click(object sender, RoutedEventArgs e)
        {
            if (this._findReplace == null)
            {
                this._findReplace = new FindReplace();
            }
            this._findReplace.Mode = FindReplaceMode.FindAndReplace;
            this._findReplace.Editor = this.textEditor;
            if (this._findReplace.Visibility != Visibility.Visible) this._findReplace.Show();
            this._findReplace.BringIntoView();
            this._findReplace.Focus();
        }

        private void mnuGoToLine_Click(object sender, RoutedEventArgs e)
        {
            GoToLine gotoLine = new GoToLine(this.textEditor);
            gotoLine.Owner = this;
            if (gotoLine.ShowDialog() == true)
            {
                this.textEditor.CaretOffset = this.textEditor.Document.GetOffset(gotoLine.Line, 1);
                this.textEditor.ScrollToLine(gotoLine.Line);
            }
        }

        private void mnuCommentSelection_Click(object sender, RoutedEventArgs e)
        {
            if (textEditor.SelectionLength == 0) return;

            String syntax = this._manager.CurrentSyntax;
            SyntaxDefinition def = SyntaxManager.GetDefinition(syntax);
            if (def != null)
            {
                if (def.CanComment)
                {
                    String selection = textEditor.SelectedText;
                    int startLine = textEditor.Document.GetLineByOffset(textEditor.SelectionStart).LineNumber;
                    int endLine = textEditor.Document.GetLineByOffset(textEditor.SelectionStart + textEditor.SelectionLength).LineNumber;

                    if (startLine == endLine && def.SingleLineComment != null)
                    {
                        //Single Line Comment
                        textEditor.Document.Replace(textEditor.SelectionStart, textEditor.SelectionLength, def.SingleLineComment + selection);
                    }
                    else
                    {
                        //Multi Line Comment
                        if (def.MultiLineCommentStart != null && def.MultiLineCommentEnd != null)
                        {
                            textEditor.Document.Replace(textEditor.SelectionStart, textEditor.SelectionLength, def.MultiLineCommentStart + selection + def.MultiLineCommentEnd);
                        }
                        else
                        {
                            //Multi-Line Comment but only supports single line comments
                            textEditor.BeginChange();
                            for (int i = startLine; i <= endLine; i++)
                            {
                                DocumentLine line = textEditor.Document.GetLineByNumber(i);
                                int startOffset = Math.Max(textEditor.SelectionStart, line.Offset);
                                textEditor.Document.Insert(startOffset, def.SingleLineComment);
                            }
                            textEditor.EndChange();
                            if (textEditor.SelectionStart > 0) textEditor.SelectionStart--;
                            if (textEditor.SelectionStart + textEditor.SelectionLength < textEditor.Text.Length) textEditor.SelectionLength++;
                        }
                    }
                }
            }
        }

        private void mnuUncommentSelection_Click(object sender, RoutedEventArgs e)
        {
            if (textEditor.SelectionLength == 0) return;

            String syntax = this._manager.CurrentSyntax;
            SyntaxDefinition def = SyntaxManager.GetDefinition(syntax);
            if (def != null)
            {
                if (def.CanComment)
                {
                    String selection = textEditor.SelectedText;
                    int startLine = textEditor.Document.GetLineByOffset(textEditor.SelectionStart).LineNumber;
                    int endLine = textEditor.Document.GetLineByOffset(textEditor.SelectionStart + textEditor.SelectionLength).LineNumber;

                    if (startLine == endLine && def.SingleLineComment != null)
                    {
                        //Single Line Comment
                        int index = selection.IndexOf(def.SingleLineComment);
                        if (index > -1)
                        {
                            textEditor.Document.Remove(textEditor.SelectionStart + index, def.SingleLineComment.Length);
                        }
                    }
                    else
                    {
                        //Multi Line Comment
                        if (def.MultiLineCommentStart != null && def.MultiLineCommentEnd != null)
                        {
                            int startIndex = selection.IndexOf(def.MultiLineCommentStart);
                            int endIndex = selection.LastIndexOf(def.MultiLineCommentEnd);
                            textEditor.BeginChange();
                            textEditor.Document.Remove(textEditor.SelectionStart + startIndex, def.MultiLineCommentStart.Length);
                            textEditor.Document.Remove(textEditor.SelectionStart + endIndex - def.MultiLineCommentStart.Length, def.MultiLineCommentEnd.Length);
                            textEditor.EndChange();
                        }
                        else
                        {
                            textEditor.BeginChange();
                            for (int i = startLine; i <= endLine; i++)
                            {
                                DocumentLine line = textEditor.Document.GetLineByNumber(i);
                                int startOffset = Math.Max(textEditor.SelectionStart, line.Offset);
                                int endOffset = Math.Min(textEditor.SelectionStart + textEditor.SelectionLength, line.EndOffset);
                                String lineText = textEditor.Document.GetText(startOffset, endOffset - startOffset);
                                int index = lineText.IndexOf(def.SingleLineComment);
                                textEditor.Document.Remove(startOffset + index, def.SingleLineComment.Length);
                            }
                            textEditor.EndChange();
                        }
                    }
                }
            }
        }

        private void mnuSymbolSelectEnabled_Click(object sender, RoutedEventArgs e)
        {
            this._manager.IsSymbolSelectionEnabled = this.mnuSymbolSelectEnabled.IsChecked;
            Properties.Settings.Default.EnableSymbolSelection = this.mnuSymbolSelectEnabled.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void mnuSymbolSelectIncludeBoundary_Click(object sender, RoutedEventArgs e)
        {
            this._manager.IncludeBoundaryInSymbolSelection = this.mnuSymbolSelectIncludeBoundary.IsChecked;
            Properties.Settings.Default.IncludeSymbolBoundaries = this.mnuSymbolSelectIncludeBoundary.IsChecked;
            Properties.Settings.Default.Save();
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

        private void mnuShowSpecialAll_Click(object sender, RoutedEventArgs e)
        {
            this.mnuShowSpecialEOL.IsChecked = true;
            this.mnuShowSpecialSpaces.IsChecked = true;
            this.mnuShowSpecialTabs.IsChecked = true;
            mnuShowSpecialEOL_Click(sender, e);
            mnuShowSpecialSpaces_Click(sender, e);
            mnuShowSpecialTabs_Click(sender, e);
        }

        private void mnuShowSpecialEOL_Click(object sender, RoutedEventArgs e)
        {
            textEditor.Options.ShowEndOfLine = this.mnuShowSpecialEOL.IsChecked;
            Properties.Settings.Default.ShowEndOfLine = textEditor.Options.ShowEndOfLine;
            Properties.Settings.Default.Save();
        }

        private void mnuShowSpecialSpaces_Click(object sender, RoutedEventArgs e)
        {
            textEditor.Options.ShowSpaces = this.mnuShowSpecialSpaces.IsChecked;
            Properties.Settings.Default.ShowSpaces = textEditor.Options.ShowSpaces;
            Properties.Settings.Default.Save();
        }

        private void mnuShowSpecialTabs_Click(object sender, RoutedEventArgs e)
        {
            textEditor.Options.ShowTabs = this.mnuShowSpecialTabs.IsChecked;
            Properties.Settings.Default.ShowTabs = textEditor.Options.ShowTabs;
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
            if (!Properties.Settings.Default.DefaultHighlighter.Equals("None"))
            {
                foreach (MenuItem item in mnuCurrentHighlighter.Items.OfType<MenuItem>())
                {
                    if (item.Tag != null)
                    {
                        String syntax = (String)item.Tag;
                        if (syntax.Equals(Properties.Settings.Default.DefaultHighlighter))
                        {
                            if (!((String)item.Header).EndsWith(" (Default)"))
                            {
                                item.Header += " (Default)";
                            }
                        }
                        else if (((String)item.Header).EndsWith(" (Default)"))
                        {
                            item.Header = ((String)item.Header).Substring(0, ((String)item.Header).Length - 10);
                        }
                    }
                }
            }
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

        private void mnuCustomiseAppearance_Click(object sender, RoutedEventArgs e)
        {
            AppearanceSettings settings = new AppearanceSettings(textEditor);
            settings.Owner = this;
            if (settings.ShowDialog() == true)
            {
                if (Properties.Settings.Default.EditorFontFace != null)
                {
                    textEditor.FontFamily = Properties.Settings.Default.EditorFontFace;
                }
                textEditor.FontSize = Math.Round(Properties.Settings.Default.EditorFontSize, 0);
                textEditor.Foreground = new SolidColorBrush(Properties.Settings.Default.EditorForeground);
                textEditor.Background = new SolidColorBrush(Properties.Settings.Default.EditorBackground);
            }
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


        private void mnuStructureView_Click(object sender, RoutedEventArgs e)
        {
            ISyntaxValidator validator = this._manager.CurrentValidator;
            if (validator != null)
            {
                ISyntaxValidationResults results = validator.Validate(textEditor.Text);
                if (results.IsValid)
                {
                    if (!this._manager.CurrentSyntax.Equals("None"))
                    {
                        try 
                        {
                            SyntaxDefinition def = SyntaxManager.GetDefinition(this._manager.CurrentSyntax);
                            if (def.DefaultParser != null)
                            {
                                NonIndexedGraph g = new NonIndexedGraph();
                                def.DefaultParser.Load(g, new StringReader(textEditor.Text));
                                TriplesWindow window = new TriplesWindow(g);
                                window.ShowDialog();
                            }
                            //else if (def.Validator is RdfDatasetSyntaxValidator)
                            //{
                            //    TripleStore store = new TripleStore();
                            //    StringParser.ParseDataset(store, textEditor.Text);
                            //}
                            else if (def.Validator is SparqlResultsValidator)
                            {
                                SparqlResultSet sparqlResults = new SparqlResultSet();
                                StringParser.ParseResultSet(sparqlResults, textEditor.Text);
                                if (sparqlResults.ResultsType == SparqlResultsType.VariableBindings)
                                {
                                    ResultSetWindow window = new ResultSetWindow(sparqlResults);
                                    window.ShowDialog();
                                }
                                else
                                {
                                    MessageBox.Show("Cannot open Structured View since this form of SPARQL Results is not structured");
                                }
                            }
                            else
                            {
                                MessageBox.Show("Cannot open Structured View since this is not a syntax for which Structure view is available");
                            }
                        } 
                        catch
                        {
                            MessageBox.Show("Unable to open Structured View as could not parse the Syntax successfully for structured display");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Cannot open Structured View since this is not a syntax for which Structure view is available");
                    }
                }
                else
                {
                    MessageBox.Show("Cannot open Structured View as the Syntax is not valid");
                }
            }
            else
            {
                MessageBox.Show("Cannot open Structured View as you have not selected a Syntax");
            }
        }

        #endregion

        #region Help Menu

        private void mnuAbout_Click(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.ShowDialog();
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

        private void FindCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.mnuFind_Click(sender, e);
        }

        private void FindNextCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.mnuFindNext_Click(sender, e);
        }

        private void ReplaceCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.mnuReplace_Click(sender, e);
        }

        private void GoToLineCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.mnuGoToLine_Click(sender, e);
        }

        private void CommentSelectionExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            mnuCommentSelection_Click(sender, e);
        }

        private void UncommentSelectionExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            mnuUncommentSelection_Click(sender, e);
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

        private void ToggleClickableUrisExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.mnuClickableUris.IsChecked = !this.mnuClickableUris.IsChecked;
            mnuClickableUris_Click(sender, e);
        }

        private void IncreaseTextSizeExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.textEditor.FontSize = Math.Round(this.textEditor.FontSize + 1.0, 0);
            Properties.Settings.Default.EditorFontSize = this.textEditor.FontSize;
            Properties.Settings.Default.Save();
        }

        private void DecreaseTextSizeExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.textEditor.FontSize = Math.Round(this.textEditor.FontSize - 1.0, 0);
            Properties.Settings.Default.EditorFontSize = this.textEditor.FontSize;
            Properties.Settings.Default.Save();
        }

        private void ResetTextSizeExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.textEditor.FontSize = 13.0d;
            Properties.Settings.Default.EditorFontSize = this.textEditor.FontSize;
            Properties.Settings.Default.Save();
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

        private void ToggleValidationErrorHighlighting(object sender, ExecutedRoutedEventArgs e)
        {
            this.mnuHighlightErrors.IsChecked = !this.mnuHighlightErrors.IsChecked;
            mnuHighlightErrors_Click(sender, e);
        }

        private void ToggleAutoCompletionExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.mnuAutoComplete.IsChecked = !this.mnuAutoComplete.IsChecked;
            mnuAutoComplete_Click(sender, e);
        }

        private void ValidateSyntaxExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            mnuValidateSyntax_Click(sender, e);
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
                        this.UpdateMruList(args[1]);
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

            //Set Window size
            if (Properties.Settings.Default.WindowHeight > 0 && Properties.Settings.Default.WindowWidth > 0)
            {
                this.Height = Properties.Settings.Default.WindowHeight;
                this.Width = Properties.Settings.Default.WindowWidth;
            }
            this._saveWindowSize = true;

            //Fill The MRU List
            this.ShowMruList();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Application.Current.ShutdownMode != ShutdownMode.OnExplicitShutdown )
            {
                mnuClose_Click(sender, new RoutedEventArgs());
                Application.Current.Shutdown();
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!this._saveWindowSize) return;

            if (this.Width > 0 && this.Height > 0)
            {
                Properties.Settings.Default.WindowHeight = this.Height;
                Properties.Settings.Default.WindowWidth = this.Width;
                Properties.Settings.Default.Save();
            }
        }

        #endregion

        #region MRU List

        private void ShowMruList()
        {
            if (VDS.RDF.Utilities.Editor.App.RecentFiles != null)
            {
                while (this.mnuRecentFiles.Items.Count > 2)
                {
                    this.mnuRecentFiles.Items.RemoveAt(2);
                }

                int i = 0;
                foreach (String file in VDS.RDF.Utilities.Editor.App.RecentFiles.Files)
                {
                    i++;
                    MenuItem item = new MenuItem();
                    item.Header = i + ": " + MruList.ShortenFilename(file);
                    item.Tag = file;
                    item.Click += new RoutedEventHandler(this.MruListFileClicked);
                    this.mnuRecentFiles.Items.Add(item);
                }
            }
        }

        private void UpdateMruList(String file)
        {
            if (VDS.RDF.Utilities.Editor.App.RecentFiles != null)
            {
                VDS.RDF.Utilities.Editor.App.RecentFiles.Add(file);
                this.ShowMruList();
            }
        }

        private void MruListFileClicked(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem)
            {
                MenuItem item = (MenuItem)sender;
                if (item.Tag != null)
                {
                    String file = item.Tag as String;
                    if (file != null)
                    {
                        if (File.Exists(file))
                        {
                            try
                            {
                                if (!PreOpenCheck(file)) return;
                                using (StreamReader reader = new StreamReader(file))
                                {
                                    String text = reader.ReadToEnd();
                                    textEditor.Text = String.Empty;
                                    textEditor.Text = text;
                                    this._manager.AutoDetectSyntax(file);
                                }
                                this._manager.CurrentFile = file;
                                this.Title = "rdfEditor - " + System.IO.Path.GetFileName(this._manager.CurrentFile);
                                this._manager.HasChanged = false;
                                this.UpdateMruList(file);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("An error occurred while opening the selected file: " + ex.Message, "Unable to Open File");
                            }
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("Cannot Open the Recent File '" + file + "' as it no longer exists!", "Unable to Open File", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
        }

        #endregion

        private void mnuClearRecentFiles_Click(object sender, RoutedEventArgs e)
        {
            if (VDS.RDF.Utilities.Editor.App.RecentFiles != null)
            {
                VDS.RDF.Utilities.Editor.App.RecentFiles.Clear();
            }
        }

        private void mnuSaveWithPromptOptions_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.SaveWithOptionsPrompt = this.mnuSaveWithPromptOptions.IsChecked;
            Properties.Settings.Default.Save();
        }

    }
}
