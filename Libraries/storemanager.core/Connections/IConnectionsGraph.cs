using System;
using System.Collections.Generic;

namespace VDS.RDF.Utilities.StoreManager.Connections
{
    /// <summary>
    /// Interface for connections graphs which manage a set of connections which are serialized to a graph and a file on disk
    /// </summary>
    public interface IConnectionsGraph
    {
        /// <summary>
        /// Gets the file on disk where this graph is saved
        /// </summary>
        String File { get; }

        /// <summary>
        /// Gets the underlying graph
        /// </summary>
        IGraph Graph { get; }

        /// <summary>
        /// Gets the managed connections
        /// </summary>
        IEnumerable<Connection> Connections { get; }

        /// <summary>
        /// Adds a connection
        /// </summary>
        /// <param name="connection">Connection</param>
        void Add(Connection connection);

        /// <summary>
        /// Removes a connection
        /// </summary>
        /// <param name="connection">Connection</param>
        void Remove(Connection connection);

        /// <summary>
        /// Clears all connections
        /// </summary>
        void Clear();
    }
}