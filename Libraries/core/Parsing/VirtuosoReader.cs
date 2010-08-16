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
    /// Class for reading RDF Graphs from a Virtuoso Native Quad Store into arbitrary Graphs
    /// </summary>
    public class VirtuosoReader
    {
        private VirtuosoManager _manager;

        /// <summary>
        /// Creates a new instance of the Virtuoso Reader which connects to a Virtuoso Native Quad Store using the given Manager
        /// </summary>
        /// <param name="manager">Manager for the connection to Virtuoso</param>
        public VirtuosoReader(VirtuosoManager manager)
        {
            this._manager = manager;
        }

        /// <summary>
        /// Creates a new instance of the Virtuoso Reader which connects to a Virtuoso Native Quad Store using the given Manager
        /// </summary>
        /// <param name="dbname">Database Name</param>
        /// <param name="dbuser">Database User</param>
        /// <param name="dbpassword">Database Password</param>
        /// <remarks>Assumes that Virtuoso is installed on the local host using the default port 1111</remarks>
        public VirtuosoReader(String dbname, String dbuser, String dbpassword) : this(new VirtuosoManager(dbname, dbuser, dbpassword)) { }

        /// <summary>
        /// Creates a new instance of the Virtuoso Reader which connects to a Virtuoso Native Quad Store using the given Manager
        /// </summary>
        /// <param name="dbserver">Database Server</param>
        /// <param name="dbport">Database Port</param>
        /// <param name="dbname">Database Name</param>
        /// <param name="dbuser">Database User</param>
        /// <param name="dbpassword">Database Password</param>
        public VirtuosoReader(String dbserver, int dbport, String dbname, String dbuser, String dbpassword) : this(new VirtuosoManager(dbserver, dbport, dbname, dbuser, dbpassword)) { }

        /// <summary>
        /// Loads a Graph from the Native Quad Store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">Uri of the Graph to load</param>
        public void Load(IGraph g, Uri graphUri)
        {
            this._manager.LoadGraph(g, graphUri);
        }

        /// <summary>
        /// Loads a Graph from the Native Quad Store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">Uri of the Graph to load</param>
        public void Load(IGraph g, String graphUri)
        {
            this._manager.LoadGraph(g, graphUri);
        }
    }
}

#endif