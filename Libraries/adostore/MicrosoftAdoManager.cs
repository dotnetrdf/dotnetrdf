using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace VDS.RDF.Storage
{
    public class MicrosoftAdoManager 
        : BaseAdoSqlClientStore
    {
        private String _connString;

        public MicrosoftAdoManager(String server, String db, String user, String password)
            : base(CreateConnection(server, db, user, password))
        {
            this._connString = CreateConnectionString(server, db, user, password);
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

        protected override SqlCommand GetCommand()
        {
            return new SqlCommand();
        }

        protected override SqlParameter GetParameter(string name)
        {
            SqlParameter param = new SqlParameter();
            param.ParameterName = name;
            return param;
        }

        protected override SqlDataAdapter GetAdaptor()
        {
            return new SqlDataAdapter();
        }

        protected override int EnsureSetup(SqlConnection connection)
        {
            throw new NotImplementedException("Automatic Store Setup is not yet implemented");
        }
    }
}
