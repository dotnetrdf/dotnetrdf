using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Parsing.Handlers
{
    public class AnyHandler 
        : BaseRdfHandler
    {
        private bool _any = false;

        public bool Any
        {
            get
            {
                return this._any;
            }
        }

        protected override void StartRdfInternal()
        {
            this._any = false;
        }

        protected override bool HandleBaseUriInternal(Uri baseUri)
        {
            return true;
        }

        protected override bool HandleNamespaceInternal(string prefix, Uri namespaceUri)
        {
            return true;
        }

        protected override bool HandleTripleInternal(Triple t)
        {
            this._any = true;
            return false;
        }

        public override bool AcceptsAll
        {
            get 
            {
                return false;
            }
        }
    }
}
