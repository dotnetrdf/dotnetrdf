using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;

namespace rdfEditor.AutoComplete
{
    public abstract class BaseAutoCompleter : IAutoCompleter
    {
        private AutoCompleteState _state = AutoCompleteState.None;
        private AutoCompleteState _lastCompletion = AutoCompleteState.None;

        public abstract void Initialise(TextEditor editor);

        public abstract void DetectState(TextEditor editor);

        public abstract void StartAutoComplete(TextEditor editor, TextCompositionEventArgs e);

        public abstract void TryAutoComplete(TextCompositionEventArgs e);

        public abstract void EndAutoComplete(TextEditor editor);

        public AutoCompleteState State
        {
            get
            {
                return this._state;
            }
            set
            {
                this._state = value;
            }
        }

        public AutoCompleteState LastCompletion
        {
            get
            {
                return this._lastCompletion;
            }
            set
            {
                this._lastCompletion = value;
            }
        }
    }
}
