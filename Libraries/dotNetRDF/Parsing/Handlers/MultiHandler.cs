/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// Creates a new Multi Handler
        /// </summary>
        /// <param name="handlers">Inner Handlers for this Handler</param>
        public MultiHandler(IEnumerable<IRdfHandler> handlers)
        {
            if (handlers == null) throw new ArgumentNullException("handlers", "Must be at least 1 Handler for use by the MultiHandler");
            if (!handlers.Any()) throw new ArgumentException("Must be at least 1 Handler for use by the MultiHandler", "handlers");

            _handlers.AddRange(handlers);

            // Check there are no identical handlers in the List
            for (int i = 0; i < _handlers.Count; i++)
            {
                for (int j = i + 1; j < _handlers.Count; j++)
                {
                    if (ReferenceEquals(_handlers[i], _handlers[j])) throw new ArgumentException("All Handlers must be distinct IRdfHandler instances", "handlers");
                }
            }
        }

        /// <summary>
        /// Creates a new Multi Handler with a known Node Factory
        /// </summary>
        /// <param name="handlers">Inner Handlers for this Handler</param>
        /// <param name="factory">Node Factory to use for this Handler</param>
        public MultiHandler(IEnumerable<IRdfHandler> handlers, INodeFactory factory) : this(handlers)
        {
            this.NodeFactory = factory ?? throw new ArgumentNullException("factory");
        }

        /// <summary>
        /// Gets the Inner Handlers used by this Handler
        /// </summary>
        public IEnumerable<IRdfHandler> InnerHandlers
        {
            get
            {
                return _handlers;
            }
        }

        /// <summary>
        /// Starts RDF Handling by starting handling on all inner handlers
        /// </summary>
        protected override void StartRdfInternal()
        {
            _handlers.ForEach(h => h.StartRdf());
        }

        /// <summary>
        /// Ends RDF Handling by ending handling on all inner handlers
        /// </summary>
        /// <param name="ok">Whether parsing completed without error</param>
        protected override void EndRdfInternal(bool ok)
        {
            _handlers.ForEach(h => h.EndRdf(ok));
        }

        /// <summary>
        /// Handles Base URIs by getting all inner handlers to handle the Base URI
        /// </summary>
        /// <param name="baseUri">Base URI</param>
        /// <returns></returns>
        /// <remarks>
        /// Handling ends if any of the Handlers indicates it should stop but all Handlers are given the chance to finish the current handling action first
        /// </remarks>
        protected override bool HandleBaseUriInternal(Uri baseUri)
        {
            List<bool> results = _handlers.Select(h => h.HandleBaseUri(baseUri)).ToList();
            return results.All(x => x);
        }

        /// <summary>
        /// Handles Namespace Declarations by getting all inner handlers to handle it
        /// </summary>
        /// <param name="prefix">Namespace Prefix</param>
        /// <param name="namespaceUri">Namespace URI</param>
        /// <returns></returns>
        /// <remarks>
        /// Handling ends if any of the Handlers indicates it should stop but all Handlers are given the chance to finish the current handling action first
        /// </remarks>
        protected override bool HandleNamespaceInternal(string prefix, Uri namespaceUri)
        {
            List<bool> results = _handlers.Select(h => h.HandleNamespace(prefix, namespaceUri)).ToList();
            return results.All(x => x);
        }

        /// <summary>
        /// Handles Triples by getting all inner handlers to handler it
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        /// <remarks>
        /// Handling ends if any of the Handlers indicates it should stop but all Handlers are given the chance to finish the current handling action first
        /// </remarks>
        protected override bool HandleTripleInternal(Triple t)
        {
            List<bool> results = _handlers.Select(h => h.HandleTriple(t)).ToList();
            return results.All(x => x);
        }

        /// <summary>
        /// Gets whether this Handler accepts all Triples based on whether all inner handlers do so
        /// </summary>
        public override bool AcceptsAll
        {
            get
            {
                return _handlers.All(h => h.AcceptsAll);
            }
        }
    }
}
