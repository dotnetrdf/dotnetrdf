/*

Copyright Robert Vesse 2009-12
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections
{
    public abstract class BaseHttpConnectionDefinition
        : BaseConnectionDefinition
    {
        public BaseHttpConnectionDefinition(String storeName, String storeDescrip)
            : base(storeName, storeDescrip) { }

        [Connection(DisplayName = "Proxy Server", DisplayOrder = 20, IsRequired = false, AllowEmptyString = true, Type = ConnectionSettingType.String)]
        public String ProxyServer
        {
            get;
            set;
        }

        [Connection(DisplayName = "Proxy Username", DisplayOrder = 21, IsRequired = false, AllowEmptyString = false, Type = ConnectionSettingType.String)]
        public String ProxyUsername
        {
            get;
            set;
        }

        [Connection(DisplayName = "Proxy Password", DisplayOrder = 22, IsRequired = false, AllowEmptyString = true, Type = ConnectionSettingType.Password)]
        public String ProxyPassword
        {
            get;
            set;
        }

        protected bool UseProxy
        {
            get
            {
                return this.ProxyServer != null && !this.ProxyServer.Equals(String.Empty);
            }
        }

        protected WebProxy GetProxy()
        {
            if (this.UseProxy)
            {
                WebProxy proxy = new WebProxy(this.ProxyServer);
                if (this.ProxyUsername != null && this.ProxyPassword != null && !this.ProxyUsername.Equals(String.Empty))
                {
                    proxy.Credentials = new NetworkCredential(this.ProxyUsername, this.ProxyPassword);
                }
            }
            return null;
        }
    }

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

    public abstract class BaseHttpServerConnectionDefinition
        : BaseServerConnectionDefinition
    {
        public BaseHttpServerConnectionDefinition(String storeName, String storeDescrip)
            : base(storeName, storeDescrip) { }

        [Connection(DisplayName = "Proxy Server", DisplayOrder = 20, IsRequired = false, AllowEmptyString = true, Type = ConnectionSettingType.String)]
        public String ProxyServer
        {
            get;
            set;
        }

        [Connection(DisplayName = "Proxy Username", DisplayOrder = 21, IsRequired = false, AllowEmptyString = false, Type = ConnectionSettingType.String)]
        public String ProxyUsername
        {
            get;
            set;
        }

        [Connection(DisplayName = "Proxy Password", DisplayOrder = 22, IsRequired = false, AllowEmptyString = true, Type = ConnectionSettingType.Password)]
        public String ProxyPassword
        {
            get;
            set;
        }

        protected bool UseProxy
        {
            get
            {
                return this.ProxyServer != null && !this.ProxyServer.Equals(String.Empty);
            }
        }

        protected WebProxy GetProxy()
        {
            if (this.UseProxy)
            {
                WebProxy proxy = new WebProxy(this.ProxyServer);
                if (this.ProxyUsername != null && this.ProxyPassword != null && !this.ProxyUsername.Equals(String.Empty))
                {
                    proxy.Credentials = new NetworkCredential(this.ProxyUsername, this.ProxyPassword);
                }
            }
            return null;
        }
    }

    public abstract class BaseCredentialsRequiredServerConnectionDefinition
        : BaseServerConnectionDefinition
    {
        public BaseCredentialsRequiredServerConnectionDefinition(String storeName, String storeDescrip)
            : base(storeName, storeDescrip) { }

        [Connection(DisplayName="Username", DisplayOrder=10, IsRequired=true, AllowEmptyString=false, Type=ConnectionSettingType.String)]
        public String Username
        {
            get;
            set;
        }

        [Connection(DisplayName="Password", DisplayOrder=11, IsRequired=true, AllowEmptyString=true, Type=ConnectionSettingType.Password)]
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

        [Connection(DisplayName="Username", DisplayOrder=10, IsRequired=false, AllowEmptyString=true, Type=ConnectionSettingType.String)]
        public String Username
        {
            get;
            set;
        }

        [Connection(DisplayName="Password", DisplayOrder=11, IsRequired=false, AllowEmptyString=true, Type=ConnectionSettingType.Password)]
        public String Password
        {
            get;
            set;
        }
    }

    public abstract class BaseHttpCredentialsRequiredServerConnectionDefinition
        : BaseCredentialsRequiredServerConnectionDefinition
    {
        public BaseHttpCredentialsRequiredServerConnectionDefinition(String storeName, String storeDescrip)
            : base(storeName, storeDescrip) { }

        [Connection(DisplayName="Proxy Server", DisplayOrder=20, IsRequired=false, AllowEmptyString=true, Type=ConnectionSettingType.String)]
        public String ProxyServer
        {
            get;
            set;
        }

        [Connection(DisplayName = "Proxy Username", DisplayOrder = 21, IsRequired = false, AllowEmptyString = false, Type = ConnectionSettingType.String)]
        public String ProxyUsername
        {
            get;
            set;
        }

        [Connection(DisplayName = "Proxy Password", DisplayOrder = 22, IsRequired = false, AllowEmptyString = true, Type = ConnectionSettingType.Password)]
        public String ProxyPassword
        {
            get;
            set;
        }
    }

    public abstract class BaseHttpCredentialsOptionalServerConnectionDefinition
    : BaseCredentialsOptionalServerConnectionDefinition
    {

        public BaseHttpCredentialsOptionalServerConnectionDefinition(String storeName, String storeDescrip)
            : base(storeName, storeDescrip) { }

        [Connection(DisplayName = "Proxy Server", DisplayOrder = 20, IsRequired = false, AllowEmptyString = true, Type = ConnectionSettingType.String)]
        public String ProxyServer
        {
            get;
            set;
        }

        [Connection(DisplayName = "Proxy Username", DisplayOrder = 21, IsRequired = false, AllowEmptyString = false, Type = ConnectionSettingType.String)]
        public String ProxyUsername
        {
            get;
            set;
        }

        [Connection(DisplayName = "Proxy Password", DisplayOrder = 22, IsRequired = false, AllowEmptyString = true, Type = ConnectionSettingType.Password)]
        public String ProxyPassword
        {
            get;
            set;
        }

        protected bool UseProxy
        {
            get
            {
                return this.ProxyServer != null && !this.ProxyServer.Equals(String.Empty);
            }
        }

        protected WebProxy GetProxy()
        {
            if (this.UseProxy)
            {
                WebProxy proxy = new WebProxy(this.ProxyServer);
                if (this.ProxyUsername != null && this.ProxyPassword != null && !this.ProxyUsername.Equals(String.Empty))
                {
                    proxy.Credentials = new NetworkCredential(this.ProxyUsername, this.ProxyPassword);
                }
            }
            return null;
        }
    }
}
