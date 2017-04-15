/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
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
