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
using System.Threading;

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// A RDF Handler that loads Quads into a <see cref="ITripleStore">ITripleStore</see> instance
    /// </summary>
    public class StoreHandler : BaseRdfHandler
    {
        private ITripleStore _store;

        /// <summary>
        /// Creates a new Store Handler
        /// </summary>
        /// <param name="store">Triple Store</param>
        public StoreHandler(ITripleStore store)
            : base()
        {
            if (store == null) throw new ArgumentNullException("store");
            this._store = store;
        }

        /// <summary>
        /// Gets the Triple Store that this Handler is populating
        /// </summary>
        protected ITripleStore Store
        {
            get
            {
                return this._store;
            }
        }

        #region IRdfHandler Members

        /// <summary>
        /// Handles Triples by asserting them into the appropriate Graph creating the Graph if necessary
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        protected override bool HandleTripleInternal(Triple t)
        {
            if (!this._store.HasGraph(t.GraphUri))
            {
                Graph g = new Graph();
                g.BaseUri = t.GraphUri;
                this._store.Add(g);
            }
            IGraph target = this._store.Graph(t.GraphUri);
            target.Assert(t.CopyTriple(target));
            return true;
        }

        /// <summary>
        /// Gets that the Handler accepts all Triples
        /// </summary>
        public override bool AcceptsAll
        {
            get
            {
                return true;
            }
        }

        #endregion
    }
}
