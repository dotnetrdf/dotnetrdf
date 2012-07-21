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
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections.BuiltIn
{
    /// <summary>
    /// Definition for connections to stores supporting the SPARQL Graph Store protocol
    /// </summary>
    public class SparqlGraphStoreConnectionDefinition
        : BaseHttpConnectionDefinition
    {
        /// <summary>
        /// Creates a new definition
        /// </summary>
        public SparqlGraphStoreConnectionDefinition()
            : base("SPARQL Graph Store", "Connect to a SPARQL Graph Store server which uses the new RESTful HTTP protocol for communicating with a Graph Store defined by SPARQL 1.1", typeof(SparqlHttpProtocolConnector)) { }

        /// <summary>
        /// Gets/Sets the Server
        /// </summary>
        [Connection(DisplayName = "Protocol Server", DisplayOrder = 1, IsRequired = true, AllowEmptyString = false, PopulateFrom = ConfigurationLoader.PropertyServer)]
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
                return new SparqlHttpProtocolConnector(this.Server, this.GetProxy());
            }
            else
            {
                return new SparqlHttpProtocolConnector(this.Server);
            }
        }
    }
}
