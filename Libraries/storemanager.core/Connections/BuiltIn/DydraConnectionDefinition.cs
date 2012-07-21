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
using System.Net;
using System.Text;
using VDS.RDF.Configuration;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections.BuiltIn
{
    /// <summary>
    /// Definition for connections to the Dydra cloud based Triple Store
    /// </summary>
    public class DydraConnectionDefinition
        : BaseHttpConnectionDefinition
    {
        /// <summary>
        /// Creates a new definition
        /// </summary>
        public DydraConnectionDefinition()
            : base("Dydra", "Connect to a repository hosted on Dydra the RDF database in the cloud", typeof(DydraConnector)) { }

        /// <summary>
        /// Gets/Sets the Account ID
        /// </summary>
        [Connection(DisplayName = "Account ID", DisplayOrder = 1, AllowEmptyString = false, IsRequired = true, PopulateFrom = ConfigurationLoader.PropertyCatalog)]
        public String AccountID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the Repository ID
        /// </summary>
        [Connection(DisplayName = "Repository ID", DisplayOrder = 2, AllowEmptyString = false, IsRequired = true, PopulateFrom = ConfigurationLoader.PropertyStore)]
        public String StoreID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the API Key
        /// </summary>
        [Connection(DisplayName = "API Key", DisplayOrder = 3, AllowEmptyString = true, IsRequired = false, PopulateFrom = ConfigurationLoader.PropertyUser)]
        public String ApiKey
        {
            get;
            set;
        }

        /// <summary>
        /// Opens the connection
        /// </summary>
        /// <returns></returns>
        protected override IStorageProvider OpenConnectionInternal()
        {
            if (this.UseProxy)
            {
                return new DydraConnector(this.AccountID, this.StoreID, this.ApiKey, this.GetProxy());
            }
            else
            {
                return new DydraConnector(this.AccountID, this.StoreID, this.ApiKey);
            }
        }
    }
}
