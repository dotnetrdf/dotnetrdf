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
using System.ComponentModel;
using System.Linq;
using System.Text;
using VDS.RDF.Configuration;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections.BuiltIn
{
    /// <summary>
    /// Definition for connections to Microsoft SQL Server based stores
    /// </summary>
    [Obsolete("The Data.Sql library is considered obsolete and will be removed in future releases", false)]
    public class MicrosoftAdoConnectionDefinition
        : BaseCredentialsOptionalServerConnectionDefinition
    {
        /// <summary>
        /// Creates a new definition
        /// </summary>
        public MicrosoftAdoConnectionDefinition()
            : base("SQL Server", "Connect to a dotNetRDF ADO Store which is backed by Microsoft SQL Server (2005/2008 recommended)", typeof(MicrosoftAdoManager)) { }

        /// <summary>
        /// Gets/Sets the Database
        /// </summary>
        [Connection(DisplayName = "Database", DisplayOrder = 1, IsRequired = true, AllowEmptyString = false, PopulateFrom = ConfigurationLoader.PropertyDatabase)]
        public String Database
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets whether to encrypt the connection
        /// </summary>
        [Connection(DisplayName = "Encrypt Connection?", DisplayOrder = 2, Type = ConnectionSettingType.Boolean, PopulateFrom = ConfigurationLoader.PropertyEncryptConnection),
         DefaultValue(false)]
        public bool EncryptConnection
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the access mode
        /// </summary>
        [Connection(DisplayName = "Access Mode", DisplayOrder = 3, Type = ConnectionSettingType.Enum, PopulateFrom = ConfigurationLoader.PropertyLoadMode),
         DefaultValue(AdoAccessMode.Streaming)]
        public AdoAccessMode AccessMode
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
            return new MicrosoftAdoManager(this.Server, this.Database, this.Username, this.Password, this.EncryptConnection, this.AccessMode);
        }
    }

    /// <summary>
    /// Definition for connections to Microsoft SQL Azure based stores
    /// </summary>
    [Obsolete("The Data.Sql library is considered obsolete and will be removed in future releases", false)]
    public class AzureAdoConnectionDefinition
        : BaseCredentialsRequiredServerConnectionDefinition
    {
        /// <summary>
        /// Creates a new definition
        /// </summary>
        public AzureAdoConnectionDefinition()
            : base("SQL Azure", "Connect to a dotNetRDF ADO Store which is backed by Microsoft SQL Azure", typeof(AzureAdoManager)) { }

        /// <summary>
        /// Gets/Sets the Database Name
        /// </summary>
        [Connection(DisplayName = "Database", DisplayOrder = 1, DisplaySuffix = ".database.windows.net", IsRequired = true, AllowEmptyString = false, IsValueRestricted = true, MinValue = 10, MaxValue = 10, PopulateFrom = ConfigurationLoader.PropertyDatabase)]
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
            return new AzureAdoManager(this.Server, this.Database, this.Username, this.Password);
        }
    }
}
