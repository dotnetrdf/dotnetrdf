using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Validation;

namespace VDS.RDF.Utilities.Editor
{
    public class Document<T>
    {
        //General State
        private bool _changed = false;
        private bool _enableHighlighting = true;
        private String _filename, _title;
        private String _syntax = "None";
        private ITextEditorAdaptor<T> _editor;
        private Encoding _encoding = Encoding.UTF8;

        //Validation
        private ISyntaxValidator _currValidator;
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
            this._editor.TextChanged += new TextEditorChangedHandler<T>(this.HandleTextChanged);
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
                throw new NotImplementedException();
            }
        }

        public int SelectionLength
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region Syntax

        public void AutoDetectSyntax()
        {

        }

        /// <summary>
        /// Gets/Sets the Current Validator
        /// </summary>
        public ISyntaxValidator CurrentValidator
        {
            get
            {
                return this._currValidator;
            }
            set
            {
                this._currValidator = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Last Validation Error
        /// </summary>
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

        public bool IsHighlightingEnabled
        {
            get
            {
                return this._enableHighlighting;
            }
            set
            {
                this._enableHighlighting = value;
            }
        }

        #endregion

        #region Text Manipulation

        public char GetCharAt(int index)
        {
            throw new NotImplementedException();
        }

        public void Select(int start, int length)
        {
            throw new NotImplementedException();
        }

        public int GetLineByOffset(int offset)
        {
            throw new NotImplementedException();
        }

        public void ScrollToLine(int line)
        {

        }

        public void Cut()
        {
            this._editor.Cut();
        }

        public void Copy()
        {
            this._editor.Copy();
        }

        public void Paste()
        {
            this._editor.Paste();
        }

        public void Undo()
        {
            this._editor.Undo();
        }

        public void Redo()
        {
            this._editor.Redo();
        }

        #endregion

        #region File Actions

        public void Save()
        {
            if (this._filename != null && !this._filename.Equals(String.Empty))
            {
                //TODO: Get the target Encoding from somewhere
                using (StreamWriter writer = new StreamWriter(this._filename, false, this._encoding))
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

        public event DocumentChangedHandler<T> TextChanged;

        public event DocumentChangedHandler<T> Reloaded;

        public event DocumentChangedHandler<T> Opened;

        public event DocumentChangedHandler<T> SyntaxChanged;

        public event DocumentChangedHandler<T> FilenameChanged;

        public event DocumentChangedHandler<T> TitleChanged;

        public event DocumentChangedHandler<T> Saved;

        #endregion
    }
}
