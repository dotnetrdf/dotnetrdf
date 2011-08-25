using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;

namespace VDS.RDF.Storage
{
    public class AzureAdoManager
        : MicrosoftAdoManager
    {
        private const String AzureServerSuffix = ".database.windows.net";

        private String _azureServer, _azureUser;

        public AzureAdoManager(String server, String db, String username, String password)
            : base(server + AzureServerSuffix, db, username + "@" + server, password, true)
        {
            this._azureServer = server;
            this._azureUser = username;
        }

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

        public override string ToString()
        {
            return "[ADO Store (SQL Azure)] '" + this._db + "' on '" + this._azureServer + "' as User '" + this._azureUser + "'";
        }
    }
}
