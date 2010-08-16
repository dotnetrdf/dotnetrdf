/*

Copyright Robert Vesse 2009-10
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

If this license is not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using VDS.RDF.Storage;

namespace dotNetRDFStore
{
    public class MicrosoftSQLServerConnection : BaseDBConnection
    {
        private SqlConnection _db;

        public MicrosoftSQLServerConnection(String server, String db, String user, String password)
            : base(server, db, user, password)
        {
            this._db = new SqlConnection();
            this._db.ConnectionString = "Data Source=" + this._server + ";Initial Catalog=" + this._dbname + ";User ID=" + this._user + ";Password=" + this._pwd;
        }

        public override void Connect()
        {
            this._db.Open();
        }

        public override void Disconnect()
        {
            switch (this._db.State) {
                case ConnectionState.Open:
                case ConnectionState.Connecting:
                    this._db.Close();
                    break;
            }
        }

        public override bool IsConnected
        {
            get 
            {
                switch (this._db.State)
                {
                    case ConnectionState.Open:
                        return true;
                    default:
                        return false;
                }
            }
        }

        public override string Version()
        {
            if (!this.IsDotNetRDFStore())
            {
                return "N/A";
            }
            else
            {
                try
                {
                    String version011Test = "SELECT COUNT(graphHash) AS TotalHashes FROM GRAPHS";
                    Object result = this.ExecuteScalar(version011Test);

                    return "0.1.1";
                }
                catch
                {
                    return "0.1.0";
                }
            }
        }

        public override bool IsDotNetRDFStore()
        {
            String[] tests = new String[] { 
                "SELECT * FROM GRAPHS WHERE graphID=1",
                "SELECT * FROM NAMESPACES WHERE nsID=1",
                "SELECT * FROM NS_PREFIXES WHERE nsPrefixID=1",
                "SELECT * FROM NS_URIS WHERE nsUriID=1",
                "SELECT * FROM GRAPH_TRIPLES WHERE graphID=1",
                "SELECT * FROM TRIPLES WHERE tripleID=1",
                "SELECT * FROM NODES WHERE nodeID=1"
            };

            //If all the above SQL Commands work then this is a dotNetRDF Store
            try
            {
                Object result;
                foreach (String test in tests)
                {
                    result = this.ExecuteScalar(test);
                }

                //All worked OK without error
                return true;
            }
            catch
            {
                //Some error so one/more of the required tables doesn't exist
                return false;
            }
        }

        public override void CreateStore()
        {
            if (!this.IsDotNetRDFStore())
            {
                //Read the Setup SQL Script into a local string (it's an embedded resource in the assembly)
                StreamReader reader = new StreamReader(Assembly.GetAssembly(typeof(VDS.RDF.Graph)).GetManifestResourceStream("VDS.RDF.Storage.CreateMSSQLStoreTables.sql"));
                String setup = reader.ReadToEnd();
                reader.Close();

                this.ExecuteNonQuery(setup);
            }
        }

        public override void DropStore()
        {
            if (this.IsDotNetRDFStore())
            {
                //Read the Drop SQL Script into a local string
                StreamReader reader = new StreamReader(Assembly.GetAssembly(typeof(VDS.RDF.Graph)).GetManifestResourceStream("VDS.RDF.Storage.DropMSSQLStoreTables.sql"));
                String drop = reader.ReadToEnd();
                reader.Close();

                this.ExecuteNonQuery(drop);
            }
        }

        public override object ExecuteScalar(string sqlCmd)
        {
            SqlCommand cmd = new SqlCommand(sqlCmd, this._db);
            return cmd.ExecuteScalar();
        }

        public override void ExecuteNonQuery(string sqlCmd)
        {
            SqlCommand cmd = new SqlCommand(sqlCmd, this._db);
            cmd.ExecuteNonQuery();
        }

        public override DataTable ExecuteQuery(string sqlCmd)
        {
            SqlCommand cmd = new SqlCommand(sqlCmd, this._db);
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);

            DataTable results = new DataTable();
            adapter.Fill(results);

            return results;
        }

        protected override void InitialiseManager()
        {
            this._manager = new MicrosoftSqlStoreManager(this._server, this._dbname, this._user, this._pwd);
        }

        public override string ToString()
        {
            return "[SQL Server] Database '" + this._dbname + "' on '" + this._server;
        }
    }
}
