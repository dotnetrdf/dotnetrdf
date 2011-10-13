using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using VDS.Alexandria;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections
{
    public class MongoDBConnectionDefinition
        : BaseConnectionDefinition
    {
        public MongoDBConnectionDefinition()
            : base("Alexandria MongoDB", "Use our experimental Alexandria document based Triple Store with MongoDB as the underlying store.  To connect to a MongoDB server on the local machine with standard settings just enter a DB Name, otherwise tick the Use Custom Connection String button and enter a MongoDB Connection String.") { }

        [Connection(DisplayName="Database", DisplayOrder = 1, IsRequired=true)]
        public String Database
        {
            get;
            set;
        }

        [Connection(DisplayName="Use Custom Connection String?", DisplayOrder=2, Type=ConnectionSettingType.Boolean),
         DefaultValue(false)]
        public bool UseCustomConnectionString
        {
            get;
            set;
        }

        [Connection(DisplayName="Connection String", DisplayOrder=3),
         DefaultValue("mongodb://localhost")]
        public String ConnectionString
        {
            get;
            set;
        }

        protected override IGenericIOManager OpenConnectionInternal()
        {
            if (this.UseCustomConnectionString)
            {
                return new AlexandriaMongoDBManager(this.ConnectionString, this.Database);
            }
            else
            {
                return new AlexandriaMongoDBManager(this.Database);
            }
        }
    }
}
