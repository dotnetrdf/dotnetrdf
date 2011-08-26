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
using System.Linq;
using System.Text;

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// A RDF Handler which simply counts the Triples and Graphs
    /// </summary>
    public class StoreCountHandler : BaseRdfHandler
    {
        private int _counter = 0;
        private HashSet<String> _graphs;

        /// <summary>
        /// Creates a new Store Count Handler
        /// </summary>
        public StoreCountHandler()
            : base(new MockNodeFactory()) { }

        /// <summary>
        /// Starts RDF Handling by reseting the counters
        /// </summary>
        protected override void StartRdfInternal()
        {
            this._counter = 0;
            this._graphs = new HashSet<string>();
        }

        /// <summary>
        /// Handles Triples/Quads by counting the Triples and distinct Graph URIs
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        protected override bool HandleTripleInternal(Triple t)
        {
            this._counter++;
            this._graphs.Add(t.GraphUri.ToSafeString());
            return true;
        }

        /// <summary>
        /// Gets the count of Triples
        /// </summary>
        public int TripleCount
        {
            get
            {
                return this._counter;
            }
        }

        /// <summary>
        /// Gets the count of distinct Graph URIs
        /// </summary>
        public int GraphCount
        {
            get 
            {
                return this._graphs.Count;
            }
        }

        /// <summary>
        /// Gets that this Handler accepts all Triples
        /// </summary>
        public override bool AcceptsAll
        {
            get 
            {
                return true; 
            }
        }
    }
}
