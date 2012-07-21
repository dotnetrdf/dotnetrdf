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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Win32;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Validation;
using VDS.RDF.Query;
using VDS.RDF.Writing;
using VDS.RDF.Utilities.Editor.AutoComplete;
using VDS.RDF.Utilities.Editor.Selection;
using VDS.RDF.Utilities.Editor.Syntax;
using VDS.RDF.Utilities.Editor.Wpf.Syntax;

namespace VDS.RDF.Utilities.Editor.Wpf
{
    /// <summary>
    /// Interaction logic for EditorWindow.xaml
    /// </summary>
    public partial class EditorWindow
        : Window
    {
        private readonly String FileFilterRdf, FileFilterSparql, FileFilterRdfDataset, FileFilterAll;

        private OpenFileDialog _ofd = new OpenFileDialog();
        private SaveFileDialog _sfd = new SaveFileDialog();
        private Editor<TextEditor, FontFamily, Color> _editor;
        private bool _saveWindowSize = false;
        private FindReplace _findReplace;

        public EditorWindow()
        {
            InitializeComponent();

            //rdfEditor must disable URI interning otherwise it will eat memory as the user edits lots of files
            Options.InternUris = false;

            //Generate Filename Filters
            FileFilterRdf = MimeTypesHelper.GetFilenameFilter(true, false, false, false, false, true);
            FileFilterSparql = MimeTypesHelper.GetFilenameFilter(false, false, true, false, false, true);
            FileFilterRdfDataset = MimeTypesHelper.GetFilenameFilter(false, true, false, false, false, true);
            FileFilterAll = MimeTypesHelper.GetFilenameFilter();

            //Initialise Highlighting and Auto-Completion
            WpfHighlightingManager.Initialise(Properties.Settings.Default.UseCustomisedXshdFiles);
            AutoCompleteManager.Initialise();

            //Create the Editor Manager
            WpfEditorFactory factory = new WpfEditorFactory();
            this._editor = new Editor<TextEditor, FontFamily, Color>(factory);
            this._editor.DocumentManager.DefaultSaveChangesCallback = new SaveChangesCallback<TextEditor>(this.SaveChangesCallback);
            this._editor.DocumentManager.DefaultSaveAsCallback = new SaveAsCallback<TextEditor>(this.SaveAsCallback);
            this._editor.DocumentManager.DocumentCreated += new DocumentChangedHandler<TextEditor>(HandleDocumentCreated);
          
            //Set up the Editor Options
            if (this._editor.DocumentManager.VisualOptions == null) this._editor.DocumentManager.VisualOptions = new WpfVisualOptions();
            this._editor.DocumentManager.VisualOptions.EnableClickableUris = Properties.Settings.Default.EnableClickableUris;
            this._editor.DocumentManager.VisualOptions.ShowEndOfLine = Properties.Settings.Default.ShowEndOfLine;
            this._editor.DocumentManager.VisualOptions.ShowLineNumbers = Properties.Settings.Default.ShowLineNumbers;
            this._editor.DocumentManager.VisualOptions.ShowSpaces = Properties.Settings.Default.ShowSpaces;
            this._editor.DocumentManager.VisualOptions.ShowTabs = Properties.Settings.Default.ShowTabs;
            if (Properties.Settings.Default.EditorFontFace != null)
            {
                this._editor.DocumentManager.VisualOptions.FontFace = Properties.Settings.Default.EditorFontFace;
            }
            this._editor.DocumentManager.VisualOptions.FontSize = Math.Round(Properties.Settings.Default.EditorFontSize, 0);
            this._editor.DocumentManager.VisualOptions.Foreground = Properties.Settings.Default.EditorForeground;
            this._editor.DocumentManager.VisualOptions.Background = Properties.Settings.Default.EditorBackground;
            this._editor.DocumentManager.VisualOptions.ErrorBackground = Properties.Settings.Default.ErrorHighlightBackground;
            this._editor.DocumentManager.VisualOptions.ErrorDecoration = Properties.Settings.Default.ErrorHighlightDecoration;
            this._editor.DocumentManager.VisualOptions.ErrorFontFace = Properties.Settings.Default.ErrorHighlightFontFamily;
            this._editor.DocumentManager.VisualOptions.ErrorForeground = Properties.Settings.Default.ErrorHighlightForeground;

            //If custom highlighting colours have been used this call forces them to be used
            AppearanceSettings.UpdateHighlightingColours();
            
            //Setup Options based on the User Config file
            Options.UseBomForUtf8 = false;
            if (Properties.Settings.Default.UseUtf8Bom)
            {
                this.mnuUseBomForUtf8.IsChecked = true;
                Options.UseBomForUtf8 = true;
                GlobalOptions.UseBomForUtf8 = true;
            }
            if (Properties.Settings.Default.SaveWithOptionsPrompt)
            {
                this.mnuSaveWithPromptOptions.IsChecked = true;
            }
            if (!Properties.Settings.Default.EnableSymbolSelection)
            {
                this._editor.DocumentManager.Options.IsSymbolSelectionEnabled = false;
                this.mnuSymbolSelectEnabled.IsChecked = false;
            }
            if (!Properties.Settings.Default.IncludeSymbolBoundaries)
            {
                this._editor.DocumentManager.Options.IncludeBoundaryInSymbolSelection = false;
                this.mnuSymbolSelectIncludeBoundary.IsChecked = false;
            }
            switch (Properties.Settings.Default.SymbolSelectionMode)
            {
                case "Punctuation":
                    this._editor.DocumentManager.Options.CurrentSymbolSelector = new PunctuationSelector<TextEditor>();
                    this.mnuBoundariesPunctuation.IsChecked = true;
                    break;
                case "WhiteSpace":
                    this._editor.DocumentManager.Options.CurrentSymbolSelector = new WhiteSpaceSelector<TextEditor>();
                    this.mnuBoundariesWhiteSpace.IsChecked = true;
                    break;
                case "All":
                    this._editor.DocumentManager.Options.CurrentSymbolSelector = new WhiteSpaceOrPunctuationSelection<TextEditor>();
                    this.mnuBoundariesAll.IsChecked = true;
                    break;
                case "Default":
                default:
                    this.mnuBoundariesDefault.IsChecked = true;
                    break;
            }
            if (!Properties.Settings.Default.EnableAutoComplete) 
            {
                this._editor.DocumentManager.Options.IsAutoCompletionEnabled = false;
                this.mnuAutoComplete.IsChecked = false;
            }
            if (!Properties.Settings.Default.EnableHighlighting)
            {
                this._editor.DocumentManager.Options.IsSyntaxHighlightingEnabled = false;
                this.mnuEnableHighlighting.IsChecked = false;
            }
            if (!Properties.Settings.Default.EnableValidateAsYouType)
            {
                this._editor.DocumentManager.Options.IsValidateAsYouTypeEnabled = false;
                this.mnuValidateAsYouType.IsChecked = false;
            }
            if (!Properties.Settings.Default.ShowLineNumbers)
            {
                this._editor.DocumentManager.VisualOptions.ShowLineNumbers = false;
                this.mnuShowLineNumbers.IsChecked = false;
            }
            if (Properties.Settings.Default.WordWrap)
            {
                this._editor.DocumentManager.VisualOptions.WordWrap = true;
                this.mnuWordWrap.IsChecked = true;
            }
            if (Properties.Settings.Default.EnableClickableUris)
            {
                this._editor.DocumentManager.VisualOptions.EnableClickableUris = true;
                this.mnuClickableUris.IsChecked = true;
            }
            if (Properties.Settings.Default.ShowEndOfLine)
            {
                this._editor.DocumentManager.VisualOptions.ShowEndOfLine = true;
                this.mnuShowSpecialEOL.IsChecked = true;
            }
            if (Properties.Settings.Default.ShowSpaces)
            {
                this._editor.DocumentManager.VisualOptions.ShowSpaces = true;
                this.mnuShowSpecialSpaces.IsChecked = true;
            }
            if (Properties.Settings.Default.ShowTabs)
            {
                this._editor.DocumentManager.VisualOptions.ShowTabs = true;
                this.mnuShowSpecialTabs.IsChecked = true;
            }
            this._editor.DocumentManager.DefaultSyntax = Properties.Settings.Default.DefaultHighlighter;

            //Add an initial document for editing
            this.AddTextEditor();
            this.tabDocuments.SelectedIndex = 0;
            this._editor.DocumentManager.ActiveDocument.TextEditor.Control.Focus();

            //Create our Dialogs
            _ofd.Title = "Open RDF/SPARQL File";
            _ofd.DefaultExt = ".rdf";
            _ofd.Filter = FileFilterAll;
            _ofd.Multiselect = true;
            _sfd.Title = "Save RDF/SPARQL File";
            _sfd.DefaultExt = ".rdf";
            _sfd.Filter = _ofd.Filter;

            //Setup dropping of files
            this.AllowDrop = true;
            this.Drop += new DragEventHandler(EditorWindow_Drop);
        }

        #region Text Editor Management

        private void AddTextEditor()
        {
            this.AddTextEditor(new TabItem());
        }

        private void AddTextEditor(TabItem tab)
        {
            Document<TextEditor> doc = this._editor.DocumentManager.New();
            this.AddTextEditor(tab, doc);
        }

        private void AddTextEditor(TabItem tab, Document<TextEditor> doc)
        {
            //Register for relevant events on the document
            doc.FilenameChanged +=
                new DocumentChangedHandler<TextEditor>((sender, e) =>
                {
                    if (e.Document.Filename != null && !e.Document.Filename.Equals(String.Empty))
                    {
                        tab.Header = System.IO.Path.GetFileName(e.Document.Filename);
                    }
                });
            doc.TitleChanged += new DocumentChangedHandler<TextEditor>((sender, e) =>
            {
                if (e.Document.Title != null && !e.Document.Title.Equals(String.Empty))
                {
                    tab.Header = e.Document.Title;
                }
            });
            doc.SyntaxChanged += new DocumentChangedHandler<TextEditor>((sender, e) =>
            {
                if (ReferenceEquals(this._editor.DocumentManager.ActiveDocument, e.Document))
                {
                    this.stsCurrSyntax.Content = "Syntax: " + e.Document.Syntax;
                }
            });
            doc.Validated += new DocumentValidatedHandler<TextEditor>(this.HandleValidation);
            doc.ValidatorChanged += new DocumentChangedHandler<TextEditor>(this.HandleValidatorChanged);
            doc.TextChanged += new DocumentChangedHandler<TextEditor>(this.HandleTextChanged);

            //Set Tab title where appropriate
            if (doc.Filename != null && !doc.Filename.Equals(String.Empty))
            {
                tab.Header = System.IO.Path.GetFileName(doc.Filename);
            }
            else if (doc.Title != null && !doc.Title.Equals(String.Empty))
            {
                tab.Header = doc.Title;
            }

            //Add to Tabs
            this.tabDocuments.Items.Add(tab);
            tab.Content = doc.TextEditor.Control;

            //Add appropriate event handlers on tabs
            //tab.Enter +=
            //    new EventHandler((sender, e) =>
            //    {
            //        var page = ((TabPage)sender);
            //        if (page.Controls.Count > 0)
            //        {
            //            page.BeginInvoke(new Action<TabPage>(p => p.Controls[0].Focus()), page);
            //        }
            //    });
        }

        #endregion

        #region File Menu

        private void mnuFile_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            bool hasDoc = this._editor.DocumentManager.ActiveDocument != null;
            this.mnuNewFromActive.IsEnabled = hasDoc;
            this.mnuSave.IsEnabled = hasDoc;
            this.mnuSaveAs.IsEnabled = hasDoc;
            this.mnuSaveAll.IsEnabled = hasDoc;
            this.mnuSaveWith.IsEnabled = hasDoc;
            this.mnuPageSetup.IsEnabled = hasDoc;
            this.mnuPrint.IsEnabled = hasDoc;
            this.mnuPrintNoHighlighting.IsEnabled = hasDoc;
            this.mnuPrintPreview.IsEnabled = hasDoc;
            this.mnuPrintPreviewNoHighlighting.IsEnabled = hasDoc;
            this.mnuClose.IsEnabled = hasDoc;
            this.mnuCloseAll.IsEnabled = hasDoc;
        }

