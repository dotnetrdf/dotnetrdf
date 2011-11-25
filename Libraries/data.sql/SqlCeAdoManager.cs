using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using VDS.RDF.Configuration;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Storage
{
    public class SqlCeAdoManager
        : BaseAdoStore<SqlCeConnection, SqlCeCommand, SqlCeParameter, SqlCeDataAdapter, SqlCeException>
    {
        private String _filename;

        public SqlCeAdoManager()
            : base(new Dictionary<string, string>(), AdoAccessMode.Streaming) { }

        public SqlCeAdoManager(String filename)
            : base(SqlCeAdoManager.CreateConnectionParameters(filename), AdoAccessMode.Streaming) { }

        private static Dictionary<String,String> CreateConnectionParameters(String filename)
        {
            Dictionary<String,String> ps = new Dictionary<string,string>();
            ps.Add("filename", filename);
            return ps;
        }

        protected override SqlCeConnection CreateConnection(Dictionary<string, string> parameters)
        {
            if (parameters.ContainsKey("filename"))
            {
                this._filename = parameters["filename"];
                if (!File.Exists(this._filename))
                {
                    SqlCeEngine engine = new SqlCeEngine("Data Source=" + this._filename + ";");
                    engine.CreateDatabase();
                }
                return new SqlCeConnection("Data Source=" + this._filename + ";");
            }
            else
            {
                return new SqlCeConnection();
            }
        }

        protected internal override SqlCeCommand GetCommand()
        {
            return new SqlCeCommand();
        }

        protected internal override SqlCeParameter GetParameter(string name)
        {
            return new SqlCeParameter(name, null);
        }

        protected internal override SqlCeDataAdapter GetAdapter()
        {
            return new SqlCeDataAdapter();
        }

        protected override int EnsureSetup(Dictionary<string, string> parameters)
        {
            try
            {
                AdoSchemaHelper.DefaultSchema = AdoSchemaHelper.GetSchema("Simple");
                AdoSchemaDefinition definition = AdoSchemaHelper.DefaultSchema;
                if (definition == null) throw new RdfStorageException("Unable to setup ADO Store for SQL Server Compact as no default schema is currently available from AdoSchemaHelper.DefaultSchema");
                if (!definition.HasScript(AdoSchemaScriptType.Create, AdoSchemaScriptDatabase.MicrosoftSqlServer)) throw new RdfStorageException("Unable to setup ADO Store for SQL Server Compact as the current default schema does not provide a compatible creation script");

                String resource = definition.GetScript(AdoSchemaScriptType.Create, AdoSchemaScriptDatabase.MicrosoftSqlServer);
                if (resource == null) throw new RdfStorageException("Unable to setup ADO Store for SQL Server Compact as the default schema returned a null resource for the creation script");

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
                        throw new RdfStorageException("Unable to setup ADO Store for SQL Server Compact as the creation script is the resource '" + resource + "' from assembly '" + assmName + "' but this assembly could not be loaded, please see inner exception for details", ex);
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
                                SqlCeCommand cmd = new SqlCeCommand();
                                cmd.Connection = this.Connection;
                                cmd.CommandText = "EXEC sp_addrolemember 'rdf_readwrite', @user;";
                                cmd.Parameters.Add(this.GetParameter("user"));
                                cmd.Parameters["user"].Value = parameters["user"];
                                cmd.ExecuteNonQuery();

                                //Succeeded - return 1
                                return 1;
                            }
                            catch (SqlCeException sqlEx)
                            {
                                throw new RdfStorageException("ADO Store database for SQL Server Compact was created successfully but we were unable to add you to an appropriate ADO Store role automatically.  Please manually add yourself to one of the following roles - rdf_admin, rdf_readwrite, rdf_readinsert or rdf_readonly - before attempting to use the store", sqlEx);
                            }
                        }
                        else
                        {
                            throw new RdfStorageException("ADO Store database for SQL Server Compact was created successfully but you are using a trusted connection so the system was unable to add you to an appropriate ADO Store role.  Please manually add yourself to one of the following roles - rdf_admin, rdf_readwrite, rdf_readinsert or rdf_readonly - before attempting to use the store");
                        }
                    }
                    catch (SqlCeException sqlEx)
                    {
                        throw new RdfStorageException("Failed to create ADO Store database for SQL Server Compact due to errors executing the creation script, please see inner exception for details.", sqlEx);
                    }
                }
                else
                {
                    throw new RdfStorageException("Unable to setup ADO Store for SQL Server Compact as database creation script is missing from the referenced assembly");
                }
            }
            finally
            {
                AdoSchemaHelper.DefaultSchema = AdoSchemaHelper.GetSchema("Hash");
            }
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

        public override string DatabaseType
        {
            get 
            {
                return "SQL Server Compact"; 
            }
        }

        protected override VDS.RDF.Query.Datasets.ISparqlDataset GetDataset()
        {
            return new SqlCeAdoDataset(this);
        }

        public override void SerializeConfiguration(VDS.RDF.Configuration.ConfigurationSerializationContext context)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "[ADO Store (SQL CE)] " + this._filename;
        }
    }
}
