using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Optimisation;

namespace VDS.RDF.Storage
{
    public class MicrosoftAdoManager 
        : BaseAdoSqlClientStore
    {
        private String _connString;
        protected String _server, _db, _user, _password;
        protected bool _encrypt = false;
        private IEnumerable<IAlgebraOptimiser> _optimisers;

        public const String DefaultServer = "localhost";

        public MicrosoftAdoManager(String server, String db, String user, String password, bool encrypt)
            : base(CreateConnectionParameters(server, db, user, password, encrypt))
        {
            this._connString = CreateConnectionString(server, db, user, password, encrypt);
            this._server = server;
            this._db = db;
            this._user = user;
            this._password = password;
            this._encrypt = encrypt;
        }

        public MicrosoftAdoManager(String server, String db, String user, String password)
            : this(server, db, user, password, false) { }

        public MicrosoftAdoManager(String db, String user, String password, bool encrypt)
            : this(DefaultServer, db, user, password, encrypt) { }

        public MicrosoftAdoManager(String db, String user, String password)
            : this(db, user, password, false) { }

        public MicrosoftAdoManager(String server, String db, bool encrypt)
            : this(server, db, null, null, encrypt) { }

        public MicrosoftAdoManager(String server, String db)
            : this(server, db, false) { }

        public MicrosoftAdoManager(String db, bool encrypt)
            : this(DefaultServer, db, encrypt) { }

        public MicrosoftAdoManager(String db)
            : this(db, false) { }

        private static String CreateConnectionString(String server, String db, String user, String password, bool encrypt)
        {
            if (server == null) throw new ArgumentNullException("server", "Server cannot be null, use localhost for a server on the local machine or use an overload which does not take the server argument");
            if (db == null) throw new ArgumentNullException("db", "Database cannot be null");
            if (user == null && password != null) throw new ArgumentNullException("user", "User cannot be null if password is specified, use null for both arguments to use Windows Integrated Authentication");
            if (user != null && password == null) throw new ArgumentNullException("password", "Password cannot be null if user is specified, use null for both arguments to use Windows Integrated Authentication or use String.Empty for password if you have no password");

            String enc = encrypt ? "Encrypt=True;" : String.Empty;

            if (user != null && password != null)
            {
                return "Data Source=" + server + ";Initial Catalog=" + db + ";User ID=" + user + ";Password=" + password + ";MultipleActiveResultSets=True;" + enc;
            }
            else
            {
                return "Data Source=" + server + ";Initial Catalog=" + db + ";Trusted_Connection=True;MultipleActiveResultSets=True;" + enc;
            }
        }

        private static Dictionary<String,String> CreateConnectionParameters(String server, String db, String user, String password, bool encrypt)
        {
            Dictionary<String, String> ps = new Dictionary<String, String>();
            ps.Add("server", server);
            ps.Add("db", db);
            ps.Add("user", user);
            ps.Add("password", password);
            ps.Add("encrypt", encrypt.ToString());
            return ps;
        }

        protected override SqlConnection CreateConnection(Dictionary<string,string> parameters)
        {
            return new SqlConnection(CreateConnectionString(parameters["server"], parameters["db"], parameters["user"], parameters["password"], Boolean.Parse(parameters["encrypt"])));
        }

        protected internal override SqlCommand GetCommand()
        {
            return new SqlCommand();
        }

        protected internal override SqlParameter GetParameter(string name)
        {
            SqlParameter param = new SqlParameter();
            param.ParameterName = name;
            return param;
        }

        protected internal override SqlDataAdapter GetAdaptor()
        {
            return new SqlDataAdapter();
        }

        protected override int CheckForUpgrades(int currVersion)
        {
            switch (currVersion)
            {
                case 1:
                    //Note - In future versions this may take upgrade actions on the database
                    return currVersion;
                default:
                    return currVersion;
            }
        }

        protected override int EnsureSetup(Dictionary<String,String> parameters)
        {
            AdoSchemaDefinition definition = AdoSchemaHelper.DefaultSchema;
            if (definition == null) throw new RdfStorageException("Unable to setup ADO Store for Microsoft SQL Server as no default schema is currently available from AdoSchemaHelper.DefaultSchema");
            if (!definition.HasScript(AdoSchemaScriptType.Create, AdoSchemaScriptDatabase.MicrosoftSqlServer)) throw new RdfStorageException("Unable to setup ADO Store for Microsoft SQL Server as the current default schema does not provide a compatible creation script");

            String resource = definition.GetScript(AdoSchemaScriptType.Create, AdoSchemaScriptDatabase.MicrosoftSqlServer);
            if (resource == null) throw new RdfStorageException("Unable to setup ADO Store for Microsoft SQL Server as the default schema returned a null resource for the creation script");

            //Get the appropriate assembly
            Assembly assm;
            if (resource.Contains(","))
            {
                //Assembly qualified name so need to do an Assembly.Load()
                String assmName = resource.Substring(resource.IndexOf(",") + 1).TrimStart();
                resource = resource.Substring(0, resource.IndexOf(",")).TrimEnd();
                try
                {
                    assm = Assembly.Load(assmName);
                }
                catch (Exception ex)
                {
                    throw new RdfStorageException("Unable to setup ADO Store for Microsoft SQL Server as the creation script is the resource '" + resource + "' from assembly '" + assmName + "' but this assembly could not be loaded, please see inner exception for details", ex);
                }
            }
            else
            {
                //Assume executing assembly
                assm = Assembly.GetExecutingAssembly();
            }

            Stream s = assm.GetManifestResourceStream(resource);
            if (s != null)
            {
                try
                {
                    this.ExecuteSql(s);

                    //Now try and add the user to the rdf_readwrite role
                    if (parameters["user"] != null)
                    {
                        try
                        {
                            SqlCommand cmd = new SqlCommand();
                            cmd.Connection = this.Connection;
                            cmd.CommandText = "EXEC sp_addrolemember 'rdf_readwrite', @user;";
                            cmd.Parameters.Add(this.GetParameter("user"));
                            cmd.Parameters["user"].Value = parameters["user"];
                            cmd.ExecuteNonQuery();

                            //Succeeded - return 1
                            return 1;
                        }
                        catch (SqlException sqlEx)
                        {
                            throw new RdfStorageException("ADO Store database for Microsoft SQL Server was created successfully but we were unable to add you to an appropriate ADO Store role automatically.  Please manually add yourself to one of the following roles - rdf_admin, rdf_readwrite, rdf_readinsert or rdf_readonly - before attempting to use the store", sqlEx);
                        }
                    }
                    else
                    {
                        throw new RdfStorageException("ADO Store database for Microsoft SQL Server was created successfully but you are using a trusted connection so the system was unable to add you to an appropriate ADO Store role.  Please manually add yourself to one of the following roles - rdf_admin, rdf_readwrite, rdf_readinsert or rdf_readonly - before attempting to use the store");
                    }
                }
                catch (SqlException sqlEx)
                {
                    throw new RdfStorageException("Failed to create ADO Store database for Microsoft SQL Server due to errors executing the creation script, please see inner exception for details.", sqlEx);
                }
            }
            else
            {
                throw new RdfStorageException("Unable to setup ADO Store for Microsoft SQL Server as database creation script is missing from the referenced assembly");
            }
        }

        protected override ISparqlDataset GetDataset()
        {
            return new MicrosoftAdoDataset(this);
        }

        protected override IEnumerable<IAlgebraOptimiser> GetOptimisers()
        {
            if (this._optimisers == null)
            {
                this._optimisers = new IAlgebraOptimiser[] { new SimpleVirtualAlgebraOptimiser(this) };
            }
            return this._optimisers;
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
            context.Graph.Assert(new Triple(manager, server, context.Graph.CreateLiteralNode(this._server)));
            context.Graph.Assert(new Triple(manager, db, context.Graph.CreateLiteralNode(this._db)));
            if (this._encrypt) context.Graph.Assert(new Triple(manager, encrypt, this._encrypt.ToLiteral(context.Graph)));

            if (this._user != null && this._password != null)
            {
                INode username = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyUser);
                INode pwd = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyPassword);
                context.Graph.Assert(new Triple(manager, username, context.Graph.CreateLiteralNode(this._user)));
                context.Graph.Assert(new Triple(manager, pwd, context.Graph.CreateLiteralNode(this._password)));
            }
        }

        /// <summary>
        /// Gets the string representation of the connection
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this._user != null)
            {
                return "[ADO Store (SQL Server)] '" + this._db + "' on '" + this._server + "' as User '" + this._user + "'";
            }
            else
            {
                return "[ADO Store (SQL Server)] '" + this._db + "' on '" + this._server + "'";
            }
        }
    }
}
