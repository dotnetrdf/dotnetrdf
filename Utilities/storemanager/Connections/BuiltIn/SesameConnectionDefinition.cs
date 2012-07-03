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
    public class SesameConnectionDefinition
        : BaseHttpCredentialsOptionalServerConnectionDefinition
    {
        public SesameConnectionDefinition()
            : base("Sesame", "Connect to any Sesame based store which is exposed via a Sesame HTTP Server e.g. Sesame Native, BigOWLIM") { }

        [Connection(DisplayName = "Server URI", IsRequired = true, Type = ConnectionSettingType.String, DisplayOrder = -1),
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

        [Connection(DisplayName = "Repository ID", DisplayOrder = 1, AllowEmptyString = false, IsRequired = true, Type = ConnectionSettingType.String)]
        public String StoreID
        {
            get;
            set;
        }

        protected override IStorageProvider OpenConnectionInternal()
        {
            if (this.UseProxy)
            {
                return new SesameHttpProtocolConnector(this.Server, this.StoreID, this.Username, this.Password, this.GetProxy());
            }
            else
            {
                return new SesameHttpProtocolConnector(this.Server, this.StoreID, this.Username, this.Password);
            }
        }
    }
}
