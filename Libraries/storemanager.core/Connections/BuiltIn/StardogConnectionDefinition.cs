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
    /// Definition for connections to Stardog
    /// </summary>
    public class StardogConnectionDefinition
        : BaseHttpServerConnectionDefinition
    {
        /// <summary>
        /// Creates a new definition
        /// </summary>
        public StardogConnectionDefinition()
            : base("Stardog", "Connect to a Stardog database exposed via the Stardog HTTP server", typeof(StardogConnector)) { }

        /// <summary>
        /// Gets/Sets the Server URI
        /// </summary>
        [Connection(DisplayName="Server URI", IsRequired=true, AllowEmptyString=false, DisplayOrder=-1, PopulateFrom=ConfigurationLoader.PropertyServer),
         DefaultValue("http://localhost:5822/")]
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
        /// Gets/Sets the Store ID
        /// </summary>
        [Connection(DisplayName = "Store ID", DisplayOrder = 1, AllowEmptyString = false, IsRequired = true, PopulateFrom = ConfigurationLoader.PropertyStore)]
        public String StoreID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the Reasoning Mode for queries
        /// </summary>
        [Connection(DisplayName="Query Reasoning Mode", DisplayOrder=2, Type=ConnectionSettingType.Enum, PopulateFrom = ConfigurationLoader.PropertyLoadMode),
         DefaultValue(StardogReasoningMode.None)]
        public StardogReasoningMode ReasoningMode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the Username
        /// </summary>
        [Connection(DisplayName = "Username", DisplayOrder = 3, IsRequired = false, AllowEmptyString = true, PopulateFrom = ConfigurationLoader.PropertyUser)]
        public String Username
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the Password
        /// </summary>
        [Connection(DisplayName = "Password", DisplayOrder = 4, IsRequired = false, AllowEmptyString = true, Type = ConnectionSettingType.Password, PopulateFrom = ConfigurationLoader.PropertyPassword)]
        public String Password
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets whether to use the Stardog anonymous account
        /// </summary>
        [Connection(DisplayName="Use the Stardog default anonymous user account instead of an explicit Username and Password?", DisplayOrder=5, Type=ConnectionSettingType.Boolean),
         DefaultValue(false)]
        public bool UseAnonymousAccount
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
            if (this.UseAnonymousAccount)
            {
                if (this.UseProxy)
                {
                    return new StardogConnector(this.Server, this.StoreID, this.ReasoningMode, StardogConnector.AnonymousUser, StardogConnector.AnonymousUser, this.GetProxy());
                }
                else
                {
                    return new StardogConnector(this.Server, this.StoreID, this.ReasoningMode, StardogConnector.AnonymousUser, StardogConnector.AnonymousUser);
                }
            }
            else
            {
                if (this.UseProxy)
                {
                    return new StardogConnector(this.Server, this.StoreID, this.ReasoningMode, this.Username, this.Password, this.GetProxy());
                }
                else
                {
                    return new StardogConnector(this.Server, this.StoreID, this.ReasoningMode, this.Username, this.Password);
                }
            }
        }
    }
}
