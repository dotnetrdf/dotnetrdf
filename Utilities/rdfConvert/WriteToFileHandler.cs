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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Utilities.Convert
{
    class WriteToFileHandler : BaseRdfHandler
    {
        private String _file;
        private Encoding _encoding;
        private Type _formatterType;
        private WriteThroughHandler _handler;

        public WriteToFileHandler(String file, Encoding enc, Type formatterType)
        {
            this._file = file;
            this._encoding = enc;
            this._formatterType = formatterType;
        }

        protected override void StartRdfInternal()
        {
            this._handler = new WriteThroughHandler(this._formatterType, new StreamWriter(this._file, false, this._encoding));
            this._handler.StartRdf();
        }

        protected override void EndRdfInternal(bool ok)
        {
            this._handler.EndRdf(ok);
        }

        protected override bool HandleBaseUriInternal(Uri baseUri)
        {
            return this._handler.HandleBaseUri(baseUri);
        }

        protected override bool HandleNamespaceInternal(string prefix, Uri namespaceUri)
        {
            return this._handler.HandleNamespace(prefix, namespaceUri);
        }

        protected override bool HandleTripleInternal(Triple t)
        {
            return this._handler.HandleTriple(t);
        }

        public override bool AcceptsAll
        {
            get 
            {
                return true; 
            }
        }
    }
}
