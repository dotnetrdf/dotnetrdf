using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor.AutoComplete.Vocabularies
{
    public class VocabularyDefinition
    {
        private String _prefix, _uri, _descrip;

        public VocabularyDefinition(String prefix, String uri, String description)
        {
            this._prefix = prefix;
            this._uri = uri;
            this._descrip = description;
        }

        public String Prefix
        {
            get
            {
                return this._prefix;
            }
        }

        public String NamespaceUri
        {
            get
            {
                return this._uri;
            }
        }

        public String Description
        {
            get
            {
                return this._descrip;
            }
        }
    }
}
