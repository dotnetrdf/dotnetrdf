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

        protected override IGenericIOManager OpenConnectionInternal()
        {
            return new MicrosoftAdoManager(this.Server, this.Database, this.Username, this.Password, this.EncryptConnection);
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

        protected override IGenericIOManager OpenConnectionInternal()
        {
            return new AzureAdoManager(this.Server, this.Database, this.Username, this.Password);
        }
    }
}
