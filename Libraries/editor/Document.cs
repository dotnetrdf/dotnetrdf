using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Validation;
using VDS.RDF.Utilities.Editor.AutoComplete;
using VDS.RDF.Utilities.Editor.Syntax;

namespace VDS.RDF.Utilities.Editor
{
    public class Document<T>
    {
        //General State
        private bool _changed = false;
        private bool _enableHighlighting = true, _enableAutoCompletion = true;
        private String _filename, _title;
        private String _syntax = "None";
        private ITextEditorAdaptor<T> _editor;
        private Encoding _encoding = Encoding.UTF8;

        //Validation
        private ISyntaxValidator _validator;
        private Exception _lastError = null;

        internal Document(ITextEditorAdaptor<T> editor)
            : this(editor, null, null) { }

        internal Document(ITextEditorAdaptor<T> editor, String filename)
            : this(editor, filename, Path.GetFileName(filename)) { }

        internal Document(ITextEditorAdaptor<T> editor, String filename, String title)
        {
            if (editor == null) throw new ArgumentNullException("editor");
            this._editor = editor;
            this._filename = filename;
            this._title = title;

            //Subscribe to relevant events on the Editor
            this._editor.TextChanged += new TextEditorEventHandler<T>(this.HandleTextChanged);
            this._editor.DoubleClick += new TextEditorEventHandler<T>(this.HandleDoubleClick);
        }

        #region General State

        public ITextEditorAdaptor<T> TextEditor
        {
            get
            {
                return this._editor;
            }
        }

