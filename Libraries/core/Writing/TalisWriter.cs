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

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class for writing RDF Graphs to a Talis Store
    /// </summary>
    public class TalisWriter
    {
        private TalisPlatformConnector _talis;

        /// <summary>
        /// Creates a new Talis Reader that connects to a Store using the given Connector
        /// </summary>
        /// <param name="connector">Connector</param>
        public TalisWriter(TalisPlatformConnector connector)
        {
            this._talis = connector;
        }

        /// <summary>
        /// Creates a new Talis Reader that connects to a Store using the given Store settings
        /// </summary>
        /// <param name="storeName">Store Name</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        public TalisWriter(String storeName, String username, String password) : this(new TalisPlatformConnector(storeName, username, password)) { }

        /// <summary>
        /// Creates a new Talis Reader that connects to a Store using the given Store settings
        /// </summary>
        /// <param name="storeName">Store Name</param>
        /// <remarks>Generally authentication is required for adding data to a Talis store but some stores may be world writable so this constructor is still provided</remarks>
        public TalisWriter(String storeName) : this(new TalisPlatformConnector(storeName)) { }

        /// <summary>
        /// Saves a Graph to the Metabox of the Talis Store
        /// </summary>
        /// <param name="g">Graph to save</param>
        public void Save(IGraph g)
        {
            this._talis.Add(g);
        }

        /// <summary>
        /// Saves a Graph to a Private Graph in the Talis Store
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="privateGraphID">Private Graph ID</param>
        public void Save(IGraph g, String privateGraphID)
        {
            this._talis.Add(g, privateGraphID);
        }
    }
}

#endif