using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
using ICSharpCode.AvalonEdit.Rendering;
using Microsoft.Win32;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Validation;
using VDS.RDF.Query;
using VDS.RDF.Update;
using VDS.RDF.Writing;
using rdfEditor.Syntax;
using rdfEditor.AutoComplete;

namespace rdfEditor
{
    public class EditorManager
    {
        private TextEditor _editor;
        private MenuItem _highlightersMenu;
        private bool _changed = false;
        private bool _enableHighlighting = true;
        private String _currFile;

        //Validation
        private bool _validateAsYouType = false;
        private ISyntaxValidator _currValidator;
        private StatusBarItem _validatorStatus;
        private Exception _lastError = null;
        private bool _highlightErrors = true;
        private ToolTip _errorTip = new ToolTip();

        //Auto-complete
        private Dictionary<String, IAutoCompleter> _completers = new Dictionary<string, IAutoCompleter>();
        private bool _enableAutoComplete = true;
        private bool _endAutoComplete = false;
        private IAutoCompleter _autoCompleter;
        private int _lastCaretPos = 0;

        public EditorManager(TextEditor editor)
        {
            SyntaxManager.Initialise();
            AutoCompleteManager.Initialise();
            this._editor = editor;

            //Register Event Handlers for the Editor
            this._editor.TextChanged += new EventHandler(EditorTextChanged);
            this._editor.TextArea.TextEntering += new TextCompositionEventHandler(EditorTextEntering);
            this._editor.TextArea.TextEntered += new TextCompositionEventHandler(EditorTextEntered);
            //this._editor.TextArea.TextView.MouseDown += new MouseButtonEventHandler(TextView_MouseDown);
            //this._editor.TextArea.TextView.MouseUp += new MouseButtonEventHandler(TextView_MouseUp);
            this._editor.Document.Changed += new EventHandler<DocumentChangeEventArgs>(EditorDocumentChanged);
            this._editor.Document.UpdateFinished += new EventHandler(EditorDocumentUpdateFinished);

            //Add the Validation Error Element Generator
            this._editor.TextArea.TextView.ElementGenerators.Add(new ValidationErrorElementGenerator(this));
        }

        //void TextView_MouseUp(object sender, MouseButtonEventArgs e)
        //{
        //    if (this._lastCaretPos != this._editor.CaretOffset)
        //    {
        //        if (this._enableAutoComplete && this._autoCompleter != null)
        //        {
        //            this._autoCompleter.DetectState(this._editor);
        //        }
        //    }
        //}

        //void TextView_MouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    this._lastCaretPos = this._editor.CaretOffset;
        //}

        public EditorManager(TextEditor editor, MenuItem highlightersMenu)
            : this(editor)
        {
            this._highlightersMenu = highlightersMenu;

            //Need to register the Event Handlers for the Menu Items in the Highlighters Menu
            foreach (MenuItem item in this._highlightersMenu.Items.OfType<MenuItem>())
            {
                if (item.Tag != null)
                {
                    item.Click += new RoutedEventHandler(HighlighterClick);
                    String syntax = (String)item.Tag;
                    if (!syntax.Equals("None"))
                    {
                        if (syntax.Equals(Properties.Settings.Default.DefaultHighlighter))
                        {
                            item.Header += " (Default)";
                        }
                    }
                }
            }
        }

        public EditorManager(TextEditor editor, MenuItem highlightersMenu, StatusBarItem validatorStatus)
            : this(editor, highlightersMenu)
        {
            this._validatorStatus = validatorStatus;
            this._validateAsYouType = true;
        }

        public bool HasChanged
        {
            get
            {
                return this._changed;
            }
            set
            {
                this._changed = value;
            }
        }

        public bool IsHighlightingEnabled
        {
            get
            {
                return this._enableHighlighting;
            }
            set
            {
                this._enableHighlighting = value;

                //Detect Syntax Highlighting, Validator and Auto-Complete settings
                this.AutoDetectSyntax();
                if (!this._enableHighlighting) this._editor.SyntaxHighlighting = null;
                if (this._highlightersMenu != null)
                {
                    this._highlightersMenu.IsEnabled = this._enableHighlighting;
                }
            }
        }

        public bool IsValidateAsYouType
        {
            get
            {
                return this._validateAsYouType;
            }
            set
            {
                this._validateAsYouType = value;
            }
        }