        public bool HasChanged
        {
            get
            {
                return this._changed;
            }
            private set
            {
                this._changed = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Current Filename of the Document
        /// </summary>
        public String Filename
        {
            get
            {
                return this._filename;
            }
            set
            {
                if (this._filename != value)
                {
                    this._filename = value;
                    this.HasChanged = true;
                    this.RaiseEvent(this.FilenameChanged);
                }
            }
        }

        /// <summary>
        /// Gets/Sets the Title of the Document, if a filename is present that is always returned instead of any set title
        /// </summary>
        public String Title
        {
            get
            {
                if (this._filename != null && !this._filename.Equals(String.Empty))
                {
                    return Path.GetFileName(this._filename);
                }
                else
                {
                    return this._title;
                }
            }
            set
            {
                if (this._title != value)
                {
                    this._title = value;
                    this.RaiseEvent(this.TitleChanged);
                }
            }
        }

        public String Text
        {
            get
            {
                return this._editor.Text;
            }
            set
            {
                this._editor.Text = value;
            }
        }

        public int TextLength
        {
            get
            {
                return this._editor.TextLength;
            }
        }

        public int CaretOffset
        {
            get
            {
                return this._editor.CaretOffset;
            }
        }

        public int SelectionStart
        {
            get
            {
                return this._editor.SelectionStart;
            }
        }

        public int SelectionLength
        {
            get
            {
                return this._editor.SelectionLength;
            }
        }

        #endregion

        #region Syntax Highlighting and Validation

        public String Syntax
        {
            get
            {
                return this._syntax;
            }
            set
            {
                if (this._syntax != value)
                {
                    this._syntax = value;
                    this.SetSyntax(this._syntax);
                }
            }
        }

        public void AutoDetectSyntax()
        {
            if (this._filename != null && !this._filename.Equals(String.Empty))
            {
                try
                {
                    //Try filename based syntax detection
                    MimeTypeDefinition def = MimeTypesHelper.GetDefinitions(MimeTypesHelper.GetMimeTypes(Path.GetExtension(this._filename))).FirstOrDefault();
                    if (def != null)
                    {
                        this.Syntax = def.SyntaxName.GetSyntaxName();
                        return;
                    }
                }
                catch (RdfParserSelectionException)
                {
                    //Ignore and use string based detection instead
                }
            }

            //Otherwise try and use string based detection
            //First take a guess at it being a SPARQL Results format
            String text = this.Text;
            try
            {
                ISparqlResultsReader resultsReader = StringParser.GetResultSetParser(text);
                this.Syntax = resultsReader.GetSyntaxName();
            }
            catch (RdfParserSelectionException)
            {
                //Then see whether it may be a SPARQL query
                if (text.Contains("SELECT") || text.Contains("CONSTRUCT") || text.Contains("DESCRIBE") || text.Contains("ASK"))
                {
                    //Likely a SPARQL Query
                    this.Syntax = "SparqlQuery11";
                }
                else
                {
                    //Then take a guess at it being a RDF format
                    try
                    {
                        IRdfReader rdfReader = StringParser.GetParser(text);
                        this.Syntax = rdfReader.GetSyntaxName();
                    }
                    catch (RdfParserSelectionException)
                    {
                        //Finally take a guess at it being a RDF Dataset format
                        IStoreReader datasetReader = StringParser.GetDatasetParser(text);
                        this.Syntax = datasetReader.GetSyntaxName();
                    }
                }
            }
        }

        private void SetSyntax(String syntax)
        {
            if (this._enableHighlighting)
            {
                this._editor.SetHighlighter(syntax);
            }
            this.SyntaxValidator = SyntaxManager.GetValidator(syntax);
            if (this._editor.CanAutoComplete)
            {
                if (this.IsAutoCompleteEnabled)
                {
                    this.TextEditor.AutoCompleter = AutoCompleteManager.GetAutoCompleter<T>(this.Syntax, this.TextEditor);
                }
                else
                {
                    this.TextEditor.AutoCompleter = null;
                }
            }

            this.RaiseEvent(this.SyntaxChanged);
        }

        public ISyntaxValidationResults Validate()
        {
            if (this._validator != null)
            {
                ISyntaxValidationResults results = this._validator.Validate(this.Text);
                this._lastError = results.Error;
                this.RaiseValidated(results);
                return results;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets/Sets the Current Validator
        /// </summary>
        public ISyntaxValidator SyntaxValidator
        {
            get
            {
                return this._validator;
            }
            set
            {
                if (!ReferenceEquals(this._validator, value))
                {
                    this._validator = value;
                    this.RaiseEvent(this.ValidatorChanged);
                }
            }
        }

        /// <summary>
        /// Gets the Last Validation Error
        /// </summary>
        public Exception LastValidationError
        {
            get
            {
                return this._lastError;
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
                if (value != this._enableHighlighting)
                {
                    this._enableHighlighting = value;
                    if (value)
                    {
                        this.TextEditor.SetHighlighter(this.Syntax);
                    }
                    else
                    {
                        this.TextEditor.SetHighlighter(null);
                    }
                }
            }
        }

        public bool IsAutoCompleteEnabled
        {
            get
            {
                return this._enableAutoCompletion;
            }
            set
            {
                if (value != this._enableAutoCompletion)
                {
                    this._enableAutoCompletion = value;
                    if (value)
                    {
                        this.TextEditor.AutoCompleter = AutoCompleteManager.GetAutoCompleter<T>(this.Syntax, this.TextEditor);
                    }
                    else
                    {
                        this.TextEditor.AutoCompleter = null;
                    }
                }
            }
        }

        #endregion

        #region File Actions

        private Encoding GetEncoding()
        {
            if (this._encoding.Equals(Encoding.UTF8))
            {
                return new UTF8Encoding(GlobalOptions.UseBomForUtf8);
            }
            else
            {
                return this._encoding;
            }
        }

        public void Save()
        {
            if (this._filename != null && !this._filename.Equals(String.Empty))
            {
                //TODO: Get the target Encoding from somewhere
                using (StreamWriter writer = new StreamWriter(this._filename, false, this.GetEncoding()))
                {
                    writer.Write(this.Text);
                    writer.Close();
                }
                this.RaiseEvent(this.Saved);
                this.HasChanged = false;
            }
        }

        public void SaveAs(String filename)
        {
            if (filename == null) throw new ArgumentNullException("filename");
            if (filename.Equals(String.Empty)) throw new ArgumentException("filename", "Filename cannot be empty");
            this._filename = filename;
            this.RaiseEvent(this.FilenameChanged);
            this.Save();
        }

        public void Open(String filename)
        {
            this.Filename = filename;
            using (StreamReader reader = new StreamReader(this._filename))
            {
                this._encoding = reader.CurrentEncoding;
                this.Text = reader.ReadToEnd();
                reader.Close();
            }
            this.AutoDetectSyntax();
            this.HasChanged = false;
            this.RaiseEvent(this.Opened);
        }

        public void Reload()
        {

        }

        #endregion

        #region Text Editor Events

        private void HandleTextChanged(Object sender, TextEditorEventArgs<T> args)
        {
            this.HasChanged = true;
            this.RaiseEvent(sender, this.TextChanged);
        }

        private void HandleDoubleClick(Object sender, TextEditorEventArgs<T> args)
        {
            if (this._editor.SymbolSelector != null)
            {
                this._editor.SymbolSelector.SelectSymbol(this);
            }
        }

        #endregion

        #region Document Events

        private void RaiseEvent(DocumentChangedHandler<T> evt)
        {
            this.RaiseEvent(this, evt);
        }

        private void RaiseEvent(Object sender, DocumentChangedHandler<T> evt)
        {
            if (evt != null)
            {
                evt(sender, new DocumentChangedEventArgs<T>(this));
            }
        }

        private void RaiseValidated(ISyntaxValidationResults results)
        {
            DocumentValidatedHandler<T> d = this.Validated;
            if (d != null)
            {
                d(this, new DocumentValidatedEventArgs<T>(this, results));
            }
        }

        public event DocumentChangedHandler<T> TextChanged;

        public event DocumentChangedHandler<T> Reloaded;

        public event DocumentChangedHandler<T> Opened;

        public event DocumentChangedHandler<T> SyntaxChanged;

        public event DocumentChangedHandler<T> FilenameChanged;

        public event DocumentChangedHandler<T> TitleChanged;

        public event DocumentChangedHandler<T> Saved;

        public event DocumentChangedHandler<T> ValidatorChanged;

        public event DocumentValidatedHandler<T> Validated;

        #endregion
    }
}
