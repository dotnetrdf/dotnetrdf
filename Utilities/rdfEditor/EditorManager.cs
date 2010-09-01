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
using Microsoft.Win32;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
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

        //Auto-complete
        private Dictionary<String, IAutoCompleter> _completers = new Dictionary<string, IAutoCompleter>();
        private bool _enableAutoComplete = true;
        private bool _endAutoComplete = false;
        private IAutoCompleter _autoCompleter;

        public EditorManager(TextEditor editor)
        {
            SyntaxManager.Initialise();
            AutoCompleteManager.Initialise();
            this._editor = editor;

            //Register Event Handlers for the Editor
            this._editor.TextChanged += new EventHandler(EditorTextChanged);
            this._editor.TextArea.TextEntering += new TextCompositionEventHandler(EditorTextEntering);
            this._editor.TextArea.TextEntered += new TextCompositionEventHandler(EditorTextEntered);
            this._editor.Document.Changed += new EventHandler<DocumentChangeEventArgs>(EditorDocumentChanged);
            this._editor.Document.UpdateFinished += new EventHandler(EditorDocumentUpdateFinished);
        }

        public EditorManager(TextEditor editor, MenuItem highlightersMenu)
            : this(editor)
        {
            this._highlightersMenu = highlightersMenu;

            //Need to register the Event Handlers for the Menu Items in the Highlighters Menu
            foreach (MenuItem item in this._highlightersMenu.Items.OfType<MenuItem>())
            {
                item.Click += new RoutedEventHandler(HighlighterClick);
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
                if (!this._enableHighlighting) this.SetNoHighlighting();
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
        
        public void AutoDetectSyntaxHighlighter()
        {
            this.AutoDetectSyntaxHighlighter(this._currFile);
        }

        public void AutoDetectSyntaxHighlighter(String filename)
        {
            if (this._editor == null) return; //Not yet ready

            if (!this._enableHighlighting) return;
            if (filename == null)
            {
                this.SetNoHighlighting();
                return;
            }

            try
            {
                IHighlightingDefinition def = HighlightingManager.Instance.GetDefinitionByExtension(System.IO.Path.GetExtension(filename));
                this._editor.SyntaxHighlighting = def;

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
            this._editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition(name);
            String syntax = (this._editor.SyntaxHighlighting == null) ? "None" : name;
            this.SetCurrentHighlighterChecked(syntax);
            this.SetCurrentValidator(syntax);
            this.SetCurrentAutoCompleter(syntax);
        }

        public void SetHighlighter(IHighlightingDefinition def)
        {
            this._editor.SyntaxHighlighting = def;
            String syntax = (this._editor.SyntaxHighlighting == null) ? "None" : def.Name;
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
            if (this._highlightersMenu != null)
            {
                foreach (MenuItem item in this._highlightersMenu.Items.OfType<MenuItem>())
                {
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
                this._validatorStatus.Content = "No Validator is available for currently selected Syntax";
            }
            else
            {
                String message;
                bool validate = this._currValidator.Validate(this._editor.Text, out message);
                this._validatorStatus.Content = message;
                ToolTip tip = new ToolTip();
                TextBlock block = new TextBlock();
                block.TextWrapping = TextWrapping.Wrap;
                block.Width = 800;
                block.Text = message;
                tip.Content = block;
                this._validatorStatus.ToolTip = tip;
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
