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
using VDS.RDF.Configuration;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections.BuiltIn
{
    /// <summary>
    /// Definition for connections to Stardog 3.x
    /// </summary>
    public class StardogV3ConnectionDefinition
        : BaseStardogConnectionDefinition
    {
        /// <summary>
        /// Creates a new definition
        /// </summary>
        public StardogV3ConnectionDefinition()
            : base("Stardog 3.x", "Connect to a Stardog 3.x database exposed via the Stardog HTTP server", typeof(StardogConnector))
        {
            this.ReasoningMode = StardogReasoningMode.DatabaseControlled;
        }

        /// <summary>
        /// Opens the connection
        /// </summary>
        /// <returns></returns>
        protected override IStorageProvider OpenConnectionInternal()
        {
            if (this.UseAnonymousAccount)
            {
                if (this.UseProxy)
                {
                    return new StardogConnector(this.Server, this.StoreID, BaseStardogConnector.AnonymousUser, BaseStardogConnector.AnonymousUser, this.GetProxy());
                }

                return new StardogConnector(this.Server, this.StoreID, BaseStardogConnector.AnonymousUser, BaseStardogConnector.AnonymousUser);
            }
            if (this.UseProxy)
            {
                return new StardogConnector(this.Server, this.StoreID, this.Username, this.Password, this.GetProxy());
            }

            return new StardogConnector(this.Server, this.StoreID, this.Username, this.Password);
        }

        /// <summary>
        /// Gets/Sets the Reasoning Mode for queries
        /// </summary>
        public override StardogReasoningMode ReasoningMode { get; set; }

        /// <summary>
        /// Makes a copy of the current connection definition
        /// </summary>
        /// <returns>Copy of the connection definition</returns>
        public override IConnectionDefinition Copy()
        {
            StardogV3ConnectionDefinition definition = new StardogV3ConnectionDefinition();
            definition.Server = this.Server;
            definition.StoreID = this.StoreID;
            definition.ReasoningMode = StardogReasoningMode.DatabaseControlled;
            definition.UseAnonymousAccount = this.UseAnonymousAccount;
            definition.ProxyPassword = this.ProxyPassword;
            definition.ProxyUsername = this.ProxyUsername;
            definition.ProxyServer = this.ProxyServer;
            definition.Username = this.Username;
            definition.Password = this.Password;
            return definition;
        }
    }

    /// <summary>
    /// Definition for connections to Stardog 2.x
    /// </summary>
    public class StardogV2ConnectionDefinition
        : BaseStardogConnectionDefinition
    {
        /// <summary>
        /// Creates a new definition
        /// </summary>
        public StardogV2ConnectionDefinition()
            : base("Stardog 2.x", "Connect to a Stardog 2.x database exposed via the Stardog HTTP server", typeof (StardogConnector))
        {
        }

        /// <summary>
        /// Opens the connection
        /// </summary>
        /// <returns></returns>
        protected override IStorageProvider OpenConnectionInternal()
        {
            if (this.UseAnonymousAccount)
            {
                if (this.UseProxy)
                {
                    return new StardogV2Connector(this.Server, this.StoreID, this.ReasoningMode, BaseStardogConnector.AnonymousUser, BaseStardogConnector.AnonymousUser, this.GetProxy());
                }

                return new StardogV2Connector(this.Server, this.StoreID, this.ReasoningMode, BaseStardogConnector.AnonymousUser, BaseStardogConnector.AnonymousUser);
            }
            if (this.UseProxy)
            {
                return new StardogV2Connector(this.Server, this.StoreID, this.ReasoningMode, this.Username, this.Password, this.GetProxy());
            }

            return new StardogV2Connector(this.Server, this.StoreID, this.ReasoningMode, this.Username, this.Password);
        }

        /// <summary>
        /// Makes a copy of the current connection definition
        /// </summary>
        /// <returns>Copy of the connection definition</returns>
        public override IConnectionDefinition Copy()
        {
            StardogV2ConnectionDefinition definition = new StardogV2ConnectionDefinition();
            definition.Server = this.Server;
            definition.StoreID = this.StoreID;
            definition.ReasoningMode = this.ReasoningMode;
            definition.UseAnonymousAccount = this.UseAnonymousAccount;
            definition.ProxyPassword = this.ProxyPassword;
            definition.ProxyUsername = this.ProxyUsername;
            definition.ProxyServer = this.ProxyServer;
            definition.Username = this.Username;
            definition.Password = this.Password;
            return definition;
        }
    }

        /// <summary>
    /// Definition for connections to Stardog 1.x
    /// </summary>
    public class StardogV1ConnectionDefinition
        : BaseStardogConnectionDefinition
    {
        /// <summary>
        /// Creates a new definition
        /// </summary>
        public StardogV1ConnectionDefinition()
            : base("Stardog 1.x", "Connect to a Stardog 1.x database exposed via the Stardog HTTP server", typeof (StardogConnector))
        {
        }

        /// <summary>
        /// Gets/Sets the Server URI
        /// </summary>
        [Connection(DisplayName = "Server URI", IsRequired = true, AllowEmptyString = false, DisplayOrder = -1, PopulateFrom = ConfigurationLoader.PropertyServer),
         DefaultValue("http://localhost:5822/")]
        public override String Server
        {
            get { return base.Server; }
            set { base.Server = value; }
        }

        /// <summary>
        /// Opens the connection
        /// </summary>
        /// <returns></returns>
        protected override IStorageProvider OpenConnectionInternal()
        {
            if (this.UseAnonymousAccount)
            {
                if (this.UseProxy)
                {
                    return new StardogV1Connector(this.Server, this.StoreID, this.ReasoningMode, BaseStardogConnector.AnonymousUser, BaseStardogConnector.AnonymousUser, this.GetProxy());
                }

                return new StardogV1Connector(this.Server, this.StoreID, this.ReasoningMode, BaseStardogConnector.AnonymousUser, BaseStardogConnector.AnonymousUser);
            }
            if (this.UseProxy)
            {
                return new StardogV1Connector(this.Server, this.StoreID, this.ReasoningMode, this.Username, this.Password, this.GetProxy());
            }

            return new StardogV1Connector(this.Server, this.StoreID, this.ReasoningMode, this.Username, this.Password);
        }

        /// <summary>
        /// Makes a copy of the current connection definition
        /// </summary>
        /// <returns>Copy of the connection definition</returns>
        public override IConnectionDefinition Copy()
        {
            StardogV1ConnectionDefinition definition = new StardogV1ConnectionDefinition();
            definition.Server = this.Server;
            definition.StoreID = this.StoreID;
            definition.ReasoningMode = this.ReasoningMode;
            definition.UseAnonymousAccount = this.UseAnonymousAccount;
            definition.ProxyPassword = this.ProxyPassword;
            definition.ProxyUsername = this.ProxyUsername;
            definition.ProxyServer = this.ProxyServer;
            definition.Username = this.Username;
            definition.Password = this.Password;
            return definition;
        }
    }
}