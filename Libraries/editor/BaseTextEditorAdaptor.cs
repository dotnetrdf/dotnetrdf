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
using System.Linq;
using System.Text;
using VDS.RDF.Utilities.Editor.AutoComplete;
using VDS.RDF.Utilities.Editor.AutoComplete.Data;
using VDS.RDF.Utilities.Editor.Selection;

namespace VDS.RDF.Utilities.Editor
{
    /// <summary>
    /// Abstract Base class for text editor adaptor implementations
    /// </summary>
    /// <typeparam name="T">Text Editor Control type</typeparam>
    public abstract class BaseTextEditorAdaptor<T>
        : ITextEditorAdaptor<T>
    {
        private T _control;
        private IAutoCompleter<T> _completer;

        /// <summary>
        /// Creates a new adaptor
        /// </summary>
        /// <param name="control">Text Editor control</param>
        public BaseTextEditorAdaptor(T control)
        {
            this._control = control;
        }

        /// <summary>
        /// Gets the text editor control
        /// </summary>
        public T Control
        {
            get 
            { 
                return this._control; 
            }
        }

        public abstract void Apply<TFont, TColor>(VisualOptions<TFont, TColor> options)
            where TFont : class
            where TColor : struct;

        #region State

        /// <summary>
        /// Gets/Sets the Text in the editor
        /// </summary>
        public abstract string Text
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the length of the Text in the editor
        /// </summary>
        public abstract int TextLength
        {
            get;
        }

        /// <summary>
        /// Gets the caret offset
        /// </summary>
        public abstract int CaretOffset
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the current selection start
        /// </summary>
        public abstract int SelectionStart
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the current selection length
        /// </summary>
        public abstract int SelectionLength
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets word wrapping (if supported)
        /// </summary>
        public abstract bool WordWrap
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets whether line numbers are shown (if supported)
        /// </summary>
        public abstract bool ShowLineNumbers
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets whether end of line markers are shown (if supported)
        /// </summary>
        public abstract bool ShowEndOfLine
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets whether space markers are shown (if supported)
        /// </summary>
        public abstract bool ShowSpaces
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets whether tab markers are shown (if supported)
        /// </summary>
        public abstract bool ShowTabs
        {
            get;
            set;
        }

        #endregion

        #region Visual Manuipulation

        /// <summary>
        /// Scrolls the editor to a specific line (if supported)
        /// </summary>
        /// <param name="line">Line</param>
        public abstract void ScrollToLine(int line);

        public virtual void Refresh() { }

        public virtual void BeginUpdate() { }

        public virtual void EndUpdate() { }

        #endregion

        #region Text Manipulation

        /// <summary>
        /// Gets the line number based on the offset
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <returns></returns>
        public abstract int GetLineByOffset(int offset);

        /// <summary>
        /// Gets a section of text starting from a specific offset
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns></returns>
        public virtual String GetText(int offset, int length)
        {
            return this.Text.Substring(offset, length);
        }

        /// <summary>
        /// Gets the character at a specific offset
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <returns></returns>
        public virtual char GetCharAt(int offset)
        {
            return this.Text[offset];
        }

        /// <summary>
        /// Selects a section of text
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        public virtual void Select(int offset, int length)
        {
            this.SelectionStart = offset;
            this.SelectionLength = length;
        }

        /// <summary>
        /// Replaces a portion of text
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <param name="text">Replacement Text</param>
        public virtual void Replace(int offset, int length, String text)
        {
            String currText = this.Text;
            this.Text = currText.Substring(0, offset - 1) + text + currText.Substring(offset + length);
        }

        /// <summary>
        /// Cuts the current selection
        /// </summary>
        public abstract void Cut();

        /// <summary>
        /// Copies the current selection
        /// </summary>
        public abstract void Copy();

        /// <summary>
        /// Pastes the clipboard text
        /// </summary>
        public abstract void Paste();

        /// <summary>
        /// Undo an action
        /// </summary>
        public abstract void Undo();

        /// <summary>
        /// Redo an action
        /// </summary>
        public abstract void Redo();

        #endregion

        #region Highlighting

        public virtual ISymbolSelector<T> SymbolSelector
        {
            get;
            set;
        }

        /// <summary>
        /// Sets the Highlighter to be used (if supported)
        /// </summary>
        /// <param name="name">Syntax Name</param>
        public virtual void SetHighlighter(String name) { }

        /// <summary>
        /// Clears any highlighted errors (if supported)
        /// </summary>
        public virtual void AddErrorHighlight(Exception ex) { }

        /// <summary>
        /// Clears any highlighted errors (if supported)
        /// </summary>
        public virtual void ClearErrorHighlights() { }

        #endregion

        #region Auto-Complete

        /// <summary>
        /// Gets whether auto-completion is supported
        /// </summary>
        public virtual bool CanAutoComplete
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets/Sets the Auto-Completer to be used
        /// </summary>
        public IAutoCompleter<T> AutoCompleter
        {
            get
            {
                return this._completer;
            }
            set
            {
                if (!ReferenceEquals(this._completer, value))
                {
                    if (this._completer != null)
                    {
                        //End Suggestion on the existing completer
                        this.EndSuggestion();
                    }
                    //Attach the New Completer
                    this._completer = value;
                    if (this._completer != null) this.AttachAutoCompleter();
                }
            }
        }

        /// <summary>
        /// Method called when a new auto-completer is specified which should be overridden by implementations to wire up the editor to calling <see cref="IAutoCompleter<T>.TryAutoComplete">TryAutoComplete</see> where appropriate
        /// </summary>
        protected virtual void AttachAutoCompleter() { }

        /// <summary>
        /// Tells the editor to display auto-complete suggestions (if supported)
        /// </summary>
        /// <param name="suggestions">Suggestions</param>
        public virtual void Suggest(IEnumerable<ICompletionData> suggestions) { }

        /// <summary>
        /// Tells the editor to complete using whatever suggestion is currently selected (if supported)
        /// </summary>
        public virtual void Complete() { }

        /// <summary>
        /// Tells the editor that the auto-complete suggestions should no longer be shown (if supported)
        /// </summary>
        public virtual void EndSuggestion() { }

        #endregion

        #region Events

        private void RaiseEvent(TextEditorEventHandler<T> evt)
        {
            this.RaiseEvent(this, evt);
        }

        private void RaiseEvent(Object sender, TextEditorEventHandler<T> evt)
        {
            if (evt != null)
            {
                evt(sender, new TextEditorEventArgs<T>(this));
            }
        }

        /// <summary>
        /// Helper method that can be used to raise the TextChanged event
        /// </summary>
        /// <param name="sender">Sender for the event</param>
        protected void RaiseTextChanged(Object sender)
        {
            this.RaiseEvent(sender, this.TextChanged);
        }

        protected void RaiseDoubleClick(Object sender)
        {
            this.RaiseEvent(sender, this.DoubleClick);
        }

        /// <summary>
        /// Event which is raised when the text in the editor changes
        /// </summary>
        public event TextEditorEventHandler<T> TextChanged;

        public event TextEditorEventHandler<T> DoubleClick;

        #endregion
    }
}
