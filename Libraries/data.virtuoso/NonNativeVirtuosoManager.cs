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

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

#if !NO_DATA && !NO_STORAGE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading;
using System.Reflection;
using System.IO;
using OpenLink.Data.Virtuoso;
using VDS.RDF.Configuration;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// A <see cref="ISqlIOManager">ISqlIOManager</see> implementation which uses a Virtual Database from Virtuoso Universal Server as the backing store
    /// </summary>
    /// <threadsafety instance="true">
    /// <para>
    /// Designed to be Thread safe for concurrent read and write access
    /// </para>
    /// <para>
    /// <strong>Note:</strong> To ensure correct behaviour for multi-threaded writing you must set the <see cref="BaseStoreManager.DisableTransactions">DisableTransactions</see> property to be true.  Classes which are designed specifically to do multi-threaded writing will <em>usually</em> set this automatically.
    /// </para>
    /// <para>
    /// Will only be thread safe for writing if all the classes writing to the database are using a single instance of this Manager.
    /// </para>
    /// </threadsafety>
    /// <remarks>
    /// Since there is no way of knowing what the underlying Database of a Virtual Database in Virtuoso is this Manager cannot automatically create the dotNetRDF Store since it does not know which SQL Setup script to use.  You <strong>must</strong> configure your virtual database yourself before using this Manager to access it.
    /// </remarks>
    [Obsolete("The legacy SQL store format and related classes are officially deprecated - please see http://www.dotnetrdf.org?content.asp?pageID=dotNetRDF%20Store#migration for details on upgrading to the new ADO store format", false)]
    public class NonNativeVirtuosoManager : MicrosoftSqlStoreManager, IConfigurationSerializable
    {
        private Dictionary<int, VirtuosoConnection> _dbConnections = new Dictionary<int, VirtuosoConnection>();
        private Dictionary<int, VirtuosoTransaction> _dbTrans = new Dictionary<int, VirtuosoTransaction>();
        private Dictionary<int, bool> _dbKeepOpen = new Dictionary<int, bool>();

        private int _dbport = 1111;

        /// <summary>
        /// Creates a new instance of the Non-Native Virtuoso Manager
        /// </summary>
        /// <param name="server">Server</param>
        /// <param name="port">Port</param>
        /// <param name="db">Database Name</param>
        /// <param name="user">Username</param>
        /// <param name="password">Password</param>
        public NonNativeVirtuosoManager(String server, int port, String db, String user, String password)
            : base()
        {
            //Fill in Database Properties
            this._dbserver = server;
            this._dbport = port;
            this._dbname = db;
            this._dbuser = user;
            this._dbpwd = password;

            //This Manager doesn't create the Store since it doesn't know what the Virtual Databases supported SQL Syntax is
        }

        /// <summary>
        /// Creates a new instance of the Non-Native Virtuoso Manager
        /// </summary>
        /// <param name="server">Server</param>
        /// <param name="db">Database Name</param>
        /// <param name="user">Username</param>
        /// <param name="password">Password</param>
        /// <remarks>Assumes the Database uses the default port of <strong>1111</strong></remarks>
        public NonNativeVirtuosoManager(String server, String db, String user, String password)
            : this(server, 1111, db, user, password) { }

        /// <summary>
        /// Creates a new instance of the Non-Native Virtuoso Manager
        /// </summary>
        /// <param name="db">Database Name</param>
        /// <param name="user">Username</param>
        /// <param name="password">Password</param>
        /// <remarks>Assumes the Database is on the <strong>localhost</strong> and uses the default port of <strong>1111</strong></remarks>
        public NonNativeVirtuosoManager(String db, String user, String password)
            : this("localhost", 1111, db, user, password) { }


        #region Database IO (Thread Safe)

        /// <summary>
        /// Opens a Connection to the Database
        /// </summary>
        /// <param name="keepOpen">Indicates that the Connection should be kept open and a Transaction started</param>
        /// <remarks>A Connection and Transaction per Thread are used</remarks>
        public override void Open(bool keepOpen)
        {
            //Get Thread ID and setup a Connection for this Thread if needed
            int thread = Thread.CurrentThread.ManagedThreadId;
            try
            {
                Monitor.Enter(this._dbConnections);
                Monitor.Enter(this._dbTrans);
                Monitor.Enter(this._dbKeepOpen);
                if (!this._dbConnections.ContainsKey(thread))
                {
                    this._dbConnections.Add(thread, new VirtuosoConnection());
                    this._dbConnections[thread].ConnectionString = "Server=" + this._dbserver + ":" + this._dbport + ";Database=" + this._dbname + ";uid=" + this._dbuser + ";pwd=" + this._dbpwd;
                    this._dbTrans.Add(thread, null);
                    this._dbKeepOpen.Add(thread, false);
                }
            }
            finally
            {
                Monitor.Exit(this._dbConnections);
                Monitor.Exit(this._dbTrans);
                Monitor.Exit(this._dbKeepOpen);
            }

            switch (this._dbConnections[thread].State)
            {
                case ConnectionState.Broken:
                case ConnectionState.Closed:
                    this._dbConnections[thread].Open();

                    //Start a Transaction
                    if (this._dbTrans[thread] == null && !this._noTrans)
                    {
                        this._dbTrans[thread] = this._dbConnections[thread].BeginTransaction();
                    }
                    break;
            }
            if (keepOpen) this._dbKeepOpen[thread] = true;
        }

        /// <summary>
        /// Closes the Connection to the Database
        /// </summary>
        /// <param name="forceClose">Indicates that the connection should be closed even if keepOpen was specified when the Connection was opened</param>
        /// <param name="rollbackTrans">Indicates that the Transaction should be rolled back because something has gone wrong</param>
        /// <remarks>A Connection and Transaction per Thread are used</remarks>
        public override void Close(bool forceClose, bool rollbackTrans)
        {
            //Get Thread ID
            int thread = Thread.CurrentThread.ManagedThreadId;
            if (!this._dbKeepOpen.ContainsKey(thread)) return;

            //Don't close if we're keeping open and not forcing Close or rolling back a Transaction
            if (this._dbKeepOpen[thread] && !forceClose && !rollbackTrans)
            {
                return;
            }

            switch (this._dbConnections[thread].State)
            {
                case ConnectionState.Open:
                    //Finish the Transaction if exists
                    if (this._dbTrans[thread] != null)
                    {
                        lock (this._dbTrans[thread])
                        {
                            if (!rollbackTrans)
                            {
                                //Commit normally
                                this._dbTrans[thread].Commit();
                            }
                            else
                            {
                                //Want to Rollback
                                this._dbTrans[thread].Rollback();
                            }
                            this._dbTrans[thread] = null;
                        }
                    }
                    this._dbConnections[thread].Close();

                    this._dbKeepOpen[thread] = false;
                    break;
            }
        }

        /// <summary>
        /// Executes a Non-Query SQL Command against the database
        /// </summary>
        /// <param name="sqlCmd">SQL Command</param>
        public override void ExecuteNonQuery(string sqlCmd)
        {
            //Get Thread ID
            int thread = Thread.CurrentThread.ManagedThreadId;

            //Create the SQL Command
            VirtuosoCommand cmd = new VirtuosoCommand(sqlCmd, this._dbConnections[thread]);
            if (this._dbTrans[thread] != null)
            {
                //Add to the Transaction if required
                cmd.Transaction = this._dbTrans[thread];
            }

            //Execute
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes a Query SQL Command against the database and returns a DataTable
        /// </summary>
        /// <param name="sqlCmd">SQL Command</param>
        /// <returns>DataTable of results</returns>
        public override DataTable ExecuteQuery(string sqlCmd)
        {
            //Get Thread ID
            int thread = Thread.CurrentThread.ManagedThreadId;

            //Create the SQL Command
            VirtuosoCommand cmd = new VirtuosoCommand(sqlCmd, this._dbConnections[thread]);
            if (this._dbTrans[thread] != null)
            {
                //Add to the Transaction if required
                cmd.Transaction = this._dbTrans[thread];
            }

            //Execute the Query
            VirtuosoDataAdapter adapter = new VirtuosoDataAdapter(cmd);
            DataTable results = new DataTable();
            adapter.Fill(results);

            return results;
        }

        /// <summary>
        /// Executes a Query SQL Command against the database and fills the supplied DataTable with the results
        /// </summary>
        /// <param name="sqlCmd">SQL Command</param>
        /// <param name="data">DataTable to fill with results</param>
        /// <remarks>Allows for queries which wish to strongly type the results for quicker reading</remarks>
        protected override void ExecuteQuery(string sqlCmd, DataTable data)
        {
            //Get Thread ID
            int thread = Thread.CurrentThread.ManagedThreadId;

            //Create the SQL Command
            VirtuosoCommand cmd = new VirtuosoCommand(sqlCmd, this._dbConnections[thread]);
            if (this._dbTrans[thread] != null)
            {
                //Add to the Transaction if required
                cmd.Transaction = this._dbTrans[thread];
            }

            //Execute the Query
            VirtuosoDataAdapter adapter = new VirtuosoDataAdapter(cmd);
            adapter.Fill(data);
        }

        /// <summary>
        /// Executes a Query SQL Command against the database and returns the results in a streaming fashion
        /// </summary>
        /// <param name="sqlCmd">SQL Command</param>
        public override System.Data.Common.DbDataReader ExecuteStreamingQuery(string sqlCmd)
        {
            //Get Thread ID
            int thread = Thread.CurrentThread.ManagedThreadId;

            //Create the SQL Command
            VirtuosoCommand cmd = new VirtuosoCommand(sqlCmd, this._dbConnections[thread]);
            if (this._dbTrans[thread] != null)
            {
                //Add to the Transaction if required
                cmd.Transaction = this._dbTrans[thread];
            }

            //Return the Data Reader
            return cmd.ExecuteReader();
        }

        /// <summary>
        /// Executes a Query SQL Command against the database and returns the scalar result (first column of first row of the result)
        /// </summary>
        /// <param name="sqlCmd">SQL Command</param>
        /// <returns>First Column of First Row of the Results</returns>
        public override object ExecuteScalar(string sqlCmd)
        {
            //Get Thread ID
            int thread = Thread.CurrentThread.ManagedThreadId;

            //Create the SQL Command
            VirtuosoCommand cmd = new VirtuosoCommand(sqlCmd, this._dbConnections[thread]);
            if (this._dbTrans[thread] != null)
            {
                //Add to the Transaction if required
                cmd.Transaction = this._dbTrans[thread];
            }

            //Execute the Scalar
            return cmd.ExecuteScalar();
        }

        #endregion

        /// <summary>
        /// Serializes the connection's configuration
        /// </summary>
        /// <param name="context">Configuration Serialization Context</param>
        public override void SerializeConfiguration(VDS.RDF.Configuration.ConfigurationSerializationContext context)
        {
            INode manager = context.NextSubject;
            context.NextSubject = manager;
            base.SerializeConfiguration(context);

            if (this._dbport != VirtuosoManager.DefaultPort)
            {
                INode port = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyPort);
                context.Graph.Assert(new Triple(manager, port, context.Graph.CreateLiteralNode(this._dbport.ToString())));
            }
        }
    }
}

#endif