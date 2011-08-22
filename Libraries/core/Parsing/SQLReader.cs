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

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Class for reading RDF Graphs from a SQL Server Database
    /// </summary>
    /// <remarks>The Database format is that of the dotNetRDF Store which can be found at <see>http://www.dotnetrdf.org/content.asp?pageID=dotNetRDF%20Store</see>, arbitrary database formats can be used by implementing the <see cref="ISqlIOManager">ISqlIOManager</see> interface for your chosen database.</remarks>
    [Obsolete("The legacy SQL store format and related classes are officially deprecated - please see http://www.dotnetrdf.org?content.asp?pageID=dotNetRDF%20Store#migration for details on upgrading to the new ADO store format", false)]
    public class SqlReader
    {
        private ISqlIOManager _manager;

        /// <summary>
        /// Creates a new instance of the SqlReader which will use the SQL Store accessed with the given Database Information
        /// </summary>
        /// <param name="dbserver">Hostname of the Database Server</param>
        /// <param name="dbname">Name of the Database</param>
        /// <param name="dbuser">Username for the Database</param>
        /// <param name="dbpassword">Password for the Database</param>
        /// <remarks>Assumes that the SQL Store is a dotNetRDF MS SQL Store</remarks>
        public SqlReader(String dbserver, String dbname, String dbuser, String dbpassword)
            : this(new MicrosoftSqlStoreManager(dbserver, dbname, dbuser, dbpassword)) { }

        /// <summary>
        /// Creates a new instance of the SqlReader which will use the SQL Store accessed with the given Database Information and assuming the Database is located on the localhost
        /// </summary>
        /// <param name="dbname">Name of the Database</param>
        /// <param name="dbuser">Username for the Database</param>
        /// <param name="dbpassword">Password for the Database</param>
        /// <remarks>Assumes that the SQL Store is a dotNetRDF MS SQL Store</remarks>
        public SqlReader(String dbname, String dbuser, String dbpassword)
            : this("localhost", dbname, dbuser, dbpassword) { }

        /// <summary>
        /// Creates a new instance of the SqlReader which will use the SQL Store accessed using the given <see cref="ISqlIOManager">ISqlIOManager</see> which allows writing to arbitrary SQL Stores
        /// </summary>
        /// <param name="manager">An <see cref="ISqlIOManager">ISqlIOManager</see> for your chosen underlying store</param>
        public SqlReader(ISqlIOManager manager)
        {
            this._manager = manager;
        }

        /// <summary>
        /// Loads a Graph from a SQL Store
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to Load</param>
        /// <returns></returns>
        public IGraph Load(String graphUri)
        {
            if (graphUri.Equals(String.Empty) || graphUri.Equals(GraphCollection.DefaultGraphUri))
            {
                return this.Load((Uri)null);
            }
            else
            {
                return this.Load(new Uri(graphUri));
            }
        }

        /// <summary>
        /// Loads a Graph from the SQL Store into a Graph object
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to load</param>
        /// <returns></returns>
        public IGraph Load(Uri graphUri)
        {
            Graph g = new Graph();
            try
            {
                //Get the Database Connection
                this._manager.Open(true);

                //Retrieve the existing Graph ID if any
                if (!this._manager.Exists(graphUri))
                {
                    throw new RdfStorageException("The Graph '" + graphUri.ToSafeString() + "' does not exist in the underlying Store");
                }
                String graphID = this._manager.GetGraphID(graphUri);

                g.BaseUri = graphUri;

                //Load Namespaces
                this._manager.LoadNamespaces(g, graphID);

                //Load Triples
                this._manager.LoadTriples(g, graphID);

                this._manager.Close(true);
            }
            catch
            {
                this._manager.Close(true, true);
                throw;
            }
            return g;
        }
    }
}

#endif