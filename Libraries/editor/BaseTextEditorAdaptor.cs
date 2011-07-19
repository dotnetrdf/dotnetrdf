using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor
{
    public abstract class BaseTextEditorAdaptor<T> : ITextEditorAdaptor<T>
    {
        private T _control;

        public BaseTextEditorAdaptor(T control)
        {
            this._control = control;
        }

        public T Control
        {
            get 
            { 
                return this._control; 
            }
        }

        #region State

        public abstract string Text
        {
            get;
            set;
        }

        public abstract int TextLength
        {
            get;
        }

        public abstract int CaretOffset
        {
            get;
        }

        public abstract int SelectionStart
        {
            get;
            set;
        }

        public abstract int SelectionLength
        {
            get;
            set;
        }

        public abstract bool WordWrap
        {
            get;
            set;
        }

        public abstract bool ShowLineNumbers
        {
            get;
            set;
        }

        public abstract bool ShowEndOfLine
        {
            get;
            set;
        }

        public abstract bool ShowSpaces
        {
            get;
            set;
        }

        public abstract bool ShowTabs
        {
            get;
            set;
        }
        #endregion

        #region Visual Manuipulation

        public abstract void ScrollToLine(int line);

        #endregion

        #region Text Manipulation

        public abstract int GetLineByOffset(int offset);

        public virtual String GetText(int offset, int length)
        {
            return this.Text.Substring(offset, length);
        }

        public virtual char GetCharAt(int offset)
        {
            return this.Text[offset];
        }

        public virtual void Select(int offset, int length)
        {
            this.SelectionStart = offset;
            this.SelectionLength = length;
        }

        public abstract void Cut();

        public abstract void Copy();

        public abstract void Paste();

        public abstract void Undo();

        public abstract void Redo();

        #endregion

        #region Highlighting

        public abstract void SetHighlighter(String name);

        public abstract void AddErrorHighlight(Exception ex);

        public abstract void ClearErrorHighlights();

        #endregion

        #region Events

        private void RaiseEvent(TextEditorChangedHandler<T> evt)
        {
            this.RaiseEvent(this, evt);
        }

        private void RaiseEvent(Object sender, TextEditorChangedHandler<T> evt)
        {
            if (evt != null)
            {
                evt(sender, new TextEditorEventArgs<T>(this));
            }
        }

        protected void RaiseTextChanged(Object sender)
        {
            this.RaiseEvent(sender, this.TextChanged);
        }

        public event TextEditorChangedHandler<T> TextChanged;

        #endregion
    }
}
