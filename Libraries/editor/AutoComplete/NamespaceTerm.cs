/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor.AutoComplete
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
