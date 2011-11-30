/*

Copyright Robert Vesse 2009-11
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Storage.Virtualisation;
using VDS.RDF.Update;
using VDS.RDF.Writing;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Possible modes of operation for accessing an ADO Store
    /// </summary>
    public enum AdoAccessMode
    {
        /// <summary>
        /// Streaming is the default mode based upon <see cref="DbDataReader">DbDataReader</see> usage, uses the least memory but performs poorly when the network distance between the client and server is large
        /// </summary>
        Streaming,
        /// <summary>
        /// Batched is the alternative mode based upon <see cref="DbDataAdapter">DbDataAdapter</see> usage, this uses more memory but performs much better when the network distance between the client and server is large
        /// </summary>
        Batched
    }

    /// <summary>
    /// Abstract Base implementation of the ADO Store
    /// </summary>
    /// <typeparam name="TConn">Connection Type</typeparam>
    /// <typeparam name="TCommand">Command Type</typeparam>
    /// <typeparam name="TParameter">Parameter Type</typeparam>
    /// <typeparam name="TAdapter">Adapter Type</typeparam>
    /// <typeparam name="TException">Exception Type</typeparam>
    /// <remarks>
    /// <para>
    /// The ADO Store is a complete redesign of our SQL backed storage that does everything via stored procedures which provides a much better level of abstraction between the code and the SQL database schema.  This allows the database schemas to be flexible and take adavantage of the features of different SQL backends, we ship with three default ADO Schemas, use the <see cref="AdoSchemaHelper">AdoSchemaHelper</see> class to get information about these.
    /// </para>
    /// <para>
    /// This code cannot communicate with legacy SQL Stores and this is by design, please see <a href="http://www.dotnetrdf.org?content.asp?pageID=dotNetRDF%20Store#migration">this page</a> for details on migrating legacy stores
    /// </para>
    /// </remarks>
    public abstract class BaseAdoStore<TConn,TCommand,TParameter,TAdapter,TException> 
        : IUpdateableGenericIOManager, IVirtualRdfProvider<int, int>, IConfigurationSerializable, IDisposable
        where TConn : DbConnection
        where TCommand : DbCommand
        where TParameter : DbParameter
        where TAdapter : DbDataAdapter
        where TException : Exception
    {
        private int _version = 1;
        private AdoAccessMode _accessMode = AdoAccessMode.Streaming;
        private String _schema = "Hash";
        private TConn _connection;
        private SimpleVirtualNodeCache<int> _cache = new SimpleVirtualNodeCache<int>();
        private NodeFactory _factory = new NodeFactory();
        private ISparqlDataset _dataset;
        private SparqlQueryParser _queryParser;
        private LeviathanQueryProcessor _queryProcessor;
        private SparqlUpdateParser _updateParser;
        private LeviathanUpdateProcessor _updateProcessor;
        private Dictionary<String, String> _parameters;
        private Dictionary<int, Uri> _graphUris = new Dictionary<int, Uri>();

        #region Constructor and Destructor

        /// <summary>
        /// Creates a new ADO Store
        /// </summary>
        /// <param name="parameters">Parameters for the connection</param>
        /// <param name="accessMode">Access Mode</param>
        public BaseAdoStore(Dictionary<String,String> parameters, AdoAccessMode accessMode)
        {
            this._parameters = parameters;
            this._connection = this.CreateConnection(parameters);
            this._connection.Open();
            this._accessMode = accessMode;

            //Do a Version and Schema Check
            this._version = this.CheckVersion();
            this._schema = this.CheckSchema();
        }

        /// <summary>
        /// Finalizer for the ADO Store Manager which ensures that it is disposed of properly
        /// </summary>
        ~BaseAdoStore()
        {
            this.Dispose(false);
        }

        #endregion

        #region Abstract Implementation

        /// <summary>
        /// Method that will be called by the constructor to allow the derived class to instantiate the connection
        /// </summary>
        /// <param name="parameters">Parameters</param>
        /// <returns></returns>
        protected abstract TConn CreateConnection(Dictionary<String, String> parameters);

        /// <summary>
        /// Gets a Command for sending SQL Commands to the underlying Database
        /// </summary>
        /// <returns></returns>
        protected internal abstract TCommand GetCommand();

        /// <summary>
        /// Gets a Parameter with the given name for sending parameters with SQL Commands to the underlying Database
        /// </summary>
        /// <param name="name">Parameter Name</param>
        /// <returns></returns>
        protected internal abstract TParameter GetParameter(String name);

        /// <summary>
        /// Gets an Adaptor for converting results from SQL queries on the underlying Database into a DataTable
        /// </summary>
        /// <returns></returns>
        protected internal abstract TAdapter GetAdapter();

        /// <summary>
        /// Ensures that the Database is setup and returns the Version of the Database Schema
        /// </summary>
        /// <param name="parameters">Parameters for the connection</param>
        /// <returns>The Version of the Database Schema</returns>
        protected abstract int EnsureSetup(Dictionary<String,String> parameters);

        /// <summary>
        /// Allows the derived implementation to check whether an upgrade to the database schema is required and apply it if necessary
        /// </summary>
        /// <param name="currVersion">Current Version</param>
        /// <returns></returns>
        protected abstract int CheckForUpgrades(int currVersion);

        #region Script Execution Functions

        /// <summary>
        /// Executes the SQL from an embedded resource
        /// </summary>
        /// <param name="resource">Embedded Resource Name</param>
        /// <remarks>
        /// Assumes that the resource is embedded in this assembly
        /// </remarks>
        protected internal void ExecuteSqlFromResource(String resource)
        {
            this.ExecuteSqlFromResource(Assembly.GetExecutingAssembly(), resource);
        }

        /// <summary>
        /// Executes the SQL from an embedded resource
        /// </summary>
        /// <param name="assm">Assembly</param>
        /// <param name="resource">Embedded Resource Name</param>
        /// <remarks>
        /// <para>
        /// Heavily adapted from code used in <a href="http://www.bugnetproject.com">BugNet</a>
        /// </para>
        /// </remarks>
        protected internal void ExecuteSqlFromResource(Assembly assm, String resource)
        {
            Stream stream = assm.GetManifestResourceStream(resource);
            if (stream == null)
            {
                throw new RdfStorageException("Cannot execute an SQL script from an embedded resource as requested resource '" + resource + "' could not be loaded");
            }
            this.ExecuteSql(stream);
        }

        /// <summary>
        /// Executes the SQL from a stream
        /// </summary>
        /// <remarks>
        /// <para>
        /// Heavily adapted from code used in <a href="http://www.bugnetproject.com">BugNet</a>
        /// </para>
        /// </remarks>
        protected internal void ExecuteSql(Stream stream)
        {
            List<String> statements = new List<String>();

            using (StreamReader reader = new StreamReader(stream))
            {
                String statement;
                do
                {
                    statement = this.ReadNextStatementFromStream(reader);
                    if (statement != null)
                    {
                        statements.Add(statement);
                    }
                } while (statement != null);
                reader.Close();
            }

            foreach (String cmd in statements)
            {
                TCommand command = this.GetCommand();
                command.Connection = this._connection;
                command.CommandType = CommandType.Text;
                command.CommandText = cmd;
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Reads the next statement from stream.
        /// </summary>
        /// <param name="reader">Stream to read from</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Taken from code used in <a href="http://www.bugnetproject.com">BugNet</a>
        /// </para>
        /// </remarks>
        private String ReadNextStatementFromStream(StreamReader reader)
        {
            StringBuilder sb = new StringBuilder();
            String line;

            while (true)
            {
                line = reader.ReadLine();
                if (line == null)
                {
                    if (sb.Length > 0)
                    {
                        return sb.ToString();
                    }
                    else
                    {
                        return null;
                    }
                }
                if (line.TrimEnd().ToUpper() == "GO") break;

                sb.AppendLine(line);
            }
            return sb.ToString();
        }

        #endregion

        #endregion

        #region Internal Implementation

        /// <summary>
        /// Checks the Version of the Store
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is intended for two purposes:
        /// <ol>
        ///     <li>Future proofing so later versions of the library can add additional stored procedures to the database and the code can decide which are available to it</li>
        ///     <li>Detecting when users try to use the class to connect to legacy databases created with the old Schema which are not compatible with this code</li>
        /// </ol>
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
                        break;
                    default:
                        throw new RdfStorageException("Unknown ADO Store Version");
                }

                //Allow the derived class to apply updates if it so desires
                version = this.CheckForUpgrades(version);

                //Return the final version
                return version;
            }
            catch (TException ex)
            {
                if (ex.Message.IndexOf("permission", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    throw new RdfStorageException("Unable to connect to an ADO Store as it appears you may not have the necessary permissions on this database, see inner exception for details.  Users for ADO stores should be added to one of the roles rdf_admin, rdf_readwrite, rdf_readinsert or rdf_readonly", ex);
                }

                //If we get an SQL Exception then it may mean we've been used to try to connect to a legacy
                //SQL Store so check this now
                try
                {
                    //Try the following SQL, if we are talking to a legacy store it will have the
                    //graphHash field which doesn't appear in the new database schema
                    this.ExecuteScalar("SELECT graphHash FROM GRAPHS WHERE graphID=1");

                    //If it executes succesfully then it's a legacy store
                    throw new RdfStorageException("The underlying Database appears to be a legacy SQL Store using the old dotNetRDF Store Format.  You may connect to this for the time being using one of the old ISqlIOManager implementations but should see the documentation at http://www.dotnetrdf.org?content.asp?pageID=dotNetRDF%20Store#migration with regards to upgrading your store");
                }
                catch (TException)
                {
                    //If this check errors then not a legacy store so may just be not set up yet
                    return this.EnsureSetup(this._parameters);
                }
            }
        }

        /// <summary>
        /// Checks the Schema of the Store
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// As the ADO Store is designed entirely in terms of stored procedures the underlying database schema is up to the implementor, two different schemas are provided in the library by default.
        /// </para>
        /// <para>
        /// This method should report the name of the schema, this may refer to one of the inbuilt schemas or may refer to a custom implemented schema.
        /// </para>
        /// </remarks>
        public String CheckSchema()
        {
            TCommand cmd = this.GetCommand();
            cmd.CommandText = "GetSchemaName";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Connection = this._connection;
            return (String)cmd.ExecuteScalar();
        }

        /// <summary>
        /// Gets/Sets what mode is used to retrieve data from the ADO Store
        /// </summary>
        [Description("Gets/Sets what mode is used to retrieve data from the ADO Store")]
        public AdoAccessMode AccessMode
        {
            get
            {
                return this._accessMode;
            }
            set
            {
                this._accessMode = value;
            }
        }

        /// <summary>
        /// Gets the Version as detected when this instance was created, use <see cref="CheckVersion()">CheckVersion()</see> to directly query the store for its version
        /// </summary>
        [Description("Shows the version of the ADO Store used.  This is distinct from the Schema and indicates the exact set of standardised stored procedure that the store will support regardless of the actual database schema used to store the data.")]
        public int Version
        {
            get
            {
                return this._version;
            }
        }

        /// <summary>
        /// Gets the Schema as detected when this instance was created, use <see cref="CheckSchema()">CheckSchema()</see> to directly query the store for its schema
        /// </summary>
        [Description("Shows the schema that this ADO Store uses.")]
        public String Schema
        {
            get
            {
                return this._schema;
            }
        }

        /// <summary>
        /// Gets the Database Type that is used by this ADO Store
        /// </summary>
        [Description("Shows the underlying database for this ADO Store")]
        public abstract String DatabaseType
        {
            get;
        }

        /// <summary>
        /// Clears the Store
        /// </summary>
        public void ClearStore()
        {
            this.ClearStore(false);
        }

        /// <summary>
        /// Clears the Store
        /// </summary>
        /// <param name="fullClear">Whether to perform a full clear</param>
        /// <remarks>
        /// <para>
        /// A Full Clear will remove all existing Node Value to ID mappings whereas a normal clear leaves those in place.  If the data you intend to insert into the store after clearing it is similar to the data in the store currently then you should <strong>not</strong> perform a full clear as leaving the Node Value to ID mappings will make future imports faster.
        /// </para>
        /// </remarks>
        public void ClearStore(bool fullClear)
        {
            TCommand cmd = this.GetCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = fullClear ? "ClearStoreFull" : "ClearStore";
            cmd.Connection = this._connection;
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Gets the Connection to the underlying database
        /// </summary>
        internal TConn Connection
        {
            get
            {
                return this._connection;
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

        private void ApplyNodeCommands(IEnumerable<Triple> ts, AdoStoreWriteCache cache, TCommand cmd, TCommand nodeCmd, TCommand bnodeCmd)
        {
            AdoStoreNodeID s, p, o;
            foreach (Triple t in ts)
            {
                //Get/Create Node ID for each Node
                s = cache.GetNodeID(t.Subject);
                if (s.ID <= 0)
                {
                    if (t.Subject.NodeType != NodeType.Blank)
                    {
                        this.EncodeNode(nodeCmd, t.Subject);
                        nodeCmd.ExecuteNonQuery();
                        s.ID = (int)nodeCmd.Parameters["RC"].Value;
                    }
                    else
                    {
                        bnodeCmd.ExecuteNonQuery();
                        s.ID = (int)bnodeCmd.Parameters["RC"].Value;
                    }
                    cache.AddNodeID(s);
                }
                p = cache.GetNodeID(t.Predicate);
                if (p.ID <= 0)
                {
                    if (t.Predicate.NodeType != NodeType.Blank)
                    {
                        this.EncodeNode(nodeCmd, t.Predicate);
                        nodeCmd.ExecuteNonQuery();
                        p.ID = (int)nodeCmd.Parameters["RC"].Value;
                    }
                    else
                    {
                        bnodeCmd.ExecuteNonQuery();
                        p.ID = (int)bnodeCmd.Parameters["RC"].Value;
                    }
                    cache.AddNodeID(p);
                }
                o = cache.GetNodeID(t.Object);
                if (o.ID <= 0)
                {
                    if (t.Object.NodeType != NodeType.Blank)
                    {
                        this.EncodeNode(nodeCmd, t.Object);
                        nodeCmd.ExecuteNonQuery();
                        o.ID = (int)nodeCmd.Parameters["RC"].Value;
                    }
                    else
                    {
                        bnodeCmd.ExecuteNonQuery();
                        o.ID = (int)bnodeCmd.Parameters["RC"].Value;
                    }
                    cache.AddNodeID(o);
                }

                this.EncodeNodeID(cmd, s, TripleSegment.Subject);
                this.EncodeNodeID(cmd, p, TripleSegment.Predicate);
                this.EncodeNodeID(cmd, o, TripleSegment.Object);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Encodes the values for a Node onto a command
        /// </summary>
        /// <param name="cmd">Command</param>
        /// <param name="n">Node</param>
        internal void EncodeNode(TCommand cmd, INode n)
        {
            this.EncodeNode(cmd, n, null);
        }

        /// <summary>
        /// Encodes the values for a Node onto a command for the given triple segment
        /// </summary>
        /// <param name="cmd">Command</param>
        /// <param name="n">Node</param>
        /// <param name="segment">Triple Segment</param>
        internal void EncodeNode(TCommand cmd, INode n, TripleSegment? segment)
        {
            String prefix = "node";
            if (segment != null)
            {
                switch (segment)
                {
                    case TripleSegment.Subject:
                        prefix = "subject";
                        break;
                    case TripleSegment.Predicate:
                        prefix = "predicate";
                        break;
                    case TripleSegment.Object:
                        prefix = "object";
                        break;
                }
            }

            //Node Type Parameter
            if (!cmd.Parameters.Contains(prefix + "Type"))
            {
                cmd.Parameters.Add(this.GetParameter(prefix + "Type"));
                cmd.Parameters[prefix + "Type"].DbType = DbType.Byte;
            }
            cmd.Parameters[prefix + "Type"].Value = (byte)n.NodeType;

            //Node Value Parameter
            if (!cmd.Parameters.Contains(prefix + "Value"))
            {
                cmd.Parameters.Add(this.GetParameter(prefix + "Value"));
                cmd.Parameters[prefix + "Value"].DbType = DbType.String;
            }
            switch (n.NodeType)
            {
                case NodeType.Blank:
                    cmd.Parameters[prefix + "Value"].Value = n.ToString();
                    break;
                case NodeType.Literal:
                    cmd.Parameters[prefix + "Value"].Value = ((LiteralNode)n).Value;
                    break;
                case NodeType.Uri:
                    cmd.Parameters[prefix + "Value"].Value = n.ToString();
                    break;
                default:
                    throw new NotSupportedException("Only Blank, URI and Literal Nodes are currently supported");
            }

            //Node Meta Parameter
            if (n.NodeType == NodeType.Literal)
            {
                LiteralNode lit = (LiteralNode)n;
                if (lit.DataType != null)
                {
                    if (!cmd.Parameters.Contains(prefix + "Meta"))
                    {
                        cmd.Parameters.Add(this.GetParameter(prefix + "Meta"));
                        cmd.Parameters[prefix + "Meta"].DbType = DbType.String;
                    }
                    cmd.Parameters[prefix + "Meta"].Value = lit.DataType.ToString();
                }
                else if (!lit.Language.Equals(String.Empty))
                {
                    if (!cmd.Parameters.Contains(prefix + "Meta"))
                    {
                        cmd.Parameters.Add(this.GetParameter(prefix + "Meta"));
                        cmd.Parameters[prefix + "Meta"].DbType = DbType.String;
                    }
                    cmd.Parameters[prefix + "Meta"].Value = "@" + lit.Language;
                }
                else
                {
                    if (cmd.Parameters.Contains(prefix + "Meta")) cmd.Parameters.RemoveAt(prefix + "Meta");
                }
            }
            else
            {
                if (cmd.Parameters.Contains(prefix + "Meta")) cmd.Parameters.RemoveAt(prefix + "Meta");
            }
        }

        /// <summary>
        /// Encodes the ID for a Node onto a command
        /// </summary>
        /// <param name="cmd">Command</param>
        /// <param name="id">Node ID</param>
        internal void EncodeNodeID(TCommand cmd, AdoStoreNodeID id)
        {
            this.EncodeNodeID(cmd, id, null);
        }

        /// <summary>
        /// Encodes the ID for a Node onto a command
        /// </summary>
        /// <param name="cmd">Command</param>
        /// <param name="id">Node ID</param>
        internal void EncodeNodeID(TCommand cmd, int id)
        {
            this.EncodeNodeID(cmd, id, null);
        }

        /// <summary>
        /// Encodes the ID for a Node onto a command for a given triple segment
        /// </summary>
        /// <param name="cmd">Command</param>
        /// <param name="id">Node ID</param>
        /// <param name="segment">Triple Segment</param>
        internal void EncodeNodeID(TCommand cmd, AdoStoreNodeID id, TripleSegment? segment)
        {
            this.EncodeNodeID(cmd, id.ID, segment);
        }

        /// <summary>
        /// Encodes the ID for a Node onto a command for a given triple segment
        /// </summary>
        /// <param name="cmd">Command</param>
        /// <param name="id">Node ID</param>
        /// <param name="segment">Triple segment</param>
        internal void EncodeNodeID(TCommand cmd, int id, TripleSegment? segment)
        {
            String prefix = "node";
            switch (segment)
            {
                case TripleSegment.Subject:
                    prefix = "subject";
                    break;
                case TripleSegment.Predicate:
                    prefix = "predicate";
                    break;
                case TripleSegment.Object:
                    prefix = "object";
                    break;
            }

            if (!cmd.Parameters.Contains(prefix + "ID"))
            {
                cmd.Parameters.Add(this.GetParameter(prefix + "ID"));
                cmd.Parameters[prefix + "ID"].DbType = DbType.Int32;
            }
            cmd.Parameters[prefix + "ID"].Value = id;
        }

        /// <summary>
        /// Decodes a Node from the constituent values in the database
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="type">Node Type</param>
        /// <param name="value">Node Value</param>
        /// <param name="meta">Node Meta</param>
        /// <returns></returns>
        internal INode DecodeNode(IGraph g, byte type, String value, String meta)
        {
            return this.DecodeNode((INodeFactory)g, type, value, meta);
        }

        /// <summary>
        /// Decodes a Node from the constituent values in the database
        /// </summary>
        /// <param name="factory">Node Factory</param>
        /// <param name="type">Node Type</param>
        /// <param name="value">Node Value</param>
        /// <param name="meta">Node Meta</param>
        /// <returns></returns>
        internal INode DecodeNode(INodeFactory factory, byte type, String value, String meta)
        {
            if (factory == null) factory = this._factory;
            switch (type)
            {
                case 0:
                    return factory.CreateBlankNode(value.Substring(2));
                case 1:
                    return factory.CreateUriNode(new Uri(value));
                case 2:
                    if (meta == null)
                    {
                        return factory.CreateLiteralNode(value);
                    }
                    else if (meta.StartsWith("@"))
                    {
                        return factory.CreateLiteralNode(value, meta.Substring(1));
                    }
                    else
                    {
                        return factory.CreateLiteralNode(value, new Uri(meta));
                    }
                default:
                    throw new NotSupportedException("Only Blank, URI and Literal Nodes are currently supported");
            }
        }

        /// <summary>
        /// Decodes a Virtual Node from ID and type
        /// </summary>
        /// <param name="factory">Node Factory</param>
        /// <param name="type">Node Type</param>
        /// <param name="id">Node ID</param>
        /// <returns></returns>
        internal INode DecodeVirtualNode(INodeFactory factory, byte type, int id)
        {
            switch (type)
            {
                case 0:
                    return new SimpleVirtualBlankNode(null, id, this);
                case 1:
                    return new SimpleVirtualUriNode(null, id, this);
                case 2:
                    return new SimpleVirtualLiteralNode(null, id, this);
                default:
                    throw new NotSupportedException("Only Blank, URI and Literal Nodes are currently supported");
            }
        }

        /// <summary>
        /// Decodes a Virtual Node from ID and type
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="type">Node Type</param>
        /// <param name="id">Node ID</param>
        /// <returns></returns>
        internal INode DecodeVirtualNode(IGraph g, byte type, int id)
        {
            switch (type)
            {
                case 0:
                    return new SimpleVirtualBlankNode(g, id, this);
                case 1:
                    return new SimpleVirtualUriNode(g, id, this);
                case 2:
                    return new SimpleVirtualLiteralNode(g, id, this);
                default:
                    throw new NotSupportedException("Only Blank, URI and Literal Nodes are currently supported");
            }
        }

        /// <summary>
        /// Decodes the Node Meta information
        /// </summary>
        /// <param name="meta">Meta Object</param>
        /// <returns></returns>
        internal String DecodeMeta(Object meta)
        {
            if (Convert.IsDBNull(meta))
            {
                return null;
            }
            else
            {
                return (String)meta;
            }
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
                try
                {
                    this._connection.Close();
                }
                catch
                {
                    //Suppress errors closing the connection
                }
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

        #region IGenericIOManager Members

        /// <summary>
        /// Loads a Graph from the store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">Graph URI</param>
        public void LoadGraph(IGraph g, Uri graphUri)
        {
            if (g.IsEmpty) g.BaseUri = graphUri;
            this.LoadGraph(new GraphHandler(g), graphUri);
        }

        /// <summary>
        /// Loads a Graph from the store
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">Graph URI</param>
        public void LoadGraph(IRdfHandler handler, Uri graphUri)
        {
            if (handler == null) throw new RdfStorageException("Cannot load a Graph using a null RDF Handler");

            try
            {
                handler.StartRdf();

                //First need to get the Graph ID (if any)
                TCommand cmd = this.GetCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetGraphID";
                cmd.Connection = this._connection;
                if (graphUri != null)
                {
                    cmd.Parameters.Add(this.GetParameter("graphUri"));
                    cmd.Parameters["graphUri"].DbType = DbType.String;
                    cmd.Parameters["graphUri"].Value = graphUri.ToString();
                }
                cmd.Parameters.Add(this.GetParameter("RC"));
                cmd.Parameters["RC"].DbType = DbType.Int32;
                cmd.Parameters["RC"].Direction = ParameterDirection.ReturnValue;
                cmd.ExecuteNonQuery();

                int id = (int)cmd.Parameters["RC"].Value;

                if (id > 0)
                {
                    //We got an ID so can start the load process
                    cmd = this.GetCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "GetGraphQuadsData";
                    cmd.Connection = this._connection;
                    cmd.Parameters.Add(this.GetParameter("graphID"));
                    cmd.Parameters["graphID"].DbType = DbType.Int32;
                    cmd.Parameters["graphID"].Value = id;

                    switch (this._accessMode)
                    {
                        case AdoAccessMode.Batched:
                            DataTable table = new DataTable();
                            using (DbDataAdapter adapter = this.GetAdapter())
                            {
                                adapter.SelectCommand = cmd;
                                adapter.Fill(table);
                                foreach (DataRow row in table.Rows)
                                {
                                    INode s = this.DecodeNode(handler, (byte)row["subjectType"], (String)row["subjectValue"], this.DecodeMeta(row["subjectMeta"]));
                                    INode p = this.DecodeNode(handler, (byte)row["predicateType"], (String)row["predicateValue"], this.DecodeMeta(row["predicateMeta"]));
                                    INode o = this.DecodeNode(handler, (byte)row["objectType"], (String)row["objectValue"], this.DecodeMeta(row["objectMeta"]));

                                    if (!handler.HandleTriple(new Triple(s, p, o))) ParserHelper.Stop();
                                }
                                table.Dispose();
                                adapter.Dispose();
                            }
                            break;

                        case AdoAccessMode.Streaming:
                        default:
                            using (DbDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    INode s = this.DecodeNode(handler, (byte)reader["subjectType"], (String)reader["subjectValue"], this.DecodeMeta(reader["subjectMeta"]));
                                    INode p = this.DecodeNode(handler, (byte)reader["predicateType"], (String)reader["predicateValue"], this.DecodeMeta(reader["predicateMeta"]));
                                    INode o = this.DecodeNode(handler, (byte)reader["objectType"], (String)reader["objectValue"], this.DecodeMeta(reader["objectMeta"]));

                                    if (!handler.HandleTriple(new Triple(s, p, o))) ParserHelper.Stop();
                                }
                                reader.Close();
                                reader.Dispose();
                            }
                            break;
                    }
                }

                handler.EndRdf(true);
            }
            catch (RdfParsingTerminatedException)
            {
                handler.EndRdf(true);
            }
            catch
            {
                handler.EndRdf(false);
                throw;
            }
        }

        /// <summary>
        /// Loads a Graph from the store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">Graph URI</param>
        public void LoadGraph(IGraph g, String graphUri)
        {
            if (graphUri == null || graphUri.Equals(String.Empty))
            {
                this.LoadGraph(g, (Uri)null);
            }
            else
            {
                this.LoadGraph(g, new Uri(graphUri));
            }
        }

        /// <summary>
        /// Loads a Graph from the store
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">Graph URI</param>
        public void LoadGraph(IRdfHandler handler, String graphUri)
        {
            if (graphUri == null || graphUri.Equals(String.Empty))
            {
                this.LoadGraph(handler, (Uri)null);
            }
            else
            {
                this.LoadGraph(handler, new Uri(graphUri));
            }
        }

        /// <summary>
        /// Saves a Graph to the store
        /// </summary>
        /// <param name="g">Graph to save</param>
        public void SaveGraph(IGraph g)
        {
            //First need to get/create the Graph ID (if any)
            TCommand cmd = this.GetCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetOrCreateGraphID";
            cmd.Connection = this._connection;
            if (g.BaseUri != null)
            {
                cmd.Parameters.Add(this.GetParameter("graphUri"));
                cmd.Parameters["graphUri"].DbType = DbType.String;
                cmd.Parameters["graphUri"].Value = g.BaseUri.ToString();
            }
            cmd.Parameters.Add(this.GetParameter("RC"));
            cmd.Parameters["RC"].DbType = DbType.Int32;
            cmd.Parameters["RC"].Direction = ParameterDirection.ReturnValue;
            cmd.ExecuteNonQuery();

            int id = (int)cmd.Parameters["RC"].Value;

            if (id > 0)
            {
                //Then we need to ensure that all Quads associated with the Graph currently are removed before we overwrite the graph
                cmd = this.GetCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "ClearGraphForOverwrite";
                cmd.Connection = this._connection;
                cmd.Parameters.Add(this.GetParameter("graphID"));
                cmd.Parameters["graphID"].DbType = DbType.Int32;
                cmd.Parameters["graphID"].Value = id;
                cmd.ExecuteNonQuery();

                //Commands for inserting Nodes
                TCommand nodeCmd = this.GetCommand();
                nodeCmd.CommandType = CommandType.StoredProcedure;
                nodeCmd.CommandText = "GetOrCreateNodeID";
                nodeCmd.Connection = this._connection;
                nodeCmd.Parameters.Add(this.GetParameter("RC"));
                nodeCmd.Parameters["RC"].DbType = DbType.Int32;
                nodeCmd.Parameters["RC"].Direction = ParameterDirection.ReturnValue;

                TCommand bnodeCmd = this.GetCommand();
                bnodeCmd.CommandType = CommandType.StoredProcedure;
                bnodeCmd.CommandText = "CreateBlankNodeID";
                bnodeCmd.Connection = this._connection;
                bnodeCmd.Parameters.Add(this.GetParameter("graphID"));
                bnodeCmd.Parameters["graphID"].DbType = DbType.Int32;
                bnodeCmd.Parameters["graphID"].Value = id;
                bnodeCmd.Parameters.Add(this.GetParameter("RC"));
                bnodeCmd.Parameters["RC"].DbType = DbType.Int32;
                bnodeCmd.Parameters["RC"].Direction = ParameterDirection.ReturnValue;

                //Then we can insert the triples
                cmd = this.GetCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "AssertQuad";
                cmd.Connection = this._connection;
                cmd.Parameters.Add(this.GetParameter("graphID"));
                cmd.Parameters["graphID"].DbType = DbType.Int32;
                cmd.Parameters["graphID"].Value = id;

                //Create a cache and call the method to do the insertions
                AdoStoreWriteCache cache = new AdoStoreWriteCache();
                this.ApplyNodeCommands(g.Triples, cache, cmd, nodeCmd, bnodeCmd);
            }
            else
            {
                throw new RdfStorageException("Unable to Save a Graph as the underlying Store failed to generate a Graph ID");
            }
        }

        /// <summary>
        /// Gets the IO Behaviour of an ADO Store
        /// </summary>
        public virtual IOBehaviour IOBehaviour
        {
            get
            {
                return IOBehaviour.GraphStore | IOBehaviour.CanUpdateTriples;
            }
        }

        /// <summary>
        /// Updates a Graph in the Store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to update</param>
        /// <param name="additions">Triples to add</param>
        /// <param name="removals">Triples to remove</param>
        /// <remarks>
        /// Removals happen prior to additions, if you wish to change this order then make separate calls providing only additions/removals to each call
        /// </remarks>
        public void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            //First need to get/create the Graph ID (if any)
            TCommand cmd = this.GetCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetOrCreateGraphID";
            cmd.Connection = this._connection;
            if (graphUri != null)
            {
                cmd.Parameters.Add(this.GetParameter("graphUri"));
                cmd.Parameters["graphUri"].DbType = DbType.String;
                cmd.Parameters["graphUri"].Value = graphUri.ToString();
            }
            cmd.Parameters.Add(this.GetParameter("RC"));
            cmd.Parameters["RC"].DbType = DbType.Int32;
            cmd.Parameters["RC"].Direction = ParameterDirection.ReturnValue;
            cmd.ExecuteNonQuery();

            int id = (int)cmd.Parameters["RC"].Value;

            if (id > 0)
            {
                //Commands for inserting Nodes
                TCommand nodeCmd = this.GetCommand();
                nodeCmd.CommandType = CommandType.StoredProcedure;
                nodeCmd.CommandText = "GetOrCreateNodeID";
                nodeCmd.Connection = this._connection;
                nodeCmd.Parameters.Add(this.GetParameter("RC"));
                nodeCmd.Parameters["RC"].DbType = DbType.Int32;
                nodeCmd.Parameters["RC"].Direction = ParameterDirection.ReturnValue;

                TCommand bnodeCmd = this.GetCommand();
                bnodeCmd.CommandType = CommandType.StoredProcedure;
                bnodeCmd.CommandText = "CreateBlankNodeID";
                bnodeCmd.Connection = this._connection;
                bnodeCmd.Parameters.Add(this.GetParameter("graphID"));
                bnodeCmd.Parameters["graphID"].DbType = DbType.Int32;
                bnodeCmd.Parameters["graphID"].Value = id;
                bnodeCmd.Parameters.Add(this.GetParameter("RC"));
                bnodeCmd.Parameters["RC"].DbType = DbType.Int32;
                bnodeCmd.Parameters["RC"].Direction = ParameterDirection.ReturnValue;

                //Then we can update the Graph
                cmd = this.GetCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "RetractQuad";
                cmd.Connection = this._connection;
                cmd.Parameters.Add(this.GetParameter("graphID"));
                cmd.Parameters["graphID"].DbType = DbType.Int32;
                cmd.Parameters["graphID"].Value = id;

                //Create a Cache, do the retractions then the assertions
                AdoStoreWriteCache cache = new AdoStoreWriteCache();
                if (removals != null && removals.Any()) this.ApplyNodeCommands(removals, cache, cmd, nodeCmd, bnodeCmd);
                cmd.CommandText = "AssertQuad";
                if (additions != null && additions.Any()) this.ApplyNodeCommands(additions, cache, cmd, nodeCmd, bnodeCmd);
            }
            else
            {
                throw new RdfStorageException("Unable to Update a Graph as the underlying Store failed to generate a Graph ID");
            }
        }

        /// <summary>
        /// Updates a Graph in the Store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to update</param>
        /// <param name="additions">Triples to add</param>
        /// <param name="removals">Triples to remove</param>
        /// <remarks>
        /// Removals happen prior to additions, if you wish to change this order then make separate calls providing only additions/removals to each call
        /// </remarks>
        public void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            if (graphUri == null || graphUri.Equals(String.Empty))
            {
                this.UpdateGraph((Uri)null, additions, removals);
            }
            else
            {
                this.UpdateGraph(new Uri(graphUri), additions, removals);
            }
        }

        /// <summary>
        /// Returns that Updates are supported
        /// </summary>
        public bool UpdateSupported
        {
            get 
            {
                return true;
            }
        }

        /// <summary>
        /// Deletes a Graph from the store
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        public void DeleteGraph(Uri graphUri)
        {
            //Delete the Graph by URI
            TCommand cmd = this.GetCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "DeleteGraphByUri";
            cmd.Connection = this._connection;
            if (graphUri != null)
            {
                cmd.Parameters.Add(this.GetParameter("graphUri"));
                cmd.Parameters["graphUri"].DbType = DbType.String;
                cmd.Parameters["graphUri"].Value = graphUri.ToString();
            }
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Deletes a Graph from the store
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        public void DeleteGraph(string graphUri)
        {
            if (graphUri == null || graphUri.Equals(String.Empty))
            {
                this.DeleteGraph((Uri)null);
            }
            else
            {
                this.DeleteGraph(new Uri(graphUri));
            }
        }

        /// <summary>
        /// Returns that deleting graphs is supported
        /// </summary>
        public bool DeleteSupported
        {
            get 
            {
                return true;
            }
        }

        /// <summary>
        /// Lists the Graphs in the store
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Uri> ListGraphs()
        {
            TCommand cmd = this.GetCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetGraphUris";
            cmd.Connection = this._connection;

            List<Uri> uris = new List<Uri>();
            switch (this._accessMode)
            {
                case AdoAccessMode.Batched:
                    using (DbDataAdapter adapter = this.GetAdapter())
                    {
                        adapter.SelectCommand = cmd;
                        DataTable table = new DataTable();
                        adapter.Fill(table);
                        foreach (DataRow row in table.Rows)
                        {
                            String u = this.DecodeMeta(row["graphUri"]);
                            if (u == null)
                            {
                                uris.Add((Uri)null);
                            }
                            else
                            {
                                uris.Add(new Uri(u));
                            }
                        }
                        table.Dispose();
                        adapter.Dispose();
                    }
                    break;

                case AdoAccessMode.Streaming:
                default:
                    using (DbDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            String u = this.DecodeMeta(reader["graphUri"]);
                            if (u == null)
                            {
                                uris.Add((Uri)null);
                            }
                            else
                            {
                                uris.Add(new Uri(u));
                            }
                        }
                        reader.Close();
                    }
                    break;
            }
            return uris;
        }

        /// <summary>
        /// Returns that listing graphs is supported
        /// </summary>
        public bool ListGraphsSupported
        {
            get 
            {
                return true;
            }
        }

        /// <summary>
        /// Returns that the store is ready
        /// </summary>
        public bool IsReady
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns that the store is not read-only
        /// </summary>
        public bool IsReadOnly
        {
            get 
            {
                return false;
            }
        }

        #endregion

        #region IQueryableGenericIOManager Members

        /// <summary>
        /// Makes a SPARQL Query against the Store
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns></returns>
        public Object Query(String sparqlQuery)
        {
            Graph g = new Graph();
            SparqlResultSet results = new SparqlResultSet();
            this.Query(new GraphHandler(g), new ResultSetHandler(results), sparqlQuery);

            if (results.ResultsType != SparqlResultsType.Unknown)
            {
                return results;
            }
            else
            {
                return g;
            }
        }

        /// <summary>
        /// Makes a SPARQL Query against the Store processing the results with an appropriate handler from those provided
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String sparqlQuery)
        {
            //Parse the Query
            if (this._queryParser == null)
            {
                this._queryParser = new SparqlQueryParser();
            }

            SparqlQuery q = this._queryParser.ParseFromString(sparqlQuery);

            //Initialise Dataset if necessary
            if (this._dataset == null)
            {
                this._dataset = this.GetDataset();
            }

            //Process the Query
            if (this._queryProcessor == null)
            {
                this._queryProcessor = new LeviathanQueryProcessor(this._dataset);
            }
            q.AlgebraOptimisers = this.GetOptimisers();
            this._queryProcessor.ProcessQuery(rdfHandler, resultsHandler, q);
        }

        /// <summary>
        /// Gets the <see cref="ISparqlDataset">ISparqlDataset</see> implementation that should be used within the Query methods of this instance
        /// </summary>
        /// <returns></returns>
        protected abstract ISparqlDataset GetDataset();

        /// <summary>
        /// Gets the <see cref="IAlgebraOptimiser">IAlgebraOptimiser</see> instances that should be used to optimise queries made via the Query methods of this instance
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable<IAlgebraOptimiser> GetOptimisers()
        {
            return Enumerable.Empty<IAlgebraOptimiser>();
        }

        #endregion

        #region IUpdateableGenericIOManager Members

        /// <summary>
        /// Performa a SPARQL Update on the Store
        /// </summary>
        /// <param name="sparqlUpdate">SPARQL Update</param>
        public void Update(String sparqlUpdate)
        {
            //Parse the Updates
            if (this._updateParser == null)
            {
                this._updateParser = new SparqlUpdateParser();
            }

            SparqlUpdateCommandSet cmds = this._updateParser.ParseFromString(sparqlUpdate);

            //Initialise Dataset if necessary
            if (this._dataset == null)
            {
                this._dataset = this.GetDataset();
            }

            //Process the Updates
            if (this._updateProcessor == null)
            {
                this._updateProcessor = new LeviathanUpdateProcessor(this._dataset);
            }
            cmds.AlgebraOptimisers = this.GetOptimisers();
            this._updateProcessor.ProcessCommandSet(cmds);
        }

        #endregion

        #region IVirtualRdfProvider<int,int> Members

        /// <summary>
        /// Gets the materialised value based on a Node ID
        /// </summary>
        /// <param name="g">Graph to materialise the value in</param>
        /// <param name="id">Node ID</param>
        /// <returns></returns>
        public INode GetValue(IGraph g, int id)
        {
            INode value = this._cache[id];
            if (value == null)
            {
                TCommand command = this.GetCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "GetNodeData";
                command.Connection = this._connection;
                command.Parameters.Add(this.GetParameter("nodeID"));
                command.Parameters["nodeID"].DbType = DbType.Int32;
                command.Parameters["nodeID"].Value = id;

                DbDataReader reader = command.ExecuteReader();
                if (reader.HasRows && reader.Read())
                {
                    INode temp = this.DecodeNode(g, (byte)reader["nodeType"], (String)reader["nodeValue"], this.DecodeMeta(reader["nodeMeta"]));
                    this._cache[id] = temp;
                    return temp;
                }
                else
                {
                    throw new RdfStorageException("The ADO Store does not contain a Node with ID " + id);
                }
            }
            else
            {
                return value.CopyNode(g);
            }
        }

        /// <summary>
        /// Gets the Graph URI based on a Graph ID
        /// </summary>
        /// <param name="graphID">Graph ID</param>
        /// <returns></returns>
        public Uri GetGraphUri(int graphID)
        {
            if (this._graphUris.ContainsKey(graphID))
            {
                return this._graphUris[graphID];
            }
            else
            {
                TCommand cmd = this.GetCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetGraphUri";
                cmd.Connection = this._connection;
                cmd.Parameters.Add(this.GetParameter("graphID"));
                cmd.Parameters["graphID"].DbType = DbType.Int32;
                cmd.Parameters["graphID"].Value = graphID;

                Object res = cmd.ExecuteScalar();
                try
                {
                    Monitor.Enter(this._graphUris);
                    if (Convert.IsDBNull(res))
                    {
                        this._graphUris.Add(graphID, null);
                        return null;
                    }
                    else
                    {
                        Uri u = new Uri((String)res);
                        this._graphUris.Add(graphID, u);
                        return u;
                    }
                }
                finally
                {
                    Monitor.Exit(this._graphUris);
                }
            }
        }

        /// <summary>
        /// Gets the Node ID for a value (if such a value exists in the store)
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Either a Node ID or zero if the value does not exist in the store</returns>
        /// <remarks>
        /// <para>
        /// This overload does not create Node IDs if the value does not exist, if you need to create a value use the overload which allows you to specify whether to create new Node IDs as needed
        /// </para>
        /// <para>
        /// <strong>Cannot be used to get IDs for Blank Nodes</strong>
        /// </para>
        /// </remarks>
        public int GetID(INode value)
        {
            return this.GetID(value, false);
        }

        /// <summary>
        /// Gets the Graph ID for a Graph (if such a graph exists in the store)
        /// </summary>
        /// <param name="g">Graph</param>
        /// <returns>Either a Graph ID or zero if the Graph does not exist in the store</returns>
        /// <remarks>
        /// This overload does not create Graph IDs if the value does not exist, if you need to create a value use the overload which allows you to specify whether to create new Graph IDs as needed
        /// </remarks>
        public int GetGraphID(IGraph g)
        {
            return this.GetGraphID(g, false);
        }

        /// <summary>
        /// Gets the Graph ID for a Graph potentially creating a new ID if <paramref name="createIfNotExists"/> was set to true and the graph does not exist in the store
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="createIfNotExists">Whether to create a new Graph ID if there is no existing ID for the graph</param>
        /// <returns>
        /// Either a Graph ID or zero if such a graph does not exist and <paramref name="createIfNotExists"/> was set to false
        /// </returns>
        public int GetGraphID(IGraph g, bool createIfNotExists)
        {
            TCommand cmd = this.GetCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = createIfNotExists ? "GetOrCreateGraphID" : "GetGraphID";
            cmd.Connection = this._connection;
            if (g != null && g.BaseUri != null)
            {
                cmd.Parameters.Add(this.GetParameter("graphUri"));
                cmd.Parameters["graphUri"].DbType = DbType.String;
                cmd.Parameters["graphUri"].Value = g.BaseUri.ToString();
            }
            cmd.Parameters.Add(this.GetParameter("RC"));
            cmd.Parameters["RC"].DbType = DbType.Int32;
            cmd.Parameters["RC"].Direction = ParameterDirection.ReturnValue;
            cmd.ExecuteNonQuery();

            return (int)cmd.Parameters["RC"].Value;
        }

        /// <summary>
        /// Gets the Graph ID for a Graph (if such a graph exists in the store)
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns>Either a Graph ID or zero if the Graph does not exist in the store</returns>
        /// <remarks>
        /// This overload does not create Graph IDs if the value does not exist, if you need to create a value use the overload which allows you to specify whether to create new Graph IDs as needed
        /// </remarks>
        public int GetGraphID(Uri graphUri)
        {
            return this.GetGraphID(graphUri, false);
        }

        /// <summary>
        /// Gets the Graph ID for a Graph potentially creating a new ID if <paramref name="createIfNotExists"/> was set to true and the graph does not exist in the store
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="createIfNotExists">Whether to create a new Graph ID if there is no existing ID for the graph</param>
        /// <returns>
        /// Either a Graph ID or zero if such a graph does not exist and <paramref name="createIfNotExists"/> was set to false
        /// </returns>
        public int GetGraphID(Uri graphUri, bool createIfNotExists)
        {
            TCommand cmd = this.GetCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = createIfNotExists ? "GetOrCreateGraphID" : "GetGraphID";
            cmd.Connection = this._connection;
            if (graphUri != null)
            {
                cmd.Parameters.Add(this.GetParameter("graphUri"));
                cmd.Parameters["graphUri"].DbType = DbType.String;
                cmd.Parameters["graphUri"].Value = graphUri.ToString();
            }
            cmd.Parameters.Add(this.GetParameter("RC"));
            cmd.Parameters["RC"].DbType = DbType.Int32;
            cmd.Parameters["RC"].Direction = ParameterDirection.ReturnValue;
            cmd.ExecuteNonQuery();

            return (int)cmd.Parameters["RC"].Value;
        }

        /// <summary>
        /// Gets the Node ID for a value potentially creating a new ID if <paramref name="createIfNotExists"/> was set to true and the value does not exist in the store
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="createIfNotExists">Whether to create a new Node ID if there is no existing ID for the value</param>
        /// <returns>
        /// Either a Node ID or zero if such a value does not exist and <paramref name="createIfNotExists"/> was set to false
        /// </returns>
        /// <remarks>
        /// <strong>Cannot be used to get IDs for Blank Nodes</strong>
        /// </remarks>
        public int GetID(INode value, bool createIfNotExists)
        {
            if (value.NodeType == NodeType.Blank) throw new RdfStorageException("Cannot use the GetID() method to generate an ID for a Blank Node as Blank Nodes must be scoped to a specific Graph");

            //If already a Virtual Node getting the ID is dead easy provided we are the provider
            if (value is IVirtualNode<int, int>)
            {
                IVirtualNode<int, int> virt = (IVirtualNode<int, int>)value;
                if (ReferenceEquals(this, virt.Provider)) return virt.VirtualID;
            }

            TCommand cmd = this.GetCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = createIfNotExists ? "GetOrCreateNodeID" : "GetNodeID";
            cmd.Connection = this._connection;
            this.EncodeNode(cmd, value);
            cmd.Parameters.Add(this.GetParameter("RC"));
            cmd.Parameters["RC"].DbType = DbType.Int32;
            cmd.Parameters["RC"].Direction = ParameterDirection.ReturnValue;
            cmd.ExecuteNonQuery();

            return (int)cmd.Parameters["RC"].Value;
        }

        /// <summary>
        /// Gets the Node ID for a blank node value potentially creating a new ID if <paramref name="createIfNotExists"/> was set to true and the value does not exist in the store
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="createIfNotExists">Whether to create a new Node ID if there is no existing ID for the value</param>
        /// <returns>
        /// Either a Node ID or zero if such a value does not exist and <paramref name="createIfNotExists"/> was set to false
        /// </returns>
        public int GetBlankNodeID(IBlankNode value, bool createIfNotExists)
        {
            //If already a Virtual Node getting the ID is dead easy provided we are the provider
            if (value is IVirtualNode<int, int>)
            {
                IVirtualNode<int, int> virt = (IVirtualNode<int, int>)value;
                if (ReferenceEquals(this, virt.Provider)) return virt.VirtualID;
            }

            if (!createIfNotExists) return 0;

            int graphID = this.GetGraphID(value.Graph, createIfNotExists);
            if (graphID == 0) return 0;

            TCommand cmd = this.GetCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "CreateBlankNodeID";
            cmd.Connection = this._connection;
            cmd.Parameters.Add(this.GetParameter("graphID"));
            cmd.Parameters["graphID"].DbType = DbType.Int32;
            cmd.Parameters["graphID"].Value = graphID;
            cmd.Parameters.Add(this.GetParameter("RC"));
            cmd.Parameters["RC"].DbType = DbType.Int32;
            cmd.Parameters["RC"].Direction = ParameterDirection.ReturnValue;
            cmd.ExecuteNonQuery();

            return (int)cmd.Parameters["RC"].Value;
        }

        /// <summary>
        /// Gets the Node ID for a blank node value (if it already exists in the store)
        /// </summary>
        /// <param name="value">Value</param>
        public int GetBlankNodeID(IBlankNode value)
        {
            return this.GetBlankNodeID(value, false);
        }

        /// <summary>
        /// Gets the ID which signifies that no ID exists for the input (Is zero for ADO Stores)
        /// </summary>
        public int NullID
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Loads a Graph from the store as virtual nodes
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public void LoadGraphVirtual(IGraph g, Uri graphUri)
        {
            //First need to get the Graph ID (if any)
            TCommand cmd = this.GetCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetGraphID";
            cmd.Connection = this._connection;
            if (graphUri != null)
            {
                cmd.Parameters.Add(this.GetParameter("graphUri"));
                cmd.Parameters["graphUri"].DbType = DbType.String;
                cmd.Parameters["graphUri"].Value = graphUri.ToString();
            }
            cmd.Parameters.Add(this.GetParameter("RC"));
            cmd.Parameters["RC"].DbType = DbType.Int32;
            cmd.Parameters["RC"].Direction = ParameterDirection.ReturnValue;
            cmd.ExecuteNonQuery();

            int id = (int)cmd.Parameters["RC"].Value;

            if (id > 0)
            {
                //We got an ID so can start the load process
                //Set the Target Graph
                IGraph target = (g.IsEmpty) ? g : new Graph();

                cmd = this.GetCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetGraphQuadsVirtual";
                cmd.Connection = this._connection;
                cmd.Parameters.Add(this.GetParameter("graphID"));
                cmd.Parameters["graphID"].DbType = DbType.Int32;
                cmd.Parameters["graphID"].Value = id;

                switch (this._accessMode)
                {
                    case AdoAccessMode.Batched:
                        using (DbDataAdapter adapter = this.GetAdapter())
                        {
                            adapter.SelectCommand = cmd;
                            DataTable table = new DataTable();
                            adapter.Fill(table);

                            foreach (DataRow row in table.Rows)
                            {
                                INode s = this.DecodeVirtualNode(target, (byte)row["subjectType"], (int)row["subjectID"]);
                                INode p = this.DecodeVirtualNode(target, (byte)row["predicateType"], (int)row["predicateID"]);
                                INode o = this.DecodeVirtualNode(target, (byte)row["objectType"], (int)row["objectID"]);

                                target.Assert(new Triple(s, p, o));
                            }

                            table.Dispose();
                            adapter.Dispose();
                        }
                        break;
                    case AdoAccessMode.Streaming:
                    default:
                        using (DbDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                INode s = this.DecodeVirtualNode(target, (byte)reader["subjectType"], (int)reader["subjectID"]);
                                INode p = this.DecodeVirtualNode(target, (byte)reader["predicateType"], (int)reader["predicateID"]);
                                INode o = this.DecodeVirtualNode(target, (byte)reader["objectType"], (int)reader["objectID"]);

                                target.Assert(new Triple(s, p, o));
                            }
                            reader.Close();
                            reader.Dispose();
                        }
                        break;
                }

                if (!ReferenceEquals(target, g))
                {
                    g.Merge(target);
                }
            }
        }

        #endregion

        #region IConfigurationSerializable Members

        /// <summary>
        /// Serializes this connection to a Configuration Graph
        /// </summary>
        /// <param name="context">Configuration Serialization Context</param>
        public abstract void SerializeConfiguration(ConfigurationSerializationContext context);

        #endregion

        /// <summary>
        /// Gets the string representation of this connection
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();
    }

    /// <summary>
    /// Abstract Base implementation of the ADO Store for stores that are accessed using the System.Data.SqlClient API
    /// </summary>
    public abstract class BaseAdoSqlClientStore
        : BaseAdoStore<SqlConnection, SqlCommand, SqlParameter, SqlDataAdapter, SqlException>
    {
        /// <summary>
        /// Creates a new SQL Client based ADO Store
        /// </summary>
        /// <param name="parameters">Connection Parameters</param>
        /// <param name="mode">Access Mode</param>
        public BaseAdoSqlClientStore(Dictionary<String,String> parameters, AdoAccessMode mode)
            : base(parameters, mode) { }

        /// <summary>
        /// Creates a new SQL Client based ADO Store
        /// </summary>
        /// <param name="parameters">Connection Parameters</param>
        public BaseAdoSqlClientStore(Dictionary<String, String> parameters)
            : this(parameters, AdoAccessMode.Streaming) { }
    }
}
