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
using System.Collections.Generic;

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// A RDF Handler which wraps another Handler passing only the Triples falling within a given Limit and Offset to the underlying Handler
    /// </summary>
    public class PagingHandler : BaseRdfHandler, IWrappingRdfHandler
    {
        private IRdfHandler _handler;
        private int _limit = 0, _offset = 0;
        private int _counter = 0;

        /// <summary>
        /// Creates a new Paging Handler
        /// </summary>
        /// <param name="handler">Inner Handler to use</param>
        /// <param name="limit">Limit</param>
        /// <param name="offset">Offset</param>
        /// <remarks>
        /// If you just want to use an offset and not apply a limit then set limit to be less than zero
        /// </remarks>
        public PagingHandler(IRdfHandler handler, int limit, int offset)
            : base(handler)
        {
            if (handler == null) throw new ArgumentNullException("handler");
            this._handler = handler;
            this._limit = Math.Max(-1, limit);
            this._offset = Math.Max(0, offset);
        }

        /// <summary>
        /// Creates a new Paging Handler
        /// </summary>
        /// <param name="handler">Inner Handler to use</param>
        /// <param name="limit">Limit</param>
        public PagingHandler(IRdfHandler handler, int limit)
            : this(handler, limit, 0) { }

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
        /// Starts RDF Handler
        /// </summary>
        protected override void StartRdfInternal()
        {
            this._handler.StartRdf();
            this._counter = 0;
        }

        /// <summary>
        /// Ends RDF Handler
        /// </summary>
        /// <param name="ok">Indicated whether parsing completed without error</param>
        protected override void EndRdfInternal(bool ok)
        {
            this._handler.EndRdf(ok);
            this._counter = 0;
        }

        /// <summary>
        /// Handles a Triple by passing it to the Inner Handler only if the Offset has been passed and the Limit has yet to be reached
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        /// <remarks>
        /// Terminates handling immediately upon the reaching of the limit
        /// </remarks>
        protected override bool HandleTripleInternal(Triple t)
        {
            //If the Limit is zero stop parsing immediately
            if (this._limit == 0) return false;

            this._counter++;
            if (this._limit > 0)
            {
                //Limit greater than zero means get a maximum of limit triples after the offset
                if (this._counter > this._offset && this._counter <= this._limit + this._offset)
                {
                    return this._handler.HandleTriple(t);
                }
                else if (this._counter > this._limit + this._offset)
                {
                    //Stop parsing when we've reached the limit
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                //Limit less than zero means get all triples after the offset
                if (this._counter > this._offset)
                {
                    return this._handler.HandleTriple(t);
                }
                else
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// Handles Namespace Declarations by allowing the inner handler to handle it
        /// </summary>
        /// <param name="prefix">Namespace Prefix</param>
        /// <param name="namespaceUri">Namespace URI</param>
        /// <returns></returns>
        protected override bool HandleNamespaceInternal(string prefix, Uri namespaceUri)
        {
            return this._handler.HandleNamespace(prefix, namespaceUri);
        }

        /// <summary>
        /// Handles Base URI Declarations by allowing the inner handler to handle it
        /// </summary>
        /// <param name="baseUri">Base URI</param>
        protected override bool HandleBaseUriInternal(Uri baseUri)
        {
            return this._handler.HandleBaseUri(baseUri);
        }

        /// <summary>
        /// Gets whether the Handler will accept all Triples based on its Limit setting
        /// </summary>
        public override bool AcceptsAll
        {
            get 
            {
                return this._limit < 0;
            }
        }
    }
}
