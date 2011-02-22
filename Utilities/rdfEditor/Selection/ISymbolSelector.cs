using System;
using ICSharpCode.AvalonEdit;

namespace rdfEditor.Selection
{
    /// <summary>
    /// Interface for Symbol Selectors
    /// </summary>
    public interface ISymbolSelector
    {
        /// <summary>
        /// Gets/Sets whether the Symbol Selector should include deliminator (i.e. boundary) characters in the selected symbol
        /// </summary>
        bool IncludeDeliminator 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// Selects a Symbol around the current Selection (if any) or Caret Position in the given Text Editor
        /// </summary>
        /// <param name="editor">Text Editor</param>
        void SelectSymbol(TextEditor editor);
    }
}
