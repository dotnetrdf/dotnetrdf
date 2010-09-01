using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rdfEditor.AutoComplete
{
    public class NamespaceTerm
    {
        private String _namespace, _term, _label = String.Empty;

        public NamespaceTerm(String namespaceUri, String term)
        {
            this._namespace = namespaceUri;
            this._term = term;
        }

        public NamespaceTerm(String namespaceUri, String term, String label)
            : this(namespaceUri, term)
        {
            this._label = label;
        }

        public String NamespaceUri
        {
            get
            {
                return this._namespace;
            }
        }

        public String Term
        {
            get
            {
                return this._term;
            }
        }

        public String Label
        {
            get
            {
                return this._label;
            }
            set
            {
                this._label = value;
            }
        }

        public override string ToString()
        {
            return this._namespace + this._term;
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is NamespaceTerm)
            {
                NamespaceTerm other = (NamespaceTerm)obj;
                return this.ToString().Equals(other.ToString(), StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                return false;
            }
        }
    }
}
