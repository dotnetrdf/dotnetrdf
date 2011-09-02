using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.Data.Sql.Clients.Cmd
{
    public abstract class BaseConnectAction
        : BaseManagementAction
    {
        private String _dbtype = "sql";
        protected String _server, _db, _username, _password;
        protected bool _encrypt = false;

        public BaseConnectAction(String name, String descrip)
            : base(name, descrip) { }

        public override int MinimumArguments
        {
            get 
            {
                return 2; 
            }
        }

        public sealed override void ShowUsage()
        {
            Console.WriteLine("rdfSqlStorage (" + this.Name + " mode)");
            Console.WriteLine(new String('=', 21 + this.Name.Length));
            Console.WriteLine();
            Console.WriteLine(this.Description);
            Console.WriteLine();
            Console.WriteLine("Usage is rdfSqlStorage " + this.Name + " server db [options]");
            Console.WriteLine();
            Console.WriteLine("Supported Options");
            Console.WriteLine("-----------------");
            Console.WriteLine();
            Console.WriteLine("-dbtype type");
            Console.WriteLine(" Sets the database type to connect to, supported values currently are: sql, azure");
            Console.WriteLine();
            Console.WriteLine("-encrypt");
            Console.WriteLine(" Sets that the connection to the database should be encrypted");
            Console.WriteLine();
            Console.WriteLine("-password pwd");
            Console.WriteLine(" Sets the password for connecting to the database");
            Console.WriteLine();
            Console.WriteLine("-user username");
            Console.WriteLine(" Sets the username for connecting to the database");
            this.ShowAdditionalOptions();
        }

        protected virtual void ShowAdditionalOptions() { }

        public override void Run(string[] args)
        {
            if (args.Length < 3)
            {
                this.ShowUsage();
                return;
            }

            //Check Server and Database are OK
            String config = args[1];
            if (args[1].Equals("-help"))
            {
                this.ShowUsage();
                return;
            }
            this._server = args[1];
            this._db = args[2];

            //Check for other options
            this.CheckOptions(args);

            try
            {
                switch (this._dbtype)
                {
                    case "sql":
                        if (this._username != null || this._password != null)
                        {
                            this.RunSql(new MicrosoftAdoManager(this._server, this._db, this._username, this._password, this._encrypt));
                        } 
                        else
                        {
                            this.RunSql(new MicrosoftAdoManager(this._server, this._db, this._encrypt));
                        }
                        break;

                    case "azure":
                        this.RunSql(new AzureAdoManager(this._server, this._db, this._username, this._password));
                        break;

                    default:
                        Console.Error.WriteLine("rdfSqlStorage: Error: Unknown Database Type");
                        break;
                }
            }
            catch (SqlException sqlEx)
            {
                Console.Error.WriteLine("rdfSqlStorage: Error: SQL Error occurred!");
                this.PrintErrorTrace(sqlEx);
            }
            catch (DbException dbEx)
            {
                Console.Error.WriteLine("rdfSqlStorage: Error: Database Exception occurred!");
                this.PrintErrorTrace(dbEx);
            }
            catch (RdfStorageException storeEx)
            {
                Console.Error.WriteLine("rdfSqlStorage: Error: RDF Storage Exception occurred!");
                this.PrintErrorTrace(storeEx);
            }
        }

        protected abstract void Run<TConn, TCommand, TParameter, TAdaptor, TException>(BaseAdoStore<TConn, TCommand, TParameter, TAdaptor, TException> manager)
            where TConn : DbConnection
            where TCommand : DbCommand
            where TParameter : DbParameter
            where TAdaptor : DbDataAdapter
            where TException : DbException;

        protected virtual void RunSqlClient(BaseAdoSqlClientStore manager)
        {
            this.Run<SqlConnection, SqlCommand, SqlParameter, SqlDataAdapter, SqlException>(manager);
        }

        protected virtual void RunSql(MicrosoftAdoManager manager)
        {
            this.RunSqlClient(manager);
        }

        protected virtual void RunAzure(AzureAdoManager manager)
        {
            this.RunSql(manager);
        }

        private void CheckOptions(String[] args)
        {
            if (args.Length >= 4)
            {
                for (int i = 3; i < args.Length; i++)
                {
                    String arg = args[i];
                    switch (arg)
                    {
                        case "-dbtype":
                            if (i < args.Length - 1)
                            {
                                i++;
                                switch (args[i])
                                {
                                    case "sql":
                                        this._dbtype = "sql";
                                        break;
                                    case "azure":
                                        this._dbtype = "azure";
                                        break;
                                    default:
                                        throw new Exception("Invalid value for -dbtype argument, supported values are: sql, azure");
                                }
                            }
                            else
                            {
                                throw new Exception("Invalid -dbtype switch, must specify the database type as the next argument after the -dbtype switch");
                            }
                            break;
                        case "-encrypt":
                            this._encrypt = true;
                            break;

                        case "-user":
                            if (i < args.Length - 1)
                            {
                                i++;
                                this._username = args[i];
                            }
                            else
                            {
                                throw new Exception("Invalid -user switch, must specify the username as the next argument after the -user switch");
                            }
                            break;

                        case "-password":
                            if (i < args.Length - 1)
                            {
                                i++;
                                this._password = args[i];
                            }
                            else
                            {
                                throw new Exception("Invalid -password switch, must specify the password as the next argument after the -password switch");
                            }
                            break;

                        default:
                            if (!this.CheckOption(args, i, out i))
                            {
                                Console.Error.WriteLine("rdfSqlStorage: Warning: Unknown Option " + arg + " ignored");
                            }
                            break;
                    }
                }
            }
        }

        protected virtual bool CheckOption(String[] args, int i, out int i2)
        {
            i2 = i;
            return false;
        }
    }
}
