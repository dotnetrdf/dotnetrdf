using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections.BuiltIn
{
    public class AllegroGraphConnectionDefinition
        : BaseCredentialsOptionalServerConnectionDefinition
    {
        public AllegroGraphConnectionDefinition()
            : base("Allegro Graph", "Connect to Franz AllegroGraph, Version 3.x and 4.x are supported") { }

        [Connection(DisplayName="Server URI", IsRequired=true, DisplayOrder=-1),
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

        [Connection(DisplayName="Catalog ID", DisplayOrder=1, AllowEmptyString=true, IsRequired=true, Type=ConnectionSettingType.String, NotRequiredIf="UseRootCatalog")]
        public String CatalogID
        {
            get;
            set;
        }

        [Connection(DisplayName="Use Root Catalog? (4.x and Higher)", DisplayOrder=2, Type=ConnectionSettingType.Boolean)]
        public bool UseRootCatalog
        {
            get;
            set;
        }

        [Connection(DisplayName="Store ID", DisplayOrder=3, AllowEmptyString=false, IsRequired=true, Type=ConnectionSettingType.String)]
        public String StoreID
        {
            get;
            set;
        }

        protected override IGenericIOManager OpenConnectionInternal()
        {
            if (this.UseRootCatalog)
            {
                return new AllegroGraphConnector(this.Server, this.StoreID, this.Username, this.Password);
            }
            else
            {
                return new AllegroGraphConnector(this.Server, this.CatalogID, this.StoreID, this.Username, this.Password);
            }
        }
    }
}
