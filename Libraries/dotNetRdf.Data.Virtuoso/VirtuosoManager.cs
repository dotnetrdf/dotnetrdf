/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2021 dotNetRDF Project (http://dotnetrdf.org/)
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
using OpenLink.Data.Virtuoso;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Update;

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
        : VirtuosoConnectorBase, IUpdateableStorage, IConfigurationSerializable
    {
        #region Variables & Constructors

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
        :base(server, port, db, user, password, timeout)
        {
            
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
            : base(server, port, db, user, password)
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
            : base("localhost", VirtuosoManager.DefaultPort, db, user, password, timeout)
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
            : base("localhost", VirtuosoManager.DefaultPort, db, user, password)
        {
        }

        /// <summary>
        /// Creates a Manager for a Virtuoso Native Quad Store.
        /// </summary>
        /// <param name="connectionString">Connection String.</param>
        /// <remarks>
        /// Allows the end user to specify a customized connection string.
        /// </remarks>
        public VirtuosoManager(string connectionString):base(connectionString)
        {
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
                LoadGraph(handler, UriFactory.Create(graphUri));
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
                Open();

                SaveGraphInternal(g);

                Close(false);
            }
            catch
            {
                Close(true);
                throw;
            }
        }

        #endregion

        #region Native Query & Update

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
        public override void Update(string sparqlUpdate)
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
                    cmd.CommandTimeout = _dbTimeout > 0 ? _dbTimeout : cmd.CommandTimeout;
                    cmd.CommandText = "SPARQL " + command;
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
                    cmd.CommandTimeout = _dbTimeout > 0 ? _dbTimeout : cmd.CommandTimeout;
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


        #region IDisposable Members

        /// <summary>
        /// Disposes of the Manager.
        /// </summary>
        public override void Dispose()
        {
            Close(true);
        }

        #endregion

        /// <summary>
        /// Gets a String which gives details of the Connection.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (_customConnectionString)
            {
                return "[Virtuoso] Custom Connection String";
            }
            return "[Virtuoso] " + _dbServer + ":" + _dbPort;
        }
    }
}
