/*

Copyright Robert Vesse 2009-11
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

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// A RDF Handler which just determines whether any Triples are present terminating parsing as soon as the first triple is received
    /// </summary>
    public class AnyHandler 
        : BaseRdfHandler
    {
        private bool _any = false;

        /// <summary>
        /// Creates a new Any Handler
        /// </summary>
        public AnyHandler()
            : base(new MockNodeFactory()) { }

        /// <summary>
        /// Gets whether any Triples have been parsed
        /// </summary>
        public bool Any
        {
            get
            {
                return this._any;
            }
        }

        /// <summary>
        /// Starts handling RDF by resetting the <see cref="AnyHandler.Any">Any</see> flag to false
        /// </summary>
        protected override void StartRdfInternal()
        {
            this._any = false;
        }

        /// <summary>
        /// Handles Base URIs by ignoring them
        /// </summary>
        /// <param name="baseUri">Base URI</param>
        /// <returns></returns>
        protected override bool HandleBaseUriInternal(Uri baseUri)
        {
            return true;
        }

        /// <summary>
        /// Handles Namespaces by ignoring them
        /// </summary>
        /// <param name="prefix">Prefix</param>
        /// <param name="namespaceUri">Namespace URI</param>
        /// <returns></returns>
        protected override bool HandleNamespaceInternal(string prefix, Uri namespaceUri)
        {
            return true;
        }

        /// <summary>
        /// Handles Triples by setting the <see cref="AnyHandler.Any">Any</see> flag and terminating parsing
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        protected override bool HandleTripleInternal(Triple t)
        {
            this._any = true;
            return false;
        }

        /// <summary>
        /// Gets that this handler does not accept all triples since it stops as soon as it sees the first triple
        /// </summary>
        public override bool AcceptsAll
        {
            get 
            {
                return false;
            }
        }
    }
}
