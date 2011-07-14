using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Validation;

namespace VDS.RDF.Utilities.Editor
{
    public class Document
    {
        //General State
        private bool _changed = false;
        private bool _enableHighlighting = true;
        private String _currFile;
        private String _currSyntax = "None";
        private ITextEditorAdaptor _editor;

        //Validation
        private ISyntaxValidator _currValidator;
        private Exception _lastError = null;

        public Document(ITextEditorAdaptor editor)
            : this(editor, null) { }

        public Document(ITextEditorAdaptor editor, String filename)
        {
            if (editor == null) throw new ArgumentNullException("editor");
            this._editor = editor;

            //TODO: Syntax auto-detection here
        }

        public Document(Document doc)
            : this(doc.CloneEditor(), null)
        {
            //TODO: Add any other copy actions that are required
        }

        #region General State

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

        public ITextEditorAdaptor CloneEditor()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Events

        public event DocumentChangedHandler TextChanged;

        public event DocumentChangedHandler Reloaded;

        #endregion
    }
}
