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
using OpenLink.Data.Virtuoso;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using VDS.RDF.Update;

namespace VDS.RDF
{
    /// <summary>
    /// Class for representing a Virtuoso Native Quad Store and executing queries against it
    /// </summary>
    /// <remarks>
    /// <para>
    /// No data is automatically loaded into this class when it is instantiated, it acts as a queryable view onto the given Virtuoso Store specified by the given <see cref="VirtuosoManager">VirtuosoManager</see>
    /// </para>
    /// <para>
    /// Currently Graphs added/removed from this Class do not affect the Virtuoso Store
    /// </para>
    /// <para>
    /// If you wish to alter the Store you must manipulate the Store directly using the <see cref="VirtuosoManager">VirtuosoManager</see> or by issuing SPARQL Update commands using the <see cref="VirtuosoTripleStore.ExecuteUpdate()">ExecuteUpdate()</see> method.
    /// </para>
    /// </remarks>
    [Obsolete("The VirtuosoTripleStore is obsolete, please use the PersistentTripleStore instead which supercedes this class and provides more useful behaviour", false)]
    public class VirtuosoTripleStore 
        : BaseTripleStore, INativelyQueryableStore, IUpdateableTripleStore
    {
        private VirtuosoManager _manager;

        /// <summary>
        /// Creates a new instance of a Virtuoso Triple Store which uses the given <see cref="VirtuosoManager">VirtuosoManager</see> to connect to a Virtuoso Triple Store
        /// </summary>
        /// <param name="manager">Manager for the connection to Virtuoso</param>
        public VirtuosoTripleStore(VirtuosoManager manager)
            : base(new GraphCollection())
        {
            this._manager = manager;
        }

        /// <summary>
        /// Creates a new Virtuoso Triple Store using the given parameters
        /// </summary>
        /// <param name="server">Server</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <remarks>Uses the default port of <strong>1111</strong> and assume the Quad Store is in the default database which is <strong>DB</strong></remarks>
        public VirtuosoTripleStore(String server, String username, String password)
            : this(new VirtuosoManager(server, 1111, "DB", username, password)) { }

        /// <summary>
        /// Creates a new Virtuoso Triple Store using the given parameters
        /// </summary>
        /// <param name="server">Server</param>
        /// <param name="port">Port</param>
        /// <param name="db">Database</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        public VirtuosoTripleStore(String server, int port, String db, String username, String password)
            : this(new VirtuosoManager(server, port, db, username, password)) { }

        /// <summary>
        /// Executes a SPARQL Query on the Triple Store
        /// </summary>
        /// <param name="query">SPARQL Query as unparsed String</param>
        /// <returns>
        /// A <see cref="SparqlResultSet">SparqlResultSet</see> or a <see cref="Graph">Graph</see> if the query is a normal Sparql Query or a null if the query is a Sparql Update command.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method invokes the <see cref="VirtuosoManager.Query">Query</see> method of the <see cref="VirtuosoManager">VirtuosoManager</see> which executes the Sparql query against the store using SPASQL (Sparql+SQL).
        /// </para>
        /// <para>
        /// You can use this method to issue SPARQL Update commands against the Store if the user account you are using to connect has the relevant privilege <strong>SPARQL_UPDATE</strong> granted to them, for new development we recommend you use the <see cref="VirtuosoTripleStore.ExecuteUpdate">ExecuteUpdate()</see> method instead.
        /// </para>
        /// <para>
        /// <strong>Warning:</strong> In rare cases we have been able to crash Virtuoso by issuing malformed Sparql Update commands to it, this appears to be an issue with Virtuoso.
        /// </para>
        /// </remarks>
        /// <exception cref="RdfQueryException">Thrown if the query is malformed or the results cannot be processed</exception>
        /// <exception cref="VirtuosoException">Thrown if accessing Virtuoso fails in some way</exception>
        public object ExecuteQuery(string query)
        {
            return this._manager.Query(query);
        }

        /// <summary>
        /// Executes a SPARQL Query on the Triple Store processing the results with an appropriate handler from those provided
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="query">SPARQL Query as unparsed String</param>
        /// <returns>
        /// A <see cref="SparqlResultSet">SparqlResultSet</see> or a <see cref="Graph">Graph</see> if the query is a normal SPARQL Query or a null if the query is a SPARQL Update command.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method invokes the <see cref="VirtuosoManager.Query">Query</see> method of the <see cref="VirtuosoManager">VirtuosoManager</see> which executes the SPARQL query against the store using SPASQL (Sparql+SQL).
        /// </para>
        /// <para>
        /// You can use this method to issue SPARQL Update commands against the Store if the user account you are using to connect has the relevant privilege <strong>SPARQL_UPDATE</strong> granted to them, for new development we recommend you use the <see cref="VirtuosoTripleStore.ExecuteUpdate()">ExecuteUpdate()</see> method instead.
        /// </para>
        /// <para>
        /// <strong>Warning:</strong> In rare cases we have been able to crash Virtuoso by issuing malformed Sparql Update commands to it, this appears to be an issue with Virtuoso.
        /// </para>
        /// </remarks>
        /// <exception cref="RdfQueryException">Thrown if the query is malformed or the results cannot be processed</exception>
        /// <exception cref="VirtuosoException">Thrown if accessing Virtuoso fails in some way</exception>
        public void ExecuteQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string query)
        {
            this._manager.Query(rdfHandler, resultsHandler, query);
        }

        /// <summary>
        /// Disposes of a Virtuoso Triple Store
        /// </summary>
        public override void Dispose()
        {
            this._graphs.Dispose();
        }

        /// <summary>
        /// Executes a SPARQL Update on the Virtuoso store
        /// </summary>
        /// <param name="update">SPARQL Update</param>
        /// <remarks>
        /// <para>
        /// <strong>Warning:</strong> In rare cases we have been able to crash Virtuoso by issuing malformed Sparql Update commands to it, this appears to be an issue with Virtuoso.
        /// </para>
        /// </remarks>
        public void ExecuteUpdate(string update)
        {
            this._manager.Update(update);
        }

        /// <summary>
        /// Executes a SPARQL Update on the Virtuoso store
        /// </summary>
        /// <param name="update">SPARQL Update</param>
        /// <remarks>
        /// <para>
        /// <strong>Warning:</strong> In rare cases we have been able to crash Virtuoso by issuing malformed Sparql Update commands to it, this appears to be an issue with Virtuoso.
        /// </para>
        /// </remarks>
        public void ExecuteUpdate(SparqlUpdateCommand update)
        {
            this._manager.Update(update.ToString());
        }

        /// <summary>
        /// Executes a SPARQL Update on the Virtuoso store
        /// </summary>
        /// <param name="updates">SPARQL Updates</param>
        /// <remarks>
        /// <para>
        /// <strong>Warning:</strong> In rare cases we have been able to crash Virtuoso by issuing malformed Sparql Update commands to it, this appears to be an issue with Virtuoso.
        /// </para>
        /// </remarks>
        public void ExecuteUpdate(SparqlUpdateCommandSet updates)
        {
            this._manager.Update(updates.ToString());
        }
    }
}

#endif