using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.AvalonEdit;

namespace rdfEditor.Selection
{
    /// <summary>
    /// Selector which selects the symbol deliminated by a starting &lt; " or new line and by an ending &gt; " or new line.  If there is already a selection it selects the surrounding symbol
    /// </summary>
    public class BaseSelector
    {
        private bool _includeDelim = false;

        /// <summary>
        /// Selects a Symbol around the current selection (if any) or caret position
        /// </summary>
        /// <param name="editor">Text Editor</param>
        public void SelectSymbol(TextEditor editor)
        {
            int selStart, selLength;

            if (editor.SelectionStart >= 0 && editor.SelectionLength > 0)
            {
                selStart = editor.SelectionStart;
                selLength = editor.SelectionLength;
            }
            else
            {
                selStart = editor.CaretOffset;
                selLength = 0;
            }

            //If there is an existing Selection and deliminators are not included
            //check whether the preceding and following characters are deiliminators and if so
            //alter the selection start and length appropriately to include these otherwise our
            //select won't select the surrounding symbol properly
            if (selStart > 0 && selLength > 0 && !this._includeDelim)
            {
                if (this.IsStartingDeliminator(editor.Document.GetCharAt(selStart-1)))
                {
                    selStart--;
                    selLength++;
                }
                if (this.IsEndingDeliminator(editor.Document.GetCharAt(selStart + selLength))) selLength++;
            }

            //Extend the selection backwards
            while (selStart > 0)
            {
                selStart--;
                selLength++;

                char current = editor.Document.GetCharAt(selStart);
                if (this.IsStartingDeliminator(current)) break;
            }
            if (!this._includeDelim)
            {
                selStart++;
                selLength--;
            }

            //Extend the selection forwards
            while (selStart + selLength < editor.Document.TextLength)
            {
                selLength++;

                char current = editor.Document.GetCharAt(selStart + selLength);
                if (this.IsEndingDeliminator(current)) break;
            }
            if (this._includeDelim)
            {
                selLength++;
            }

            //Select the Symbol Text
            editor.Select(selStart, selLength);
            editor.ScrollToLine(editor.Document.GetLineByOffset(selStart).LineNumber);
        }

        /// <summary>
        /// Gets/Sets whether Selection should include the Deliminator Character
        /// </summary>
        public bool IncludeDeliminator
        {
            get
            {
                return this._includeDelim;
            }
            set
            {
                this._includeDelim = value;
            }
        }

        /// <summary>
        /// Gets whether a Character is a Starting Deliminator
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns></returns>
        protected virtual bool IsStartingDeliminator(char c)
        {
            switch (c)
            {
                case '"':
                case '<':
                case '\n':
                case '\r':
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Gets whether a Character is an Ending Deliminator
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns></returns>
        protected virtual bool IsEndingDeliminator(char c)
        {
            switch (c)
            {
                case '"':
                case '>':
                case '\n':
                case '\r':
                    return true;
                default:
                    return false;
            }
        }
    }
}
