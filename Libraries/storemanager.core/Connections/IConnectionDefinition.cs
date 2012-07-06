/*

Copyright Robert Vesse 2009-12
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
using System.Reflection;
using System.Text;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections
{
    /// <summary>
    /// Defines a Connection
    /// </summary>
    /// <remarks>
    /// <para>
    /// This interface is intended to be used in conjunction with <see cref="ConnectionAttribute"/> annotations on properties of your implementation in order to indicate configurable properties that can be controlled.  Then when your <see cref="IConnectionDefinition.OpenConnection()"/> method gets called you can use the information in those properties to actually create a connection
    /// </para>
    /// </remarks>
    public interface IConnectionDefinition
        : IEnumerable<KeyValuePair<PropertyInfo, ConnectionAttribute>>
    {
        /// <summary>
        /// Gets the display name for this connection
        /// </summary>
        String StoreName
        {
            get;
        }

        /// <summary>
        /// Gets the description for this connection
        /// </summary>
        String StoreDescription
        {
            get;
        }

        /// <summary>
        /// Gets the .Net type that connections created from this definition will have
        /// </summary>
        Type Type
        {
            get;
        }

        /// <summary>
        /// Opens a new connection
        /// </summary>
        /// <returns></returns>
        IStorageProvider OpenConnection();

        /// <summary>
        /// Populates settings from an existing Configuration Serialization Graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="objNode">Object Node</param>
        void PopulateFrom(IGraph g, INode objNode);
    }
}
