/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

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
    /// <summary>
    /// Represents a document in the editor
    /// </summary>
    /// <typeparam name="T">Control Type</typeparam>
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

        /// <summary>
        /// Creates a document
        /// </summary>
        /// <param name="editor">Text Editor</param>
        internal Document(ITextEditorAdaptor<T> editor)
            : this(editor, null, null) { }

        /// <summary>
        /// Creates a document
        /// </summary>
        /// <param name="editor">Text Editor</param>
        /// <param name="filename">Filename</param>
        internal Document(ITextEditorAdaptor<T> editor, String filename)
            : this(editor, filename, Path.GetFileName(filename)) { }

        /// <summary>
        /// Creates a document
        /// </summary>
        /// <param name="editor">Text Editor</param>
        /// <param name="filename">Filename</param>
        /// <param name="title">Title</param>
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

        /// <summary>
        /// Gets the text editor for the document
        /// </summary>
        public ITextEditorAdaptor<T> TextEditor
        {
            get
            {
                return this._editor;
            }
        }

        /// <summary>
        /// Gets/Sets whether the document has changed
        /// </summary>
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

        /// <summary>
        /// Gets/Sets the text of the document
        /// </summary>
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

        /// <summary>
        /// Gets the length of the document
        /// </summary>
        public int TextLength
        {
            get
            {
                return this._editor.TextLength;
            }
        }

        /// <summary>
        /// Gets the current caret position in the document
        /// </summary>
        public int CaretOffset
        {
            get
            {
                return this._editor.CaretOffset;
            }
        }

        /// <summary>
        /// Gets the current selection start
        /// </summary>
        public int SelectionStart
        {
            get
            {
                return this._editor.SelectionStart;
            }
        }

        /// <summary>
        /// Gets the current selection length
        /// </summary>
        public int SelectionLength
        {
            get
            {
                return this._editor.SelectionLength;
            }
        }

        #endregion

        #region Syntax Highlighting and Validation

        /// <summary>
        /// Gets/Sets the syntax for the document
        /// </summary>
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

        /// <summary>
        /// Requests that the document auto-detect its syntax
        /// </summary>
        public void AutoDetectSyntax()
        {
            if (this._filename != null && !this._filename.Equals(String.Empty))
            {
                try
                {
                    //Try filename based syntax detection
                    MimeTypeDefinition def = MimeTypesHelper.GetDefinitionsByFileExtension(MimeTypesHelper.GetTrueFileExtension(this._filename)).FirstOrDefault();
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

        /// <summary>
        /// Sets the syntax configuring the associated text editor as appropriate
        /// </summary>
        /// <param name="syntax">Syntax</param>
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

        /// <summary>
        /// Validates the document
        /// </summary>
        /// <returns>Syntax Validation Results if available, null otherwise</returns>
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

        /// <summary>
        /// Gets/Sets whether highlighting is enabled
        /// </summary>
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

        /// <summary>
        /// Gets/Sets whether auto-completion is enabled
        /// </summary>
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

        /// <summary>
        /// Gets the encoding in which the document should be saved
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Saves the document assuming it has a file associated with it
        /// </summary>
        public void Save()
        {
            if (this._filename != null && !this._filename.Equals(String.Empty))
            {
                using (StreamWriter writer = new StreamWriter(this._filename, false, this.GetEncoding()))
                {
                    writer.Write(this.Text);
                    writer.Close();
                }
                this.RaiseEvent(this.Saved);
                this.HasChanged = false;
            }
        }

        /// <summary>
        /// Saves the document with the given filename
        /// </summary>
        /// <param name="filename">Filename</param>
        public void SaveAs(String filename)
        {
            if (filename == null) throw new ArgumentNullException("filename");
            if (filename.Equals(String.Empty)) throw new ArgumentException("filename", "Filename cannot be empty");
            this._filename = filename;
            this.RaiseEvent(this.FilenameChanged);
            this.Save();
        }

        /// <summary>
        /// Opens the document from a file
        /// </summary>
        /// <param name="filename">Filename</param>
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

        /// <summary>
        /// Reloads the document
        /// </summary>
        public void Reload()
        {

        }

        #endregion

        #region Text Editor Events

        /// <summary>
        /// Handler for the TextChanged event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        private void HandleTextChanged(Object sender, TextEditorEventArgs<T> args)
        {
            this.HasChanged = true;
            this.RaiseEvent(sender, this.TextChanged);
        }

        /// <summary>
        /// Handler for the DoubleClick event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
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

        /// <summary>
        /// Event which is raised when the document text changes
        /// </summary>
        public event DocumentChangedHandler<T> TextChanged;

        /// <summary>
        /// Event which is raised when the document is reloaded
        /// </summary>
        public event DocumentChangedHandler<T> Reloaded;

        /// <summary>
        /// Event which is raised when the document is opened
        /// </summary>
        public event DocumentChangedHandler<T> Opened;

        /// <summary>
        /// Event which is raised when the syntax for the document is changed
        /// </summary>
        public event DocumentChangedHandler<T> SyntaxChanged;

        /// <summary>
        /// Event which is raised when the filename for the document is changed
        /// </summary>
        public event DocumentChangedHandler<T> FilenameChanged;

        /// <summary>
        /// Event which is raised when the title of the document is changed
        /// </summary>
        public event DocumentChangedHandler<T> TitleChanged;

        /// <summary>
        /// Event which is raised when the document is saved
        /// </summary>
        public event DocumentChangedHandler<T> Saved;

        /// <summary>
        /// Event which is raised when the syntax validator for the document is changed
        /// </summary>
        public event DocumentChangedHandler<T> ValidatorChanged;

        /// <summary>
        /// Event which is raised when the document is validated
        /// </summary>
        public event DocumentValidatedHandler<T> Validated;

        #endregion
    }
}
