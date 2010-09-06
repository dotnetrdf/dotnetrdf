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
    /// Any Changes to this Graph locally are persisted to the Talis platform using a Background Thread, if a process using this class terminates unexpectedly some changes may be lost.
    /// </remarks>
    public class TalisGraph : BackgroundPersistedGraph, IDisposable
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
            : base()
        {
            this._talis = connector;
            this._talis.Describe(this, resourceUri);
            this.Intialise();
        }

        /// <summary>
        /// Creates a new instance of a Talis Graph which contains the description of the given Uri from the given underlying Talis Store
        /// </summary>
        /// <param name="resourceUri">Uri of resource to retrieve a Description of</param>
        /// <param name="connector">Connection to a Talis Store</param>
        public TalisGraph(Uri resourceUri, TalisPlatformConnector connector)
            : this(resourceUri.ToString(), connector) { }

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

        /// <summary>
        /// Creates a new instance of a Talis Graph which represents the description of a Resource from one of the Private Graphs from the underlying Talis Store
        /// </summary>
        /// <param name="privateGraphID">Private Graph</param>
        /// <param name="connector">Connection to a Talis Store</param>
        /// <param name="resourceUri">Uri of the Resource to Describe</param>
        public TalisGraph(String privateGraphID, String resourceUri, TalisPlatformConnector connector) 
            : base()
        {
            this._talis = connector;
            this._privateGraphID = privateGraphID;
            this._talis.Describe(this, privateGraphID, resourceUri);
            this.Intialise();
        }

        /// <summary>
        /// Creates a new instance of a Talis Graph which represents the description of a Resource from one of the Private Graphs from the underlying Talis Store
        /// </summary>
        /// <param name="privateGraphID">Private Graph</param>
        /// <param name="resourceUri">Uri of the Resource to Describe</param>
        /// <param name="storeName">Name of the Talis Store</param>
        /// <param name="username">Username for the Talis Store</param>
        /// <param name="password">Password for the Talis Store</param>
        public TalisGraph(String privateGraphID, String resourceUri, String storeName, String username, String password) 
            : this(privateGraphID, resourceUri, new TalisPlatformConnector(storeName, username, password)) { }

        /// <summary>
        /// Creates a new instance of a Talis Graph which represents the description of a Resource from one of the Private Graphs from the underlying Talis Store
        /// </summary>
        /// <param name="privateGraphID">Private Graph</param>
        /// <param name="connector">Connection to a Talis Store</param>
        /// <param name="resourceUri">Uri of the Resource to Describe</param>
        public TalisGraph(String privateGraphID, Uri resourceUri, TalisPlatformConnector connector) 
            : this(privateGraphID, resourceUri.ToString(), connector) { }

        /// <summary>
        /// Creates a new instance of a Talis Graph which represents the description of a Resource from one of the Private Graphs from the underlying Talis Store
        /// </summary>
        /// <param name="privateGraphID">Private Graph</param>
        /// <param name="resourceUri">Uri of the Resource to Describe</param>
        /// <param name="storeName">Name of the Talis Store</param>
        /// <param name="username">Username for the Talis Store</param>
        /// <param name="password">Password for the Talis Store</param>
        public TalisGraph(String privateGraphID, Uri resourceUri, String storeName, String username, String password)
            : this(privateGraphID, resourceUri, new TalisPlatformConnector(storeName, username, password)) { }

        #endregion

        /// <summary>
        /// Persists the Change Buffers to the Talis Store using the Talis Platform Connector
        /// </summary>
        protected override void UpdateStore()
        {
            //Make the Update
            TalisUpdateResult result;
            if (this._privateGraphID.Equals(String.Empty))
            {
                result = this._talis.Update(this._addedTriplesBuffer, this._removedTriplesBuffer);
            }
            else
            {
                result = this._talis.Update(this._privateGraphID, this._addedTriplesBuffer, this._removedTriplesBuffer);
            }

            //Handle the Result
            switch (result)
            {
                case TalisUpdateResult.Asynchronous:
                case TalisUpdateResult.Synchronous:
                case TalisUpdateResult.NotRequired:
                case TalisUpdateResult.Done:
                    //We're OK
                    break;
                case TalisUpdateResult.Unknown:
                default:
                    //Something went wrong
                    throw new TalisException("An Unknown Result was received while trying to Update the Talis Store");
            }
        }
 
        /// <summary>
        /// Disposes of a Talis Graph
        /// </summary>
        public override void Dispose()
        {
            //Dispose of the Base Class first which will force the Change Buffers to be persisted
            base.Dispose();
            //Then dispose of the Connector as this will be necessary for our Persistence to work
            this._talis.Dispose();
        }
    }
}

#endif