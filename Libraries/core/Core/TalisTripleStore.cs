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

#if !NO_STORAGE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;
using VDS.RDF.Query;

namespace VDS.RDF
{
    /// <summary>
    /// Class for representing a Talis Platform Store and executing queries against it
    /// </summary>
    /// <remarks>
    /// <para>
    /// No data is automatically loaded into this class when it is instantiated, it acts as a queryable view onto the remote Talis Store specified by the given <see cref="TalisPlatformConnector">TalisPlatformConnector</see>
    /// </para>
    /// <para>
    /// Currently Graphs added/removed from this Class do not affect the Talis Store unless they are <see cref="TalisGraph">TalisGraph</see> instances in which case changes to <strong>those Graphs only</strong> are persisted to the Talis Store.
    /// </para>
    /// <para>
    /// If you wish to alter the Store you must use either work exclusively with <see cref="TalisGraph">TalisGraph</see> instances or manipulate the Store directly using the <see cref="TalisPlatformConnector">TalisPlatformConnector</see>.
    /// </para>
    /// </remarks>
    [Obsolete("The NativeTripleStore is obsolete, please use the PersistentTripleStore instead which supercedes this class and provides more useful behaviour", false)]
    public class TalisTripleStore : BaseTripleStore, INativelyQueryableStore
    {
        private TalisPlatformConnector _talis;

        /// <summary>
        /// Creates a new Talis Triple Store
        /// </summary>
        /// <param name="connector">Connection to a Talis Store</param>
        public TalisTripleStore(TalisPlatformConnector connector)
            : base(new GraphCollection())
        {
            this._talis = connector;
        }

        /// <summary>
        /// Creates a new Talis Triple Store
        /// </summary>
        /// <param name="storeName">Name of the Talis Store</param>
        /// <param name="username">Username for the Talis Store</param>
        /// <param name="password">Password for the Talis Store</param>
        public TalisTripleStore(String storeName, String username, String password)
            : this(new TalisPlatformConnector(storeName, username, password)) { }

        /// <summary>
        /// Creates a new Talis Triple Store
        /// </summary>
        /// <param name="storeName">Name of the Talis Store</param>
        /// <remarks>This constructor creates a connection to the Talis platform which does not use authentication, this means that the Store must be world readable in order for queries to be executed.</remarks>
        public TalisTripleStore(String storeName)
            : this(new TalisPlatformConnector(storeName)) { }

        /// <summary>
        /// Executes a Sparql Query on the Triple Store
        /// </summary>
        /// <param name="query">Sparql Query as unparsed String</param>
        /// <returns></returns>
        /// <remarks>
        /// This method invokes the <see cref="TalisPlatformConnector.Query">Query</see> method of the <see cref="TalisPlatformConnector">TalisPlatformConnector</see> which invokes the Sparql service of the Talis Store specified by the connector.  This means that only the Metabox of the Talis Store is queried and therefore any queries containing FROM and FROM NAMED clauses may fail since the Talis API states that these clauses are not supported.
        /// </remarks>
        public object ExecuteQuery(String query)
        {
            return this._talis.Query(query);
        }

        /// <summary>
        /// Executes a Sparql Query on the Triple Store processing the results with an appropriate handler from those provided
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="query">SPARQL Query as unparsed String</param>
        /// <returns></returns>
        /// <remarks>
        /// This method invokes the <see cref="TalisPlatformConnector.Query">Query</see> method of the <see cref="TalisPlatformConnector">TalisPlatformConnector</see> which invokes the SPARQL service of the Talis Store specified by the connector.  This means that only the Metabox of the Talis Store is queried and therefore any queries containing FROM and FROM NAMED clauses may fail since the Talis API states that these clauses are not supported.
        /// </remarks>
        public void ExecuteQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string query)
        {
            this._talis.Query(rdfHandler, resultsHandler, query);
        }

        /// <summary>
        /// Disposes of a Talis Store
        /// </summary>
        public override void Dispose()
        {
            this._graphs.Dispose();
        }
    }
}

#endif