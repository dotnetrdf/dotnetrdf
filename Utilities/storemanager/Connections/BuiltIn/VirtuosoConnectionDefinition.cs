using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections.BuiltIn
{
    public class VirtuosoConnectionDefinition
        : BaseCredentialsRequiredServerConnectionDefinition
    {
        public VirtuosoConnectionDefinition()
            : base("Virtuoso", "Connect to a Virtuoso Universal Server Quad Store") { }

        [Connection(DisplayName="Port", DisplayOrder=1, MinValue=1, MaxValue=65535, IsValueRestricted=true, Type=ConnectionSettingType.Integer, IsRequired=true),
         DefaultValue(VirtuosoManager.DefaultPort)]
        public int Port
        {
            get;
            set;
        }

        [Connection(DisplayName="Database", DisplayOrder=2, IsRequired=true, AllowEmptyString=false),
         DefaultValue("DB")]
        public String Database
        {
            get;
            set;
        }

        protected override IGenericIOManager OpenConnectionInternal()
        {
            return new VirtuosoManager(this.Server, this.Port, this.Database, this.Username, this.Password);
        }
    }
}
