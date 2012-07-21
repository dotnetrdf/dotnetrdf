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
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Controls
{
    /// <summary>
    /// Event Arguments for Connection Event
    /// </summary>
    public class ConnectedEventArgs
        : EventArgs
    {
        /// <summary>
        /// Creates a new Connected Event arguments
        /// </summary>
        /// <param name="connection">Storage Provider</param>
        public ConnectedEventArgs(IStorageProvider connection)
        {
            this.Connection = connection;
        }

        /// <summary>
        /// Gets/Sets the Connection
        /// </summary>
        public IStorageProvider Connection
        {
            get;
            private set;
        }
    }

    /// <summary>
    /// Delegate for Connected Event
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">Event Arguments</param>
    public delegate void Connected(Object sender, ConnectedEventArgs e);
}
