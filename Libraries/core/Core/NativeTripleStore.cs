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
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using VDS.RDF.Update;

namespace VDS.RDF
{
    /// <summary>
    /// Class for representing an arbitrary Store which can be queried over for which a <see cref="IQueryableGenericIOManager">IQueryableGenericIOManager</see> is provided
    /// </summary>
    /// <remarks>
    /// <para>
    /// No data is automatically loaded into this class when it is instantiated, it acts as a queryable view on some arbitrary Store accessed via some <see cref="IQueryableGenericIOManager">IQueryableGenericIOManager</see>
    /// </para>
    /// </remarks>
    [Obsolete("The NativeTripleStore is obsolete, please use the PersistentTripleStore instead which supercedes this class and provides more useful behaviour", false)]
    public class NativeTripleStore : BaseTripleStore, INativelyQueryableStore, IUpdateableTripleStore
    {
        private IQueryableGenericIOManager _manager;

        /// <summary>
        /// Creates a new instance of the Native Triple Store
        /// </summary>
        /// <param name="manager">Manager for the Store you want to query</param>
        public NativeTripleStore(IQueryableGenericIOManager manager)
            : base(new GraphCollection())
        {
            this._manager = manager;
        }

        /// <summary>
        /// Executes a SPARQL Query against the underlying Store
        /// </summary>
        /// <param name="query">SPARQL Query</param>
        /// <returns></returns>
        /// <exception cref="RdfParseException">The Query Results are returned in an unexpected format</exception>
        /// <exception cref="RdfStorageException">There is a problem accessing the underlying store</exception>
        public object ExecuteQuery(string query)
        {
            return this._manager.Query(query);
        }

        /// <summary>
        /// Executes a SPARQL Query against the underlying Store processing the results with an appropriate handler from those provided
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">SPARQL Results Handler</param>
        /// <param name="query">SPARQL Query</param>
        /// <returns></returns>
        public void ExecuteQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string query)
        {
            this._manager.Query(rdfHandler, resultsHandler, query);
        }

        /// <summary>
        /// Disposes of a Native Triple Store
        /// </summary>
        public override void Dispose()
        {
            this._manager.Dispose();
        }

        /// <summary>
        /// Executes a SPARQL Update on the Store
        /// </summary>
        /// <param name="update">SPARQL Update</param>
        /// <remarks>
        /// <para>
        /// If the underlying Manager is an <see cref="IUpdateableGenericIOManager">IUpdateableGenericIOManager</see> then the managers own Update implementation will be used, otherwise dotNetRDF's approximated implementation for generic stores will be used.  In the case of approximation exact feature support will vary depending on the underlying manager being used.
        /// </para>
        /// </remarks>
        public void ExecuteUpdate(string update)
        {
            SparqlUpdateParser parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet cmds = parser.ParseFromString(update);
            this.ExecuteUpdate(cmds);
        }

        /// <summary>
        /// Executes a SPARQL Update on the Store
        /// </summary>
        /// <param name="update">SPARQL Update Command</param>
        /// <remarks>
        /// <para>
        /// If the underlying Manager is an <see cref="IUpdateableGenericIOManager">IUpdateableGenericIOManager</see> then the managers own Update implementation will be used, otherwise dotNetRDF's approximated implementation for generic stores will be used.  In the case of approximation exact feature support will vary depending on the underlying manager being used.
        /// </para>
        /// </remarks>
        public void ExecuteUpdate(SparqlUpdateCommand update)
        {
            GenericUpdateProcessor processor = new GenericUpdateProcessor(this._manager);
            processor.ProcessCommand(update);
        }

        /// <summary>
        /// Executes a set of SPARQL Update commands on the Store
        /// </summary>
        /// <param name="updates">SPARQL Update Commands</param>
        /// <remarks>
        /// <para>
        /// If the underlying Manager is an <see cref="IUpdateableGenericIOManager">IUpdateableGenericIOManager</see> then the managers own Update implementation will be used, otherwise dotNetRDF's approximated implementation for generic stores will be used.  In the case of approximation exact feature support will vary depending on the underlying manager being used.
        /// </para>
        /// </remarks>
        public void ExecuteUpdate(SparqlUpdateCommandSet updates)
        {
            GenericUpdateProcessor processor = new GenericUpdateProcessor(this._manager);
            processor.ProcessCommandSet(updates);
        }
    }
}

#endif