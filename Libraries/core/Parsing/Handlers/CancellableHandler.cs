/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

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
using System.Linq;
using System.Text;

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
            this._handler = handler;
        }

        /// <summary>
        /// Gets the Inner Handler wrapped by this Handler
        /// </summary>
        public IEnumerable<IRdfHandler> InnerHandlers
        {
            get
            {
                return this._handler.AsEnumerable();
            }
        }

        /// <summary>
        /// Starts RDF Handling on the inner Handler
        /// </summary>
        protected override void StartRdfInternal()
        {
            //Note - We don't reset the cancelled flag here as it is possible that in an async environment that
            //Cancel() may get called before handling properly starts at which case we still need to cancel.
            //The cancelled flag only resets when handling ends
            this._handler.StartRdf();
        }

        /// <summary>
        /// Ends RDF Handling on the inner Handler
        /// </summary>
        /// <param name="ok">Indicates whether parsing completed without error</param>
        protected override void EndRdfInternal(bool ok)
        {
            this._cancelled = false;
            this._handler.EndRdf(ok);
        }

        /// <summary>
        /// Handles Base URIs by passing them to the inner handler and cancelling handling if it has been requested
        /// </summary>
        /// <param name="baseUri">Base URI</param>
        /// <returns></returns>
        protected override bool HandleBaseUriInternal(Uri baseUri)
        {
            if (this._cancelled) return false;
            return this._handler.HandleBaseUri(baseUri);
        }

        /// <summary>
        /// Handles Namespace Declarations by passing them to the inner handler and cancelling handling if it has been requested
        /// </summary>
        /// <param name="prefix">Namespace Prefix</param>
        /// <param name="namespaceUri">Namespace URI</param>
        /// <returns></returns>
        protected override bool HandleNamespaceInternal(string prefix, Uri namespaceUri)
        {
            if (this._cancelled) return false;
            return this._handler.HandleNamespace(prefix, namespaceUri);
        }

        /// <summary>
        /// Handles Triples by passing them to the inner handler and cancelling handling if it has been requested
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        protected override bool HandleTripleInternal(Triple t)
        {
            if (this._cancelled) return false;
            return this._handler.HandleTriple(t);
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
            this._cancelled = true;
        }
    }
}
