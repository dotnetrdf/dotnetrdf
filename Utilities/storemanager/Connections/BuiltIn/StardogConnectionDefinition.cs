using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections.BuiltIn
{
    public class StardogConnectionDefinition
        : BaseServerConnectionDefinition
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
                return new StardogConnector(this.Server, this.StoreID, this.ReasoningMode, StardogConnector.AnonymousUser, StardogConnector.AnonymousUser);
            }
            else
            {
                return new StardogConnector(this.Server, this.StoreID, this.ReasoningMode, this.Username, this.Password);
            }
        }
    }
}
