using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor.AutoComplete
{
    public interface IAutoCompleter<T>
    {
        void DetectState();

        void TryAutoComplete(String newText);

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
