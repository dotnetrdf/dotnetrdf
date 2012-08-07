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
using System.Linq;
using System.Text;

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// A decorator for handlers which ensures that all blank nodes get unique IDs even if a blank node identifier is reused
    /// </summary>
    /// <remarks>
    /// <para>
    /// In most parsing scenarios this handler is not suitable for usage as it may unintentionally modify the RDF data being parsed, in non-parsing scenarios where this handler is instead being used as a means to generate RDF data from some non-RDF source it may prove very useful.
    /// </para>
    /// <para>
    /// This handler essentially works by redirecting all calls to the argument taking form of <see cref="INodeFactory.CreateBlankNode()"/> with the non-argument form which should always generate a new blank node thus guaranteeing the uniqueness of nodes.
    /// </para>
    /// </remarks>
    public class UniqueBlankNodesHandler
        : BaseRdfHandler, IWrappingRdfHandler
    {
        private IRdfHandler _handler;

        /// <summary>
        /// Creates a new Unique Blank Nodes handler
        /// </summary>
        /// <param name="handler"></param>
        public UniqueBlankNodesHandler(IRdfHandler handler)
        {
            if (handler == null) throw new ArgumentNullException("handler");
            this._handler = handler;
        }

        public IEnumerable<IRdfHandler> InnerHandlers
        {
            get
            {
                return this._handler.AsEnumerable();
            }
        }

        public override IBlankNode CreateBlankNode(string nodeId)
        {
            return base.CreateBlankNode();
        }

        protected override void StartRdfInternal()
        {
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
                return this._handler.AcceptsAll;
            }
        }
    }
}
