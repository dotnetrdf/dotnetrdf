/*

Copyright Robert Vesse 2009-11
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
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// An implementation of an ADO Store Manager for use with SQL Azure
    /// </summary>
    /// <remarks>
    /// Essentially just derived from the more general <see cref="MicrosoftAdoManager">MicrosoftAdoManager</see> with a constructor which reduces the amount of input you need to create an instance that will connect to SQL Azure successfully
    /// </remarks>
    public class AzureAdoManager
        : MicrosoftAdoManager
    {
        private const String AzureServerSuffix = ".database.windows.net";

        private String _azureServer, _azureUser;

        /// <summary>
        /// Creates a new Azure ADO Manager instance
        /// </summary>
        /// <param name="server">Server Name (this is the alphanumeric server name issues by the Azure platform which precedes <em>.database.windows.net</em> in the Server hostname)</param>
        /// <param name="db">Database</param>
        /// <param name="username">Username (will automatically have the @server appended to it)</param>
        /// <param name="password">Password</param>
        /// <param name="mode">Access Mode</param>
        /// <remarks>
        /// SQL Azure connections are always encrypted
        /// </remarks>
        public AzureAdoManager(String server, String db, String username, String password, AdoAccessMode mode)
            : base(server + AzureServerSuffix, db, username + "@" + server, password, true, mode)
        {
            this._azureServer = server;
            this._azureUser = username;
        }

        /// <summary>
        /// Creates a new Azure ADO Manager instance
        /// </summary>
        /// <param name="server">Server Name (this is the alphanumeric server name issues by the Azure platform which precedes <em>.database.windows.net</em> in the Server hostname)</param>
        /// <param name="db">Database</param>
        /// <param name="username">Username (will automatically have the @server appended to it)</param>
        /// <param name="password">Password</param>
        /// <remarks>
        /// SQL Azure connections are always encrypted
        /// </remarks>
        public AzureAdoManager(String server, String db, String username, String password)
            : this(server, db, username, password, AdoAccessMode.Streaming) { }

        /// <summary>
        /// Gets the Database Type
        /// </summary>
        public override string DatabaseType
        {
            get
            {
                return "Microsoft SQL Azure";
            }
        }

        /// <summary>
        /// Serializes the connection's configuration
        /// </summary>
        /// <param name="context">Configuration Serialization Context</param>
        public override void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            //Firstly need to ensure our object factory has been referenced
            context.EnsureObjectFactory(typeof(AdoObjectFactory));

            //Then serialize the actual configuration
            INode dnrType = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyType);
            INode rdfType = context.Graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            INode manager = context.NextSubject;
            INode rdfsLabel = context.Graph.CreateUriNode(new Uri(NamespaceMapper.RDFS + "label"));
            INode genericManager = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.ClassGenericManager);
            INode server = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyServer);
            INode db = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyDatabase);
            INode encrypt = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyEncryptConnection);

            context.Graph.Assert(new Triple(manager, rdfType, genericManager));
            context.Graph.Assert(new Triple(manager, rdfsLabel, context.Graph.CreateLiteralNode(this.ToString())));
            context.Graph.Assert(new Triple(manager, dnrType, context.Graph.CreateLiteralNode(this.GetType().FullName + ", dotNetRDF.Data.Sql")));
            context.Graph.Assert(new Triple(manager, server, context.Graph.CreateLiteralNode(this._azureServer)));
            context.Graph.Assert(new Triple(manager, db, context.Graph.CreateLiteralNode(this._db)));
            context.Graph.Assert(new Triple(manager, encrypt, this._encrypt.ToLiteral(context.Graph)));

            INode username = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyUser);
            INode pwd = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyPassword);
            context.Graph.Assert(new Triple(manager, username, context.Graph.CreateLiteralNode(this._azureUser)));
            context.Graph.Assert(new Triple(manager, pwd, context.Graph.CreateLiteralNode(this._password)));
        }

        /// <summary>
        /// Gets the String representation of the connection
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[ADO Store (SQL Azure)] '" + this._db + "' on '" + this._azureServer + "' as User '" + this._azureUser + "'";
        }
    }
}
