using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections.BuiltIn
{
    public class VirtuosoConnectionDefinition
        : BaseServerConnectionDefinition
    {
        public VirtuosoConnectionDefinition()
            : base("Virtuoso", "Connect to a Virtuoso Universal Server Quad Store") { }

        [Connection(DisplayName="Port", MinValue=1, MaxValue=65535, IsValueRestricted=true, Type=ConnectionSettingType.Integer, IsRequired=true),
         DefaultValue(VirtuosoManager.DefaultPort)]
        public int Port
        {
            get;
            set;
        }

        [Connection(DisplayName="Database", IsRequired=true, AllowEmptyString=false),
         DefaultValue("DB")]
        public String Database
        {
            get;
            set;
        }

        [Connection(DisplayName = "Username", IsRequired = false, Type = ConnectionSettingType.String)]
        public String Username
        {
            get;
            set;
        }

        [Connection(DisplayName = "Password", IsRequired = false, Type = ConnectionSettingType.Password)]
        public String Password
        {
            get;
            set;
        }

        protected override IGenericIOManager OpenConnectionInternal()
        {
            throw new NotImplementedException();
        }
    }
}
