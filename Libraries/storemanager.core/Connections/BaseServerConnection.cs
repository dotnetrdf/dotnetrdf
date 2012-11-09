/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.ComponentModel;
using System.Net;
using VDS.RDF.Configuration;

namespace VDS.RDF.Utilities.StoreManager.Connections
{
    /// <summary>
    /// Abstract Base Class for connection definitions that are HTTP based and thus may allow for a HTTP proxied connection
    /// </summary>
    public abstract class BaseHttpConnectionDefinition
        : BaseConnectionDefinition
    {
        /// <summary>
        /// Creates a new Definition
        /// </summary>
        /// <param name="storeName">Store Name</param>
        /// <param name="storeDescrip">Store Description</param>
        /// <param name="t">Type of generated connection instances</param>
        public BaseHttpConnectionDefinition(String storeName, String storeDescrip, Type t)
            : base(storeName, storeDescrip, t) { }

        /// <summary>
        /// Gets/Sets the Proxy Server
        /// </summary>
        [Connection(DisplayName = "Proxy Server", DisplayOrder = 20, IsRequired = false, AllowEmptyString = true, Type = ConnectionSettingType.String, PopulateVia = ConfigurationLoader.PropertyProxy, PopulateFrom = ConfigurationLoader.PropertyServer)]
        public String ProxyServer
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the Proxy Username
        /// </summary>
        [Connection(DisplayName = "Proxy Username", DisplayOrder = 21, IsRequired = false, AllowEmptyString = false, Type = ConnectionSettingType.String, PopulateVia = ConfigurationLoader.PropertyProxy, PopulateFrom = ConfigurationLoader.PropertyUser)]
        public String ProxyUsername
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the Proxy Password
        /// </summary>
        [Connection(DisplayName = "Proxy Password", DisplayOrder = 22, IsRequired = false, AllowEmptyString = true, Type = ConnectionSettingType.Password, PopulateVia = ConfigurationLoader.PropertyProxy, PopulateFrom = ConfigurationLoader.PropertyPassword)]
        public String ProxyPassword
        {
            get;
            set;
        }

        /// <summary>
        /// Gets whether the Connection should use a Proxy
        /// </summary>
        protected bool UseProxy
        {
            get
            {
                return !String.IsNullOrEmpty(this.ProxyServer);
            }
        }

        /// <summary>
        /// Gets the Proxy Server to use
        /// </summary>
        /// <returns></returns>
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

    /// <summary>
    /// Abstract Base Class for connection definitions that connect to some form of server regardless of the protocol involved
    /// </summary>
    public abstract class BaseServerConnectionDefinition
        : BaseConnectionDefinition
    {
        /// <summary>
        /// Creates a new Defintion
        /// </summary>
        /// <param name="storeName">Store Naem</param>
        /// <param name="storeDescrip">Store Description</param>
        /// <param name="t">Type of generated connection instances</param>
        public BaseServerConnectionDefinition(String storeName, String storeDescrip, Type t)
            : base(storeName, storeDescrip, t) { }

        /// <summary>
        /// Gets/Sets the Server
        /// </summary>
        [Connection(DisplayName = "Server", IsRequired = true, Type = ConnectionSettingType.String, DisplayOrder = -1, PopulateFrom = ConfigurationLoader.PropertyServer),
        DefaultValue("localhost")]
        public virtual String Server
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Abstract Base Class for connection definitions that are HTTP server based
    /// </summary>
    public abstract class BaseHttpServerConnectionDefinition
        : BaseHttpConnectionDefinition
    {
        /// <summary>
        /// Create a new definition
        /// </summary>
        /// <param name="storeName">Store Name</param>
        /// <param name="storeDescrip">Store Description</param>
        /// <param name="t">Type of generated connection instances</param>
        public BaseHttpServerConnectionDefinition(String storeName, String storeDescrip, Type t)
            : base(storeName, storeDescrip, t) { }

        /// <summary>
        /// Gets/Sets the Server
        /// </summary>
        [Connection(DisplayName = "Server", IsRequired = true, Type = ConnectionSettingType.String, DisplayOrder = -1, PopulateFrom = ConfigurationLoader.PropertyServer),
        DefaultValue("localhost")]
        public virtual String Server
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Abstract Base Class for connection definitions that connect to some form of server regardless of the protocol involved and require the user to provide credentials
    /// </summary>
    public abstract class BaseCredentialsRequiredServerConnectionDefinition
        : BaseServerConnectionDefinition
    {
        /// <summary>
        /// Creates a new definition
        /// </summary>
        /// <param name="storeName">Store Name</param>
        /// <param name="storeDescrip">Store Description</param>
        /// <param name="t">Type of generated connection instances</param>
        public BaseCredentialsRequiredServerConnectionDefinition(String storeName, String storeDescrip, Type t)
            : base(storeName, storeDescrip, t) { }

        /// <summary>
        /// Gets/Sets the Username
        /// </summary>
        [Connection(DisplayName = "Username", DisplayOrder = 10, IsRequired = true, AllowEmptyString = false, Type = ConnectionSettingType.String, PopulateFrom = ConfigurationLoader.PropertyUser)]
        public String Username
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the Password
        /// </summary>
        [Connection(DisplayName = "Password", DisplayOrder = 11, IsRequired = true, AllowEmptyString = true, Type = ConnectionSettingType.Password, PopulateFrom = ConfigurationLoader.PropertyPassword)]
        public String Password
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Abstract Base Class for connection definitions that connect to some form of server regardless of the protocol involved and the user may optionally provide credentials
    /// </summary>
    public abstract class BaseCredentialsOptionalServerConnectionDefinition
        : BaseServerConnectionDefinition
    {
        /// <summary>
        /// Creates a new definition
        /// </summary>
        /// <param name="storeName">Store Name</param>
        /// <param name="storeDescrip">Store Description</param>
        /// <param name="t">Type of generated connection instances</param>
        public BaseCredentialsOptionalServerConnectionDefinition(String storeName, String storeDescrip, Type t)
            : base(storeName, storeDescrip, t) { }

        /// <summary>
        /// Gets/Sets the Username
        /// </summary>
        [Connection(DisplayName = "Username", DisplayOrder = 10, IsRequired = false, AllowEmptyString = true, Type = ConnectionSettingType.String, PopulateFrom = ConfigurationLoader.PropertyUser)]
        public String Username
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the Password
        /// </summary>
        [Connection(DisplayName = "Password", DisplayOrder = 11, IsRequired = false, AllowEmptyString = true, Type = ConnectionSettingType.Password, PopulateFrom = ConfigurationLoader.PropertyPassword)]
        public String Password
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Abstract Base Class for connection definitions that are HTTP server based and require the user to provide credentials
    /// </summary>
    public abstract class BaseHttpCredentialsRequiredServerConnectionDefinition
        : BaseHttpServerConnectionDefinition
    {
        /// <summary>
        /// Creates a new definition
        /// </summary>
        /// <param name="storeName">Store Name</param>
        /// <param name="storeDescrip">Store Description</param>
        /// <param name="t">Type of generated connection instances</param>
        public BaseHttpCredentialsRequiredServerConnectionDefinition(String storeName, String storeDescrip, Type t)
            : base(storeName, storeDescrip, t) { }

        /// <summary>
        /// Gets/Sets the Username
        /// </summary>
        [Connection(DisplayName = "Username", DisplayOrder = 10, IsRequired = true, AllowEmptyString = true, Type = ConnectionSettingType.String, PopulateFrom = ConfigurationLoader.PropertyUser)]
        public String Username
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the Password
        /// </summary>
        [Connection(DisplayName = "Password", DisplayOrder = 11, IsRequired = true, AllowEmptyString = true, Type = ConnectionSettingType.Password, PopulateFrom = ConfigurationLoader.PropertyPassword)]
        public String Password
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Abstract Base Class for connection definitions that are HTTP server based and the user may optionally provide credentials
    /// </summary>
    public abstract class BaseHttpCredentialsOptionalServerConnectionDefinition
        : BaseHttpServerConnectionDefinition
    {
        /// <summary>
        /// Creates a new definition
        /// </summary>
        /// <param name="storeName">Store Name</param>
        /// <param name="storeDescrip">Store Description</param>
        /// <param name="t">Type of generated connection instances</param>
        public BaseHttpCredentialsOptionalServerConnectionDefinition(String storeName, String storeDescrip, Type t)
            : base(storeName, storeDescrip, t) { }

        /// <summary>
        /// Gets/Sets the Username
        /// </summary>
        [Connection(DisplayName = "Username", DisplayOrder = 10, IsRequired = false, AllowEmptyString = true, Type = ConnectionSettingType.String, PopulateFrom = ConfigurationLoader.PropertyUser)]
        public String Username
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the Password
        /// </summary>
        [Connection(DisplayName = "Password", DisplayOrder = 11, IsRequired = false, AllowEmptyString = true, Type = ConnectionSettingType.Password, PopulateFrom = ConfigurationLoader.PropertyPassword)]
        public String Password
        {
            get;
            set;
        }
    }
}
