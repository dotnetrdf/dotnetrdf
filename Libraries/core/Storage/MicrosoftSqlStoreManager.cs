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
using System.Text.RegularExpressions;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Reflection;
using System.IO;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// A <see cref="ISqlIOManager">ISqlIOManager</see> implementation which uses Microsoft SQL Server as the backing store
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
    /// If the Database specified is not currently a dotNetRDF Store then the appropriate database tables will be created automatically.  If the Database is a dotNetRDF Store which does not use the database format then an <see cref="RdfStorageException">RdfStorageException</see> will be thrown as the database setup will try to recreate the existing tables and thus fail.
    /// </remarks>
    [Obsolete("The legacy SQL store format and related classes are officially deprecated - please see http://www.dotnetrdf.org?content.asp?pageID=dotNetRDF%20Store#migration for details on upgrading to the new ADO store format", false)]
	public class MicrosoftSqlStoreManager 
        : BaseStoreManager, IDotNetRDFStoreManager
	{
        private Dictionary<int, SqlConnection> _dbConnections = new Dictionary<int, SqlConnection>();
        private Dictionary<int, SqlTransaction> _dbTrans = new Dictionary<int, SqlTransaction>();
        private Dictionary<int, bool> _dbKeepOpen = new Dictionary<int, bool>();

        /// <summary>
        /// Empty Constructor for derived classes to use
        /// </summary>
        protected MicrosoftSqlStoreManager() : base() { }

        /// <summary>
        /// Creates a new instance of the Microsoft SQL Server Store Manager
        /// </summary>
        /// <param name="server">Server</param>
        /// <param name="db">Database Name</param>
        /// <param name="user">Username</param>
        /// <param name="password">Password</param>
        public MicrosoftSqlStoreManager(String server, String db, String user, String password)
            : base(server, db, user, password)
        {      
            //Check the Database is ready for use
            try
            {
                this.Open(true);

                try
                {
                    //Execute an arbitrary lookup, result is irrelevant
                    //If it throws an error then the relevant tables don't exist and we set up the database
                    //This also ensures we have a Store of the correct version (0.1.1)
                    //If the graphHash field doesn't exist but the GRAPHS table does then trying to setup
                    //a new Store will fail because tables with the relevant names will all already exist
                    this.ExecuteScalar("SELECT graphHash FROM GRAPHS WHERE graphID=1");
                }
                catch (SqlException)
                {
                    //Need to set up the Database

                    //Read the Setup SQL Script into a local string (it's an embedded resource in the assembly)
                    StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("VDS.RDF.Storage.CreateMSSQLStoreTables.sql"));
                    String setup = reader.ReadToEnd();
                    reader.Close();

                    try
                    {
                        this.ExecuteNonQuery(setup);
                    }
                    catch (SqlException sqlEx)
                    {
                        this.Close(true, true);
                        throw new RdfStorageException("Unable to carry out the necessary Database Setup actions required to prepare this Database for RDF Storage", sqlEx);
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                this.Close(true, true);
                throw new RdfStorageException("Unable to verify that the Database is ready for RDF Storage", sqlEx);
            }

            this.Close(true);
        }

        /// <summary>
        /// Creates a new instance of the Microsoft SQL Server Store Manager
        /// </summary>
        /// <param name="db">Database Name</param>
        /// <param name="user">Username</param>
        /// <param name="password">Password</param>
        /// <remarks>Assumes that the Database is on the <strong>localhost</strong></remarks>
        public MicrosoftSqlStoreManager(String db, String user, String password)
            : this("localhost", db, user, password) { }

        /// <summary>
        /// Creates a new instance of the Microsoft SQL Server Store Manager using Integrated Authentication
        /// </summary>
        /// <param name="server">Servername</param>
        /// <param name="db">Database Name</param>
        public MicrosoftSqlStoreManager(String server, String db)
            : this(server, db, null, null) { }

        /// <summary>
        /// Creates a new instance of the Microsoft SQL Server Store Manager using Integrated Authentication
        /// </summary>
        /// <param name="db">Database Name</param>
        /// <remarks>Assumes that the Database is on the <strong>localhost</strong></remarks>
        public MicrosoftSqlStoreManager(String db)
            : this("localhost", db, null, null) { }

        #region Manager Implementation

        /// <summary>
        /// Gets the ID for a Graph in the Store, creates a new Graph in the store if necessary
        /// </summary>
        /// <param name="graphUri">Graph Uri</param>
        /// <returns></returns>
        public override string GetGraphID(Uri graphUri)
        {
            if (graphUri == null) graphUri = new Uri(GraphCollection.DefaultGraphUri);

            if (this._graphIDs.ContainsKey(graphUri.GetEnhancedHashCode()))
            {
                //Looked up from mapping dictionary
                String id;
                try
                {
                    Monitor.Enter(this._graphIDs);
                    id = this._graphIDs[graphUri.GetEnhancedHashCode()];
                }
                finally
                {
                    Monitor.Exit(this._graphIDs);
                }

                return id;
            }
            else
            {
                //Get from Database
                this.Open(false);
                String getID = "SELECT graphID FROM GRAPHS WHERE graphHash=" + graphUri.GetEnhancedHashCode() + " OR graphURI=N'" + this.EscapeString(graphUri.ToString()) + "'";
                Object id = this.ExecuteScalar(getID);
                if (id == null)
                {
                    //Need to create a new Graph ID
                    String createID = "INSERT INTO GRAPHS (graphURI, graphHash) VALUES (N'" + this.EscapeString(graphUri.ToString()) + "'," + graphUri.GetEnhancedHashCode() + ")";
                    try
                    {
                        this.ExecuteNonQuery(createID);
                    }
                    catch (SqlException sqlEx)
                    {
                        this.Close(true, true);
                        throw new RdfStorageException("Unable to create a record for the given Graph in the Database", sqlEx);
                    }

                    //Get this new ID
                    id = this.ExecuteScalar(getID);
                }
                this.Close(false);

                //Add to Mapping Dictionary
                try
                {
                    Monitor.Enter(this._graphIDs);
                    if (!this._graphIDs.ContainsKey(graphUri.GetEnhancedHashCode()))
                    {
                        this._graphIDs.Add(graphUri.GetEnhancedHashCode(), id.ToString());
                    }
                }
                finally
                {
                    Monitor.Exit(this._graphIDs);
                }

                return id.ToString();
            }
        }

        /// <summary>
        /// Gets the URI of a Graph based on its ID in the Store
        /// </summary>
        /// <param name="graphID">Graph ID</param>
        /// <returns></returns>
        public override Uri GetGraphUri(string graphID)
        {
            if (this._graphUris == null) this.LoadGraphUriMap();

            if (this._graphUris.ContainsKey(graphID))
            {
                return this._graphUris[graphID];
            }
            else
            {

                try
                {
                    this.Open(false);
                    String getID = "SELECT graphUri FROM GRAPHS WHERE graphID=" + graphID;
                    Object id = this.ExecuteScalar(getID);
                    if (id != null) throw new RdfStorageException("A Graph with the given ID does not exist in the Store");
                    Uri graphUri = new Uri(id.ToString());
                    if (graphUri.ToString().Equals(GraphCollection.DefaultGraphUri))
                    {
                        graphUri = null;
                    }
                    this.Close(false);

                    //Cache and Return
                    lock (this._graphUris)
                    {
                        if (!this._graphUris.ContainsKey(graphID))
                        {
                            this._graphUris.Add(graphID, graphUri);
                        }
                    }
                    return graphUri;
                }
                catch
                {
                    this.Close(true, true);
                    throw;
                }
            }
        }

        /// <summary>
        /// Determines whether a given Graph exists in the Store
        /// </summary>
        /// <param name="graphUri">Graph Uri</param>
        /// <returns></returns>
        public override bool Exists(Uri graphUri)
        {
            if (graphUri == null) graphUri = new Uri(GraphCollection.DefaultGraphUri);

            if (this._graphIDs.ContainsKey(graphUri.GetEnhancedHashCode()))
            {
                return true;
            }
            else
            {
                bool result = false;

                //Get from Database
                try
                {
                    this.Open(false);
                    String getID = "SELECT graphID FROM GRAPHS WHERE graphHash=" + graphUri.GetEnhancedHashCode() + " OR graphUri=N'" + this.EscapeString(graphUri.ToString()) + "'";
                    Object id = this.ExecuteScalar(getID);
                    if (id != null) result = true;
                    this.Close(false);
                }
                catch (SqlException)
                {
                    this.Close(true);
                }

                return result;
            }
        }

        /// <summary>
        /// Gets the URIs of Graphs in the Store
        /// </summary>
        /// <returns></returns>
        public override List<Uri> GetGraphUris()
        {
            String getGraphs = "SELECT * FROM GRAPHS ORDER BY graphID ASC";
            this.Open(false);
            DataTable data = this.ExecuteQuery(getGraphs);
            this.Close(false);

            List<Uri> uris = new List<Uri>();
            foreach (DataRow r in data.Rows)
            {
                if (r["graphUri"].ToString().Equals(GraphCollection.DefaultGraphUri))
                {
                    uris.Add(null);
                }
                else
                {
                    uris.Add(new Uri(r["graphUri"].ToString()));
                }
            }

            return uris;
        }

        /// <summary>
        /// Loads Namespaces for the Graph with the given ID into the given Graph object
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphID">Database Graph ID</param>
        public override void LoadNamespaces(IGraph g, string graphID)
        {
            //Build the SQL for getting the Namespaces
            String getNamespaces = "SELECT * FROM NAMESPACES N INNER JOIN NS_PREFIXES P ON N.nsPrefixID=P.nsPrefixID INNER JOIN NS_URIS U ON N.nsUriID=U.nsUriID WHERE graphID=" + graphID;

            //Get the Data
            this.Open(false);
            DataTable data = this.ExecuteQuery(getNamespaces);
            this.Close(false);

            //Load the Namespace into the Graph
            String prefix, uri;
            foreach (DataRow r in data.Rows)
            {
                //Get Prefix and Uri from Row
                prefix = r["nsPrefix"].ToString();
                uri = r["nsUri"].ToString();
                Uri u = new Uri(uri);

                //Add to Graph
                g.NamespaceMap.AddNamespace(prefix, u);

                //Store the IDs for later if not already stored
                try
                {
                    Monitor.Enter(this._nsPrefixIDs);
                    Monitor.Enter(this._nsUriIDs);

                    if (!this._nsPrefixIDs.ContainsKey(prefix))
                    {
                        this._nsPrefixIDs.Add(prefix, r["nsPrefixID"].ToString());
                    }
                    if (!this._nsUriIDs.ContainsKey(u.GetEnhancedHashCode()))
                    {
                        this._nsUriIDs.Add(u.GetEnhancedHashCode(), r["nsUriID"].ToString());
                    }
                }
                finally
                {
                    Monitor.Exit(this._nsPrefixIDs);
                    Monitor.Exit(this._nsUriIDs);
                }
            }
        }

        /// <summary>
        /// Loads Triples for the Graph with the given ID into the given Graph object
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphID">Database Graph ID</param>
        public override void LoadTriples(IGraph g, string graphID)
        {
            //Build the SQL for getting the Triples
            String getTriples = "SELECT * FROM GRAPH_TRIPLES G INNER JOIN TRIPLES T ON G.tripleID=T.tripleID WHERE G.graphID=" + graphID;

            //Get the Data
            this.Open(true);
            DataTable data = this.ExecuteQuery(getTriples);

            //Load the Triples
            String s, p, o;
            INode subj, pred, obj;
            foreach (DataRow r in data.Rows)
            {
                //Get the IDs
                s = r["tripleSubject"].ToString();
                p = r["triplePredicate"].ToString();
                o = r["tripleObject"].ToString();

                //Get the Nodes from these IDs
                subj = this.LoadNode(g, s);
                pred = this.LoadNode(g, p);
                obj = this.LoadNode(g, o);

                Triple t = new Triple(subj, pred, obj);
                g.Assert(t);

                //Store ID for later
                try
                {
                    Monitor.Enter(this._tripleIDs);
                    if (!this._tripleIDs.ContainsKey(t.GetHashCode()))
                    {
                        int id = Int32.Parse(r["tripleID"].ToString());
                        this._tripleIDs.Add(t.GetHashCode(), id);
                    }
                }
                finally
                {
                    Monitor.Exit(this._tripleIDs);
                }
            }

            //Close the Database Connection
            this.Close(true);
        }

        /// <summary>
        /// Loads a Node from the Database into the relevant Graph
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="nodeID">Database Node ID</param>
        public override INode LoadNode(IGraph g, string nodeID)
        {
            if (this._nodes.ContainsKey(nodeID))
            {
                //Known in our Map already
                INode temp;
                try
                {
                    Monitor.Enter(this._nodes);
                    temp = this._nodes[nodeID];
                }
                finally
                {
                    Monitor.Exit(this._nodes);
                }

                if (temp.Graph == g)
                {
                    //Graph matches so just return
                    return temp;
                }
                else
                {
                    //Need to copy into the Graph
                    return Tools.CopyNode(temp, g);
                }
            }
            else
            {
                //Retrieve from the Database
                String getID = "SELECT * FROM NODES WHERE nodeID=" + nodeID;
                DataTable data = this.ExecuteQuery(getID);

                if (data.Rows.Count == 0)
                {
                    this.Close(true, true);
                    throw new RdfStorageException("Unable to load a required Node Record from the Database, Node ID '" + nodeID + "' is missing");
                }
                else
                {
                    DataRow r = data.Rows[0];

                    int type;
                    String value;
                    INode n;
                    type = Int32.Parse(r["nodeType"].ToString());
                    value = r["nodeValue"].ToString().Normalize();

                    //Parse the Node Value based on the Node Type
                    switch ((NodeType)type)
                    {
                        case NodeType.Blank:
                            //Ignore the first two characters which will be _:
                            value = value.Substring(2);
                            n = g.CreateBlankNode(value);
                            break;
                        case NodeType.Literal:
                            //Extract Data Type or Language Specifier as appropriate
                            String lit, typeorlang;
                            if (value.Contains("^^"))
                            {
                                lit = value.Substring(0, value.LastIndexOf("^^"));
                                typeorlang = value.Substring(value.LastIndexOf("^^") + 2);
                                n = g.CreateLiteralNode(lit, new Uri(typeorlang));
                            }
                            else if (_validLangSpecifier.IsMatch(value))
                            {
                                lit = value.Substring(0, value.LastIndexOf("@"));
                                typeorlang = value.Substring(value.LastIndexOf("@") + 1);
                                n = g.CreateLiteralNode(lit, typeorlang);
                            }
                            else
                            {
                                n = g.CreateLiteralNode(value);
                            }

                            INode orig = n;

                            //Check the Hash Code
                            int expectedHash = Int32.Parse(r["nodeHash"].ToString());
                            if (expectedHash != n.GetHashCode())
                            {
                                //We've wrongly decoded the info?
                                ILiteralNode ln = (ILiteralNode)n;
                                if (!ln.Language.Equals(String.Empty))
                                {
                                    //Wrongly attached a Language Specifier?
                                    n = g.CreateLiteralNode(value);
                                    if (expectedHash != n.GetHashCode())
                                    {
                                        n = orig;
                                        //throw new RdfStorageException("Unable to decode a Stored Node into a Literal Node correctly");
                                    }
                                }
                                else if (ln.DataType != null)
                                {
                                    //Wrongly attached a Data Type?
                                    n = g.CreateLiteralNode(value);
                                    if (expectedHash != n.GetHashCode())
                                    {
                                        //Should have been a Language Specifier instead?
                                        if (_validLangSpecifier.IsMatch(value))
                                        {
                                            lit = value.Substring(0, value.LastIndexOf("@"));
                                            typeorlang = value.Substring(value.LastIndexOf("@") + 1);
                                            n = g.CreateLiteralNode(lit, typeorlang);
                                            if (expectedHash != n.GetHashCode())
                                            {
                                                n = orig;
                                                //throw new RdfStorageException("Unable to decode a Stored Node into a Literal Node correctly");
                                            }
                                        }
                                        else
                                        {
                                            n = orig;
                                        }
                                    }
                                }
                            }

                            break;
                        case NodeType.Uri:
                            //Uri is the Value
                            n = g.CreateUriNode(new Uri(value));
                            break;
                        default:
                            this.Close(true, true);
                            throw new RdfParseException("The Node Record is for a Node of Unknown Type");
                    }

                    //Store this for later when we write stuff back to the Database
                    try
                    {
                        Monitor.Enter(this._nodes);
                        if (!this._nodes.ContainsKey(nodeID))
                        {
                            this._nodes.Add(nodeID, n);
                        }
                    }
                    finally
                    {
                        Monitor.Exit(this._nodes);
                    }

                    return n;
                }

            }
        }

        /// <summary>
        /// Gets the Node ID for a Node in the Database creating a new Database record if necessary
        /// </summary>
        /// <param name="n">Node to Save</param>
        /// <returns></returns>
        public override string SaveNode(INode n)
        {
            bool create;
            int id = this.GetNextNodeID(n, out create);

            if (create)
            {
                //Doesn't exist so Insert a Node Record
                String createNode = "INSERT INTO NODES (nodeID, nodeType, nodeValue, nodeHash) VALUES (" + id + ", " + (int)n.NodeType + ", N'" + this.EscapeString(n.ToString()) + "', " + n.GetHashCode() + ")";
                try
                {
                    this.ExecuteNonQuery(createNode);
                }
                catch (SqlException sqlEx)
                {
                    this.Close(true, true);
                    throw new RdfStorageException("Unable to create a Node Record in the Database", sqlEx);
                }
            }

            return id.ToString();
        }

        /// <summary>
        /// Saves a Triple from a Graph into the Database
        /// </summary>
        /// <param name="b">Triple to save</param>
        protected override void SaveTripleInternal(BatchTriple b)
        {
            //if (this._noTrans) this.Open(true);

            Triple t = b.Triple;
            String graphID = b.GraphID;

            bool create;
            int id = this.GetNextTripleID(t, out create);

            if (create) {
                //Get the IDs for the Nodes
                String s, p, o;
                s = this.SaveNode(t.Subject);
                p = this.SaveNode(t.Predicate);
                o = this.SaveNode(t.Object);

                //Doesn't exist so add to Database
                String addTriple = "INSERT INTO TRIPLES (tripleID, tripleSubject, triplePredicate, tripleObject, tripleHash) VALUES (" + id + ", " + s + ", " + p + ", " + o + ", " + t.GetHashCode() + ")";
                try
                {
                    this.ExecuteNonQuery(addTriple);
                }
                catch (SqlException sqlEx)
                {
                    this.Close(true, true);
                    throw new RdfStorageException("Unable to create a Triple record in the Database for Triple " + t.ToString(), sqlEx);
                }
            }

            //Is it already linked to the Graph?
            String isLinked = "SELECT * FROM GRAPH_TRIPLES WHERE graphID=" + graphID + " AND tripleID=" + id;
            if (this.ExecuteScalar(isLinked) == null)
            {
                //Add a link
                String addTriple = "INSERT INTO GRAPH_TRIPLES (graphID, tripleID) VALUES (" + graphID + ", " + id + ")";
                try
                {
                    this.ExecuteNonQuery(addTriple);
                }
                catch (SqlException sqlEx)
                {
                    this.Close(true, true);
                    throw new RdfStorageException("Unable to link a Triple record to a Graph in the Database", sqlEx);
                }
            }

            //this.Close(false);
        }

        /// <summary>
        /// Saves a Namespace from a Graph into the Database
        /// </summary>
        /// <param name="prefix">Namespace Prefix</param>
        /// <param name="u">Namespace Uri</param>
        /// <param name="graphID">Database Graph ID</param>
        public override void SaveNamespace(string prefix, Uri u, string graphID)
        {
            //First find out if the Namespace Prefix and Uri are in the Database
            String preID, uriID;

            this.Open(false);

            //Get the IDs
            preID = this.GetNSPrefixID(prefix);
            uriID = this.GetNSUriID(u);

            //Are we linked to this Namespace already?
            String nsRegistered = "SELECT nsID FROM NAMESPACES WHERE graphID=" + graphID + " AND nsPrefixID=" + preID + " AND nsUriID=" + uriID;
            if (this.ExecuteScalar(nsRegistered) == null)
            {
                //Link ourselves to the Namespace
                String addNamespace = "INSERT INTO NAMESPACES (graphID, nsPrefixID, nsUriID) VALUES (" + graphID + ", " + preID + ", " + uriID + ")";
                try
                {
                    this.ExecuteNonQuery(addNamespace);
                }
                catch (SqlException sqlEx)
                {
                    this.Close(true, true);
                    throw new RdfStorageException("Unable to link a Namespace record to the Graph in the Database", sqlEx);
                }
            }

            this.Close(false);
        }

        /// <summary>
        /// Removes a Triple from a Graphs Database record
        /// </summary>
        /// <param name="t">Triple to remove</param>
        /// <param name="graphID">Database Graph ID</param>
        public override void RemoveTriple(Triple t, string graphID)
        {
            //If the Triple being removed is in the Buffer to be added still remove it
            lock (this._writerBuffer)
            {
                this._writerBuffer.Remove(new BatchTriple(t, graphID));
            }

            //Does the Triple have a known Database ID
            if (this._tripleIDs.ContainsKey(t.GetHashCode()))
            {
                //Can use the existing ID
                int id = this._tripleIDs[t.GetHashCode()];
                String removeTriple = "DELETE FROM GRAPH_TRIPLES WHERE graphID=" + graphID + " AND tripleID=" + id;
                try
                {
                    this.Open(false);
                    this.ExecuteNonQuery(removeTriple);
                    this.Close(false);
                }
                catch (SqlException sqlEx)
                {
                    this.Close(true, true);
                    throw new RdfStorageException("Unable to unlink a Triple record to a Graph in the Database", sqlEx);
                }

            }
            else
            {
                //Get the IDs for the Nodes
                String s, p, o;
                s = this.SaveNode(t.Subject);
                p = this.SaveNode(t.Predicate);
                o = this.SaveNode(t.Object);

                //Does the Triple exist in the Database
                String getTriple = "SELECT tripleID FROM TRIPLES WHERE tripleHash=" + t.GetHashCode();
                Object id = this.ExecuteScalar(getTriple);
                if (id == null)
                {
                    //Nothing to do since the Triple isn't in the Database and thus can't be linked to the Graph for unlinking to be needed
                }
                else
                {
                    //Remove the link to the Triple
                    String removeTriple = "DELETE FROM GRAPH_TRIPLES WHERE graphID=" + graphID + " AND tripleID=" + id.ToString();
                    try
                    {
                        this.Open(false);
                        this.ExecuteNonQuery(removeTriple);
                        this.Close(false);
                    }
                    catch (SqlException sqlEx)
                    {
                        this.Close(true, true);
                        throw new RdfStorageException("Unable to unlink a Triple record to a Graph in the Database", sqlEx);
                    }
                }
            }
        }

        /// <summary>
        /// Removes a Namespace from a Graphs Database record
        /// </summary>
        /// <param name="prefix">Namespace Prefix</param>
        /// <param name="u">Namespace Uri</param>
        /// <param name="graphID">Database Graph ID</param>
        public override void RemoveNamespace(string prefix, Uri u, string graphID)
        {
            //First find out if the Namespace Prefix and Uri are in the Database
            String preID, uriID;

            this.Open(false);

            //Get the IDs
            preID = this.GetNSPrefixID(prefix);
            uriID = this.GetNSUriID(u);

            //Delete the link
            String removeNamespace = "DELETE FROM NAMESPACES WHERE graphID=" + graphID + " AND nsPrefixID=" + preID + " AND nsUriID=" + uriID;
            try
            {
                this.ExecuteNonQuery(removeNamespace);
            }
            catch (SqlException sqlEx)
            {
                this.Close(true, true);
                throw new RdfStorageException("Unable to unlink a Namespace Record from the Graph in the Database", sqlEx);
            }

            this.Close(false);
        }

        /// <summary>
        /// Removes a Graph from the Database
        /// </summary>
        /// <param name="graphID">Database Graph ID</param>
        public override void RemoveGraph(string graphID)
        {
            //Clear the Graph first
            this.ClearGraph(graphID);

            //Delete the Graph Record
            this.Open(false);
            try
            {

                String removeGraph = "DELETE FROM GRAPHS WHERE graphID=" + graphID;
                this.ExecuteNonQuery(removeGraph);

                //Remove from Mapping Dictionary
                try
                {
                    int id = (from gs in this._graphIDs
                              where gs.Value == graphID
                              select gs.Key).First();

                    lock (this._graphIDs)
                    {
                        this._graphIDs.Remove(id);
                    }
                    if (this._graphUris != null)
                    {
                        lock (this._graphUris)
                        {
                            this._graphUris.Remove(graphID);
                        }
                    }
                }
                catch
                {
                    //This might happen if the Graph wasn't in our Map
                    //But we don't care since we didn't want it in there anyway!
                }

            }
            catch (SqlException sqlEx)
            {
                this.Close(true, true);
                throw new RdfStorageException("Unable to remove the Graph Record from the Database", sqlEx);
            }
            this.Close(false);
        }

        /// <summary>
        /// Changes the Uri associated with the Prefix in a Graphs Database record
        /// </summary>
        /// <param name="prefix">Namespace Prefix</param>
        /// <param name="u">New Namespace Uri</param>
        /// <param name="graphID">Database Graph ID</param>
        public override void UpdateNamespace(string prefix, Uri u, string graphID)
        {
            //First find out if the Namespace Prefix and Uri are in the Database
            String preID, uriID;

            this.Open(false);

            //Get the IDs
            preID = this.GetNSPrefixID(prefix);
            uriID = this.GetNSUriID(u);

            //Are we linked to this Namespace already?
            String nsRegistered = "SELECT nsID FROM NAMESPACES WHERE graphID=" + graphID + " AND nsPrefixID=" + preID + " AND nsUriID=" + uriID;
            if (this.ExecuteScalar(nsRegistered) == null)
            {
                //Link ourselves to the Namespace
                String addNamespace = "INSERT INTO NAMESPACES (graphID, nsPrefixID, nsUriID) VALUES (" + graphID + ", " + preID + ", " + uriID + ")";
                try
                {
                    this.ExecuteNonQuery(addNamespace);
                }
                catch (SqlException sqlEx)
                {
                    this.Close(true, true);
                    throw new RdfStorageException("Unable to link a Namespace record to the Graph in the Database", sqlEx);
                }
            }
            else
            {
                //Update the existing link
                String updateNamespace = "UPDATE NAMESPACES SET nsUriID=" + uriID + " WHERE graphID=" + graphID + " AND nsPrefixID=" + preID;
                try
                {
                    this.ExecuteNonQuery(updateNamespace);
                }
                catch (SqlException sqlEx)
                {
                    this.Close(true, true);
                    throw new RdfStorageException("Unable to update a Namespace record for the Graph in the Database", sqlEx);
                }
            }

            this.Close(false);
        }

        /// <summary>
        /// Clears all the Namespaces and Triples for the given Graph from the Database
        /// </summary>
        public override void ClearGraph(string graphID)
        {
            this.Open(true);
            try
            {
                String removeNamespaces = "DELETE FROM NAMESPACES WHERE graphID=" + graphID;
                String removeTriples = "DELETE FROM GRAPH_TRIPLES WHERE graphID=" + graphID;

                this.ExecuteNonQuery(removeNamespaces);
                this.ExecuteNonQuery(removeTriples);
            }
            catch (SqlException sqlEx)
            {
                this.Close(true, true);
                throw new RdfStorageException("Unable to Clear Namespace and Triple Records for the Graph from the Database", sqlEx);
            }

            this.Close(true);
        }

        /// <summary>
        /// Looks up the ID for a Namespace Prefix from the Database adding it to the Database if necessary
        /// </summary>
        /// <param name="prefix">Namespace Prefix</param>
        /// <returns></returns>
        private String GetNSPrefixID(String prefix)
        {
            String preID;
            Object id;

            //Is it in the Dictionary already?
            if (this._nsPrefixIDs.ContainsKey(prefix))
            {
                try
                {
                    Monitor.Enter(this._nsPrefixIDs);
                    preID = this._nsPrefixIDs[prefix];
                }
                finally
                {
                    Monitor.Exit(this._nsPrefixIDs);
                }
            }
            else
            {
                //Lookup from Database
                String getPrefix = "SELECT nsPrefixID FROM NS_PREFIXES WHERE nsPrefix='" + prefix + "'";
                id = this.ExecuteScalar(getPrefix);
                if (id == null)
                {
                    //Needs adding to Database
                    String addPrefix = "INSERT INTO NS_PREFIXES (nsPrefix) VALUES ('" + prefix + "')";
                    try
                    {
                        this.ExecuteNonQuery(addPrefix);
                    }
                    catch (SqlException sqlEx)
                    {
                        this.Close(true, true);
                        throw new RdfStorageException("Unable to create a Namespace Prefix entry in the NS_PREFIXES Table", sqlEx);
                    }

                    //Get the new ID
                    id = this.ExecuteScalar(getPrefix);
                    if (id == null)
                    {
                        this.Close(true, true);
                        throw new RdfStorageException("Unable to retrieve a required Namespace Prefix Record from the Database");
                    }
                    else
                    {
                        preID = id.ToString();
                        try
                        {
                            Monitor.Enter(this._nsPrefixIDs);
                            if (!this._nsPrefixIDs.ContainsKey(prefix))
                            {
                                this._nsPrefixIDs.Add(prefix, preID);
                            }
                        }
                        finally
                        {
                            Monitor.Exit(this._nsPrefixIDs);
                        }
                    }
                }
                else
                {
                    preID = id.ToString();
                    try
                    {
                        Monitor.Enter(this._nsPrefixIDs);
                        if (!this._nsPrefixIDs.ContainsKey(prefix))
                        {
                            this._nsPrefixIDs.Add(prefix, preID);
                        }
                    }
                    finally
                    {
                        Monitor.Exit(this._nsPrefixIDs);
                    }
                }
            }
            return preID;
        }

        /// <summary>
        /// Looks up the ID for a Namespace Uri from the Database adding it to the Database if necessary
        /// </summary>
        /// <param name="u">Namespace Uri</param>
        /// <returns></returns>
        private String GetNSUriID(Uri u)
        {
            String uriID;
            Object id;

            //Is it in the Dictionary already?
            if (this._nsUriIDs.ContainsKey(u.GetEnhancedHashCode()))
            {
                try
                {
                    Monitor.Enter(this._nsUriIDs);
                    uriID = this._nsUriIDs[u.GetEnhancedHashCode()];
                }
                finally
                {
                    Monitor.Exit(this._nsUriIDs);
                }
            }
            else
            {
                //Lookup from Database
                String getUri = "SELECT nsUriID FROM NS_URIS WHERE nsUriHash=" + u.GetEnhancedHashCode();
                id = this.ExecuteScalar(getUri);
                if (id == null)
                {
                    //Needs adding to Database
                    String addUri = "INSERT INTO NS_URIS (nsUri, nsUriHash) VALUES (N'" + this.EscapeString(u.ToString()) + "', " + u.GetEnhancedHashCode() + ")";
                    try
                    {
                        this.ExecuteNonQuery(addUri);
                    }
                    catch (SqlException sqlEx)
                    {
                        this.Close(true, true);
                        throw new RdfException("Unable to create a Namespace URI entry in the NS_URIS Table", sqlEx);
                    }

                    //Get the new ID
                    id = this.ExecuteScalar(getUri);
                    if (id == null)
                    {
                        this.Close(true, true);
                        throw new RdfException("Unable to retrieve a required Namespace URI entry from the Database");
                    }
                    else
                    {
                        uriID = id.ToString();
                        try
                        {
                            Monitor.Enter(this._nsUriIDs);
                            if (!this._nsUriIDs.ContainsKey(u.GetEnhancedHashCode()))
                            {
                                this._nsUriIDs.Add(u.GetEnhancedHashCode(), uriID);
                            }
                        }
                        finally
                        {
                            Monitor.Exit(this._nsUriIDs);
                        }
                    }
                }
                else
                {
                    uriID = id.ToString();
                    try
                    {
                        Monitor.Enter(this._nsUriIDs);
                        if (!this._nsUriIDs.ContainsKey(u.GetEnhancedHashCode()))
                        {
                            this._nsUriIDs.Add(u.GetEnhancedHashCode(), uriID);
                        }
                    }
                    finally
                    {
                        Monitor.Exit(this._nsUriIDs);
                    }
                }
            }

            return uriID;
        }

        #endregion

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
                    this._dbConnections.Add(thread, new SqlConnection());
                    if (this._dbuser != null && this._dbpwd != null)
                    {
                        this._dbConnections[thread].ConnectionString = "Data Source=" + this._dbserver + ";Initial Catalog=" + this._dbname + ";User ID=" + this._dbuser + ";Password=" + this._dbpwd + ";MultipleActiveResultSets=True;";
                    }
                    else
                    {
                        //Patch by Michael Friis to use Integrated Authentication
                        this._dbConnections[thread].ConnectionString = "Data Source=" + this._dbserver + ";Initial Catalog=" + this._dbname + ";Trusted_Connection=True;MultipleActiveResultSets=True;";
                    }

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
        /// <remarks>A Connection and Transaction per Thread are used</remarks>
        public override void Close(bool forceClose)
        {
            this.Close(forceClose, false);
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
            SqlCommand cmd = new SqlCommand(sqlCmd, this._dbConnections[thread]);
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
            SqlCommand cmd = new SqlCommand(sqlCmd, this._dbConnections[thread]);
            if (this._dbTrans[thread] != null)
            {
                //Add to the Transaction if required
                cmd.Transaction = this._dbTrans[thread];
            }

            //Execute the Query
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
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
            SqlCommand cmd = new SqlCommand(sqlCmd, this._dbConnections[thread]);
            if (this._dbTrans[thread] != null)
            {
                //Add to the Transaction if required
                cmd.Transaction = this._dbTrans[thread];
            }

            //Execute the Query
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            adapter.Fill(data);
        }

        /// <summary>
        /// Executes a Query SQL Command against the database and gets a streaming reader of the results
        /// </summary>
        /// <param name="sqlCmd">SQL Command</param>
        /// <returns></returns>
        public override System.Data.Common.DbDataReader ExecuteStreamingQuery(string sqlCmd)
        {
            //Get Thread ID
            int thread = Thread.CurrentThread.ManagedThreadId;

            //Create the SQL Command
            SqlCommand cmd = new SqlCommand(sqlCmd, this._dbConnections[thread]);
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
            SqlCommand cmd = new SqlCommand(sqlCmd, this._dbConnections[thread]);
            if (this._dbTrans[thread] != null)
            {
                //Add to the Transaction if required
                cmd.Transaction = this._dbTrans[thread];
            }

            //Execute the Scalar
            return cmd.ExecuteScalar();
        }

        /// <summary>
        /// Escapes Strings in a manner appropriate to the underlying Database
        /// </summary>
        /// <param name="text">String to escape</param>
        /// <returns>Escaped String</returns>
        public override string EscapeString(string text)
        {
            //Escape single quotes and normalize
            return text.Replace("'", "''").Normalize();
        }

        #endregion

        #region ID Assigment Map Loading

        /// <summary>
        /// Loads the Node Hash to ID Map from the Database and determines the next available Node ID
        /// </summary>
        protected override void LoadNodeIDMap()
        {
            //Assumes Database Connection is Open
            try
            {
                Monitor.Enter(this._nodeIDs);

                this._nodeIDs.Clear();

                //Type a DataTable and retrieve the data
                String getNodeIDMap = "SELECT nodeID, nodeHash FROM NODES";
                DataTable data = new DataTable();
                data.Columns.Add("nodeID", typeof(int));
                data.Columns.Add("nodeHash", typeof(int));
                this.ExecuteQuery(getNodeIDMap, data);

                //Populate the Map
                int id, hash;
                foreach (DataRow r in data.Rows)
                {
                    id = (int)r["nodeID"];
                    hash = (int)r["nodeHash"];

                    if (!this._nodeIDs.ContainsKey(hash))
                    {
                        this._nodeIDs.Add(hash, id);
                    }
                    else
                    {
                        this._nodeIDs[hash] = -1;
                    }
                }

                //Set Next available Node ID
                Object maxid = this.ExecuteScalar("SELECT MAX(nodeID) FROM NODES");
                if (maxid != null && !maxid.ToString().Equals(String.Empty))
                {
                    this._nextNodeID = Int32.Parse(maxid.ToString());
                }
                else
                {
                    this._nextNodeID = 1;
                }
            }
            catch (SqlException sqlEx)
            {
                this.Close(true, true);
                throw new RdfStorageException("Error loading Node Hash Code to ID Map from Database", sqlEx);
            }
            finally
            {
                Monitor.Exit(this._nodeIDs);
            }
        }

        /// <summary>
        /// Loads the Triple Hash to ID Map from the Database and determines the next available Triple ID
        /// </summary>
        protected override void LoadTripleIDMap()
        {
            //Assumes Database Connection is Open
            try
            {
                Monitor.Enter(this._tripleIDs);

                this._tripleIDs.Clear();

                //Type a DataTable and retrieve the data
                String getTripleIDMap = "SELECT tripleID, tripleHash FROM TRIPLES";
                DataTable data = new DataTable();
                data.Columns.Add("tripleID", typeof(int));
                data.Columns.Add("tripleHash", typeof(int));
                this.ExecuteQuery(getTripleIDMap, data);

                //Populate the Map
                int id, hash;
                foreach (DataRow r in data.Rows)
                {
                    id = (int)r["tripleID"];
                    hash = (int)r["tripleHash"];

                    if (!this._tripleIDs.ContainsKey(hash))
                    {
                        this._tripleIDs.Add(hash, id);
                    }
                    else
                    {
                        this._tripleIDs[hash] = -1;
                    }
                }

                //Set Next available Node ID
                Object maxid = this.ExecuteScalar("SELECT MAX(tripleID) FROM TRIPLES");
                if (maxid != null && !maxid.ToString().Equals(String.Empty))
                {
                    this._nextTripleID = Int32.Parse(maxid.ToString());
                }
                else
                {
                    this._nextTripleID = 1;
                }
            }
            catch (SqlException sqlEx)
            {
                this.Close(true, true);
                throw new RdfStorageException("Error loading Triple Hash Code to ID Map from Database", sqlEx);
            }
            finally
            {
                Monitor.Exit(this._tripleIDs);
            }
        }

        /// <summary>
        /// Loads the Graph ID to URI Map
        /// </summary>
        protected override void LoadGraphUriMap()
        {
            if (this._graphUris != null) return;
            this._graphUris = new Dictionary<string,Uri>();

            try 
            {
                this.Open(false);
                String getGraphUris = "SELECT * FROM GRAPHS";
                DataTable data = this.ExecuteQuery(getGraphUris);
                lock (this._graphUris)
                {
                    foreach (DataRow row in data.Rows)
                    {
                        if (!this._graphUris.ContainsKey(row["graphID"].ToString()))
                        {
                            Uri graphUri = new Uri(row["graphUri"].ToString());
                            if (graphUri.ToString().Equals(GraphCollection.DefaultGraphUri))
                            {
                                graphUri = null;
                            }
                            this._graphUris.Add(row["graphID"].ToString(), graphUri);
                        }
                    }
                }
                this.Close(false);
            } 
            catch 
            {
                this.Close(true,true);
            }
        }

        #endregion

        /// <summary>
        /// Disposes of a Manager, Manager state will be preseved if <see cref="MicrosoftSqlStoreManager.PreserveState">PreserveState</see> was previously set to true
        /// </summary>
        public override void Dispose()
        {
            this.Close(true, true);
            base.Dispose();
        }
	}
}

#endif