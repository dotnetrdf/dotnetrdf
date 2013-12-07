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