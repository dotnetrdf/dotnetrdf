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

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// A RDF Handler which wraps another Handler allowing handling to be cancelled
    /// </summary>
    public class CancellableHandler : BaseRdfHandler, IWrappingRdfHandler
    {
        private IRdfHandler _handler;
        private bool _cancelled = false;

        /// <summary>
        /// Creates a new Cancellable Handler
        /// </summary>
        /// <param name="handler"></param>
        public CancellableHandler(IRdfHandler handler)
        {
            if (handler == null) throw new ArgumentNullException("handler", "Inner Handler cannot be null");
            _handler = handler;
        }

        /// <summary>
        /// Gets the Inner Handler wrapped by this Handler
        /// </summary>
        public IEnumerable<IRdfHandler> InnerHandlers
        {
            get
            {
                return _handler.AsEnumerable();
            }
        }

        /// <summary>
        /// Starts RDF Handling on the inner Handler
        /// </summary>
        protected override void StartRdfInternal()
        {
            // Note - We don't reset the cancelled flag here as it is possible that in an async environment that
            // Cancel() may get called before handling properly starts at which case we still need to cancel.
            // The cancelled flag only resets when handling ends
            _handler.StartRdf();
        }

        /// <summary>
        /// Ends RDF Handling on the inner Handler
        /// </summary>
        /// <param name="ok">Indicates whether parsing completed without error</param>
        protected override void EndRdfInternal(bool ok)
        {
            _cancelled = false;
            _handler.EndRdf(ok);
        }

        /// <summary>
        /// Handles Base URIs by passing them to the inner handler and cancelling handling if it has been requested
        /// </summary>
        /// <param name="baseUri">Base URI</param>
        /// <returns></returns>
        protected override bool HandleBaseUriInternal(Uri baseUri)
        {
            if (_cancelled) return false;
            return _handler.HandleBaseUri(baseUri);
        }

        /// <summary>
        /// Handles Namespace Declarations by passing them to the inner handler and cancelling handling if it has been requested
        /// </summary>
        /// <param name="prefix">Namespace Prefix</param>
        /// <param name="namespaceUri">Namespace URI</param>
        /// <returns></returns>
        protected override bool HandleNamespaceInternal(string prefix, Uri namespaceUri)
        {
            if (_cancelled) return false;
            return _handler.HandleNamespace(prefix, namespaceUri);
        }

        /// <summary>
        /// Handles Triples by passing them to the inner handler and cancelling handling if it has been requested
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        protected override bool HandleTripleInternal(Triple t)
        {
            if (_cancelled) return false;
            return _handler.HandleTriple(t);
        }

        /// <summary>
        /// Gets that this Handler does not accept all Triples
        /// </summary>
        public override bool AcceptsAll
        {
            get 
            {
                return false;
            }
        }

        /// <summary>
        /// Informs the Handler that it should cancel handling at the next point possible assuming handling has not already completed
        /// </summary>
        public void Cancel()
        {
            _cancelled = true;
        }
    }
}
