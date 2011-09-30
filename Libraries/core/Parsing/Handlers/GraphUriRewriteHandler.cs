using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Parsing.Handlers
{
    public class GraphUriRewriteHandler
        : BaseRdfHandler, IWrappingRdfHandler
    {
        private IRdfHandler _handler;
        private Uri _graphUri;

        public GraphUriRewriteHandler(IRdfHandler handler, Uri graphUri)
            : base()
        {
            this._handler = handler;
            this._graphUri = graphUri;
        }

        public IEnumerable<IRdfHandler> InnerHandlers
        {
            get
            {
                return this._handler.AsEnumerable();
            }
        }

        protected override void StartRdfInternal()
        {
            base.StartRdfInternal();
            this._handler.StartRdf();
        }

        protected override void EndRdfInternal(bool ok)
        {
            this._handler.EndRdf(ok);
            base.EndRdfInternal(ok);
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
            t = new Triple(t.Subject, t.Predicate, t.Object, this._graphUri);
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
