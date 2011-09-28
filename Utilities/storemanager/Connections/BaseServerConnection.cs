using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections
{
    public abstract class BaseServerConnectionDefinition
        : BaseConnectionDefinition
    {
        public BaseServerConnectionDefinition(String storeName, String storeDescrip)
            : base(storeName, storeDescrip) { }

        [Connection(DisplayName="Server", IsRequired=true, Type=ConnectionSettingType.String, DisplayOrder=-1),
        DefaultValue("localhost")]
        public virtual String Server
        {
            get;
            set;
        }
    }

    public abstract class BaseCredentialsRequiredServerConnectionDefinition
        : BaseServerConnectionDefinition
    {
        public BaseCredentialsRequiredServerConnectionDefinition(String storeName, String storeDescrip)
            : base(storeName, storeDescrip) { }

        [Connection(DisplayName = "Username", DisplayOrder = 10, IsRequired = true, AllowEmptyString=false, Type = ConnectionSettingType.String)]
        public String Username
        {
            get;
            set;
        }

        [Connection(DisplayName = "Password", DisplayOrder = 11, IsRequired = true, AllowEmptyString=true, Type = ConnectionSettingType.Password)]
        public String Password
        {
            get;
            set;
        }
    }

    public abstract class BaseCredentialsOptionalServerConnectionDefinition
        : BaseServerConnectionDefinition
    {
        public BaseCredentialsOptionalServerConnectionDefinition(String storeName, String storeDescrip)
            : base(storeName, storeDescrip) { }

        [Connection(DisplayName = "Username", DisplayOrder = 10, IsRequired = false, AllowEmptyString=true, Type = ConnectionSettingType.String)]
        public String Username
        {
            get;
            set;
        }

        [Connection(DisplayName = "Password", DisplayOrder = 11, IsRequired = false, AllowEmptyString=true, Type = ConnectionSettingType.Password)]
        public String Password
        {
            get;
            set;
        }
    }
}