        private void mnuNew_Click(object sender, RoutedEventArgs e)
        {
            this.AddTextEditor();
            this._editor.DocumentManager.SwitchTo(this._editor.DocumentManager.Count - 1);
            this.tabDocuments.SelectedIndex = this.tabDocuments.Items.Count - 1;
        }

        private void mnuNewFromActive_Click(object sender, RoutedEventArgs e)
        {
            Document<TextEditor> doc = this._editor.DocumentManager.ActiveDocument;
            if (doc != null)
            {
                Document<TextEditor> newDoc = this._editor.DocumentManager.NewFromActive(true);

                TabItem tab = new TabItem();
                tab.Header = newDoc.Title;
                this.AddTextEditor(tab, newDoc);
                this.tabDocuments.SelectedIndex = this.tabDocuments.Items.Count - 1;
            }
            else
            {
                this.AddTextEditor();
            }
        }

        private void mnuOpen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this._ofd.ShowDialog() == true)
                {
                    if (this._ofd.FileNames.Length == 1)
                    {
                        Document<TextEditor> doc, active;
                        active = this._editor.DocumentManager.ActiveDocument;
                        if (active.TextLength == 0 && (active.Filename == null || active.Filename.Equals(String.Empty)))
                        {
                            doc = active;
                            doc.Filename = this._ofd.FileName;
                            this.UpdateMruList(doc.Filename);
                        }
                        else
                        {
                            doc = this._editor.DocumentManager.New(System.IO.Path.GetFileName(this._ofd.FileName), true);
                        }

                        //Open the file and display in new tab if necessary
                        doc.Open(this._ofd.FileName);
                        if (!ReferenceEquals(active, doc))
                        {
                            this.AddTextEditor(new TabItem(), doc);
                            this.tabDocuments.SelectedIndex = this.tabDocuments.Items.Count - 1;
                        }
                    }
                    else
                    {
                        foreach (String filename in this._ofd.FileNames)
                        {
                            Document<TextEditor> doc = this._editor.DocumentManager.New(System.IO.Path.GetFileName(filename), false);
                            try
                            {
                                doc.Open(filename);
                                this.AddTextEditor(new TabItem(), doc);
                                this.UpdateMruList(doc.Filename);
                            }
                            catch (Exception ex)
                            {
                                System.Windows.MessageBox.Show("An error occurred while opening the selected file(s): " + ex.Message, "Open File Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                                this._editor.DocumentManager.Close(this._editor.DocumentManager.Count - 1);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("An error occurred while opening the selected file(s): " + ex.Message, "Open File(s) Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void mnuOpenUri_Click(object sender, RoutedEventArgs e)
        {
            Document<TextEditor> doc, active;
            active = this._editor.DocumentManager.ActiveDocument;
            if (active != null)
            {
                if (active.TextLength == 0 && (active.Filename == null || active.Filename.Equals(String.Empty)))
                {
                    doc = active;
                }
                else
                {
                    doc = this._editor.DocumentManager.New(true);
                    this.AddTextEditor(new TabItem(), doc);
                }
            }
            else
            {
                doc = this._editor.DocumentManager.New(true);
                this.AddTextEditor(new TabItem(), doc);
            }

            OpenUri diag = new OpenUri();
            if (diag.ShowDialog() == true)
            {
                doc.Text = diag.RetrievedData;
                if (diag.Parser != null)
                {
                    doc.Syntax = diag.Parser.GetSyntaxName();
                }
                else
                {
                    doc.AutoDetectSyntax();
                }
            }
        }

        private void mnuOpenQueryResults_Click(object sender, RoutedEventArgs e)
        {
            String queryText = String.Empty;
            if (this._editor.DocumentManager.ActiveDocument != null && this._editor.DocumentManager.ActiveDocument.Syntax.StartsWith("SparqlQuery"))
            {
                queryText = this._editor.DocumentManager.ActiveDocument.Text;
            }

            OpenQueryResults diag = new OpenQueryResults(this._editor.DocumentManager.VisualOptions);
            if (!queryText.Equals(String.Empty)) diag.Query = queryText;
            if (diag.ShowDialog() == true)
            {
                Document<TextEditor> doc = this._editor.DocumentManager.New(true);
                this.AddTextEditor(new TabItem(), doc);
                this.tabDocuments.SelectedIndex = this.tabDocuments.Items.Count - 1;
                doc.Text = diag.RetrievedData;
                doc.AutoDetectSyntax();
            }
        }

        private void mnuSave_Click(object sender, RoutedEventArgs e)
        {
            Document<TextEditor> doc = this._editor.DocumentManager.ActiveDocument;
            if (doc != null)
            {
                if (doc.Filename == null || doc.Filename.Equals(String.Empty))
                {
                    mnuSaveAs_Click(sender, e);
                }
                else
                {
                    doc.Save();
                }
            }
        }

        private void mnuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            Document<TextEditor> doc = this._editor.DocumentManager.ActiveDocument;
            if (doc != null)
            {
                String filename = this.SaveAsCallback(doc);
                if (filename != null)
                {
                    doc.SaveAs(_sfd.FileName);
                }
            }
        }

        private void mnuSaveAll_Click(object sender, RoutedEventArgs e)
        {
            this._editor.DocumentManager.SaveAll();
        }

        private void SaveWith(IRdfWriter writer)
        {
            if (this._editor.DocumentManager.ActiveDocument == null) return;

            Document<TextEditor> doc = this._editor.DocumentManager.ActiveDocument;
            IRdfReader parser = SyntaxManager.GetParser(doc.Syntax);
            if (parser == null)
            {
                MessageBox.Show("To use Save With the source document must be in a RDF Graph Syntax.  If the document is in a RDF Graph Syntax please change the syntax setting to the relevant format under Options > Syntax", "Save With Unavailable");
                return;
            }

            Graph g = new Graph();
            try
            {
                StringParser.Parse(g, doc.Text, parser);
            }
            catch
            {
                MessageBox.Show("Unable to Save With an RDF Writer as the current document is not a valid RDF document when parsed with the " + parser.GetType().Name + ".  If you believe this is a valid RDF document please select the correct Syntax Highlighting from the Options Menu and retry", "Save With Failed");
                return;
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
                System.IO.StringWriter strWriter = new System.IO.StringWriter();
                writer.Save(g, strWriter);
                Document<TextEditor> newDoc = this._editor.DocumentManager.New(true);
                newDoc.Text = strWriter.ToString();
                newDoc.AutoDetectSyntax();
                this.AddTextEditor(new TabItem(), newDoc);
                this.tabDocuments.SelectedIndex = this.tabDocuments.Items.Count - 1;
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

        private void mnuSaveWithPromptOptions_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.SaveWithOptionsPrompt = this.mnuSaveWithPromptOptions.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void mnuUseBomForUtf8_Click(object sender, RoutedEventArgs e)
        {
            Options.UseBomForUtf8 = this.mnuUseBomForUtf8.IsChecked;
            GlobalOptions.UseBomForUtf8 = Options.UseBomForUtf8;
            Properties.Settings.Default.UseUtf8Bom = Options.UseBomForUtf8;
            Properties.Settings.Default.Save();
        }

        private void mnuPageSetup_Click(object sender, RoutedEventArgs e)
        {
            if (this._editor.DocumentManager.ActiveDocument != null)
            {
                this._editor.DocumentManager.ActiveDocument.TextEditor.Control.PageSetupDialog();
            }
        }

        private void mnuPrintPreview_Click(object sender, RoutedEventArgs e)
        {
            if (this._editor.DocumentManager.ActiveDocument != null)
            {
                this._editor.DocumentManager.ActiveDocument.TextEditor.Control.PrintPreviewDialog(this._editor.DocumentManager.ActiveDocument.Title);
            }
        }

        private void mnuPrintPreviewNoHighlighting_Click(object sender, RoutedEventArgs e)
        {
            if (this._editor.DocumentManager.ActiveDocument != null)
            {
                this._editor.DocumentManager.ActiveDocument.TextEditor.Control.PrintPreviewDialog(this._editor.DocumentManager.ActiveDocument.Title, false);
            }
        }

        private void mnuPrint_Click(object sender, RoutedEventArgs e)
        {
            if (this._editor.DocumentManager.ActiveDocument != null)
            {
                this._editor.DocumentManager.ActiveDocument.TextEditor.Control.PrintDialog(this._editor.DocumentManager.ActiveDocument.Title);
            }
        }

        private void mnuPrintNoHighlighting_Click(object sender, RoutedEventArgs e)
        {
            if (this._editor.DocumentManager.ActiveDocument != null)
            {
                this._editor.DocumentManager.ActiveDocument.TextEditor.Control.PrintDialog(this._editor.DocumentManager.ActiveDocument.Title, false);
            }
        }

        private void mnuClose_Click(object sender, RoutedEventArgs e)
        {
            if (this._editor.DocumentManager.ActiveDocument != null)
            {
                if (this._editor.DocumentManager.Close())
                {
                    int index = this._editor.DocumentManager.ActiveDocumentIndex;
                    try
                    {
                        this.tabDocuments.Items.RemoveAt(this.tabDocuments.SelectedIndex);
                        this.tabDocuments.SelectedIndex = index;
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        //Ignore as may be possible to get into this state without intending
                        //to
                    }
                }
            }
        }

        private void mnuCloseAll_Click(object sender, RoutedEventArgs e)
        {
            this._editor.DocumentManager.CloseAll();
            foreach (TabItem tab in this.tabDocuments.Items)
            {
                tab.Content = null;
            }
            this.tabDocuments.Items.Clear();

            //Recreate new Tabs for any Documents that were not closed
            foreach (Document<TextEditor> doc in this._editor.DocumentManager.Documents)
            {
                this.AddTextEditor(new TabItem(), doc);
            }
            try
            {
                this._editor.DocumentManager.SwitchTo(0);
                this.tabDocuments.SelectedIndex = 0;
            }
            catch (IndexOutOfRangeException)
            {
                //Ignore as if there are no documents left this may be thrown
            }
        }

        private void mnuExit_Click(object sender, RoutedEventArgs e)
        {
            mnuCloseAll_Click(sender, e);
            if (this.tabDocuments.Items.Count == 0)
            {
                Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                Application.Current.Shutdown();
            }
        }

        #endregion

        #region Edit Menu

        private void mnuEdit_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            bool hasDoc = this._editor.DocumentManager.ActiveDocument != null;
            this.mnuUndo.IsEnabled = hasDoc;
            this.mnuRedo.IsEnabled = hasDoc;
            this.mnuCut.IsEnabled = hasDoc;
            this.mnuCopy.IsEnabled = hasDoc;
            this.mnuPaste.IsEnabled = hasDoc;
            this.mnuFind.IsEnabled = hasDoc;
            this.mnuFindNext.IsEnabled = hasDoc;
            this.mnuReplace.IsEnabled = hasDoc;
            this.mnuGoToLine.IsEnabled = hasDoc;
            this.mnuCommentSelection.IsEnabled = hasDoc;
            this.mnuUncommentSelection.IsEnabled = hasDoc;
            this.mnuSymbolBoundaries.IsEnabled = this._editor.DocumentManager.Options.IsSymbolSelectionEnabled;
        }

        private void mnuUndo_Click(object sender, RoutedEventArgs e)
        {
            if (this._editor.DocumentManager.ActiveDocument.TextEditor.Control.CanUndo)
            {
                this._editor.DocumentManager.ActiveDocument.TextEditor.Undo();
            }
        }

        private void mnuRedo_Click(object sender, RoutedEventArgs e)
        {
            if (this._editor.DocumentManager.ActiveDocument != null)
            {
                if (this._editor.DocumentManager.ActiveDocument.TextEditor.Control.CanRedo)
                {
                    this._editor.DocumentManager.ActiveDocument.TextEditor.Redo();
                }
            }
        }

        private void mnuCut_Click(object sender, RoutedEventArgs e)
        {
            if (this._editor.DocumentManager.ActiveDocument != null)
            {
                this._editor.DocumentManager.ActiveDocument.TextEditor.Cut();
            }
        }

        private void mnuCopy_Click(object sender, RoutedEventArgs e)
        {
            if (this._editor.DocumentManager.ActiveDocument != null)
            {
                this._editor.DocumentManager.ActiveDocument.TextEditor.Copy();
            }
        }

        private void mnuPaste_Click(object sender, RoutedEventArgs e)
        {
            if (this._editor.DocumentManager.ActiveDocument != null)
            {
                this._editor.DocumentManager.ActiveDocument.TextEditor.Paste();
            }
        }

        private void mnuFind_Click(object sender, RoutedEventArgs e)
        {
            if (this._editor.DocumentManager.ActiveDocument == null) return;
            if (this._findReplace == null)
            {
                this._findReplace = new FindReplace();
            }
            this._findReplace.Editor = this._editor.DocumentManager.ActiveDocument.TextEditor;
            if (this._findReplace.Visibility != Visibility.Visible)
            {
                this._findReplace.Mode = FindReplaceMode.Find;
                this._findReplace.Show();
            }
            this._findReplace.BringIntoView();
            this._findReplace.Focus();
        }

        private void mnuFindNext_Click(object sender, RoutedEventArgs e)
        {
            if (this._editor.DocumentManager.ActiveDocument == null) return;
            if (this._findReplace == null)
            {
                this.mnuFind_Click(sender, e);
            }
            else
            {
                this._findReplace.Editor = this._editor.DocumentManager.ActiveDocument.TextEditor;
                this._findReplace.FindNext();
            }
        }

        private void mnuReplace_Click(object sender, RoutedEventArgs e)
        {
            if (this._editor.DocumentManager.ActiveDocument == null) return;
            if (this._findReplace == null)
            {
                this._findReplace = new FindReplace();
            }
            this._findReplace.Mode = FindReplaceMode.FindAndReplace;
            this._findReplace.Editor = this._editor.DocumentManager.ActiveDocument.TextEditor;
            if (this._findReplace.Visibility != Visibility.Visible) this._findReplace.Show();
            this._findReplace.BringIntoView();
            this._findReplace.Focus();
        }

        private void mnuGoToLine_Click(object sender, RoutedEventArgs e)
        {
            if (this._editor.DocumentManager.ActiveDocument == null) return;
            ITextEditorAdaptor<TextEditor> editor = this._editor.DocumentManager.ActiveDocument.TextEditor;
            GoToLine gotoLine = new GoToLine(editor);
            gotoLine.Owner = this;
            if (gotoLine.ShowDialog() == true)
            {
                editor.ScrollToLine(gotoLine.Line);
            }
        }

        private void mnuCommentSelection_Click(object sender, RoutedEventArgs e)
        {
            if (this._editor.DocumentManager.ActiveDocument == null) return;

            Document<TextEditor> doc = this._editor.DocumentManager.ActiveDocument;
            TextEditor textEditor = doc.TextEditor.Control;
            if (textEditor.SelectionLength == 0) return;

            String syntax = doc.Syntax;
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
            if (this._editor.DocumentManager.ActiveDocument == null) return;

            Document<TextEditor> doc = this._editor.DocumentManager.ActiveDocument;
            TextEditor textEditor = doc.TextEditor.Control;
            if (textEditor.SelectionLength == 0) return;

            String syntax = doc.Syntax;
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
            this._editor.DocumentManager.Options.IsSymbolSelectionEnabled = this.mnuSymbolSelectEnabled.IsChecked;
            Properties.Settings.Default.EnableSymbolSelection = this.mnuSymbolSelectEnabled.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void mnuSymbolSelectIncludeBoundary_Click(object sender, RoutedEventArgs e)
        {
            this._editor.DocumentManager.Options.IncludeBoundaryInSymbolSelection = this.mnuSymbolSelectIncludeBoundary.IsChecked;
            Properties.Settings.Default.IncludeSymbolBoundaries = this.mnuSymbolSelectIncludeBoundary.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void SymbolSelectorMode_Click(object sender, RoutedEventArgs e)
        {
            MenuItem selected = sender as MenuItem;
            if (selected == null) return;
            String tag = (String)selected.Tag;
            if (selected.IsChecked == false) tag = "Default";

            foreach (MenuItem item in this.mnuSymbolBoundaries.Items.OfType<MenuItem>())
            {
                if (tag.Equals((String)item.Tag))
                {
                    item.IsChecked = true;
                }
                else
                {
                    item.IsChecked = false;
                }
            }

            ISymbolSelector<TextEditor> current = this._editor.DocumentManager.Options.CurrentSymbolSelector;
            switch (tag)
            {
                case "Punctuation":
                    if (!(current is PunctuationSelector<TextEditor>))
                    {
                        this._editor.DocumentManager.Options.CurrentSymbolSelector = new PunctuationSelector<TextEditor>();
                        Properties.Settings.Default.SymbolSelectionMode = tag;
                        Properties.Settings.Default.Save();
                    }
                    break;
                case "WhiteSpace":
                    if (!(current is WhiteSpaceSelector<TextEditor>))
                    {
                        this._editor.DocumentManager.Options.CurrentSymbolSelector = new WhiteSpaceSelector<TextEditor>();
                    }
                    break;
                case "All":
                    if (!(current is WhiteSpaceOrPunctuationSelection<TextEditor>))
                    {
                        this._editor.DocumentManager.Options.CurrentSymbolSelector = new WhiteSpaceOrPunctuationSelection<TextEditor>();
                    }
                    break;
                case "Default":
                default:
                    tag = "Default";
                    if (!(current is DefaultSelector<TextEditor>))
                    {
                        this._editor.DocumentManager.Options.CurrentSymbolSelector = new DefaultSelector<TextEditor>();
                    }
                    break;
            }

            //Update default Symbol Selection Mode
            Properties.Settings.Default.SymbolSelectionMode = tag;
            Properties.Settings.Default.Save();
        }

        #endregion

        #region View Menu

        private void mnuShowLineNumbers_Click(object sender, RoutedEventArgs e)
        {
            this._editor.DocumentManager.VisualOptions.ShowLineNumbers = this.mnuShowLineNumbers.IsChecked;
            Properties.Settings.Default.ShowLineNumbers = this._editor.DocumentManager.VisualOptions.ShowLineNumbers;
            Properties.Settings.Default.Save();
        }

        private void mnuWordWrap_Click(object sender, RoutedEventArgs e)
        {
            this._editor.DocumentManager.VisualOptions.WordWrap = this.mnuWordWrap.IsChecked;
            Properties.Settings.Default.WordWrap = this._editor.DocumentManager.VisualOptions.WordWrap;
            Properties.Settings.Default.Save();
        }

        private void mnuClickableUris_Click(object sender, RoutedEventArgs e)
        {
            this._editor.DocumentManager.VisualOptions.EnableClickableUris = this.mnuClickableUris.IsChecked;
            Properties.Settings.Default.EnableClickableUris = this._editor.DocumentManager.VisualOptions.EnableClickableUris;
            Properties.Settings.Default.Save();
        }

        private void mnuShowSpecialAll_Click(object sender, RoutedEventArgs e)
        {
            bool all = this.mnuShowSpecialAll.IsChecked;
            this.mnuShowSpecialEOL.IsChecked = all;
            this.mnuShowSpecialSpaces.IsChecked = all;
            this.mnuShowSpecialTabs.IsChecked = all;
            mnuShowSpecialEOL_Click(sender, e);
            mnuShowSpecialSpaces_Click(sender, e);
            mnuShowSpecialTabs_Click(sender, e);
        }

        private void mnuShowSpecialEOL_Click(object sender, RoutedEventArgs e)
        {
            this._editor.DocumentManager.VisualOptions.ShowEndOfLine = this.mnuShowSpecialEOL.IsChecked;
            Properties.Settings.Default.ShowEndOfLine = this._editor.DocumentManager.VisualOptions.ShowEndOfLine;
            Properties.Settings.Default.Save();
        }

        private void mnuShowSpecialSpaces_Click(object sender, RoutedEventArgs e)
        {
            this._editor.DocumentManager.VisualOptions.ShowSpaces = this.mnuShowSpecialSpaces.IsChecked;
            Properties.Settings.Default.ShowSpaces = this._editor.DocumentManager.VisualOptions.ShowSpaces;
            Properties.Settings.Default.Save();
        }

        private void mnuShowSpecialTabs_Click(object sender, RoutedEventArgs e)
        {
            this._editor.DocumentManager.VisualOptions.ShowTabs = this.mnuShowSpecialTabs.IsChecked;
            Properties.Settings.Default.ShowTabs = this._editor.DocumentManager.VisualOptions.ShowTabs;
            Properties.Settings.Default.Save();
        }

        #endregion

        #region Options Menu

        private void mnuEnableHighlighting_Click(object sender, RoutedEventArgs e)
        {
            this._editor.DocumentManager.Options.IsSyntaxHighlightingEnabled = mnuEnableHighlighting.IsChecked;
            Properties.Settings.Default.EnableHighlighting = this._editor.DocumentManager.Options.IsSyntaxHighlightingEnabled;
            Properties.Settings.Default.Save();
        }

        private void mnuCurrentHighlighter_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            String currSyntax = this._editor.DocumentManager.ActiveDocument != null ? this._editor.DocumentManager.ActiveDocument.Syntax : "None";
            foreach (MenuItem item in mnuCurrentHighlighter.Items.OfType<MenuItem>())
            {
                if (item.Tag != null)
                {
                    if (item.Tag.Equals(currSyntax))
                    {
                        item.IsChecked = true;
                    }
                    else
                    {
                        item.IsChecked = false;
                    }
                    String header = (String)item.Header;
                    if (header.EndsWith(" (Default)"))
                    {
                        if (!item.Tag.Equals(this._editor.DocumentManager.DefaultSyntax))
                        {
                            item.Header = header.Substring(0, header.Length - 10);
                        }
                    }
                    else if (item.Tag.Equals(this._editor.DocumentManager.DefaultSyntax))
                    {
                        item.Header += " (Default)";
                    }
                }
            }
        }

        private void mnuSetDefaultHighlighter_Click(object sender, RoutedEventArgs e)
        {
            this._editor.DocumentManager.DefaultSyntax = this._editor.DocumentManager.ActiveDocument != null ? this._editor.DocumentManager.ActiveDocument.Syntax : "None";
            Properties.Settings.Default.DefaultHighlighter = this._editor.DocumentManager.DefaultSyntax;
            Properties.Settings.Default.Save();
        }

        private void mnuSetHighlighter_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (item == null) return;
            if (item.Tag == null) return;
            if (this._editor.DocumentManager.ActiveDocument == null) return;
            this._editor.DocumentManager.ActiveDocument.Syntax = (String)item.Tag;
        }

        private void mnuValidateAsYouType_Click(object sender, RoutedEventArgs e)
        {
            this._editor.DocumentManager.Options.IsValidateAsYouTypeEnabled = this.mnuValidateAsYouType.IsChecked;
            Properties.Settings.Default.EnableValidateAsYouType = this._editor.DocumentManager.Options.IsValidateAsYouTypeEnabled;
            Properties.Settings.Default.Save();
            if (this._editor.DocumentManager.Options.IsValidateAsYouTypeEnabled)
            {
                if (this._editor.DocumentManager.ActiveDocument != null)
                {
                    this._editor.DocumentManager.ActiveDocument.Validate();
                }
            }
            else
            {
                this.stsSyntaxValidation.Content = "Validate as you Type disabled, go to Tools > Validate Syntax to check your syntax";
                this.stsSyntaxValidation.ToolTip = String.Empty;
            }
        }

        private void mnuHighlightErrors_Click(object sender, RoutedEventArgs e)
        {
            this._editor.DocumentManager.Options.IsHighlightErrorsEnabled = this.mnuHighlightErrors.IsChecked;
            Properties.Settings.Default.EnableErrorHighlighting = this._editor.DocumentManager.Options.IsHighlightErrorsEnabled;
            Properties.Settings.Default.Save();
        }

        private void mnuAutoComplete_Click(object sender, RoutedEventArgs e)
        {
            this._editor.DocumentManager.Options.IsAutoCompletionEnabled = this.mnuAutoComplete.IsChecked;
            Properties.Settings.Default.EnableAutoComplete = this._editor.DocumentManager.Options.IsAutoCompletionEnabled;
            Properties.Settings.Default.Save();
        }

        private void mnuCustomiseFileAssociations_Click(object sender, RoutedEventArgs e)
        {
            FileAssociations diag = new FileAssociations();
            diag.ShowDialog();
        }

        private void mnuCustomiseAppearance_Click(object sender, RoutedEventArgs e)
        {
            AppearanceSettings settings = new AppearanceSettings(this._editor.DocumentManager.VisualOptions);
            settings.Owner = this;
            if (settings.ShowDialog() == true)
            {
                if (Properties.Settings.Default.EditorFontFace != null)
                {
                    this._editor.DocumentManager.VisualOptions.FontFace = Properties.Settings.Default.EditorFontFace;
                }
                this._editor.DocumentManager.VisualOptions.FontSize = Math.Round(Properties.Settings.Default.EditorFontSize, 0);
                this._editor.DocumentManager.VisualOptions.Foreground = Properties.Settings.Default.EditorForeground;
                this._editor.DocumentManager.VisualOptions.Background = Properties.Settings.Default.EditorBackground;

                this._editor.DocumentManager.VisualOptions.ErrorBackground = Properties.Settings.Default.ErrorHighlightBackground;
                this._editor.DocumentManager.VisualOptions.ErrorDecoration = Properties.Settings.Default.ErrorHighlightDecoration;
                if (Properties.Settings.Default.ErrorHighlightFontFamily != null)
                {
                    this._editor.DocumentManager.VisualOptions.ErrorFontFace = Properties.Settings.Default.ErrorHighlightFontFamily;
                }
                this._editor.DocumentManager.VisualOptions.ErrorForeground = Properties.Settings.Default.ErrorHighlightForeground;
            }
        }

        #endregion

        #region Tools Menu

        private void mnuTools_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            bool hasDoc = this._editor.DocumentManager.ActiveDocument != null;
            this.mnuValidateSyntax.IsEnabled = hasDoc;
            this.mnuStructureView.IsEnabled = hasDoc;
        }

        private void mnuValidateSyntax_Click(object sender, RoutedEventArgs e)
        {
            if (this._editor.DocumentManager.ActiveDocument == null) return;

            ISyntaxValidator validator = this._editor.DocumentManager.ActiveDocument.SyntaxValidator;
            if (validator != null)
            {
                ISyntaxValidationResults results = this._editor.DocumentManager.ActiveDocument.Validate();
                String caption = results.IsValid ? "Valid Syntax" : "Invalid Syntax";
                MessageBox.Show(results.Message, caption);
            }
            else
            {
                MessageBox.Show("Validation is not possible as there is no Syntax Validator registered for your currently selected Syntax Highlighting", "Validation Unavailable");
            }
        }

        private void mnuStructureView_Click(object sender, RoutedEventArgs e)
        {
            if (this._editor.DocumentManager.ActiveDocument == null) return;
            ISyntaxValidator validator = this._editor.DocumentManager.ActiveDocument.SyntaxValidator;
            if (validator != null)
            {
                ISyntaxValidationResults results = validator.Validate(this._editor.DocumentManager.ActiveDocument.Text);
                if (results.IsValid)
                {
                    if (!this._editor.DocumentManager.ActiveDocument.Syntax.Equals("None"))
                    {
                        try
                        {
                            SyntaxDefinition def = SyntaxManager.GetDefinition(this._editor.DocumentManager.ActiveDocument.Syntax);
                            if (def.DefaultParser != null)
                            {
                                NonIndexedGraph g = new NonIndexedGraph();
                                def.DefaultParser.Load(g, new StringReader(this._editor.DocumentManager.ActiveDocument.Text));
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
                                StringParser.ParseResultSet(sparqlResults, this._editor.DocumentManager.ActiveDocument.Text);
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

        private void NewFromActiveCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            mnuNewFromActive_Click(sender, e);
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

        private void SaveAllCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            mnuSaveAll_Click(sender, e);
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

        private void CloseAllCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            mnuCloseAll_Click(sender, e);
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
            this._editor.DocumentManager.VisualOptions.FontSize = Math.Round(this._editor.DocumentManager.VisualOptions.FontSize + 1.0, 0);
            Properties.Settings.Default.EditorFontSize = this._editor.DocumentManager.VisualOptions.FontSize;
            Properties.Settings.Default.Save();
        }

        private void DecreaseTextSizeExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this._editor.DocumentManager.VisualOptions.FontSize >= 5.0d)
            {
                this._editor.DocumentManager.VisualOptions.FontSize = Math.Round(this._editor.DocumentManager.VisualOptions.FontSize - 1.0, 0);
                Properties.Settings.Default.EditorFontSize = this._editor.DocumentManager.VisualOptions.FontSize;
                Properties.Settings.Default.Save();
            }
        }

        private void ResetTextSizeExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this._editor.DocumentManager.VisualOptions.FontSize = 13.0d;
            Properties.Settings.Default.EditorFontSize = this._editor.DocumentManager.VisualOptions.FontSize;
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

        #region Other Event Handlers

        void HandleDocumentCreated(object sender, DocumentChangedEventArgs<TextEditor> args)
        {
            args.Document.TextEditor.Control.TextArea.TextView.ElementGenerators.Add(new ValidationErrorElementGenerator(args.Document.TextEditor as WpfEditorAdaptor, this._editor.DocumentManager.VisualOptions));
        }

        private void HandleValidatorChanged(Object sender, DocumentChangedEventArgs<TextEditor> args)
        {
            if (ReferenceEquals(args.Document, this._editor.DocumentManager.ActiveDocument))
            {
                if (args.Document.SyntaxValidator == null)
                {
                    this.stsSyntaxValidation.Content = "No Syntax Validator available for the currently selected syntax";
                }
                else
                {
                    this.stsSyntaxValidation.Content = "Syntax Validation available, enable Validate as you Type or select Tools > Validate to validate";
                    this._editor.DocumentManager.ActiveDocument.Validate();
                }
            }
        }

        private void HandleValidation(Object sender, DocumentValidatedEventArgs<TextEditor> args)
        {
            if (ReferenceEquals(args.Document, this._editor.DocumentManager.ActiveDocument))
            {
                this.stsSyntaxValidation.ToolTip = String.Empty;
                if (args.ValidationResults != null)
                {
                    this.stsSyntaxValidation.Content = args.ValidationResults.Message;
                    //Build a TextBlock with wrapping for the ToolTip
                    TextBlock block = new TextBlock();
                    block.TextWrapping = TextWrapping.Wrap;
                    block.Width = 800;
                    block.Text = args.ValidationResults.Message;
                    this.stsSyntaxValidation.ToolTip = block;
                    if (args.ValidationResults.Warnings.Any())
                    {
                        this.stsSyntaxValidation.ToolTip += "\n" + String.Join("\n", args.ValidationResults.Warnings.ToArray());
                    }
                }
                else
                {
                    this.stsSyntaxValidation.Content = "Syntax Validation unavailable";
                }
            }
        }

        private void HandleTextChanged(Object sender, DocumentChangedEventArgs<TextEditor> args)
        {
            this.mnuUndo.IsEnabled = this._editor.DocumentManager.ActiveDocument != null && this._editor.DocumentManager.ActiveDocument.TextEditor.Control.CanUndo;
            this.mnuRedo.IsEnabled = this._editor.DocumentManager.ActiveDocument != null && this._editor.DocumentManager.ActiveDocument.TextEditor.Control.CanRedo;
        }

        private void tabDocuments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.tabDocuments.SelectedIndex >= 0 && this.tabDocuments.Items.Count > 0)
            {
                try
                {
                    this._editor.DocumentManager.SwitchTo(this.tabDocuments.SelectedIndex);
                    this.stsCurrSyntax.Content = "Current Syntax: " + this._editor.DocumentManager.ActiveDocument.Syntax;
                    this.stsSyntaxValidation.Content = String.Empty;
                    this.stsSyntaxValidation.ToolTip = String.Empty;
                    this._editor.DocumentManager.ActiveDocument.Validate();
                }
                catch (IndexOutOfRangeException)
                {
                    //Ignore this since we may get this because of events firing after objects have already
                    //been thrown away
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Open a File if we've been asked to do so
            String[] args = Environment.GetCommandLineArgs();
            if (args.Length >= 2)
            {
                if (File.Exists(args[1]))
                {
                    Document<TextEditor> doc = this._editor.DocumentManager.New();
                    try
                    {
                        doc.Open(args[1]);
                        this.AddTextEditor(new TabItem(), doc);
                        this._editor.DocumentManager.Close(0);
                        this.tabDocuments.Items.RemoveAt(0);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred while opening the selected file: " + ex.Message, "Unable to Open File");
                        this._editor.DocumentManager.Close(this._editor.DocumentManager.Count - 1);
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
                mnuCloseAll_Click(sender, new RoutedEventArgs());
                if (this.tabDocuments.Items.Count == 0)
                {
                    Application.Current.Shutdown();
                }
                else
                {
                    e.Cancel = true;
                }
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

        void EditorWindow_Drop(object sender, DragEventArgs e)
        {
            //Is the data FileDrop data?
            String[] droppedFilePaths = e.Data.GetData(DataFormats.FileDrop, false) as string[];
            if (droppedFilePaths == null) return;

            e.Handled = true;

            foreach (String file in droppedFilePaths)
            {
                Document<TextEditor> doc = this._editor.DocumentManager.New();
                try
                {
                    doc.Open(file);
                    this.AddTextEditor(new TabItem(), doc);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("The dropped file '" + file + "' could not be opened due to an error: " + ex.Message, "Open File Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    this._editor.DocumentManager.Close(this._editor.DocumentManager.Count - 1);
                }
            }
        }

        #endregion

        #region Callbacks

        private SaveChangesMode SaveChangesCallback(Document<TextEditor> doc)
        {
            MessageBoxResult result = System.Windows.MessageBox.Show(doc.Title + " has unsaved changes, do you wish to save these changes before closing the document?", "Save Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    return SaveChangesMode.Save;
                case MessageBoxResult.Cancel:
                    return SaveChangesMode.Cancel;
                case MessageBoxResult.No:
                default:
                    return SaveChangesMode.Discard;
            }
        }

        private String SaveAsCallback(Document<TextEditor> doc)
        {
            _sfd.Filter = this.FileFilterAll;
            if (doc.Filename == null || doc.Filename.Equals(String.Empty))
            {
                _sfd.Title = "Save " + doc.Title + " As...";
            }
            else
            {
                _sfd.Title = "Save " + System.IO.Path.GetFileName(doc.Filename) + " As...";
                _sfd.InitialDirectory = System.IO.Path.GetDirectoryName(doc.Filename);
                _sfd.FileName = doc.Filename;
            }

            if (this._sfd.ShowDialog() == true)
            {
                return this._sfd.FileName;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region MRU List

        private void ShowMruList()
        {
            if (VDS.RDF.Utilities.Editor.Wpf.App.RecentFiles != null)
            {
                while (this.mnuRecentFiles.Items.Count > 2)
                {
                    this.mnuRecentFiles.Items.RemoveAt(2);
                }

                int i = 0;
                foreach (String file in VDS.RDF.Utilities.Editor.Wpf.App.RecentFiles.Files)
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
            if (VDS.RDF.Utilities.Editor.Wpf.App.RecentFiles != null)
            {
                VDS.RDF.Utilities.Editor.Wpf.App.RecentFiles.Add(file);
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
                            Document<TextEditor> doc;
                            bool add = false;
                            if (this._editor.DocumentManager.ActiveDocument != null && this._editor.DocumentManager.ActiveDocument.TextLength == 0 && String.IsNullOrEmpty(this._editor.DocumentManager.ActiveDocument.Filename))
                            {
                                doc = this._editor.DocumentManager.ActiveDocument;
                            } 
                            else 
                            {
                                doc = this._editor.DocumentManager.New();
                                add = true;
                            }
                            try
                            {
                                doc.Open(file);
                                if (add)
                                {
                                    this.AddTextEditor(new TabItem(), doc);
                                    this._editor.DocumentManager.SwitchTo(this._editor.DocumentManager.Count - 1);
                                    this.tabDocuments.SelectedIndex = this.tabDocuments.Items.Count - 1;
                                    this.tabDocuments.SelectedItem = this.tabDocuments.Items[this.tabDocuments.Items.Count - 1];
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("An error occurred while opening the selected file: " + ex.Message, "Open File Failed");
                                this._editor.DocumentManager.Close(this._editor.DocumentManager.Count - 1);
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

        private void mnuClearRecentFiles_Click(object sender, RoutedEventArgs e)
        {
            if (VDS.RDF.Utilities.Editor.Wpf.App.RecentFiles != null)
            {
                VDS.RDF.Utilities.Editor.Wpf.App.RecentFiles.Clear();
            }
        }

        #endregion
    }
}
