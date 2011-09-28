using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections.BuiltIn
{
    public class FourStoreConnectionDefinition
        : BaseServerConnectionDefinition
    {
        public FourStoreConnectionDefinition()
            : base("4store", "Connect to a 4store server") { }

        [Connection(DisplayName="Enable SPARQL Update support? (Requires 4store 1.1.x)", DisplayOrder=1, Type=ConnectionSettingType.Boolean),
         DefaultValue(true)]
        public bool EnableUpdates
        {
            get;
            set;
        }

        protected override IGenericIOManager OpenConnectionInternal()
        {
            return new FourStoreConnector(this.Server, this.EnableUpdates);
        }
    }
}
