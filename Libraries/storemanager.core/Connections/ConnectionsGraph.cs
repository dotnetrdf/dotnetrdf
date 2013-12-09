/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.StoreManager.Connections
{
    /// <summary>
    /// Manages a set of connections which are serialized to an underlying graph
    /// </summary>
    public class ConnectionsGraph
        : BaseConnectionsGraph
    {
        /// <summary>
        /// Creates a new connection graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="file">File on disk to which the graph should be saved</param>
        public ConnectionsGraph(IGraph g, String file)
            : base(g, file)
        {
        }
    }

    /// <summary>
    /// Manages a set of connections where a maximum number of connections are managed, if the number if exceeded then the least recent connection(s) are eliminated
    /// </summary>
    public class RecentConnectionsesGraph :
        ConnectionsGraph
    {
        private int _maxConnections;

        /// <summary>
        /// Creates a new connection graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="file">File on disk to which the graph should be saved</param>
        /// <param name="maxConnections">Maximum recent connections to manage</param>
        public RecentConnectionsesGraph(IGraph g, String file, int maxConnections)
            : base(g, file)
        {
            if (maxConnections < 1) throw new ArgumentException("Number of recent connections must be at least 1", "maxConnections");
            this.MaxConnections = maxConnections;
        }

        /// <summary>
        /// Gets/Sets the maximum recent connections to remember
        /// </summary>
        public int MaxConnections
        {
            get { return this._maxConnections; }
            set
            {
                if (value < 1) throw new ArgumentException("Number of recent connections must be at least 1");
                this._maxConnections = value;
                this.Save();
            }
        }

        /// <summary>
        /// Loads the connections from the underlying graph
        /// </summary>
        protected override void Load()
        {
            base.Load();
            // Doing the save will trim any excess connections
            this.Save();
        }

        /// <summary>
        /// Saves the connections to the underlying graph and file on disk
        /// </summary>
        protected override void Save()
        {
            if (this._connections.Count > this.MaxConnections)
            {
                while (this._connections.Count > this.MaxConnections)
                {
                    Connection leastRecent = this._connections.OrderBy(c => c.LastOpened).ThenBy(c => c.LastModified).First();
                    this._connections.Remove(leastRecent);
                }
            }
            base.Save();
        }
    }
}