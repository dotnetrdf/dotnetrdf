using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rdfEditor.AutoComplete
{
    public class AutoCompleteDefinition
    {
        private String _name;
        private IAutoCompleter _completer;

        public AutoCompleteDefinition(String name, IAutoCompleter autoCompleter)
        {
            this._name = name;
            this._completer = autoCompleter;
        }

        public String Name
        {
            get
            {
                return this._name;
            }
        }

        public IAutoCompleter AutoCompleter
        {
            get
            {
                return this._completer;
            }
        }
    }
}
