using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections.BuiltIn
{
    public class JosekiConnectionDefinition
        : BaseConnectionDefinition
    {
        public JosekiConnectionDefinition()
            : base("Joseki", "Connect to a Joseki Server which exposes SPARQL based access to any Jena based stores e.g. SDB and TDB.") { }

        [Connection(DisplayName = "Server URI", IsRequired = true, Type = ConnectionSettingType.String, DisplayOrder = -1),
DefaultValue("http://localhost:2020")]
        public String Server
        {
            get;
            set;
        }

        [Connection(DisplayName="Query Path", IsRequired=true, AllowEmptyString=false),
         DefaultValue("sparql")]
        public String QueryPath
        {
            get;
            set;
        }

        [Connection(DisplayName="Update Path", IsRequired=false, AllowEmptyString=true, DisplaySuffix="(Leave blank for read-only connection)"),
         DefaultValue("update")]
        public String UpdatePath
        {
            get;
            set;
        }

        protected override IGenericIOManager OpenConnectionInternal()
        {
            return new JosekiConnector(this.Server, this.QueryPath, (String.IsNullOrEmpty(this.UpdatePath) ? null : this.UpdatePath));
        }
    }
}
