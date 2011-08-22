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
using VDS.RDF.Storage;
using VDS.RDF.Parsing;

namespace VDS.RDF
{
    /// <summary>
    /// Class for representing Triple Stores which are collections of RDF Graphs
    /// </summary>
    /// <remarks>
    /// The 'On Demand' Triple Store is a <strong>read-only</strong> view of a SQL Based Store which loads Graphs as required rather than loading everything ahead of time.  This makes for a much more responsive Triple Store class though it does mean that SPARQL queries will generally only operate over a subset of the Store.
    /// <br /><br />
    /// Users of this class should be aware of the implications of the <strong>read-only</strong> nature of this Store.  Although it represents a view of a SQL Based Store changes made to either the Store (adding/removing Graphs) or to individual Graphs are <strong>never</strong> persisted to the backing Store.
    /// </remarks>
    [Obsolete("The legacy SQL store format and related classes are officially deprecated - please see http://www.dotnetrdf.org?content.asp?pageID=dotNetRDF%20Store#migration for details on upgrading to the new ADO store format", false)]
    public class OnDemandTripleStore : TripleStore
    {
        /// <summary>
        /// Opens an On Demand SQL Triple Store using the provided Store Manager
        /// </summary>
        /// <param name="manager">An <see cref="ISqlIOManager">ISqlIOManager</see> for your chosen backing SQL Store</param>
        /// <param name="defaultGraphUri">A Uri for the Default Graph which should be loaded from the backing Store</param>
        public OnDemandTripleStore(ISqlIOManager manager, Uri defaultGraphUri)
            : this(manager)
        {
            //Call Contains() which will try to load the Graph if it exists in the Store
            if (!this._graphs.Contains(defaultGraphUri))
            {
                throw new RdfStorageException("Cannot load the requested Default Graph since a Graph with that URI does not exist in the Triple Store");
            }
        }

        /// <summary>
        /// Opens an On Demand SQL Triple Store using the provided Store Manager
        /// </summary>
        /// <param name="manager">An <see cref="ISqlIOManager">ISqlIOManager</see> for your chosen backing SQL Store</param>
        public OnDemandTripleStore(ISqlIOManager manager)
            : base(new OnDemandGraphCollection(manager)) { }

        /// <summary>
        /// Opens an On Demand SQL Triple Store using the default Manager for a dotNetRDF Store accessible at the given database settings
        /// </summary>
        /// <param name="dbserver">Database Server</param>
        /// <param name="dbname">Database Name</param>
        /// <param name="dbuser">Database User</param>
        /// <param name="dbpassword">Database Password</param>
        public OnDemandTripleStore(String dbserver, String dbname, String dbuser, String dbpassword)
            : this(new MicrosoftSqlStoreManager(dbserver,dbname,dbuser,dbpassword)) { }

        /// <summary>
        /// Opens an On Demand SQL Triple Store using the default Manager for a dotNetRDF Store accessible at the given database settings
        /// </summary>
        /// <param name="dbname">Database Name</param>
        /// <param name="dbuser">Database User</param>
        /// <param name="dbpassword">Database Password</param>
        /// <remarks>Assumes that the Store is located on the localhost</remarks>
        public OnDemandTripleStore(String dbname, String dbuser, String dbpassword) 
            : this("localhost", dbname, dbuser, dbpassword) { }
    }

    /// <summary>
    /// A Graph Collection connected to a backing SQL Store where Graphs can be loaded on-demand from the Store as needed
    /// </summary>
    [Obsolete("The legacy SQL store format and related classes are officially deprecated - please see http://www.dotnetrdf.org?content.asp?pageID=dotNetRDF%20Store#migration for details on upgrading to the new ADO store format", false)]   
    public class OnDemandGraphCollection : GraphCollection, IEnumerable<IGraph>
    {
        /// <summary>
        /// The Manager for the underlying SQL Store
        /// </summary>
        protected ISqlIOManager _manager;
        /// <summary>
        /// A Reader which reads Graphs from the underlying SQL Store
        /// </summary>
        protected SqlReader _sqlreader;

        /// <summary>
        /// Creates a new On Demand Graph Collection which loads Graphs from a backing SQL Store on demand
        /// </summary>
        /// <param name="manager">Manager for the SQL Store</param>
        public OnDemandGraphCollection(ISqlIOManager manager)
        {
            this._manager = manager;
            this._manager.PreserveState = true;
            this._sqlreader = new SqlReader(this._manager);
        }

        /// <summary>
        /// Checks whether the Graph with the given Uri exists in this Graph Collection.  If it doesn't but is in the backing Store it will be loaded into the Graph Collection
        /// </summary>
        /// <param name="graphUri">Graph Uri to test</param>
        /// <returns></returns>
        public override bool Contains(Uri graphUri)
        {
            if (base.Contains(graphUri))
            {
                return true;
            }
            else
            {
                if (this._manager.Exists(graphUri))
                {
                    this.Load(graphUri);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Loads the specified Graph into the Graph Collection
        /// </summary>
        /// <param name="graphUri"></param>
        private void Load(Uri graphUri)
        {
            if (!base.Contains(graphUri))
            {
                IGraph g = this._sqlreader.Load(graphUri);
                this.Add(g, false);
            }
        }

        /// <summary>
        /// Disposes of a On Demand Graph Collection
        /// </summary>
        public override void Dispose()
        {
            this._sqlreader = null;
            this._manager.Dispose();
            base.Dispose();
        }
    }
}

#endif