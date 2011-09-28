using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections.BuiltIn
{
    public class SesameConnectionDefinition
        : BaseCredentialsOptionalServerConnectionDefinition
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

        protected override IGenericIOManager OpenConnectionInternal()
        {
            return new SesameHttpProtocolConnector(this.Server, this.StoreID, this.Username, this.Password);
        }
    }
}
