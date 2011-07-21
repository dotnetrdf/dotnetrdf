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
        private String _server, _db, _user, _password;
        private IEnumerable<IAlgebraOptimiser> _optimisers;

        public MicrosoftAdoManager(String server, String db, String user, String password)
            : base(CreateConnection(server, db, user, password))
        {
            this._connString = CreateConnectionString(server, db, user, password);
            this._server = server;
            this._db = db;
            this._user = user;
            this._password = password;
        }

        public MicrosoftAdoManager(String db, String user, String password)
            : this("localhost", db, user, password) { }

        public MicrosoftAdoManager(String db)
            : this("localhost", db, null, null) { }

        public MicrosoftAdoManager(String server, String db)
            : this(server, db, null, null) { }

        private static String CreateConnectionString(String server, String db, String user, String password)
        {
            if (server == null) throw new ArgumentNullException("server", "Server cannot be null, use localhost for a server on the local machine or use an overload which does not take the server argument");
            if (db == null) throw new ArgumentNullException("db", "Database cannot be null");
            if (user == null && password != null) throw new ArgumentNullException("user", "User cannot be null if password is specified, use null for both arguments to use Windows Integrated Authentication");
            if (user != null && password == null) throw new ArgumentNullException("password", "Password cannot be null if user is specified, use null for both arguments to use Windows Integrated Authentication or use String.Empty for password if you have no password");

            if (user != null && password != null)
            {
                return "Data Source=" + server + ";Initial Catalog=" + db + ";User ID=" + user + ";Password=" + password + ";MultipleActiveResultSets=True;";
            }
            else
            {
                return "Data Source=" + server + ";Initial Catalog=" + db + ";Trusted_Connection=True;MultipleActiveResultSets=True;";
            }
        }

        private static SqlConnection CreateConnection(String server, String db, String user, String password)
        {
            return new SqlConnection(CreateConnectionString(server, db, user, password));
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

        protected override int EnsureSetup(SqlConnection connection)
        {
            Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("VDS.RDF.Storage.CreateMicrosoftAdoStore.sql");
            if (s != null)
            {
                String commands;
                using (StreamReader reader = new StreamReader(s))
                {
                    commands = reader.ReadToEnd();
                    reader.Close();
                }

                SqlCommand cmd = this.GetCommand();
                cmd.Connection = this.Connection as SqlConnection;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = commands;
                try
                {
                    cmd.ExecuteNonQuery();

                    //Now try and add the user to the rdf_readwrite role
                    if (this._user != null)
                    {
                        try
                        {
                            cmd.CommandText = "EXEC sp_addrolemember 'rdf_readwrite', @user;";
                            cmd.Parameters.Add(this.GetParameter("user"));
                            cmd.Parameters["user"].Value = this._user;
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
                throw new RdfStorageException("Unable to setup ADO Store for Microsoft SQL Server as database creation script is missing from the DLL");
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

            context.Graph.Assert(new Triple(manager, rdfType, genericManager));
            context.Graph.Assert(new Triple(manager, rdfsLabel, context.Graph.CreateLiteralNode(this.ToString())));
            context.Graph.Assert(new Triple(manager, dnrType, context.Graph.CreateLiteralNode(this.GetType().FullName + ", dotNetRDF.Data.Sql")));
            context.Graph.Assert(new Triple(manager, server, context.Graph.CreateLiteralNode(this._server)));
            context.Graph.Assert(new Triple(manager, db, context.Graph.CreateLiteralNode(this._db)));

            if (this._user != null && this._password != null)
            {
                INode username = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyUser);
                INode pwd = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyPassword);
                context.Graph.Assert(new Triple(manager, username, context.Graph.CreateLiteralNode(this._user)));
                context.Graph.Assert(new Triple(manager, pwd, context.Graph.CreateLiteralNode(this._password)));
            }
        }

        public override string ToString()
        {
            if (this._user != null)
            {
                return "[ADO Store (MS SQL)] '" + this._db + "' on '" + this._server + "' as User '" + this._user + "'";
            }
            else
            {
                return "[ADO Store (MS SQL)] '" + this._db + "' on '" + this._server + "'";
            }
        }
    }
}
