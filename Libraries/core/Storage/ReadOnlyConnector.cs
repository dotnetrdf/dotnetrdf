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

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Provides a Read-Only wrapper that can be placed around another <see cref="IGenericIOManager">IGenericIOManager</see> instance
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is useful if you want to allow some code read-only access to a mutable store and ensure that it cannot modify the store via the manager instance
    /// </para>
    /// </remarks>
    public class ReadOnlyConnector : IGenericIOManager
    {
        public IGenericIOManager _manager;

        public ReadOnlyConnector(IGenericIOManager manager)
        {
            this._manager = manager;
        }

        #region IGenericIOManager Members

        public void LoadGraph(IGraph g, Uri graphUri)
        {
            this._manager.LoadGraph(g, graphUri);
        }

        public void LoadGraph(IGraph g, string graphUri)
        {
            this._manager.LoadGraph(g, graphUri);
        }

        public void SaveGraph(IGraph g)
        {
            throw new RdfStorageException("The Read-Only Connector is a read-only connection");
        }

        public void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            throw new RdfStorageException("The Read-Only Connector is a read-only connection");
        }

        public void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            throw new RdfStorageException("The Read-Only Connector is a read-only connection");
        }

        public bool UpdateSupported
        {
            get 
            {
                return false; 
            }
        }

        public void DeleteGraph(Uri graphUri)
        {
            throw new RdfStorageException("The Read-Only Connector is a read-only connection");
        }

        public void DeleteGraph(string graphUri)
        {
            throw new RdfStorageException("The Read-Only Connector is a read-only connection");
        }

        public bool DeleteSupported
        {
            get 
            {
                return false; 
            }
        }

        public bool IsReady
        {
            get 
            {
                return this._manager.IsReady; 
            }

        }

        public bool IsReadOnly
        {
            get 
            {
                return true; 
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            this._manager.Dispose();
        }

        #endregion
    }
}

#endif