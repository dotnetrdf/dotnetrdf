using System;
using System.Collections.Generic;
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
        private String _currFile;
        private String _currSyntax = "None";
        private ITextEditorAdaptor<T> _editor;

        //Validation
        private ISyntaxValidator _currValidator;
        private Exception _lastError = null;

        public Document(ITextEditorAdaptorFactory<T> factory)
            : this(factory.CreateAdaptor()) { }

        public Document(ITextEditorAdaptorFactory<T> factory, String filename)
            : this(factory.CreateAdaptor(), filename) { }

        public Document(ITextEditorAdaptor<T> editor)
            : this(editor, null) { }

        public Document(ITextEditorAdaptor<T> editor, String filename)
        {
            if (editor == null) throw new ArgumentNullException("editor");
            this._editor = editor;

            //TODO: Syntax auto-detection here
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
        }

        /// <summary>
        /// Gets/Sets the Current Filename of the Document
        /// </summary>
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

        public String Text
        {
            get
            {
                return this._editor.Text;
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

        #region Validation

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

        #endregion

        #region Highlighting

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

        public void SetHighlighter(String name)
        {
            this._editor.SetHighlighter(name);
        }

        #endregion

        #region Actions

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

        public void Save()
        {

        }

        public void SaveAs(String filename)
        {

        }

        public void Reload()
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

        #region Events

        public event DocumentChangedHandler<T> TextChanged;

        public event DocumentChangedHandler<T> Reloaded;

        #endregion
    }
}
