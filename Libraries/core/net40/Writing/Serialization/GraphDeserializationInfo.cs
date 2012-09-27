/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

#if !SILVERLIGHT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace VDS.RDF.Writing.Serialization
{
    class GraphDeserializationInfo
    {
        private List<Triple> _triples;
        private List<KeyValuePair<String, String>> _namespaces;
        private Uri _baseUri;

        public GraphDeserializationInfo(SerializationInfo info, StreamingContext context)
        {
            this._triples = (List<Triple>)info.GetValue("triples", typeof(List<Triple>));
            this._namespaces = (List<KeyValuePair<String, String>>)info.GetValue("namespaces", typeof(List<KeyValuePair<String, String>>));
            String baseUri = info.GetString("base");
            if (!baseUri.Equals(String.Empty))
            {
                this._baseUri = UriFactory.Create(baseUri);
            }
        }

        public void Apply(IGraph g)
        {
            g.BaseUri = this._baseUri;
            g.Assert(this._triples);
            foreach (KeyValuePair<String, String> ns in this._namespaces)
            {
                g.NamespaceMap.AddNamespace(ns.Key, UriFactory.Create(ns.Value));
            }
        }
    }
}


#endif