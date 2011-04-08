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

#if !NO_STORAGE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using VDS.RDF.Storage;

namespace VDS.RDF
{
    /// <summary>
    /// A Graph which represents the description that an underlying Talis store has of a given Uri from the Metabox or a Private Graph
    /// </summary>
    /// <remarks>
    /// Any Changes to this Graph locally are persisted to the Talis platform whenever the Graph is disposed or the <see cref="TalisGraph.Flush">Flush()</see> method is called.  Alternatively the <see cref="TalisGraph.Discard">Discard()</see> method may be called to discard the changes that have yet to be persisted
    /// </remarks>
    [Obsolete("TalisGraph is considered obsolete, use the more general StoreGraphPersistenceWrapper instead", false)]
    public class TalisGraph : StoreGraphPersistenceWrapper
    {
        private TalisPlatformConnector _talis;
        private String _privateGraphID = String.Empty;

        #region Constructors

        /// <summary>
        /// Creates a new instance of a Talis Graph which contains the description of the given Uri from the given underlying Talis Store
        /// </summary>
        /// <param name="resourceUri">Uri of resource to retrieve a Description of</param>
        /// <param name="connector">Connection to a Talis Store</param>
        public TalisGraph(String resourceUri, TalisPlatformConnector connector)
            : this(new Uri(resourceUri), connector) { }

        /// <summary>
        /// Creates a new instance of a Talis Graph which contains the description of the given Uri from the given underlying Talis Store
        /// </summary>
        /// <param name="resourceUri">Uri of resource to retrieve a Description of</param>
        /// <param name="connector">Connection to a Talis Store</param>
        public TalisGraph(Uri resourceUri, TalisPlatformConnector connector)
            : base(connector, resourceUri)
        {
            this._talis = connector;
        }

        /// <summary>
        /// Creates a new instance of a Talis Graph which contains the description of the given Uri from the given underlying Talis Store
        /// </summary>
        /// <param name="resourceUri">Uri of resource to retrieve a Description of</param>
        /// <param name="storeName">Name of the Talis Store</param>
        /// <param name="username">Username for the Talis Store</param>
        /// <param name="password">Password for the Talis Store</param>
        public TalisGraph(String resourceUri, String storeName, String username, String password) 
            : this(resourceUri, new TalisPlatformConnector(storeName, username, password)) { }

        /// <summary>
        /// Creates a new instance of a Talis Graph which contains the description of the given Uri from the given underlying Talis Store
        /// </summary>
        /// <param name="resourceUri">Uri of resource to retrieve a Description of</param>
        /// <param name="storeName">Name of the Talis Store</param>
        /// <param name="username">Username for the Talis Store</param>
        /// <param name="password">Password for the Talis Store</param>
        public TalisGraph(Uri resourceUri, String storeName, String username, String password)
            : this(resourceUri.ToString(), new TalisPlatformConnector(storeName, username, password)) { }

        #endregion
    }
}

#endif