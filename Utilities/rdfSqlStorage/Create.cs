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
    public class Create
        : BaseConnectAction
    {
        private String _dbtype = "sql";
        private String _username, _password;
        private bool _encrypt = false;

        public Create()
            : base("create", "Connects to an ADO store, creating it if necessary and reports the current schema and version") { }

        protected override void Run<TConn,TCommand,TParameter,TAdaptor,TException>(BaseAdoStore<TConn,TCommand,TParameter,TAdaptor,TException> manager)
        {
            try
            {
                Console.WriteLine("rdfSqlStorage: Connected OK");
                int version = manager.CheckVersion();
                Console.WriteLine("rdfSqlStorage: Version " + version);
                String schema = manager.CheckSchema();
                Console.WriteLine("rdfSqlStorage: Schema " + schema);
            }
            finally
            {
                manager.Dispose();
            }
        }
    }
}
