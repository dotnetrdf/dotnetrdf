using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections.BuiltIn
{
    public class TalisConnectionDefinition
        : BaseConnectionDefinition
    {
        public TalisConnectionDefinition()
            : base("Talis", "Connect to a store hosted on the Talis Platform") { }

        [Connection(DisplayName="Store ID", DisplayOrder=1, IsRequired=true, AllowEmptyString=false)]
        public String StoreID
        {
            get;
            set;
        }

        [Connection(DisplayName="Username", DisplayOrder=10, IsRequired=false, AllowEmptyString=true, Type=ConnectionSettingType.String)]
        public String Username
        {
            get;
            set;
        }

        [Connection(DisplayName="Password", DisplayOrder=11, IsRequired=false, AllowEmptyString=true, Type=ConnectionSettingType.Password)]
        public String Password
        {
            get;
            set;
        }

        protected override IGenericIOManager OpenConnectionInternal()
        {
            return new TalisPlatformConnector(this.StoreID, this.Username, this.Password);
        }
    }
}
