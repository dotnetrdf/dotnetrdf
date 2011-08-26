using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// A RDF Handler which just determines whether any Triples are present terminating parsing as soon as the first triple is received
    /// </summary>
    public class AnyHandler 
        : BaseRdfHandler
    {
        private bool _any = false;

        public AnyHandler()
            : base(new MockNodeFactory()) { }

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
