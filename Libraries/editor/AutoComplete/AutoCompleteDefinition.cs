using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor.AutoComplete
{
    public class AutoCompleteDefinition
    {
        private String _name;
        private Type _type;

        public AutoCompleteDefinition(String name, Type autoCompleteType)
        {
            this._name = name;
            this._type = autoCompleteType;
        }

        public String Name
        {
            get
            {
                return this._name;
            }
        }

        public Type Type
        {
            get
            {
                return this._type;
            }
        }
    }
}
