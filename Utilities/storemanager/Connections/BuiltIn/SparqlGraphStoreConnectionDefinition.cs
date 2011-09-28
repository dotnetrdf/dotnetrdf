using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections.BuiltIn
{
    public class SparqlGraphStoreConnectionDefinition
        : BaseConnectionDefinition
    {
        public SparqlGraphStoreConnectionDefinition()
            : base("SPARQL Graph Store", "Connect to a SPARQL Graph Store server which uses the new RESTful HTTP protocol for communicating with a Graph Store defined by SPARQL 1.1") { }

        [Connection(DisplayName = "Protocol Server", DisplayOrder = 1, IsRequired = true, AllowEmptyString = false)]
        public String Server
        {
            get;
            set;
        }

        protected override IGenericIOManager OpenConnectionInternal()
        {
            return new SparqlHttpProtocolConnector(this.Server);
        }
    }
}
