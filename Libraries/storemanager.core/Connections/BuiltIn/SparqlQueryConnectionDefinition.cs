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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using VDS.RDF.Configuration;
using VDS.RDF.Query;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections.BuiltIn
{
    /// <summary>
    /// Definition for read-only connection to SPARQL endpoints
    /// </summary>
    public class SparqlQueryConnectionDefinition
        : BaseHttpConnectionDefinition
    {
        /// <summary>
        /// Creates a new definition
        /// </summary>
        public SparqlQueryConnectionDefinition()
            : base("SPARQL Query", "Connect to any SPARQL Query endpoint as a read-only connection", typeof(SparqlConnector)) { }

        /// <summary>
        /// Gets/Sets the Endpoint URI
        /// </summary>
        [Connection(DisplayName = "Endpoint URI", DisplayOrder = 1, IsRequired = true, AllowEmptyString = false, PopulateVia = ConfigurationLoader.PropertyQueryEndpoint, PopulateFrom = ConfigurationLoader.PropertyQueryEndpointUri),
         DefaultValue("http://example.org/sparql")]
        public String EndpointUri
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the Default Graph URI
        /// </summary>
        [Connection(DisplayName = "Default Graph", DisplayOrder = 2, IsRequired = false, AllowEmptyString = true, PopulateVia = ConfigurationLoader.PropertyQueryEndpoint, PopulateFrom = ConfigurationLoader.PropertyDefaultGraphUri)]
        public String DefaultGraphUri
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the Load Method
        /// </summary>
        [Connection(DisplayName = "Load Method", DisplayOrder = 3, Type = ConnectionSettingType.Enum, PopulateVia = ConfigurationLoader.PropertyQueryEndpoint, PopulateFrom = ConfigurationLoader.PropertyLoadMode),
         DefaultValue(SparqlConnectorLoadMethod.Construct)]
        public SparqlConnectorLoadMethod LoadMode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets whether local parsing of queries should be skipped
        /// </summary>
        [Connection(DisplayName = "Skip Local Parsing?", DisplayOrder = 4, Type = ConnectionSettingType.Boolean, PopulateFrom = ConfigurationLoader.PropertySkipParsing),
         DefaultValue(false)]
        public bool SkipLocalParsing { get; set; }

        /// <summary>
        /// Opens the connection
        /// </summary>
        /// <returns></returns>
        protected override IStorageProvider OpenConnectionInternal()
        {
            SparqlRemoteEndpoint endpoint = String.IsNullOrEmpty(this.DefaultGraphUri) ? new SparqlRemoteEndpoint(new Uri(this.EndpointUri)) : new SparqlRemoteEndpoint(new Uri(this.EndpointUri), this.DefaultGraphUri);
            if (this.UseProxy)
            {
                endpoint.Proxy = this.GetProxy();
            }
            SparqlConnector connector = new SparqlConnector(endpoint, this.LoadMode);
            connector.SkipLocalParsing = this.SkipLocalParsing;
            return connector;
        }

        /// <summary>
        /// Makes a copy of the current connection definition
        /// </summary>
        /// <returns>Copy of the connection definition</returns>
        public override IConnectionDefinition Copy()
        {
            SparqlQueryConnectionDefinition definition = new SparqlQueryConnectionDefinition();
            definition.EndpointUri = this.EndpointUri;
            definition.DefaultGraphUri = this.DefaultGraphUri;
            definition.LoadMode = this.LoadMode;
            definition.SkipLocalParsing = this.SkipLocalParsing;
            definition.ProxyPassword = this.ProxyPassword;
            definition.ProxyUsername = this.ProxyUsername;
            definition.ProxyServer = this.ProxyServer;
            return definition;
        }

        public override string ToString()
        {
            return "[SPARQL Query] " + this.EndpointUri.ToSafeString();
        }
    }
}
