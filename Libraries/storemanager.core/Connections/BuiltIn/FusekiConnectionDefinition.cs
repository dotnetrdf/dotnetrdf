/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.ComponentModel;
using System.Net;
using VDS.RDF.Configuration;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections.BuiltIn
{
    /// <summary>
    /// Definition for connections to Fuseki
    /// </summary>
    public class FusekiConnectionDefinition
        : BaseHttpConnectionDefinition
    {
        /// <summary>
        /// Creates a new definition
        /// </summary>
        public FusekiConnectionDefinition()
            : base("Fuseki", "Connect to a Fuseki Server which exposes SPARQL based access to any Jena based stores e.g. SDB and TDB.  The Server URI must be the /data URI of your dataset e.g. http://localhost:3030/dataset/data", typeof(FusekiConnector)) { }

        /// <summary>
        /// Gets/Sets the Server URI
        /// </summary>
        [Connection(DisplayName = "Server URI", IsRequired = true, Type = ConnectionSettingType.String, DisplayOrder = -1, PopulateFrom = ConfigurationLoader.PropertyServer),
DefaultValue("http://localhost:3030/dataset/data")]
        public String Server
        {
            get;
            set;
        }

        /// <summary>
        /// Opens the Connection
        /// </summary>
        /// <returns></returns>
        protected override IStorageProvider OpenConnectionInternal()
        {
            if (this.UseProxy)
            {
                return new FusekiConnector(this.Server, this.GetProxy());
            }
            return new FusekiConnector(this.Server);
        }

        /// <summary>
        /// Makes a copy of the current connection definition
        /// </summary>
        /// <returns>Copy of the connection definition</returns>
        public override IConnectionDefinition Copy()
        {
            FusekiConnectionDefinition definition = new FusekiConnectionDefinition();
            definition.Server = this.Server;
            definition.ProxyPassword = this.ProxyPassword;
            definition.ProxyUsername = this.ProxyUsername;
            definition.ProxyServer = this.ProxyServer;
            return definition;
        }

        public override string ToString()
        {
            return "[Fuseki] " + this.Server.ToSafeString();
        }
    }
}
