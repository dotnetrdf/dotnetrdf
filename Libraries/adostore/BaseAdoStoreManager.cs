using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace VDS.RDF.Storage
{
    public abstract class BaseAdoStore<TConn,TCommand,TParameter,TAdaptor,TException> : IDisposable
        where TConn : DbConnection
        where TCommand : DbCommand
        where TParameter : DbParameter
        where TAdaptor : DbDataAdapter
        where TException : DbException
    {
        private TConn _connection;

        #region Constructor and Destructor

        public BaseAdoStore(TConn connection)
        {
            this._connection = connection;
            this._connection.Open();

            //Do a Version Check
            this.CheckVersion();
        }

        /// <summary>
        /// Finalizer for the Store Manager which ensures the
        /// </summary>
        ~BaseAdoStore()
        {
            this.Dispose(false);
        }

        #endregion

        #region Abstract Implementation

        /// <summary>
        /// Gets a Command for sending SQL Commands to the underlying Database
        /// </summary>
        /// <returns></returns>
        protected abstract TCommand GetCommand();

        protected abstract TParameter GetParameter(String name);

        /// <summary>
        /// Gets an Adaptor for converting results from SQL queries on the underlying Database into a DataTable
        /// </summary>
        /// <returns></returns>
        protected abstract TAdaptor GetAdaptor();

        /// <summary>
        /// Ensures that the Database is setup and returns the Version of the Database Schema
        /// </summary>
        /// <param name="connection">Database Connection</param>
        /// <returns>The Version of the Database Schema</returns>
        protected abstract int EnsureSetup(TConn connection);

        #endregion

        #region Internal Implementation

        /// <summary>
        /// Checks the Version of the Store
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is intended for two purposes
        /// <ol>
        ///     <li>Future proofing so later versions of the library can add additional stored procedures to the database and the code can decide which are available to it</li>
        ///     <li>Detecting when users try to use the class to connect to legacy databases created with the old Schema which are not compatible with this code</li>
        /// </para>
        /// </remarks>
        public int CheckVersion()
        {
            try
            {
                TCommand cmd = this.GetCommand();
                cmd.CommandText = "GetVersion";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(this.GetParameter("RC"));
                cmd.Parameters["RC"].DbType = DbType.Int32;
                cmd.Parameters["RC"].Direction = ParameterDirection.ReturnValue;
                cmd.Connection = this._connection;
                cmd.ExecuteNonQuery();

                int version = (int)cmd.Parameters["RC"].Value;
                switch (version)
                {
                    case 1:
                        //OK
                        return version;
                    default:
                        throw new RdfStorageException("Unknown ADO Store Version");
                }
            }
            catch (TException ex)
            {
                //If we get an SQL Exception then it may mean we've been used to try to connect to a legacy
                //SQL Store so check this now
                try
                {
                    //Try the following SQL, if we are talking to a legacy store it will have the
                    //graphHash field which doesn't appear in the new database schema
                    this.ExecuteScalar("SELECT graphHash FROM GRAPHS WHERE graphID=1");

                    //If it executes succesfully then it's a legacy store
                    //REQ: Add a link to the documentation on upgrading
                    throw new RdfStorageException("The underlying Database appears to be a legacy SQL Store using the old dotNetRDF Store Format.  You may connect to this for the time being using one of the old ISqlIOManager implementations but should see the documentation at ?? with regards to upgrading your store");
                }
                catch (TException)
                {
                    //If this check errors then not a legacy store so may just be not set up yet
                    return this.EnsureSetup(this._connection);
                }
            }
        }

        /// <summary>
        /// Executes a Scalar Query on the Database
        /// </summary>
        /// <param name="query">SQL Query</param>
        /// <returns></returns>
        internal Object ExecuteScalar(String query)
        {
            TCommand cmd = this.GetCommand();
            cmd.CommandText = query;
            cmd.Connection = this._connection;

            return cmd.ExecuteScalar();
        }

        internal DbDataReader GetReader(String query)
        {
            return this.GetReader(query, CommandType.Text);
        }

        internal DbDataReader GetReader(String query, CommandType type)
        {
            TCommand cmd = this.GetCommand();
            cmd.CommandType = type;
            cmd.CommandText = query;
            cmd.Connection = this._connection;

            return cmd.ExecuteReader();
        }

        #endregion

        #region Dispose Logic

        /// <summary>
        /// Disposes of the Store Manager
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Disposes of the Store Manager
        /// </summary>
        /// <param name="disposing">Whether this was invoked by the Dispose() method (if not was invoked by the Finalizer)</param>
        private void Dispose(bool disposing)
        {
            if (disposing) GC.SuppressFinalize(this);

            this.DisposeInternal();
            if (this._connection != null)
            {
                this._connection.Close();
            }
        }

        /// <summary>
        /// Does any additional dispose actions required by derived implementations
        /// </summary>
        /// <remarks>
        /// Will be called <em>before</em> the Connection is closed so derived implementations may
        /// </remarks>
        protected virtual void DisposeInternal()
        {

        }

        #endregion
    }

    public abstract class BaseAdoSqlClientStore
        : BaseAdoStore<SqlConnection, SqlCommand, SqlParameter, SqlDataAdapter, SqlException>
    {
        public BaseAdoSqlClientStore(SqlConnection connection)
            : base(connection) { }
    }
}
