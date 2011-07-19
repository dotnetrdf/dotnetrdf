using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using ICSharpCode.AvalonEdit;

namespace VDS.RDF.Utilities.Editor.Selection
{
    /// <summary>
    /// Selector which selects the symbol deliminated by a starting &lt; " or new line and by an ending &gt; " or new line.  If there is already a selection it selects the surrounding symbol
    /// </summary>
    public abstract class BaseSelector<T> : ISymbolSelector<T>
    {
        private bool _includeDelim = false;

        /// <summary>
        /// Selects a Symbol around the current selection (if any) or caret position
        /// </summary>
        public void SelectSymbol(Document<T> doc)
        {
            int selStart, selLength;

            if (doc.SelectionStart >= 0 && doc.SelectionLength > 0)
            {
                selStart = doc.SelectionStart;
                selLength = doc.SelectionLength;
            }
            else
            {
                selStart = doc.CaretOffset;
                selLength = 0;
            }

            //If there is an existing Selection and deliminators are not included
            //check whether the preceding and following characters are deiliminators and if so
            //alter the selection start and length appropriately to include these otherwise our
            //select won't select the surrounding symbol properly
            if (selStart > 0 && selLength > 0 && !this._includeDelim)
            {
                if (this.IsStartingDeliminator(doc.TextEditor.GetCharAt(selStart-1)))
                {
                    selStart--;
                    selLength++;
                }
                if (selStart + selLength < doc.TextLength - 1)
                {
                    if (this.IsEndingDeliminator(doc.TextEditor.GetCharAt(selStart + selLength))) selLength++;
                }
            }

            char? endDelim = null;

            //Extend the selection backwards
            while (selStart >= 0)
            {
                selStart--;
                selLength++;

                //Start of Document is always a Boundary
                if (selStart == 0) break;

                //Otherwise check if character at start of selection is a boundary
                char current = doc.TextEditor.GetCharAt(selStart);
                if (this.IsStartingDeliminator(current))
                {
                    endDelim = this.RequireMatchingDeliminator(current);
                    break;
                }
            }
            if (!this._includeDelim)
            {
                if (selStart > 0 || this.IsStartingDeliminator(doc.TextEditor.GetCharAt(selStart)))
                {
                    selStart++;
                    selLength--;
                }
            }

            //Extend the selection forwards
            while (selStart + selLength < doc.TextLength)
            {
                selLength++;

                //End of Document is always a Boundary
                if (selStart + selLength == doc.TextLength) break;

                //Otherwise check if character after end of selection is a boundary
                char current = doc.TextEditor.GetCharAt(selStart + selLength);
                if (endDelim != null )
                {
                    //If a matching End Deliminator is required then stop when that is reached
                    if (endDelim == current) break;
                }
                else if (this.IsEndingDeliminator(current))
                {
                    //Otherwise stop when any End Deliminator is found
                    break;
                }
            }
            if (this._includeDelim)
            {
                selLength++;
            }

            //Select the Symbol Text
            doc.TextEditor.Select(selStart, selLength);
            doc.TextEditor.ScrollToLine(doc.TextEditor.GetLineByOffset(selStart));
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
        /// Gets whether a specific Starting Deliminator should be matched with a specific ending deliminator
        /// </summary>
        /// <param name="c">Starting Deliminator</param>
        /// <returns></returns>
        protected abstract char? RequireMatchingDeliminator(char c);

        /// <summary>
        /// Gets whether the Character is a Starting Deliminator
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns></returns>
        protected abstract bool IsStartingDeliminator(char c);

        /// <summary>
        /// Gets whether the Character is an Ending Deliminator
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns></returns>
        protected abstract bool IsEndingDeliminator(char c);
    }
}
