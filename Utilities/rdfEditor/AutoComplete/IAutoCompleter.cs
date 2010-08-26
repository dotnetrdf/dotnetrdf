using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace rdfEditor.AutoComplete
{
    public interface IAutoCompleter
    {
        void StartAutoComplete(TextEditor editor, TextCompositionEventArgs e);

        void TryAutoComplete(TextCompositionEventArgs e);

        void EndAutoComplete(TextEditor editor);

        AutoCompleteState State
        {
            get;
            set;
        }

        AutoCompleteState LastCompletion
        {
            get;
            set;
        }
    }
}
