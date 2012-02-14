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
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections.BuiltIn
{
    public class StardogConnectionDefinition
        : BaseHttpServerConnectionDefinition
    {
        public StardogConnectionDefinition()
            : base("Stardog", "Connect to a Stardog database exposed via the Stardog HTTP server") { }

        [Connection(DisplayName="Server URI", IsRequired=true, AllowEmptyString=false, DisplayOrder=-1),
         DefaultValue("http://localhost/")]
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

        [Connection(DisplayName="Store ID", DisplayOrder=1, AllowEmptyString=false, IsRequired=true)]
        public String StoreID
        {
            get;
            set;
        }

        [Connection(DisplayName="Query Reasoning Mode", DisplayOrder=2, Type=ConnectionSettingType.Enum),
         DefaultValue(StardogReasoningMode.None)]
        public StardogReasoningMode ReasoningMode
        {
            get;
            set;
        }

        [Connection(DisplayName="Username", DisplayOrder=3, IsRequired=false, AllowEmptyString=true)]
        public String Username
        {
            get;
            set;
        }

        [Connection(DisplayName="Password", DisplayOrder=4, IsRequired=false, AllowEmptyString=true, Type=ConnectionSettingType.Password)]
        public String Password
        {
            get;
            set;
        }

        [Connection(DisplayName="Use the Stardog default anonymous user account instead of an explicit Username and Password?", DisplayOrder=5, Type=ConnectionSettingType.Boolean),
         DefaultValue(false)]
        public bool UseAnonymousAccount
        {
            get;
            set;
        }

        protected override IGenericIOManager OpenConnectionInternal()
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
