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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using VDS.RDF.Configuration;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections.BuiltIn
{
    /// <summary>
    /// Supported Stardog versions
    /// </summary>
    public enum StardogVersion
    {
        /// <summary>
        /// Version 1.x
        /// </summary>
        Version1,
        /// <summary>
        /// Version 2.x
        /// </summary>
        Version2
    }

    /// <summary>
    /// Definition for connections to Stardog
    /// </summary>
    public class StardogConnectionDefinition
        : BaseHttpServerConnectionDefinition
    {
        /// <summary>
        /// Creates a new definition
        /// </summary>
        public StardogConnectionDefinition()
            : base("Stardog", "Connect to a Stardog database exposed via the Stardog HTTP server", typeof (StardogConnector))
        {
            this.Version = StardogVersion.Version2;
        }

        /// <summary>
        /// Gets/Sets the Stardog version to use
        /// </summary>
        [Connection(DisplayName="Stardog Version", IsRequired=true, DisplayOrder=-2, Type=ConnectionSettingType.Enum),
         DefaultValue(StardogVersion.Version2)]
        public StardogVersion Version { get; set; }

        /// <summary>
        /// Gets/Sets the Server URI
        /// </summary>
        [Connection(DisplayName="Server URI", IsRequired=true, AllowEmptyString=false, DisplayOrder=-1, PopulateFrom=ConfigurationLoader.PropertyServer),
         DefaultValue("http://localhost:5820/")]
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

        /// <summary>
        /// Gets/Sets the Store ID
        /// </summary>
        [Connection(DisplayName = "Store ID", DisplayOrder = 1, AllowEmptyString = false, IsRequired = true, PopulateFrom = ConfigurationLoader.PropertyStore)]
        public String StoreID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the Reasoning Mode for queries
        /// </summary>
        [Connection(DisplayName="Query Reasoning Mode", DisplayOrder=2, Type=ConnectionSettingType.Enum, PopulateFrom = ConfigurationLoader.PropertyLoadMode),
         DefaultValue(StardogReasoningMode.None)]
        public StardogReasoningMode ReasoningMode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the Username
        /// </summary>
        [Connection(DisplayName = "Username", DisplayOrder = 3, IsRequired = false, AllowEmptyString = true, PopulateFrom = ConfigurationLoader.PropertyUser)]
        public String Username
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the Password
        /// </summary>
        [Connection(DisplayName = "Password", DisplayOrder = 4, IsRequired = false, AllowEmptyString = true, Type = ConnectionSettingType.Password, PopulateFrom = ConfigurationLoader.PropertyPassword)]
        public String Password
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets whether to use the Stardog anonymous account
        /// </summary>
        [Connection(DisplayName="Use the Stardog default anonymous user account instead of an explicit Username and Password?", DisplayOrder=5, Type=ConnectionSettingType.Boolean),
         DefaultValue(false)]
        public bool UseAnonymousAccount
        {
            get;
            set;
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
                    switch (this.Version)
                    {
                        case StardogVersion.Version1:
                            return new StardogV1Connector(this.Server, this.StoreID, this.ReasoningMode, BaseStardogConnector.AnonymousUser, BaseStardogConnector.AnonymousUser, this.GetProxy());
                        default:
                            return new StardogConnector(this.Server, this.StoreID, this.ReasoningMode, BaseStardogConnector.AnonymousUser, BaseStardogConnector.AnonymousUser, this.GetProxy());
                    }
                }
                else
                {
                    switch (this.Version)
                    {
                        case StardogVersion.Version1:
                            return new StardogV1Connector(this.Server, this.StoreID, this.ReasoningMode, BaseStardogConnector.AnonymousUser, BaseStardogConnector.AnonymousUser);
                        default:
                            return new StardogConnector(this.Server, this.StoreID, this.ReasoningMode, BaseStardogConnector.AnonymousUser, BaseStardogConnector.AnonymousUser);
                    }
                }
            }
            else
            {
                if (this.UseProxy)
                {
                    switch (this.Version)
                    {
                        case StardogVersion.Version1:
                            return new StardogV1Connector(this.Server, this.StoreID, this.ReasoningMode, this.Username, this.Password, this.GetProxy());
                        default:
                            return new StardogConnector(this.Server, this.StoreID, this.ReasoningMode, this.Username, this.Password, this.GetProxy());
                    }
                }
                else
                {
                    switch (this.Version)
                    {
                        case StardogVersion.Version1:
                            return new StardogV1Connector(this.Server, this.StoreID, this.ReasoningMode, this.Username, this.Password);
                        default:
                            return new StardogConnector(this.Server, this.StoreID, this.ReasoningMode, this.Username, this.Password);
                    }
                }
            }
        }
    }
}