        public bool IsHighlightErrorsEnabled
        {
            get
            {
                return this._highlightErrors;
            }
            set
            {
                this._highlightErrors = value;
                if (this._highlightErrors)
                {
                    if (!this._editor.TextArea.TextView.ElementGenerators.Any(g => g is ValidationErrorElementGenerator))
                    {
                        this._editor.TextArea.TextView.ElementGenerators.Add(new ValidationErrorElementGenerator(this));
                    }
                }
                else
                {
                    if (this._editor.TextArea.TextView.ElementGenerators.Any(g => g is ValidationErrorElementGenerator))
                    {
                        for (int i = 0; i < this._editor.TextArea.TextView.ElementGenerators.Count; i++)
                        {
                            if (this._editor.TextArea.TextView.ElementGenerators[i] is ValidationErrorElementGenerator)
                            {
                                this._editor.TextArea.TextView.ElementGenerators.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                }
            }
        }

        public bool IsAutoCompleteEnabled
        {
            get
            {
                return this._enableAutoComplete;
            }
            set
            {
                this._enableAutoComplete = value;
                if (this._autoCompleter != null)
                {
                    if (!this._enableAutoComplete)
                    {
                        this._autoCompleter.State = AutoCompleteState.Disabled;
                    }
                    else
                    {
                        this._autoCompleter.Initialise(this._editor);
                    }
                }
            }
        }

        public String CurrentFile
        {
            get
            {
                return this._currFile;
            }
            set
            {
                this._currFile = value;
            }
        }

        public IHighlightingDefinition CurrentHighlighter
        {
            get
            {
                return this._editor.SyntaxHighlighting;
            }
        }

        public ISyntaxValidator CurrentValidator
        {
            get
            {
                return this._currValidator;
            }
        }

        public Exception LastValidationError
        {
            get
            {
                return this._lastError;
            }
            set
            {
                this._lastError = value;
            }
        }
        
        public void AutoDetectSyntax()
        {
            this.AutoDetectSyntax(this._currFile);
        }

        public void AutoDetectSyntax(String filename)
        {
            if (this._editor == null) return; //Not yet ready

            if (filename == null)
            {
                try
                {
                    //First see if it's an RDF format
                    IRdfReader parser = StringParser.GetParser(this._editor.Text);

                    if (parser is NTriplesParser)
                    {
                        //NTriples is the fallback so if we get this check if it's actually SPARQL Results
                        try
                        {
                            ISparqlResultsReader sparqlParser = StringParser.GetResultSetParser(this._editor.Text);
                        }
                        catch (RdfParserSelectionException)
                        {
                            //Not a valid SPARQL Results format - may be a SPARQL Query or a SPARQL Update?
                            SparqlQueryParser queryParser = new SparqlQueryParser(SparqlQuerySyntax.Sparql_1_1);
                            try
                            {
                                SparqlQuery q = queryParser.ParseFromString(this._editor.Text);
                                this.SetHighlighter("SparqlQuery11");
                            }
                            catch (RdfParseException)
                            {
                                //Not a valid SPARQL Query - valid SPARQL Update?
                                SparqlUpdateParser updateParser = new SparqlUpdateParser();
                                try
                                {
                                    SparqlUpdateCommandSet cmds = updateParser.ParseFromString(this._editor.Text);
                                    this.SetHighlighter("SparqlUpdate11");
                                }
                                catch (RdfParseException)
                                {
                                    //Was probably actually NTriples
                                    this.SetHighlighter(parser);
                                }
                            }
                        }
                    }
                    else
                    {
                        //Got a non NTriples RDF parser so use that to set Highlighter
                        this.SetHighlighter(parser);
                    }
                }
                catch (RdfParserSelectionException)
                {
                    this.SetNoHighlighting();
                }
                return;
            }

            try
            {
                IHighlightingDefinition def = HighlightingManager.Instance.GetDefinitionByExtension(System.IO.Path.GetExtension(filename));
                if (this._enableHighlighting) this._editor.SyntaxHighlighting = def;

                if (def != null)
                {
                    this.SetCurrentHighlighterChecked(def.Name);
                    this.SetCurrentValidator(def.Name);
                    this.SetCurrentAutoCompleter(def.Name);
                }
                else
                {
                    this.SetNoHighlighting();
                }
            }
            catch
            {
                this.SetNoHighlighting();
            }
        }

        public void SetHighlighter(String name)
        {
            String syntax;
            if (this._enableHighlighting)
            {
                this._editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition(name);
                syntax = (this._editor.SyntaxHighlighting == null) ? "None" : name;
            }
            else
            {
                syntax = name;
            }
            this.SetCurrentHighlighterChecked(syntax);
            this.SetCurrentValidator(syntax);
            this.SetCurrentAutoCompleter(syntax);
        }

        public void SetHighlighter(IHighlightingDefinition def)
        {
            String syntax;
            if (this._enableHighlighting)
            {
                this._editor.SyntaxHighlighting = def;
                syntax = (this._editor.SyntaxHighlighting == null) ? "None" : def.Name;
            }
            else
            {
                syntax = def.Name;
            }
            this.SetCurrentHighlighterChecked(syntax);
            this.SetCurrentValidator(syntax);
            this.SetCurrentAutoCompleter(syntax);
        }

        public void SetHighlighter(IRdfReader parser)
        {
            if (parser is NTriplesParser)
            {
                this.SetHighlighter("NTriples");
            }
            else if (parser is TurtleParser)
            {
                this.SetHighlighter("Turtle");
            }
            else if (parser is Notation3Parser)
            {
                this.SetHighlighter("Notation3");
            }
            else if (parser is RdfXmlParser)
            {
                this.SetHighlighter("RdfXml");
            }
            else if (parser is RdfJsonParser)
            {
                this.SetHighlighter("RdfJson");
            }
            else if (parser is RdfAParser)
            {
                this.SetHighlighter("XHtmlRdfA");
            }
            else
            {
                this.SetNoHighlighting();
            }
        }

        public void SetHighlighter(ISparqlResultsReader parser)
        {
            if (parser is SparqlXmlParser)
            {
                this.SetHighlighter("SparqlResultsXml");
            }
            else if (parser is SparqlJsonParser)
            {
                this.SetHighlighter("SparqlResultsJson");
            }
            else
            {
                this.SetNoHighlighting();
            }
        }

        public void SetValidator(ISyntaxValidator validator)
        {
            this._currValidator = validator;
            if (this._validatorStatus != null)
            {
                if (this._validateAsYouType)
                {
                    this.DoValidation();
                }
                else
                {
                    this._validatorStatus.Content = "Validate Syntax as you Type is Disabled";
                }
            }
        }

        public void SetAutoCompleter(IAutoCompleter completer)
        {
            this._autoCompleter = completer;
            if (this._autoCompleter != null)
            {
                this._autoCompleter.Initialise(this._editor);
                if (!this._enableAutoComplete)
                {
                    this._autoCompleter.State = AutoCompleteState.Disabled;
                }
            }
        }

        public void SetAutoCompleter(String name)
        {
            this.SetCurrentAutoCompleter(name);
        }

        public void ToggleHighlighter(String name)
        {
            if (this._editor.SyntaxHighlighting != null)
            {
                //Currently a highlighter selected
                if (this._editor.SyntaxHighlighting.Name.Equals(name))
                {
                    //It's the active highlighter we've toggled so set to off
                    this.SetNoHighlighting();
                }
                else
                {
                    //Switching to a new Syntax Highlighter
                    this._editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition(name);
                    if (this._editor.SyntaxHighlighting != null)
                    {
                        //Valid Highlighter so check the appropriate highlighter in the list
                        this.SetCurrentValidator(name);
                        this.SetCurrentHighlighterChecked(name);
                        this.SetCurrentAutoCompleter(name);
                    }
                    else
                    {
                        //Not a valid Highlighter so set to no Highlighting
                        this.SetNoHighlighting();
                    }
                }
            }
            else if (!name.Equals("None"))
            {
                //No Highlighting currently but enabling a Highlighter
                this._editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition(name);
                if (this._editor.SyntaxHighlighting != null)
                {
                    //Valid Highlighter so check the appropriate highlighter in the list
                    this.SetCurrentValidator(name);
                    this.SetCurrentHighlighterChecked(name);
                    this.SetCurrentAutoCompleter(name);
                }
                else
                {
                    //Not a valid Highlighter so set to no Highlighting
                    this.SetNoHighlighting();
                }
            }
            else
            {
                //No Highlighting selected
                this.SetNoHighlighting();
            }
        }

        public void SetNoHighlighting()
        {
            this._editor.SyntaxHighlighting = null;
            this.SetCurrentHighlighterChecked("None");
            this.SetCurrentValidator("None");
            this.SetCurrentAutoCompleter("None");
        }

        private void SetCurrentHighlighterChecked(String name)
        {
            if (!this._enableHighlighting) return;

            if (this._highlightersMenu != null)
            {
                foreach (MenuItem item in this._highlightersMenu.Items.OfType<MenuItem>())
                {
                    if (item.Tag == null) continue;
                    item.IsChecked = item.Tag.Equals(name);
                }
            }
        }

        private void SetCurrentValidator(String name)
        {
            this._currValidator = SyntaxManager.GetValidator(name);
            if (this._validatorStatus != null)
            {
                if (this._validateAsYouType)
                {
                    this.DoValidation();
                }
                else
                {
                    this._validatorStatus.Content = "Validate Syntax as you Type is Disabled";
                }
            }
        }

        private void SetCurrentAutoCompleter(String name)
        {
            //If disabled then no Auto-Completer will be set
            if (!this._enableAutoComplete)
            {
                this._autoCompleter = null;
                return;
            }

            if (this._completers.ContainsKey(name))
            {
                this._autoCompleter = this._completers[name];
            }
            else
            {
                this._autoCompleter = AutoCompleteManager.GetAutoCompleter(name);
            }
            if (this._autoCompleter != null)
            {
                //Cache auto-completer only if non-null
                if (!this._completers.ContainsKey(name)) this._completers.Add(name, this._autoCompleter);
                this._autoCompleter.Initialise(this._editor);
                if (!this._enableAutoComplete)
                {
                    this._autoCompleter.State = AutoCompleteState.Disabled;
                }
            }
        }

        public IRdfReader GetParser()
        {
            IRdfReader parser = null;
            if (this._enableHighlighting)
            {
                //Use the current Highlighting to select the Parser
                if (this._editor.SyntaxHighlighting != null)
                {
                    parser = SyntaxManager.GetParser(this._editor.SyntaxHighlighting.Name);
                    if (parser == null)
                    {
                        parser = StringParser.GetParser(this._editor.Text);
                    }
                }
                else
                {
                    parser = StringParser.GetParser(this._editor.Text);
                }
            }
            else if (this._currFile != null)
            {
                //Use the Current Filename to select the Parser
                try
                {
                    parser = MimeTypesHelper.GetParser(MimeTypesHelper.GetMimeType(System.IO.Path.GetExtension(this._currFile)));
                }
                catch (RdfParserSelectionException)
                {
                    parser = StringParser.GetParser(this._editor.Text);
                }
            }
            else
            {
                //Use Heuristics to guess the parser
                parser = StringParser.GetParser(this._editor.Text);
            }
            return parser;
        }

        public void DoValidation()
        {
            if (this._currValidator == null)
            {
                this._lastError = null;
                this._validatorStatus.Content = "No Validator is available for currently selected Syntax";
            }
            else
            {
                ISyntaxValidationResults results = this._currValidator.Validate(this._editor.Text);
                this._validatorStatus.Content = results.Message;
                this._lastError = results.Error;
                ToolTip tip = new ToolTip();
                TextBlock block = new TextBlock();
                block.TextWrapping = TextWrapping.Wrap;
                block.Width = 800;
                block.Text = results.Message;
                tip.Content = block;
                this._validatorStatus.ToolTip = tip;

                //TODO: Work out how to invalidate specific visual lines to force them to be redrawn
                //if (results.Error != null)
                //{
                //    if (results.Error is RdfParseException)
                //    {
                //        RdfParseException parseEx = (RdfParseException)results.Error;
                //        if (parseEx.StartLine != parseEx.EndLine && parseEx.StartLine > parseEx.EndLine)
                //        {
                //            for (int i = parseEx.StartLine; i < parseEx.EndLine; i++)
                //            {
                //                this._editor.TextArea.TextView.VisualLines[i]
                //            }
                //        }
                //    }
                //}
            }
        }

        #region Event Handlers

        private void EditorTextChanged(object sender, EventArgs e)
        {
            this._changed = true;
            if (this._validateAsYouType && this._validatorStatus != null)
            {
                this.DoValidation();
            }
        }

        private void EditorTextEntered(object sender, TextCompositionEventArgs e)
        {
            if (!this._enableAutoComplete) return;
            if (this._autoCompleter == null) return;

            this._autoCompleter.TryAutoComplete(this._editor, e);

            if (this._enableAutoComplete && this._autoCompleter != null)
            {
                System.Diagnostics.Debug.WriteLine("State: " + this._autoCompleter.State.ToString());
            }
        }

        private void EditorTextEntering(object sender, TextCompositionEventArgs e)
        {

        }

        private void EditorDocumentChanged(object sender, DocumentChangeEventArgs e)
        {
            if (e.InsertionLength > 0)
            {
                if (this._autoCompleter != null)
                {
                    this._endAutoComplete = true;
                }
            }
        }

        private void EditorDocumentUpdateFinished(object sender, EventArgs e)
        {
            if (this._endAutoComplete)
            {
                if (this._autoCompleter != null) this._autoCompleter.EndAutoComplete(this._editor);
            }
        }

        private void HighlighterClick(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem)
            {
                this.ToggleHighlighter(((MenuItem)sender).Tag.ToString());
            }
        }

        #endregion
    }
}
