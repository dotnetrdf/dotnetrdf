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
    public class MicrosoftAdoConnectionDefinition
        : BaseCredentialsOptionalServerConnectionDefinition
    {
        public MicrosoftAdoConnectionDefinition()
            : base("SQL Server", "Connect to a dotNetRDF ADO Store which is backed by Microsoft SQL Server (2005/2008 recommended)") { }

        [Connection(DisplayName="Database", DisplayOrder=1, IsRequired=true, AllowEmptyString=false)]
        public String Database
        {
            get;
            set;
        }

        [Connection(DisplayName="Encrypt Connection?", DisplayOrder=2, Type=ConnectionSettingType.Boolean),
         DefaultValue(false)]
        public bool EncryptConnection
        {
            get;
            set;
        }

        [Connection(DisplayName="Access Mode", DisplayOrder=3, Type=ConnectionSettingType.Enum),
         DefaultValue(AdoAccessMode.Streaming)]
        public AdoAccessMode AccessMode
        {
            get;
            set;
        }

        protected override IStorageProvider OpenConnectionInternal()
        {
            return new MicrosoftAdoManager(this.Server, this.Database, this.Username, this.Password, this.EncryptConnection, this.AccessMode);
        }
    }

    public class AzureAdoConnectionDefinition
        : BaseCredentialsRequiredServerConnectionDefinition
    {
        public AzureAdoConnectionDefinition()
            : base("SQL Azure", "Connect to a dotNetRDF ADO Store which is backed by Microsoft SQL Azure") { }

        [Connection(DisplayName = "Database", DisplayOrder = 1, DisplaySuffix=".database.windows.net", IsRequired = true, AllowEmptyString = false, IsValueRestricted=true, MinValue=10, MaxValue=10)]
        public String Database
        {
            get;
            set;
        }

        protected override IStorageProvider OpenConnectionInternal()
        {
            return new AzureAdoManager(this.Server, this.Database, this.Username, this.Password);
        }
    }
}
