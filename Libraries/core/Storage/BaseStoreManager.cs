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
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Text.RegularExpressions;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Abstract Base Class for dotNetRDF Store Managers which provides the basic infrastructure for a Thread Safe Manager with writing handled by a background buffer
    /// </summary>
    /// <remarks>
    /// Provides a Background Buffer Thread which does writing when Transactions are enabled, when transactions are disabled it assumes that this has been done to allow multi-threaded writing and so all writes are done synchronously.  Benchmarking shows this is about 3 times as fast as using the single buffer Thread when doing a large multi-threaded write.
    /// </remarks>
    [Obsolete("The legacy SQL store format and related classes are officially deprecated - please see http://www.dotnetrdf.org?content.asp?pageID=dotNetRDF%20Store#migration for details on upgrading to the new ADO store format", false)]
    public abstract class BaseStoreManager 
        : ISqlIOManager, IThreadedSqlIOManager, IGenericIOManager, IConfigurationSerializable
    {
        #region Protected Member Variables for use by derived classes

        /// <summary>
        /// Variables for Database Connection Properties
        /// </summary>
        protected String _dbserver, _dbname, _dbuser, _dbpwd;

        /// <summary>
        /// Indicates whether the Database Connection should be kept open
        /// </summary>
        protected bool _keepOpen = false;

        /// <summary>
        /// Variable indicating if the Manager should preserve its State when its <see cref="BaseStoreManager.Dispose">Dispose</see> method is called
        /// </summary>
        protected bool _preserveState = false;

        /// <summary>
        /// Defines a Regular Expression for Valid Language Specifiers to aid decoding of Literal Node values from the Database
        /// </summary>
        protected static Regex _validLangSpecifier = new Regex("@[A-Za-z]{2}(\\-[A-Za-z]{2})?$");

        /// <summary>
        /// Dictionary for Mapping Graph Uri Enhanced Hash Codes to Database Graph IDs
        /// </summary>
        protected Dictionary<int, String> _graphIDs = new Dictionary<int, String>();
        /// <summary>
        /// Dictionary for Mapping Database Graph IDs to Graph URIs
        /// </summary>
        protected Dictionary<String, Uri> _graphUris;
        /// <summary>
        /// Dictionary for Mapping Triple Objects by Hash Code to Database Triple IDs
        /// </summary>
        protected Dictionary<int, int> _tripleIDs = new Dictionary<int, int>();
        /// <summary>
        /// Dictionary for Mapping Node IDs to Node Objects
        /// </summary>
        protected Dictionary<String, INode> _nodes = new Dictionary<String, INode>();
        /// <summary>
        /// Dictionary for Mapping Node Objects by Hash Code to Database Node IDs
        /// </summary>
        protected Dictionary<int, int> _nodeIDs = new Dictionary<int, int>();
        /// <summary>
        /// Dictionary for Mapping Namespace Prefixes to Database Prefix IDs
        /// </summary>
        protected Dictionary<String, String> _nsPrefixIDs = new Dictionary<String, String>();
        /// <summary>
        /// Dictionary for Mapping Namespace Uri Enhanced Hash Codes to Database Uri IDs
        /// </summary>
        protected Dictionary<int, String> _nsUriIDs = new Dictionary<int, String>();

        /// <summary>
        /// The Next Triple ID to be used
        /// </summary>
        protected int _nextTripleID = -1;
        /// <summary>
        /// The Next Node ID to be used
        /// </summary>
        protected int _nextNodeID = -1;
        /// <summary>
        /// Triple Collection used to record which Triples have been written during a session and thus spot Triple Collisions
        /// </summary>
        protected TripleCollection _tripleCollection = new TripleCollection();
        /// <summary>
        /// Node Collection used to record which Nodes have been written during a session and thus spot Node Collisions
        /// </summary>
        protected NodeCollection _nodeCollection = new NodeCollection();

        /// <summary>
        /// Default Batch Size for the Buffered Writing
        /// </summary>
        public const int DefaultBatchSize = 1000;

        /// <summary>
        /// Buffer for Writing
        /// </summary>
        protected LinkedList<BatchTriple> _writerBuffer = new LinkedList<BatchTriple>();
        /// <summary>
        /// Batch Size for the Buffer
        /// </summary>
        protected int _batchSize = DefaultBatchSize;
        /// <summary>
        /// Thread that does the Writing of the Buffer to the Database
        /// </summary>
        private Thread _writer = null;
        /// <summary>
        /// Indicates whether the Writer should Flush the Buffer to the Database even if it is below the Batch Size
        /// </summary>
        protected bool _flushWriter = false;
        /// <summary>
        /// Indicates whether the Writer should stop if the Buffer is empty
        /// </summary>
        protected bool _terminateWriter = false;

        /// <summary>
        /// Indicates whether Transactions should be used with Database Connections
        /// </summary>
        protected bool _noTrans = false;

        #endregion

        #region Basic Properties

        /// <summary>
        /// Gets/Sets whether the Manager should preserve its State when disposed of
        /// </summary>
        /// <remarks>
        /// <para>
        /// Set this property to true if this instance will be used multiple times and may get disposed of multiple times to cause it to preserve its state and not dispose itself.  Classes that use an ISqlIOManager will typically dispose of the manager in their <strong>Dispose()</strong> methods since these methods should rightly dispose of resources they no longer need.
        /// </para>
        /// </remarks>
        public virtual bool PreserveState
        {
            get
            {
                return this._preserveState;
            }
            set
            {
                this._preserveState = true;
            }
        }

        /// <summary>
        /// Gets whether the Manager has completed all of its Buffered Write Operations
        /// </summary>
        /// <remarks>
        /// <para>
        /// Signals the Writer to Flush the Buffer (if it is not doing so already) since if someone wants to know if we've completed we should get on and complete
        /// </para>
        /// <para>
        /// If
        /// </para>
        /// </remarks>
        public virtual bool HasCompleted
        {
            get
            {
                if (this._writer.ThreadState == ThreadState.Stopped || this._writer.ThreadState == ThreadState.StopRequested)
                {
                    throw new RdfStorageException("The Background Writer process of this Manager has already been terminated, if you are using this Manager instance multiple times then you should ensure the PreserveState property is set to true");
                }
                this._flushWriter = true;
                bool completed = false;
                lock (this._writerBuffer)
                {
                    completed = (this._writerBuffer.Count == 0);
                }
                return completed;
            }
        }

        /// <summary>
        /// Gets/Sets whether Database Transactions are disabled
        /// </summary>
        public virtual bool DisableTransactions
        {
            get
            {
                return this._noTrans;
            }
            set
            {
                this._noTrans = value;
            }
        }

        /// <summary>
        /// Gets the Database Server name
        /// </summary>
        public String DatabaseServer
        {
            get
            {
                return this._dbserver;
            }
        }

        /// <summary>
        /// Gets the Database Name
        /// </summary>
        public String DatabaseName
        {
            get
            {
                return this._dbname;
            }
        }

        /// <summary>
        /// Gets the Database Username
        /// </summary>
        public String DatabaseUser
        {
            get
            {
                return this._dbuser;
            }
        }

        /// <summary>
        /// Gets the Database Password
        /// </summary>
        public String DatabasePassword
        {
            get
            {
                return this._dbpwd;
            }
        }

        #endregion

        #region Base Constructor

        /// <summary>
        /// Constructor which sets up the Writer Thread
        /// </summary>
        public BaseStoreManager()
        {
            this._writer = new Thread(new ThreadStart(this.ProcessBuffer));
            this._writer.IsBackground = true;
            this._writer.Start();
        }

        /// <summary>
        /// Base Constructor for derived classes which fills in the Database Property fields
        /// </summary>
        /// <param name="server">Database Server</param>
        /// <param name="db">Database Name</param>
        /// <param name="user">Database Username</param>
        /// <param name="password">Database Password</param>
        public BaseStoreManager(String server, String db, String user, String password) : this()
        {
            this._dbserver = server;
            this._dbname = db;
            this._dbuser = user;
            this._dbpwd = password;
        }

        #endregion

        #region Abstract definitions for Interface Methods

        /// <summary>
        /// Gets the ID for a Graph in the Store, creates a new Graph in the store if necessary
        /// </summary>
        /// <param name="graphUri">Graph Uri</param>
        /// <returns></returns>
        public abstract string GetGraphID(Uri graphUri);

        /// <summary>
        /// Gets the URI of a Graph based on its ID in the Store
        /// </summary>
        /// <param name="graphID">Graph ID</param>
        /// <returns></returns>
        public abstract Uri GetGraphUri(String graphID);

        /// <summary>
        /// Determines whether a given Graph exists in the Store
        /// </summary>
        /// <param name="graphUri">Graph Uri</param>
        /// <returns></returns>
        public abstract bool Exists(Uri graphUri);

        /// <summary>
        /// Gets the URIs of Graphs in the Store
        /// </summary>
        /// <returns></returns>
        public abstract List<Uri> GetGraphUris();

        /// <summary>
        /// Loads Namespaces for the Graph with the given ID into the given Graph object
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphID">Database Graph ID</param>
        public abstract void LoadNamespaces(IGraph g, string graphID);

        /// <summary>
        /// Loads Triples for the Graph with the given ID into the given Graph object
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphID">Database Graph ID</param>
        public abstract void LoadTriples(IGraph g, string graphID);

        /// <summary>
        /// Loads a Node from the Database into the relevant Graph
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="nodeID">Database Node ID</param>
        public abstract INode LoadNode(IGraph g, string nodeID);

        /// <summary>
        /// Gets the Node ID for a Node in the Database creating a new Database record if necessary
        /// </summary>
        /// <param name="n">Node to Save</param>
        /// <returns></returns>
        public abstract string SaveNode(INode n);

        /// <summary>
        /// Saves a Triple from a Graph into the Database
        /// </summary>
        /// <param name="t">Triple to save</param>
        /// <param name="graphID">Database Graph ID</param>
        public virtual void SaveTriple(Triple t, string graphID)
        {
            if (this._writer.ThreadState == ThreadState.Stopped || this._writer.ThreadState == ThreadState.StopRequested)
            {
                throw new RdfStorageException("The Background Writer process of this Manager has already been terminated, if you are using this Manager instance multiple times then you should ensure the PreserveState property is set to true");
            }

            BatchTriple b = new BatchTriple(t, graphID);

            lock (this._writerBuffer)
            {
                this._writerBuffer.AddLast(b);
            }
        }

        /// <summary>
        /// Saves a Triple from the Buffer into the Database
        /// </summary>
        /// <param name="b">Batch Triple information</param>
        /// <remarks>Can assume an open Database connection</remarks>
        protected abstract void SaveTripleInternal(BatchTriple b);

        /// <summary>
        /// Saves a Namespace from a Graph into the Database
        /// </summary>
        /// <param name="prefix">Namespace Prefix</param>
        /// <param name="u">Namespace Uri</param>
        /// <param name="graphID">Database Graph ID</param>
        public abstract void SaveNamespace(string prefix, Uri u, string graphID);

        /// <summary>
        /// Removes a Triple from a Graphs Database record
        /// </summary>
        /// <param name="t">Triple to remove</param>
        /// <param name="graphID">Database Graph ID</param>
        public abstract void RemoveTriple(Triple t, string graphID);

        /// <summary>
        /// Removes a Namespace from a Graphs Database record
        /// </summary>
        /// <param name="prefix">Namespace Prefix</param>
        /// <param name="u">Namespace Uri</param>
        /// <param name="graphID">Database Graph ID</param>
        public abstract void RemoveNamespace(string prefix, Uri u, string graphID);

        /// <summary>
        /// Removes a Graph from the Database
        /// </summary>
        /// <param name="graphID">Database Graph ID</param>
        public abstract void RemoveGraph(String graphID);

        /// <summary>
        /// Changes the Uri associated with the Prefix in a Graphs Database record
        /// </summary>
        /// <param name="prefix">Namespace Prefix</param>
        /// <param name="u">New Namespace Uri</param>
        /// <param name="graphID">Database Graph ID</param>
        public abstract void UpdateNamespace(String prefix, Uri u, String graphID);

        /// <summary>
        /// Clears all the Namespaces and Triples for the given Graph from the Database
        /// </summary>
        public abstract void ClearGraph(String graphID);

        /// <summary>
        /// Opens a Connection to the Database
        /// </summary>
        /// <param name="keepOpen">Indicates that the Connection should be kept open and a Transaction started</param>
        public abstract void Open(bool keepOpen);

        /// <summary>
        /// Closes the Connection to the Database
        /// </summary>
        /// <param name="forceClose">Indicates that the connection should be closed even if keepOpen was specified when the Connection was opened</param>
        public abstract void Close(bool forceClose);

        /// <summary>
        /// Closes the Connection to the Database
        /// </summary>
        /// <param name="forceClose">Indicates that the connection should be closed even if keepOpen was specified when the Connection was opened</param>
        /// <param name="rollbackTrans">Indicates that the Transaction should be rolled back because something has gone wrong</param>
        public abstract void Close(bool forceClose, bool rollbackTrans);

        /// <summary>
        /// Executes a Non-Query SQL Command against the database
        /// </summary>
        /// <param name="sqlCmd">SQL Command</param>
        public abstract void ExecuteNonQuery(string sqlCmd);

        /// <summary>
        /// Executes a Query SQL Command against the database and fills the given Data Table
        /// </summary>
        /// <param name="sqlCmd">SQL Command</param>
        /// <param name="data">Data Table to fill with the results</param>
        /// <remarks>Allows for typed Data Tables</remarks>
        protected abstract void ExecuteQuery(string sqlCmd, DataTable data);

        /// <summary>
        /// Executes a Query SQL Command against the database and returns a DataTable
        /// </summary>
        /// <param name="sqlCmd">SQL Command</param>
        /// <returns>DataTable of results</returns>
        public abstract DataTable ExecuteQuery(string sqlCmd);

        /// <summary>
        /// Returns a DataReader that can be used to read results from a query in a streaming fashion
        /// </summary>
        /// <param name="sqlCmd">SQL Command</param>
        /// <returns></returns>
        public abstract DbDataReader ExecuteStreamingQuery(string sqlCmd);

        /// <summary>
        /// Executes a Query SQL Command against the database and returns the scalar result (first column of first row of the result)
        /// </summary>
        /// <param name="sqlCmd">SQL Command</param>
        /// <returns>First Column of First Row of the Results</returns>
        public abstract object ExecuteScalar(string sqlCmd);

        /// <summary>
        /// Escapes Strings in a manner appropriate to the underlying Database
        /// </summary>
        /// <param name="text">String to escape</param>
        /// <returns>Escaped String</returns>
        public abstract string EscapeString(string text);

        #endregion

        #region ID Assigment

        /// <summary>
        /// Gets the next available Database Node ID if the Node is not in the Database or the existing Database Node ID
        /// </summary>
        /// <param name="n">Node to get an ID for</param>
        /// <param name="createRequired">Whether the Manager needs to create a Node Record in the Database</param>
        /// <returns></returns>
        protected virtual int GetNextNodeID(INode n, out bool createRequired)
        {
            int id = -1;
            createRequired = false;

            try
            {
                Monitor.Enter(this._nodeIDs);
                if (this._nextNodeID == -1)
                {
                    this.LoadNodeIDMap();
                }

                if (this._nodeIDs.ContainsKey(n.GetHashCode()))
                {
                    id = this._nodeIDs[n.GetHashCode()];

                    //Use the Node Collection to record Nodes we've requested IDs for
                    //This allows us to take advantage of the Node Collections ability to detect
                    //when Node Hash Codes collide and then we can use this in the next step to
                    //do a Database lookup if necessary
                    if (!this._nodeCollection.Contains(n))
                    {
                        this._nodeCollection.Add(n);
                    }

                    if (id == -1 || n.Collides)
                    {
                        //There are multiple Nodes with this Hash Code
                        //Lookup from Database
                        String getNodeID = "SELECT nodeID FROM NODES WHERE nodeType=" + (int)n.NodeType + " AND nodeValue=N'" + this.EscapeString(n.ToString()) + "'";
                        Object nodeID = this.ExecuteScalar(getNodeID);
                        if (nodeID != null)
                        {
                            return Int32.Parse(nodeID.ToString());
                        }
                        else
                        {
                            id = ++this._nextNodeID;
                            createRequired = true;
                        }
                    }
                }
                else
                {
                    id = ++this._nextNodeID;
                    this._nodeIDs.Add(n.GetHashCode(), id);
                    createRequired = true;
                }
            }
            finally
            {
                Monitor.Exit(this._nodeIDs);
            }
            if (id != -1)
            {
                return id;
            }
            else
            {
                throw new RdfStorageException("Error obtaining a Node ID");
            }
        }

        /// <summary>
        /// Loads the Node Hash Code to Database ID Map from the Database and sets the Next Node ID to the maximum in-use Node ID
        /// </summary>
        /// <remarks>
        /// Since Hash Codes may collide where there is a collision the implementor should set the ID mapped to the Hash as -1, this indicates to the <see cref="BaseStoreManager.GetNextNodeID">GetNextNodeID()</see> method that it must do a database lookup to determine the correct Node ID in this case
        /// </remarks>
        protected abstract void LoadNodeIDMap();

        /// <summary>
        /// Loads the Database Graph ID to Graph URI Map
        /// </summary>
        protected abstract void LoadGraphUriMap();

        /// <summary>
        /// Gets the next available Database Triple ID if the Triple is not in the Database of the existing Database Triple ID
        /// </summary>
        /// <param name="t">Triple to get an ID for</param>
        /// <param name="createRequired">Whether the Manager needs to create a Triple Record in the Database</param>
        /// <returns></returns>
        protected virtual int GetNextTripleID(Triple t, out bool createRequired)
        {
            int id = -1;
            createRequired = false;

            try
            {
                Monitor.Enter(this._tripleIDs);
                if (this._nextTripleID == -1)
                {
                    this.LoadTripleIDMap();
                }

                //Use the Triple Collection to record Triples we've requested IDs for
                //This allows us to take advantage of the Triple Collections ability to detect
                //when Triple Hash Codes collide and then we can use this in the next step to
                //do a Database lookup if necessary
                if (!this._tripleCollection.Contains(t))
                {
                    this._tripleCollection.Add(t);
                }

                if (this._tripleIDs.ContainsKey(t.GetHashCode()))
                {
                    id = this._tripleIDs[t.GetHashCode()];

                    if (id == -1 || t.Collides)
                    {
                        //There are multiple Triples with this Hash Code
                        //Lookup from Database
                        String s, p, o;
                        s = this.SaveNode(t.Subject);
                        p = this.SaveNode(t.Predicate);
                        o = this.SaveNode(t.Object);
                        String getTripleID = "SELECT tripleID FROM TRIPLES WHERE tripleSubject=" + s + " AND triplePredicate=" + p + " AND tripleObject=" + o;
                        Object tripleID = this.ExecuteScalar(getTripleID);
                        if (tripleID != null)
                        {
                            return Int32.Parse(tripleID.ToString());
                        }
                        else
                        {
                            id = ++this._nextTripleID;
                            createRequired = true;
                        }
                    }
                }
                else
                {
                    id = ++this._nextTripleID;
                    this._tripleIDs.Add(t.GetHashCode(), id);
                    createRequired = true;
                }
            }
            finally
            {
                Monitor.Exit(this._tripleIDs);
            }
            if (id != -1)
            {
                return id;
            }
            else
            {
                throw new RdfStorageException("Error obtaining a Triple ID");
            }
        }

        /// <summary>
        /// Loads the Triple Hash Code to Databas ID Map from the Database and sets the Next Triple ID to the maximum in-use Triple ID
        /// </summary>
        /// <remarks>
        /// Since Hash Codes may collide where there is a collision the implementor should set the ID mapped to the Hash as -1, this indicates to the <see cref="BaseStoreManager.GetNextTripleID">GetNextTripleID()</see> method that it must do a database lookup to determine the correct Triple ID in this case
        /// </remarks>
        protected abstract void LoadTripleIDMap();

        #endregion

        #region Buffered Writing

        /// <summary>
        /// Internal Helper Method which is run as a Background Thread and writes the Buffer to the Database
        /// </summary>
        private void ProcessBuffer()
        {
            int total;
            while (true)
            {
                //Get how many Triples are in the Buffer
                lock (this._writerBuffer)
                {
                    total = this._writerBuffer.Count;
                }

                //Write them if they meet our Batch Size ir we've been asked to Flush the Buffer
                if ((this._flushWriter && total > 0) || total >= this._batchSize)
                {
                    this._flushWriter = false;
                    try
                    {
                        this.Open(true);

                        //Write all the Triples that were in the Buffer when we counted it just now
                        BatchTriple b;
                        total = Math.Min(total, this._batchSize);
                        while (total > 0)
                        {
                            lock (this._writerBuffer)
                            {
                                b = this._writerBuffer.First.Value;
                                this._writerBuffer.RemoveFirst();
                            }
                            this.SaveTripleInternal(b);
                            total--;
                        }

                        this.Close(true);
                    }
                    catch
                    {
                        this.Close(true, true);
                        throw;
                    }
                }
                else if (this._flushWriter && total == 0)
                {
                    this._flushWriter = false;
                }

                //Get an updated Buffer size
                lock (this._writerBuffer)
                {
                    total = this._writerBuffer.Count;
                }

                if (total == 0 && this._terminateWriter)
                {
                    //Stop since the Buffer is empty and we've been asked to terminate
                    return;
                }
                else if (!this._flushWriter && total < this._batchSize)
                {
                    //Sleep since we've not been asked to Flush and the Buffer is below the Batch size
                    Thread.Sleep(250);
                }
            }
        }

        #endregion

        #region Generic IO Manager Implementation

        /// <summary>
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <remarks>
        /// If the Graph URI doesn't exist in the Store nothing will be returned
        /// </remarks>
        public virtual void LoadGraph(IGraph g, Uri graphUri)
        {
            this.LoadGraph(new GraphHandler(g), graphUri);
        }

        /// <summary>
        /// Loads a Graph from the Store using the given RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public virtual void LoadGraph(IRdfHandler handler, Uri graphUri)
        {
            try
            {
                this.Open(true);

                if (this.Exists(graphUri))
                {
                    //Load into an Empty Graph and then Merge
                    IGraph g = new Graph();

                    //Load Namespaces and Triples
                    String graphID = this.GetGraphID(graphUri);
                    this.LoadNamespaces(g, graphID);
                    this.LoadTriples(g, graphID);

                    handler.Apply(g);
                }
                else
                {
                    handler.Apply((IGraph)null);
                }

                this.Close(true);
            }
            catch
            {
                this.Close(true, true);
                throw;
            }
        }

        /// <summary>
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">Uri of the Graph to load</param>
        /// <remarks>
        /// If the Graph Uri doesn't exist in the Store nothing will be returned
        /// </remarks>
        public virtual void LoadGraph(IGraph g, String graphUri)
        {
            Uri u = (graphUri.Equals(String.Empty)) ? null : new Uri(graphUri);
            this.LoadGraph(g, u);
        }

        /// <summary>
        /// Loads a Graph from the Store using the given RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public virtual void LoadGraph(IRdfHandler handler, String graphUri)
        {
            Uri u = (graphUri.Equals(String.Empty)) ? null : new Uri(graphUri);
            this.LoadGraph(handler, u);
        }

        /// <summary>
        /// Saves a Graph to the Store (Warning: Completely replaces any existing Graph with the same URI)
        /// </summary>
        /// <param name="g">Graph to Save</param>
        /// <remarks>
        /// Saving a Graph will overwrite any existing Graph of the same Uri.
        /// <br /><br />
        /// A Graph with a Null Base Uri <strong>cannot</strong> be saved.
        /// </remarks>
        public virtual void SaveGraph(IGraph g)
        {
            try
            {
                //Open the Database Connection
                this.Open(true);

                //Graph Properties
                String graphID = String.Empty;
                Uri graphUri;
                if (g.BaseUri != null)
                {
                    graphUri = g.BaseUri;
                }
                else
                {
                    throw new RdfStorageException("Unable to store this Graph since it has no Base URI");
                }

                //Retrieve the existing Graph ID if any
                graphID = this.GetGraphID(graphUri);

                //Delete all existing Triples for this Graph
                this.ClearGraph(graphID);

                //Save Namespaces
                foreach (String prefix in g.NamespaceMap.Prefixes)
                {
                    this.SaveNamespace(prefix, g.NamespaceMap.GetNamespaceUri(prefix), graphID);
                }

                //Save Triples
                foreach (Triple t in g.Triples)
                {
                    this.SaveTriple(t, graphID);
                }

                //Close Database
                this.Close(true);

                //Wait for Complete
                while (!this.HasCompleted)
                {
                    Thread.Sleep(100);
                }
            }
            catch
            {
                this.Close(true, true);
                throw;
            }
        }

        /// <summary>
        /// Gets the IO Behaviour of the Store
        /// </summary>
        public virtual IOBehaviour IOBehaviour
        {
            get
            {
                return IOBehaviour.IsQuadStore | IOBehaviour.HasDefaultGraph | IOBehaviour.HasNamedGraphs | IOBehaviour.OverwriteDefault | IOBehaviour.OverwriteNamed | IOBehaviour.CanUpdateTriples;
            }
        }

        /// <summary>
        /// Updates a Graph in the Store
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to update</param>
        /// <param name="additions">Triples to add to the Graph</param>
        /// <param name="removals">Triples to remove from the Graph</param>
        /// <remarks>
        /// If the Graph Uri is null of the Graph doesn't exist in the Store a <see cref="RdfStorageException">RdfStorageException</see> will be thrown
        /// </remarks>
        /// <exception cref="RdfStorageException">Thrown when the Graph Uri is null or the Graph to be updated doesn't exist in the Store</exception>
        public virtual void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            try
            {
                this.Open(true);

                //Get the Graph ID
                String graphID = this.GetGraphID(graphUri);

                //Add Triples
                if (additions != null)
                {
                    foreach (Triple t in additions)
                    {
                        this.SaveTriple(t, graphID);
                    }
                }
                this.Flush();

                //Delete Triples
                if (removals != null)
                {
                    foreach (Triple t in removals)
                    {
                        this.RemoveTriple(t, graphID);
                    }
                }
                this.Flush();

                this.Close(true);
            }
            catch
            {
                this.Close(true, true);
                throw;
            }
        }

        /// <summary>
        /// Updates a Graph in the Store
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to update</param>
        /// <param name="additions">Triples to add to the Graph</param>
        /// <param name="removals">Triples to remove from the Graph</param>
        /// <remarks>
        /// If the Graph Uri is null of the Graph doesn't exist in the Store a <see cref="RdfStorageException">RdfStorageException</see> will be thrown
        /// </remarks>
        /// <exception cref="RdfStorageException">Thrown when the Graph Uri is null or the Graph to be updated doesn't exist in the Store</exception>
        public virtual void UpdateGraph(String graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            Uri u = (graphUri.Equals(String.Empty)) ? null : new Uri(graphUri);
            this.UpdateGraph(u, additions, removals);
        }

        /// <summary>
        /// Indicates that Updates are supported by SQL based Store Manager
        /// </summary>
        public virtual bool UpdateSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Deletes a Graph from the Store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        public virtual void DeleteGraph(Uri graphUri)
        {
            this.RemoveGraph(this.GetGraphID(graphUri));
        }

        /// <summary>
        /// Deletes a Graph from the Store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        public virtual void DeleteGraph(String graphUri)
        {
            Uri u = (graphUri.ToSafeString().Equals(String.Empty)) ? null : new Uri(graphUri);
            this.DeleteGraph(u);
        }

        /// <summary>
        /// Returns that deleting Graphs is supported
        /// </summary>
        public bool DeleteSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Lists the URIs of Graphs in the Store
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<Uri> ListGraphs()
        {
            return this.GetGraphUris();
        }

        /// <summary>
        /// Returns that listing Graph URIs is supported
        /// </summary>
        public virtual bool ListGraphsSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns that the Manager is ready
        /// </summary>
        public virtual bool IsReady
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns that the Manager is not read-only
        /// </summary>
        public virtual bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        #endregion

        /// <summary>
        /// Base Implementation of Dispose which stops the Writer Thread and clears all the Mapping dictionaries unless <see cref="BaseStoreManager.PreserveState">PreserveState</see> has been set as True
        /// </summary>
        public virtual void Dispose()
        {
            if (!this._preserveState)
            {
                //Stop the Writer Thread
                this._flushWriter = true;
                this._terminateWriter = true;
                while (!this.HasCompleted)
                {
                    Thread.Sleep(250);
                }

                //Clear Triple Collection
                this._tripleCollection.Dispose();

                //Clear Mapping dictionaries
                this._graphIDs.Clear();
                this._nodeIDs.Clear();
                this._tripleIDs.Clear();
                this._nodes.Clear();
                this._nsPrefixIDs.Clear();
                this._nsUriIDs.Clear();
                if (this._graphUris != null)
                {
                    this._graphUris.Clear();
                    this._graphUris = null;
                }

                //Reset Next Node and Triple ID
                this._nextNodeID = -1;
                this._nextTripleID = -1;

                //Poll until the writer thread has stopped
                while (this._writer.ThreadState != ThreadState.Stopped)
                {
                    Thread.Sleep(100);
                }
            }
            else
            {
                //Flush the Writer Thread
                this._flushWriter = true;
                while (!this.HasCompleted)
                {
                    Thread.Sleep(250);
                }
            }
        }

        /// <summary>
        /// Flushes any outstanding changes to the underlying store
        /// </summary>
        public void Flush()
        {
            //Flush the Writer Thread
            this._flushWriter = true;
            while (!this.HasCompleted)
            {
                Thread.Sleep(250);
            }
        }

        #region IConfigurationSerializable Members

        /// <summary>
        /// Serializes the connection's configuration
        /// </summary>
        /// <param name="context">Configuration Serialization Context</param>
        public virtual void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            INode manager = context.NextSubject;
            INode rdfType = context.Graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            INode rdfsLabel = context.Graph.CreateUriNode(new Uri(NamespaceMapper.RDFS + "label"));
            INode dnrType = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyType);
            INode sqlManager = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.ClassSqlManager);
            INode server = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyServer);
            INode database = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyDatabase);

            context.Graph.Assert(new Triple(manager, rdfType, sqlManager));
            context.Graph.Assert(new Triple(manager, rdfsLabel, context.Graph.CreateLiteralNode(this.ToString())));
            context.Graph.Assert(new Triple(manager, dnrType, context.Graph.CreateLiteralNode(this.GetType().FullName)));
            context.Graph.Assert(new Triple(manager, server, context.Graph.CreateLiteralNode(this._dbserver)));
            context.Graph.Assert(new Triple(manager, database, context.Graph.CreateLiteralNode(this._dbname)));

            if (this._dbuser != null && this._dbpwd != null)
            {
                INode username = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyUser);
                INode pwd = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyPassword);
                context.Graph.Assert(new Triple(manager, username, context.Graph.CreateLiteralNode(this._dbuser)));
                context.Graph.Assert(new Triple(manager, pwd, context.Graph.CreateLiteralNode(this._dbpwd)));
            }
        }

        #endregion
    }
}

#endif