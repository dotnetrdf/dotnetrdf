using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections.BuiltIn
{
    public class SparqlQueryConnectionDefinition
        : BaseConnectionDefinition
    {
        public SparqlQueryConnectionDefinition()
            : base("SPARQL Query", "Connect to any SPARQL Query endpoint as a read-only connection") { }

        [Connection(DisplayName="Endpoint URI", DisplayOrder=1, IsRequired=true, AllowEmptyString=false),
         DefaultValue("http://example.org/sparql")]
        public String EndpointUri
        {
            get;
            set;
        }

        [Connection(DisplayName="Default Graph", DisplayOrder=2, IsRequired=false, AllowEmptyString=true)]
        public String DefaultGraphUri
        {
            get;
            set;
        }

        [Connection(DisplayName="Load Method", DisplayOrder=3, Type=ConnectionSettingType.Enum),
         DefaultValue(SparqlConnectorLoadMethod.Construct)]
        public SparqlConnectorLoadMethod LoadMode
        {
            get;
            set;
        }

        protected override IGenericIOManager OpenConnectionInternal()
        {
            return new SparqlConnector(new Uri(this.EndpointUri), this.LoadMode);
        }
    }
}
