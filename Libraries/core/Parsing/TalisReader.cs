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
using VDS.RDF.Storage;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Class for reading RDF Graphs describing a given Resource from a Talis Store into arbitrary Graphs
    /// </summary>
    public class TalisReader
    {
        private TalisPlatformConnector _talis;

        /// <summary>
        /// Creates a new Talis Reader that connects to a Store using the given Connector
        /// </summary>
        /// <param name="connector">Connector</param>
        public TalisReader(TalisPlatformConnector connector)
        {
            this._talis = connector;
        }

        /// <summary>
        /// Creates a new Talis Reader that connects to a Store using the given Store settings
        /// </summary>
        /// <param name="storeName">Store Name</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        public TalisReader(String storeName, String username, String password) : this(new TalisPlatformConnector(storeName, username, password)) { }

        /// <summary>
        /// Creates a new Talis Reader that connects to a Store using the given Store settings
        /// </summary>
        /// <param name="storeName">Store Name</param>
        public TalisReader(String storeName) : this(new TalisPlatformConnector(storeName)) { }

        /// <summary>
        /// Loads the description of a given Resource from the Talis store into the given Graph
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="resourceUri">Resource Uri</param>
        public void Load(IGraph g, String resourceUri)
        {
            this._talis.Describe(g, resourceUri);
        }

        /// <summary>
        /// Loads the description of a given Resource from the Talis store into the given Graph
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="resourceUri">Resource Uri</param>
        public void Load(IGraph g, Uri resourceUri)
        {
            this._talis.Describe(g, resourceUri);
        }

        /// <summary>
        /// Loads the description of a given Resource from a Private Graph in the Talis store into the given Graph
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="privateGraphID">Private Graph ID</param>
        /// <param name="resourceUri">Resource Uri</param>
        public void Load(IGraph g, String privateGraphID, String resourceUri)
        {
            this._talis.Describe(g, privateGraphID, resourceUri);
        }

        /// <summary>
        /// Loads the description of a given Resource from a Private Graph in the Talis store into the given Graph
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="privateGraphID">Private Graph ID</param>
        /// <param name="resourceUri">Resource Uri</param>
        public void Load(IGraph g, String privateGraphID, Uri resourceUri)
        {
            this._talis.Describe(g, privateGraphID, resourceUri);
        }
    }
}

#endif