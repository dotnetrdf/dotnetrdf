using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace VDS.RDF.Utilities.Editor.AutoComplete
{
    public interface IAutoCompleter : ICloneable
    {
        void Initialise(TextEditor editor);

        void DetectState(TextEditor editor);

        void TryAutoComplete(TextEditor editor, TextCompositionEventArgs e);

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
