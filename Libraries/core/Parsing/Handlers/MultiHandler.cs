using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// A Handler which passes the RDF to be handled to multiple Handlers where Handling terminates in the handling request where one of the Handlers returns false
    /// </summary>
    /// <remarks>
    /// <para>
    /// This differs from <see cref="ChainedHandler">ChainedHandler</see> in that even if one Handler indicates that handling should stop by returning false all the Handlers still have a chance to handle the Base URI/Namespace/Triple before handling is terminated.  All Handlers will always have their StartRdf and EndRdf methods called
    /// </para>
    /// </remarks>
    public class MultiHandler : BaseRdfHandler, IWrappingRdfHandler
    {
        private List<IRdfHandler> _handlers = new List<IRdfHandler>();

        public MultiHandler(IEnumerable<IRdfHandler> handlers)
        {
            if (handlers == null) throw new ArgumentNullException("handlers", "Must be at least 1 Handler for use by the MultiHandler");
            if (!handlers.Any()) throw new ArgumentException("Must be at least 1 Handler for use by the MultiHandler", "handlers");

            this._handlers.AddRange(handlers);

            //Check there are no identical handlers in the List
            for (int i = 0; i < this._handlers.Count; i++)
            {
                for (int j = i + 1; j < this._handlers.Count; j++)
                {
                    if (ReferenceEquals(this._handlers[i], this._handlers[j])) throw new ArgumentException("All Handlers must be distinct IRdfHandler instances", "handlers");
                }
            }
        }

        public IEnumerable<IRdfHandler> InnerHandlers
        {
            get
            {
                return this._handlers;
            }
        }

        protected override void StartRdfInternal()
        {
            this._handlers.ForEach(h => h.StartRdf());
        }

        protected override void EndRdfInternal(bool ok)
        {
            this._handlers.ForEach(h => h.EndRdf(ok));
        }

        protected override bool HandleBaseUriInternal(Uri baseUri)
        {
            List<bool> results = this._handlers.Select(h => h.HandleBaseUri(baseUri)).ToList();
            return results.All(x => x);
        }

        protected override bool HandleNamespaceInternal(string prefix, Uri namespaceUri)
        {
            List<bool> results = this._handlers.Select(h => h.HandleNamespace(prefix, namespaceUri)).ToList();
            return results.All(x => x);
        }

        protected override bool HandleTripleInternal(Triple t)
        {
            List<bool> results = this._handlers.Select(h => h.HandleTriple(t)).ToList();
            return results.All(x => x);
        }

        public override bool AcceptsAll
        {
            get 
            {
                return this._handlers.All(h => h.AcceptsAll);
            }
        }
    }
}
