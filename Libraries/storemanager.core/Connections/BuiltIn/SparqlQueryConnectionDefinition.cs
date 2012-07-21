/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
        [Connection(DisplayName = "Endpoint URI", DisplayOrder = 1, IsRequired = true, AllowEmptyString = false, PopulateVia = ConfigurationLoader.PropertyEndpoint, PopulateFrom = ConfigurationLoader.PropertyEndpointUri),
         DefaultValue("http://example.org/sparql")]
        public String EndpointUri
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the Default Graph URI
        /// </summary>
        [Connection(DisplayName = "Default Graph", DisplayOrder = 2, IsRequired = false, AllowEmptyString = true, PopulateVia = ConfigurationLoader.PropertyEndpoint, PopulateFrom = ConfigurationLoader.PropertyDefaultGraphUri)]
        public String DefaultGraphUri
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the Load Method
        /// </summary>
        [Connection(DisplayName = "Load Method", DisplayOrder = 3, Type = ConnectionSettingType.Enum, PopulateVia = ConfigurationLoader.PropertyEndpoint, PopulateFrom = ConfigurationLoader.PropertyLoadMode),
         DefaultValue(SparqlConnectorLoadMethod.Construct)]
        public SparqlConnectorLoadMethod LoadMode
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
            SparqlRemoteEndpoint endpoint = String.IsNullOrEmpty(this.DefaultGraphUri) ? new SparqlRemoteEndpoint(new Uri(this.EndpointUri)) : new SparqlRemoteEndpoint(new Uri(this.EndpointUri), this.DefaultGraphUri);
            if (this.UseProxy)
            {
                endpoint.Proxy = this.GetProxy();
            }
            return new SparqlConnector(endpoint, this.LoadMode);
        }
    }
}
