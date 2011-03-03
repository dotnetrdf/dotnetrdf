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
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager
{
    public delegate void OperationProgress(int operationsCompleted);

    public delegate void OperationError(Exception ex);

    public abstract class BaseDBConnection
    {
        protected String _server, _dbname, _user, _pwd;
        protected int _port;
        protected ISqlIOManager _manager = null;

        public BaseDBConnection(String server, String db, String user, String password) : this(server, -1, db, user, password) { }
     
        public BaseDBConnection(String server, int port, String db, String user, String password) {
            this._server = server;
            this._port = port;
            this._dbname = db;
            this._user = user;
            this._pwd = password;
        }

        public abstract void Connect();

        public abstract void Disconnect();

        public abstract bool IsConnected
        {
            get;
        }

        public abstract String Version();

        public abstract bool IsDotNetRDFStore();

        public abstract void CreateStore();

        public abstract void DropStore();

        public abstract Object ExecuteScalar(String sqlCmd);

        public abstract void ExecuteNonQuery(String sqlCmd);

        public abstract DataTable ExecuteQuery(String sqlCmd);

        public virtual ISqlIOManager Manager
        {
            get
            {
                if (this._manager != null)
                {
                    return this._manager;
                }
                else
                {
                    this.InitialiseManager();
                    return this._manager;
                }
            }
        }

        public virtual IThreadedSqlIOManager ThreadedManager
        {
            get
            {
                if (this._manager != null)
                {
                    if (this._manager is IThreadedSqlIOManager)
                    {
                        return (IThreadedSqlIOManager)this._manager;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    this.InitialiseManager();
                    return this.ThreadedManager;
                }
            }
        }

        protected abstract void InitialiseManager();

        public override string ToString()
        {
            return "Database '" + this._dbname + "' on '" + this._server;
        }
    }
}
