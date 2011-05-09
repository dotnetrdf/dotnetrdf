using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// A Handler which passes the RDF to be handled through a sequence of Handlers where Handling is terminated as soon as any Handler returns false
    /// </summary>
    /// <remarks>
    /// <para>
    /// This differs from the <see cref="MultiHandler">MultiHandler</see> in that as soon as any Handler indicates that handling should stop by returning false handling is <strong>immediately</strong> terminated.  All Handlers will always have their StartRdf and EndRdf methods called
    /// </para>
    /// </remarks>
    public class ChainedHandler : BaseRdfHandler, IWrappingRdfHandler
    {
        private List<IRdfHandler> _handlers = new List<IRdfHandler>();

        /// <summary>
        /// Creates a new Chained Handler
        /// </summary>
        /// <param name="handlers">Inner Handlers to use</param>
        public ChainedHandler(IEnumerable<IRdfHandler> handlers)
        {
            if (handlers == null) throw new ArgumentNullException("handlers", "Must be at least 1 Handler for use by the ChainedHandler");
            if (!handlers.Any()) throw new ArgumentException("Must be at least 1 Handler for use by the ChainedHandler", "handlers");

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

        /// <summary>
        /// Gets the Inner Handlers used by this Handler
        /// </summary>
        public IEnumerable<IRdfHandler> InnerHandlers
        {
            get
            {
                return this._handlers;
            }
        }

        /// <summary>
        /// Starts the Handling of RDF for each inner handler
        /// </summary>
        protected override void StartRdfInternal()
        {
            this._handlers.ForEach(h => h.StartRdf());
        }

        /// <summary>
        /// Ends the Handling of RDF for each inner handler
        /// </summary>
        /// <param name="ok">Whether parsing completed without errors</param>
        protected override void EndRdfInternal(bool ok)
        {
            this._handlers.ForEach(h => h.EndRdf(ok));
        }

        protected override bool HandleBaseUriInternal(Uri baseUri)
        {
            return this._handlers.All(h => h.HandleBaseUri(baseUri));
        }

        protected override bool HandleNamespaceInternal(string prefix, Uri namespaceUri)
        {
            return this._handlers.All(h => h.HandleNamespace(prefix, namespaceUri));
        }

        protected override bool HandleTripleInternal(Triple t)
        {
            return this._handlers.All(h => h.HandleTriple(t));
        }

        /// <summary>
        /// Gets that this Handler accepts all Triples if all inner handlers do so
        /// </summary>
        public override bool AcceptsAll
        {
            get
            {
                return this._handlers.All(h => h.AcceptsAll);
            }
        }
    }
}
