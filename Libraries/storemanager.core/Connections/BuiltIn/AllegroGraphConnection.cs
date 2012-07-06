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
using System.ComponentModel;
using VDS.RDF.Configuration;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections.BuiltIn
{
    /// <summary>
    /// Definition for connections to AllegroGraph
    /// </summary>
    public class AllegroGraphConnectionDefinition
        : BaseHttpCredentialsOptionalServerConnectionDefinition
    {
        /// <summary>
        /// Creates a new definition
        /// </summary>
        public AllegroGraphConnectionDefinition()
            : base("Allegro Graph", "Connect to Franz AllegroGraph, Version 3.x and 4.x are supported", typeof(AllegroGraphConnector)) { }

        /// <summary>
        /// Gets/Sets the Server URI
        /// </summary>
        [Connection(DisplayName = "Server URI", IsRequired = true, DisplayOrder = -1, PopulateFrom = ConfigurationLoader.PropertyServer),
         DefaultValue("http://localhost:9875/")]
        public override String Server
        {
            get
            {
                return base.Server;
            }
            set
            {
                base.Server = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Catalog ID
        /// </summary>
        [Connection(DisplayName = "Catalog ID", DisplayOrder = 1, AllowEmptyString = true, IsRequired = true, Type = ConnectionSettingType.String, NotRequiredIf = "UseRootCatalog", PopulateFrom = ConfigurationLoader.PropertyCatalog)]
        public String CatalogID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets whether to use the Root Catalog
        /// </summary>
        [Connection(DisplayName = "Use Root Catalog? (4.x and Higher)", DisplayOrder = 2, Type = ConnectionSettingType.Boolean, PopulateFrom = ConfigurationLoader.PropertyUser)]
        public bool UseRootCatalog
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the Store ID
        /// </summary>
        [Connection(DisplayName = "Store ID", DisplayOrder = 3, AllowEmptyString = false, IsRequired = true, Type = ConnectionSettingType.String, PopulateFrom = ConfigurationLoader.PropertyPassword)]
        public String StoreID
        {
            get;
            set;
        }

        /// <summary>
        /// Opens the connection to AllegroGraph
        /// </summary>
        /// <returns></returns>
        protected override IStorageProvider OpenConnectionInternal()
        {
            if (this.UseRootCatalog)
            {
                if (this.UseProxy)
                {
                    return new AllegroGraphConnector(this.Server, this.StoreID, this.Username, this.Password, this.GetProxy());
                }
                else
                {
                    return new AllegroGraphConnector(this.Server, this.StoreID, this.Username, this.Password);
                }
            }
            else
            {
                if (this.UseProxy)
                {
                    return new AllegroGraphConnector(this.Server, this.CatalogID, this.StoreID, this.GetProxy());
                }
                else
                {
                    return new AllegroGraphConnector(this.Server, this.CatalogID, this.StoreID, this.Username, this.Password);
                }
                
            }
        }
    }
}
