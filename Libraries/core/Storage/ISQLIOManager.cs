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
using System.Data.Common;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Interface for classes which provide the Read/Write functionality for a SQL Store of some description
    /// </summary>
    /// <remarks>
    /// <para>
    /// Designed to allow for arbitrary database formats to be used and then implemented by simply implementing this interface and providing it to the relevant classes which use SQL Stores.  It has also been designed such that 1 Manager instance may be used across multiple Graphs to facilitate the implementation of a <see cref="SqlTripleStore">SqlTripleStore</see>.
    /// </para>
    /// <para>
    /// The design of this interface assumes fairly fine grained data storage in the SQL Store, generally it will be necessary as a minimum to implement most of the methods (except perhaps the Namespace related methods).  If your SQL Store implements storage in a way that is not easy to support with this interface consider implementing the <see cref="IGenericIOManager">IGenericIOManager</see> interface instead.
    /// </para>
    /// <para>
    /// While the interface does not define any methods related to the creation/destruction of a Store we recommend that the constructor of implementations of this class should set up the necessary database tables if it determines that they do not exist.
    /// </para>
    /// </remarks>
    [Obsolete("The legacy SQL store format and related classes are officially deprecated - please see http://www.dotnetrdf.org?content.asp?pageID=dotNetRDF%20Store#migration for details on upgrading to the new ADO store format", false)]
    public interface ISqlIOManager : IDisposable
    {
        /// <summary>
        /// Gets/Sets whether the Manager should preserve its state (Object to ID maps and ID to Object maps) even if its Dispose method is called
        /// </summary>
        /// <remarks>
        /// <para>
        /// Useful if the Manager is used multiple times by classes which will invoke its Dispose method when they are Disposed of
        /// </para>
        /// <para>
        /// A user should set this property to true if an instance will be used multiple times and may get disposed of multiple times to cause it to preserve its state and not dispose itself.  Classes that use an ISqlIOManager will typically dispose of the manager in their <strong>Dispose()</strong> methods since these methods should rightly dispose of resources they no longer need.
        /// </para>
        /// </remarks>
        bool PreserveState
        {
            get;
            set;
        }

        /// <summary>
        /// Gets whether the Manager has completed any outstanding database operations
        /// </summary>
        /// <remarks>Useful if you need to wait for the Manager to complete and you don't know whether it's finished</remarks>
        bool HasCompleted
        {
            get;
        }

        /// <summary>
        /// Gets the Database Server this Manager connects to
        /// </summary>
        String DatabaseServer
        {
            get;
        }

        /// <summary>
        /// Gets the Database this Manager is using
        /// </summary>
        String DatabaseName
        {
            get;
        }

        /// <summary>
        /// Gets the Database Username this Manager is using
        /// </summary>
        String DatabaseUser
        {
            get;
        }

        /// <summary>
        /// Gets the Database Password this Manager is using
        /// </summary>
        String DatabasePassword
        {
            get;
        }

        /// <summary>
        /// Gets the ID for a Graph in the Store, creates a new Graph in the store if necessary
        /// </summary>
        /// <param name="graphUri">Graph Uri</param>
        /// <returns></returns>
        String GetGraphID(Uri graphUri);

        /// <summary>
        /// Gets the URI for a Graph based on the ID throwing an error if the Graph doesn't exist
        /// </summary>
        /// <param name="graphID">ID</param>
        /// <returns></returns>
        Uri GetGraphUri(String graphID);

        /// <summary>
        /// Determines whether a given Graph exists in the Store
        /// </summary>
        /// <param name="graphUri">Graph Uri</param>
        /// <returns></returns>
        bool Exists(Uri graphUri);

        /// <summary>
        /// Gets the URIs of the Graphs in the Store
        /// </summary>
        /// <returns></returns>
        List<Uri> GetGraphUris();

        /// <summary>
        /// Loads Namespaces for the Graph with the given ID into the given Graph object
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphID">Database Graph ID</param>
        void LoadNamespaces(IGraph g, String graphID);

        /// <summary>
        /// Loads Triples for the Graph with the given ID into the given Graph object
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphID">Database Graph ID</param>
        void LoadTriples(IGraph g, String graphID);

        /// <summary>
        /// Loads a Node from the Database into the relevant Graph
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="nodeID">Database Node ID</param>
        INode LoadNode(IGraph g, String nodeID);

        /// <summary>
        /// Gets the Node ID for a Node in the Database creating a new Database record if necessary
        /// </summary>
        /// <param name="n">Node to Save</param>
        /// <returns></returns>
        String SaveNode(INode n);

        /// <summary>
        /// Saves a Triple from a Graph into the Database
        /// </summary>
        /// <param name="t">Triple to save</param>
        /// <param name="graphID">Database Graph ID</param>
        void SaveTriple(Triple t, String graphID);

        /// <summary>
        /// Saves a Namespace from a Graph into the Database
        /// </summary>
        /// <param name="prefix">Namespace Prefix</param>
        /// <param name="u">Namespace Uri</param>
        /// <param name="graphID">Database Graph ID</param>
        void SaveNamespace(String prefix, Uri u, String graphID);

        /// <summary>
        /// Removes a Triple from a Graphs Database record
        /// </summary>
        /// <param name="t">Triple to remove</param>
        /// <param name="graphID">Database Graph ID</param>
        void RemoveTriple(Triple t, String graphID);

        /// <summary>
        /// Removes a Namespace from a Graphs Database record
        /// </summary>
        /// <param name="prefix">Namespace Prefix</param>
        /// <param name="u">Namespace Uri</param>
        /// <param name="graphID">Database Graph ID</param>
        void RemoveNamespace(String prefix, Uri u, String graphID);

        /// <summary>
        /// Removes a Graph from the Database
        /// </summary>
        /// <param name="graphID">Database Graph ID</param>
        void RemoveGraph(String graphID);

        /// <summary>
        /// Changes the Uri associated with the Prefix in a Graphs Database record
        /// </summary>
        /// <param name="prefix">Namespace Prefix</param>
        /// <param name="u">New Namespace Uri</param>
        /// <param name="graphID">Database Graph ID</param>
        void UpdateNamespace(String prefix, Uri u, String graphID);

        /// <summary>
        /// Clears all the Namespaces and Triples for the given Graph from the Database
        /// </summary>
        void ClearGraph(String graphID);

        /// <summary>
        /// Opens a Connection to the Database
        /// </summary>
        /// <param name="keepOpen">Indicates that the Connection should be kept open and a Transaction started</param>
        void Open(bool keepOpen);

        /// <summary>
        /// Closes the Connection to the Database
        /// </summary>
        /// <param name="forceClose">Indicates that the connection should be closed even if keepOpen was specified when the Connection was opened</param>
        void Close(bool forceClose);

        /// <summary>
        /// Closes the Connection to the Database
        /// </summary>
        /// <param name="forceClose">Indicates that the connection should be closed even if keepOpen was specified when the Connection was opened</param>
        /// <param name="rollbackTrans">Indicates that the Transaction should be rolled back because something has gone wrong</param>
        void Close(bool forceClose, bool rollbackTrans);

        /// <summary>
        /// Executes a Non-Query SQL Command against the database
        /// </summary>
        /// <param name="sqlCmd">SQL Command</param>
        void ExecuteNonQuery(String sqlCmd);

        /// <summary>
        /// Executes a Query SQL Command against the database and returns a DataTable
        /// </summary>
        /// <param name="sqlCmd">SQL Command</param>
        /// <returns>DataTable of results</returns>
        DataTable ExecuteQuery(String sqlCmd);

        /// <summary>
        /// Returns a DataReader that can be used to read results from a query in a streaming fashion
        /// </summary>
        /// <param name="sqlCmd">SQL Command</param>
        /// <returns></returns>
        DbDataReader ExecuteStreamingQuery(String sqlCmd);

        /// <summary>
        /// Executes a Query SQL Command against the database and returns the scalar result (first column of first row of the result)
        /// </summary>
        /// <param name="sqlCmd">SQL Command</param>
        /// <returns>First Column of First Row of the Results</returns>
        Object ExecuteScalar(String sqlCmd);

        /// <summary>
        /// Escapes Strings in a manner appropriate to the underlying Database
        /// </summary>
        /// <param name="text">String to escape</param>
        /// <returns>Escaped String</returns>
        String EscapeString(String text);

        /// <summary>
        /// Flushes any outstanding changes to the underlying SQL database
        /// </summary>
        void Flush();
    }

    /// <summary>
    /// Interface for classes which provide the Read/Write functionality for a SQL Store in a Thread Safe way
    /// </summary>
    [Obsolete("The legacy SQL store format and related classes are officially deprecated - please see http://www.dotnetrdf.org?content.asp?pageID=dotNetRDF%20Store#migration for details on upgrading to the new ADO store format", false)]
    public interface IThreadedSqlIOManager : ISqlIOManager
    {
        /// <summary>
        /// Gets/Sets whether Database Transactions should be disabled
        /// </summary>
        bool DisableTransactions
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Marker interface for classes which implement SQL Stores using the dotNetRDF Store Format
    /// </summary>
    [Obsolete("The legacy SQL store format and related classes are officially deprecated - please see http://www.dotnetrdf.org?content.asp?pageID=dotNetRDF%20Store#migration for details on upgrading to the new ADO store format", false)]
    public interface IDotNetRDFStoreManager : ISqlIOManager, IGenericIOManager
    {

    }
}

#endif