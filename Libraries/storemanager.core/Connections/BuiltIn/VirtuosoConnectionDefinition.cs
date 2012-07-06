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
using System.ComponentModel;
using System.Linq;
using System.Text;
using VDS.RDF.Configuration;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections.BuiltIn
{
    /// <summary>
    /// Definition for connections to Virtuoso
    /// </summary>
    public class VirtuosoConnectionDefinition
        : BaseCredentialsRequiredServerConnectionDefinition
    {
        /// <summary>
        /// Creates a new Definition
        /// </summary>
        public VirtuosoConnectionDefinition()
            : base("Virtuoso", "Connect to a Virtuoso Universal Server Quad Store", typeof(VirtuosoManager)) { }

        /// <summary>
        /// Gets/Sets the Server Port
        /// </summary>
        [Connection(DisplayName = "Port", DisplayOrder = 1, MinValue = 1, MaxValue = 65535, IsValueRestricted = true, Type = ConnectionSettingType.Integer, IsRequired = true, PopulateFrom = ConfigurationLoader.PropertyPort),
         DefaultValue(VirtuosoManager.DefaultPort)]
        public int Port
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the Database
        /// </summary>
        [Connection(DisplayName = "Database", DisplayOrder = 2, IsRequired = true, AllowEmptyString = false, PopulateFrom = ConfigurationLoader.PropertyDatabase),
         DefaultValue(VirtuosoManager.DefaultDB)]
        public String Database
        {
            get;
            set;
        }

        /// <summary>
        /// Opens the Connection
        /// </summary>
        /// <returns></returns>
        protected override IStorageProvider OpenConnectionInternal()
        {
            return new VirtuosoManager(this.Server, this.Port, this.Database, this.Username, this.Password);
        }
    }
}
