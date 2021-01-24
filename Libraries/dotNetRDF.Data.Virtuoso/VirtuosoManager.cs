/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2020 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

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
    /// A Manager for accessing the Native Virtuoso Quad Store.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class implements <see cref="IStorageProvider">IStorageProvider</see> allowing it to be used with any of the general classes that support this interface as well as the Virtuoso specific classes.
    /// </para>
    /// <para>
    /// Although this class takes a Database Name to ensure compatibility with any Virtuoso installation (i.e. this allows for the Native Quad Store to be in a non-standard database) generally you should always specify <strong>DB</strong> as the Database Name parameter.
    /// </para>
    /// <para>
    /// Virtuoso automatically assigns IDs to Blank Nodes input into it, these IDs are <strong>not</strong> based on the actual Blank Node ID so inputting a Blank Node with the same ID multiple times will result in multiple Nodes being created in Virtuoso.  This means that data containing Blank Nodes which is stored to Virtuoso and then retrieved will have different Blank Node IDs to those input.  In addition there is no guarentee that when you save a Graph containing Blank Nodes into Virtuoso that retrieving it will give the same Blank Node IDs even if the Graph being saved was originally retrieved from Virtuoso.  Finally please see the remarks on the <see cref="VirtuosoManager.UpdateGraph(Uri,IEnumerable{Triple},IEnumerable{Triple})">UpdateGraph()</see> method which deal with how insertion and deletion of triples containing blank nodes into existing graphs operates.
    /// </para>
    /// <para>
    /// You can use a null Uri or an empty String as a Uri to indicate that operations should affect the Default Graph.  Where the argument is only a Graph a null <see cref="IGraph.Name"/> property indicates that the Graph affects the Default Graph.
    /// </para>
    /// </remarks>
    public class VirtuosoManager
        : BaseAsyncSafeConnector, IUpdateableStorage, IConfigurationSerializable
    {
        /// <summary>
        /// Default Port for Virtuoso Servers.
        /// </summary>
        public const int DefaultPort = 1111;

        /// <summary>
        /// Default Database for Virtuoso Server Quad Store.
        /// </summary>
        public const string DefaultDB = "DB";

        private const string SubjectColumn = "S", PredicateColumn = "P", ObjectColumn = "O";
        private const string VirtuosoRelativeBaseString = "virtuoso-relative:";
        private readonly Uri VirtuosoRelativeBase = new Uri(VirtuosoRelativeBaseString);

        #region Variables & Constructors

        private readonly VirtuosoConnection _db;
        private VirtuosoTransaction _dbtrans;
        private readonly ITripleFormatter _formatter = new VirtuosoFormatter();

        private readonly string _dbserver, _dbname, _dbuser, _dbpwd;
        private readonly int _dbport, _timeout = 0;

        /// <summary>
        /// Indicates whether the Database Connection is currently being kept open.
        /// </summary>
        private bool _keepOpen = false;

        private readonly bool _customConnString = false;

        /// <summary>
        /// Creates a Manager for a Virtuoso Native Quad Store.
        /// </summary>
        /// <param name="server">Server.</param>
        /// <param name="port">Port.</param>
        /// <param name="db">Database Name.</param>
        /// <param name="user">Username.</param>
        /// <param name="password">Password.</param>
        /// <param name="timeout">Connection Timeout in Seconds.</param>
        /// <remarks>
        /// Timeouts less than equal to zero are ignored and treated as using the default timeout which is dictated by the underlying Virtuoso ADO.Net provider.
        /// </remarks>
        public VirtuosoManager(string server, int port, string db, string user, string password, int timeout)
        {
            //Set the Connection Properties
            _dbserver = server;
            _dbname = db;
            _dbuser = user;
            _dbpwd = password;
            _dbport = port;
            _timeout = timeout;

            var connString = new StringBuilder();
            connString.Append("Server=");
            connString.Append(_dbserver);
            connString.Append(":");
            connString.Append(_dbport);
            connString.Append(";Database=");
            connString.Append(_dbname);
            connString.Append(";uid=");
            connString.Append(_dbuser);
            connString.Append(";pwd=");
            connString.Append(_dbpwd);
            connString.Append(";Charset=utf-8");
            if (_timeout > 0)
            {
                connString.Append(";Connection Timeout=" + _timeout);
            }

            //Create the Connection Object
            _db = new VirtuosoConnection(connString.ToString());
        }

        /// <summary>
        /// Creates a Manager for a Virtuoso Native Quad Store.
        /// </summary>
        /// <param name="server">Server.</param>
        /// <param name="port">Port.</param>
        /// <param name="db">Database Name.</param>
        /// <param name="user">Username.</param>
        /// <param name="password">Password.</param>
        public VirtuosoManager(string server, int port, string db, string user, string password)
            : this(server, port, db, user, password, 0)
        {
        }

        /// <summary>
        /// Creates a Manager for a Virtuoso Native Quad Store.
        /// </summary>
        /// <param name="db">Database Name.</param>
        /// <param name="user">Username.</param>
        /// <param name="password">Password.</param>
        /// <param name="timeout">Connection Timeout in Seconds.</param>
        /// <remarks>
        /// Assumes the Server is on the localhost and the port is the default installation port of 1111.
        /// </remarks>
        public VirtuosoManager(string db, string user, string password, int timeout)
            : this("localhost", VirtuosoManager.DefaultPort, db, user, password, timeout)
        {
        }

        /// <summary>
        /// Creates a Manager for a Virtuoso Native Quad Store.
        /// </summary>
        /// <param name="db">Database Name.</param>
        /// <param name="user">Username.</param>
        /// <param name="password">Password.</param>
        /// <remarks>
        /// Assumes the Server is on the localhost and the port is the default installation port of 1111.
        /// </remarks>
        public VirtuosoManager(string db, string user, string password)
            : this("localhost", VirtuosoManager.DefaultPort, db, user, password, 0)
        {
        }

        /// <summary>
        /// Creates a Manager for a Virtuoso Native Quad Store.
        /// </summary>
        /// <param name="connectionString">Connection String.</param>
        /// <remarks>
        /// Allows the end user to specify a customized connection string.
        /// </remarks>
        public VirtuosoManager(string connectionString)
        {
            _db = new VirtuosoConnection(connectionString);
            _customConnString = true;
        }

        #endregion

        #region Triple Loading & Saving

        /// <summary>
        /// Loads a Graph from the Quad Store.
        /// </summary>
        /// <param name="g">Graph to load into.</param>
        /// <param name="graphUri">URI of the Graph to Load.</param>
        public override void LoadGraph(IGraph g, Uri graphUri)
        {
            if (g.IsEmpty && graphUri != null)
            {
                g.BaseUri = graphUri;
            }
            LoadGraph(new GraphHandler(g), graphUri);
        }

        /// <summary>
        /// Loads a Graph from the Quad Store.
        /// </summary>
        /// <param name="handler">RDF Handler.</param>
        /// <param name="graphUri">URI of the Graph to Load.</param>
        public override void LoadGraph(IRdfHandler handler, Uri graphUri)
        {
            if (graphUri == null) throw new RdfStorageException("Cannot load an unnamed Graph from Virtuoso as this would require loading the entirety of the Virtuoso Quad Store into memory!");

            try
            {
                handler.StartRdf();

                //Need to keep Database Open as Literals require extra trips to the Database to get additional
                //information about Language and Type
                Open(false);

                DataTable data = LoadTriples(graphUri);

                foreach (DataRow row in data.Rows)
                {
                    object s, p, o;
                    INode subj, pred, obj;

                    //Get Data
                    s = row["S"];
                    p = row["P"];
                    o = row["O"];

                    //Create Nodes
                    subj = LoadNode(handler, s);
                    pred = LoadNode(handler, p);
                    obj = LoadNode(handler, o);

                    //Assert Triple
                    if (!handler.HandleTriple(new Triple(subj, pred, obj))) ParserHelper.Stop();
                }
                handler.EndRdf(true);
                Close(false);
            }
            catch (RdfParsingTerminatedException)
            {
                handler.EndRdf(true);
                Close(false);
            }
            catch
            {
                handler.EndRdf(false);
                Close(true);
                throw;
            }
        }

        /// <summary>
        /// Loads a Graph from the Quad Store.
        /// </summary>
        /// <param name="g">Graph to load into.</param>
        /// <param name="graphUri">URI of the Graph to Load.</param>
        public override void LoadGraph(IGraph g, string graphUri)
        {
            if (graphUri == null || graphUri.Equals(string.Empty))
            {
                LoadGraph(g, (Uri) null);
            }
            else
            {
                LoadGraph(g, g.UriFactory.Create(graphUri));
            }
        }

        /// <summary>
        /// Loads a Graph from the Quad Store.
        /// </summary>
        /// <param name="handler">RDF Handler.</param>
        /// <param name="graphUri">URI of the Graph to Load.</param>
        public override void LoadGraph(IRdfHandler handler, string graphUri)
        {
            if (graphUri == null || graphUri.Equals(string.Empty))
            {
                LoadGraph(handler, (Uri) null);
            }
            else
            {
                LoadGraph(handler, UriFactory.Root.Create(graphUri));
            }
        }

        /// <summary>
        /// Gets a Table of Triples that are in the given Graph.
        /// </summary>
        /// <param name="graphUri">Graph Uri.</param>
        /// <returns></returns>
        /// <remarks>
        /// Assumes that the caller has opened the Database connection.
        /// </remarks>
        private DataTable LoadTriples(Uri graphUri)
        {
            var dt = new DataTable();
            string getTriples;
            if (graphUri != null)
            {
                getTriples = "SPARQL define output:format '_JAVA_' SELECT * FROM <" + UnmarshalUri(graphUri) + "> WHERE {?s ?p ?o}";
            }
            else
            {
                getTriples = "SPARQL define output:format '_JAVA_' SELECT * WHERE {?s ?p ?o}";
            }

            VirtuosoCommand cmd = _db.CreateCommand();
            cmd.CommandTimeout = _timeout > 0 ? _timeout : cmd.CommandTimeout;
            cmd.CommandText = getTriples;

            var adapter = new VirtuosoDataAdapter(cmd);

            dt.Columns.Add("S", typeof (object));
            dt.Columns.Add("P", typeof (object));
            dt.Columns.Add("O", typeof (object));
            adapter.Fill(dt);

            return dt;
        }

        /// <summary>
        /// Decodes an Object into an appropriate Node.
        /// </summary>
        /// <param name="factory">Node Factory to use to create Node.</param>
        /// <param name="n">Object to convert.</param>
        /// <returns></returns>
        private INode LoadNode(INodeFactory factory, object n)
        {
            INode temp;
            switch (n)
            {
                case SqlExtendedString iri when iri.IriType == SqlExtendedStringType.BNODE:
                    //Blank Node
                    temp = factory.CreateBlankNode(iri.ToString().Substring(9));
                    break;
                case SqlExtendedString iri when iri.IriType != iri.StrType:
                    //Literal
                    temp = factory.CreateLiteralNode(iri.ToString());
                    break;
                case SqlExtendedString iri when iri.IriType == SqlExtendedStringType.IRI:
                {
                    //Uri
                    Uri u = MarshalUri(iri.ToString());
                    temp = factory.CreateUriNode(u);
                    break;
                }
                case SqlExtendedString iri:
                    //Assume a Literal
                    temp = factory.CreateLiteralNode(iri.ToString());
                    break;
                case SqlRdfBox lit when lit.StrLang != null:
                    //Language Specified Literal
                    temp = factory.CreateLiteralNode(lit.ToString(), lit.StrLang);
                    break;
                case SqlRdfBox lit when lit.StrType != null:
                    //Data Typed Literal
                    temp = factory.CreateLiteralNode(lit.ToString(), MarshalUri(lit.StrType));
                    break;
                case SqlRdfBox lit:
                    //Literal
                    temp = factory.CreateLiteralNode(lit.ToString());
                    break;
                case string _:
                {
                    var s = n.ToString();
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

                    break;
                }
                case int i:
                    temp = i.ToLiteral(factory);
                    break;
                case short s:
                    temp = s.ToLiteral(factory);
                    break;
                case float f:
                    temp = f.ToLiteral(factory);
                    break;
                case double d:
                    temp = d.ToLiteral(factory);
                    break;
                case decimal @decimal:
                    temp = @decimal.ToLiteral(factory);
                    break;
                case DateTime time:
                    temp = time.ToLiteral(factory);
                    break;
                case TimeSpan span:
                    temp = span.ToLiteral(factory);
                    break;
                case bool b:
                    temp = b.ToLiteral(factory);
                    break;
                case DBNull _:
                    //Fix by Alexander Sidarov for Virtuoso's results for unbound variables in OPTIONALs
                    temp = null;
                    break;
                case VirtuosoDateTime vDateTime:
                {
                    //New type in Virtuoso 7
                    var dateTime = new DateTime(vDateTime.Year, vDateTime.Month, vDateTime.Day, vDateTime.Hour, vDateTime.Minute, vDateTime.Second, vDateTime.Millisecond, vDateTime.Kind);
                    return dateTime.ToLiteral(factory);
                }
                case VirtuosoDateTimeOffset vDateTimeOffset:
                {
                    //New type in Virtuoso 7
                    var dateTimeOffset = new DateTimeOffset(vDateTimeOffset.Year, vDateTimeOffset.Month, vDateTimeOffset.Day, vDateTimeOffset.Hour, vDateTimeOffset.Minute, vDateTimeOffset.Second, vDateTimeOffset.Millisecond, vDateTimeOffset.Offset);
                    return dateTimeOffset.ToLiteral(factory);
                }
                default:
                    throw new RdfStorageException(
                        $"Unexpected Object Type '{n.GetType()}' returned from SPARQL SELECT query to the Virtuoso Quad Store");
            }
            return temp;
        }

        private Uri MarshalUri(string uriData)
        {
            var u = new Uri(uriData, UriKind.RelativeOrAbsolute);
            if (!u.IsAbsoluteUri)
            {
                // As of VIRT-375 we marshal this to a form we can round trip later rather than erroring as we did previously
                u = new Uri(VirtuosoRelativeBase, u);
            }
            return u;
        }

        private string UnmarshalName(IRefNode name)
        {
            if (name.NodeType == NodeType.Uri)
            {
                return UnmarshalUri(((IUriNode)name).Uri);
            }

            if (name.NodeType == NodeType.Blank)
            {
                return ((IBlankNode)name).InternalID;
            }

            throw new ArgumentException("Unexpected node type", nameof(name));
        }

        private string UnmarshalUri(Uri u)
        {
            if (u.IsAbsoluteUri)
            {
                if (u.AbsoluteUri.StartsWith(VirtuosoRelativeBaseString))
                {
                    u = new Uri(u.AbsoluteUri.Substring(VirtuosoRelativeBase.AbsoluteUri.Length), UriKind.Relative);
                    return u.OriginalString;
                }
                else
                {
                    return u.AbsoluteUri;
                }
            }
            else
            {
                return u.OriginalString;
            }
        }

        /// <summary>
        /// Saves a Graph into the Quad Store (Warning: Completely replaces any existing Graph with the same URI).
        /// </summary>
        /// <param name="g">Graph to save.</param>
        /// <remarks>
        /// Completely replaces any previously saved Graph with the same Graph URI.
        /// </remarks>
        public override void SaveGraph(IGraph g)
        {
            if (g.Name == null) throw new RdfStorageException("Cannot save a Graph without a name to Virtuoso");

            try
            {
                Open(false);

                //Delete the existing Graph (if it exists)
                ExecuteNonQuery("DELETE FROM DB.DBA.RDF_QUAD WHERE G = DB.DBA.RDF_MAKE_IID_OF_QNAME('" + UnmarshalName(g.Name) + "')");

                //Make a call to the TTLP() Virtuoso function
                var cmd = new VirtuosoCommand();
                cmd.CommandTimeout = _timeout > 0 ? _timeout : cmd.CommandTimeout;
                cmd.CommandText = "DB.DBA.TTLP(@data, @base, @graph, 1)";
                cmd.Parameters.Add("data", VirtDbType.VarChar);
                cmd.Parameters["data"].Value = Writing.StringWriter.Write(g, new NTriplesWriter());
                //var baseUri = UnmarshalUri(g.BaseUri);
                var baseUri = UnmarshalName(g.Name);
                cmd.Parameters.Add("base", VirtDbType.VarChar);
                cmd.Parameters.Add("graph", VirtDbType.VarChar);
                cmd.Parameters["base"].Value = baseUri;
                cmd.Parameters["graph"].Value = baseUri;
                cmd.Connection = _db;
                cmd.ExecuteNonQuery();

                Close(false);
            }
            catch
            {
                Close(true);
                throw;
            }
        }

        /// <summary>
        /// Gets the IO Behaviour of the store.
        /// </summary>
        public override IOBehaviour IOBehaviour
        {
            get { return IOBehaviour.IsQuadStore | IOBehaviour.HasNamedGraphs | IOBehaviour.OverwriteNamed | IOBehaviour.CanUpdateTriples; }
        }

        /// <summary>
        /// Updates a Graph in the Quad Store.
        /// </summary>
        /// <param name="graphUri">Graph Uri of the Graph to update.</param>
        /// <param name="additions">Triples to be added.</param>
        /// <param name="removals">Triples to be removed.</param>
        /// <remarks>
        /// <para>
        /// In the case of inserts where blank nodes are present the data will be inserted but new blank nodes will be created.  You cannot insert data that refers to existing blank nodes via this method, consider using a INSERT WHERE style SPARQL Update instead.
        /// </para>
        /// <para>
        /// Note that Blank Nodes cannot always be deleted successfully, if you have retrieved the triples you are now trying to delete from Virtuoso and they contain blank nodes then this will likely work as expected.  Otherwise deletetions of Blank Nodes cannot be guaranteed.
        /// </para>
        /// <para>
        /// If the Graph being modified is relatively small it may be safer to load the graph into memory, makes the modifications there and then persist the graph back to the store (which overwrites the previous version of the graph).
        /// </para>
        /// </remarks>
        public override void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            try
            {
                Open(true);
                int r;

                //Build the Delete Data Command
                if (removals != null)
                {
                    if (removals.Any())
                    {
                        //HACK: This is super hacky but works in most cases provided the blank node containing triples
                        //we're attempting to delete originated from Virtuoso
                        //We use the VirtuosoFormatter as our formatter which formats Blank Nodes as calls to the
                        //bif:rdf_make_iid_of_qname('nodeID://bnode') function which works if the blank node originates from Virtuoso

                        var deleteCmd = new VirtuosoCommand();
                        deleteCmd.CommandTimeout = _timeout > 0 ? _timeout : deleteCmd.CommandTimeout;
                        var delete = new StringBuilder();
                        if (removals.All(t => t.IsGroundTriple))
                        {
                            delete.AppendLine("SPARQL define output:format '_JAVA_' DELETE DATA");
                        }
                        else
                        {
                            //If there are Blank Nodes present we must use a DELETE rather than a DELETE DATA since
                            //DELETE DATA does not allow the backquoted expressions required to do this hack
                            delete.AppendLine("SPARQL define output:format '_JAVA_' DELETE");
                        }
                        if (graphUri != null)
                        {
                            delete.AppendLine(" FROM <" + UnmarshalUri(graphUri) + ">");
                        }
                        else
                        {
                            throw new RdfStorageException("Cannot update an unnamed Graph in a Virtuoso Store using this method - you must specify the URI of a Graph to Update");
                        }
                        delete.AppendLine("{");
                        foreach (Triple t in removals)
                        {
                            delete.AppendLine(t.ToString(_formatter));
                        }
                        delete.AppendLine("}");

                        //If there are Blank Nodes present we will be using a DELETE rather than a DELETE DATA
                        //so we need to add a WHERE clause
                        if (removals.Any(t => !t.IsGroundTriple))
                        {
                            delete.AppendLine("WHERE { }");
                        }

                        //Run the Delete
                        deleteCmd.CommandText = delete.ToString();
                        deleteCmd.Connection = _db;
                        deleteCmd.Transaction = _dbtrans;

                        r = deleteCmd.ExecuteNonQuery();
                        if (r < 0) throw new RdfStorageException("Virtuoso encountered an error when deleting Triples");
                    }
                }

                //Build the Insert Data Command
                if (additions != null)
                {
                    if (additions.Any())
                    {
                        if (additions.All(t => t.IsGroundTriple))
                        {
                            var insertCmd = new VirtuosoCommand();
                            insertCmd.CommandTimeout = _timeout > 0 ? _timeout : insertCmd.CommandTimeout;
                            var insert = new StringBuilder();
                            insert.AppendLine("SPARQL define output:format '_JAVA_' INSERT DATA");
                            if (graphUri != null)
                            {
                                insert.AppendLine(" INTO <" + UnmarshalUri(graphUri) + ">");
                            }
                            else
                            {
                                throw new RdfStorageException("Cannot update an unnamed Graph in Virtuoso using this method - you must specify the URI of a Graph to Update");
                            }
                            insert.AppendLine("{");
                            foreach (Triple t in additions)
                            {
                                insert.AppendLine(t.ToString(_formatter));
                            }
                            insert.AppendLine("}");
                            insertCmd.CommandText = insert.ToString();
                            insertCmd.Connection = _db;
                            insertCmd.Transaction = _dbtrans;

                            r = insertCmd.ExecuteNonQuery();
                            if (r < 0) throw new RdfStorageException("Virtuoso encountered an error when inserting Triples");
                        }
                        else
                        {
                            //When data to be inserted contains Blank Nodes we must make a call to the TTLP() Virtuoso function
                            //instead of using INSERT DATA
                            var g = new Graph();
                            g.Assert(additions);
                            var cmd = new VirtuosoCommand();
                            cmd.CommandTimeout = _timeout > 0 ? _timeout : cmd.CommandTimeout;
                            cmd.CommandText = "DB.DBA.TTLP(@data, @base, @graph, 1)";
                            cmd.Parameters.Add("data", VirtDbType.VarChar);
                            cmd.Parameters["data"].Value = VDS.RDF.Writing.StringWriter.Write(g, new NTriplesWriter());
                            var baseUri = UnmarshalUri(graphUri);
                            if (string.IsNullOrEmpty(baseUri)) throw new RdfStorageException("Cannot updated an unnamed Graph in Virtuoso using this method - you must specify the URI of a Graph to Update");
                            cmd.Parameters.Add("base", VirtDbType.VarChar);
                            cmd.Parameters.Add("graph", VirtDbType.VarChar);
                            cmd.Parameters["base"].Value = baseUri;
                            cmd.Parameters["graph"].Value = baseUri;
                            cmd.Connection = _db;
                            var result = cmd.ExecuteNonQuery();
                        }
                    }
                }

                Close(false);
            }
            catch
            {
                Close(true, true);
                throw;
            }
        }

        /// <summary>
        /// Updates a Graph in the Quad Store.
        /// </summary>
        /// <param name="graphUri">Graph Uri of the Graph to update.</param>
        /// <param name="additions">Triples to be added.</param>
        /// <param name="removals">Triples to be removed.</param>
        public override void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            Uri u = graphUri.Equals(string.Empty) ? null : UriFactory.Root.Create(graphUri);
            UpdateGraph(u, additions, removals);
        }

        /// <summary>
        /// Indicates that Updates are supported by the Virtuoso Native Quad Store.
        /// </summary>
        public override bool UpdateSupported
        {
            get { return true; }
        }

        /// <summary>
        /// Returns that the Manager is ready.
        /// </summary>
        public override bool IsReady
        {
            get { return true; }
        }

        /// <summary>
        /// Returns that the Manager is not read-only.
        /// </summary>
        public override bool IsReadOnly
        {
            get { return false; }
        }

        #endregion

        #region Native Query & Update

        /// <summary>
        /// Executes a SPARQL Query on the native Quad Store.
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query to execute.</param>
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
        /// <exception cref="RdfQueryException">Thrown if an error occurs in making the query.</exception>
        public object Query(string sparqlQuery)
        {
            var g = new Graph();
            var results = new SparqlResultSet();
            Query(new GraphHandler(g), new ResultSetHandler(results), sparqlQuery);

            if (results.ResultsType != SparqlResultsType.Unknown)
            {
                return results;
            }
            return g;
        }

        /// <summary>
        /// Executes a SPARQL Query on the native Quad Store processing the results with an appropriate handler from those provided.
        /// </summary>
        /// <param name="rdfHandler">RDF Handler.</param>
        /// <param name="resultsHandler">Results Handler.</param>
        /// <param name="sparqlQuery">SPARQL Query to execute.</param>
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
        /// <exception cref="RdfQueryException">Thrown if an error occurs in making the query.</exception>
        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery)
        {
            try
            {
                if (resultsHandler != null) resultsHandler.StartResults();

                var results = new DataTable();
                results.Columns.CollectionChanged += Columns_CollectionChanged;

                //See if the query can be parsed into a SparqlQuery object
                //It might not since the user might use Virtuoso's extensions to Sparql in their query
                try
                {
                    //We'll set the Parser to SPARQL 1.1 mode even though Virtuoso's SPARQL implementation has
                    //various peculiarties in their SPARQL 1.1 implementation and we'll try and 
                    //handle the potential results in the catch branch if a valid SPARQL 1.0 query
                    //cannot be parsed
                    //Change made in response to a bug report by Aleksandr A. Zaripov [zaripov@tpu.ru]
                    var parser = new SparqlQueryParser {SyntaxMode = SparqlQuerySyntax.Sparql_1_1};
                    SparqlQuery query;
                    try
                    {
                        query = parser.ParseFromString(sparqlQuery);
                    }
                    catch (RdfException rdfEx)
                    {
                        //Need to re-wrap errors during parsing so we fall into correct catch branch, can't generally re-wrap as we might
                        //get RdfException's from other places which would indicate unrecoverable errors
                        throw new RdfParseException("RDF exception generated in query parsing", rdfEx);
                    }

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
                                    results.Columns.Add(var.Name, typeof (object));
                                }
                            }
                            break;
                    }

                    try
                    {
                        #region Valid SPARQL Query Handling

                        Open(false);

                        //Make the Query against Virtuoso
                        VirtuosoCommand cmd = _db.CreateCommand();
                        cmd.CommandTimeout = _timeout > 0 ? _timeout : cmd.CommandTimeout;
                        cmd.CommandText = "SPARQL " + sparqlQuery;
                        var adapter = new VirtuosoDataAdapter(cmd);
                        adapter.Fill(results);

                        //Decide how to process the results based on the return type
                        switch (query.QueryType)
                        {
                            case SparqlQueryType.Ask:
                                //Expect a DataTable containing a single row and column which contains a boolean

                                //Ensure Results Handler is not null
                                if (resultsHandler == null) throw new ArgumentNullException(nameof(resultsHandler), "Cannot handle a Boolean Result with a null SPARQL Results Handler");

                                bool result;
                                if (results.Rows.Count == 1 && results.Columns.Count == 1)
                                {
                                    //Try and parse the result
                                    if (bool.TryParse(results.Rows[0][0].ToString(), out result))
                                    {
                                        resultsHandler.HandleBooleanResult(result);
                                    }
                                    else if (int.TryParse(results.Rows[0][0].ToString(), out var r))
                                    {
                                        resultsHandler.HandleBooleanResult(r == 1);
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
                                if (rdfHandler == null) throw new ArgumentNullException(nameof(rdfHandler), "Cannot handle a Graph result with a null RDF Handler");

                                if (results.Rows.Count == 1 && results.Columns.Count == 1)
                                {
                                    try
                                    {
                                        //Use StringParser to parse
                                        var data = results.Rows[0][0].ToString();
                                        var ttlparser = new TurtleParser();
                                        ttlparser.Load(rdfHandler, new StringReader(data));
                                    }
                                    catch (RdfParseException parseEx)
                                    {
                                        throw new RdfQueryException("Expected a valid Turtle serialization of the Graph resulting from a CONSTRUCT/DESCRIBE query but the result failed to parse", parseEx);
                                    }
                                }
                                else if (results.Columns.Count == 3)
                                {
                                    rdfHandler.StartRdf();
                                    try
                                    {
                                        foreach (DataRow row in results.Rows)
                                        {
                                            INode s = LoadNode(rdfHandler, row[0]);
                                            INode p = LoadNode(rdfHandler, row[1]);
                                            INode o = LoadNode(rdfHandler, row[2]);
                                            if (!rdfHandler.HandleTriple(new Triple(s, p, o))) break;
                                        }
                                        rdfHandler.EndRdf(true);
                                    }
                                    catch
                                    {
                                        rdfHandler.EndRdf(false);
                                        throw;
                                    }
                                }
                                else
                                {
                                    throw new RdfQueryException("Unexpected results data received for a CONSTRUCT/DESCRIBE query (Got " + results.Rows.Count + " row(s) with " + results.Columns.Count + " column(s)");
                                }
                                break;

                            case SparqlQueryType.Select:
                            case SparqlQueryType.SelectAll:
                            case SparqlQueryType.SelectAllDistinct:
                            case SparqlQueryType.SelectAllReduced:
                            case SparqlQueryType.SelectDistinct:
                            case SparqlQueryType.SelectReduced:
                                //Ensure Results Handler is not null
                                if (resultsHandler == null) throw new ArgumentNullException(nameof(resultsHandler), "Cannot handle SPARQL Results with a null Results Handler");

                                //Get Result Variables
                                var resultVars = query.Variables.Where(v => v.IsResultVariable).ToList();
                                foreach (SparqlVariable var in resultVars)
                                {
                                    if (!resultsHandler.HandleVariable(var.Name)) ParserHelper.Stop();
                                }
                                var temp = new Graph();

                                //Convert each solution into a SPARQLResult
                                foreach (DataRow r in results.Rows)
                                {
                                    var s = new Set();
                                    foreach (SparqlVariable var in resultVars)
                                    {
                                        if (r[var.Name] != null)
                                        {
                                            s.Add(var.Name, LoadNode(temp, r[var.Name]));
                                        }
                                    }
                                    if (!resultsHandler.HandleResult(new SparqlResult(s))) ParserHelper.Stop();
                                }
                                break;

                            default:
                                throw new RdfQueryException("Unable to process the Results of an Unknown query type");
                        }

                        Close(false);

                        #endregion
                    }
                    catch
                    {
                        throw;
                    }
                }
                catch (RdfParseException)
                {
                    //Unable to parse a SPARQL 1.0 query
                    //Have to attempt to detect the return type based on the DataTable that
                    //the SPASQL (Sparql+SQL) query gives back

                    try
                    {
                        #region Potentially Invalid SPARQL Query Handling

                        Open(false);

                        //Make the Query against Virtuoso
                        VirtuosoCommand cmd = _db.CreateCommand();
                        cmd.CommandTimeout = _timeout > 0 ? _timeout : cmd.CommandTimeout;
                        cmd.CommandText = "SPARQL " /*define output:format '_JAVA_' "*/+ sparqlQuery;
                        var adapter = new VirtuosoDataAdapter(cmd);
                        adapter.Fill(results);

                        //Try to detect the return type based on the DataTable configuration
                        if (results.Columns.Count == 3
                                && results.Columns[0].ColumnName.Equals(SubjectColumn)
                                && results.Columns[1].ColumnName.Equals(PredicateColumn)
                                && results.Columns[2].ColumnName.Equals(ObjectColumn)
                                && !Regex.IsMatch(sparqlQuery, "SELECT", RegexOptions.IgnoreCase))
                        {
                            //Ensure that RDF Handler is not null
                            if (rdfHandler == null) throw new ArgumentNullException(nameof(rdfHandler), "Cannot handle a Graph result with a null RDF Handler");

                            rdfHandler.StartRdf();
                            try
                            {
                                foreach (DataRow row in results.Rows)
                                {
                                    INode s = LoadNode(rdfHandler, row[0]);
                                    INode p = LoadNode(rdfHandler, row[1]);
                                    INode o = LoadNode(rdfHandler, row[2]);
                                    if (!rdfHandler.HandleTriple(new Triple(s, p, o))) break;
                                }
                                rdfHandler.EndRdf(true);
                            }
                            catch
                            {
                                rdfHandler.EndRdf(false);
                                throw;
                            }
                        }
                        else if (results.Rows.Count == 0 && results.Columns.Count > 0)
                        {
                            if (resultsHandler == null) throw new ArgumentNullException(nameof(resultsHandler), "Cannot handle SPARQL Results with a null Results Handler");

                            //No Rows but some columns implies empty SELECT results
                            foreach (DataColumn col in results.Columns)
                            {
                                if (!resultsHandler.HandleVariable(col.ColumnName)) ParserHelper.Stop();
                            }
                        }
                        else if (results.Rows.Count == 1 && results.Columns.Count == 1 && !Regex.IsMatch(sparqlQuery, "SELECT", RegexOptions.IgnoreCase))
                        {
                            //Added a fix here suggested by Alexander Sidorov - not entirely happy with this fix as what happens if SELECT just happens to occur in a URI/Variable Name?

                            //Single Row and Column implies ASK/DESCRIBE/CONSTRUCT results

                            if (results.Rows[0][0].ToString().Equals(string.Empty))
                            {
                                //Empty Results - no need to do anything
                            }
                            else if (bool.TryParse(results.Rows[0][0].ToString(), out var result))
                            {
                                //Parseable Boolean so ASK Results
                                if (resultsHandler == null) throw new ArgumentNullException(nameof(resultsHandler), "Cannot handle a Boolean result with a null Results Handler");
                                resultsHandler.HandleBooleanResult(result);
                            }
                            else if (int.TryParse(results.Rows[0][0].ToString(), out var r))
                            {
                                if (resultsHandler == null) throw new ArgumentNullException(nameof(resultsHandler), "Cannot handle SPARQL results with a null Results Handler");

                                //Parseable Integer so Aggregate SELECT Query Results
                                if (!resultsHandler.HandleVariable("Result")) ParserHelper.Stop();
                                var s = new Set();
                                s.Add("Result", r.ToLiteral(resultsHandler));
                                if (!resultsHandler.HandleResult(new SparqlResult(s))) ParserHelper.Stop();
                            }
                            else if (float.TryParse(results.Rows[0][0].ToString(), out var rflt))
                            {
                                if (resultsHandler == null) throw new ArgumentNullException(nameof(resultsHandler), "Cannot handle SPARQL results with a null Results Handler");

                                //Parseable Single so Aggregate SELECT Query Results
                                if (!resultsHandler.HandleVariable("Result")) ParserHelper.Stop();
                                var s = new Set();
                                s.Add("Result", rflt.ToLiteral(resultsHandler));
                                if (!resultsHandler.HandleResult(new SparqlResult(s))) ParserHelper.Stop();
                            }
                            else if (double.TryParse(results.Rows[0][0].ToString(), out var rdbl))
                            {
                                if (resultsHandler == null) throw new ArgumentNullException(nameof(resultsHandler), "Cannot handle SPARQL results with a null Results Handler");

                                //Parseable Double so Aggregate SELECT Query Results
                                if (!resultsHandler.HandleVariable("Result")) ParserHelper.Stop();
                                var s = new Set();
                                s.Add("Result", rdbl.ToLiteral(resultsHandler));
                                if (!resultsHandler.HandleResult(new SparqlResult(s))) ParserHelper.Stop();
                            }
                            else if (decimal.TryParse(results.Rows[0][0].ToString(), out var rdec))
                            {
                                if (resultsHandler == null) throw new ArgumentNullException(nameof(resultsHandler), "Cannot handle SPARQL results with a null Results Handler");

                                //Parseable Decimal so Aggregate SELECT Query Results
                                if (!resultsHandler.HandleVariable("Result")) ParserHelper.Stop();
                                var s = new Set();
                                s.Add("Result", rdec.ToLiteral(resultsHandler));
                                if (!resultsHandler.HandleResult(new SparqlResult(s))) ParserHelper.Stop();
                            }
                            else
                            {
                                //String so try and parse as Turtle
                                try
                                {
                                    //Use StringParser to parse
                                    var data = results.Rows[0][0].ToString();
                                    var ttlparser = new TurtleParser();
                                    ttlparser.Load(rdfHandler, new StringReader(data));
                                }
                                catch (RdfParseException)
                                {
                                    if (resultsHandler == null) throw new ArgumentNullException(nameof(resultsHandler), "Cannot handle SPARQL results with a null Results Handler");

                                    //If it failed to parse then it might be the result of one of the aggregate
                                    //functions that Virtuoso extends Sparql with
                                    if (!resultsHandler.HandleVariable(results.Columns[0].ColumnName)) ParserHelper.Stop();
                                    var s = new Set();
                                    s.Add(results.Columns[0].ColumnName, LoadNode(resultsHandler, results.Rows[0][0]));
                                    //Nothing was returned here previously - fix submitted by Aleksandr A. Zaripov [zaripov@tpu.ru]
                                    if (!resultsHandler.HandleResult(new SparqlResult(s))) ParserHelper.Stop();
                                }
                            }
                        }
                        else
                        {
                            //Any other number of rows/columns we have to assume that it's normal SELECT results
                            //Changed in response to bug report by Aleksandr A. Zaripov [zaripov@tpu.ru]

                            if (resultsHandler == null) throw new ArgumentNullException(nameof(resultsHandler), "Cannot handle SPARQL results with a null Results Handler");

                            //Get Result Variables
                            var vars = new List<string>();
                            foreach (DataColumn col in results.Columns)
                            {
                                vars.Add(col.ColumnName);
                                if (!resultsHandler.HandleVariable(col.ColumnName)) ParserHelper.Stop();
                            }

                            //Convert each solution into a SPARQLResult
                            foreach (DataRow r in results.Rows)
                            {
                                var s = new Set();
                                foreach (var var in vars)
                                {
                                    if (r[var] != null)
                                    {
                                        s.Add(var, LoadNode(resultsHandler, r[var]));
                                    }
                                }
                                if (!resultsHandler.HandleResult(new SparqlResult(s))) ParserHelper.Stop();
                            }
                        }
                        Close(false);

                        #endregion
                    }
                    catch
                    {
                        Close(true, true);
                        throw;
                    }
                }

                if (resultsHandler != null) resultsHandler.EndResults(true);
                Close(false);
            }
            catch (RdfParsingTerminatedException)
            {
                if (resultsHandler != null) resultsHandler.EndResults(true);
                Close(false);
            }
            catch
            {
                Close(true);
                if (resultsHandler != null) resultsHandler.EndResults(false);
                Close(false);
                throw;
            }
        }

        private static void Columns_CollectionChanged(object sender, System.ComponentModel.CollectionChangeEventArgs e)
        {
            Type reqType = typeof (object);
            if (e.Action != System.ComponentModel.CollectionChangeAction.Add) return;
            var column = (DataColumn) e.Element;
            if (!column.DataType.Equals(reqType))
            {
                column.DataType = reqType;
            }
        }

        /// <summary>
        /// Executes a SPARQL Update on the native Quad Store.
        /// </summary>
        /// <param name="sparqlUpdate">SPARQL Update to execute.</param>
        /// <remarks>
        /// <para>
        /// This method will first attempt to parse the update into a <see cref="SparqlUpdateCommandSet">SparqlUpdateCommandSet</see> object.  If this succeeds then each command in the command set will be issued to Virtuoso.
        /// </para>
        /// <para>
        /// If the parsing fails then the update will be executed anyway using Virtuoso's SPASQL (SPARQL + SQL) syntax.  Parsing can fail because Virtuoso supports various SPARQL extensions which the library does not support and primarily supports SPARUL updates (the precusor to SPARQL 1.1 Update).
        /// </para>
        /// </remarks>
        /// <exception cref="SparqlUpdateException">Thrown if an error occurs in making the update.</exception>
        public void Update(string sparqlUpdate)
        {
            try
            {
                Open(true);

                //Try and parse the SPARQL Update String
                var parser = new SparqlUpdateParser();
                SparqlUpdateCommandSet commands = parser.ParseFromString(sparqlUpdate);

                //Process each Command individually
                foreach (SparqlUpdateCommand command in commands.Commands)
                {
                    //Make the Update against Virtuoso
                    VirtuosoCommand cmd = _db.CreateCommand();
                    cmd.CommandTimeout = _timeout > 0 ? _timeout : cmd.CommandTimeout;
                    cmd.CommandText = "SPARQL " + command.ToString();
                    cmd.ExecuteNonQuery();
                }

                Close(true);
            }
            catch (RdfParseException)
            {
                try
                {
                    //Ignore failed parsing and attempt to execute anyway
                    VirtuosoCommand cmd = _db.CreateCommand();
                    cmd.CommandTimeout = _timeout > 0 ? _timeout : cmd.CommandTimeout;
                    cmd.CommandText = "SPARQL " + sparqlUpdate;

                    cmd.ExecuteNonQuery();
                    Close(true);
                }
                catch (Exception ex)
                {
                    Close(true, true);
                    throw new SparqlUpdateException("An error occurred while trying to perform the SPARQL Update with Virtuoso.  Note that Virtuoso historically has primarily supported SPARUL (the precursor to SPARQL Update) and many valid SPARQL Update Commands may not be supported by Virtuoso", ex);
                }
            }
            catch (SparqlUpdateException)
            {
                Close(true, true);
                throw;
            }
            catch (Exception ex)
            {
                //Wrap in a SPARQL Update Exception
                Close(true, true);
                throw new SparqlUpdateException("An error occurred while trying to perform the SPARQL Update with Virtuoso.  Note that Virtuoso historically has primarily supported SPARUL (the precursor to SPARQL Update) and many valid SPARQL Update Commands may not be supported by Virtuoso if you are not using a recent version.", ex);
            }
        }

        #endregion

        /// <summary>
        /// Deletes a Graph from the Virtuoso store.
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete.</param>
        public override void DeleteGraph(Uri graphUri)
        {
            DeleteGraph(UnmarshalUri(graphUri));
        }

        /// <summary>
        /// Deletes a Graph from the store.
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete.</param>
        public override void DeleteGraph(string graphUri)
        {
            if (graphUri == null) return;
            if (graphUri.Equals(string.Empty)) return;

            try
            {
                Open(false);
                ExecuteNonQuery("DELETE FROM DB.DBA.RDF_QUAD WHERE G = DB.DBA.RDF_MAKE_IID_OF_QNAME('" + graphUri + "')");
                Close(false);
            }
            catch
            {
                Close(true, true);
                throw;
            }
        }

        /// <summary>
        /// Returns that deleting Graphs is supported.
        /// </summary>
        public override bool DeleteSupported
        {
            get { return true; }
        }

        /// <summary>
        /// Lists the Graphs in the store.
        /// </summary>
        /// <returns></returns>
        [Obsolete("Replaced by ListGraphNames")]
        public override IEnumerable<Uri> ListGraphs()
        {
            try
            {
                Open(false);
                var results = new DataTable();
                VirtuosoCommand cmd = _db.CreateCommand();
                cmd.CommandTimeout = _timeout > 0 ? _timeout : cmd.CommandTimeout;
                cmd.CommandText = "DB.DBA.SPARQL_SELECT_KNOWN_GRAPHS()";
                var adapter = new VirtuosoDataAdapter(cmd);
                adapter.Fill(results);
                var graphs = new List<Uri>();
                foreach (DataRow row in results.Rows)
                {
                    try
                    {
                        graphs.Add(new Uri(row[0].ToString()));
                    }
                    catch (UriFormatException)
                    {
                        // Virtuoso graph names include some non-URI names. These will be ignored
                    }
                }

                return graphs;
            }
            catch (Exception ex)
            {
                throw new RdfStorageException("Underlying Store returned an error while trying to List Graphs", ex);
            }
            finally
            {
                Close(false);
            }
        }

        /// <summary>
        /// Gets an enumeration of the names of the graphs in the store.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Implementations should implement this method only if they need to provide a custom way of listing Graphs.  If the Store for which you are providing a manager can efficiently return the Graphs using a SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } } query then there should be no need to implement this function.
        /// </para>
        /// </remarks>
        public override IEnumerable<string> ListGraphNames()
        {
            try
            {
                Open(false);
                var results = new DataTable();
                VirtuosoCommand cmd = _db.CreateCommand();
                cmd.CommandTimeout = _timeout > 0 ? _timeout : cmd.CommandTimeout;
                cmd.CommandText = "DB.DBA.SPARQL_SELECT_KNOWN_GRAPHS()";
                var adapter = new VirtuosoDataAdapter(cmd);
                adapter.Fill(results);
                var graphs = new List<string>();
                foreach (DataRow row in results.Rows)
                {
                    graphs.Add(row[0].ToString());
                }

                return graphs;
            }
            catch (Exception ex)
            {
                throw new RdfStorageException("Underlying Store returned an error while trying to List Graphs", ex);
            }
            finally
            {
                Close(false);
            }
        }

        /// <summary>
        /// Returns that listing graphs is supported.
        /// </summary>
        public override bool ListGraphsSupported
        {
            get { return true; }
        }

        #region Database IO

        /// <summary>
        /// Opens a Connection to the Database.
        /// </summary>
        /// <param name="keepOpen">Indicates that the Connection should be kept open and a Transaction started.</param>
        private void Open(bool keepOpen)
        {
            Open(keepOpen, IsolationLevel.ReadCommitted);
        }

        /// <summary>
        /// Opens a Connection to the Database.
        /// </summary>
        /// <param name="keepOpen">Indicates that the Connection should be kept open and a Transaction started.</param>
        /// <param name="level">Isolation Level to use.</param>
        private void Open(bool keepOpen, IsolationLevel level)
        {
            switch (_db.State)
            {
                case ConnectionState.Broken:
                case ConnectionState.Closed:
                    _db.Open();

                    //Start a Transaction
                    if (_dbtrans == null)
                    {
                        _dbtrans = _db.BeginTransaction(level);
                    }
                    break;
            }
            if (keepOpen) _keepOpen = true;
        }

        /// <summary>
        /// Closes the Connection to the Database.
        /// </summary>
        /// <param name="forceClose">Indicates that the connection should be closed even if keepOpen was specified when the Connection was opened.</param>
        private void Close(bool forceClose)
        {
            Close(forceClose, false);
        }

        /// <summary>
        /// Closes the Connection to the Database.
        /// </summary>
        /// <param name="forceClose">Indicates that the connection should be closed even if keepOpen was specified when the Connection was opened.</param>
        /// <param name="rollbackTrans">Indicates that the Transaction should be rolled back because something has gone wrong.</param>
        private void Close(bool forceClose, bool rollbackTrans)
        {
            //Don't close if we're keeping open and not forcing Close or rolling back a Transaction
            if (_keepOpen && !forceClose && !rollbackTrans)
            {
                return;
            }

            switch (_db.State)
            {
                case ConnectionState.Open:
                    //Finish the Transaction if exists
                    if (_dbtrans != null)
                    {
                        if (!rollbackTrans)
                        {
                            //Commit normally
                            _dbtrans.Commit();
                        }
                        else
                        {
                            //Want to Rollback
                            _dbtrans.Rollback();
                        }
                        _dbtrans = null;

                        _db.Close();
                    }

                    _keepOpen = false;
                    break;
            }
        }

        /// <summary>
        /// Executes a Non-Query SQL Command against the database.
        /// </summary>
        /// <param name="sqlCmd">SQL Command.</param>
        private void ExecuteNonQuery(string sqlCmd)
        {
            //Create the SQL Command
            var cmd = new VirtuosoCommand(sqlCmd, _db);
            cmd.CommandTimeout = _timeout > 0 ? _timeout : cmd.CommandTimeout;
            if (_dbtrans != null)
            {
                //Add to the Transaction if required
                cmd.Transaction = _dbtrans;
            }

            //Execute
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Gets whether there is an active connection to the Virtuoso database.
        /// </summary>
        public bool HasOpenConnection
        {
            get { return _db.State != ConnectionState.Broken && _db.State != ConnectionState.Closed; }
        }

        /// <summary>
        /// Gets whether there is any active transaction on the Virtuoso database.
        /// </summary>
        public bool HasActiveTransaction
        {
            get { return _dbtrans != null; }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Disposes of the Manager.
        /// </summary>
        public override void Dispose()
        {
            Close(true, false);
        }

        #endregion

        /// <summary>
        /// Gets a String which gives details of the Connection.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (_customConnString)
            {
                return "[Virtuoso] Custom Connection String";
            }
            return "[Virtuoso] " + _dbserver + ":" + _dbport;
        }

        /// <summary>
        /// Serializes the connection's configuration.
        /// </summary>
        /// <param name="context">Configuration Serialization Context.</param>
        public void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            if (_customConnString)
            {
                throw new DotNetRdfConfigurationException("Cannot serialize the configuration of a VirtuosoManager which was created with a custom connection string");
            }

            //Firstly need to ensure our object factory has been referenced
            context.EnsureObjectFactory(typeof (VirtuosoObjectFactory));

            //Then serialize the actual configuration
            INode dnrType = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyType));
            INode rdfType = context.Graph.CreateUriNode(context.UriFactory.Create(RdfSpecsHelper.RdfType));
            INode manager = context.NextSubject;
            INode rdfsLabel = context.Graph.CreateUriNode(context.UriFactory.Create(NamespaceMapper.RDFS + "label"));
            INode genericManager = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.ClassStorageProvider));
            INode server = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyServer));

            context.Graph.Assert(new Triple(manager, rdfType, genericManager));
            context.Graph.Assert(new Triple(manager, rdfsLabel, context.Graph.CreateLiteralNode(ToString())));
            context.Graph.Assert(new Triple(manager, dnrType, context.Graph.CreateLiteralNode(GetType().FullName + ", dotNetRDF.Data.Virtuoso")));
            context.Graph.Assert(new Triple(manager, server, context.Graph.CreateLiteralNode(_dbserver)));

            if (_dbport != DefaultPort)
            {
                INode port = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyPort));
                context.Graph.Assert(new Triple(manager, port, _dbport.ToLiteral(context.Graph)));
            }
            if (!_dbname.Equals(DefaultDB))
            {
                INode db = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyDatabase));
                context.Graph.Assert(new Triple(manager, db, context.Graph.CreateLiteralNode(_dbname)));
            }
            if (_timeout > 0)
            {
                INode timeout = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyTimeout));
                context.Graph.Assert(new Triple(manager, timeout, _timeout.ToLiteral(context.Graph)));
            }
            if (_dbuser != null && _dbpwd != null)
            {
                INode username = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyUser));
                INode pwd = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyPassword));
                context.Graph.Assert(new Triple(manager, username, context.Graph.CreateLiteralNode(_dbuser)));
                context.Graph.Assert(new Triple(manager, pwd, context.Graph.CreateLiteralNode(_dbpwd)));
            }
        }
    }
}
