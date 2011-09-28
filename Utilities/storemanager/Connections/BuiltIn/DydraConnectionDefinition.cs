using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections.BuiltIn
{
    public class DydraConnectionDefinition
        : BaseConnectionDefinition
    {
        public DydraConnectionDefinition()
            : base("Dydra", "Connect to a repository hosted on Dydra the RDF database in the cloud") { }

        [Connection(DisplayName="Account ID", DisplayOrder=1, AllowEmptyString=false, IsRequired=true)]
        public String AccountID
        {
            get;
            set;
        }

        [Connection(DisplayName="Repository ID", DisplayOrder=2, AllowEmptyString=false, IsRequired=true)]
        public String StoreID
        {
            get;
            set;
        }

        [Connection(DisplayName = "API Key", DisplayOrder=3, AllowEmptyString=true, IsRequired=false)]
        public String ApiKey
        {
            get;
            set;
        }

        protected override IGenericIOManager OpenConnectionInternal()
        {
            return new DydraConnector(this.AccountID, this.StoreID, this.ApiKey);
        }
    }
}
