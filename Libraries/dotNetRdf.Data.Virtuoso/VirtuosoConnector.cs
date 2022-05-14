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

using OpenLink.Data.Virtuoso;
using System;
using System.Collections.Generic;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Update;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// A connector for accessing the native Virtuoso Quad Store.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class implements <see cref="IStorageProvider">IStorageProvider</see> allowing it to be used with any of
    /// the general classes that support this interface as well as the Virtuoso specific classes.
    /// </para>
    /// <para>
    /// This class supports user-controlled transactions via to <see cref="ITransactionalStorage"/> interface.
    /// Calls to methods outside of a user-controlled transaction will take place in their own transaction which will
    /// be auto-committed on success if required.
    /// </para>
    /// <para>
    /// Although this class takes a Database Name to ensure compatibility with any Virtuoso installation
    /// (i.e. this allows for the Native Quad Store to be in a non-standard database) generally you should always
    /// specify <strong>DB</strong> as the Database Name parameter.
    /// </para>
    /// <para>
    /// Virtuoso automatically assigns IDs to Blank Nodes input into it, these IDs are <strong>not</strong> based on
    /// the actual Blank Node ID so inputting a Blank Node with the same ID multiple times will result in multiple
    /// Nodes being created in Virtuoso.  This means that data containing Blank Nodes which is stored to Virtuoso and
    /// then retrieved will have different Blank Node IDs to those input.  In addition there is no guarantee that when
    /// you save a Graph containing Blank Nodes into Virtuoso that retrieving it will give the same Blank Node IDs
    /// even if the Graph being saved was originally retrieved from Virtuoso.  Finally please see the remarks on the
    /// <see cref="VirtuosoConnectorBase.UpdateGraph(Uri,IEnumerable{Triple},IEnumerable{Triple})">UpdateGraph()</see>
    /// method which deal with how insertion and deletion of triples containing blank nodes into existing graphs operates.
    /// </para>
    /// <para>
    /// You can use a null Uri or an empty String as a Uri to indicate that operations should affect the Default Graph.  Where the argument is only a Graph a null <see cref="IGraph.Name"/> property indicates that the Graph affects the Default Graph.
    /// </para>
    /// <para>
    /// The bulk update operations on a graph: <see cref="SaveGraph"/>, <see cref="VirtuosoConnectorBase.DeleteGraph(String)"/>
    /// and <see cref="VirtuosoConnectorBase.DeleteGraph(Uri)"/> will autocommit their updates regardless of the use of
    /// <see cref="Begin"/>, <see cref="Commit"/> or <see cref="Rollback"/>. This is default Virtuoso behaviour which enables
    /// these methods to handle updates on large graphs.
    /// </para>
    /// </remarks>
    public class VirtuosoConnector :VirtuosoConnectorBase, ITransactionalStorage, IUpdateableStorage, IConfigurationSerializable
    {

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
        public VirtuosoConnector(string server, int port, string db, string user, string password, int timeout = 0)
        :base(server, port, db, user, password, timeout)
        {
        }

        /// <summary>
        /// Creates a Manager for a Virtuoso Native Quad Store.
        /// </summary>
        /// <remarks></remarks>
        /// <param name="db">Database Name.</param>
        /// <param name="user">Username.</param>
        /// <param name="password">Password.</param>
        /// <param name="timeout">Connection Timeout in Seconds.</param>
        /// <remarks>
        /// <para> Assumes the server is on the localhost and the port is the default installation port of 1111.</para>
        /// <para>Timeouts less than equal to zero are ignored and treated as using the default timeout which is dictated by the underlying Virtuoso ADO.Net provider.</para>
        /// </remarks>
        public VirtuosoConnector(string db, string user, string password, int timeout = 0)
            : this("localhost", VirtuosoManager.DefaultPort, db, user, password, timeout)
        {
        }

        /// <summary>
        /// Creates a Manager for a Virtuoso Native Quad Store.
        /// </summary>
        /// <param name="connectionString">Connection String.</param>
        /// <remarks>
        /// Allows the end user to specify a customized connection string.
        /// </remarks>
        public VirtuosoConnector(string connectionString) : base(connectionString)
        {
        }

        /// <inheritdoc />
        public override void LoadGraph(IGraph g, Uri graphUri)
        {
            LoadGraph(new GraphHandler(g), graphUri);
        }

        /// <inheritdoc />
        public override void LoadGraph(IGraph g, string graphUri)
        {
            LoadGraph(new GraphHandler(g), string.IsNullOrEmpty(graphUri) ? null : g.UriFactory.Create(graphUri));
        }

        /// <inheritdoc />
        public override void LoadGraph(IRdfHandler handler, string graphUri)
        {
            LoadGraph(handler, string.IsNullOrEmpty(graphUri) ? null : UriFactory.Create(graphUri));
        }


        /// <summary>
        /// Saves a Graph to the Store.
        /// </summary>
        /// <param name="g">Graph to save.</param>
        /// <remarks>This implementation of this method uses Virtuoso's row auto-commit feature to allow loading large graphs. This means
        /// that the graph will be committed to the Virtuoso server, regardless of the use of <see cref="Begin"/> and <see cref="Commit"/> or <see cref="Rollback"/>
        /// to control the transaction.</remarks>
        public override void SaveGraph(IGraph g)
        {
            try
            {
                Open();
                SaveGraphInternal(g);
                Close(false);
            }
            catch (Exception)
            {
                Close(true, true);
                throw;
            }
        }

        /// <inheritdoc />
        public override void Update(string sparqlUpdate)
        {
            try
            {
                var localTxn = Open(true);

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

                if (localTxn) Close(true);
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
                    throw new SparqlUpdateException(
                        "An error occurred while trying to perform the SPARQL Update with Virtuoso.  Note that Virtuoso historically has primarily supported SPARUL (the precursor to SPARQL Update) and many valid SPARQL Update Commands may not be supported by Virtuoso",
                        ex);
                }
            }
            catch (SparqlUpdateException)
            {
                Close(true, rollbackTrans: true);
            }
            catch (Exception ex)
            {
                //Wrap in a SPARQL Update Exception
                Close(true, true);
                throw new SparqlUpdateException("An error occurred while trying to perform the SPARQL Update with Virtuoso.  Note that Virtuoso historically has primarily supported SPARUL (the precursor to SPARQL Update) and many valid SPARQL Update Commands may not be supported by Virtuoso if you are not using a recent version.", ex);
            }
        }

        #region ITransactionalStorage Implementation

        /// <inheritdoc />
        public void Begin()
        {
            Open(true);
        }

        /// <inheritdoc />
        public void Commit()
        {
            Close(true);
        }

        /// <inheritdoc />
        public void Rollback()
        {
            Close(true, rollbackTrans:true);
        }

        #endregion



        /// <inheritdoc />
        public override void Dispose()
        {
            Close(true);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (_customConnectionString)
            {
                return "[VirtuosoConnector] Custom Connection String";
            }
            return "[Virtuoso] " + _dbServer + ":" + _dbPort;
        }
    }
}
