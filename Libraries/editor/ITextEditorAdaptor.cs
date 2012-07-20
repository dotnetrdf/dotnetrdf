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
    /// Adaptor Interface which links the editor core to an actual text editor control
    /// </summary>
    /// <typeparam name="T">Text Editor Control Type</typeparam>
    public interface ITextEditorAdaptor<T>
    {
        /// <summary>
        /// Gets the Text Editor Control
        /// </summary>
        T Control
        {
            get;
        }

        void Apply<TFont, TColor>(VisualOptions<TFont, TColor> options)
            where TFont : class
            where TColor : struct;

        /// <summary>
        /// Gets/Sets the Text in the editor
        /// </summary>
        String Text
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the length of the Text in the editor
        /// </summary>
        int TextLength
        {
            get;
        }

        /// <summary>
        /// Gets/Sets the caret offset
        /// </summary>
        int CaretOffset
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the current selection start
        /// </summary>
        int SelectionStart
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the current selection length
        /// </summary>
        int SelectionLength
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets word wrapping (if supported)
        /// </summary>
        bool WordWrap
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets whether line numbers are shown (if supported)
        /// </summary>
        bool ShowLineNumbers
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets whether end of line markers are shown (if supported)
        /// </summary>
        bool ShowEndOfLine
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets whether space markers are shown (if supported)
        /// </summary>
        bool ShowSpaces
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets whether tab markers are shown (if supported)
        /// </summary>
        bool ShowTabs
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the character at a specific offset
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <returns></returns>
        char GetCharAt(int offset);

        /// <summary>
        /// Gets a section of text starting from a specific offset
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns></returns>
        String GetText(int offset, int length);

        /// <summary>
        /// Selects a section of text
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        void Select(int offset, int length);

        void Replace(int offset, int length, String text);

        /// <summary>
        /// Gets the line number based on the offset
        /// </summary>
        /// <param name="offset">Offset</param>
        /// <returns></returns>
        int GetLineByOffset(int offset);

        /// <summary>
        /// Scrolls the editor to a specific line (if supported)
        /// </summary>
        /// <param name="line">Line</param>
        void ScrollToLine(int line);

        void BeginUpdate();

        void EndUpdate();

        /// <summary>
        /// Indicates that the Text Editor should refresh its visual display
        /// </summary>
        void Refresh();

        /// <summary>
        /// Cuts the current selection
        /// </summary>
        void Cut();

        /// <summary>
        /// Copies the current selection
        /// </summary>
        void Copy();

        /// <summary>
        /// Pastes the clipboard text
        /// </summary>
        void Paste();

        /// <summary>
        /// Undo an action
        /// </summary>
        void Undo();

        /// <summary>
        /// Redo an action
        /// </summary>
        void Redo();

        ISymbolSelector<T> SymbolSelector
        {
            get;
            set;
        }

        /// <summary>
        /// Sets the Highlighter to be used (if supported)
        /// </summary>
        /// <param name="name">Syntax Name</param>
        void SetHighlighter(String name);

        /// <summary>
        /// Clears any highlighted errors (if supported)
        /// </summary>
        void ClearErrorHighlights();

        /// <summary>
        /// Highlights an error (if supported)
        /// </summary>
        /// <param name="ex">Error</param>
        void AddErrorHighlight(Exception ex);

        /// <summary>
        /// Gets whether auto-completion is supported
        /// </summary>
        bool CanAutoComplete
        {
            get;
        }

        /// <summary>
        /// Gets/Sets the Auto-Completer to be used
        /// </summary>
        IAutoCompleter<T> AutoCompleter
        {
            get;
            set;
        }

        /// <summary>
        /// Tells the editor to display auto-complete suggestions (if supported)
        /// </summary>
        /// <param name="suggestions">Suggestions</param>
        void Suggest(IEnumerable<ICompletionData> suggestions);

        /// <summary>
        /// Tells the editor to complete using whatever suggestion is currently selected (if supported)
        /// </summary>
        void Complete();

        /// <summary>
        /// Tells the editor that the auto-complete suggestions should no longer be shown (if supported)
        /// </summary>
        void EndSuggestion();

        /// <summary>
        /// Event which is raised when the text in the editor changes
        /// </summary>
        event TextEditorEventHandler<T> TextChanged;

        event TextEditorEventHandler<T> DoubleClick;
    }

}
