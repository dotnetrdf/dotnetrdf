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
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Threading;
using VDS.RDF.Storage;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Class for storing RDF Graphs into SQL backed Storage
    /// </summary>
    /// <remarks>The Default Database format is that of the dotNetRDF Store which can be found at <see>http://www.dotnetrdf.org/content.asp?pageID=dotNetRDF%20Store</see>, arbitrary database formats can be used by implementing the <see cref="ISqlIOManager">ISqlIOManager</see> interface for your chosen database.</remarks>
    [Obsolete("The legacy SQL store format and related classes are officially deprecated - please see http://www.dotnetrdf.org?content.asp?pageID=dotNetRDF%20Store#migration for details on upgrading to the new ADO store format", false)]
    public class SqlWriter
    {
        private ISqlIOManager _manager;

        /// <summary>
        /// Creates a new instance of the SqlWriter which will use the SQL Store accessed with the given Database Information
        /// </summary>
        /// <param name="dbserver">Hostname of the Database Server</param>
        /// <param name="dbname">Name of the Database</param>
        /// <param name="dbuser">Username for the Database</param>
        /// <param name="dbpassword">Password for the Database</param>
        /// <remarks>Assumes that the SQL Store is a dotNetRDF MS SQL Store</remarks>
        public SqlWriter(String dbserver, String dbname, String dbuser, String dbpassword)
            : this(new MicrosoftSqlStoreManager(dbserver,dbname,dbuser,dbpassword)) { }

        /// <summary>
        /// Creates a new instance of the SqlWriter which will use the SQL Store accessed with the given Database Information and assuming the Database is on the localhost
        /// </summary>
        /// <param name="dbname">Name of the Database</param>
        /// <param name="dbuser">Username for the Database</param>
        /// <param name="dbpassword">Password for the Database</param>
        /// <remarks>Assumes that the SQL Store is a dotNetRDF MS SQL Store</remarks>
        public SqlWriter(String dbname, String dbuser, String dbpassword)
            : this("localhost", dbname, dbuser, dbpassword) { }

        /// <summary>
        /// Creates a new instance of the SqlWriter which will use the SQL Store accessed using the given <see cref="ISqlIOManager">ISqlIOManager</see> which allows writing to arbitrary SQL Stores
        /// </summary>
        /// <param name="manager">An <see cref="ISqlIOManager">ISqlIOManager</see> for your chosen underlying store</param>
        public SqlWriter(ISqlIOManager manager)
        {
            this._manager = manager;
        }

        /// <summary>
        /// Saves a Graph into a SQL Store
        /// </summary>
        /// <param name="g">Graph to Save</param>
        /// <param name="clearIfExists">Boolean indicating whether this Graph should completely replace an existing Graph with the same Base Uri</param>
        public void Save(IGraph g, bool clearIfExists)
        {
            try
            {
                //Open the Database Connection
                this._manager.Open(true);

                //Graph Properties
                String graphID = String.Empty;
                Uri graphUri = g.BaseUri;

                //Retrieve the existing Graph ID if any
                graphID = this._manager.GetGraphID(graphUri);

                //Clear Existing Graph if option chosen
                if (clearIfExists)
                {
                    //Delete all existing Triples for this Graph
                    this._manager.ClearGraph(graphID);
                }

                //Save Namespaces
                foreach (String prefix in g.NamespaceMap.Prefixes)
                {
                    this._manager.SaveNamespace(prefix, g.NamespaceMap.GetNamespaceUri(prefix), graphID);
                }

                //Save Triples
                foreach (Triple t in g.Triples)
                {
                    this._manager.SaveTriple(t, graphID);
                }

                //Wait for Completion
                while (!this._manager.HasCompleted)
                {
                    Thread.Sleep(250);
                }

                //Close Database
                this._manager.Close(true);
            }
            catch
            {
                this._manager.Close(true, true);
                throw;
            }
        }
    }

}

#endif