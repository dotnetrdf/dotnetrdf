using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections.BuiltIn
{
    public class FusekiConnectionDefinition
        : BaseConnectionDefinition
    {
        public FusekiConnectionDefinition()
            : base("Fuseki", "Connect to a Fuseki Server which exposes SPARQL based access to any Jena based stores e.g. SDB and TDB.  The Server URI must be the /data URI of your dataset e.g. http://localhost:3030/dataset/data") { }

        [Connection(DisplayName = "Server URI", IsRequired = true, Type = ConnectionSettingType.String, DisplayOrder = -1),
DefaultValue("http://localhost:3030/dataset/data")]
        public String Server
        {
            get;
            set;
        }

        protected override IGenericIOManager OpenConnectionInternal()
        {
            return new FusekiConnector(this.Server);
        }
    }
}
