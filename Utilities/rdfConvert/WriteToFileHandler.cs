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
