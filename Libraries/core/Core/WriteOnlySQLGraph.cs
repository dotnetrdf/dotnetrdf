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

namespace VDS.RDF
{
    /// <summary>
    /// Class for representing an RDF Graph which is automatically stored to a backing SQL Store as it is modified
    /// </summary>
    /// <remarks>
    /// <para>
    /// Designed for situations where you wish to write data to a Graph in the Store where the data being added is not affected by existing data in the Graph.
    /// </para>
    /// <para>
    /// Unlike it's parent class <see cref="SqlGraph">SqlGraph</see> this class will not automatically load the contents of the existing Graph when instantiated.  Instead it will only load the Namespaces so that you can use QNames based on the prefixes defined in the existing Graph to insert data.
    /// </para>
    /// <para>
    /// Iterating over the Triples/Nodes of this Graph will only return those that have been added to the Graph since its instantiation or the most recent call to <see cref="WriteOnlySqlGraph.Refresh">Refresh()</see>
    /// </para>
    /// </remarks>
    [Obsolete("The legacy SQL store format and related classes are officially deprecated - please see http://www.dotnetrdf.org?content.asp?pageID=dotNetRDF%20Store#migration for details on upgrading to the new ADO store format", false)]   
    public class WriteOnlySqlGraph : SqlGraph 
    {
        /// <summary>
        /// Creates a new instance of a Graph which is automatically saved to the given SQL Store
        /// </summary>
        /// <param name="graphUri">Uri of the Graph</param>
        /// <param name="manager">An <see cref="ISqlIOManager">ISqlIOManager</see> for your chosen underlying Store</param>
        public WriteOnlySqlGraph(Uri graphUri, ISqlIOManager manager)
        {
            //Set Database Mananger
            this._manager = manager;

            //Base Uri is the Graph Uri
            this.BaseUri = graphUri;

            //Get the Graph ID and then Load only the Namespaces
            this._graphID = this._manager.GetGraphID(graphUri);
            this._manager.LoadNamespaces(this, this._graphID);

            //Subscribe to Namespace Map Events
            //This has to happen after we load from the Database as otherwise the Loading will fire off all the
            //Namespace Map events creating unecessary overhead
            this._nsmapper.NamespaceAdded += this.HandleNamespaceAdded;
            this._nsmapper.NamespaceModified += this.HandleNamespaceModified;
            this._nsmapper.NamespaceRemoved += this.HandleNamespaceRemoved;
        }

        /// <summary>
        /// Creates a new instance of a Graph which is automatically saved to the given SQL Store
        /// </summary>
        /// <param name="graphUri">Uri of the Graph</param>
        /// <param name="dbname">Database Name</param>
        /// <param name="dbuser">Database User</param>
        /// <param name="dbpassword">Database Password</param>
        /// <remarks>Assumes that the SQL Store is a dotNetRDF MS SQL Store accessible on the localhost</remarks>
        public WriteOnlySqlGraph(Uri graphUri, String dbname, String dbuser, String dbpassword) 
            : this(graphUri, new MicrosoftSqlStoreManager("localhost", dbname, dbuser, dbpassword)) { }

        /// <summary>
        /// Creates a new instance of a Graph which is automatically saved to the given SQL Store
        /// </summary>
        /// <param name="graphUri">Uri of the Graph</param>
        /// <param name="dbserver">Database Server</param>
        /// <param name="dbname">Database Name</param>
        /// <param name="dbuser">Database User</param>
        /// <param name="dbpassword">Database Password</param>
        /// <remarks>Assumes that the SQL Store is a dotNetRDF MS SQL Store</remarks>
        public WriteOnlySqlGraph(Uri graphUri, String dbserver, String dbname, String dbuser, String dbpassword) 
            : this(graphUri, new MicrosoftSqlStoreManager(dbserver, dbname, dbuser, dbpassword)) { }

        /// <summary>
        /// Refreshes the Graphs Namespace Map from the Database
        /// </summary>
        /// <remarks>Any Triples that have been written are forgotten when refresh is called</remarks>
        public override void Refresh()
        {
            //Clear Triples and Nodes
            this._triples.Dispose();
            this._nodes.Dispose();

            //Clear the Namespace Map
            this._nsmapper.Clear();

            //Temporarily disable handling of the NamespaceAdded event
            this._nsmapper.NamespaceAdded -= this.HandleNamespaceAdded;

            //Reload Namespace Map
            this._manager.LoadNamespaces(this, this._graphID);

            //Handle the NamespaceAdded event again
            this._nsmapper.NamespaceAdded += this.HandleNamespaceAdded;
        }
    }
}

#endif