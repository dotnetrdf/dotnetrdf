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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using OpenLink.Data.Virtuoso;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Update;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// A Manager for accessing the Native Virtuoso Quad Store
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class implements <see cref="IGenericIOManager">IGenericIOManager</see> allowing it to be used with any of the general classes that support this interface as well as the Virtuoso specific classes.
    /// </para>
    /// <para>
    /// Although this class takes a Database Name to ensure compatability with any Virtuoso installation (i.e. this allows for the Native Quad Store to be in a non-standard database) generally you should always specify <strong>DB</strong> as the Database Name parameter
    /// </para>
    /// <para>
    /// Virtuoso automatically assigns IDs to Blank Nodes input into it, these IDs are <strong>not</strong> based on the actual Blank Node ID so inputting a Blank Node with the same ID multiple times will result in multiple Nodes being created in Virtuoso.  This means that data containing Blank Nodes which is stored to Virtuoso and then retrieved will have different Blank Node IDs to those input.  In addition there is no guarentee that when you save a Graph containing Blank Nodes into Virtuoso that retrieving it will give the same Blank Node IDs even if the Graph being saved was originally retrieved from Virtuoso.
    /// </para>
    /// <para>
    /// You can use a null Uri or an empty String as a Uri to indicate that operations should affect the Default Graph.  Where the argument is only a Graph a null <see cref="IGraph.BaseUri">BaseUri</see> property indicates that the Graph affects the Default Graph
    /// </para>
    /// </remarks>
    public class VirtuosoManager
        : IUpdateableGenericIOManager, IConfigurationSerializable, IDisposable
    {
        /// <summary>
        /// Default Port for Virtuoso Servers
        /// </summary>
        public const int DefaultPort = 1111;
        /// <summary>
        /// Default Database for Virtuoso Server Quad Store
        /// </summary>
        public const String DefaultDB = "DB";

        #region Variables & Constructors

        private VirtuosoConnection _db;
        private VirtuosoTransaction _dbtrans;
        private SparqlFormatter _formatter = new SparqlFormatter();

        /// <summary>
        /// Variables for Database Connection Properties
        /// </summary>
        protected String _dbserver, _dbname, _dbuser, _dbpwd;
        /// <summary>
        /// Database Port
        /// </summary>
        protected int _dbport;

        /// <summary>
        /// Indicates whether the Database Connection is currently being kept open
        /// </summary>
        private bool _keepOpen = false;
        private bool _customConnString = false;

        /// <summary>
        /// Creates a Manager for a Virtuoso Native Quad Store
        /// </summary>
        /// <param name="server">Server</param>
        /// <param name="port">Port</param>
        /// <param name="db">Database Name</param>
        /// <param name="user">Username</param>
        /// <param name="password">Password</param>
        public VirtuosoManager(String server, int port, String db, String user, String password)
        {
            //Set the Connection Properties
            this._dbserver = server;
            this._dbname = db;
            this._dbuser = user;
            this._dbpwd = password;
            this._dbport = port;

            //Create the Connection Object
            this._db = new VirtuosoConnection("Server=" + this._dbserver + ":" + this._dbport + ";Database=" + this._dbname + ";uid=" + this._dbuser + ";pwd=" + this._dbpwd + ";Charset=utf-8");
        }

        /// <summary>
        /// Creates a Manager for a Virtuoso Native Quad Store
        /// </summary>
        /// <param name="db">Database Name</param>
        /// <param name="user">Username</param>
        /// <param name="password">Password</param>
        /// <remarks>
        /// Assumes the Server is on the localhost and the port is the default installation port of 1111
        /// </remarks>
        public VirtuosoManager(String db, String user, String password) 
            : this("localhost", VirtuosoManager.DefaultPort, db, user, password) { }

        /// <summary>
        /// Creates a Manager for a Virtuoso Native Quad Store
        /// </summary>
        /// <param name="connectionString">Connection String</param>
        /// <remarks>
        /// Allows the end user to specify a customised connection string
        /// </remarks>
        public VirtuosoManager(String connectionString)
        {
            this._db = new VirtuosoConnection(connectionString);
            this._customConnString = true;
        }

        #endregion

        #region Triple Loading & Saving

        /// <summary>
        /// Loads a Graph from the Quad Store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to Load</param>
        public void LoadGraph(IGraph g, Uri graphUri)
        {
            if (g.IsEmpty && graphUri != null)
            {
                g.BaseUri = graphUri;
            }
            this.LoadGraph(new GraphHandler(g), graphUri);
        }

        /// <summary>
        /// Loads a Graph from the Quad Store
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">URI of the Graph to Load</param>
        public void LoadGraph(IRdfHandler handler, Uri graphUri)
        {
            if (graphUri == null) throw new RdfStorageException("Cannot load an unnamed Graph from Virtuoso as this would require loading the entirety of the Virtuoso Quad Store into memory!");

            try
            {
                handler.StartRdf();

                //Need to keep Database Open as Literals require extra trips to the Database to get additional
                //information about Language and Type
                this.Open(false);

                DataTable data = this.LoadTriples(graphUri);

                foreach (DataRow row in data.Rows)
                {
                    Object s, p, o;
                    INode subj, pred, obj;

                    //Get Data
                    s = row["S"];
                    p = row["P"];
                    o = row["O"];

                    //Create Nodes
                    subj = this.LoadNode(handler, s);
                    pred = this.LoadNode(handler, p);
                    obj = this.LoadNode(handler, o);

                    //Assert Triple
                    if (!handler.HandleTriple(new Triple(subj, pred, obj))) ParserHelper.Stop();
                }
                handler.EndRdf(true);
                this.Close(false);
            }
            catch (RdfParsingTerminatedException)
            {
                handler.EndRdf(true);
                this.Close(false);
            }
            catch
            {
                handler.EndRdf(false);
                this.Close(true);
                throw;
            }
        }

        /// <summary>
        /// Loads a Graph from the Quad Store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to Load</param>
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
        /// Loads a Graph from the Quad Store
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">URI of the Graph to Load</param>
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
        /// Gets a Table of Triples that are in the given Graph
        /// </summary>
        /// <param name="graphUri">Graph Uri</param>
        /// <returns></returns>
        /// <remarks>
        /// Assumes that the caller has opened the Database connection
        /// </remarks>
        private DataTable LoadTriples(Uri graphUri)
        {
            DataTable dt = new DataTable();
            String getTriples;
            if (graphUri != null)
            {
                getTriples = "SPARQL define output:format '_JAVA_' SELECT * FROM <" + graphUri.ToString() + "> WHERE {?s ?p ?o}";
            }
            else
            {
                getTriples = "SPARQL define output:format '_JAVA_' SELECT * WHERE {?s ?p ?o}";
            }

            VirtuosoCommand cmd = this._db.CreateCommand();
            cmd.CommandText = getTriples;

            VirtuosoDataAdapter adapter = new VirtuosoDataAdapter(cmd);

            dt.Columns.Add("S", typeof(System.Object));
            dt.Columns.Add("P", typeof(System.Object));
            dt.Columns.Add("O", typeof(System.Object));
            try
            {
                adapter.Fill(dt);
            }
            catch
            {
                throw;
            }

            return dt;
        }

        /// <summary>
        /// Decodes an Object into an appropriate Node
        /// </summary>
        /// <param name="factory">Node Factory to use to create Node</param>
        /// <param name="n">Object to convert</param>
        /// <returns></returns>
        private INode LoadNode(INodeFactory factory, Object n)
        {
            INode temp;
            if (n is SqlExtendedString)
            {
                SqlExtendedString iri = (SqlExtendedString)n;
                if (iri.IriType == SqlExtendedStringType.BNODE)
                {
                    //Blank Node
                    temp = factory.CreateBlankNode(n.ToString().Substring(9));

                }
                else if (iri.IriType != iri.StrType)
                {
                    //Literal
                    temp = factory.CreateLiteralNode(n.ToString());
                }
                else if (iri.IriType == SqlExtendedStringType.IRI)
                {
                    //Uri
                    Uri u = new Uri(n.ToString(), UriKind.RelativeOrAbsolute);
                    if (!u.IsAbsoluteUri)
                    {
                        throw new RdfParseException("Virtuoso returned a URI Node which has a relative URI, unable to resolve the URI for this node");
                    }
                    temp = factory.CreateUriNode(u);
                }
                else
                {
                    //Assume a Literal
                    temp = factory.CreateLiteralNode(n.ToString());
                }
            }
            else if (n is SqlRdfBox)
            {
                SqlRdfBox lit = (SqlRdfBox)n;
                if (lit.StrLang != null)
                {
                    //Language Specified Literal
                    temp = factory.CreateLiteralNode(n.ToString(), lit.StrLang);
                }
                else if (lit.StrType != null)
                {
                    //Data Typed Literal
                    temp = factory.CreateLiteralNode(n.ToString(), new Uri(lit.StrType));
                }
                else
                {
                    //Literal
                    temp = factory.CreateLiteralNode(n.ToString());
                }
            }
            else if (n is String)
            {
                String s = n.ToString();
                if (s.StartsWith("nodeID://"))
                {
                    //Blank Node
                    temp = factory.CreateBlankNode(s.Substring(9));
                }
                else
                {
                    //Literal
                    temp = factory.CreateLiteralNode(s);
                }
            }
            else if (n is Int32)
            {
                temp = ((Int32)n).ToLiteral(factory);
            }
            else if (n is Int16)
            {
                temp = ((Int16)n).ToLiteral(factory);
            }
            else if (n is Single)
            {
                temp = ((Single)n).ToLiteral(factory);
            }
            else if (n is Double)
            {
                temp = ((Double)n).ToLiteral(factory);
            }
            else if (n is Decimal)
            {
                temp = ((Decimal)n).ToLiteral(factory);
            }
            else if (n is DateTime)
            {
                temp = ((DateTime)n).ToLiteral(factory);
            }
            else if (n is TimeSpan)
            {
                temp = ((TimeSpan)n).ToLiteral(factory);
            }
            else if (n is Boolean)
            {
                temp = ((Boolean)n).ToLiteral(factory);
            }
            else if (n is DBNull)
            {
                //Fix by Alexander Sidarov for Virtuoso's results for unbound variables in OPTIONALs
                temp = null;
            }
            else
            {
                throw new RdfStorageException("Unexpected Object Type '" + n.GetType().ToString() + "' returned from SPASQL SELECT query to the Virtuoso Quad Store");
            }
            return temp;
        }

        /// <summary>
        /// Saves a Graph into the Quad Store (Warning: Completely replaces any existing Graph with the same URI)
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <remarks>
        /// Completely replaces any previously saved Graph with the same Graph URI
        /// </remarks>
        public void SaveGraph(IGraph g)
        {
            if (g.BaseUri == null) throw new RdfStorageException("Cannot save a Graph without a Base URI to Virtuoso");

            try
            {
                this.Open(false);

                //Delete the existing Graph (if it exists)
                this.ExecuteNonQuery("DELETE FROM DB.DBA.RDF_QUAD WHERE G = DB.DBA.RDF_MAKE_IID_OF_QNAME('" + g.BaseUri.ToString() + "')");

                //Make a call to the TTLP() Virtuoso function
                VirtuosoCommand cmd = new VirtuosoCommand();
                cmd.CommandText = "DB.DBA.TTLP(@data, @base, @graph, 1)";
                 cmd.Parameters.Add("data", VirtDbType.VarChar);
                cmd.Parameters["data"].Value = VDS.RDF.Writing.StringWriter.Write(g, new NTriplesWriter());
                String baseUri = g.BaseUri.ToSafeString();
                cmd.Parameters.Add("base", VirtDbType.VarChar);
                cmd.Parameters.Add("graph", VirtDbType.VarChar);
                cmd.Parameters["base"].Value = baseUri;
                cmd.Parameters["graph"].Value = baseUri;
                cmd.Connection = this._db;
                int result = cmd.ExecuteNonQuery();

                this.Close(false);
            }
            catch
            {
                this.Close(true);
                throw;
            }
        }

        /// <summary>
        /// Gets the IO Behaviour of the store
        /// </summary>
        public IOBehaviour IOBehaviour
        {
            get
            {
                return IOBehaviour.IsQuadStore | IOBehaviour.HasNamedGraphs | IOBehaviour.OverwriteNamed | IOBehaviour.CanUpdateTriples;
            }
        }

        /// <summary>
        /// Updates a Graph in the Quad Store
        /// </summary>
        /// <param name="graphUri">Graph Uri of the Graph to update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        public void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            try
            {
                this.Open(true);
                int r;

                //Build the Delete Data Command
                if (removals != null)
                {
                    if (removals.Any())
                    {
                        VirtuosoCommand deleteCmd = new VirtuosoCommand();
                        StringBuilder delete = new StringBuilder();
                        delete.AppendLine("SPARQL define output:format '_JAVA_' DELETE DATA");
                        if (graphUri != null)
                        {
                            delete.AppendLine(" FROM <" + graphUri.ToString() + ">");
                        }
                        else
                        {
                            throw new RdfStorageException("Cannot update an unnamed Graph in a Virtuoso Store using this method - you must specify the URI of a Graph to Update");
                        }
                        delete.AppendLine("{");
                        foreach (Triple t in removals)
                        {
                            delete.AppendLine(t.ToString(this._formatter));
                        }
                        delete.AppendLine("}");
                        deleteCmd.CommandText = delete.ToString();
                        deleteCmd.Connection = this._db;
                        deleteCmd.Transaction = this._dbtrans;

                        r = deleteCmd.ExecuteNonQuery();
                        if (r < 0) throw new RdfStorageException("Virtuoso encountered an error when deleting Triples");
                    }
                }

                //Build the Insert Data Command
                if (additions != null)
                {
                    if (additions.Any())
                    {
                        VirtuosoCommand insertCmd = new VirtuosoCommand();
                        StringBuilder insert = new StringBuilder();
                        insert.AppendLine("SPARQL define output:format '_JAVA_' INSERT DATA");
                        if (graphUri != null)
                        {
                            insert.AppendLine(" INTO <" + graphUri.ToString() + ">");
                        }
                        else
                        {
                            throw new RdfStorageException("Cannot update an unnamed Graph in a Virtuoso Store using this method - you must specify the URI of a Graph to Update");
                        }
                        insert.AppendLine("{");
                        foreach (Triple t in additions)
                        {
                            insert.AppendLine(t.ToString(this._formatter));
                        }
                        insert.AppendLine("}");
                        insertCmd.CommandText = insert.ToString();
                        insertCmd.Connection = this._db;
                        insertCmd.Transaction = this._dbtrans;

                        r = insertCmd.ExecuteNonQuery();
                        if (r < 0) throw new RdfStorageException("Virtuoso encountered an error when inserting Triples");
                    }
                }

                this.Close(true, false);
            }
            catch
            {
                this.Close(true, true);
                throw;
            }
        }

        /// <summary>
        /// Updates a Graph in the Quad Store
        /// </summary>
        /// <param name="graphUri">Graph Uri of the Graph to update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        public void UpdateGraph(String graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            Uri u = (graphUri.Equals(String.Empty)) ? null : new Uri(graphUri);
            this.UpdateGraph(u, additions, removals);
        }

        /// <summary>
        /// Indicates that Updates are supported by the Virtuoso Native Quad Store
        /// </summary>
        public bool UpdateSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns that the Manager is ready
        /// </summary>
        public bool IsReady
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns that the Manager is not read-only
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region Native Query & Update

        /// <summary>
        /// Executes a SPARQL Query on the native Quad Store
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query to execute</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// This method will first attempt to parse the query into a <see cref="SparqlQuery">SparqlQuery</see> object.  If this succeeds then the Query Type can be used to determine how to handle the response.
        /// </para>
        /// <para>
        /// If the parsing fails then the query will be executed anyway using Virtuoso's SPASQL (SPARQL + SQL) syntax.  Parsing can fail because Virtuoso supports various SPARQL extensions which the library does not support.  These include things like aggregate functions but also SPARUL updates (the non-standard precusor to SPARQL 1.1 Update).
        /// </para>
        /// <para>
        /// If you use an aggregate query which has an Integer, Decimal or Double type result then you will receive a <see cref="SparqlResultSet">SparqlResultSet</see> containing a single <see cref="SparqlResult">SparqlResult</see> which has contains a binding for a variable named <strong>Result</strong> which contains a <see cref="LiteralNode">LiteralNode</see> typed to the appropriate datatype.
        /// </para>
        /// </remarks>
        /// <exception cref="RdfQueryException">Thrown if an error occurs in making the query</exception>
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
        /// Executes a SPARQL Query on the native Quad Store processing the results with an appropriate handler from those provided
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="sparqlQuery">SPARQL Query to execute</param>
        /// <remarks>
        /// <para>
        /// This method will first attempt to parse the query into a <see cref="SparqlQuery">SparqlQuery</see> object.  If this succeeds then the Query Type can be used to determine how to handle the response.
        /// </para>
        /// <para>
        /// If the parsing fails then the query will be executed anyway using Virtuoso's SPASQL (SPARQL + SQL) syntax.  Parsing can fail because Virtuoso supports various SPARQL non-standardised extensions which the library does not support.  These include things like aggregate functions but also SPARUL updates (the non-standard precusor to SPARQL 1.1 Update).
        /// </para>
        /// <para>
        /// If you use an aggregate query which has an Integer, Decimal or Double type result then you will receive a <see cref="SparqlResultSet">SparqlResultSet</see> containing a single <see cref="SparqlResult">SparqlResult</see> which has contains a binding for a variable named <strong>Result</strong> which contains a <see cref="LiteralNode">LiteralNode</see> typed to the appropriate datatype.
        /// </para>
        /// </remarks>
        /// <exception cref="RdfQueryException">Thrown if an error occurs in making the query</exception>
        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String sparqlQuery)
        {
            try
            {
                if (resultsHandler != null) resultsHandler.StartResults();

                DataTable results = new DataTable();
                results.Columns.CollectionChanged += new System.ComponentModel.CollectionChangeEventHandler(Columns_CollectionChanged);

                //See if the query can be parsed into a SparqlQuery object
                //It might not since the user might use Virtuoso's extensions to Sparql in their query
                try
                {
                    //We'll set the Parser to SPARQL 1.1 mode even though Virtuoso's SPARQL implementation has
                    //various perculiarties in their SPARQL 1.1 implementation and we'll try and 
                    //handle the potential results in the catch branch if a valid SPARQL 1.0 query
                    //cannot be parsed
                    //Change made in response to a bug report by Aleksandr A. Zaripov [zaripov@tpu.ru]
                    SparqlQueryParser parser = new SparqlQueryParser();
                    parser.SyntaxMode = SparqlQuerySyntax.Sparql_1_1;
                    SparqlQuery query = parser.ParseFromString(sparqlQuery);

                    switch (query.QueryType)
                    {
                        case SparqlQueryType.Select:
                        case SparqlQueryType.SelectAll:
                        case SparqlQueryType.SelectAllDistinct:
                        case SparqlQueryType.SelectAllReduced:
                        case SparqlQueryType.SelectDistinct:
                        case SparqlQueryType.SelectReduced:
                            //Type the Tables columns as System.Object
                            foreach (SparqlVariable var in query.Variables)
                            {
                                if (var.IsResultVariable)
                                {
                                    results.Columns.Add(var.Name, typeof(System.Object));
                                }
                            }
                            break;
                    }

                    try
                    {
                        this.Open(false);

                        //Make the Query against Virtuoso
                        VirtuosoCommand cmd = this._db.CreateCommand();
                        cmd.CommandText = "SPARQL " + sparqlQuery;
                        VirtuosoDataAdapter adapter = new VirtuosoDataAdapter(cmd);
                        adapter.Fill(results);

                        //Decide how to process the results based on the return type
                        switch (query.QueryType)
                        {
                            case SparqlQueryType.Ask:
                                //Expect a DataTable containing a single row and column which contains a boolean

                                //Ensure Results Handler is not null
                                if (resultsHandler == null) throw new ArgumentNullException("Cannot handle a Boolean Result with a null SPARQL Results Handler");

                                if (results.Rows.Count == 1 && results.Columns.Count == 1)
                                {
                                    //Try and parse the result
                                    bool result;
                                    int r;
                                    if (Boolean.TryParse(results.Rows[0][0].ToString(), out result))
                                    {
                                        resultsHandler.HandleBooleanResult(result);
                                    }
                                    else if (Int32.TryParse(results.Rows[0][0].ToString(), out r))
                                    {
                                        if (r == 1)
                                        {
                                            resultsHandler.HandleBooleanResult(true);
                                        }
                                        else
                                        {
                                            resultsHandler.HandleBooleanResult(false);
                                        }
                                    }
                                    else
                                    {
                                        throw new RdfQueryException("Expected a Boolean as the result of an ASK query but the non-boolean value '" + results.Rows[0][0].ToString() + "' was received");
                                    }
                                }
                                else
                                {
                                    //If we get anything else then we'll return that the result was False
                                    resultsHandler.HandleBooleanResult(false);
                                }
                                break;

                            case SparqlQueryType.Construct:
                            case SparqlQueryType.Describe:
                            case SparqlQueryType.DescribeAll:
                                //Expect a DataTable containing a single row and column which contains a String
                                //That string will be a Turtle serialization of the Graph

                                //Ensure that RDF Handler is not null
                                if (rdfHandler == null) throw new ArgumentNullException("Cannot handle a Graph result with a null RDF Handler");

                                if (results.Rows.Count == 1 && results.Columns.Count == 1)
                                {
                                    try
                                    {
                                        //Use StringParser to parse
                                        String data = results.Rows[0][0].ToString();
                                        TurtleParser ttlparser = new TurtleParser();
                                        ttlparser.Load(rdfHandler, new StringReader(data));
                                    }
                                    catch (RdfParseException parseEx)
                                    {
                                        throw new RdfQueryException("Expected a valid Turtle serialization of the Graph resulting from a CONSTRUCT/DESCRIBE query but the result failed to parse", parseEx);
                                    }
                                }
                                else
                                {
                                    throw new RdfQueryException("Expected a single string value representing the serialization of the Graph resulting from a CONSTRUCT/DESCRIBE query but this was not received");
                                }
                                break;

                            case SparqlQueryType.Select:
                            case SparqlQueryType.SelectAll:
                            case SparqlQueryType.SelectAllDistinct:
                            case SparqlQueryType.SelectAllReduced:
                            case SparqlQueryType.SelectDistinct:
                            case SparqlQueryType.SelectReduced:
                                //Ensure Results Handler is not null
                                if (resultsHandler == null) throw new ArgumentNullException("Cannot handle SPARQL Results with a null Results Handler");

                                //Get Result Variables
                                List<SparqlVariable> resultVars = query.Variables.Where(v => v.IsResultVariable).ToList();
                                foreach (SparqlVariable var in resultVars)
                                {
                                    if (!resultsHandler.HandleVariable(var.Name)) ParserHelper.Stop();
                                }
                                Graph temp = new Graph();

                                //Convert each solution into a SPARQLResult
                                foreach (DataRow r in results.Rows)
                                {
                                    Set s = new Set();
                                    foreach (SparqlVariable var in resultVars)
                                    {
                                        if (r[var.Name] != null)
                                        {
                                            s.Add(var.Name, this.LoadNode(temp, r[var.Name]));
                                        }
                                    }
                                    if (!resultsHandler.HandleResult(new SparqlResult(s))) ParserHelper.Stop();
                                }
                                break;

                            default:
                                throw new RdfQueryException("Unable to process the Results of an Unknown query type");
                        }

                        this.Close(false);
                    }
                    catch
                    {
                        this.Close(true, true);
                        throw;
                    }
                }
                catch (RdfParseException)
                {
                    //Unable to parse a SPARQL 1.0 query
                    //Have to attempt to detect the return type based on the DataTable that
                    //the SPASQL (Sparql+SQL) query gives back

                    //Make the Query against Virtuoso
                    VirtuosoCommand cmd = this._db.CreateCommand();
                    cmd.CommandText = "SPARQL " /*define output:format '_JAVA_' "*/ + sparqlQuery;
                    VirtuosoDataAdapter adapter = new VirtuosoDataAdapter(cmd);
                    adapter.Fill(results);

                    //Try to detect the return type based on the DataTable configuration
                    if (results.Rows.Count == 0 && results.Columns.Count > 0)
                    {
                        if (resultsHandler == null) throw new ArgumentNullException("Cannot handler SPARQL Results with a null Results Handler");

                        //No Rows but some columns implies empty SELECT results
                        SparqlResultSet rset = new SparqlResultSet();
                        foreach (DataColumn col in results.Columns)
                        {
                            if (!resultsHandler.HandleVariable(col.ColumnName)) ParserHelper.Stop();
                        }
                    }
                    else if (results.Rows.Count == 1 && results.Columns.Count == 1 && !Regex.IsMatch(sparqlQuery, "SELECT", RegexOptions.IgnoreCase))
                    {
                        //Added a fix here suggested by Alexander Sidorov - not entirely happy with this fix as what happens if SELECT just happens to occur in a URI/Variable Name?

                        //Single Row and Column implies ASK/DESCRIBE/CONSTRUCT results
                        bool result;
                        int r;
                        decimal rdec;
                        double rdbl;
                        float rflt;

                        if (results.Rows[0][0].ToString().Equals(String.Empty))
                        {
                            //Empty Results - no need to do anything
                        }
                        else if (Boolean.TryParse(results.Rows[0][0].ToString(), out result))
                        {
                            //Parseable Boolean so ASK Results
                            if (resultsHandler == null) throw new ArgumentNullException("Cannot handle a Boolean result with a null Results Handler");
                            resultsHandler.HandleBooleanResult(result);
                        }
                        else if (Int32.TryParse(results.Rows[0][0].ToString(), out r))
                        {
                            if (resultsHandler == null) throw new ArgumentNullException("Cannot handle SPARQL results with a null Results Handler");

                            //Parseable Integer so Aggregate SELECT Query Results
                            if (!resultsHandler.HandleVariable("Result")) ParserHelper.Stop();
                            Set s = new Set();
                            s.Add("Result", resultsHandler.CreateLiteralNode(r.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger)));
                            if (!resultsHandler.HandleResult(new SparqlResult(s))) ParserHelper.Stop();
                        }
                        else if (Single.TryParse(results.Rows[0][0].ToString(), out rflt))
                        {
                            if (resultsHandler == null) throw new ArgumentNullException("Cannot handle SPARQL results with a null Results Handler");

                            //Parseable Single so Aggregate SELECT Query Results
                            if (!resultsHandler.HandleVariable("Result")) ParserHelper.Stop();
                            Set s = new Set();
                            s.Add("Result", resultsHandler.CreateLiteralNode(rflt.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeFloat)));
                            if (!resultsHandler.HandleResult(new SparqlResult(s))) ParserHelper.Stop();
                        }
                        else if (Double.TryParse(results.Rows[0][0].ToString(), out rdbl))
                        {
                            if (resultsHandler == null) throw new ArgumentNullException("Cannot handle SPARQL results with a null Results Handler");

                            //Parseable Double so Aggregate SELECT Query Results
                            if (!resultsHandler.HandleVariable("Result")) ParserHelper.Stop();
                            Set s = new Set();
                            s.Add("Result", resultsHandler.CreateLiteralNode(rdbl.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDouble)));
                            if (!resultsHandler.HandleResult(new SparqlResult(s))) ParserHelper.Stop();
                        }
                        else if (Decimal.TryParse(results.Rows[0][0].ToString(), out rdec))
                        {
                            //Parseable Decimal so Aggregate SELECT Query Results
                            if (!resultsHandler.HandleVariable("Result")) ParserHelper.Stop();
                            Set s = new Set();
                            s.Add("Result", resultsHandler.CreateLiteralNode(rdec.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeDecimal)));
                            if (!resultsHandler.HandleResult(new SparqlResult(s))) ParserHelper.Stop();
                        }
                        else
                        {
                            //String so try and parse as Turtle
                            try
                            {
                                //Use StringParser to parse
                                String data = results.Rows[0][0].ToString();
                                TurtleParser ttlparser = new TurtleParser();
                                ttlparser.Load(rdfHandler, new StringReader(data));
                            }
                            catch (RdfParseException)
                            {
                                if (resultsHandler == null) throw new ArgumentNullException("Cannot handle SPARQL results with a null Results Handler");

                                //If it failed to parse then it might be the result of one of the aggregate
                                //functions that Virtuoso extends Sparql with
                                if (!resultsHandler.HandleVariable(results.Columns[0].ColumnName)) ParserHelper.Stop();
                                Set s = new Set();
                                s.Add(results.Columns[0].ColumnName, this.LoadNode(resultsHandler, results.Rows[0][0]));
                                //Nothing was returned here previously - fix submitted by Aleksandr A. Zaripov [zaripov@tpu.ru]
                                if (!resultsHandler.HandleResult(new SparqlResult(s))) ParserHelper.Stop();
                            }
                        }
                    }
                    else
                    {
                        //Any other number of rows/columns we have to assume that it's normal SELECT results
                        //Changed in response to bug report by Aleksandr A. Zaripov [zaripov@tpu.ru]

                        if (resultsHandler == null) throw new ArgumentNullException("Cannot handle SPARQL results with a null Results Handler");

                        //Get Result Variables
                        List<String> vars = new List<string>();
                        foreach (DataColumn col in results.Columns)
                        {
                            vars.Add(col.ColumnName);
                            if (!resultsHandler.HandleVariable(col.ColumnName)) ParserHelper.Stop();
                        }

                        //Convert each solution into a SPARQLResult
                        foreach (DataRow r in results.Rows)
                        {
                            Set s = new Set();
                            foreach (String var in vars)
                            {
                                if (r[var] != null)
                                {
                                    s.Add(var, this.LoadNode(resultsHandler, r[var]));
                                }
                            }
                            if (!resultsHandler.HandleResult(new SparqlResult(s))) ParserHelper.Stop();
                        }
                    }
                }

                if (resultsHandler != null) resultsHandler.EndResults(true);
            }
            catch (RdfParsingTerminatedException)
            {
                if (resultsHandler != null) resultsHandler.EndResults(true);
            }
            catch
            {
                if (resultsHandler != null) resultsHandler.EndResults(false);
                throw;
            }
        }

        void Columns_CollectionChanged(object sender, System.ComponentModel.CollectionChangeEventArgs e)
        {
            Type reqType = typeof(Object);
            if (e.Action != System.ComponentModel.CollectionChangeAction.Add) return;
            DataColumn column = (DataColumn)e.Element;
            if (!column.DataType.Equals(reqType))
            {
                column.DataType = reqType;
            }
        }

        /// <summary>
        /// Executes a SPARQL Update on the native Quad Store
        /// </summary>
        /// <param name="sparqlUpdate">SPARQL Update to execute</param>
        /// <remarks>
        /// <para>
        /// This method will first attempt to parse the update into a <see cref="SparqlUpdateCommandSet">SparqlUpdateCommandSet</see> object.  If this succeeds then each command in the command set will be issued to Virtuoso.
        /// </para>
        /// <para>
        /// If the parsing fails then the update will be executed anyway using Virtuoso's SPASQL (SPARQL + SQL) syntax.  Parsing can fail because Virtuoso supports various SPARQL extensions which the library does not support and primarily supports SPARUL updates (the precusor to SPARQL 1.1 Update).
        /// </para>
        /// </remarks>
        /// <exception cref="SparqlUpdateException">Thrown if an error occurs in making the update</exception>
        public void Update(String sparqlUpdate)
        {
            try
            {
                this.Open(true);

                //Try and parse the SPARQL Update String
                SparqlUpdateParser parser = new SparqlUpdateParser();
                SparqlUpdateCommandSet commands = parser.ParseFromString(sparqlUpdate);

                //Process each Command individually
                foreach (SparqlUpdateCommand command in commands.Commands)
                {
                    //Make the Update against Virtuoso
                    VirtuosoCommand cmd = this._db.CreateCommand();
                    cmd.CommandText = "SPARQL " + command.ToString();
                    cmd.ExecuteNonQuery();
                }

                this.Close(true);
            }
            catch (RdfParseException)
            {
                //Ignore failed parsing and attempt to execute anyway
                VirtuosoCommand cmd = this._db.CreateCommand();
                cmd.CommandText = "SPARQL " + sparqlUpdate;
                cmd.ExecuteNonQuery();

                this.Close(true);
            }
            catch (SparqlUpdateException)
            {
                this.Close(true, true);
                throw;
            }
            catch (Exception ex)
            {
                //Wrap in a SPARQL Update Exception
                this.Close(true, true);
                throw new SparqlUpdateException("An error occurred while trying to perform the SPARQL Update with Virtuoso.  Note that Virtuoso primarily supports SPARUL (the precursor to SPARQL Update) and many valid SPARQL Update Commands are not yet supported by Virtuoso.", ex);
            }
        }

        #endregion

        /// <summary>
        /// Deletes a Graph from the Virtuoso store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        public void DeleteGraph(Uri graphUri)
        {
            this.DeleteGraph(graphUri.ToSafeString());
        }

        /// <summary>
        /// Deletes a Graph from the store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        public void DeleteGraph(String graphUri)
        {
            if (graphUri == null) return;
            if (graphUri.Equals(String.Empty)) return;

            try
            {
                this.Open(false);
                this.ExecuteNonQuery("DELETE FROM DB.DBA.RDF_QUAD WHERE G = DB.DBA.RDF_MAKE_IID_OF_QNAME('" + graphUri + "')");
            }
            catch
            {
                this.Close(true, true);
                throw;
            }
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
        /// Lists the Graphs in the store
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Uri> ListGraphs()
        {
            try
            {
                Object results = this.Query("SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } }");
                if (results is SparqlResultSet)
                {
                    List<Uri> graphs = new List<Uri>();
                    foreach (SparqlResult r in ((SparqlResultSet)results))
                    {
                        if (r.HasValue("g"))
                        {
                            INode temp = r["g"];
                            try
                            {
                                if (temp.NodeType == NodeType.Uri)
                                {
                                    graphs.Add(((IUriNode)temp).Uri);
                                }
                                else if (temp.NodeType == NodeType.Literal)
                                {
                                    //HACK: Virtuoso wrongly returns Literals instead of URIs in the results for the above query prior to Virtuoso 6.1.3
                                    graphs.Add(new Uri(((ILiteralNode)temp).Value));
                                }
                            }
                            catch
                            {
                                //HACK: Virtuoso has some special Graphs which have non-URI names so ignore these
                                continue;
                            }
                        }
                    }
                    return graphs;
                }
                else
                {
                    return Enumerable.Empty<Uri>();
                }
            }
            catch (Exception ex)
            {
                throw new RdfStorageException("Underlying Store returned an error while trying to List Graphs", ex);
            }
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

        #region Database IO

        /// <summary>
        /// Opens a Connection to the Database
        /// </summary>
        /// <param name="keepOpen">Indicates that the Connection should be kept open and a Transaction started</param>
        public void Open(bool keepOpen)
        {
            this.Open(keepOpen, IsolationLevel.ReadCommitted);
        }

        /// <summary>
        /// Opens a Connection to the Database
        /// </summary>
        /// <param name="keepOpen">Indicates that the Connection should be kept open and a Transaction started</param>
        /// <param name="level">Isolation Level to use</param>
        public void Open(bool keepOpen, IsolationLevel level)
        {
            switch (this._db.State)
            {
                case ConnectionState.Broken:
                case ConnectionState.Closed:
                    this._db.Open();

                    //Start a Transaction
                    if (this._dbtrans == null)
                    {
                        this._dbtrans = this._db.BeginTransaction(level);
                    }
                    break;
            }
            if (keepOpen) this._keepOpen = true;
        }

        /// <summary>
        /// Closes the Connection to the Database
        /// </summary>
        /// <param name="forceClose">Indicates that the connection should be closed even if keepOpen was specified when the Connection was opened</param>
        public void Close(bool forceClose)
        {
            this.Close(forceClose, false);
        }

        /// <summary>
        /// Closes the Connection to the Database
        /// </summary>
        /// <param name="forceClose">Indicates that the connection should be closed even if keepOpen was specified when the Connection was opened</param>
        /// <param name="rollbackTrans">Indicates that the Transaction should be rolled back because something has gone wrong</param>
        public void Close(bool forceClose, bool rollbackTrans)
        {
            //Don't close if we're keeping open and not forcing Close or rolling back a Transaction
            if (this._keepOpen && !forceClose && !rollbackTrans)
            {
                return;
            }

            switch (this._db.State)
            {
                case ConnectionState.Open:
                    //Finish the Transaction if exists
                    if (this._dbtrans != null)
                    {
                        if (!rollbackTrans)
                        {
                            //Commit normally
                            this._dbtrans.Commit();
                        }
                        else
                        {
                            //Want to Rollback
                            this._dbtrans.Rollback();
                        }
                        this._dbtrans = null;

                        this._db.Close();
                    }

                    this._keepOpen = false;
                    break;
            }
        }

        /// <summary>
        /// Executes a Non-Query SQL Command against the database
        /// </summary>
        /// <param name="sqlCmd">SQL Command</param>
        public void ExecuteNonQuery(string sqlCmd)
        {
            //Create the SQL Command
            VirtuosoCommand cmd = new VirtuosoCommand(sqlCmd, this._db);
            if (this._dbtrans != null)
            {
                //Add to the Transaction if required
                cmd.Transaction = this._dbtrans;
            }

            //Execute
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes a Query SQL Command against the database and returns a DataTable
        /// </summary>
        /// <param name="sqlCmd">SQL Command</param>
        /// <returns>DataTable of results</returns>
        public DataTable ExecuteQuery(string sqlCmd)
        {
            //Create the SQL Command
            VirtuosoCommand cmd = new VirtuosoCommand(sqlCmd, this._db);
            if (this._dbtrans != null)
            {
                //Add to the Transaction if required
                cmd.Transaction = this._dbtrans;
            }

            //Execute the Query
            VirtuosoDataAdapter adapter = new VirtuosoDataAdapter(cmd);
            DataTable results = new DataTable();
            adapter.Fill(results);

            return results;
        }

        /// <summary>
        /// Executes a Query SQL Command against the database and returns the scalar result (first column of first row of the result)
        /// </summary>
        /// <param name="sqlCmd">SQL Command</param>
        /// <returns>First Column of First Row of the Results</returns>
        public object ExecuteScalar(string sqlCmd)
        {
            //Create the SQL Command
            VirtuosoCommand cmd = new VirtuosoCommand(sqlCmd, this._db);
            if (this._dbtrans != null)
            {
                //Add to the Transaction if required
                cmd.Transaction = this._dbtrans;
            }

            //Execute the Scalar
            return cmd.ExecuteScalar();
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Disposes of the Manager
        /// </summary>
        public void Dispose()
        {
            this.Close(true, false);
        }

        #endregion

        /// <summary>
        /// Gets a String which gives details of the Connection
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this._customConnString)
            {
                return "[Virtuoso] Custom Connection String";
            }
            else
            {
                return "[Virtuoso] " + this._dbserver + ":" + this._dbport;
            }
        }


        /// <summary>
        /// Serializes the connection's configuration
        /// </summary>
        /// <param name="context">Configuration Serialization Context</param>
        public void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            if (this._customConnString)
            {
                throw new DotNetRdfConfigurationException("Cannot serialize the configuration of a VirtuosoManager which was created with a custom connection string");
            }

            //Firstly need to ensure our object factory has been referenced
            context.EnsureObjectFactory(typeof(VirtuosoObjectFactory));

            //Then serialize the actual configuration
            INode dnrType = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyType);
            INode rdfType = context.Graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            INode manager = context.NextSubject;
            INode rdfsLabel = context.Graph.CreateUriNode(new Uri(NamespaceMapper.RDFS + "label"));
            INode genericManager = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.ClassGenericManager);
            INode server = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyServer);
            
            context.Graph.Assert(new Triple(manager, rdfType, genericManager));
            context.Graph.Assert(new Triple(manager, rdfsLabel, context.Graph.CreateLiteralNode(this.ToString())));
            context.Graph.Assert(new Triple(manager, dnrType, context.Graph.CreateLiteralNode(this.GetType().FullName + ", dotNetRDF.Data.Virtuoso")));
            context.Graph.Assert(new Triple(manager, server, context.Graph.CreateLiteralNode(this._dbserver)));

            if (this._dbport != DefaultPort)
            {
                INode port = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyPort);
                context.Graph.Assert(new Triple(manager, port, this._dbport.ToLiteral(context.Graph)));
            }
            if (!this._dbname.Equals(DefaultDB))
            {
                INode db = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyDatabase);
                context.Graph.Assert(new Triple(manager, db, context.Graph.CreateLiteralNode(this._dbname)));
            }
            if (this._dbuser != null && this._dbpwd != null)
            {
                INode username = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyUser);
                INode pwd = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyPassword);
                context.Graph.Assert(new Triple(manager, username, context.Graph.CreateLiteralNode(this._dbuser)));
                context.Graph.Assert(new Triple(manager, pwd, context.Graph.CreateLiteralNode(this._dbpwd)));
            }
        }
    }
}

#endif